namespace RustyDagger.Shared.Models;

public class Hero
{
    public string Name { get; set; } = string.Empty;
    public int Guts { get; set; } = 4;
    public int Wits { get; set; } = 4;
    public int Charm { get; set; } = 4;

    public int Level { get; set; } = 1;
    public int Age { get; set; } = 16;
    public int Fame { get; set; }
    public int Stipend { get; set; }
    public int Social { get; set; }
    public int Favor { get; set; }

    public int Actions { get; set; } = 30;
    public int Wounds { get; set; }

    public GamePlace Place { get; set; } = GamePlace.Fields;
    public HeroState State { get; set; } = HeroState.Create;

    // Equipped gear (up to 5 slots)
    public Arms?[] Gear { get; set; } = new Arms?[5];

    // Pack inventory
    public List<PackItem> Pack { get; set; } = new();

    // Guild skill ranks (permanent)
    public int FightRank { get; set; }
    public int MagicRank { get; set; }
    public int ThiefRank { get; set; }
    public int IeatsuRank { get; set; }

    // Per-session guild uses
    public int FightUses { get; set; }
    public int MagicUses { get; set; }
    public int ThiefUses { get; set; }
    public int IeatsuUses { get; set; }

    // Traits (Noble, Warrior, Wizard, Trader, Fencer, Alert, Reflex, Ranger, etc.)
    public HashSet<string> Traits { get; set; } = new();

    // Appearance
    public HeroLooks Looks { get; set; } = new();

    // Marks (money)
    public int Marks
    {
        get => GetPackCount("Marks");
        set => SetPackCount("Marks", value);
    }

    // Food
    public int Food
    {
        get => GetPackCount("Food") + GetPackCount("Fish");
    }

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

    // Status effects (Blind, Panic, Disease)
    public List<PackItem> StatusEffects { get; set; } = new();

    public bool HasTrait(string trait) => Traits.Contains(trait);

    public int GetPackCount(string name)
    {
        var item = Pack.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return item?.Count ?? 0;
    }

    public void SetPackCount(string name, int count)
    {
        var item = Pack.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (item != null) item.Count = count;
        else if (count > 0) Pack.Add(new PackItem(name, count));
    }

    public void AddPackItem(string name, int count = 1)
    {
        var item = Pack.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (item != null) item.Count += count;
        else Pack.Add(new PackItem(name, count));
    }

    public bool RemovePackItem(string name, int count = 1)
    {
        var item = Pack.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (item == null || item.Count < count) return false;
        item.Count -= count;
        if (item.Count <= 0) Pack.Remove(item);
        return true;
    }

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

    public void ClearStatus(string name)
    {
        StatusEffects.RemoveAll(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void ClearAllStatus()
    {
        StatusEffects.Clear();
    }

    public void Equip(Arms arms)
    {
        int slot = (int)arms.Slot;
        Gear[slot] = arms;
    }

    public void Unequip(EquipSlot slot)
    {
        Gear[(int)slot] = null;
    }

    public void AddWounds(int damage)
    {
        Wounds += damage;
        if (Wounds >= Guts) State = HeroState.Dead;
    }

    public void Heal(int amount)
    {
        Wounds = Math.Max(0, Wounds - amount);
    }

    public void FullHeal()
    {
        Wounds = 0;
        ClearAllStatus();
    }

    public int CalculateActions()
    {
        return 27 + 3 * Level;
    }

    public void GainGuts(int weight, Engine.Rng rng)
    {
        if (rng.Roll(Guts) >= weight) Guts++;
    }

    public void GainWits(int weight, Engine.Rng rng)
    {
        if (rng.Roll(Wits) >= weight) Wits++;
    }

    public void GainCharm(int weight, Engine.Rng rng)
    {
        if (rng.Roll(Charm) >= weight) Charm++;
    }

    public void LevelUp()
    {
        Level++;
        Guts += 2;
        Wits += 2;
        Charm += 2;
        if (HasTrait("Trader") && Level % 8 == 0)
            ThiefRank++;
        if (HasTrait("Berzerk") && Level % 8 == 0)
            FightRank++;
        if (HasTrait("Mystic") && Level % 8 == 0)
            MagicRank++;
    }
}
