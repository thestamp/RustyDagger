using RustyDagger.Shared.Engine;
using RustyDagger.Shared.Models;

namespace RustyDagger.Shared.Static;

public class MonsterTemplate
{
    public string Name { get; init; } = string.Empty;
    public int Guts { get; init; }
    public int Wits { get; init; }
    public int Charm { get; init; }
    public string Picture { get; init; } = string.Empty;
    public MonsterPassion Passion { get; init; }
    public string Description { get; init; } = string.Empty;
    public string[] Options { get; init; } = Array.Empty<string>();
    public string[] GearNames { get; init; } = Array.Empty<string>();
    public (string Name, int Chance)[] LootPercent { get; init; } = Array.Empty<(string, int)>();
    public (string Name, int Max)[] LootRandom { get; init; } = Array.Empty<(string, int)>();
    public int FightRank { get; init; }
    public int MagicRank { get; init; }
    public int ThiefRank { get; init; }
    public string[] Traits { get; init; } = Array.Empty<string>();

    public Monster Instantiate(Hero hero, Rng rng)
    {
        double scale = 0.9 + 0.1 * hero.Level;
        var mob = new Monster
        {
            Name = Name,
            Guts = rng.Spread((int)(Guts * scale)),
            Wits = rng.Spread((int)(Wits * scale)),
            Charm = rng.Spread((int)(Charm * scale)),
            Picture = Picture,
            Description = Description,
            Passion = Passion,
            Stance = (int)Passion,
            FightRank = FightRank,
            MagicRank = MagicRank,
            ThiefRank = ThiefRank
        };

        foreach (var opt in Options) mob.Options.Add(opt);
        foreach (var trait in Traits) mob.Traits.Add(trait);

        // Equip gear
        foreach (var gearName in GearNames)
        {
            var arms = ArmsData.Find(gearName);
            if (arms != null) mob.Equip(arms);
        }

        // Build loot pack
        foreach (var (name, chance) in LootPercent)
        {
            if (rng.Percent(chance))
                mob.Pack.Add(new PackItem(name, 1));
        }
        foreach (var (name, max) in LootRandom)
        {
            int count = rng.Roll(max) + 1;
            mob.Pack.Add(new PackItem(name, count));
        }

        return mob;
    }

    private void Equip(Monster mob, Arms arms)
    {
        int slot = (int)arms.Slot;
        mob.Gear[slot] = arms;
    }
}

public static class MonsterData
{
    // === FIELDS MONSTERS ===
    public static readonly MonsterTemplate Rodent = new()
    {
        Name = "Rodent", Guts = 8, Wits = 5, Charm = 2,
        Picture = "Fields/Rodent.jpg", Passion = MonsterPassion.Timid,
        Description = "A large, feral rodent lunges from the tall grass!",
        Options = new[] { "feed", "backstab", "control" },
        GearNames = new[] { "Knife" },
        LootPercent = new[] { ("Teeth", 80) },
        LootRandom = new[] { ("Food", 5) }
    };

    public static readonly MonsterTemplate Goblin = new()
    {
        Name = "Goblin", Guts = 15, Wits = 10, Charm = 5,
        Picture = "Fields/Goblin.jpg", Passion = MonsterPassion.Hostile,
        Description = "A scrappy goblin jumps out, waving a rusty blade!",
        Options = new[] { "bribe", "feed", "backstab", "control", "swindle" },
        GearNames = new[] { "Short Sword", "Buckler" },
        LootPercent = new[] { ("Short Sword", 20) },
        LootRandom = new[] { ("Marks", 20) }
    };

    public static readonly MonsterTemplate Centaur = new()
    {
        Name = "Centaur", Guts = 25, Wits = 20, Charm = 15,
        Picture = "Fields/Centaur.jpg", Passion = MonsterPassion.Defensive,
        Description = "A proud centaur blocks your path, spear at the ready.",
        Options = new[] { "bribe", "riddle", "trade", "help", "seduce", "backstab", "control", "swindle" },
        GearNames = new[] { "War Spear" },
        LootPercent = new[] { ("War Spear", 10), ("Long Bow", 10) },
        LootRandom = new[] { ("Marks", 100) }
    };

    public static readonly MonsterTemplate Merchant = new()
    {
        Name = "Merchant", Guts = 20, Wits = 15, Charm = 25,
        Picture = "Fields/Oldman.jpg", Passion = MonsterPassion.Passive,
        Description = "A traveling merchant approaches with a loaded cart.",
        Options = new[] { "bribe", "trade", "help", "seduce", "backstab", "swindle" },
        GearNames = new[] { "Walking Staff" },
        LootRandom = new[] { ("Marks", 500) }
    };

    public static readonly MonsterTemplate FieldsWizard = new()
    {
        Name = "Wizard", Guts = 20, Wits = 30, Charm = 15,
        Picture = "Fields/Oldman.jpg", Passion = MonsterPassion.Defensive,
        Description = "A mysterious wizard bars the path, staff aglow.",
        Options = new[] { "riddle", "help", "backstab", "control" },
        GearNames = new[] { "Magic Staff" },
        MagicRank = 3,
        LootPercent = new[] { ("Magic Staff", 5) },
        LootRandom = new[] { ("Marks", 50) }
    };

    public static readonly MonsterTemplate Gypsy = new()
    {
        Name = "Gypsy", Guts = 15, Wits = 20, Charm = 20,
        Picture = "Fields/Gypsy.jpg", Passion = MonsterPassion.Passive,
        Description = "A brightly-dressed gypsy woman waves you closer.",
        Options = new[] { "bribe", "riddle", "trade", "seduce", "backstab", "swindle" },
        GearNames = new[] { "Knife" },
        ThiefRank = 2,
        LootRandom = new[] { ("Marks", 50) }
    };

    public static readonly MonsterTemplate Soldier = new()
    {
        Name = "Soldier", Guts = 30, Wits = 15, Charm = 10,
        Picture = "Fields/Soldier.jpg", Passion = MonsterPassion.Aggressive,
        Description = "An armored soldier charges on sight!",
        Options = new[] { "bribe", "backstab", "control" },
        GearNames = new[] { "Long Sword", "Large Shield", "Chain Mail" },
        FightRank = 1,
        LootPercent = new[] { ("Long Sword", 30), ("Chain Mail", 20) },
        LootRandom = new[] { ("Marks", 80) }
    };

    // === FOREST MONSTERS ===
    public static readonly MonsterTemplate Boar = new()
    {
        Name = "Boar", Guts = 20, Wits = 8, Charm = 5,
        Picture = "Forest/Boar.jpg", Passion = MonsterPassion.Aggressive,
        Description = "A massive boar bursts from the underbrush!",
        Options = new[] { "feed", "backstab", "control" },
        GearNames = new[] { "Hatchet" },
        LootPercent = new[] { ("Tusks", 80), ("War Tusk", 20) },
        LootRandom = new[] { ("Food", 5) }
    };

    public static readonly MonsterTemplate Orc = new()
    {
        Name = "Orc", Guts = 30, Wits = 12, Charm = 8,
        Picture = "Forest/Orc.jpg", Passion = MonsterPassion.Hostile,
        Description = "An orc war party spots you through the trees!",
        Options = new[] { "bribe", "feed", "backstab", "control", "swindle" },
        GearNames = new[] { "Battle Axe", "Hard Leather" },
        FightRank = 1,
        LootPercent = new[] { ("Battle Axe", 20), ("Hard Leather", 30) },
        LootRandom = new[] { ("Marks", 50) }
    };

    public static readonly MonsterTemplate Elf = new()
    {
        Name = "Elf", Guts = 20, Wits = 25, Charm = 20,
        Picture = "Forest/Elf.jpg", Passion = MonsterPassion.Defensive,
        Description = "An elven archer appears silently from the canopy.",
        Options = new[] { "riddle", "trade", "help", "seduce", "backstab", "control", "swindle" },
        GearNames = new[] { "Elf Bow", "Studded Leather" },
        MagicRank = 2,
        LootPercent = new[] { ("Elf Bow", 15) },
        LootRandom = new[] { ("Marks", 100) }
    };

    public static readonly MonsterTemplate Gryphon = new()
    {
        Name = "Gryphon", Guts = 40, Wits = 20, Charm = 10,
        Picture = "Forest/Gryphon.jpg", Passion = MonsterPassion.Hostile,
        Description = "A majestic gryphon swoops down with razor talons!",
        Options = new[] { "feed", "backstab", "control" },
        GearNames = new[] { "War Spear" },
        LootPercent = new[] { ("Horn", 30) },
        LootRandom = new[] { ("Marks", 100) }
    };

    public static readonly MonsterTemplate Unicorn = new()
    {
        Name = "Unicorn", Guts = 35, Wits = 30, Charm = 30,
        Picture = "Forest/Unicorn.jpg", Passion = MonsterPassion.Passive,
        Description = "A magnificent unicorn steps into the moonlit glade.",
        Options = new[] { "riddle", "help", "seduce", "capture", "control" },
        GearNames = new[] { "Unicorn Horn" },
        LootPercent = new[] { ("Unicorn Horn", 5) }
    };

    // === HILLS MONSTERS ===
    public static readonly MonsterTemplate Goat = new()
    {
        Name = "Goat", Guts = 20, Wits = 10, Charm = 5,
        Picture = "Hills/Goat.jpg", Passion = MonsterPassion.Defensive,
        Description = "A stubborn mountain goat blocks the narrow path.",
        Options = new[] { "feed", "backstab", "control" },
        GearNames = new[] { "Hand Axe" },
        LootPercent = new[] { ("Horn", 50) }
    };

    public static readonly MonsterTemplate Troll = new()
    {
        Name = "Troll", Guts = 60, Wits = 15, Charm = 8,
        Picture = "Hills/Troll.jpg", Passion = MonsterPassion.Aggressive,
        Description = "A massive troll lumbers from beneath a stone bridge!",
        Options = new[] { "bribe", "feed", "backstab", "control" },
        GearNames = new[] { "War Axe", "Hard Leather" },
        FightRank = 2,
        Traits = new[] { "Hardy" },
        LootPercent = new[] { ("Wart", 80) },
        LootRandom = new[] { ("Marks", 200) }
    };

    public static readonly MonsterTemplate Giant = new()
    {
        Name = "Giant", Guts = 80, Wits = 20, Charm = 10,
        Picture = "Hills/Giant.jpg", Passion = MonsterPassion.Aggressive,
        Description = "The ground shakes as a hill giant stomps into view!",
        Options = new[] { "bribe", "backstab", "control" },
        GearNames = new[] { "Great Maul", "Great Plate" },
        FightRank = 3,
        LootPercent = new[] { ("Crown", 20), ("Great Maul", 5) },
        LootRandom = new[] { ("Marks", 500) }
    };

    public static readonly MonsterTemplate Dragon = new()
    {
        Name = "Dragon", Guts = 250, Wits = 250, Charm = 250,
        Picture = "Hills/Dragon.jpg", Passion = MonsterPassion.Aggressive,
        Description = "An ancient dragon rises with flame and fury!",
        Options = new[] { "backstab", "control" },
        GearNames = new[] { "Flaming Sword", "Snake Scale" },
        FightRank = 5, MagicRank = 5,
        Traits = new[] { "Stubborn", "Clever" },
        LootPercent = new[] { ("Dragon Shield", 10), ("Scales", 80), ("Masamune", 2) },
        LootRandom = new[] { ("Marks", 2000) }
    };

    // === MOUND MONSTERS ===
    public static readonly MonsterTemplate GateGuard = new()
    {
        Name = "Gate Guard", Guts = 30, Wits = 20, Charm = 15,
        Picture = "Mound/Gate.jpg", Passion = MonsterPassion.Hostile,
        Description = "A goblin gate guard challenges intruders!",
        Options = new[] { "bribe", "feed", "backstab", "control", "swindle" },
        GearNames = new[] { "Short Spear", "Buckler" },
        LootRandom = new[] { ("Marks", 30) }
    };

    public static readonly MonsterTemplate Gang = new()
    {
        Name = "Gang", Guts = 25, Wits = 15, Charm = 10,
        Picture = "Mound/Gang.jpg", Passion = MonsterPassion.Aggressive,
        Description = "A gang of goblin miners swarms around you!",
        Options = new[] { "bribe", "feed", "backstab", "control" },
        GearNames = new[] { "Hatchet" },
        LootRandom = new[] { ("Marks", 20), ("Nugget", 2) }
    };

    public static readonly MonsterTemplate TownGuard = new()
    {
        Name = "Town Guard", Guts = 50, Wits = 50, Charm = 50,
        Picture = "Faces/Gareth.jpg", Passion = MonsterPassion.Hostile,
        Description = "A guard challenges you at the castle gate.",
        Options = new[] { "help", "backstab", "swindle", "control" },
        GearNames = new[] { "Long Sword", "Chain Mail" },
        LootRandom = new[] { ("Marks", 5) }
    };

    // Area monster pools
    public static readonly MonsterTemplate[] FieldsMonsters = { Rodent, Goblin, Centaur, Merchant, FieldsWizard, Gypsy, Soldier };
    public static readonly MonsterTemplate[] ForestMonsters = { Boar, Orc, Elf, Gryphon, Unicorn };
    public static readonly MonsterTemplate[] HillsMonsters = { Goat, Troll, Giant, Dragon };
    public static readonly MonsterTemplate[] MoundMonsters = { GateGuard, Gang };

    public static MonsterTemplate[] GetMonstersForArea(GamePlace place) => place switch
    {
        GamePlace.Fields => FieldsMonsters,
        GamePlace.Forest => ForestMonsters,
        GamePlace.Hills => HillsMonsters,
        GamePlace.Mound => MoundMonsters,
        _ => FieldsMonsters
    };
}
