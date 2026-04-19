# 03 — Character System

## 1. Character Creation Flow

### Step 1: Name & Password (arEntry)
- Hero name: 4-15 characters
- Password: any length (used for server auth)
- Leading/trailing whitespace replaced with underscores

### Step 2: Stat Allocation (arCreate)

**Starting Budget:** 20 build points

**Base Stats (minimum values):**
| Stat | Base | Min | Cost per point |
|------|------|-----|----------------|
| Guts | 4 | 4 | 1 |
| Wits | 4 | 4 | 1 |
| Charm | 4 | 4 | 1 |
| Money | 1 | 1 | 1 (×25 Marks) |

**Starting Money:** `money_points × 25` Marks

**Trait Selection (costs from budget):**

| Trait | Cost | Effect |
|-------|------|--------|
| Noble | 12 | +1 Social rank, doubled Bribe charm |
| Wizard | 9 | Starts with `magic=1` guild skill |
| Warrior | 8 | Starts with `fight=1` guild skill |
| Trader | 10 | Starts with `thief=1` guild skill, +20 pack capacity, +1/8 thief on advance |

**Build must equal exactly 0 to proceed** (all 20 points spent).

**Initial Hero State:**
```
Level:      1
Social:     1 if Noble, else 0
Age:        16
Place:      FIELDS
State:      CREATE
Marks:      money × 25
Actions:    calculated (27 + 3×level = 30 base)
Guild:      fight/magic/thief = 1 if corresponding trait selected
```

### Step 3: Appearance (arBuild)

Only shown if `hero.level > 5 AND hero.looks.count < 1` (i.e., first time or reset).

**Appearance Fields:**

| Field | Max Length | Random Source |
|-------|-----------|---------------|
| Race | 15 | `GameStrings.races[10]` |
| Build | 15 | `GameStrings.builds[9]` |
| Sign | 15 | `GameStrings.signs[12]` |
| Skin | 15 | `GameStrings.colors[12]` |
| Eyes | 15 | `GameStrings.colors[12]` |
| Hair | 15 | `GameStrings.colors[12]` |
| Habit | 40 | `GameStrings.habits[7]` |
| Marks | 60 | `GameStrings.features[11]` |
| Catchphrase | 60 | `GameStrings.phrases[5]` |

**Checkboxes (4 groups):**
- Gender: Male / Female
- Dress: Civilian / Warrior
- Behavior: Introvert / Extrovert
- Title: Assigned from rank system

**Stored in `hero.looks` itList as itValue pairs:**
```
{~|looks|
  {=|Title|Sir}|
  {=|Gender|male}|
  {=|Dress|Warrior}|
  {=|Behave|Extrovert}|
  {=|Race|Human}|
  {=|Build|Average}|
  {=|Sign|Aries}|
  {=|Skin|Fair}|
  {=|Eyes|Brown}|
  {=|Hair|Black}|
  {=|Habit|bites nails when nervous}|
  {=|Marks|small scar above left eye}|
  {=|Phrase|Have at thee!}
}
```

---

## 2. Primary Attributes

### 2.1 Guts
- **Role:** Hit points, physical power, melee damage
- **Combat:** Damage = `(guts × (2 + hit)) / 10 + attack - defend`
- **max HP equivalent:** Unwounded guts = max health
- **Growth:** +2 per level, random gain from combat weight checks

### 2.2 Wits
- **Role:** Intelligence, speed component, contest resolution
- **Combat:** Initiative speed = `skill()` (gear + enchant - disease)
- **Social:** Riddle contests, Control (magic) uses `2 × wits`
- **Navigation:** Forest/Hills discovery, avoiding ambush
- **Growth:** +2 per level, random gain from riddle/study

### 2.3 Charm
- **Role:** Social interactions, trading leverage
- **Combat:** Swindle uses `2 × charm`
- **Social:** Bribe, Feed, Seduce, Trade outcomes
- **Economy:** Shop prices affected by charm vs base
- **Growth:** +2 per level, random gain from social encounters

### 2.4 Stat Growth Formula
```
gainGuts(weight):
    if roll(guts) >= weight:
        guts += 1

gainWits(weight):
    if roll(wits) >= weight:
        wits += 1

gainCharm(weight):
    if roll(charm) >= weight:
        charm += 1
```

Higher weight = harder to gain (need higher existing stat for the roll to succeed).

---

## 3. Combat Stats (Derived)

### 3.1 Attack
```
attack = sum of all equipped gear fullAttack()

where per-item:
  fullAttack = base_attack + ((enchant + 9) / 10) + (8 if RIGHT_HAND and FLAME)
```

### 3.2 Defend
```
defend = sum of all equipped gear fullDefend()

where per-item:
  fullDefend = base_defend + ((enchant + 4) / 10) + (1 if BLESS)
```

### 3.3 Skill (Speed)
```
skill = sum of all equipped gear fullSkill() - disease_count

where per-item:
  fullSkill = base_skill + enchant + (12 if RIGHT_HAND and LUCKY) + (2 if GLOWS)
```

---

## 4. Character Traits (33 Total)

Traits are stored as named items in the `stat` or `temp` lists.

### Background Traits (Selected at Creation)

| Trait | Effect |
|-------|--------|
| `Noble` | +1 Social start; ×2 bribe charm |
| `Wizard` | Start with magic=1 |
| `Warrior` | Start with fight=1; resist disease/injury/exhaust on camp |
| `Trader` | Start with thief=1; +20 pack capacity; +1/8 thief per advance |

### Guild Traits (Earned via Training)

| Trait | Cost to Train | Per-Session Uses | Effect |
|-------|--------------|------------------|--------|
| `Fighter` | Scales with rank | `fight` count | Enables Berzerk action |
| `Mage` | Scales with rank | `magic` count | Enables Control action |
| `Thief` | Scales with rank | `thief` count | Enables Backstab/Swindle |
| `Ieatsu` | Bushido Token trade | `ieatsu` count | Enables Ieatsu action |

### Combat Traits

| Trait | Source | Effect |
|-------|--------|--------|
| `Berzerk` | Fighter perk | Double guts+speed on Berzerk action; +1/8 fight per advance |
| `Mystic` | Mage perk | +1/8 magic per advance |
| `Fencer` | Combat perk | +30 speed defense vs Berzerk/Ieatsu |
| `Alert` | Combat perk | +30 speed defense vs Backstab |
| `Reflex` | Combat perk | +30 initiative speed |

### Exploration Traits

| Trait | Source | Effect |
|-------|--------|--------|
| `Ranger` | Achievement | ×2 wits for search fatigue |
| `Gypsy` | Achievement | ×2 wits for travel fatigue |
| `Hillfolk` | Achievement | Counts as rope for hill climbing |
| `Catseyes` | Achievement | Counts as light source in dark areas |
| `Quick` | Achievement | +4 base quests per day (instead of +3) |

### Social Traits

| Trait | Source | Effect |
|-------|--------|--------|
| `Popular` | Achievement | Faster petition progress (700 divisor vs 1000) |
| `Dragon` | Hills quest | Enables dragon trade option |
| `Merchant` | Achievement | +20 pack capacity (stacks with Trader for +40 total) |

### Status Traits (Applied/Removed During Play)

| Trait | Source | Effect |
|-------|--------|--------|
| `Blind` | Monster weapon/dust | Speed halved, hit halved |
| `Panic` | Monster weapon/dust | Can only Runaway |
| `Disease` | Monster weapon/dust | Reduces effective skill |
| `Hardy` | Achievement | Halves disease damage received |
| `Stubborn` | Achievement | +30 Wits defense vs Control |
| `Clever` | Achievement | +30 Charm defense vs Swindle |
| `Unaging` | Achievement | Age stays at 33 |

---

## 5. Guild System

### Training (at arGuild in Forest)

Three guilds available: Fighter, Mage, Thief.

**Training Cost:** Scales with current guild rank (details in Economy spec).

**Training Process:**
1. Pay cost
2. Relevant guild count increased by 1
3. Uses 5 quests

### Guild Rank Usage Per Session

Guild ranks reset each session via `advance(powers)`:

```
advance(powers):
    fight_rank = temp.fight (from previous session save)
    magic_rank = temp.magic
    thief_rank = temp.thief
    ieatsu_rank = temp.ieatsu

    // Trait bonuses
    if has BERZERK: fight_rank += fight_rank / 8
    if has MYSTIC:  magic_rank += magic_rank / 8
    if has TRADER:  thief_rank += thief_rank / 8
    
    // Set as session resources
    temp.fix("fight", fight_rank)
    temp.fix("magic", magic_rank)
    temp.fix("thief", thief_rank)
    temp.fix("ieatsu", ieatsu_rank)
```

### Ieatsu (Samurai) Training

Special guild — not trained at Guild Hall. Requires:
1. Find a Samurai-type monster
2. Have a Bushido Token in pack
3. Use the `tryToken()` encounter option
4. Win a power contest: `hero.guildRank() vs mob.guildRank()`

Success grants:
- +3 to each stat type
- Large exp gain
- Samurai training text displayed

---

## 6. Social Rank System

### Rank Titles (0-10)

| Rank | Male Title | Female Title |
|------|-----------|--------------|
| 0 | Peasant | Peasant |
| 1 | Sir Knight | Dame |
| 2 | Baron | Baroness |
| 3 | Viscount | Viscountess |
| 4 | Count | Countess |
| 5 | Marquis | Marquise |
| 6 | Duke | Duchess |
| 7 | Archduke | Archduchess |
| 8 | Prince | Princess |
| 9 | King | Queen |
| 10 | Emperor | Empress |

### Promotion Costs (Marks)

| Rank → | Cost |
|--------|------|
| 0 → 1 | 5 |
| 1 → 2 | 20 |
| 2 → 3 | 80 |
| 3 → 4 | 320 |
| 4 → 5 | 1,250 |
| 5 → 6 | 5,000 |
| 6 → 7 | 20,000 |
| 7 → 8 | 80,000 |
| 8 → 9 | 320,000 |
| 9 → 10 | 0 (special) |
| 10 → 11 | 0 (cap) |

### Petition System

Petitions to the Queen advance social rank. See Quest System spec for full details.

**Formula:** `index = (favor / divisor × 4) / rankCost[rank]` capped at 4
- Divisor: 700 if POPULAR, 900 if moderate, 1000 default

---

## 7. Leveling System

### Experience Requirements

```
getRaise():
    return 50 × 1.5^(level - 1)
```

| Level | XP Required | Cumulative |
|-------|-------------|------------|
| 1 | 50 | 50 |
| 2 | 75 | 125 |
| 3 | 112 | 237 |
| 4 | 169 | 406 |
| 5 | 253 | 659 |
| 6 | 379 | 1,038 |
| 7 | 569 | 1,607 |
| 8 | 854 | 2,461 |
| 9 | 1,281 | 3,742 |
| 10 | 1,921 | 5,663 |

### Level-Up Rewards

```
tryToLevel(screen):
    while exp >= raise:
        exp -= raise
        level += 1
        guts += 2
        wits += 2
        charm += 2
        quests += 3    // Added to base quests for the day
        recalculate raise for next level
        update fame
```

---

## 8. Fatigue & Quests System

### Base Quests Per Day

```
getBaseQuests():
    base = 27 + 3 × level
    if has QUICK trait: base += 4 per level (i.e., use 4 instead of 3)
    return base
```

### Available Quests

```
getQuests():
    return baseQuests - fatigue - overload

where:
    overload = max(0, pack.size() - packMax())
    packMax = 60 + (20 if TRADER) + (20 if MERCHANT)
```

### Fatigue Sources

| Action | Fatigue Cost |
|--------|-------------|
| Each quest/encounter | 1 |
| Camp at location | `placeCamp[location]` (0-7) |
| Petition to Queen | 3 |
| Investment | 5 |
| Guild training | 5 |
| Death penalty | baseQuests / 4 |
| Full exhaustion | Set to baseQuests |

### Camp Fatigue by Location

| Location | Camp Cost |
|----------|-----------|
| Suite | 0 |
| Room | 0 |
| Floor | 1 |
| Cot | 3 |
| Town | 0 |
| Fields | 3 |
| Forest | 7 |
| Hills | 3 |
| Mound | 1 |
| Docks | 0 |
| Dungeon | 0 |

### Fatigue Recovery (Decay)

On session start, fatigue decays based on lodging:

| Location | Decay Rate | Gear Bonus |
|----------|------------|------------|
| Suite | 5 | +1 per camp gear |
| Room | 4 | +1 per camp gear |
| Floor | 3 | +1 per camp gear |
| Town | 3 | N/A |
| Fields | 3 | +1 per camp gear |
| Forest | 2 | +1 per camp gear |
| Hills | 2 | +1 per camp gear |
| Mound | 2 | +1 per camp gear |
| Cot | 3 | +1 per camp gear |

Camp gear items: `Sleeping Bag`, `Cooking Gear`, `Camp Tent` — each adds +1 to decay rate.

---

## 9. Search & Travel Fatigue

```
searchWork(val):
    if has RANGER: fatigue += roll(val)
    else: fatigue += val

travelWork(val):
    if has GYPSY: fatigue += roll(val)
    else: fatigue += val
```

---

## 10. Death & Revival

### Death Sequence (`killedScreen()`)

```
1. Check pack for "Bottled Faery":
   IF found:
       - Remove Bottled Faery from pack
       - Transport hero to healer
       - Display revival message
       - Hero survives (not dead)
   
   ELSE:
       - Set state = DEAD
       - Fame reduced by 10%: fame -= fame / 10
       - Fatigue cost = baseQuests / 4
       - If pack should be lost (losePack flag):
           pack.loseHalf()    // Random ~50% item loss
       - If quests < 1:
           Force exit (session over)
       - Else:
           Show death notice, continue play
```

### Full Exhaustion

```
doExhaust():
    fatigue = baseQuests    // No more quests possible this session
```

---

## 11. Daily Advancement

Called via `hero.advance(powers)` at session start:

```
advance(powers):
    1. Set guild ranks from temp (fight/magic/thief/ieatsu)
    2. Apply trait bonuses:
       - BERZERK: fight += fight / 8
       - MYSTIC:  magic += magic / 8
       - TRADER:  thief += thief / 8
    
    3. Age management:
       - If UNAGING: age = 33
       - Else: age += 1
    
    4. Fame decay:
       fame = fame - (fame / 10) + (social × 10)
    
    5. Stipend:
       stipend = social² × 50
       Add to stat as STIPEND count
    
    6. Reset Actions:
       Actions = getBaseQuests()
```

---

## 12. Grant System (Clan Allegiance)

```
doGrant(note):
    if no existing clan:
        guts += 1
        wits += 1
        charm += 1
        Set clan from note
    
    else (switching clans):
        Blind all equipment (destroy stats)
        Add wounds
        Add experience (consolation)
```

---

## 13. Hero Rank String (Leaderboard Display)

```
rankString():
    Returns pipe-delimited string:
    "Name|Level|Social|Fame|Skill|GuildRank|ClanName|..."
```

Used for saving scores and displaying rankings.

---

## 14. Peer Description (MadLib-based)

When another player views your hero (arPeer), a description is generated:

```
Template variables:
    $NAME$    → hero name
    $TITLE$   → rank title
    $RACE$    → looks.Race
    $BUILD$   → looks.Build
    $SIGN$    → looks.Sign
    $SKIN$    → looks.Skin
    $EYES$    → looks.Eyes
    $HAIR$    → looks.Hair
    $HABIT$   → looks.Habit
    $MARKS$   → looks.Marks
    $PHRASE$  → looks.Phrase
    
    Gender-based: $HE$, $HIM$, $HIS$, $MAN$, $BOY$
```

Description includes: Race, build, age, combat stats, notable traits, equipment summary.
