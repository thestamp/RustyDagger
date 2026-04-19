using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RustyDagger.Data;
using RustyDagger.Data.Entities;
using RustyDagger.Shared.Dto;
using RustyDagger.Shared.Engine;
using RustyDagger.Shared.Models;
using RustyDagger.Shared.Static;

namespace RustyDagger.Server.Services;

public class HeroService
{
    private readonly DragonCourtDbContext _db;
    private readonly Rng _rng = new();
    private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public HeroService(DragonCourtDbContext db) => _db = db;

    public async Task<HeroEntity?> GetHeroAsync(int heroId, int accountId)
    {
        return await _db.Heroes
            .FirstOrDefaultAsync(h => h.Id == heroId && h.AccountId == accountId);
    }

    public async Task<List<HeroSummary>> GetHeroesAsync(int accountId)
    {
        return await _db.Heroes
            .Where(h => h.AccountId == accountId)
            .Select(h => new HeroSummary
            {
                Id = h.Id,
                Name = h.Name,
                Level = h.Level,
                Guts = h.Guts,
                Wits = h.Wits,
                Charm = h.Charm,
                Fame = h.Fame,
                Place = h.Place
            })
            .ToListAsync();
    }

    public async Task<HeroEntity> CreateHeroAsync(int accountId, CreateHeroRequest req)
    {
        // Validate build points: guts + wits + charm + money + trait_cost = 20 + 12 (base)
        int spent = (req.Guts - 4) + (req.Wits - 4) + (req.Charm - 4) + (req.Money - 1);
        int traitCost = req.Trait switch
        {
            "Noble" => 12,
            "Trader" => 10,
            "Wizard" => 9,
            "Warrior" => 8,
            _ => 0
        };
        spent += traitCost;
        if (spent != 20)
            throw new InvalidOperationException("Build points must total exactly 20.");

        var traits = new List<string>();
        int fightRank = 0, magicRank = 0, thiefRank = 0;
        int social = 0;

        if (!string.IsNullOrEmpty(req.Trait))
        {
            traits.Add(req.Trait);
            switch (req.Trait)
            {
                case "Noble": social = 1; break;
                case "Wizard": magicRank = 1; break;
                case "Warrior": fightRank = 1; break;
                case "Trader": thiefRank = 1; break;
            }
        }

        // Starting gear: Rusty Dagger
        var gear = new Arms?[5];
        gear[0] = ArmsData.Find("Rusty Dagger");
        var gearList = gear.Where(g => g != null).ToList();

        var hero = new HeroEntity
        {
            AccountId = accountId,
            Name = req.Name.Trim(),
            Guts = req.Guts,
            Wits = req.Wits,
            Charm = req.Charm,
            Level = 1,
            Age = 16,
            Marks = req.Money * 25,
            Actions = 30,
            Social = social,
            FightRank = fightRank,
            MagicRank = magicRank,
            ThiefRank = thiefRank,
            Traits = string.Join(",", traits),
            GearJson = JsonSerializer.Serialize(gearList, _jsonOpts),
            PackJson = "[]",
            LooksJson = "{}",
            Place = "Fields",
            State = "Town"
        };

        _db.Heroes.Add(hero);
        await _db.SaveChangesAsync();
        return hero;
    }

    public Hero ToGameHero(HeroEntity entity)
    {
        var hero = new Hero
        {
            Name = entity.Name,
            Guts = entity.Guts,
            Wits = entity.Wits,
            Charm = entity.Charm,
            Level = entity.Level,
            Age = entity.Age,
            Fame = entity.Fame,
            Stipend = entity.Stipend,
            Social = entity.Social,
            Favor = entity.Favor,
            Actions = entity.Actions,
            Wounds = entity.Wounds,
            Place = Enum.TryParse<GamePlace>(entity.Place, out var p) ? p : GamePlace.Fields,
            State = Enum.TryParse<HeroState>(entity.State, out var s) ? s : HeroState.Town,
            FightRank = entity.FightRank,
            MagicRank = entity.MagicRank,
            ThiefRank = entity.ThiefRank,
            IeatsuRank = entity.IeatsuRank,
        };

        if (!string.IsNullOrWhiteSpace(entity.Traits))
        {
            foreach (var t in entity.Traits.Split(',', StringSplitOptions.RemoveEmptyEntries))
                hero.Traits.Add(t.Trim());
        }

        try
        {
            var gearList = JsonSerializer.Deserialize<List<Arms>>(entity.GearJson, _jsonOpts) ?? new();
            foreach (var arms in gearList)
                hero.Equip(arms);
        }
        catch { /* default empty gear */ }

        try
        {
            hero.Pack = JsonSerializer.Deserialize<List<PackItem>>(entity.PackJson, _jsonOpts) ?? new();
        }
        catch { hero.Pack = new(); }

        // Add Marks as pack item if not present
        if (!hero.Pack.Any(p => p.Name == "Marks"))
            hero.Pack.Add(new PackItem("Marks", entity.Marks));
        else
            hero.SetPackCount("Marks", entity.Marks);

        try
        {
            hero.Looks = JsonSerializer.Deserialize<HeroLooks>(entity.LooksJson, _jsonOpts) ?? new();
        }
        catch { hero.Looks = new(); }

        return hero;
    }

    public void SyncToEntity(Hero hero, HeroEntity entity)
    {
        entity.Guts = hero.Guts;
        entity.Wits = hero.Wits;
        entity.Charm = hero.Charm;
        entity.Level = hero.Level;
        entity.Age = hero.Age;
        entity.Fame = hero.Fame;
        entity.Stipend = hero.Stipend;
        entity.Social = hero.Social;
        entity.Favor = hero.Favor;
        entity.Actions = hero.Actions;
        entity.Wounds = hero.Wounds;
        entity.Marks = hero.Marks;
        entity.Place = hero.Place.ToString();
        entity.State = hero.State.ToString();
        entity.FightRank = hero.FightRank;
        entity.MagicRank = hero.MagicRank;
        entity.ThiefRank = hero.ThiefRank;
        entity.IeatsuRank = hero.IeatsuRank;
        entity.Traits = string.Join(",", hero.Traits);

        var gearList = hero.Gear.Where(g => g != null).ToList();
        entity.GearJson = JsonSerializer.Serialize(gearList, _jsonOpts);

        // Remove Marks from pack before serializing (stored separately)
        var packWithoutMarks = hero.Pack.Where(p => p.Name != "Marks").ToList();
        entity.PackJson = JsonSerializer.Serialize(packWithoutMarks, _jsonOpts);

        entity.LooksJson = JsonSerializer.Serialize(hero.Looks, _jsonOpts);
        entity.LastPlayed = DateTime.UtcNow;
    }

    public async Task SaveHeroAsync(Hero hero, HeroEntity entity)
    {
        SyncToEntity(hero, entity);
        await _db.SaveChangesAsync();
    }

    public Monster GenerateEncounter(Hero hero)
    {
        var templates = MonsterData.GetMonstersForArea(hero.Place);
        var template = _rng.Pick(templates);
        return template.Instantiate(hero, _rng);
    }
}
