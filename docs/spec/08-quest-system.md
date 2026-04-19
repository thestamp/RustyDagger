# 08 — Quest System

## 1. Quest Flow

### 1.1 Encounter Generation

```
1. Hero clicks Quest portrait in a wilderness screen
2. canAdvance() checks:
   a. quests >= 1 (not exhausted)
   b. rope if needsRope
   c. light if needsLight
3. selectQuest():
   a. 1% chance → Faery encounter
   b. Otherwise → weighted random from area beast list
4. Monster created and balanced to hero level
5. Hero gains +1 fatigue
6. arQuest screen presented
```

### 1.2 Quest Screen Layout

**Class:** `arQuest`  
**Background:** `Color(255, 255, 0)`

| Element | Position | Size |
|---------|----------|------|
| Monster portrait | (10, 10) | 160×160 |
| Options panel | (180, 20) | 200×110 |
| Encounter text | below portrait | Full width |

---

## 2. Encounter Options

### 2.1 Option Availability

| # | Option | First Round | Later Rounds | Requirements |
|---|--------|-------------|--------------|-------------|
| 0 | Bribe | Yes | No | money ≥ 1 |
| 1 | Feed | Yes | No | Food ≥ 1 |
| 2 | Riddle | Yes | No | — |
| 3 | Trade | Yes | No | — |
| 4 | Help | Yes | No | — |
| 5 | Seduce | Yes | No | — |
| 6 | Control | Yes | Yes | magic skill |
| 7 | Backstab | Yes | Yes | thief skill |
| 8 | Berzerk | Yes | Yes | fight skill |
| 9 | Swindle | Yes | Yes | thief skill |
| 10 | Ieatsu | Yes | Yes | ieatsu skill |
| 11 | Attack | Yes | Yes | — |
| 12 | Flee | Yes | Yes | — |
| 13 | Fish | Yes | No | Fish ≥ 1 |
| 14 | Bushido | Yes | No | Token ≥ 1 AND quests ≥ 10 AND guildRank < level |
| 15 | Capture | Yes | No | — |
| 16 | Spells | Yes | Yes | — |

Options are filtered by the monster's `opts` list — only options listed in `opts` appear.

### 2.2 Round Progression

- **Round 1:** All available options shown (social + combat)
- **Round 2+:** Only combat options shown (Attack, Flee, guild skills, Spells)
- Failed social interactions escalate to combat rounds

---

## 3. Social Option Resolution

### 3.1 Bribe

```
cost = weight × (mob.guts + mob.wits) / 2
if hero.money >= cost:
    hero.money -= cost
    → victory (flee variant — monster leaves)
else:
    → "not enough money" message
```

No stat check. Always succeeds if funds are sufficient.

### 3.2 Feed

```
foodNeeded = (mob.guts + 4) / 5
if hero.pack has enough Food:
    remove foodNeeded Food
    if mob.passion != "aggressive" AND contest(hero.charm, mob.charm):
        → victory (peaceful)
        hero gains gainCharm(weight)
    else:
        → fail, escalate to combat
```

### 3.3 Riddle

```
if contest(hero.wits, mob.wits):
    → present random riddle from riddle table
    if correct answer:
        → victory: half mob pack + exp + gainWits(weight) + FAME
    else:
        → fail, escalate to combat
else:
    → fail (mob refuses), escalate to combat
```

**Riddle Format:** Multiple-choice from `QuestStrings.RIDDLE` table (13 riddles, each with question + correct answer + wrong answers).

### 3.4 Trade

```
cost = weight × (mob.charm + mob.wits)
tradeCharm = hero.charm + (MERCHANT trait bonus)
if hero.money >= cost AND mob.pack not empty:
    if contest(tradeCharm, mob.tradeCharm):
        hero.money -= cost
        → victory: all mob pack items + exp + gainCharm(weight)
    else:
        → fail, escalate to combat
```

### 3.5 Help / Assist

```
if contest(hero.wits, mob.wits):
    → victory: half mob pack + exp + gainWits(weight) + FAME
else:
    → fail, escalate to combat
```

FAME is added equal to `weight` (affects social standing).

### 3.6 Seduce

```
seduceCharm = hero.charm + (BEAUTY trait bonus)
if contest(seduceCharm, mob.seduceCharm):
    → victory: half mob pack + exp + gainCharm(weight × 2)
    → display random seduction outcome from QuestStrings
else:
    → fail, escalate to combat
```

**8 Seduction Outcomes** (random flavor text from `QuestStrings.SEDUCE`).

### 3.7 Fish

```
if hero has Fish in pack:
    remove 1 Fish
    if contest(hero.charm, mob.charm):
        → victory (peaceful)
    else:
        → fail
```

---

## 4. Combat Options

### 4.1 Attack (Normal)

Standard combat as documented in [04-combat-system.md](04-combat-system.md).

On victory: `gainGuts(weight)`

### 4.2 Backstab

```
Requirement: thief rank ≥ 1
Effect: hero.guts and hero.speed doubled; mob limited to 1 hit
On victory: gainCharm(weight×3), gainGuts(weight×2)
```

### 4.3 Berzerk

```
Requirement: fight rank ≥ 1
Effect: hero.guts and hero.speed doubled; hero gets 4 hits
On victory: gainGuts(weight×5)
```

### 4.4 Ieatsu

```
Requirement: ieatsu rank ≥ 1
Effect: hero.guts and hero.speed doubled; hero gets 4 hits
On victory: gainGuts(weight×5)
```

Mechanically identical to Berzerk but from a different skill tree.

### 4.5 Control / Hypnotize

```
Requirement: magic rank ≥ 1
Effect: uses hero.wits instead of hero.guts for speed calculation
On victory: all mob treasure + exp + gainWits(weight)
```

### 4.6 Swindle

```
Requirement: thief rank ≥ 1
Effect: uses hero.charm instead of hero.guts
On victory: all mob gear + exp + gainCharm(weight)
```

### 4.7 Flee

```
speed contest: contest(hero.speed, mob.speed)
Success: hero escapes, no loot, no exp
Failure: mob gets one free attack
```

### 4.8 Bushido Training

```
Requirements:
    - Has "Bushido Token" in pack
    - quests >= 10
    - guildRank < level
    - Samurai encounter

Effect:
    - Consume Bushido Token
    - +1 ieatsu rank
    - +5 fatigue
    - gainGuts(weight)
```

### 4.9 Capture (Faery)

```
if contest(hero.wits, mob.wits):
    → add "Bottled Faery" to hero pack
else:
    → faery escapes
```

---

## 5. Victory Processing

```
onVictory(mob, weight):
    // Experience
    baseExp = mob.baseExp()
    bonusExp = (2 × mob.guts + mob.wits + mob.charm) × weight / 4
    totalExp = baseExp + bonusExp
    hero.exp += totalExp

    // Loot transfer
    for each item in mob.pack:
        if hero.pack not full:
            transfer item to hero.pack

    // Level check
    if hero.exp >= hero.raise:
        → level up sequence
```

### 5.1 Level Up

```
raise threshold = 50 × 1.5^(level - 1)

On level up:
    level += 1
    exp -= raise (carry over excess)
    raise = next threshold
    hero chooses stat allocation (guts/wits/charm)
```

---

## 6. Defeat Processing

```
onDefeat():
    // Hero is wounded (guts reduced)
    // Pack items may be looted by monster
    // Hero wakes at last save location
    // Fame penalty: fame -= fame / 5
```

---

## 7. Quest Generation by Area

### 7.1 Fields Quests

**Power:** 1  
**Weight:** 1  

Two weight tables based on hero level:

**Level < 3:**

| Monster | Weight |
|---------|--------|
| Rodent | 12 |
| Goblin | 10 |
| Merchant | 10 |
| Centaur | 6 |
| Wizard | 2 |
| Gypsy | 1 |
| Soldier | 0 |

**Level ≥ 3:**

| Monster | Weight |
|---------|--------|
| Rodent | 8 |
| Goblin | 6 |
| Merchant | 5 |
| Gypsy | 5 |
| Centaur | 4 |
| Wizard | 2 |
| Soldier | 2 |

### 7.2 Forest Quests

**Power:** 2, **Weight:** 2

| Monster | Weight |
|---------|--------|
| Boar | 10 |
| Orc | 9 |
| Elf | 8 |
| Gryphon | 6 |
| Snot | 4 |
| Unicorn | 3 |

### 7.3 Hills Quests

**Power:** 3, **Weight:** 3

| Monster | Weight |
|---------|--------|
| Goat | 7 |
| Basilisk | 5 |
| Troll | 5 |
| Wyvern | 4 |
| Giant | 3 |
| Sphinx | 3 |

Dragon is NOT in the random table — accessed via Mines cavern only.

### 7.4 Mound Quests (3 Depth Levels)

**Power:** 3, **Weight:** 3

**Warrens:** Gate(10), Gang(8), Rager(6), Thief(4), Worm(3)  
**Treasury:** Gang(5), Rager(6), Thief(5), Mage(6), Guard(3)  
**Throne Room:** Guard(4), Mage(4), Vault(3), Champion(3), Queen(2)

### 7.5 Castle Sub-Area Quests

**Dungeon** (Power=2): Rodent(8), Snot(6), Rager(5), Gang(4), Troll(3), Mage(2)  
**Ocean** (Power=3): Traders(5), Traders(4), Serpent(3), Mermaid(2)  
**Brasil** (Power=4): Harpy(5), Fighter(4), Golem(3), Medusa(3), Hero(2)  
**Shangala** (Power=5): Gunner(6), Peasant(5), Ninja(4), Plague(3), Shogun(2), Panda(1), Samurai(1)

---

## 8. Special Monster Abilities

### 8.1 Goat Skill

```
goatSkill():
    if hero has "Rope" in pack:
        steal Rope from hero
        // Goat eats the rope
```

### 8.2 Worm Skill

```
wormSkill():
    for each equipped gear:
        if gear has GLOWS or FLAME trait:
            steal gear from hero
            // Worm is attracted to light sources
```

### 8.3 Monster Stance Escalation

During combat, monsters escalate behavior based on wounds:

```
if monster.wounds > monster.guts / 2:
    stance increases by 1 (more aggressive)
if monster.wounds > monster.guts × 3 / 4:
    stance increases to maximum (desperate)
```
