namespace RustyDagger.Shared.Models;

public enum EquipSlot
{
    Right = 0,
    Left = 1,
    Head = 2,
    Body = 3,
    Feet = 4
}

[Flags]
public enum ArmsTrait
{
    None    = 0,
    RIGHT   = 1 << 0,
    LEFT    = 1 << 1,
    HEAD    = 1 << 2,
    BODY    = 1 << 3,
    FEET    = 1 << 4,
    FLAME   = 1 << 5,
    BLESS   = 1 << 6,
    LUCKY   = 1 << 7,
    GLOWS   = 1 << 8,
    DECAY   = 1 << 9,
    BLAST   = 1 << 10,
    BLIND   = 1 << 11,
    PANIC   = 1 << 12,
    DISEASE = 1 << 13
}

public enum GamePlace
{
    Town,
    Fields,
    Forest,
    Hills,
    Mound,
    Castle,
    Ocean,
    Brasil,
    Shang,
    Queen,
    Dungeon
}

public enum HeroState
{
    Create,
    Build,
    Town,
    Quest,
    Battle,
    Dead,
    Loading,
    Control,
    Swindle
}

public enum CombatAction
{
    Bribe = 0,
    Feed = 1,
    Riddle = 2,
    Trade = 3,
    Help = 4,
    Seduce = 5,
    Control = 6,
    Backstab = 7,
    Berzerk = 8,
    Swindle = 9,
    Ieatsu = 10,
    Attack = 11,
    Runaway = 12,
    Carp = 13,
    Bushido = 14,
    Capture = 15,
    Spells = 16
}

public enum MonsterPassion
{
    Passive = 0,
    Timid = 1,
    Defensive = 2,
    Hostile = 3,
    Aggressive = 4
}
