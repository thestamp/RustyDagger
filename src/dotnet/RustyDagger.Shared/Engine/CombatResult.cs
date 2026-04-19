using RustyDagger.Shared.Models;

namespace RustyDagger.Shared.Engine;

public class CombatResult
{
    public string AttackerName { get; set; } = string.Empty;
    public string DefenderName { get; set; } = string.Empty;
    public CombatAction HeroAction { get; set; }
    public int Damage { get; set; }
    public int HitLevel { get; set; }
    public bool HeroFirst { get; set; }
    public bool MonsterKilled { get; set; }
    public bool HeroKilled { get; set; }
    public bool HeroFled { get; set; }
    public bool MonsterFled { get; set; }
    public bool Controlled { get; set; }
    public bool Swindled { get; set; }
    public int HeroDamage { get; set; }
    public int MonsterDamage { get; set; }
    public List<string> Log { get; set; } = new();

    public static readonly string[] HitLabels =
    {
        "DODGED!", "Unharmed", "Scratched", "Injured!", "Wounded!!", "KILLED!!!"
    };

    public static readonly string[] PowerLabels =
    {
        "Fly Swat", "Weak Blow", "Good Hit", "Potent Hit", "POWER HIT!"
    };
}
