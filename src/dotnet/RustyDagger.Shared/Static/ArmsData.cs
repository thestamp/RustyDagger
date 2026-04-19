using RustyDagger.Shared.Models;

namespace RustyDagger.Shared.Static;

public static class ArmsData
{
    public static readonly Arms[] All = new Arms[]
    {
        // === Melee Weapons (RIGHT) ===
        new() { Name = "Rusty Dagger",     BaseAttack = 2,  BaseDefend = 0,  BaseSkill = -2,  Traits = ArmsTrait.RIGHT | ArmsTrait.DECAY },
        new() { Name = "Knife",            BaseAttack = 2,  BaseDefend = 0,  BaseSkill = 2,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Hatchet",          BaseAttack = 4,  BaseDefend = 0,  BaseSkill = 2,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Short Sword",      BaseAttack = 5,  BaseDefend = 0,  BaseSkill = 2,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Long Sword",       BaseAttack = 8,  BaseDefend = 0,  BaseSkill = 4,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Broad Sword",      BaseAttack = 12, BaseDefend = 0,  BaseSkill = 5,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Two-Hander",       BaseAttack = 16, BaseDefend = 0,  BaseSkill = 5,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Bastard Sword",    BaseAttack = 20, BaseDefend = 0,  BaseSkill = 10,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Claymore",         BaseAttack = 25, BaseDefend = 0,  BaseSkill = 12,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Flaming Sword",    BaseAttack = 30, BaseDefend = 0,  BaseSkill = 15,  Traits = ArmsTrait.RIGHT | ArmsTrait.FLAME },
        new() { Name = "Great Sword",      BaseAttack = 40, BaseDefend = 0,  BaseSkill = 20,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Masamune",         BaseAttack = 50, BaseDefend = 0,  BaseSkill = 40,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Short Spear",      BaseAttack = 9,  BaseDefend = 2,  BaseSkill = 2,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Long Spear",       BaseAttack = 14, BaseDefend = 4,  BaseSkill = 4,   Traits = ArmsTrait.RIGHT },
        new() { Name = "War Spear",        BaseAttack = 22, BaseDefend = 6,  BaseSkill = 6,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Great Spear",      BaseAttack = 35, BaseDefend = 8,  BaseSkill = 20,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Naginata",         BaseAttack = 54, BaseDefend = 10, BaseSkill = 24,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Hand Axe",         BaseAttack = 6,  BaseDefend = 0,  BaseSkill = 2,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Battle Axe",       BaseAttack = 13, BaseDefend = 0,  BaseSkill = 2,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Dwarf Axe",        BaseAttack = 16, BaseDefend = 0,  BaseSkill = 8,   Traits = ArmsTrait.RIGHT | ArmsTrait.BLESS | ArmsTrait.LUCKY },
        new() { Name = "Foul Axe",         BaseAttack = 20, BaseDefend = 0,  BaseSkill = 0,   Traits = ArmsTrait.RIGHT | ArmsTrait.DISEASE },
        new() { Name = "War Axe",          BaseAttack = 30, BaseDefend = 0,  BaseSkill = 5,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Walking Staff",    BaseAttack = 5,  BaseDefend = 4,  BaseSkill = 5,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Magic Staff",      BaseAttack = 10, BaseDefend = 5,  BaseSkill = 10,  Traits = ArmsTrait.RIGHT | ArmsTrait.FLAME | ArmsTrait.BLESS | ArmsTrait.LUCKY },
        new() { Name = "Mystic Staff",     BaseAttack = 15, BaseDefend = 8,  BaseSkill = 20,  Traits = ArmsTrait.RIGHT | ArmsTrait.BLESS | ArmsTrait.LUCKY },
        new() { Name = "Wizard Staff",     BaseAttack = 25, BaseDefend = 10, BaseSkill = 25,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Nunchaku",         BaseAttack = 25, BaseDefend = 10, BaseSkill = 10,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Shakrum",          BaseAttack = 13, BaseDefend = 0,  BaseSkill = 4,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Sword Breaker",    BaseAttack = 6,  BaseDefend = 6,  BaseSkill = 4,   Traits = ArmsTrait.RIGHT },
        new() { Name = "War Tusk",         BaseAttack = 7,  BaseDefend = 0,  BaseSkill = 2,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Maul",             BaseAttack = 45, BaseDefend = 0,  BaseSkill = 0,   Traits = ArmsTrait.RIGHT },
        new() { Name = "War Hammer",       BaseAttack = 50, BaseDefend = 0,  BaseSkill = 5,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Katana",           BaseAttack = 50, BaseDefend = 5,  BaseSkill = 30,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Whip",             BaseAttack = 25, BaseDefend = 0,  BaseSkill = 10,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Rat Tail Whip",    BaseAttack = 30, BaseDefend = 0,  BaseSkill = 12,  Traits = ArmsTrait.RIGHT | ArmsTrait.DISEASE },
        new() { Name = "Great Maul",       BaseAttack = 90, BaseDefend = 0,  BaseSkill = 0,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Silver Masamune",  BaseAttack = 50, BaseDefend = 0,  BaseSkill = 40,  Traits = ArmsTrait.RIGHT | ArmsTrait.PANIC },
        new() { Name = "Terror Rod",       BaseAttack = 20, BaseDefend = 5,  BaseSkill = 15,  Traits = ArmsTrait.RIGHT | ArmsTrait.PANIC },
        new() { Name = "Silver Weird Knife", BaseAttack = 4, BaseDefend = 0, BaseSkill = 2,   Traits = ArmsTrait.RIGHT | ArmsTrait.DISEASE },
        new() { Name = "Silver Throwing Knife", BaseAttack = 6, BaseDefend = 0, BaseSkill = 6, Traits = ArmsTrait.RIGHT | ArmsTrait.PANIC },
        new() { Name = "Silver Gladius",   BaseAttack = 15, BaseDefend = 0,  BaseSkill = 8,   Traits = ArmsTrait.RIGHT | ArmsTrait.BLIND },

        // === Ranged (RIGHT) ===
        new() { Name = "Sling",            BaseAttack = 4,  BaseDefend = 0,  BaseSkill = 4,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Short Bow",        BaseAttack = 8,  BaseDefend = 0,  BaseSkill = 8,   Traits = ArmsTrait.RIGHT },
        new() { Name = "Long Bow",         BaseAttack = 12, BaseDefend = 0,  BaseSkill = 12,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Crossbow",         BaseAttack = 20, BaseDefend = 0,  BaseSkill = 10,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Elf Bow",          BaseAttack = 20, BaseDefend = 0,  BaseSkill = 20,  Traits = ArmsTrait.RIGHT },
        new() { Name = "War Bow",          BaseAttack = 25, BaseDefend = 0,  BaseSkill = 25,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Great Bow",        BaseAttack = 35, BaseDefend = 0,  BaseSkill = 35,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Shuriken",         BaseAttack = 30, BaseDefend = 0,  BaseSkill = 20,  Traits = ArmsTrait.RIGHT },
        new() { Name = "Matchlock Rifle",  BaseAttack = 50, BaseDefend = 0,  BaseSkill = 10,  Traits = ArmsTrait.RIGHT | ArmsTrait.BLAST },
        new() { Name = "Silver Elf Bow",   BaseAttack = 20, BaseDefend = 0,  BaseSkill = 20,  Traits = ArmsTrait.RIGHT | ArmsTrait.BLAST },

        // === Shields (LEFT) ===
        new() { Name = "Buckler",          BaseAttack = 0,  BaseDefend = 2,  BaseSkill = 0,   Traits = ArmsTrait.LEFT },
        new() { Name = "Small Buckler",    BaseAttack = 0,  BaseDefend = 3,  BaseSkill = 0,   Traits = ArmsTrait.LEFT },
        new() { Name = "Large Buckler",    BaseAttack = 0,  BaseDefend = 5,  BaseSkill = 0,   Traits = ArmsTrait.LEFT },
        new() { Name = "Small Targe",      BaseAttack = 0,  BaseDefend = 2,  BaseSkill = 2,   Traits = ArmsTrait.LEFT },
        new() { Name = "Large Targe",      BaseAttack = 0,  BaseDefend = 4,  BaseSkill = 2,   Traits = ArmsTrait.LEFT },
        new() { Name = "Great Targe",      BaseAttack = 0,  BaseDefend = 35, BaseSkill = 15,  Traits = ArmsTrait.LEFT },
        new() { Name = "Small Shield",     BaseAttack = 0,  BaseDefend = 3,  BaseSkill = 0,   Traits = ArmsTrait.LEFT },
        new() { Name = "Large Shield",     BaseAttack = 0,  BaseDefend = 4,  BaseSkill = 0,   Traits = ArmsTrait.LEFT },
        new() { Name = "Great Shield",     BaseAttack = 0,  BaseDefend = 6,  BaseSkill = 0,   Traits = ArmsTrait.LEFT },
        new() { Name = "Spike Shield",     BaseAttack = 3,  BaseDefend = 5,  BaseSkill = 0,   Traits = ArmsTrait.LEFT },
        new() { Name = "Dragon Shield",    BaseAttack = 0,  BaseDefend = 25, BaseSkill = 10,  Traits = ArmsTrait.LEFT },
        new() { Name = "Serpent Shield",   BaseAttack = 0,  BaseDefend = 20, BaseSkill = 10,  Traits = ArmsTrait.LEFT },
        new() { Name = "Goblin Shield",    BaseAttack = 0,  BaseDefend = 13, BaseSkill = 5,   Traits = ArmsTrait.LEFT },
        new() { Name = "Ram's Horn",       BaseAttack = 7,  BaseDefend = 13, BaseSkill = 8,   Traits = ArmsTrait.LEFT },
        new() { Name = "Unicorn Horn",     BaseAttack = 3,  BaseDefend = 10, BaseSkill = 5,   Traits = ArmsTrait.LEFT | ArmsTrait.BLESS },

        // === Head ===
        new() { Name = "Skull Cap",        BaseAttack = 0,  BaseDefend = 1,  BaseSkill = 0,   Traits = ArmsTrait.HEAD },
        new() { Name = "Iron Cap",         BaseAttack = 0,  BaseDefend = 2,  BaseSkill = 0,   Traits = ArmsTrait.HEAD },
        new() { Name = "Miners Cap",       BaseAttack = 0,  BaseDefend = 1,  BaseSkill = 2,   Traits = ArmsTrait.HEAD | ArmsTrait.GLOWS },
        new() { Name = "Iron Pot",         BaseAttack = 0,  BaseDefend = 2,  BaseSkill = -2,  Traits = ArmsTrait.HEAD },
        new() { Name = "Steel Pot",        BaseAttack = 0,  BaseDefend = 3,  BaseSkill = -3,  Traits = ArmsTrait.HEAD },
        new() { Name = "Great Pot",        BaseAttack = 0,  BaseDefend = 4,  BaseSkill = -4,  Traits = ArmsTrait.HEAD },
        new() { Name = "Chain Coif",       BaseAttack = 0,  BaseDefend = 4,  BaseSkill = 0,   Traits = ArmsTrait.HEAD },
        new() { Name = "Scale Coif",       BaseAttack = 0,  BaseDefend = 5,  BaseSkill = 0,   Traits = ArmsTrait.HEAD },
        new() { Name = "Plate Coif",       BaseAttack = 0,  BaseDefend = 6,  BaseSkill = 0,   Traits = ArmsTrait.HEAD },
        new() { Name = "Roman Helm",       BaseAttack = 0,  BaseDefend = 7,  BaseSkill = 0,   Traits = ArmsTrait.HEAD },

        // === Body Armor ===
        new() { Name = "Robes",            BaseAttack = 0,  BaseDefend = 1,  BaseSkill = 0,   Traits = ArmsTrait.BODY },
        new() { Name = "Magic Robes",      BaseAttack = 0,  BaseDefend = 3,  BaseSkill = 5,   Traits = ArmsTrait.BODY | ArmsTrait.FLAME | ArmsTrait.BLESS | ArmsTrait.LUCKY },
        new() { Name = "Mystic Robes",     BaseAttack = 0,  BaseDefend = 5,  BaseSkill = 10,  Traits = ArmsTrait.BODY | ArmsTrait.BLESS | ArmsTrait.LUCKY },
        new() { Name = "Soft Leather",     BaseAttack = 0,  BaseDefend = 2,  BaseSkill = 0,   Traits = ArmsTrait.BODY },
        new() { Name = "Hard Leather",     BaseAttack = 0,  BaseDefend = 3,  BaseSkill = 0,   Traits = ArmsTrait.BODY },
        new() { Name = "Studded Leather",  BaseAttack = 0,  BaseDefend = 4,  BaseSkill = 0,   Traits = ArmsTrait.BODY },
        new() { Name = "Brigandine",       BaseAttack = 0,  BaseDefend = 5,  BaseSkill = 0,   Traits = ArmsTrait.BODY },
        new() { Name = "Chain Mail",       BaseAttack = 0,  BaseDefend = 5,  BaseSkill = -2,  Traits = ArmsTrait.BODY },
        new() { Name = "Banded Mail",      BaseAttack = 0,  BaseDefend = 6,  BaseSkill = -3,  Traits = ArmsTrait.BODY },
        new() { Name = "Scale Mail",       BaseAttack = 0,  BaseDefend = 7,  BaseSkill = -4,  Traits = ArmsTrait.BODY },
        new() { Name = "Heavy Scale",      BaseAttack = 0,  BaseDefend = 10, BaseSkill = -5,  Traits = ArmsTrait.BODY },
        new() { Name = "Half Plate",       BaseAttack = 0,  BaseDefend = 10, BaseSkill = -8,  Traits = ArmsTrait.BODY },
        new() { Name = "Full Plate",       BaseAttack = 0,  BaseDefend = 13, BaseSkill = -10, Traits = ArmsTrait.BODY },
        new() { Name = "Great Plate",      BaseAttack = 0,  BaseDefend = 15, BaseSkill = -10, Traits = ArmsTrait.BODY },
        new() { Name = "Mithril Mail",     BaseAttack = 0,  BaseDefend = 8,  BaseSkill = 0,   Traits = ArmsTrait.BODY },
        new() { Name = "Mithril Scale",    BaseAttack = 0,  BaseDefend = 10, BaseSkill = 0,   Traits = ArmsTrait.BODY },
        new() { Name = "Mithril Plate",    BaseAttack = 0,  BaseDefend = 13, BaseSkill = 0,   Traits = ArmsTrait.BODY },
        new() { Name = "Snake Scale",      BaseAttack = 0,  BaseDefend = 50, BaseSkill = 25,  Traits = ArmsTrait.BODY },

        // === Feet ===
        new() { Name = "Sandals",          BaseAttack = 0,  BaseDefend = 0,  BaseSkill = 2,   Traits = ArmsTrait.FEET },
        new() { Name = "Shoes",            BaseAttack = 0,  BaseDefend = 1,  BaseSkill = 2,   Traits = ArmsTrait.FEET },
        new() { Name = "Half Boots",       BaseAttack = 0,  BaseDefend = 1,  BaseSkill = 3,   Traits = ArmsTrait.FEET },
        new() { Name = "High Boots",       BaseAttack = 0,  BaseDefend = 2,  BaseSkill = 4,   Traits = ArmsTrait.FEET },
        new() { Name = "Armored Boots",    BaseAttack = 0,  BaseDefend = 3,  BaseSkill = 2,   Traits = ArmsTrait.FEET },
        new() { Name = "Doc Martins",      BaseAttack = 4,  BaseDefend = 3,  BaseSkill = 0,   Traits = ArmsTrait.FEET },
        new() { Name = "War Boots",        BaseAttack = 7,  BaseDefend = 4,  BaseSkill = 2,   Traits = ArmsTrait.FEET },
        new() { Name = "Mercury Sandals",  BaseAttack = 0,  BaseDefend = 0,  BaseSkill = 20,  Traits = ArmsTrait.FEET },
        new() { Name = "Asaura Boots",     BaseAttack = 0,  BaseDefend = 5,  BaseSkill = 15,  Traits = ArmsTrait.FEET },
        new() { Name = "Sea Slippers",     BaseAttack = 0,  BaseDefend = 0,  BaseSkill = 200, Traits = ArmsTrait.FEET },
    };

    private static readonly Dictionary<string, Arms> _lookup = All.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);

    public static Arms? Find(string name) =>
        _lookup.TryGetValue(name, out var arms) ? arms.Copy() : null;
}
