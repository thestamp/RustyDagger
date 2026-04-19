# 11 — Progression

## 1. Daily Cycle

### 1.1 New Day Detection

Each login checks `isNewday()`. If the game day has advanced:

1. Quests reset to `baseQuests`
2. Overnight hazards apply (see §2)
3. Fame decays
4. Stipend paid (if in town lodging)
5. Gear decay processed

### 1.2 Quest Allowance

```
baseQuests = derived from hero stats
    base = guts / 10 + wits / 15 + charm / 20
    trait modifiers:
        ENERGETIC: +3
        COUCH_POTATO: -2 (conceptual, from traits)
    minimum: 5
```

Each quest action (adventure, court activity, drink, training) costs 1 quest.

---

## 2. Overnight Hazards

### 2.1 Effective Decay Rate

```
decay = PlaceTable[location].decay
for each camp item matching location's use flags:
    decay += 1

// Maximum decay with all items + best location:
// SUITE(6) + Sleeping Bag + Cooking Gear + Camp Tent = 9
// Worst case: FOREST(2) with no items = 2
```

### 2.2 Hazard Rolls

Each hazard: `roll(decay) == 0` triggers it.

| Hazard | Trigger Probability (decay=2) | Trigger Probability (decay=6) | Effect | Immunity |
|--------|------------------------------|------------------------------|--------|----------|
| Disease | 50% | 17% | `ail(skill/decay)` | Fighter ≥ 1 |
| Injury | 50% | 17% | `addWounds(guts/decay - 1)` | Fighter ≥ 2 |
| Exhaustion | 50% | 17% | `addFatigue(quests/(decay+1) - 1)` | Fighter ≥ 3 |

### 2.3 Gear Decay Rate

```
rate = (decay + fightRank) × 5
```

- Higher value = slower decay (rate is used as denominator)
- Camp items can break: `roll(rate) == 0` destroys one

---

## 3. Fame System

### 3.1 Daily Fame Decay

```
fame = fame - fame/10 + social × 10
```

- Loses 10% per day
- Gains `social × 10` per day (higher rank = more baseline fame)
- Equilibrium: `fame = social × 100` (when decay = gain)

### 3.2 Fame Sources

| Action | Fame Gain |
|--------|-----------|
| Help monster (success) | +weight |
| Riddle (success) | +weight |
| Seduce (success) | +weight |
| Combat victory | Varies by monster |
| Court activities | Via favor → rank |

### 3.3 Fame Effects

Fame contributes to ranking position. Rankings are viewable at Castle.

---

## 4. Social Rank Advancement

### 4.1 Rank Table

| Rank | Title | Index |
|------|-------|-------|
| 0 | Commoner | 0 |
| 1 | Peasant | 1 |
| 2 | Baron | 2 |
| 3 | Viscount | 3 |
| 4 | Count | 4 |
| 5 | Marquis | 5 |
| 6 | Duke | 6 |
| 7 | Archduke | 7 |
| 8 | Prince | 8 |
| 9 | King | 9 |

### 4.2 Advancement Mechanic

Promotion happens at Queen's Court via Petition:

```
cost: $5,000 + 3 quests
index = (favor / divisor) × 4 / rankCost[rank]
divisor = 700 if POPULAR trait, else 1000

if index >= 4:
    social += 1
    favor = 0
```

### 4.3 Rank Benefits

| Benefit | Formula |
|---------|---------|
| Daily stipend | `social² × 50` marks |
| Daily fame base | `social × 10` |
| Healing discount | costLevel = `max(1, level - social - 1)` |
| Clan creation | Requires social ≥ 2 (Baron) |
| Court access | Requires social ≥ 1 |

---

## 5. Experience & Leveling

### 5.1 Experience Sources

| Source | Amount |
|--------|--------|
| Combat victory | `baseExp + (2×guts + wits + charm) × weight / 4` |
| Social victory | `baseExp + bonuses` |
| Tithe at temple | `tithe / level²` |
| Court games | Indirect (via favor → rank) |

### 5.2 Level Thresholds

```
raise[level] = 50 × 1.5^(level - 1)

Level 1:  50
Level 2:  75
Level 3:  112
Level 4:  168
Level 5:  253
Level 10: 1,924
Level 15: 14,629
Level 20: 111,169
```

### 5.3 Level Up Process

```
if exp >= raise:
    level += 1
    exp -= raise
    raise = next threshold
    → player allocates stat point(s) to guts/wits/charm
```

---

## 6. Stat Growth

### 6.1 Direct Stat Gains

Stats grow through use during quests:

| Function | Effect |
|----------|--------|
| `gainGuts(weight)` | Guts increases by a fraction of weight |
| `gainWits(weight)` | Wits increases by a fraction of weight |
| `gainCharm(weight)` | Charm increases by a fraction of weight |

### 6.2 Combat Victory Gains

| Victory Type | Stat Gains |
|-------------|------------|
| Normal Attack | `gainGuts(weight)` |
| Backstab | `gainCharm(weight×3)`, `gainGuts(weight×2)` |
| Berzerk | `gainGuts(weight×5)` |
| Ieatsu | `gainGuts(weight×5)` |
| Help | `gainWits(weight)` |
| Riddle | `gainWits(weight)` |
| Trade | `gainCharm(weight)` |
| Seduce | `gainCharm(weight×2)` |
| Control | `gainWits(weight)` |
| Swindle | `gainCharm(weight)` |

### 6.3 Training Losses

Guild training trades stats:

| Training | Gains | Loses |
|----------|-------|-------|
| Fighter | +1 fight, +1 temp fight | -2 wits, -2 charm |
| Magery | +1 magic, +1 temp magic | -2 guts, -2 charm |
| Thief | +1 thief, +1 temp thief | -2 guts, -2 wits |

---

## 7. Guild Rank Progression

```
guildRank = fight + magic + thief + ieatsu

Maximum per session: guildRank < level (cannot train past current level)
Training cost: guildRank × $1,000
Training fatigue: +5 quests
```

### 7.1 Skill Progression Requirements

| Skill | How to Advance |
|-------|---------------|
| Fighter | Train at Guild |
| Magery | Train at Guild |
| Thief | Train at Guild |
| Ieatsu | Bushido Token + Samurai encounter |

Ieatsu is unique — cannot be trained at the guild. Must find a Samurai in Shangala, have a Bushido Token, ≥10 quests remaining, and guildRank < level.

---

## 8. Aging

The hero ages over time (day counter). Aging is tracked but has no mechanical effect in the original implementation — it exists for flavor/roleplay in the peer description.

---

## 9. Death & Revival

### 9.1 Death Trigger

```
if wounds >= guts:
    hero dies
```

### 9.2 Death Consequences

- Hero respawns at last save location
- Pack may be partially looted
- Fame penalty: `fame -= fame / 5`
- Wounds set to guts - 1 (barely alive)

### 9.3 Permanent Death

The original game does NOT have permadeath. Heroes always revive.
