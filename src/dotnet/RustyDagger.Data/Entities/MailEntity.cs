using System.ComponentModel.DataAnnotations;

namespace RustyDagger.Data.Entities;

public class MailEntity
{
    public int Id { get; set; }

    public int RecipientAccountId { get; set; }

    [Required, MaxLength(50)]
    public string FromName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Subject { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Body { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}

public class RankingEntity
{
    public int Id { get; set; }
    public int HeroId { get; set; }

    [Required, MaxLength(50)]
    public string HeroName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string AccountName { get; set; } = string.Empty;

    public int Level { get; set; }
    public int Fame { get; set; }
    public int Guts { get; set; }
    public int Wits { get; set; }
    public int Charm { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
