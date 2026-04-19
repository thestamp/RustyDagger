using System.ComponentModel.DataAnnotations;

namespace RustyDagger.Shared.Dto;

public class LoginRequest
{
    [Required, MinLength(4), MaxLength(15)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(1)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required, MinLength(4), MaxLength(15)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public class HeroSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Guts { get; set; }
    public int Wits { get; set; }
    public int Charm { get; set; }
    public int Fame { get; set; }
    public int Actions { get; set; }
    public int Wounds { get; set; }
    public int Experience { get; set; }
    public string Place { get; set; } = string.Empty;
}

public class CreateHeroRequest
{
    [Required, MinLength(4), MaxLength(15)]
    public string Name { get; set; } = string.Empty;

    [Range(4, 20)]
    public int Guts { get; set; } = 4;

    [Range(4, 20)]
    public int Wits { get; set; } = 4;

    [Range(4, 20)]
    public int Charm { get; set; } = 4;

    [Range(0, 12)]
    public int Money { get; set; } = 1;

    public string? Trait { get; set; }
}

public class GameActionRequest
{
    public int HeroId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Target { get; set; }
    public int? Choice { get; set; }
}

public class GameStateResponse
{
    public HeroSummary Hero { get; set; } = new();
    public string Screen { get; set; } = string.Empty;
    public string? AreaDescription { get; set; }
    public List<string> AvailableActions { get; set; } = new();
    public List<string> Log { get; set; } = new();
    public MonsterView? CurrentMonster { get; set; }
}

public class MonsterView
{
    public string Name { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int HealthPercent { get; set; }
    public bool IsDead { get; set; }
}

public class RankingEntry
{
    public string HeroName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Fame { get; set; }
    public int Guts { get; set; }
    public int Wits { get; set; }
    public int Charm { get; set; }
}
