# 09 — Economy

## 1. Shop Framework

All shops extend `Shop.java`, which provides buy/sell mechanics with a shared pricing model.

### 1.1 Layout

| Element | Position | Size |
|---------|----------|------|
| Table (item list) | (162, 75) | 230×186 |
| Buy checkbox | (165, 28) | — |
| Sell checkbox | (240, 28) | — |
| Info button | (110, 242) | 40×20 |

**Table background:** `Color(64, 255, 192)`

### 1.2 Sell Price Formula

```
packValue(item):
    cost = stockValue(item)
    if hasTrait(MERCHANT):
        cost2 = cost × RESALE / 95
    else:
        cost2 = cost × RESALE / 100
    return cost2 - (cost2 × BASE / (2 × BASE + heroCharm))
```

Where `RESALE` and `BASE` are shop-specific constants.

**Key Insight:** Higher charm → better sell prices (reduces the penalty divisor). MERCHANT trait gives ~5% bonus.

### 1.3 Buy Price

Buy prices use the item's `stockValue()` directly (no markup formula — already includes shop-specific multipliers).

---

## 2. Town Shops

### 2.1 Bill Smith's Weapon Shoppe

**Class:** `arWeapon`  
**RESALE:** 60  
**BASE:** 10  

**Stock:**

| Item | Stock Value |
|------|------------|
| Knife | 5 |
| Hatchet | 10 |
| Short Sword | 30 |
| Long Sword | 100 |
| Spear | 15 |
| Broad Sword | 200 |
| Battle Axe | 150 |
| Pike | 75 |
| Sling | 5 |
| Short Bow | 25 |
| Long Bow | 80 |
| Spike Helm | 100 |
| Main Gauche | 50 |

**Special Service:** "Identify" — costs 40 marks  
**Stock Value Modifier:** `arm.stockValue() × 1.3` for RIGHT-hand slot items, minimum 2

### 2.2 Aileen Suitor's Armour Shoppe

**Class:** `arArmour`  
**RESALE:** 50  
**BASE:** 15  
**MAXFIX_POWER:** 60  

**Stock:**

| Item | Category |
|------|----------|
| Clothes | Body |
| Leather Jacket | Body |
| Brigandine | Body |
| Chain Suit | Body |
| Scale Suit | Body |
| Buckler | Left |
| Targe | Left |
| Shield | Left |
| Spike Shield | Left |
| Sandals | Feet |
| Shoes | Feet |
| Boots | Feet |
| Leather Cap | Head |
| Pot Helm | Head |
| Chain Coif | Head |

**Special Service:** "Polish" — repairs decay damage

```
Polish Cost:
    base = 1 per decay point
    if base_power < MAXFIX_POWER (60):
        + (baseAttack - currentAttack)² × 5
        + (baseDefend - currentDefend)² × 4
        + (baseSkill  - currentSkill)²  × 2
```

**Stock Value Modifier:** `arm.stockValue() × 1.3` for BODY slot items, minimum 2

### 2.3 Sally Trader's Curious Goods

**Class:** `arTrader`  
**RESALE:** 80  
**BASE:** 15  

Uses `Trade` template with quantity buttons.

**Stock:**

| Item | Price |
|------|-------|
| Food | 1 |
| Fish | 2 |
| Torch | 3 |
| Rope | 5 |
| Pen & Paper | 10 |
| Sleeping Bag | 25 |
| Cooking Gear | 25 |
| Camp Tent | 50 |
| Identify Scroll | 60 |
| Healing Salve | 40 |
| Seltzer Water | 30 |
| Panic Dust | 20 |
| Blinding Dust | 25 |
| Blast Powder | 30 |
| Castle Permit | 1000 |

**Quantity Buttons:** 1/10/100/1K at x-positions 295/250/205/160, y=50, size 40×20

---

## 3. Forest Shops

### 3.1 Gareth Shortleg's Forest Smithy

**Class:** `arDwfSmith`  
**RESALE:** 50  
**BASE:** 20  

**Stock:**

| Item | Category |
|------|----------|
| Steel Sword | Right |
| Bill Hook | Right |
| Sword Breaker | Left |
| Shakrum | Right/Thrown |
| Recurve Bow | Right/Ranged |
| Half Plate | Body |
| Full Plate | Body |
| Steel Buckler | Left |
| Roman Helm | Head |
| Doc Martins | Feet |
| Mercury Sandals | Feet |

**Special Service:** "Identify" — costs 60 marks  
**Stock Value Modifier:** `arm.stockValue() × 1.3` for LEFT slot items, minimum 2

---

## 4. Hills Shops

### 4.1 Gakthrak Cunning's Priceless Gems

**Class:** `arGemShop`  
**RESALE:** 70  
**BASE:** 30  

**Stock:**

| Gem | Base Value |
|-----|-----------|
| Quartz | 50 |
| Opal | 150 |
| Garnet | 500 |
| Emerald | 2,000 |
| Ruby | 5,000 |
| Turquoise | 500 |

Also buys items from `GearTable.findList("Buy", 4)`.

**Special Service:** "Peer" — costs 250 marks, opens arPeer screen

### 4.2 Djinni's Ethereal Magic Shop

**Class:** `arMagicShop`  
**RESALE:** 55  
**BASE:** 22  

**Stock:**

| Item | Type |
|------|------|
| Identify Scroll | Scroll |
| Glow Scroll | Scroll |
| Healing Salve | Potion |
| Seltzer Water | Potion |
| Panic Dust | Dust |
| Gold Apple | Special |
| Blinding Dust | Dust |
| Bless Scroll | Scroll |
| Luck Scroll | Scroll |
| Enchant Scroll | Scroll |
| Flame Scroll | Scroll |
| Faceless Potion | Special |

Also buys from `GearTable.findList("Buy", 6)` merged with `findList("", 7)`.

**Known Bug:** `stockValue()` calls itself recursively instead of `super.stockValue()`, causing `StackOverflowError`. Must be fixed in implementation.

---

## 5. Mound Shop

### 5.1 Smidgeon Crumb's Gobble Inn

**Class:** `arGoblin`  
Dual-mode: Inn + Shop

**Inn Services:**

| Service | Cost | Effect |
|---------|------|--------|
| Sleep on Floor | Free | Save at MOUND location |
| Buy a Drink | $10 | Goblin rumors (costs 1 quest) |
| Rent Smelly Cot | $75 + 25×level | Save at COT location |

HOTEL trait → cot cost ÷ 10

**Shop Stock:**

| Item |
|------|
| Gobble Inn Postcard |
| T-Shirt |
| Identify Scroll |
| Healing Salve |
| Seltzer Water |
| Map to Warrens |
| Thief Insurance |
| Map to Treasury |

---

## 6. Tavern Economy

### 6.1 Silas Keeper's Bed & Breakfast

**Class:** `arTavern`

| Service | Cost | Effect |
|---------|------|--------|
| Buy a Drink | $1 | Rumor + 1 quest fatigue |
| Sleep on Floor | $4 + level | Save at FLOOR |
| Rent a Room | $20 + 5×level | Save at ROOM |
| Rent a Suite | $75 + 25×level | Save at SUITE |
| Storage | level × $50 | Opens arStorage |

HOTEL trait → all lodging costs ÷ 10

### 6.2 Rumor Mechanic

```
buyDrink():
    hero.quests -= 1
    switch hero.charm:
        0:      "mickey" — lose money, +1 fatigue
        1-2:    "pass out" — +1 fatigue
        3-9:    "nothing interesting"
        10+:    random rumor from Rumors table + gainCharm(2)
```

Goblin Inn uses same mechanic but pulls from GRumors table. Charm 0 in Goblin Inn = pack cleared.

---

## 7. Healing Services

### 7.1 Elden Bishop's Temple of Brotherly Sharing

**Class:** `arHealer`

**Cost Level:** `max(1, heroLevel - social - 1)`

| Service | Cost Formula | Effect |
|---------|-------------|--------|
| Minor Heal | `(wounds/4) × costLevel` | Heal 25% wounds |
| Half Heal | `(wounds/2) × costLevel` | Heal 50% wounds |
| Full Heal | `wounds × costLevel` | Heal 100% wounds |
| Mercy | Free (or 1 at level 1) | Minimal heal |
| Cure Disease | `10 × costLevel` | Remove Disease/Blind/Panic |
| Tithe | `(cash + 9) / 10` | Donate 10% gold → gain exp |

**Tithe Experience:** `learn(tithe / level²)`, capped at raise threshold

**Button Layout:** (180, 40 + i×30, 180, 25)

---

## 8. Queen's Court Economy

### 8.1 Court Activities (each costs 1 quest/fatigue)

**MAXIMUM_RANK:** 9  
**PETITION_COST:** $5,000  
**PETITION_QUEST:** 3  
**INVEST_COST:** $100,000  
**INVEST_QUEST:** 5  

### 8.2 Court Games

All use `fourTest(skill, difficulty)` → 5 outcomes (0 worst, 4 best):

| Activity | Skill | Difficulty | Stakes |
|----------|-------|-----------|--------|
| Dice | wits + thief×5 (if swap) | level×8 + thief×5 | Bet = $2,500×(rank+1), cap=money |
| Mingle | charm + magic×5 | level × 5 | Favor change |
| Boast | charm + fight×5 | level × 10 | Favor change |
| Game | guts | (1+rank) × 20 | Risk of wounds |

**fourTest Resolution:**

```
fourTest(skill, difficulty):
    roll1 = contest(skill, difficulty)
    roll2 = contest(skill, difficulty)
    return roll1 + roll2  // Range: 0-4 (but practically 0-2 per roll)
    // Actually returns 0..4 mapped from two boolean contests
```

### 8.3 Petition for Social Promotion

```
index = (favor / divisor) × 4 / rankCost[rank]
divisor = 700 if hasTrait(POPULAR), else 1000

if index >= 4:
    social += 1
    favor resets to 0
```

Requires PETITION_COST ($5,000) and PETITION_QUEST (3) quests remaining.

### 8.4 Investment

```
risk = (rank + roll(rank + 2)) / 3
reward = INVEST_COST × (23 + risk × 3) / 20

fourTest(wits, 20 + risk × 40):
    outcome 0: return 0              (total loss)
    outcome 1: return INVEST_COST / 2 (partial loss)
    outcome 2: return INVEST_COST     (break even)
    outcome 3: return reward          (profit)
    outcome 4: return reward × 3/2   (windfall)
```

Requires INVEST_COST ($100,000) and INVEST_QUEST (5) quests.

**Button Layout:** Activities at `160 + ((i/2)×100), 50 + ((i%2)×40)`, size 90×25. Petition at (170,200,170,25). Invest at (170,130,170,25).

---

## 9. Guild Training Costs

### 9.1 Free Adventurers Guild

**Class:** `arGuild`  
**Location:** Forest (discovered) or Fields Tower

| Action | Cost | Fatigue | Requirements |
|--------|------|---------|-------------|
| Join | $4,000 | +5 | — |
| Train | guildRank × $1,000 | +5 | member, guildRank < level, quests ≥ 5 |

ILLUMINATI trait → all costs ÷ 2

### 9.2 Training Effects

| Skill | Stat Gains | Stat Losses |
|-------|-----------|-------------|
| Fighter | +1 fight rank, +1 temp fight | -2 wits, -2 charm |
| Magery | +1 magic rank, +1 temp magic | -2 guts, -2 charm |
| Trader (Thief) | +1 thief rank, +1 temp thief | -2 guts, -2 wits |

---

## 10. Mail Economy

### 10.1 Postal Services

**Class:** `arPostal` (at Castle)

| Service | Cost |
|---------|------|
| Take Mail | $100 |
| Send Package | stashCount × $100 per item |

**Layout:** Take button (160,40,150,20), Send button (10,240,140,20), Postbox list (160,70,230,180)

---

## 11. Peer / View Other Heroes

| Source | Cost |
|--------|------|
| Gem Shop (USEMONEY) | $250 |
| Magic (USEMAGIC) | 3 Opals |
| Palantir (USEPALANTIR) | Free |
| Clan Peer (CLANPEER) | Free (leaders only) |
| Self-Peer | Free |

---

## 12. Storage

**Class:** `arStorage`  
**Entry Cost:** level × $50  

Uses Transfer template: Pack list (5,70,190,190), Storage list (205,70,190,190).

Storage capacity is shared between sessions (persisted).

---

## 13. Clan Economy

| Action | Quests Required | Gold Cost |
|--------|----------------|-----------|
| Join (Petition) | 1 | $1,000 |
| Quit Clan | 5 | $5,000 |
| Disband Clan | 15 | $50,000 |
| Create Clan | 75 | $250,000 |

Create requires Baron or higher (social ≥ 2).
