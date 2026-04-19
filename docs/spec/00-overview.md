# Dragon Court — Technical Specification Overview

**Version:** 1.0  
**Original Game:** Dragon Court by Fred Haslam (Fred's Friends, Inc., 1998)  
**Source:** Decompiled Java Applet (RustyDagger project)  
**Target:** Faithful WebAssembly recreation with multi-platform deployment

---

## Table of Contents (Specification Document Set)

| Document | Title | Description |
|----------|-------|-------------|
| [00-overview.md](00-overview.md) | Overview | This document — game summary, goals, deployment targets |
| [01-architecture.md](01-architecture.md) | Architecture | System architecture, WASM runtime, backend, storage |
| [02-data-model.md](02-data-model.md) | Data Model | Entity system, serialization format, item factory |
| [03-character-system.md](03-character-system.md) | Character System | Hero creation, stats, traits, guilds, leveling |
| [04-combat-system.md](04-combat-system.md) | Combat System | Battle mechanics, formulas, initiative, damage |
| [05-item-system.md](05-item-system.md) | Item System | Equipment, consumables, full gear/arms tables |
| [06-monster-system.md](06-monster-system.md) | Monster System | Monster definitions, AI, balancing, loot |
| [07-world-navigation.md](07-world-navigation.md) | World & Navigation | Locations, screen flow, map structure |
| [08-quest-system.md](08-quest-system.md) | Quest System | Encounters, interaction options, outcomes |
| [09-economy.md](09-economy.md) | Economy | Shops, pricing, trading, investments |
| [10-ui-specification.md](10-ui-specification.md) | UI Specification | Screen layouts, components, rendering |
| [11-progression.md](11-progression.md) | Progression | Leveling, ranks, fame, daily cycle, fatigue |
| [12-multiplayer.md](12-multiplayer.md) | Multiplayer | Mail, clans, rankings, peer viewing |
| [13-persistence.md](13-persistence.md) | Persistence & Security | Save/load, auth, encryption, storage |
| [14-assets.md](14-assets.md) | Assets | Images, audio, asset pipeline |
| [15-content-tables.md](15-content-tables.md) | Content Tables | All static game data — rumors, riddles, strings |

---

## 1. Game Summary

Dragon Court is a single-player (with optional multiplayer) browser-based RPG originally built as a Java Applet in 1998. The player creates a hero, explores wilderness areas, battles monsters in turn-based combat, trades with NPCs, and advances through a social ranking system.

### Core Gameplay Loop

```
1. Hero awakens at a location (camp, inn, suite)
   → Daily bonuses/penalties applied (decay, disease, fatigue, stipend)

2. Hero navigates between areas:
   → Town (shops, tavern, lodging)
   → Fields → Forest → Hills (progressively harder wilderness)
   → Goblin Mound (dungeon with tiered access via maps)
   → Castle (court, dungeons, docks, clan hall)

3. Questing in wilderness:
   → Weighted random encounter selection
   → Social/skill interaction options (Bribe, Feed, Riddle, Trade, Help, Seduce)
   → Combat options (Attack, Backstab, Berzerk, Control, Swindle, Ieatsu)
   → Loot, experience, stat gains on success

4. Session ends:
   → Hero sleeps/exits → stats saved → session report shown
   → Equipment decays overnight → daily advancement applied
```

### Genre & Tone

- **Genre:** Turn-based RPG with roguelike elements
- **Setting:** Medieval fantasy with humor (goblin dialect, absurd riddles, sardonic NPCs)
- **Tone:** Light-hearted, accessible, browser-game pacing
- **Art Style:** Hand-drawn character portraits, scenic location thumbnails
- **Audio:** MIDI background music (4 tracks)

---

## 2. Design Goals for Recreation

### 2.1 Faithful Recreation

Every game mechanic, formula, combat calculation, stat progression, item table, monster definition, and UI flow from the original Java source must be preserved exactly. This specification captures all such details from the decompiled source.

### 2.2 Multi-Platform Deployment

| Platform | Runtime | Storage | Notes |
|----------|---------|---------|-------|
| **Web Browser** | WebAssembly | REST API to backend server | Primary target |
| **Mobile (PWA/Native)** | WASM in WebView | Local SQLite database | Offline-capable |
| **Desktop Container** | WASM + local server | SQLite / Postgres | Docker or native |
| **Self-Hosted Server** | WASM client + backend | Postgres / SQLite | Full multiplayer |

### 2.3 Architecture Principles

- **Game logic in WASM:** All game rules, combat formulas, and progression logic compile to WebAssembly for bit-exact parity across all platforms
- **Thin rendering layer:** Platform-specific rendering (Canvas2D / WebGL) calls into the WASM game core
- **Pluggable persistence:** Storage layer abstracted behind an interface (REST API, SQLite, or in-memory)
- **Optional multiplayer:** Single-player works fully offline; multiplayer features activate when a backend is available

### 2.4 Scope

| Feature | Status |
|---------|--------|
| All original game mechanics | **In scope** — faithfully recreated |
| All original content (monsters, items, strings) | **In scope** — exact reproduction |
| Original art assets | **In scope** — reused, with upgrade path |
| Single-player mode | **In scope** — core mode |
| Multiplayer mode (mail, clans, rankings) | **In scope** — opt-in when backend available |
| Queen's Court: Flirt activity | **Stub** — documented as unimplemented |
| Queen's Court: Study activity | **Stub** — documented as unimplemented |
| New features beyond original | **Out of scope** |

---

## 3. Original Game Technical Profile

| Aspect | Original Implementation |
|--------|------------------------|
| **Language** | Java 1.1 (AWT applet) |
| **Entry Point** | `DCourtApplet.java` (browser) / `DCourtFrame.java` (standalone) |
| **Window Size** | 400×300 pixels (applet), 500×400 (HTML embed) |
| **UI Framework** | Java AWT with custom components |
| **Layout** | Manual positioning (null layout manager) |
| **Rendering** | `Graphics.drawString()`, `Graphics.fillRect()`, custom 3D borders |
| **Network** | HTTP POST to CGI endpoint (XOR "encryption") |
| **Persistence** | Server-side hero files; local file fallback |
| **Audio** | MIDI playback (4 tracks: DC1-DC4.mid) |
| **Images** | 95 JPG/GIF files (portraits, locations, UI) |
| **RNG** | `java.util.Random` with deterministic seeding |

---

## 4. Key Game Systems Reference

### Stats & Attributes
- **Primary:** Guts (HP/strength), Wits (intelligence/speed), Charm (social/charisma)
- **Combat Derived:** Attack, Defend, Skill (calculated from primary + gear)
- **Progression:** Level, Experience, Fame, Social Rank (0-10)
- **Status:** Wounds, Fatigue, Disease
- **Guilds:** Fighter, Mage, Thief, Ieatsu (Samurai)

### 33 Character Traits
Noble, Wizard, Warrior, Trader, Merchant, Ranger, Gypsy, Fighter, Mage, Thief, Ieatsu, Quick, Mystic, Berzerk, Fencer, Alert, Hardy, Hillfolk, Catseyes, Reflex, Popular, Stubborn, Clever, Unaging, Dragon, plus combat states

### 10 Game Locations
Suite, Room, Floor, Cot, Town, Fields, Forest, Hills, Mound, Docks, Dungeon

### Combat Resolution
Turn-based with initiative, 17 action types, weapon traits, spell effects, monster AI with stance system (passive → aggressive)

---

## 5. Naming Conventions

| Original Java | Spec Reference | Notes |
|---------------|----------------|-------|
| `itHero` | Hero | Player character entity |
| `itMonster` | Monster | Enemy entity |
| `itAgent` | Agent | Abstract base for Hero/Monster |
| `itArms` | Equipment / Arms | Wearable gear with combat stats |
| `itList` | ItemList | Container for items |
| `itCount` | CountItem | Stackable numbered item |
| `itValue` | ValueItem | Key-value storage item |
| `itToken` | Token | Simple named item |
| `itText` | TextItem | Templated text block |
| `itNote` | Note | In-game mail/message |
| `arBattle` | BattleScreen | Combat resolution |
| `arQuest` | QuestScreen | Encounter/negotiation |
| `Screen` | GameScreen | Base screen class |

---

## 6. Document Reading Order

For implementation, read in this order:

1. **Architecture** (01) — Understand the system boundaries
2. **Data Model** (02) — Core entity/item framework everything builds on
3. **Character System** (03) — Hero structure and rules
4. **Item System** (05) — Equipment and consumables
5. **Monster System** (06) — Enemy definitions and AI
6. **Combat System** (04) — Battle resolution
7. **Quest System** (08) — Encounter flow and options
8. **World & Navigation** (07) — Screen flow and locations
9. **Economy** (09) — Shops and trading
10. **Progression** (11) — Leveling, ranks, daily cycle
11. **UI Specification** (10) — Rendering details
12. **Multiplayer** (12) — Optional online features
13. **Persistence** (13) — Save/load and security
14. **Assets** (14) — Image/audio pipeline
15. **Content Tables** (15) — All static data
