# 04 — Combat System

## 1. Overview

Combat in Dragon Court is resolved in a single exchange per round. Each round, both combatants choose actions, initiative determines who strikes first, and damage/effects are applied. Combat continues until one party is killed, flees, or a special outcome occurs (Control, Swindle).

---

## 2. Pre-Combat Setup

### Monster Preparation

When a quest encounter begins, the monster is balanced to the encounter weight:

```
monster.balance(weight):
    1. If hero has DRAGON trait: add "trade" to monster opts
    
    2. calcPrimary(weight):
        - Apply identity substitution from text template
        - Scale stats: multiply by (0.9 + 0.1 × hero_level)
        - Spread values (randomize variance)
        - If "adjust" value set: scale Guts/Wits/Charm by ratio
    
    3. calcCombat():
        - attack = gear.fullAttack()
        - defend = gear.fullDefend()
        - skill  = gear.fullSkill()
    
    4. calcSecondary(weight):
        - Actions = temp.Actions (or 1 if missing)
        - Fame = (Guts + Wits + Charm) / 30 + (skills + weight) / 4
        - Exp = ((1 + Attack) × (100 + Skill)) / 100
        - Set stance from passion value:
            "aggressive" → 4
            "hostile"    → 3
            "defensive"  → 2
            "timid"      → 1
            "passive"    → 0
    
    5. buildPack():
        - For each itPercent: if roll(100) < value, add item
        - For each itRandom: add roll(maxCount) copies
        - For each itCount: keep as-is
    
    6. buildGear(slot):
        - For each gear itPercent: materialize equipment
```

### Action Queue Reset
```
hero.actions.clear()     // Clear blind/panic/disease/blast counts
monster.actions.clear()
```

---

## 3. Action Types

### 3.1 Action Definitions

| ID | Name | Type | Requirement | Consumes |
|----|------|------|-------------|----------|
| 0 | BRIBE | Social | Money ≥ cost | Marks |
| 1 | FEED | Social | Food/Fish in pack | Food/Fish items |
| 2 | RIDDLE | Social | None | None |
| 3 | TRADE | Social | Money ≥ cost | Marks |
| 4 | HELP | Social | None | None |
| 5 | SEDUCE | Social | None | None |
| 6 | CONTROL | Magic | magic rank ≥ 1 | 1 magic use |
| 7 | BACKSTAB | Thief | thief rank ≥ 1 | 1 thief use |
| 8 | BERZERK | Fighter | fight rank ≥ 1 | 1 fight use |
| 9 | SWINDLE | Thief | thief rank ≥ 1 | 1 thief use |
| 10 | IEATSU | Samurai | ieatsu rank ≥ 1 | 1 ieatsu use |
| 11 | ATTACK | Combat | Always available | None |
| 12 | RUNAWAY | Combat | Always available | None |
| 13 | CARP | Social | None | None |
| 14 | BUSHIDO | Special | Bushido Token in pack | Token |
| 15 | CAPTURE | Special | Faery encounter | None |
| 16 | SPELLS | Magic | Magic items in pack | Items |

### 3.2 Option Availability Rules

```
fixList():
    if hero has PANIC:
        only RUNAWAY available
        return
    
    if firstRound:
        // Social options available based on monster opts
        if "bribe" in monster.opts AND hero.money > 0:   add BRIBE
        if "feed" in monster.opts AND hero has food:       add FEED
        if "riddle" in monster.opts:                        add RIDDLE
        if "trade" in monster.opts AND hero.money > 0:     add TRADE
        if "help" in monster.opts:                          add HELP
        if "seduce" in monster.opts:                        add SEDUCE
    
    // Combat options (always available after first round)
    if hero.magicRank() > 0 AND "control" in monster.opts:  add CONTROL
    if hero.thiefRank() > 0 AND "backstab" in monster.opts: add BACKSTAB
    if hero.fightRank() > 0:                                 add BERZERK
    if hero.thiefRank() > 0 AND "swindle" in monster.opts:  add SWINDLE
    if hero.ieatsuRank() > 0:                                add IEATSU
    
    add ATTACK    // Always
    add RUNAWAY   // Always
    
    // Special
    if hero.packMagic() > 0:                    add SPELLS
    if "bushido" in monster.opts AND has token:  add BUSHIDO
    if "capture" in monster.opts:                add CAPTURE
```

### 3.3 Round Progression

```
nextRound():
    firstRound = false
    if monster.stance == "hostile" or "defensive":
        monster.stance += 1    // Escalates over time
```

---

## 4. Initiative System

### Speed Calculation

```
Hero:
    hspeed = hero.skill()                    // gear skill - disease
    if has REFLEX trait: hspeed += 30
    if BLIND status:    hspeed /= 2

Monster:
    mspeed = mob.skill()
    if has REFLEX trait: mspeed += 30
    if BLIND status:    mspeed /= 2
```

### Action-Specific Speed Modifiers

| Action | Attacker Speed | Attacker Guts | Hit Value |
|--------|----------------|---------------|-----------|
| BACKSTAB | ×2 | ×2 | = 1 (weak hit) |
| BERZERK | ×2 | ×2 | = 4 (power hit) |
| IEATSU | ×2 | ×2 | = 4 (power hit) |
| CONTROL | = 2 × Wits | normal | N/A |
| SWINDLE | = 2 × Charm | normal | N/A |
| ATTACK | normal | normal | twice(3) [0-6] |
| RUNAWAY | normal | normal | N/A |

### Initiative Determination

```
if hero.action == RUNAWAY AND mob.action != RUNAWAY:
    heroFirst = true
else if mob.action == RUNAWAY AND hero.action != RUNAWAY:
    heroFirst = false
else:
    heroFirst = contest(hspeed, mspeed)
    // contest(a, b) = roll(a + b) < a
```

---

## 5. Attack Resolution

### Hit Determination

```
hhit = twice(3)    // 0-6, bell curve centered on 3

Modifiers:
    if BACKSTAB: hit = 1 (opponent's perspective — weak but doubled stats)
    if BERZERK or IEATSU: hit = 4 (guaranteed power hit)
    if BLIND: hit /= 2
```

### Hit Power Labels

| Hit Value | Label |
|-----------|-------|
| 0 | "Fly Swat" |
| 1 | "Weak Blow" |
| 2 | "Good Hit" |
| 3 | "Potent Hit" |
| 4 | "POWER HIT!" |

### Accuracy Check

```
agentAct(attacker, defender, guts, hit, asSpeed, dsSpeed):
    // Defense modifiers
    if attacker.action == BACKSTAB AND defender has ALERT:
        dsSpeed += 30
    if attacker.action == BERZERK AND defender has FENCER:
        dsSpeed += 30
    if attacker.action == IEATSU AND defender has FENCER:
        dsSpeed += 30
    
    // Miss check
    if roll(dsSpeed) > asSpeed:
        damage = 0
        stk = 0    // "DODGED!"
    else:
        // Hit — calculate damage
```

### Damage Formula

```
damage = max(0, (guts × (2 + hit)) / 10 + attacker.attack - defender.defend)

// Blast override check
blastCount = attacker.actions.count("Blast")
blastDamage = 25 × blastCount
if blastDamage > damage:
    damage = blastDamage
    useBlast = true
```

### Wound Level (Stun/Kill)

```
maxHealth = defender.guts - defender.wounds

if damage < 1:
    stk = 1    // "Unharmed" (hit but no damage)
else if damage >= maxHealth:
    stk = 5    // "KILLED!!!"
    defender.state = DEAD
    killStop = true
else:
    stk = 2 + (3 × damage / maxHealth)    // 2-4 scale
    // 2 = "Scratched", 3 = "Injured!", 4 = "Wounded!!"

if stk > 1:
    defender.addWounds(damage)
```

### Effect Labels

| stk | Label |
|-----|-------|
| 0 | "DODGED!" |
| 1 | "Unharmed" |
| 2 | "Scratched" |
| 3 | "Injured!" |
| 4 | "Wounded!!" |
| 5 | "KILLED!!!" |

---

## 6. Weapon Secondary Effects

After a successful hit (stk ≥ 2), weapon traits apply:

```
if attacker weapon has BLIND trait:
    add Blind action (count=1) to attacker's actions

if attacker weapon has PANIC trait:
    add Panic action (count=1) to attacker's actions

if attacker weapon has DISEASE trait AND useBlast == false:
    add Disease action (count = (damage + 3) / 5) to attacker's actions
```

---

## 7. Spell Effects Phase

After both sides' attacks resolve, spell/dust effects are applied:

```
spellEffects(attacker, defender):
    for each action in attacker.actions:
        
        if action == "Blind":
            count = action.getCount()
            if contest(attacker.wits × count, defender.wits):
                defender.fixTrait("Blind")
                output += " *BLIND*"
        
        if action == "Panic":
            count = action.getCount()
            if contest(attacker.wits × count, defender.wits):
                defender.fixTrait("Panic")
                if defender is Monster: defender.setPassive()
                output += " +PANIC+"
        
        if action == "Disease":
            count = action.getCount()
            if count > 0:
                if defender has HARDY: effectiveDmg = count / 2
                else: effectiveDmg = count
                defender.ail(effectiveDmg)
                output += " ^Sick^"
        
        if action == "Blast":
            output += " >KABOOM<"
```

---

## 8. Control (Magic)

### Hero Controls Monster

```
actorControls(attacker, defender, power):
    // power = 2 × attacker.wits
    defPower = defender.charm
    if defender has STUBBORN: defPower += 30
    
    if contest(power, defPower):
        defender.state = CONTROL
        killStop = true
        return "was Mesmerized!"
    else:
        return "Resists!"
    
    attacker.magic(1)    // Consume 1 magic use
```

### Monster Controls Hero

```
if monster.stance == aggressive:
    hero is killed
elif monster.stance == passive:
    message only, no effect
elif monster.stance >= hostile:
    hero loses half pack + control action applied
```

### Control Action Messages (Random)

```
"leave it thinking it is a chicken. Cluck, cluck."
"force its head to explode just like 'Scanners'"
"send it on a road trip to Moscow for Vodka."
"tattoo \"I'm with stupid\" on its chest."
"take it to dinner and stick it with the bill."
"direct it to leap of the nearest cliff."
"order it to go chase Fenton Magus."
"send it out collecting flowers for its mother."
```

---

## 9. Swindle (Thief)

### Hero Swindles Monster

```
actorSwindles(attacker, defender, power):
    // power = 2 × attacker.charm
    defPower = defender.charm
    if defender has CLEVER: defPower += 30
    
    if contest(power, defPower):
        attacker.state = SWINDLE
        killStop = true
        return "falls for It!"
    else:
        return "too Cunning!"
    
    attacker.thief(1)    // Consume 1 thief use
```

### Swindle Outcomes

**Hero swindles monster:**
1. Check monster for Thief Insurance
2. If has insurance: hero receives the insurance item
3. Else: hero gets all monster gear + exp + charm

**Monster swindles hero:**
1. Check hero for Thief Insurance
2. Per stance:
   - Aggressive: empties hero pack completely
   - Passive: trades a Rock for all hero's money
   - Hostile: takes half pack + half money

---

## 10. Monster AI

### Action Selection (`chooseActions()`)

```
chooseActions():
    // Emergency recovery
    if (hasBlind OR hasPanic) AND hasSeltzer:
        use Seltzer, clear Blind/Panic status
        return
    
    // Healing check
    if heavily wounded AND hasHealing:
        use healing item
        return
    
    // Main decision
    choice = decide(magic_items, guild_skills)
    
    // Force magic if:
    //   blindDust + panicDust > actions × skill_ratio
    if forced_magic:
        use magic items (dust)
    else:
        // Use guild skills
        select from: fight, control, ieatsu, backstab, swindle, berzerk
        // Based on available ranks and random weighting
```

### Monster Healing AI

```
combatEvents(mob):
    if mob has Gold Apple:
        mob.doRevive()        // Full heal
        consume Apple
    
    if mob has Salve:
        mob.doHeal()          // Heal 15 wounds
        consume Salve
    
    if mob has Troll Wart:
        mob.doRevive()        // Full heal
        consume Wart
    
    if mob has Ginseng:
        actions += 2
        consume Ginseng
```

### Special Monster Skills

**Goat Skill:**
```
goatSkill():
    if contest(2 × mob.guts, hero.guts):
        if hero has Rope:
            steal Rope from hero
            display message
```

**Worm Skill:**
```
wormSkill():
    if contest(2 × mob.guts, hero.guts):
        find hero equipment with GLOWS or FLAME trait
        if found: steal that equipment
        display message
```

### Stance Escalation

| Stance | Value | Behavior |
|--------|-------|----------|
| Passive | 0 | Won't attack, may flee |
| Timid | 1 | Reluctant fighter, avoids |
| Defensive | 2 | Defends, escalates slowly |
| Hostile | 3 | Active attacker |
| Aggressive | 4 | Maximum aggression, kills on control |

Stance increases by 1 each round if originally `hostile` or `defensive`.

---

## 11. Backstab Details

```
Backstab:
    - Consumes 1 thief use
    - Attacker guts × 2
    - Attacker speed × 2
    - Hit value = 1 (weak, but doubled guts compensate)
    - Defender with ALERT trait: +30 speed defense
    - Always goes first (speed doubled)
```

---

## 12. Berzerk Details

```
Berzerk:
    - Consumes 1 fight use
    - Attacker guts × 2
    - Attacker speed × 2
    - Hit value = 4 (POWER HIT)
    - Defender with FENCER trait: +30 speed defense
    - Devastating damage but can be dodged
```

---

## 13. Ieatsu Details

```
Ieatsu:
    - Consumes 1 ieatsu use
    - Attacker guts × 2
    - Attacker speed × 2
    - Hit value = 4 (POWER HIT)
    - Defender with FENCER trait: +30 speed defense
    - Identical to Berzerk mechanically, different skill consumed
```

---

## 14. Spells (Magic Items)

When the hero selects SPELLS, their magic item inventory is used:

Items checked from hero pack:
- **Blind Dust:** Adds Blind action
- **Panic Dust:** Adds Panic action
- **Blast Dust:** Adds Blast action (25 damage per unit)
- **Mandrake:** Healing effect
- **Ginseng:** +2 actions

These are consumed on use and resolved in the spell effects phase.

---

## 15. Combat Output Format

```
AttackerName: HitPowerLabel
    ---DefenderName EffectLabel [*BLIND*] [+PANIC+] [^Sick^] [>KABOOM<]
```

Example:
```
Giant Rat: Fly Swat
    ---Brave Hero DODGED!
Brave Hero: POWER HIT!
    ---Giant Rat Wounded!! ^Sick^
```

---

## 16. Victory Rewards

```
heroWins():
    // Experience
    exp_gained = mob.baseExp() + (2 × guts + wits + charm) × weight / 4
    hero.learn(exp_gained)
    
    // Loot
    transfer all mob pack items to hero (respecting overload)
    
    // Stat gains (by action type used)
    hero.gainGuts(weight)     // Roll-based growth
    hero.gainWits(weight)
    hero.gainCharm(weight)
    
    // Fame
    hero.addFame(mob.fame)
    
    // Level check
    hero.tryToLevel(screen)
```

### Monster Base Exp Formula

```
baseExp = ((1 + attack) × (100 + skill)) / 100
```

### Monster Fame Formula

```
fame = (guts + wits + charm) / 30 + (skills + weight) / 4
```

---

## 17. Flee Resolution

```
tryFlee():
    if mob.stance == passive or defensive:
        hero escapes free
    elif mob.stance == hostile:
        if contest(hero.runWits(), mob.runWits()):
            hero escapes
        else:
            battle occurs
    elif mob.stance == aggressive:
        battle occurs (can't flee)
```

Where `runWits()` = `wits + (wits if RANGER)`.

---

## 18. Death in Combat

When either side reaches `stk == 5` (KILLED):
- `killStop = true`: Combat ends immediately (other side doesn't get to act)
- Dead entity's state set to `DEAD`
- If hero dies: `killedScreen()` sequence (see Character System spec §10)
- If monster dies: `heroWins()` sequence
