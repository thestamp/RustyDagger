using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RustyDagger.Server.Services;
using RustyDagger.Shared.Dto;
using RustyDagger.Shared.Engine;
using RustyDagger.Shared.Models;

namespace RustyDagger.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GameController : ControllerBase
{
    private readonly HeroService _heroService;
    private readonly Rng _rng = new();

    public GameController(HeroService heroService) => _heroService = heroService;

    private int AccountId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("{heroId}/state")]
    public async Task<ActionResult<GameStateResponse>> GetState(int heroId)
    {
        var entity = await _heroService.GetHeroAsync(heroId, AccountId);
        if (entity == null) return NotFound();

        var hero = _heroService.ToGameHero(entity);
        return Ok(BuildStateResponse(hero, heroId));
    }

    [HttpPost("{heroId}/travel")]
    public async Task<ActionResult<GameStateResponse>> Travel(int heroId, [FromBody] GameActionRequest req)
    {
        var entity = await _heroService.GetHeroAsync(heroId, AccountId);
        if (entity == null) return NotFound();

        var hero = _heroService.ToGameHero(entity);

        if (hero.Actions <= 0)
            return BadRequest(new { error = "No actions remaining." });

        if (!Enum.TryParse<GamePlace>(req.Target, out var dest))
            return BadRequest(new { error = "Invalid destination." });

        // Travel cost
        int cost = GetTravelCost(hero.Place, dest);
        if (cost < 0)
            return BadRequest(new { error = "Cannot travel there from current location." });

        hero.Actions -= cost;
        hero.Place = dest;
        hero.State = HeroState.Town;

        await _heroService.SaveHeroAsync(hero, entity);
        return Ok(BuildStateResponse(hero, heroId));
    }

    [HttpPost("{heroId}/quest")]
    public async Task<ActionResult<GameStateResponse>> Quest(int heroId)
    {
        var entity = await _heroService.GetHeroAsync(heroId, AccountId);
        if (entity == null) return NotFound();

        var hero = _heroService.ToGameHero(entity);

        if (hero.Actions <= 0)
            return BadRequest(new { error = "No actions remaining." });

        hero.Actions--;
        hero.State = HeroState.Quest;

        // Generate encounter
        var mob = _heroService.GenerateEncounter(hero);
        var combat = new CombatEngine(_rng);
        var actions = combat.GetAvailableActions(hero, mob, true);

        await _heroService.SaveHeroAsync(hero, entity);

        var response = BuildStateResponse(hero, heroId);
        response.Screen = "Battle";
        response.CurrentMonster = new MonsterView
        {
            Name = mob.Name,
            Picture = mob.Picture,
            Description = mob.Description,
            HealthPercent = 100,
            IsDead = false
        };
        response.AvailableActions = actions.Select(a => a.ToString()).ToList();
        return Ok(response);
    }

    [HttpPost("{heroId}/combat")]
    public async Task<ActionResult<GameStateResponse>> Combat(int heroId, [FromBody] GameActionRequest req)
    {
        var entity = await _heroService.GetHeroAsync(heroId, AccountId);
        if (entity == null) return NotFound();

        var hero = _heroService.ToGameHero(entity);

        if (!Enum.TryParse<CombatAction>(req.Action, out var action))
            return BadRequest(new { error = "Invalid action." });

        // Re-generate encounter (stateless combat for simplicity)
        var mob = _heroService.GenerateEncounter(hero);
        var combat = new CombatEngine(_rng);
        var result = combat.ResolveCombat(hero, mob, action);

        // Process combat result
        var response = BuildStateResponse(hero, heroId);
        response.Log = result.Log;

        if (result.HeroKilled)
        {
            hero.State = HeroState.Dead;
            response.Screen = "Dead";
        }
        else if (result.MonsterKilled || result.MonsterFled || result.Controlled || result.Swindled || result.HeroFled)
        {
            // Loot
            if (result.MonsterKilled)
            {
                hero.Fame += mob.CalculateFame();
                hero.GainGuts(mob.Guts, _rng);
                foreach (var loot in mob.Pack)
                    hero.AddPackItem(loot.Name, loot.Count);
                response.Log.Add($"You defeated {mob.Name}! (+{mob.CalculateFame()} fame)");
            }
            hero.State = HeroState.Town;
            response.Screen = "QuestResult";
        }
        else
        {
            // Combat continues
            var nextActions = combat.GetAvailableActions(hero, mob, false);
            response.Screen = "Battle";
            response.CurrentMonster = new MonsterView
            {
                Name = mob.Name,
                Picture = mob.Picture,
                Description = mob.Description,
                HealthPercent = mob.MaxHealth > 0 ? mob.CurrentHealth * 100 / mob.MaxHealth : 0,
                IsDead = mob.IsDead
            };
            response.AvailableActions = nextActions.Select(a => a.ToString()).ToList();
        }

        await _heroService.SaveHeroAsync(hero, entity);
        return Ok(response);
    }

    [HttpPost("{heroId}/rest")]
    public async Task<ActionResult<GameStateResponse>> Rest(int heroId)
    {
        var entity = await _heroService.GetHeroAsync(heroId, AccountId);
        if (entity == null) return NotFound();

        var hero = _heroService.ToGameHero(entity);

        hero.FullHeal();
        hero.Actions = hero.CalculateActions();
        hero.Age++;
        hero.State = HeroState.Town;

        await _heroService.SaveHeroAsync(hero, entity);

        var response = BuildStateResponse(hero, heroId);
        response.Log.Add($"You rest at the inn. A new day dawns. (Age: {hero.Age})");
        return Ok(response);
    }

    private GameStateResponse BuildStateResponse(Hero hero, int heroId)
    {
        return new GameStateResponse
        {
            Hero = new HeroSummary
            {
                Id = heroId,
                Name = hero.Name,
                Level = hero.Level,
                Guts = hero.Guts,
                Wits = hero.Wits,
                Charm = hero.Charm,
                Fame = hero.Fame,
                Place = hero.Place.ToString()
            },
            Screen = hero.State.ToString(),
            AreaDescription = GetAreaDescription(hero.Place),
            AvailableActions = GetTownActions(hero)
        };
    }

    private static List<string> GetTownActions(Hero hero)
    {
        return new List<string>
        {
            "Quest",
            "Rest",
            "Travel",
            "Status"
        };
    }

    private static string GetAreaDescription(GamePlace place) => place switch
    {
        GamePlace.Town => "Dragon Court Town — A bustling medieval town with taverns, shops, and guilds.",
        GamePlace.Fields => "The Open Fields — Rolling grasslands dotted with travelers and dangers.",
        GamePlace.Forest => "The Arcane Forest — Dense, mysterious woods hiding elves, orcs, and stranger things.",
        GamePlace.Hills => "The Dragon Hills — Treacherous mountains where giants and dragons roam.",
        GamePlace.Mound => "The Goblin Mound — A network of goblin tunnels beneath a great hill.",
        GamePlace.Castle => "The Queen's Castle — Seat of royal power, guarded by the Dragon Guard.",
        _ => "Unknown lands stretch before you."
    };

    private static int GetTravelCost(GamePlace from, GamePlace to)
    {
        if (from == to) return 0;
        // Simplified travel cost matrix
        return (from, to) switch
        {
            (GamePlace.Town, GamePlace.Fields) => 1,
            (GamePlace.Town, GamePlace.Castle) => 2,
            (GamePlace.Fields, GamePlace.Town) => 1,
            (GamePlace.Fields, GamePlace.Forest) => 2,
            (GamePlace.Fields, GamePlace.Mound) => 2,
            (GamePlace.Forest, GamePlace.Fields) => 2,
            (GamePlace.Forest, GamePlace.Hills) => 3,
            (GamePlace.Hills, GamePlace.Forest) => 3,
            (GamePlace.Mound, GamePlace.Fields) => 2,
            (GamePlace.Castle, GamePlace.Town) => 2,
            _ => -1 // Cannot travel directly
        };
    }
}
