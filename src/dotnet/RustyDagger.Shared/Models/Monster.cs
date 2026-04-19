namespace RustyDagger.Shared.Models;

public class Monster
{
    public string Name { get; set; } = string.Empty;
    public int Guts { get; set; }
    public int Wits { get; set; }
    public int Charm { get; set; }

    public int Wounds { get; set; }

    // Gear slots
    public Arms?[] Gear { get; set; } = new Arms?[5];
    public List<PackItem> Pack { get; set; } = new();
    public List<PackItem> StatusEffects { get; set; } = new();

    // Interaction options (bribe, feed, riddle, trade, help, seduce, control, backstab, swindle)
    public HashSet<string> Options { get; set; } = new();
    public HashSet<string> Traits { get; set; } = new();

    public MonsterPassion Passion { get; set; } = MonsterPassion.Hostile;
    public int Stance { get; set; }
    public int Actions { get; set; } = 1;

    // Image path
    public string Picture { get; set; } = string.Empty;
    // Encounter text
    public string Description { get; set; } = string.Empty;

    // Guild skills
    public int FightRank { get; set; }
    public int MagicRank { get; set; }
    public int ThiefRank { get; set; }

    // Derived combat stats
    public int Attack => Gear.Where(g => g != null).Sum(g => g!.FullAttack());
    public int Defend => Gear.Where(g => g != null).Sum(g => g!.FullDefend());
    public int Skill
    {
        get
        {
            int s = Gear.Where(g => g != null).Sum(g => g!.FullSkill());
            s -= GetStatusCount("Disease");
            return s;
        }
    }

    public int MaxHealth => Guts;
    public int CurrentHealth => Guts - Wounds;

    public void Equip(Arms arms)
    {
        int slot = (int)arms.Slot;
        Gear[slot] = arms;
    }

    public bool HasTrait(string trait) => Traits.Contains(trait);

    public int GetStatusCount(string name)
    {
        var item = StatusEffects.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return item?.Count ?? 0;
    }

    public void AddStatus(string name, int count = 1)
    {
        var item = StatusEffects.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (item != null) item.Count += count;
        else StatusEffects.Add(new PackItem(name, count));
    }

    public void AddWounds(int damage)
    {
        Wounds += damage;
    }

    public bool IsDead => Wounds >= Guts;

    public int CalculateFame()
    {
        int totalSkill = Gear.Where(g => g != null).Sum(g => g!.FullSkill());
        return (Guts + Wits + Charm) / 30 + (totalSkill) / 4;
    }

    public int CalculateExp()
    {
        return ((1 + Attack) * (100 + Skill)) / 100;
    }
}
