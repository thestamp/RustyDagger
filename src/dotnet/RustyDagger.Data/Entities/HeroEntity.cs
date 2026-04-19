using System.ComponentModel.DataAnnotations;

namespace RustyDagger.Data.Entities;

public class HeroEntity
{
    public int Id { get; set; }
    public int AccountId { get; set; }

    [Required, MaxLength(15)]
    public string Name { get; set; } = string.Empty;

    public int Level { get; set; } = 1;
    public int Age { get; set; } = 16;
    public int Guts { get; set; } = 4;
    public int Wits { get; set; } = 4;
    public int Charm { get; set; } = 4;
    public int Wounds { get; set; }
    public int Fame { get; set; }
    public int Stipend { get; set; }
    public int Social { get; set; }
    public int Favor { get; set; }
    public int Actions { get; set; } = 30;
    public int Marks { get; set; } = 25;

    public string Place { get; set; } = "Fields";
    public string State { get; set; } = "Create";

    // Guild ranks (permanent)
    public int FightRank { get; set; }
    public int MagicRank { get; set; }
    public int ThiefRank { get; set; }
    public int IeatsuRank { get; set; }

    // Traits stored as comma-separated names
    [MaxLength(500)]
    public string Traits { get; set; } = string.Empty;

    // Experience toward next level
    public int Experience { get; set; }

    // Serialized JSON for complex fields
    public string GearJson { get; set; } = "[]";
    public string PackJson { get; set; } = "[]";
    public string LooksJson { get; set; } = "{}";

    // Persisted monster state for multi-round combat
    public string? CurrentMonsterJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastPlayed { get; set; } = DateTime.UtcNow;

    // Navigation
    public AccountEntity Account { get; set; } = null!;
}
