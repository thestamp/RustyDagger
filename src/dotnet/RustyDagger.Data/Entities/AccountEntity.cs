using System.ComponentModel.DataAnnotations;

namespace RustyDagger.Data.Entities;

public class AccountEntity
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;

    // Navigation
    public List<HeroEntity> Heroes { get; set; } = new();
}
