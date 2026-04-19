namespace RustyDagger.Shared.Models;

public class Arms
{
    public string Name { get; set; } = string.Empty;
    public int BaseAttack { get; set; }
    public int BaseDefend { get; set; }
    public int BaseSkill { get; set; }
    public int Enchant { get; set; }
    public ArmsTrait Traits { get; set; }

    public EquipSlot Slot =>
        Traits.HasFlag(ArmsTrait.LEFT) ? EquipSlot.Left :
        Traits.HasFlag(ArmsTrait.HEAD) ? EquipSlot.Head :
        Traits.HasFlag(ArmsTrait.BODY) ? EquipSlot.Body :
        Traits.HasFlag(ArmsTrait.FEET) ? EquipSlot.Feet :
        EquipSlot.Right;

    public bool HasTrait(ArmsTrait trait) => Traits.HasFlag(trait);

    public int FullAttack()
    {
        int atk = BaseAttack + (Enchant + 9) / 10;
        if (HasTrait(ArmsTrait.FLAME) && HasTrait(ArmsTrait.RIGHT))
            atk += 8;
        return atk;
    }

    public int FullDefend()
    {
        int def = BaseDefend + (Enchant + 4) / 10;
        if (HasTrait(ArmsTrait.BLESS))
            def += 1;
        return def;
    }

    public int FullSkill()
    {
        int skl = BaseSkill + Enchant;
        if (HasTrait(ArmsTrait.LUCKY) && HasTrait(ArmsTrait.RIGHT))
            skl += 12;
        if (HasTrait(ArmsTrait.GLOWS))
            skl += 2;
        return skl;
    }

    public void Decay(int rate)
    {
        if (!HasTrait(ArmsTrait.DECAY)) return;
        if (Enchant > 0) Enchant--;
        else if (BaseAttack > 0) BaseAttack--;
        else if (BaseDefend > 0) BaseDefend--;
        else if (BaseSkill > 0) BaseSkill--;
    }

    public Arms Copy() => new()
    {
        Name = Name,
        BaseAttack = BaseAttack,
        BaseDefend = BaseDefend,
        BaseSkill = BaseSkill,
        Enchant = Enchant,
        Traits = Traits
    };
}
