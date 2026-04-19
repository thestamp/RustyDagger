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
    public int Experience { get; set; }
    public int Fatigue { get; set; }

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
        return Math.Max(0, CalculateBaseActions() - Fatigue - CalculateOverload());
    }

    public int CalculateOverload()
    {
        int max = PackMax();
        int size = Pack.Count;
        return size > max ? size - max : 0;
    }

    public int PackMax()
    {
        int max = 60;
        if (HasTrait("Trader")) max += 20;
        if (HasTrait("Merchant")) max += 20;
        return max;
    }

    public void GainGuts(int weight, Engine.Rng rng)
    {
        if (rng.Roll(Guts) < weight) Guts++;
    }

    public void GainWits(int weight, Engine.Rng rng)
    {
        if (rng.Roll(Wits) < weight) Wits++;
    }

    public void GainCharm(int weight, Engine.Rng rng)
    {
        if (rng.Roll(Charm) < weight) Charm++;
    }

    public void LevelUp()
    {
        Level++;
        Guts += 2;
        Wits += 2;
        Charm += 2;
        Fame += Level;
        if (HasTrait("Trader") && Level % 8 == 0)
            ThiefRank++;
        if (HasTrait("Berzerk") && Level % 8 == 0)
            FightRank++;
        if (HasTrait("Mystic") && Level % 8 == 0)
            MagicRank++;
    }

    /// <summary>Experience needed to level up: 50 * 1.5^(level-1)</summary>
    public int CalculateRaise() => (int)(50 * Math.Pow(1.5, Level - 1));

    /// <summary>Try to level up if enough experience. Returns true if leveled.</summary>
    public bool TryToLevel()
    {
        int raise = CalculateRaise();
        if (Experience < raise) return false;
        Experience -= raise;
        LevelUp();
        return true;
    }

    /// <summary>Apply death penalty per Java killedScreen(). Returns log messages.</summary>
    public List<string> ApplyDeathPenalty(Engine.Rng rng, bool losePack)
    {
        var log = new List<string>();
        int cost = CalculateBaseActions() / 4;

        // Check for Bottled Faery
        if (GetPackCount("Bottled Faery") > 0)
        {
            RemovePackItem("Bottled Faery", 1);
            log.Add("A Bottled Faery breaks free from your pack and transports you to the healers!");
            // No penalties — heal and place in fields
            Wounds = Guts - (Guts / Level);
            CureDisease();
            State = HeroState.Town;
            Place = GamePlace.Fields;
            return log;
        }

        // Apply penalties
        log.Add("You are wounded mortally and fall to the ground.");
        log.Add("A friendly woodsman finds you and drags you to the healers.");

        // Fame reduced by 10%
        Fame = (Fame * 9) / 10;
        log.Add($"*** Your fame diminishes. ***");

        // Lose quests (fatigue)
        Fatigue += cost;
        log.Add($"*** You lose {cost} quests. ***");

        // Lose half pack items
        if (losePack)
        {
            LoseHalfPack(rng);
            log.Add("*** Half your equipment is lost. ***");
        }

        // Partial heal: wounds = guts - guts/level
        Wounds = Guts - (Guts / Level);
        CureDisease();

        // Revive in Fields
        State = HeroState.Town;
        Place = GamePlace.Fields;

        return log;
    }

    public void CureDisease()
    {
        ClearStatus("Disease");
        ClearStatus("Blind");
        ClearStatus("Panic");
    }

    public void LoseHalfPack(Engine.Rng rng)
    {
        var toRemove = new List<PackItem>();
        foreach (var item in Pack)
        {
            if (item.Name.Equals("Marks", StringComparison.OrdinalIgnoreCase)) continue;
            if (rng.Percent(50))
            {
                int half = item.Count / 2;
                if (half > 0) item.Count -= half;
                else toRemove.Add(item);
            }
        }
        foreach (var item in toRemove)
            Pack.Remove(item);
    }

    public int CalculateBaseActions()
    {
        return HasTrait("Quick") ? 27 + 4 * Level : 27 + 3 * Level;
    }
}
