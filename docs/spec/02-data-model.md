# 02 — Data Model

## 1. Entity Hierarchy

All game objects derive from a common `Item` interface. The original Java code uses a factory pattern to construct items from a pipe-delimited serialization format.

```
Item (interface)
  ├── itToken          — Simple named item (name only)
  ├── itCount          — Stackable item with count (obfuscated storage)
  ├── itValue          — Key-value pair (string value)
  ├── itPercent        — Percentage-chance item (for loot tables)
  ├── itRandom         — Random-count item (for loot tables)
  ├── itText           — Template text block with MadLib substitution
  ├── itNote           — In-game mail/message (from, date, body)
  ├── itList           — Container holding other items (Vector-based)
  │   ├── itArms       — Equipment with combat stats + traits
  │   └── itAgent      — Character base (stats, gear, pack, etc.)
  │       ├── itHero   — Player character
  │       └── itMonster— Enemy character
  └── (plain strings are valid tokens)
```

---

## 2. Serialization Format

All entities serialize to a custom pipe-delimited text format with nested braces.

### Token Symbols

| Symbol | Meaning | Constant |
|--------|---------|----------|
| `{` | Open entity | OPEN |
| `\|` | Field separator | DIVIDE |
| `}` | Close entity | CLOSE |

### Format Patterns

| Type | Format | Example |
|------|--------|---------|
| **itCount** | `{#\|name\|count}` | `{#\|Gold Apple\|2}` |
| **itPercent** | `{%\|name\|percent}` | `{%\|Flame Scroll\|80}` |
| **itRandom** | `{@\|name\|maxCount}` | `{@\|Gold Apple\|5}` |
| **itValue** | `{=\|key\|value}` | `{=\|pic\|Shang/Panda.jpg}` |
| **itText** | `{itText\|text\|message...}` | `{itText\|text\|A blind old woman...}` |
| **itNote** | `{itNote\|name\|from\|date\|body}` | `{itNote\|Letter\|Fred\|Today\|Hello}` |
| **itList** | `{~\|name\|items...}` | `{~\|pack\|{#\|Gold\|50}\|{#\|Food\|3}}` |
| **itArms** | `{itArms\|name\|atk\|def\|skill\|traits...}` | `{itArms\|Long Sword\|12\|0\|5}` |
| **itHero** | `{itHero\|name\|guts\|wits\|charm\|lists...}` | See §2.1 |
| **itMonster** | `{itMonster\|name\|hp\|guts\|wits\|charm\|baseA\|baseD\|baseS\|...}` | See §2.2 |
| **itToken** | Any unbraced string | `Sleeping Bag` |

### 2.1 Hero Serialization

```
{itHero|NAME|GUTS|WITS|CHARM|
  {~|gear|...equipment items...}|
  {~|pack|{#|Marks|N}|...items...}|
  {~|temp|{#|Actions|N}|{#|fight|N}|{#|magic|N}|{#|thief|N}|{#|ieatsu|N}|...}|
  {~|stat|{#|Fame|N}|{#|Stipend|N}|...traits...}|
  {~|rank|{#|Social|N}|{#|Favor|N}|...}|
  {~|looks|{=|Title|...}|{=|Gender|male}|...}|
  {~|store|...stored items...}
}
```

### 2.2 Monster Serialization

```
{itMonster|NAME|GUTS|WITS|CHARM|BASE_ATK|BASE_DEF|BASE_SKL|
  {=|pic|path/to/image.jpg}|
  {~|values|{=|passion|aggressive}|{=|adjust|true}|...}|
  {~|pack|{#|Marks|N}|{%|Item|chance}|{@|Item|max}|...}|
  {~|temp|{#|Actions|N}|{#|fight|N}|{#|magic|N}|...}|
  {~|gear|{%|WeaponName|0}|{%|ArmorName|0}|...}|
  {~|opts|option1|option2|...}|
  {itText|text|Description with $VARIABLES$...}
}
```

---

## 3. Item Interface

All items implement these core methods:

```
interface Item {
    // Identity
    getName(): string
    setName(name: string): void
    getIcon(): string          // Display icon/sprite identifier
    
    // Quantity
    getCount(): int            // Stack count (1 for non-stackable)
    setCount(n: int): void
    add(n: int): void          // Increase count
    sub(n: int): void          // Decrease count (clamped to 0)
    
    // Value
    getValue(): int            // Monetary/stat value
    setValue(n: int): void
    
    // Display
    toShow(): string           // UI display string
    toLoot(): string           // Loot table display string
    
    // Matching
    isMatch(name: string): boolean    // Case-insensitive name match
    isMatch(item: Item): boolean      // Item equality
    
    // Degradation
    decay(rate: int): void     // Random deterioration
    
    // Serialization
    toString(depth: int): string      // Serialize with indent
    copy(): Item                      // Deep clone
}
```

---

## 4. Entity Type Details

### 4.1 Token (`itToken`)

Simplest item — just a name.

```
Fields:
  name: string
  hash: int          // Hash for validation

Methods:
  getCount() → 1     // Always 1
  getValue() → 0     // No value
  copy() → new Token(name)
```

**Parsing:** Any unrecognized string in a buffer becomes a Token.

### 4.2 CountItem (`itCount`)

Stackable item with **obfuscated count storage**.

```
Fields:
  name: string
  value: int         // Stored as (actual_count + offset)
  offset: int        // Random value 1-1024

Methods:
  getCount() → value - offset
  setCount(n) → value = n + offset
  add(n) → value += n
  sub(n) → value = max(offset, value - n)   // Clamp to 0
  adds(n) → value += n                       // No bounds check
  makeCount() → static factory
```

**Security note:** The obfuscation uses a random offset to prevent trivial memory editing. The offset is chosen at construction time: `1 + Random.nextInt(1024)`.

### 4.3 ValueItem (`itValue`)

Key-value string storage.

```
Fields:
  name: string       // The key
  text: string       // The value

Methods:
  getValue() → parseInt(text) or 0
  toLong() → parseLong(text) or 0
  toInt() → parseInt(text) or 0
  toShow() → "name: text"
```

### 4.4 PercentItem (`itPercent`)

Used in loot tables — represents a percentage chance of an item appearing.

```
Fields:
  name: string       // Item name
  value: int         // Percentage (0-100)

Methods:
  getCount() → value
  toShow() → "name (value%)"
```

**Usage:** During monster `buildPack()`, each PercentItem is rolled: if `roll(100) < value`, the item materializes.

### 4.5 RandomItem (`itRandom`)

Used in loot tables — represents a random count (0 to max).

```
Fields:
  name: string       // Item name
  value: int         // Maximum count

Methods:
  getCount() → value  
```

**Usage:** During monster `buildPack()`, each RandomItem produces `roll(value)` copies.

### 4.6 TextItem (`itText`)

Template text with variable substitution via MadLib engine.

```
Fields:
  name: string       // Always "text"
  message: string    // Template with $VARIABLE$ placeholders

Methods:
  parseText() → MadLib instance with substituted values
  toShow() → rendered text
```

### 4.7 Note (`itNote`)

In-game mail message.

```
Fields:
  name: string       // "Note"
  from: string       // Sender name
  date: string       // Date string
  body: string       // Message body

Methods:
  getCount() → 1
  toShow() → "Note: from"
```

---

## 5. ItemList (`itList`)

The primary container class. Holds an ordered collection of Items.

```
Fields:
  name: string
  items: Vector<Item>

Core Methods:
  // Access
  getCount() → items.size()
  select(index: int) → Item at index
  find(name: string) → Item with matching name, or null
  
  // Modification
  append(item: Item) → void          // Add to end
  insert(item: Item, index: int)     // Insert at position
  drop(name: string) → void          // Remove first match by name
  fix(name: string, value: int)      // Find or create CountItem, set value
  clr(name: string) → void           // Remove CountItem by name
  
  // Query
  hasTrait(name: string) → boolean   // Does any item match name?
  findArms(traitId: int) → itArms    // Find equipment with specific trait
  
  // Aggregate Combat Stats
  fullAttack() → int                 // Sum of all items' attack values
  fullDefend() → int                 // Sum of all items' defend values  
  fullSkill() → int                  // Sum of all items' skill values
  
  // Trait Management
  fixTrait(name: string) → void      // Add trait if not present
  clrTrait(name: string) → void      // Remove trait
  
  // Loss
  loseHalf() → void                  // Randomly remove ~half of items (on death)
```

### loseHalf() Algorithm
```
for each item in list (backwards):
    if roll(2) == 0:
        remove item
```

---

## 6. Equipment (`itArms`)

Equipment items that can be worn in slots with combat stat modifications.

```
Fields:
  name: string
  aval: int          // Base attack value
  dval: int          // Base defend value
  sval: int          // Base skill value
  traits: itList     // Trait sub-items (slot, effects, enchantment)

Derived Stats:
  fullAttack() = aval + ((enchant + 9) / 10) + (8 if RIGHT_HAND + FLAME)
  fullDefend() = dval + ((enchant + 4) / 10) + (1 if BLESS)
  fullSkill()  = sval + enchant + (12 if RIGHT_HAND + LUCKY) + (2 if GLOWS)
```

### Equipment Slots (Traits)

| Trait | Slot | Notes |
|-------|------|-------|
| `HEAD` | Head | Caps, helms, coifs |
| `BODY` | Body | Robes, armor, plate |
| `RIGHT` | Right hand | Weapons (primary) |
| `LEFT` | Left hand | Shields, off-hand |
| `FEET` | Feet | Boots, sandals |

### Effect Traits

| Trait | Combat Effect | Stock Value |
|-------|---------------|-------------|
| `GLOWS` | Light source, +2 skill | 50 |
| `FLAME` | +8 attack if RIGHT hand | 800 |
| `BLESS` | +1 defend | 300 |
| `LUCKY` | +12 skill if RIGHT hand | 250 |
| `DISEASE` | Inflicts disease on hit | 1500 |
| `BLIND` | Inflicts blindness on hit | 4000 |
| `PANIC` | Inflicts panic on hit | 3000 |
| `BLAST` | Fixed 25×count damage | 2000 |
| `ENCHANT` | Enhancement level (numeric) | 100 per level |
| `DECAY` | Item deteriorates faster | - |
| `CURSED` | Cursed (not yet revealed) | - |
| `CURSE` | Cursed (revealed) | - |
| `SECRET` | Hidden identity until identified | - |

### Stock Value Formula
```
if SECRET or any CURSE trait:
    stockValue = 2
else:
    stockValue = ((aval + dval)² × 5 + (sval² × 2)) / 2 + sum(trait_values)
```

### Decay System
```
decay(rate):
    if roll(rate) == 0:
        // Degrade stats
        aval -= 1 ± (aval / 12)     // Randomly higher or lower
        dval -= 1 ± (dval / 12)
        sval -= 1 ± (sval / 12)
        
        // 1/12 chance to lose a random trait
        if roll(12) == 0 AND traits.size() > 0:
            remove random trait
        
        // Reduce enchantment
        enchant -= (enchant + 4) / 5
```

### Loot Randomization (`tweak()`)
```
tweak():
    aval = spread(aval)    // Randomize around base
    dval = spread(dval)
    sval = spread(sval)
    treasure_weight = 2048
```

### Wearability Checks
```
wearable() → has HEAD, BODY, RIGHT, LEFT, or FEET trait
isBright() → has GLOWS or FLAME
isCursed() → has CURSED or CURSE
revealCurse() → changes CURSED → CURSE (visible)
```

---

## 7. Agent (`itAgent`) — Abstract Character Base

Both Hero and Monster extend this. Provides stats, combat calculations, and inventory management.

### Core Stats

```
Fields:
  name: string
  guts: int          // Hit points, physical power
  wits: int          // Intelligence, speed component
  charm: int         // Social ability, persuasion
  
  // Derived (calculated)
  attack: int        // Offensive power
  defend: int        // Defensive power  
  skill: int         // Speed/accuracy
  
  // State
  state: enum { ALIVE, DEAD, CREATE, CONTROL, SWINDLE }
```

### Named Sub-Lists

| List | Purpose | Key Items |
|------|---------|-----------|
| `gear` | Equipped items | itArms entries |
| `pack` | Backpack inventory | Mixed items; must contain `{#\|Marks\|N}` |
| `temp` | Temporary/per-session | `Actions`, `fight`, `magic`, `thief`, `ieatsu` |
| `stat` | Persistent status | `Fame`, `Stipend`, traits |
| `rank` | Social progression | `Social`, `Favor` |
| `values` | Monster-specific data | `passion`, `adjust`, etc. |
| `acts` | Combat action queue | Blind, Panic, Disease, Blast counts |

### Combat Calculation

```
calcCombat():
    attack = gear.fullAttack()
    defend = gear.fullDefend()
    skill  = gear.fullSkill()
```

### Skill System

```
fight()    → consume 1 from temp.fight; return remaining
magic()    → consume 1 from temp.magic; return remaining
thief()    → consume 1 from temp.thief; return remaining
ieatsu()   → consume 1 from temp.ieatsu; return remaining

fightRank()  → temp.fight count (base rank)
magicRank()  → temp.magic count
thiefRank()  → temp.thief count
ieatsuRank() → temp.ieatsu count

guildRank()  → fightRank + magicRank + thiefRank + ieatsuRank
guildSkill() → fight + magic + thief + ieatsu (remaining uses)
```

### Effective Skill (Speed)

```
skill():
    base = gear.fullSkill()
    disease = stat disease count (if any)
    return base - disease
```

### Social Interaction Modifiers

| Method | Formula | Use |
|--------|---------|-----|
| `runWits()` | `wits + (wits if RANGER)` | Fleeing encounters |
| `bribeCharm()` | `charm + (charm if NOBLE)` | Bribing monsters |
| `tradeCharm()` | `charm + (charm if TRADER)` | Trading with monsters |
| `feedCharm()` | `charm + (charm if GYPSY)` | Feeding monsters |
| `seduceCharm()` | `charm` | Seduction attempts |

### Resource Management

```
getMoney() → pack.find("Marks").getCount()
addMoney(n) → pack.find("Marks").add(n)
subMoney(n) → pack.find("Marks").sub(n)

getWounds() → temp.find("Wounds") count  (or 0)
addWounds(n) → temp.fix("Wounds", current + n)
subWounds(n) → temp.fix("Wounds", max(0, current - n))

disease() → stat.find("Disease") count
ail(n) → stat.fix("Disease", current + n)
```

---

## 8. Buffer (Parsing Utility)

The `Buffer` class parses the pipe-delimited serialization format.

```
Fields:
  data: string       // Raw text
  index: int         // Current position
  mark: int          // Saved position
  size: int          // Total length

Methods:
  getToken() → string    // Extract text until delimiter ({ | } )
  num() → int            // Parse integer from next token
  line() → string        // Extract until newline
  split() → boolean      // Check and consume '|'
  begin() → boolean      // Check and consume '{'
  end() → boolean        // Check and consume '}'
  match(str) → boolean   // Lookahead for string match
  startsWith(str) → boolean
```

---

## 9. MadLib (Template Engine)

Text template system for dynamic game text.

### Variable Format
Variables are enclosed in `$`: `$VARIABLE_NAME$`

### Built-in Variables

| Variable | Male Value | Female Value |
|----------|-----------|--------------|
| `$HE$` | he | she |
| `$HIM$` | him | her |
| `$HIS$` | his | her |
| `$MAN$` | man | woman |
| `$BOY$` | boy | girl |
| `$CR$` | `\n` | `\n` |
| `$TB$` | `\t` | `\t` |
| `$$` | `$` | `$` |

### Methods
```
replace(key: string, value: string) → void   // Set variable
genderize(isMale: boolean) → void             // Apply gender set
capitalize() → void                            // Cap after "  " (double space)
getText() → string                             // Render template
clone() → MadLib                               // Deep copy
```

### Text Parsing in itText
```
parseText():
    1. Scan for list references (items named in $...$)
    2. For each reference:
       - Look up in monster's values/pack lists
       - If itList found: pick random element from list
       - Replace variable with selected value
    3. Apply standard MadLib substitutions
    4. Return rendered text
```

---

## 10. Item Factory

Items are constructed from serialized text via the `Item.factory()` method:

```
Item.factory(Buffer buf):
    if buf.begin():                          // Starts with '{'
        token = buf.getToken()
        buf.split()                          // Consume '|'
        
        switch(token):
            '#'       → new itCount(buf)
            '@'       → new itRandom(buf)
            '%'       → new itPercent(buf)
            '='       → new itValue(buf)
            '~'       → new itList(buf)
            'itArms'  → new itArms(buf)
            'itText'  → new itText(buf)
            'itNote'  → new itNote(buf)
            'itHero'  → new itHero(buf)
            'itMonster' → new itMonster(buf)
            default   → new itToken(token)
        
        buf.end()                            // Consume '}'
        return constructed item
    else:
        token = buf.getToken()
        return new itToken(token)
```

---

## 11. Database Schema (For Recreation)

When using SQLite/Postgres instead of file-based storage:

```sql
CREATE TABLE heroes (
    id          INTEGER PRIMARY KEY,
    name        TEXT UNIQUE NOT NULL,
    pass_hash   TEXT NOT NULL,
    data        TEXT NOT NULL,           -- Serialized hero (pipe format)
    updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE rankings (
    id          INTEGER PRIMARY KEY,
    hero_name   TEXT NOT NULL,
    rank_type   INTEGER NOT NULL,        -- 0=Fame, 1=Skill, 2=Rank, 3=Guild, 4=Clan
    score       INTEGER NOT NULL,
    rank_data   TEXT NOT NULL,            -- Serialized rank string
    updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(hero_name, rank_type)
);

CREATE TABLE mail (
    id          INTEGER PRIMARY KEY,
    from_name   TEXT NOT NULL,
    to_name     TEXT NOT NULL,
    items       TEXT NOT NULL,            -- Serialized item data
    sent_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    read        BOOLEAN DEFAULT FALSE
);

CREATE TABLE clans (
    id          INTEGER PRIMARY KEY,
    name        TEXT UNIQUE NOT NULL,
    leader      TEXT NOT NULL,
    data        TEXT NOT NULL,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```
