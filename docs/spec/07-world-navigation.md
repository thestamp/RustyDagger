# 07 — World & Navigation

## 1. Location Registry

Every location is a `PlaceRecord(name, screenClass, decay, useFlags, awakeMsg, sleepMsg)`.

| ID | Location | Screen Class | Decay | Use Flags | Description |
|----|----------|-------------|-------|-----------|-------------|
| 0 | SUITE | arTown | 6 | `"s"` | Best town lodging |
| 1 | ROOM | arTown | 5 | `"s"` | Mid-tier room |
| 2 | FLOOR | arTown | 4 | `"b"` | Tavern floor |
| 3 | TOWN | arTown | 3 | `""` | Default town spawn |
| 4 | FIELDS | arField | 3 | `"cb"` | Entry wilderness |
| 5 | FOREST | arForest | 2 | `"cb"` | Mid wilderness |
| 6 | MOUND | arMound | 2 | `"b"` | Goblin Mound |
| 7 | COT | arMound | 3 | `"bc"` | Goblin Inn cot |
| 8 | HILLS | arHills | 2 | `"bc"` | Mountains |
| 9 | DOCKS | arCastle | 2 | `""` | Castle docks |
| 10 | DUNJEON | arCastle | 2 | `"b"` | Castle dungeon |
| 11 | limbo | null | 3 | `"bc"` | Fallback location |

### Use Flags

| Flag | Item Required | Bonus |
|------|--------------|-------|
| `b` | Sleeping Bag | +1 decay |
| `c` | Cooking Gear | +1 decay |
| `t` | Camp Tent | +1 decay |
| `s` | — | Stipend on new day (`social² × 50`) |

---

## 2. Wake-up / New Day System

On login, if `isNewday()` is true:

### 2.1 Camping Gear Decay Bonus

For each use flag with a matching camp item in hero pack, effective decay increases by 1.

### 2.2 Overnight Hazards

Each hazard rolls `roll(decay) == 0` to trigger:

| Hazard | Effect | Immunity |
|--------|--------|----------|
| Disease | `ail(skill / decay)` — reduces skill | Fighter rank ≥ 1 |
| Injury | `addWounds(guts/decay - 1)` — reduces HP | Fighter rank ≥ 2 |
| Exhaustion | `addFatigue(baseQuests/(decay+1) - 1)` | Fighter rank ≥ 3 |

### 2.3 Gear Decay

```
rate = (decay + fightRank) × 5
```

- All equipped gear and pack items decay at this rate
- Camp items (Sleeping Bag, Cooking Gear, Camp Tent) can break: `roll(rate) == 0` destroys one

### 2.4 Daily Reset

- Quests reset to `baseQuests` (derived from guts/traits)
- Fame decays: `fame = fame - fame/10 + social × 10`
- Stipend (if in town lodging): `money += social² × 50`

---

## 3. Area Screens

### 3.1 Salamander Township

**Class:** `arTown`  
**Title:** "Welcome to Salamander Township"  
**Background:** `Color(0, 255, 255)`

| Clickable | Position | Size | Requirement | Destination |
|-----------|----------|------|-------------|-------------|
| Tavern | (10, 60) | 96×64 | — | arTavern |
| Weapons | (160, 40) | 96×64 | — | arWeapon |
| Armour | (30, 170) | 96×64 | — | arArmour |
| Castle Gate | (300, 20) | 64×96 | level ≥ 6 | arCastle (see §3.1.1) |
| Trade Shop | (150, 180) | 96×64 | — | arTrader |
| Leave Town | (280, 150) | 96×64 | — | arField |

#### 3.1.1 Castle Gate Entry

```
if social > 0 OR hasPack("Castle Permit"):
    → arCastle
else:
    → quest vs "Town:Guard" at weight=3
```

### 3.2 The Fields

**Class:** `arField`  
**Title:** "The Fields near Salamander Township"  
**Background:** `Color(255, 128, 128)`

| Clickable | Position | Size | Requirement | Action |
|-----------|----------|------|-------------|--------|
| Town | (30, 40) | 96×64 | — | → arTown |
| Tower (Guild) | (320, 55) | 64×96 | — | → arGuild (Forest) |
| Quest | (145, 135) | 96×64 | — | Start quest |
| Camp | (265, 185) | 96×64 | — | → arStatus |
| Forest | (10, 180) | 96×64 | level ≥ 4 | Try enter Forest |
| Mound | (190, 30) | 96×64 | level ≥ 8 | Mound gate quest |

**Forest Travel:** `contest(wits, 40)` — success → arForest + `gainWits(2)`, fail → forced quest  
**Mound Entry:** Spawns `"Mound:Gate"` at weight 2

### 3.3 The Forest

**Class:** `arForest`  
**Title:** "The Depths of the Arcane Forest"  
**Background:** `Color(0, 128, 0)`

| Clickable | Position | Size | Requirement | Action |
|-----------|----------|------|-------------|--------|
| Smithy | (20, 170) | 96×64 | Discovered (hidden bit) | → arDwfSmith |
| Guild Tower | (320, 150) | 64×96 | Discovered (hidden bit) | → arGuild |
| Hills Trail | (10, 30) | 96×64 | Discovered (hidden bit) | Try enter Hills |
| Fields | (300, 10) | 96×64 | — | Try return to Fields |
| Quest | (160, 60) | 96×64 | — | Start quest |
| Camp | (180, 180) | 96×64 | — | → arStatus |

**Hidden locations (3):** Smithy, Guild Tower, Mountain Trail  
**Discovery:** `doSearch()` → `contest(wits, power × 20)` reveals one hidden bit  
**Hills Travel:** `contest(wits, 80)` — success → arHills  
**Fields Return:** `contest(wits, 20)` — success → arField

### 3.4 The Hills

**Class:** `arHills`  
**Title:** "High Crags of the Fenris Mountains"  
**Background:** `Color(160, 160, 160)`

| Clickable | Position | Size | Requirement | Action |
|-----------|----------|------|-------------|--------|
| Jewel Shop | hidden | discovered | → arGemShop |
| Magic Shop | hidden | discovered | → arMagicShop |
| Abandoned Mines | hidden | discovered | Enter cavern |
| Forest | visible | — | Try return to Forest |
| Quest | visible | — | Start quest |
| Camp | visible | — | → arStatus |

**Rope Requirement:** `needsRope = true` — must have HILLFOLK trait, or consume 1 Rope from pack  
**Cavern Entry:** Requires rope; spawns `"Hills:Dragon"` at weight 5  
**Forest Return:** `contest(wits, 40)`  
**Discovery:** `contest(wits, power × 20)` reveals hidden: Jewel Exchange, Magic Shop, Abandoned Mines

### 3.5 The Goblin Mound

**Class:** `arMound`  
**Title:** "The Bowels of the Goblin Mound"  
**Background:** dark

**Light Requirement:** `needsLight = true` — must have CATSEYES trait, or glowing/flame gear, or consume 1 Torch  
**3 Depth Levels** — gated by maps:

| Level | Name | Map Required | Quest Pool |
|-------|------|-------------|------------|
| 0 | Warrens | Map to Warrens | Gate, Gang, Rager, Thief, Worm |
| 1 | Treasury | Map to Treasury | Gang, Rager, Thief, Mage, Guard |
| 2 | Throne Room | Map to Throne Room | Guard, Mage, Vault, Champion, Queen |

**Vortex:** Requires Map to Vortex; `contest(wits, 100)` → `"Vortex:Guard"` at weight 4  
**Fields Return:** `contest(wits, 50)`

**Inn:** Smidgeon Crumb's Gobble Inn accessible within Mound (see Economy spec)

### 3.6 Dragon Keep / Castle

**Class:** `arCastle`  
**Title:** "The Central Courtyard of Dragon Keep"  
**Background:** `Color(255, 128, 255)`

| Clickable | Position | Size | Requirement | Destination |
|-----------|----------|------|-------------|-------------|
| Town | (20, 175) | 96×64 | — | arTown |
| Royal Court | (155, 45) | 96×64 | social > 0 (else guard quest) | arQueen |
| Dungeon | (15, 70) | 96×64 | level ≥ 8 | Dungeon quests |
| Clan Hall | (295, 40) | 64×96 | — | arClanHall |
| Postal | (140, 170) | 96×64 | — | arPostal |
| Docks | (280, 180) | 96×64 | level ≥ 10 | Sail (see §3.6.1) |

#### 3.6.1 Dock Navigation

```
contest(wits, 100)
if success:
    if hasPack("Rutter") AND roll(100) < 70:
        → Shangala quests (power=5)
    elif hasPack("Rutter") AND roll(100) < 70:
        → Brasil quests (power=4)
    else:
        → Ocean quests (power=3)
else:
    → Ocean quests (power=3)
```

**Sub-area powers:**

| Region | Power |
|--------|-------|
| Castle Guard | 4 |
| Dungeon | 2 |
| Ocean | 3 |
| Hie Brasil | 4 |
| Shangala | 5 |

### 3.7 Queen's Court

**Class:** `arQueen`  
**Title:** "Queen Beth reigns over Dragon Court"  
**Background:** `Color(255, 128, 128)`

Entry requires `social > 0` (any noble rank), else redirected to castle guard quest.

See 09-economy.md for full court activity details.

---

## 4. Navigation Flow Graph

```
                    ┌──────────┐
                    │  Login   │
                    └────┬─────┘
                         ▼
              ┌──────────────────┐
              │   TOWN (default) │
              └──┬──┬──┬──┬──┬──┘
                 │  │  │  │  │
     ┌───────────┘  │  │  │  └──────────────┐
     ▼              ▼  ▼  ▼                 ▼
  Tavern        Weapons Armour         Leave Town
  (Inn/Storage) (Shop)  (Shop/Smith)        │
                                            ▼
                              ┌─────────────────────┐
              Castle Gate ◄───│      FIELDS          │
              (level ≥ 6)     │                      │
                   │          │  Forest (level ≥ 4)  │
                   ▼          │  Mound  (level ≥ 8)  │
          ┌────────────────┐  └──┬──────────┬────────┘
          │    CASTLE      │     │          │
          │                │     ▼          ▼
          │ Court (social) │  ┌──────┐  ┌──────────┐
          │ Dungeon (≥8)   │  │FOREST│  │  MOUND   │
          │ Clan Hall      │  │      │  │(3 depths)│
          │ Postal         │  │Smith │  │ Goblin   │
          │ Docks (≥10)    │  │Guild │  │  Inn     │
          └──┬─────────────┘  │Hills │  └──────────┘
             │                └──┬───┘
             ▼                   ▼
      ┌──────────┐       ┌──────────┐
      │ DOCKS    │       │  HILLS   │
      │ Ocean    │       │ Gem Shop │
      │ Brasil   │       │ Mag Shop │
      │ Shangala │       │ Mines    │
      └──────────┘       └──────────┘
```

---

## 5. Wilderness Screen Base Class

`WildsScreen` provides the template for all outdoor adventure areas.

### 5.1 Quest Execution

```
canAdvance():
    if quests < 1: "too tired" → return false
    if needsRope AND NOT findClimb(): "need rope" → return false
    if needsLight AND NOT findLight(): "need light" → return false
    return true

findClimb():
    return hasTrait(HILLFOLK) OR subPack("Rope", 1) == 1

findLight():
    return hasTrait(CATSEYES)
        OR findGearTrait(GLOWS)
        OR findGearTrait(FLAME)
        OR subPack("Torch", 1) > 0
```

### 5.2 Encounter Selection

```
selectQuest():
    if roll(100) == 0:           // 1% chance
        return Faery encounter
    else:
        return weighted random from area beast list
```

### 5.3 Discovery Mechanic

```
doSearch():
    if contest(wits, power × 20):
        reveal next hidden location (markFound)
        return true
    return false
```

---

## 6. Area Access Summary

| Area | Level Req | Item Req | Stat Check | Additional |
|------|-----------|----------|-----------|------------|
| Town | — | — | — | Starting area |
| Fields | — | — | — | Leave town |
| Forest | 4 | — | contest(wits, 40) | From Fields |
| Hills | — | Rope or HILLFOLK | contest(wits, 80) | From Forest (discovered) |
| Mound | 8 | Light source | Gate guard quest | From Fields |
| Mound depths | — | Area maps | — | Maps drop from mound monsters |
| Castle | 6 | Castle Permit OR social > 0 | — | Castle guard if no access |
| Queen's Court | — | social > 0 | — | Guard quest otherwise |
| Dungeon | 8 | — | — | From Castle |
| Docks | 10 | — | contest(wits, 100) | From Castle |
| Brasil | — | Rutter (70% chance) | — | Via Docks |
| Shangala | — | Rutter (70% chance) | — | Via Docks |
| Dragon Cavern | — | Rope | Mines discovered | From Hills |
