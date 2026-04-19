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

        // Generate encounter and persist it
        var mob = _heroService.GenerateEncounter(hero);
        _heroService.SaveMonster(mob, entity);

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

        // Load persisted monster
        var mob = _heroService.LoadMonster(entity);
        if (mob == null)
            return BadRequest(new { error = "No active encounter." });

        var combat = new CombatEngine(_rng);
        var result = combat.ResolveCombat(hero, mob, action);

        var response = BuildStateResponse(hero, heroId);
        response.Log = result.Log;

        if (result.HeroKilled)
        {
            // Apply death penalty — Java: killedScreen
            var deathLog = hero.ApplyDeathPenalty(_rng, losePack: true);
            response.Log.AddRange(deathLog);
            response.Screen = "QuestResult";
            _heroService.ClearMonster(entity);
        }
        else if (result.MonsterKilled || result.MonsterFled || result.Controlled || result.Swindled || result.HeroFled)
        {
            // Victory / encounter end
            if (result.MonsterKilled)
            {
                // Loot
                foreach (var loot in mob.Pack)
                    hero.AddPackItem(loot.Name, loot.Count);
                response.Log.Add($"You defeated {mob.Name}!");

                // Fame
                int fame = mob.CalculateFame();
                hero.Fame += fame;
                response.Log.Add($"+{fame} fame");

                // Experience (Java: baseExp + (2g+w+c)*weight/4)
                int exp = mob.CalculateExp();
                if (exp > 0)
                {
                    hero.Experience += exp;
                    response.Log.Add($"+{exp} experience");
                }

                // Stat gains — Java gives different stats based on combat action
                string? gutsMsg = TryGainStat(hero, "Guts", mob.Guts);
                string? witsMsg = TryGainStat(hero, "Wits", mob.Wits);
                string? charmMsg = TryGainStat(hero, "Charm", mob.Charm);
                if (gutsMsg != null) response.Log.Add(gutsMsg);
                if (witsMsg != null) response.Log.Add(witsMsg);
                if (charmMsg != null) response.Log.Add(charmMsg);

                // Level up check
                while (hero.TryToLevel())
                    response.Log.Add($"+++ You Have Gained A Level — Level {hero.Level} +++ (+2 Guts, +2 Wits, +2 Charm)");
            }

            if (result.HeroFled)
                response.Log.Add("You escaped!");

            if (result.Controlled)
                response.Log.Add($"{mob.Name} is under your control!");

            if (result.Swindled)
                response.Log.Add($"You swindled {mob.Name}!");

            hero.State = HeroState.Town;
            response.Screen = "QuestResult";
            _heroService.ClearMonster(entity);
        }
        else
        {
            // Combat continues — increment stance per Java Options.nextRound()
            if (mob.Stance >= 2 && mob.Stance <= 3) // Defensive or Hostile
                mob.Stance++;

            // Persist updated monster
            _heroService.SaveMonster(mob, entity);

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

    private static string? TryGainStat(Hero hero, string stat, int weight)
    {
        int before;
        switch (stat)
        {
            case "Guts":
                before = hero.Guts;
                hero.GainGuts(weight, new Rng());
                return hero.Guts > before ? "*** You grow Stronger +1 Guts! ***" : null;
            case "Wits":
                before = hero.Wits;
                hero.GainWits(weight, new Rng());
                return hero.Wits > before ? "*** You grow Smarter +1 Wits! ***" : null;
            case "Charm":
                before = hero.Charm;
                hero.GainCharm(weight, new Rng());
                return hero.Charm > before ? "*** You grow Happier +1 Charm! ***" : null;
            default: return null;
        }
    }

    [HttpPost("{heroId}/rest")]
    public async Task<ActionResult<GameStateResponse>> Rest(int heroId)
    {
        var entity = await _heroService.GetHeroAsync(heroId, AccountId);
        if (entity == null) return NotFound();

        var hero = _heroService.ToGameHero(entity);

        // Java advance(): fame decays by 10%, plus social rank * 10
        int fame = hero.Fame;
        int rank = hero.Social;
        hero.Fame = (fame - (fame / 10)) + (rank * 10);

        // Stipend from social rank: rank^2 * 50
        int stipend = rank * rank * 50;
        if (stipend > 0)
            hero.AddPackItem("Marks", stipend);

        // Age: Java checks Unaging trait
        if (!hero.HasTrait("Unaging"))
            hero.Age++;
        else
        {
            if (hero.Age > 33) hero.Age--;
            else if (hero.Age < 33) hero.Age++;
        }
        if (hero.Age < 15) hero.Age = 15;

        // Reset fatigue, heal, recalculate actions
        hero.Fatigue = 0;
        hero.FullHeal();
        hero.Actions = hero.CalculateActions();
        hero.State = HeroState.Town;

        // Reset guild uses per Java advance()
        hero.FightUses = hero.FightRank;
        hero.MagicUses = hero.MagicRank;
        hero.ThiefUses = hero.ThiefRank;
        hero.IeatsuUses = hero.IeatsuRank;
        if (hero.HasTrait("Berzerk")) hero.FightUses += (hero.Level + 7) / 8;
        if (hero.HasTrait("Mystic")) hero.MagicUses += (hero.Level + 7) / 8;
        if (hero.HasTrait("Trader")) hero.ThiefUses += (hero.Level + 7) / 8;

        // Clear any persisted monster
        _heroService.ClearMonster(entity);

        await _heroService.SaveHeroAsync(hero, entity);

        var response = BuildStateResponse(hero, heroId);
        response.Log.Add($"You rest at the inn. A new day dawns. (Age: {hero.Age})");
        if (stipend > 0)
            response.Log.Add($"You receive a stipend of {stipend} marks.");
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
                Actions = hero.Actions,
                Wounds = hero.Wounds,
                Experience = hero.Experience,
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
