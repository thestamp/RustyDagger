# 10 — UI Specification

## 1. Viewport

- **Original:** 400×300 pixels (Java AWT Applet)
- **Target:** Scale-independent; preserve 4:3 aspect ratio; render to Canvas2D
- **Constants:** `DEFAULT_WIDTH = 400`, `DEFAULT_HEIGHT = 300`

---

## 2. Rendering System

### 2.1 Double Buffering

All screens use offscreen rendering:
```
paint(Graphics):
    offscreen = createImage(width, height)
    offGfx = offscreen.getGraphics()
    // Draw to offGfx
    g.drawImage(offscreen, 0, 0)
```

### 2.2 Font System

9 predefined fonts from `Tools.java`:

| ID | Name | Style | Size | Usage |
|----|------|-------|------|-------|
| 0 | courtF | Dialog/Plain | 11 | Default screen font |
| 1 | statusF | Dialog/Bold | 11 | Status displays |
| 2 | giantF | Dialog/Bold | 36 | Large titles |
| 3 | hugeF | Dialog/Bold | 24 | Section headers |
| 4 | bigF | Dialog/Bold | 14 | Subtitles |
| 5 | titleF | Dialog/Bold | 12 | Labels |
| 6 | inputF | Dialog/Plain | 12 | Text inputs |
| 7 | littleF | Dialog/Plain | 9 | Fine print |
| 8 | tinyF | Dialog/Plain | 8 | Smallest text |

### 2.3 Color Palette

**Screen backgrounds:**

| Screen | Color |
|--------|-------|
| Town | `(0, 255, 255)` — cyan |
| Fields | `(255, 128, 128)` — salmon |
| Forest | `(0, 128, 0)` — dark green |
| Hills | `(160, 160, 160)` — gray |
| Castle | `(255, 128, 255)` — pink |
| Queen | `(255, 128, 128)` — salmon |
| Quest | `(255, 255, 0)` — yellow |
| Battle | `(192, 0, 0)` — dark red |
| Indoors | `(128, 255, 129)` — light green |
| Status | `(192, 64, 0)` — orange-brown |
| Entry | `(0, 128, 0)` — dark green |
| Notice | Black |

**Component colors:**

| Element | Color |
|---------|-------|
| Shop table | `(64, 255, 192)` — teal |
| Scrollbar bg | `(192, 192, 192)` — light gray |
| Text selection | `(0, 0, 128)` — navy |

### 2.4 3D Border Drawing

`DrawTools.pointed3DRect(g, x, y, w, h, raised)`:
- Raised: top-left highlight with `brighter()`, bottom-right shadow with `darker()`
- Sunken: inverted

---

## 3. Component Library

### 3.1 Portrait (`Components.Portrait`)

Interactive clickable image with 3D border.

| Property | Type | Description |
|----------|------|-------------|
| name | String | Display label (drawn below image) |
| image | Image | JPG/GIF loaded from Images/ |
| position | (x, y) | Screen coordinates |
| size | (w, h) | Typically 96×64 or 64×96 |
| visible | boolean | Hidden until discovered/unlocked |

Behavior: Click → triggers `action(name)` on parent screen.

### 3.2 FTextList (`Components.FTextList`)

Scrollable text list with selection support.

| Property | Description |
|----------|-------------|
| items | String array |
| selected | Currently selected index |
| scrollOffset | Vertical scroll position |
| font | Typically `littleF` or `courtF` |

Renders items vertically with highlight on selected row.

### 3.3 FTextField (`Components.FTextField`)

Single-line text input.

| Property | Description |
|----------|-------------|
| text | Current value |
| maxLength | Character limit |

### 3.4 FTextArea (`Components.FTextArea`)

Multi-line text area with word wrap.

| Property | Description |
|----------|-------------|
| text | Content |
| rows/cols | Dimensions |
| editable | Read/write flag |
| wrapWidth | Pixel width for word wrap |

### 3.5 FScrollbar (`Components.FScrollbar`)

Horizontal scrollbar for list navigation.

| Property | Description |
|----------|-------------|
| min/max | Range |
| value | Current position |
| orientation | Horizontal (used in Transfer screens) |

### 3.6 FTools

Static layout helpers:
- `pointed3DRect()` — 3D borders
- Utility paint methods shared across components

---

## 4. Screen Layouts

### 4.1 Login / Entry (`arEntry`)

**Background:** `Color(0, 128, 0)` with `Splash.jpg` background image

| Element | Position | Size |
|---------|----------|------|
| Name field | (100, 230) | 120×22 |
| Password field | (100, 260) | 120×22 |
| Lists button | (340, 232) | 55×20 |
| Credits button | (340, 262) | 55×20 |
| Enter portrait | (235, 215) | 96×64 |

### 4.2 Character Creation (`arCreate`)

**Build points:** 20 total  
**Base stats:** Guts=4, Wits=4, Charm=4, Money=1 ($25/point)

Layout: Stat sliders with +/- buttons, trait checkboxes.  
Enter button only visible when `build == 0`.

### 4.3 Character Description (`arBuild`)

| Element | Description |
|---------|-------------|
| Race field | Dropdown/text |
| Build field | Body type |
| Sign field | Zodiac/totem |
| Skin/Eyes/Hair | Color pickers |
| Nervous Habit | Text |
| Distinguishing Marks | Text |
| Catch-phrase | Text |
| Gender/Dress/Behavior | Checkboxes |

**Buttons:** Save(5,275), Done(245,275), Random(328,2)

### 4.4 Hero Status (`arStatus`)

**Background:** `Color(192, 64, 0)`

| Element | Position | Size |
|---------|----------|------|
| Hero portrait | (275, 5) | 120×120 |
| Pack list | (5, 140) | 170×140 |
| Gear display rect | (200, 145) | 200×100 |
| Exp bar | (175, 98) | 90×10 |

**Stats Layout:**

| Stat | X | Y |
|------|---|---|
| Guts | 5 | 54 |
| Wits | 5 | 72 |
| Charm | 5 | 90 |
| Attack | 140 | 54 |
| Defend | 140 | 72 |
| Skill | 140 | 90 |

**Guild line:** y=130  
**Load count:** y=295

**Exp bar rendering:**
```
fillWidth = (barWidth × exp) / raise
Draw white rect (background)
Draw blue rect (fill, width = fillWidth)
```

**Gear Slots:** 5 slots drawn 20px apart with color coding (default/selected/hovered)

**Action Buttons:** Use/Info/Peer/Dump/Oops/Exit at y=250–275 range

### 4.5 Quest Encounter (`arQuest`)

**Background:** `Color(255, 255, 0)`

| Element | Position | Size |
|---------|----------|------|
| Monster portrait | (10, 10) | 160×160 |
| Options panel | (180, 20) | 200×110 |
| Encounter text | Below | Full width |

### 4.6 Battle (`arBattle`)

**Background:** `Color(192, 0, 0)`

| Element | Position | Size |
|---------|----------|------|
| Monster portrait | (10, 10) | 160×160 |
| Hero portrait | (230, 10) | 160×160 |
| Battle text | y=170 | Full width |

### 4.7 Indoors Template

**Background:** `Color(128, 255, 129)`  
**Foreground:** `Color(0, 128, 0)`

| Element | Position | Size |
|---------|----------|------|
| Exit portrait | (320, 10) | 64×32 |
| NPC face | (10, 30) | 144×192 |
| Content area | (160, 30) | 230×250 |

### 4.8 Shop Template

Extends Indoors.

| Element | Position | Size |
|---------|----------|------|
| Item table | (162, 75) | 230×186 |
| Buy checkbox | (165, 28) | — |
| Sell checkbox | (240, 28) | — |
| Info button | (110, 242) | 40×20 |

### 4.9 Trade Template

Extends Indoors.

| Element | Position | Size |
|---------|----------|------|
| Quantity 1K | (160, 50) | 40×20 |
| Quantity 100 | (205, 50) | 40×20 |
| Quantity 10 | (250, 50) | 40×20 |
| Quantity 1 | (295, 50) | 40×20 |

### 4.10 Transfer Template

| Element | Position | Size |
|---------|----------|------|
| Exit button | (340, 5) | 50×20 |
| Transfer button | (5, 28) | 115×20 |
| Pack list | (5, 70) | 190×190 |
| Storage list | (205, 70) | 190×190 |
| Scrollbar | (125, 30) | 270×16 |

### 4.11 Notice Screen (`arNotice`)

**Background:** Black  
**Text color:** White  
**Word wrap:** `Breaker` utility at 380px width  
**Behavior:** Click anywhere → return to home screen

### 4.12 Healer Layout

**Buttons:** (180, 40 + i×30, 180, 25) for each service option (6 services)

### 4.13 Postal Layout

| Element | Position | Size |
|---------|----------|------|
| Take button | (160, 40) | 150×20 |
| Send button | (10, 240) | 140×20 |
| Postbox list | (160, 70) | 230×180 |

### 4.14 Package (Send) Layout

| Element | Position | Size |
|---------|----------|------|
| Send button | (275, 272) | 80×20 |
| "Send To:" name field | (65, 270) | 200×22 |

### 4.15 Scribe Layout

| Element | Position | Size |
|---------|----------|------|
| Text area | (20, 30) | 360×230 |
| Cancel button | (280, 5) | 50×20 |
| Done button | (340, 5) | 50×20 |

### 4.16 Peer Layout

| Element | Position | Size |
|---------|----------|------|
| Seek button | (5, 7) | 60×20 |
| Done button | (335, 7) | 60×20 |
| Hero name field | (70, 5) | 120×22 |

### 4.17 Clan Hall Layout

| Element | Position | Size |
|---------|----------|------|
| Clan name field | (180, 63) | 200×20 |
| Enact button | (170, 160) | 220×25 |
| Join/Quit/Create/Member checkboxes | (200–280, 205) | — |
| Leader: Peer | (175, 175) | 50×20 |
| Leader: Next | (230, 175) | 50×20 |
| Leader: Grant | (285, 175) | 50×20 |
| Leader: Deny | (340, 175) | 50×20 |

### 4.18 Queen's Court Layout

**Activities:** `160 + ((i/2)×100), 50 + ((i%2)×40)`, size 90×25 (6 activities in 2 columns)  
**Petition:** (170, 200, 170, 25)  
**Invest:** (170, 130, 170, 25)

### 4.19 Item Detail (`arDetail`)

**Type labels:** Junk / Map / Camp Gear / Gear / Treasure / Magic / Special / Money

**Arms detail displays:**
- Location slot name
- Attack / Defend / Skill values
- Trait list
- Enchant strength: "Weak" / "Good" / "Strong" (based on `spell vs power/2`)

---

## 5. Text Rendering

### 5.1 Word Wrap (Breaker)

`Breaker.java` handles word-wrapping for notice screens and long text:
- Takes string + pixel width
- Returns array of lines broken at word boundaries
- Used by `arNotice` at 380px wrap width

### 5.2 Static Layout (StaticLayout)

`StaticLayout.java` provides layout management for positioned text blocks. Used for formatted stat displays.

### 5.3 Status Picture (StatusPic)

`StatusPic.java` handles the hero status portrait rendering, loading face images from `Images/Faces/`.

---

## 6. Image Path Conventions

All images loaded relative to `Images/` directory:
- Portraits: `"Images/" + subdir + "/" + name + ".jpg"`
- Alternative: Some use `.gif`
- Face images: `Images/Faces/`
- Area backgrounds: `Images/Fields/`, `Images/Forest/`, etc.
