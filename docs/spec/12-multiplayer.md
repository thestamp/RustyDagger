# 12 — Multiplayer

## 1. Overview

The original Dragon Court is a multi-player browser game with shared server state. Players interact indirectly through rankings, mail, clans, and shared world state — there is no real-time PvP.

---

## 2. Communication Architecture (Original)

### 2.1 CGI Backend

The original applet communicated with a CGI server via HTTP:

```
Loader.java:
    cgiRequest(action, data):
        URL = serverBase + "/cgi-bin/dragon.cgi"
        POST: action=<action>&data=<data>
        Returns: pipe-delimited response
```

### 2.2 Actions

| Action | Purpose | Data |
|--------|---------|------|
| hero | Load hero | name + password |
| save | Save hero | serialized hero data |
| list | Get hero list | — |
| rank | Get rankings | rank type |
| mail | Get mailbox | hero name |
| send | Send mail | recipient + package |
| note | Send note | recipient + text |
| clan | Clan operations | sub-action + data |

### 2.3 Target Implementation

Replace CGI with:

| Deployment | Backend | Protocol |
|-----------|---------|----------|
| Web (multi) | REST API server | HTTPS + JWT |
| Container | REST API in container | HTTPS + JWT |
| Mobile (offline) | Local SQLite | Direct DB access |
| Mobile (online) | REST API | HTTPS + JWT |

---

## 3. Mail System

### 3.1 Postal Office

**Location:** Dragon Keep (Castle)  
**Class:** `arPostal`

**Take Mail:** $100 per retrieval  
**Send Package:** stashCount × $100 per item

### 3.2 Mail Types

| Type | Content | Sender |
|------|---------|--------|
| Package | Items from hero pack | Player |
| Note | Text written with Pen & Paper or Postcard | Player |
| System | Notifications | Server |

### 3.3 Note Writing

**Class:** `arScribe`  
Requires: "Pen & Paper" or "Postcard" in pack (consumed on send)  
Max text length: fits in 360×230 text area

### 3.4 Package Sending

**Class:** `arPackage`  
Uses Transfer template to move items from pack to outbox.

```
Process:
    1. Select items from pack → outbox
    2. Enter recipient name
    3. Pay stashCount × $100
    4. Items delivered to recipient's mailbox
```

---

## 4. Clan System

### 4.1 Clan Hall

**Location:** Dragon Keep (Castle)  
**Class:** `arClanHall`  
**Title:** "Servile Krymps Clan Gathering"

### 4.2 Clan Actions

| Action | Cost (Gold) | Cost (Quests) | Requirements |
|--------|------------|---------------|-------------|
| Create Clan | $250,000 | 75 | social ≥ 2 (Baron+) |
| Join Clan (Petition) | $1,000 | 1 | Not in a clan |
| Quit Clan | $5,000 | 5 | Currently in a clan |
| Disband Clan | $50,000 | 15 | Must be clan leader |

### 4.3 Clan Membership

- **Leader:** Can Grant/Deny join petitions, Peer clan members
- **Member:** Can view clan rankings, use clan peer (free)
- **Petitioner:** Waiting for leader approval

### 4.4 Leader Interface

| Button | Position | Size | Action |
|--------|----------|------|--------|
| Peer | (175, 175) | 50×20 | View petitioner details |
| Next | (230, 175) | 50×20 | Cycle to next petitioner |
| Grant | (285, 175) | 50×20 | Accept petitioner |
| Deny | (340, 175) | 50×20 | Reject petitioner |

---

## 5. Rankings

### 5.1 Ranking Types

| Type | Sort Criteria |
|------|--------------|
| Fame | Current fame value |
| Skill | Total combat stats |
| Rank | Social rank + level |
| Guild | Guild rank total |
| Clan | Clan standing |

### 5.2 Ranking Screen

**Class:** `arRanking`

Rankings retrieved from server via `rank` CGI action. Displayed as scrollable list.

### 5.3 Ranking Data

Each entry contains:
- Hero name
- Relevant stat value
- Rank position

---

## 6. Peer System

### 6.1 Viewing Other Heroes

The Peer system allows viewing another hero's description and stats.

**Access Methods:**

| Source | Cost | Class |
|--------|------|-------|
| Gem Shop | $250 | arPeer (USEMONEY) |
| Magic (3 Opals) | 3 Opals | arPeer (USEMAGIC) |
| Palantir item | Free | arPeer (USEPALANTIR) |
| Clan leader | Free | arPeer (CLANPEER) |
| Self | Free | arPeer (self) |

### 6.2 Peer Display

Generates a MadLib-formatted description using hero traits:
- Race, Build, Sign
- Skin, Eyes, Hair
- Dress, Behavior
- Distinguishing Marks
- Catch-phrase
- Stats summary

---

## 7. Session Management (Original)

### 7.1 Authentication

Original system used XOR "encryption" with session IDs:

```
alterSessionID(hero):
    seed = level + exp + money + age + fame + guildRank
    // Used to obfuscate save data
```

**This must be replaced with proper authentication** — see [13-persistence.md](13-persistence.md).

### 7.2 Concurrency Model

- No real-time interaction between players
- All interactions are asynchronous (mail, clan petitions)
- Rankings update on each hero save
- No locks needed — each hero's data is independent

---

## 8. Target Multi-player Architecture

### 8.1 REST API Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/auth/login` | POST | Authenticate, return JWT |
| `/api/auth/register` | POST | Create account |
| `/api/hero/load` | GET | Load hero data |
| `/api/hero/save` | POST | Save hero data |
| `/api/hero/list` | GET | List all heroes |
| `/api/rankings/{type}` | GET | Get rankings by type |
| `/api/mail/inbox` | GET | Get mailbox |
| `/api/mail/send` | POST | Send mail/package |
| `/api/clan/list` | GET | List clans |
| `/api/clan/create` | POST | Create clan |
| `/api/clan/join` | POST | Petition to join |
| `/api/clan/manage` | POST | Grant/deny/disband |
| `/api/peer/{name}` | GET | View hero description |

### 8.2 Anti-Cheat Considerations

Since the original game runs all logic client-side, the target implementation should:

1. **Validate saves server-side** — check stat consistency, exp vs level, money vs purchases
2. **Rate-limit quest completions** — enforce daily quest cap
3. **Validate item existence** — items can only come from known sources
4. **Sign save data** — prevent tampering with serialized hero data

### 8.3 Offline Mode

For mobile/offline play:
- Local SQLite stores hero data, no multiplayer features
- Sync conflict resolution when going online: server state always wins
- Mail/clan features disabled in offline mode
