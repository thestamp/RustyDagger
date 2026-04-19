# 06 — Monster System

## 1. Monster Structure

Each monster is defined as an itMonster entity with:

```
Fields:
    name:    string          // Display name
    guts:    int             // HP / physical power (base, before scaling)
    wits:    int             // Intelligence
    charm:   int             // Social ability
    baseA:   int             // Base attack (before gear)
    baseD:   int             // Base defend (before gear)
    baseS:   int             // Base skill (before gear)
    picture: string          // Image path (e.g., "Fields/Rodent.jpg")
    text:    itText          // Encounter description (MadLib template)
    opts:    string[]        // Available interaction options
    values:  itList          // Properties (passion, adjust, etc.)
    pack:    itList          // Carried items / loot table
    temp:    itList          // Per-combat resources (Actions, guild skills)
    gear:    itList          // Equipped weapons/armor
    stance:  int             // 0-4 (passive → aggressive)
```

---

## 2. Monster Balancing

When an encounter begins, the monster is scaled to the hero:

```
balance(weight):
    // Step 1: Dragon trait check
    if hero has DRAGON: add "trade" option
    
    // Step 2: Scale primary stats
    calcPrimary(weight):
        scaleFactor = 0.9 + 0.1 × hero.level
        guts  = floor(guts × scaleFactor)
        wits  = floor(wits × scaleFactor)
        charm = floor(charm × scaleFactor)
        
        // Randomize within range
        guts  = spread(guts)
        wits  = spread(wits)
        charm = spread(charm)
        
        // Adjustment (if "adjust" value is set)
        if values.find("adjust"):
            // Scale stats to match expected power
            ratio = expected_power / actual_power
            guts  = floor(guts × ratio)
            wits  = floor(wits × ratio)
            charm = floor(charm × ratio)
    
    // Step 3: Derive combat stats from gear
    calcCombat():
        attack = gear.fullAttack()
        defend = gear.fullDefend()
        skill  = gear.fullSkill()
    
    // Step 4: Secondary stats
    calcSecondary(weight):
        actions = temp.Actions (or 1)
        fame    = (guts + wits + charm) / 30 + (skill + weight) / 4
        exp     = ((1 + attack) × (100 + skill)) / 100
        
        // Stance from passion
        stance = match values.passion:
            "aggressive" → 4
            "hostile"    → 3
            "defensive"  → 2
            "timid"      → 1
            "passive"    → 0
            default      → 0
    
    // Step 5: Materialize loot
    buildPack():
        for each item in pack:
            if itPercent: if roll(100) < value → add to real pack
            if itRandom: add roll(maxCount) copies
            if itCount: keep
    
    // Step 6: Materialize gear
    buildGear():
        for each item in gear:
            if itPercent: materialize from ArmsTable
```

### Spread Function (Value Randomization)

```
spread(value):
    min = 5 × value / 7
    return 1 + min + twice(value - min)
    // Result range: [1 + min, 1 + value + min]
    // Bell-curve centered around value
```

---

## 3. Monster Definitions by Area

### 3.1 Town Guard

| Field | Value |
|-------|-------|
| Name | Town Guard |
| Guts/Wits/Charm | 50/50/50 |
| Base A/D/S | 5/5/5 |
| Picture | Faces/Gareth.jpg |
| Passion | hostile |
| Pack | 0-5 Marks |
| Gear | Long Sword, Chain Mail |
| Options | help, backstab, swindle, control |
| Text | Guard challenges hero at castle gate |

### 3.2 Fields Monsters (7)

| Monster | Guts | Wits | Charm | Base A/D/S | Passion | Image |
|---------|------|------|-------|------------|---------|-------|
| Rodent | 8 | 5 | 2 | 2/1/1 | timid | Fields/Rodent.jpg |
| Goblin | 15 | 10 | 5 | 4/2/2 | hostile | Fields/Goblin.jpg |
| Centaur | 25 | 20 | 15 | 6/4/8 | defensive | Fields/Centaur.jpg |
| Merchant | 20 | 15 | 25 | 3/3/3 | passive | Fields/Oldman.jpg |
| Wizard | 20 | 30 | 15 | 2/2/10 | defensive | Fields/Oldman.jpg |
| Gypsy | 15 | 20 | 20 | 3/2/6 | passive | Fields/Gypsy.jpg |
| Soldier | 30 | 15 | 10 | 8/6/4 | aggressive | Fields/Soldier.jpg |

**Key Loot:**
- Rodent: Teeth (80%), Food (×5)
- Goblin: Marks (10-20), Short Sword (20%), Turnip (40%)
- Centaur: Marks (50-100), War Spear (10%), Long Bow (10%), Hooves (80%)
- Merchant: Marks (100-500), various goods
- Wizard: Scrolls (various %), Identify Scroll (50%)
- Gypsy: Marks (20-50), Maps (rare), Fortune telling
- Soldier: Marks (30-80), Long Sword (30%), Chain Mail (20%)

### 3.3 Forest Monsters (6)

| Monster | Guts | Wits | Charm | Base A/D/S | Passion | Image |
|---------|------|------|-------|------------|---------|-------|
| Boar | 20 | 8 | 5 | 5/3/2 | aggressive | Forest/Boar.jpg |
| Orc | 30 | 12 | 8 | 8/5/3 | hostile | Forest/Orc.jpg |
| Elf | 20 | 25 | 20 | 5/3/12 | defensive | Forest/Elf.jpg |
| Gryphon | 40 | 20 | 10 | 12/8/10 | hostile | Forest/Gryphon.jpg |
| Snot | 15 | 15 | 5 | 4/2/4 | aggressive | Forest/Snot.jpg |
| Unicorn | 35 | 30 | 30 | 8/5/15 | passive | Forest/Unicorn.jpg |

**Key Loot:**
- Boar: Tusks (80%), Food (×5), War Tusk (20%)
- Orc: Marks (20-50), Battle Axe (20%), Hard Leather (30%)
- Elf: Elf Bow (15%), Gems (various), trade items
- Gryphon: Horn (30%), Feathers, Gold
- Snot: Teeth (50%), slimy items
- Unicorn: Unicorn Horn (5%), major quest reward

### 3.4 Hills Monsters (7)

| Monster | Guts | Wits | Charm | Base A/D/S | Passion | Image |
|---------|------|------|-------|------------|---------|-------|
| Goat | 20 | 10 | 5 | 4/4/6 | defensive | Hills/Goat.jpg |
| Basilisk | 40 | 20 | 5 | 10/8/5 | hostile | Hills/Basilisk.jpg |
| Troll | 60 | 15 | 8 | 15/10/3 | aggressive | Hills/Troll.jpg |
| Wyvern | 50 | 25 | 10 | 12/6/12 | hostile | Hills/Wyvern.jpg |
| Sphinx | 45 | 40 | 30 | 8/5/15 | defensive | Hills/Sphinx.jpg |
| Giant | 80 | 20 | 10 | 25/15/5 | aggressive | Hills/Giant.jpg |
| Dragon | 250 | 250 | 250 | 50/30/40 | aggressive | Hills/Dragon.jpg |

**Special Abilities:**
- Goat: `goatSkill()` — steals Rope
- Wyvern: Soul-steal themed (disease weapon)
- Troll: Regenerates with Troll Wart in pack
- Dragon: Highest-tier enemy; massive loot table; requires DRAGON trait for trade

**Key Loot:**
- Goat: Horn (50%), Rope (stolen)
- Basilisk: Scales (60%), Gems
- Troll: Wart (80%), Gold, Club
- Wyvern: Scales (40%), Wings, high-value gems
- Sphinx: Riddle-focused encounter, rare gems
- Giant: Crown (20%), Great weapons (rare)
- Dragon: Dragon Shield (10%), Scales (80%), massive gold, best items

### 3.5 Goblin Mound Monsters (10)

**Warrens Level:**

| Monster | Guts | Wits | Charm | Passion | Image |
|---------|------|------|-------|---------|-------|
| Gate Guard | 30 | 20 | 15 | hostile | Mound/Gate.jpg |
| Gang | 25 | 15 | 10 | aggressive | Mound/Gang.jpg |
| Rager | 35 | 10 | 5 | aggressive | Mound/Rager.jpg |
| Thief | 20 | 25 | 20 | passive | Mound/Thief.jpg |
| Worm | 40 | 5 | 2 | aggressive | Mound/Worm.jpg |

**Treasury Level:**

| Monster | Guts | Wits | Charm | Passion | Image |
|---------|------|------|-------|---------|-------|
| Mage | 30 | 35 | 20 | defensive | Mound/Mage.jpg |
| Guard | 50 | 30 | 20 | hostile | Mound/Guard.jpg |
| Vault | 80 | 10 | 10 | aggressive | Mound/Vault.jpg |

**Throne Room Level:**

| Monster | Guts | Wits | Charm | Passion | Image |
|---------|------|------|-------|---------|-------|
| Champion | 70 | 40 | 30 | aggressive | Mound/Champ.jpg |
| Queen | 60 | 50 | 50 | defensive | Mound/Queen.jpg |

**Special:**
- Worm: `wormSkill()` — steals glowing/flame equipment
- Thief: Has Swindle option, carries Thief Insurance
- Mage: Has Control option, carries scrolls
- Champion: Has flaming weapons

### 3.6 Castle / Dungeon Monsters

**Castle Guard:**

| Field | Value |
|-------|-------|
| Guts/Wits/Charm | 100/100/100 |
| Base A/D/S | 15/10/10 |
| Passion | hostile |
| Image | Faces/Gareth.jpg |

**Vortex Guard (Evil Fred):**

| Field | Value |
|-------|-------|
| Guts/Wits/Charm | 150/150/150 |
| Base A/D/S | 25/20/20 |
| Passion | aggressive |

**Dungeon Variants** (same monsters as Mound but higher stats):
- Dungeon Rodent, Dungeon Snot, Dungeon Rager, Dungeon Gang, Dungeon Troll, Dungeon Mage

### 3.7 Ocean Monsters (3)

| Monster | Guts | Wits | Charm | Passion | Image |
|---------|------|------|-------|---------|-------|
| Traders | 40 | 30 | 40 | passive | Ocean/Pirates.jpg |
| Serpent | 80 | 30 | 10 | aggressive | Ocean/Serpent.jpg |
| Mermaid | 30 | 40 | 50 | passive | Ocean/Mermaid.jpg |

### 3.8 Hie Brasil Monsters (5)

| Monster | Guts | Wits | Charm | Passion | Image |
|---------|------|------|-------|---------|-------|
| Harpy | 60 | 30 | 15 | aggressive | Brasil/Harpy.jpg |
| Fighter | 80 | 40 | 20 | hostile | Brasil/Fighter.jpg |
| Golem | 120 | 20 | 5 | aggressive | Brasil/Golem.jpg |
| Medusa | 50 | 60 | 40 | defensive | Brasil/Medusa.jpg |
| Hero | 100 | 60 | 40 | hostile | Brasil/Hero.jpg |

### 3.9 Shangala Monsters (7)

| Monster | Guts | Wits | Charm | Passion | Image |
|---------|------|------|-------|---------|-------|
| Gunner | 50 | 25 | 15 | hostile | Shang/Gunner.jpg |
| Peasant | 20 | 15 | 20 | passive | Shang/Peasant.jpg |
| Ninja | 40 | 40 | 15 | aggressive | Shang/Ninja.jpg |
| Plague Carrier | 30 | 20 | 10 | hostile | Shang/Plague.jpg |
| Shogun | 80 | 50 | 40 | hostile | Shang/Shogun.jpg |
| Panda (Blind Woman) | 100 | 120 | 120 | passive | Shang/Panda.jpg |
| Samurai | 60 | 50 | 30 | defensive | Shang/Samurai.jpg |

**Special:**
- Samurai: Has "bushido" option — hero can train Ieatsu with Bushido Token
- Panda: Very high stats but passive; designed for non-combat interaction
- Plague Carrier: Disease weapons

### 3.10 Special Encounters

**Faery:**
- Appears as 1% random chance in any wilderness area
- Has "capture" option
- Can be captured → Bottled Faery item
- Image: Other/Faery.jpg

---

## 4. Quest Weight Distribution

Monsters are selected by weighted random from area-specific tables:

### Fields (weight varies by hero level)

**Level < 3 (low weights):**

| Monster | Weight |
|---------|--------|
| Rodent | 12 |
| Goblin | 10 |
| Centaur | 6 |
| Merchant | 10 |
| Wizard | 2 |
| Gypsy | 1 |
| Soldier | 0 |

**Level ≥ 3 (high weights):**

| Monster | Weight |
|---------|--------|
| Rodent | 8 |
| Goblin | 6 |
| Centaur | 4 |
| Merchant | 5 |
| Wizard | 2 |
| Gypsy | 5 |
| Soldier | 2 |

### Forest

| Monster | Weight |
|---------|--------|
| Boar | 10 |
| Orc | 9 |
| Elf | 8 |
| Gryphon | 6 |
| Snot | 4 |
| Unicorn | 3 |

### Hills

| Monster | Weight |
|---------|--------|
| Goat | 7 |
| Basilisk | 5 |
| Troll | 5 |
| Wyvern | 4 |
| Giant | 3 |
| Sphinx | 3 |

**Note:** Dragon is NOT in the normal weight table — it's a special encounter via mines/cavern.

### Mound (3 depth levels, 5 weights each)

**Level 0 (Warrens):** Gate(10), Gang(8), Rager(6), Thief(4), Worm(3)  
**Level 1 (Treasury):** Gang(5), Rager(6), Thief(5), Mage(6), Guard(3)  
**Level 2 (Throne):** Guard(4), Mage(4), Vault(3), Champion(3), Queen(2)

### Castle (5 areas)

**Castle Guard:** Fixed encounter  
**Dungeon:** Rodent(8), Snot(6), Rager(5), Gang(4), Troll(3), Mage(2)  
**Ocean:** Traders(5), Traders(4), Serpent(3), Mermaid(2)  
**Brasil:** Harpy(5), Fighter(4), Golem(3), Medusa(3), Hero(2)  
**Shangala:** Gunner(6), Peasant(5), Ninja(4), Plague(3), Shogun(2), Panda(1), Samurai(1)

---

## 5. Area Power Levels

Each area has a `power` value that scales the encounter:

| Area | Power | Monster Stat Range |
|------|-------|--------------------|
| Fields | 1 | 8-30 Guts |
| Forest | 2 | 15-40 Guts |
| Mound | 3 | 20-80 Guts |
| Hills | 4 | 20-250 Guts |
| Dungeon | 2 | 15-60 Guts (dungeon variants) |
| Ocean | 3 | 30-80 Guts |
| Brasil | 4 | 50-120 Guts |
| Shangala | 5 | 20-100 Guts |

---

## 6. Monster Interaction Options

Each monster's `opts` list determines available player actions:

| Option | Interaction | Outcome |
|--------|-------------|---------|
| `help` | Offer assistance | Wits contest; win = half loot |
| `backstab` | Sneak attack | Thief action, doubled stats |
| `swindle` | Con/deceive | Charm contest; win = all gear |
| `control` | Mind control | Wits contest; win = all treasure + exp |
| `bribe` | Pay off | Charm contest + money |
| `feed` | Give food | Charm contest + food items |
| `riddle` | Test knowledge | Wits contest + random riddle |
| `trade` | Exchange goods | Charm + money for monster pack |
| `seduce` | Charm/flirt | Charm contest; win = loot + fame |
| `bushido` | Samurai train | Power contest + Bushido Token |
| `capture` | Catch creature | Wits contest (Faery only) |

---

## 7. Monster Text Templates

Monster descriptions use MadLib templates with variable substitution:

### Variables Available in Monster Text

| Variable | Substitution |
|----------|-------------|
| `$NAME$` | Hero name |
| `$HE$` / `$HIM$` / `$HIS$` | Gender pronouns |
| `$MAN$` / `$BOY$` | Gender nouns |
| `$CR$` | Line break |
| `$TB$` | Tab |
| Custom references | Picked from monster's value lists |

### Example Monster Text

```
"A scraggly rodent scurries out from behind some rocks.  
 It eyes $NAME$ hungrily, baring tiny yellow teeth."
```

Rendered for female hero "Luna":
```
"A scraggly rodent scurries out from behind some rocks.  
 It eyes Luna hungrily, baring tiny yellow teeth."
```
