using RustyDagger.Shared.Models;

namespace RustyDagger.Shared.Engine;

public class CombatEngine
{
    private readonly Rng _rng;

    public CombatEngine(Rng rng) => _rng = rng;

    public CombatResult ResolveCombat(Hero hero, Monster mob, CombatAction heroAction)
    {
        var result = new CombatResult
        {
            AttackerName = hero.Name,
            DefenderName = mob.Name,
            HeroAction = heroAction
        };

        // Handle flee
        if (heroAction == CombatAction.Runaway)
        {
            int hspeed = GetHeroSpeed(hero);
            int mspeed = GetMonsterSpeed(mob);
            if (_rng.Contest(hspeed, mspeed))
            {
                result.HeroFled = true;
                result.Log.Add($"{hero.Name} escapes!");
                return result;
            }
            result.Log.Add($"{hero.Name} fails to escape!");
            // Monster gets a free attack
            ResolveMonsterAttack(hero, mob, result);
            return result;
        }

        // Handle social actions
        if (heroAction <= CombatAction.Seduce)
            return ResolveSocialAction(hero, mob, heroAction, result);

        // Handle Control (magic)
        if (heroAction == CombatAction.Control)
            return ResolveControl(hero, mob, result);

        // Handle Swindle (thief)
        if (heroAction == CombatAction.Swindle)
            return ResolveSwindle(hero, mob, result);

        // Combat actions
        int heroGuts = hero.Guts;
        int heroSpeed = GetHeroSpeed(hero);
        int monsterSpeed = GetMonsterSpeed(mob);
        int heroHit = _rng.Twice(3);

        // Action-specific modifiers
        switch (heroAction)
        {
            case CombatAction.Backstab:
                heroSpeed *= 2;
                heroGuts *= 2;
                heroHit = 1;
                if (hero.ThiefUses > 0) hero.ThiefUses--;
                break;
            case CombatAction.Berzerk:
                heroSpeed *= 2;
                heroGuts *= 2;
                heroHit = 4;
                if (hero.FightUses > 0) hero.FightUses--;
                break;
            case CombatAction.Ieatsu:
                heroSpeed *= 2;
                heroGuts *= 2;
                heroHit = 4;
                if (hero.IeatsuUses > 0) hero.IeatsuUses--;
                break;
        }

        if (hero.GetStatusCount("Blind") > 0) heroHit /= 2;

        result.HitLevel = Math.Min(heroHit, 4);
        result.Log.Add($"{hero.Name}: {CombatResult.PowerLabels[result.HitLevel]}");

        // Determine initiative
        result.HeroFirst = _rng.Contest(heroSpeed, monsterSpeed);

        if (result.HeroFirst)
        {
            ResolveHeroAttack(hero, mob, heroGuts, heroHit, heroSpeed, monsterSpeed, result);
            if (!mob.IsDead)
                ResolveMonsterAttack(hero, mob, result);
        }
        else
        {
            ResolveMonsterAttack(hero, mob, result);
            if (hero.State != HeroState.Dead)
                ResolveHeroAttack(hero, mob, heroGuts, heroHit, heroSpeed, monsterSpeed, result);
        }

        // Apply spell effects (Blind, Panic, Disease)
        ApplySpellEffects(hero, mob, result);

        result.MonsterKilled = mob.IsDead;
        result.HeroKilled = hero.State == HeroState.Dead;

        return result;
    }

    private void ResolveHeroAttack(Hero hero, Monster mob, int guts, int hit, int asSpeed, int dsSpeed, CombatResult result)
    {
        if (hero.HasTrait("Backstab") && mob.HasTrait("Alert"))
            dsSpeed += 30;
        if (hero.HasTrait("Berzerk") && mob.HasTrait("Fencer"))
            dsSpeed += 30;

        if (_rng.Roll(dsSpeed) > asSpeed)
        {
            result.Log.Add($"{mob.Name}: DODGED!");
            return;
        }

        int damage = Math.Max(0, (guts * (2 + hit)) / 10 + hero.Attack - mob.Defend);
        result.HeroDamage = damage;

        int maxHealth = mob.CurrentHealth;
        int stk;
        if (damage < 1) stk = 1;
        else if (damage >= maxHealth) stk = 5;
        else stk = 2 + 3 * damage / maxHealth;

        result.Log.Add($"{mob.Name}: {CombatResult.HitLabels[stk]} ({damage} dmg)");

        if (stk > 1)
            mob.AddWounds(damage);

        // Weapon secondary effects
        var weapon = hero.Gear[(int)EquipSlot.Right];
        if (weapon != null && stk >= 2)
        {
            if (weapon.HasTrait(ArmsTrait.BLIND))
                hero.AddStatus("Blind-Apply", 1);
            if (weapon.HasTrait(ArmsTrait.PANIC))
                hero.AddStatus("Panic-Apply", 1);
            if (weapon.HasTrait(ArmsTrait.DISEASE))
                hero.AddStatus("Disease-Apply", (damage + 3) / 5);
        }
    }

    private void ResolveMonsterAttack(Hero hero, Monster mob, CombatResult result)
    {
        int mspeed = GetMonsterSpeed(mob);
        int hspeed = GetHeroSpeed(hero);
        int mguts = mob.Guts;
        int mhit = _rng.Twice(3);

        if (mob.GetStatusCount("Blind") > 0) mhit /= 2;

        if (_rng.Roll(hspeed) > mspeed)
        {
            result.Log.Add($"{hero.Name}: DODGED!");
            return;
        }

        int damage = Math.Max(0, (mguts * (2 + mhit)) / 10 + mob.Attack - hero.Defend);
        result.MonsterDamage = damage;

        int maxHealth = hero.CurrentHealth;
        int stk;
        if (damage < 1) stk = 1;
        else if (damage >= maxHealth) stk = 5;
        else stk = 2 + 3 * damage / maxHealth;

        result.Log.Add($"{hero.Name}: {CombatResult.HitLabels[stk]} ({damage} dmg)");

        if (stk > 1)
            hero.AddWounds(damage);
    }

    private CombatResult ResolveSocialAction(Hero hero, Monster mob, CombatAction action, CombatResult result)
    {
        switch (action)
        {
            case CombatAction.Bribe:
                int cost = mob.Guts + mob.Charm;
                int bribeCharm = hero.Charm;
                if (hero.HasTrait("Noble")) bribeCharm *= 2;
                if (hero.Marks >= cost && _rng.Contest(bribeCharm, mob.Charm))
                {
                    hero.RemovePackItem("Marks", cost);
                    result.Log.Add($"{hero.Name} bribes {mob.Name} successfully!");
                    result.MonsterFled = true;
                }
                else
                {
                    result.Log.Add($"{mob.Name} refuses the bribe!");
                    ResolveMonsterAttack(hero, mob, result);
                }
                break;

            case CombatAction.Feed:
                if (hero.Food > 0)
                {
                    if (hero.GetPackCount("Fish") > 0)
                        hero.RemovePackItem("Fish", 1);
                    else
                        hero.RemovePackItem("Food", 1);
                    if (_rng.Contest(hero.Charm, mob.Charm))
                    {
                        result.Log.Add($"{mob.Name} accepts the food and wanders off.");
                        result.MonsterFled = true;
                    }
                    else
                    {
                        result.Log.Add($"{mob.Name} gobbles the food and attacks!");
                        ResolveMonsterAttack(hero, mob, result);
                    }
                }
                break;

            case CombatAction.Riddle:
                if (_rng.Contest(hero.Wits * 2, mob.Wits))
                {
                    result.Log.Add($"{hero.Name} stumps {mob.Name} with a riddle!");
                    hero.GainWits(mob.Wits, _rng);
                    result.MonsterFled = true;
                }
                else
                {
                    result.Log.Add($"{mob.Name} answers correctly and attacks!");
                    ResolveMonsterAttack(hero, mob, result);
                }
                break;

            case CombatAction.Seduce:
                if (_rng.Contest(hero.Charm * 2, mob.Charm))
                {
                    result.Log.Add($"{hero.Name} charms {mob.Name}.");
                    hero.GainCharm(mob.Charm, _rng);
                    result.MonsterFled = true;
                }
                else
                {
                    result.Log.Add($"{mob.Name} is offended and attacks!");
                    ResolveMonsterAttack(hero, mob, result);
                }
                break;

            default:
                result.Log.Add($"{hero.Name} tries to be helpful.");
                if (_rng.Contest(hero.Charm, mob.Charm))
                {
                    result.MonsterFled = true;
                    result.Log.Add($"{mob.Name} is grateful and leaves.");
                }
                else
                {
                    ResolveMonsterAttack(hero, mob, result);
                }
                break;
        }

        return result;
    }

    private CombatResult ResolveControl(Hero hero, Monster mob, CombatResult result)
    {
        int power = 2 * hero.Wits;
        int defPower = mob.Charm;
        if (mob.HasTrait("Stubborn")) defPower += 30;

        if (hero.MagicUses > 0) hero.MagicUses--;

        if (_rng.Contest(power, defPower))
        {
            result.Controlled = true;
            result.Log.Add($"{mob.Name} was Mesmerized!");
        }
        else
        {
            result.Log.Add($"{mob.Name} Resists!");
            ResolveMonsterAttack(hero, mob, result);
        }

        return result;
    }

    private CombatResult ResolveSwindle(Hero hero, Monster mob, CombatResult result)
    {
        int power = 2 * hero.Charm;
        int defPower = mob.Charm;
        if (mob.HasTrait("Clever")) defPower += 30;

        if (hero.ThiefUses > 0) hero.ThiefUses--;

        if (_rng.Contest(power, defPower))
        {
            result.Swindled = true;
            result.Log.Add($"{mob.Name} falls for it!");
            // Transfer monster gear/loot to hero
            foreach (var item in mob.Pack)
                hero.AddPackItem(item.Name, item.Count);
        }
        else
        {
            result.Log.Add($"{mob.Name} is too Cunning!");
            ResolveMonsterAttack(hero, mob, result);
        }

        return result;
    }

    private void ApplySpellEffects(Hero hero, Monster mob, CombatResult result)
    {
        // Hero's weapon effects apply to monster
        if (hero.GetStatusCount("Blind-Apply") > 0)
        {
            int count = hero.GetStatusCount("Blind-Apply");
            hero.ClearStatus("Blind-Apply");
            if (_rng.Contest(hero.Wits * count, mob.Wits))
            {
                mob.AddStatus("Blind", 1);
                result.Log.Add("*BLIND*");
            }
        }
        if (hero.GetStatusCount("Panic-Apply") > 0)
        {
            int count = hero.GetStatusCount("Panic-Apply");
            hero.ClearStatus("Panic-Apply");
            if (_rng.Contest(hero.Wits * count, mob.Wits))
            {
                mob.AddStatus("Panic", 1);
                result.Log.Add("+PANIC+");
            }
        }
        if (hero.GetStatusCount("Disease-Apply") > 0)
        {
            int count = hero.GetStatusCount("Disease-Apply");
            hero.ClearStatus("Disease-Apply");
            int effectiveDmg = mob.HasTrait("Hardy") ? count / 2 : count;
            if (effectiveDmg > 0)
            {
                mob.AddStatus("Disease", effectiveDmg);
                result.Log.Add("^Sick^");
            }
        }
    }

    private int GetHeroSpeed(Hero hero)
    {
        int speed = hero.Skill;
        if (hero.HasTrait("Reflex")) speed += 30;
        if (hero.GetStatusCount("Blind") > 0) speed /= 2;
        return Math.Max(1, speed);
    }

    private int GetMonsterSpeed(Monster mob)
    {
        int speed = mob.Skill;
        if (mob.HasTrait("Reflex")) speed += 30;
        if (mob.GetStatusCount("Blind") > 0) speed /= 2;
        return Math.Max(1, speed);
    }

    public List<CombatAction> GetAvailableActions(Hero hero, Monster mob, bool firstRound)
    {
        var actions = new List<CombatAction>();

        if (hero.GetStatusCount("Panic") > 0)
        {
            actions.Add(CombatAction.Runaway);
            return actions;
        }

        if (firstRound)
        {
            if (mob.Options.Contains("bribe") && hero.Marks > 0) actions.Add(CombatAction.Bribe);
            if (mob.Options.Contains("feed") && hero.Food > 0) actions.Add(CombatAction.Feed);
            if (mob.Options.Contains("riddle")) actions.Add(CombatAction.Riddle);
            if (mob.Options.Contains("trade") && hero.Marks > 0) actions.Add(CombatAction.Trade);
            if (mob.Options.Contains("help")) actions.Add(CombatAction.Help);
            if (mob.Options.Contains("seduce")) actions.Add(CombatAction.Seduce);
        }

        if (hero.MagicRank > 0 && hero.MagicUses > 0 && mob.Options.Contains("control"))
            actions.Add(CombatAction.Control);
        if (hero.ThiefRank > 0 && hero.ThiefUses > 0 && mob.Options.Contains("backstab"))
            actions.Add(CombatAction.Backstab);
        if (hero.FightRank > 0 && hero.FightUses > 0)
            actions.Add(CombatAction.Berzerk);
        if (hero.ThiefRank > 0 && hero.ThiefUses > 0 && mob.Options.Contains("swindle"))
            actions.Add(CombatAction.Swindle);
        if (hero.IeatsuRank > 0 && hero.IeatsuUses > 0)
            actions.Add(CombatAction.Ieatsu);

        actions.Add(CombatAction.Attack);
        actions.Add(CombatAction.Runaway);

        return actions;
    }
}
