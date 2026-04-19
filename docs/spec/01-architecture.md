# 01 — System Architecture

## 1. High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    CLIENT (Per Platform)                  │
│  ┌───────────────┐  ┌─────────────┐  ┌───────────────┐  │
│  │  Rendering    │  │  WASM Game  │  │  Platform I/O │  │
│  │  Layer        │←→│  Core       │←→│  Bridge       │  │
│  │  (Canvas2D)   │  │  (Rust/C++) │  │  (JS/Native)  │  │
│  └───────────────┘  └──────┬──────┘  └───────────────┘  │
│                            │                             │
│                    ┌───────┴───────┐                     │
│                    │  Storage      │                     │
│                    │  Interface    │                     │
│                    └───────┬───────┘                     │
└────────────────────────────┼─────────────────────────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
    ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
    │  REST API    │ │  Local       │ │  In-Memory   │
    │  Backend     │ │  SQLite      │ │  (Demo)      │
    │  (Server)    │ │  (Mobile)    │ │              │
    └──────────────┘ └──────────────┘ └──────────────┘
```

## 2. Component Breakdown

### 2.1 WASM Game Core

The game core contains ALL game logic compiled to WebAssembly. This ensures bit-exact gameplay across every platform.

**Responsibilities:**
- Character creation, stat calculation, leveling
- Combat resolution (initiative, damage, effects)
- Monster AI and balancing
- Item management (equipment, consumables, loot)
- Quest encounter logic and option resolution
- RNG system with deterministic seeding
- Economy calculations (shop prices, trading, investments)
- Progression tracking (fame, rank, fatigue, daily advancement)
- Text template engine (MadLib system)
- Serialization/deserialization of game entities

**Does NOT handle:**
- Rendering (delegated to platform layer)
- Network I/O (delegated to storage interface)
- File system access (delegated to platform bridge)
- Audio playback (delegated to platform bridge)

### 2.2 Rendering Layer

Platform-specific rendering that draws the game state provided by the WASM core.

**Original Viewport:** 400×300 pixels (logical resolution)  
**Strategy:** Render at logical resolution, scale to display with aspect ratio preservation.

**Rendering primitives needed:**
- Filled rectangles with color
- 3D raised/sunken borders (triple-line depth)
- Text rendering with font metrics (alignment: left, center, right)
- Image display (scale to bounds, lazy loading)
- Scrollbar widgets (vertical/horizontal)
- Text input fields
- Clickable regions / button areas
- Checkbox groups

### 2.3 Platform I/O Bridge

Thin platform adapter providing:
- Image loading and caching
- Audio playback (MIDI → modern audio format)
- Clipboard access (for copy/paste in text fields)
- Timer/frame tick
- Input events (mouse click, key press)

### 2.4 Storage Interface

Abstract interface with three implementations:

```
interface StorageBackend {
    // Hero persistence
    findHero(name: string, pass: string) → HeroRecord | null
    saveHero(name: string, data: bytes) → void
    loadHero(name: string) → bytes
    
    // Rankings
    saveScore(name: string, rankData: string) → void
    loadRankings() → RankingData
    
    // Multiplayer (optional)
    sendMail(from: string, to: string, items: bytes) → void
    takeMail(name: string) → MailList
    listMail(name: string) → MailHeaders
    
    // Clans (optional)
    peekClan(name: string) → ClanData
    makeClan(name: string, data: string) → void
    killClan(name: string) → void
    
    // Server messages (optional)
    getMessage() → string | null
}
```

---

## 3. Deployment Configurations

### 3.1 Web Browser (Primary)

```
Browser
  ├── index.html (shell)
  ├── game.wasm (game core)
  ├── game.js (JS glue + rendering)
  ├── assets/ (images, audio)
  └── ↔ REST API server (optional)
```

- **Rendering:** HTML5 Canvas 2D context
- **Storage:** REST API to backend when online; IndexedDB fallback for offline
- **Audio:** Web Audio API (convert MIDI → OGG/MP3 ahead of time, or use a MIDI synth library)
- **Input:** DOM mouse/keyboard events forwarded to WASM

### 3.2 Mobile (PWA or Native WebView)

```
App Shell
  ├── WebView / Native Window
  │   ├── game.wasm
  │   └── game.js (rendering)
  ├── SQLite database (local)
  └── assets/ (bundled)
```

- **Storage:** Local SQLite via platform bridge
- **Offline:** Fully functional without network
- **Sync:** Optional REST API sync when online

### 3.3 Desktop Container (Docker)

```
Docker Container
  ├── Nginx (static files)
  │   ├── game.wasm, game.js, assets
  │   └── index.html
  ├── API Server (Node/Go/Rust)
  │   └── SQLite or Postgres
  └── Exposed port: 8080
```

### 3.4 Self-Hosted Server (Full Multiplayer)

```
Server
  ├── API Server
  │   ├── Hero CRUD
  │   ├── Rankings
  │   ├── Mail system
  │   ├── Clan management
  │   └── Postgres database
  └── Static file server
      ├── game.wasm, game.js
      └── assets/
```

---

## 4. Original Loading Pipeline

The original Java game loads in 7 stages. The recreation must preserve this sequence to ensure data dependencies are met:

```
Stage 0: Load splash image ("Splash.jpg")
Stage 1: Load hero portrait ("Faces/Hero.jpg"), create StatusPic
Stage 2: Initialize PlaceTable (11 locations with metadata)
Stage 3: Initialize GearTable (consumables, maps, gems, scrolls, special items)
Stage 4: Initialize ArmsTable (78 weapons/armor items)
Stage 5: Initialize Player controller
Stage 6: Initialize MonsterTable (56+ monster definitions)
         → Returns "loading complete" flag
```

After loading: Display `arEntry` (login screen) or `arNotice` (error) if piracy check fails.

**Recreation note:** The piracy check (`pirateTest()` — domain matching) should be removed or replaced with a configurable origin allowlist.

---

## 5. Game State Machine

```
                    ┌──────────┐
                    │ LOADING  │
                    └────┬─────┘
                         │
                    ┌────▼─────┐
                    │  ENTRY   │◄──────────────────────────┐
                    └────┬─────┘                           │
                    ┌────▼─────┐                           │
               ┌────│ CREATE?  │                           │
               │    └────┬─────┘                           │
               │    ┌────▼─────┐                           │
               │    │  BUILD?  │                           │
               │    └────┬─────┘                           │
               │         │                                 │
               ▼         ▼                                 │
          ┌─────────────────┐                              │
          │   AREA HUB      │◄──────────┐                  │
          │ (Field/Forest/  │           │                  │
          │  Hills/Mound/   │     ┌─────┴──────┐          │
          │  Castle/Town)   │     │  SHOP /    │          │
          └───────┬─────────┘     │  SERVICE   │          │
                  │               └────────────┘          │
            ┌─────▼──────┐                                │
            │   QUEST    │                                │
            │ (Encounter)│                                │
            └─────┬──────┘                                │
                  │                                       │
         ┌────────┼────────┐                              │
         ▼        ▼        ▼                              │
    ┌────────┐ ┌──────┐ ┌──────┐                          │
    │ BATTLE │ │ WIN  │ │ FLEE │                          │
    └───┬────┘ └──┬───┘ └──┬───┘                          │
        │         │        │                              │
        ▼         ▼        ▼                              │
    ┌─────────────────────────┐                           │
    │      AREA HUB           │                           │
    └───────────┬─────────────┘                           │
                │                                         │
           ┌────▼─────┐     ┌──────────┐    ┌────────┐   │
           │  EXIT    │────▶│ FINISH   │───▶│ ENTRY  │───┘
           │ (Sleep)  │     │ (Report) │    │        │
           └──────────┘     └──────────┘    └────────┘
```

---

## 6. Random Number Generation

The original game uses a **seeded RNG** that produces deterministic sequences for fair gameplay.

### Seed Formula
```
seed = level + exp + money + age + fame + guildRank
```

### RNG Functions (must be bit-exact)

| Function | Formula | Range | Use |
|----------|---------|-------|-----|
| `roll(n)` | `random.nextInt(n)` | `[0, n-1]` | General random |
| `twice(n)` | `roll(n) + roll(n)` | `[0, 2n-2]` | Bell-curve distribution |
| `contest(a, b)` | `roll(a + b) < a` | `boolean` | Opposed skill check |
| `percent(n)` | `roll(100) < n` | `boolean` | Percentage chance |
| `chance(n)` | `roll(n) == 0` | `boolean` | 1-in-n chance |
| `spread(n)` | `min = 5n/7; 1 + min + twice(n - min)` | `[1+min, 1+n+min]` | Value distribution |
| `skew(n)` | Count consecutive `percent(n)` successes | `[0, ∞)` | Exponential rarity |

**Critical:** The `contest()` function is used for ALL opposed checks (combat initiative, bribe attempts, riddle contests, etc.). The recreation must use the exact same formula: `roll(a + b) < a`.

---

## 7. Event System

The original uses Java AWT events. The recreation needs a platform-agnostic event system:

### Input Events
```
MouseDown(x, y)      → Screen.down(x, y) → returns next Screen or null
MouseUp(x, y)        → Component handling
MouseDrag(x, y)      → Scrollbar tracking
KeyPress(key)         → Text field input
ActionEvent(source)   → Button clicks, checkbox toggles
```

### Screen Lifecycle
```
Screen.init()         → Create UI components
Screen.localPaint(g)  → Render screen contents
Screen.down(x, y)     → Handle click, return next screen or null
Screen.action(e, o)   → Handle component events
```

### Screen Transition
```
setRegion(newScreen):
    1. Remove all components from current screen
    2. Set newScreen as active
    3. Call newScreen.init()
    4. Trigger repaint
```

---

## 8. Font System

The original game uses specific fonts at specific sizes. The recreation should use web-safe equivalents:

| Original ID | Java Font | Size | Style | Use | Web Equivalent |
|-------------|-----------|------|-------|-----|----------------|
| `courtF` | TimesRoman | 14 | Plain | General UI | `"Times New Roman", serif` |
| `questF` | TimesRoman | 14 | Plain | Quest text | Same |
| `statusF` | TimesRoman | 12 | Plain | Status bar | Same, 12px |
| `fieldF` | TimesRoman | 14 | Bold | Field labels | Same, bold |
| `fightF` | TimesRoman | 16 | Bold | Battle text | Same, 16px bold |
| `boldF` | TimesRoman | 14 | Bold | Emphasis | Same, bold |
| `textF` | TimesRoman | 14 | Plain | Text fields | Same |
| `bigF` | TimesRoman | 20 | Bold | Titles | Same, 20px bold |
| `giantF` | TimesRoman | 36 | Bold | Dragon Court logo | Same, 36px bold |

The font detection logic tries: TimesRoman → Serif → SansSerif → Helvetica → Dialog.

---

## 9. Color Palette

### System Colors
| Name | RGB | Use |
|------|-----|-----|
| `white` | 255, 255, 255 | Text, backgrounds |
| `fill` | 192, 192, 192 | Default component fill |
| `glow` | 224, 224, 224 | 3D highlight edge |
| `dull` | 128, 128, 128 | 3D shadow edge |
| `dark` | 96, 96, 96 | Deep shadow |
| `black` | 0, 0, 0 | Text, borders |

### Screen Background Colors
| Screen | Background RGB | Foreground RGB |
|--------|---------------|----------------|
| Entry | Green | Black |
| Create | Blue | White |
| Build | Blue | White |
| Town | Cyan | Red |
| Fields | Pink | Dark |
| Forest | Green(128,255,129) | Dark Green |
| Hills | Brown-ish | Dark |
| Mound | Dark | Light |
| Castle | Gray | Dark |
| Queen | Pink(255,128,128) | Dark Red(128,0,0) |
| Battle | Red | White |
| Notice | Black | White |
| Error | Red | White |
| Finish | Pink-ish | Dark |

---

## 10. Threading Model

The original Java applet is single-threaded (AWT event thread). The recreation should follow the same model:

- **Game logic:** Synchronous, runs on main thread
- **Image loading:** Asynchronous (lazy load with placeholder)
- **Network I/O:** Asynchronous with callback (save/load operations)
- **Audio:** Fire-and-forget (play MIDI track, no sync needed)
- **Rendering:** Single-pass repaint on state change

No concurrent game logic is needed. All state mutations happen in response to user input events.
