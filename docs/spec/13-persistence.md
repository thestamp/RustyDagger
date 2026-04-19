# 13 — Persistence & Security

## 1. Serialization Format

### 1.1 Entity Serialization

All entities serialize to pipe-delimited strings:

```
{TypePrefix|field1|field2|field3|...}
```

| Type | Prefix | Fields |
|------|--------|--------|
| itText | `T` | text |
| itNote | `N` | name, text |
| itCount | `C` | name, obfuscatedCount |
| itValue | `V` | name, value |
| itPercent | `P` | name, percent |
| itRandom | `R` | name, max |
| itArms | `A` | name, attack, defend, skill, slot, spell, traits... |
| itAgent | `G` | name, guts, wits, charm, baseA, baseD, baseS, picture, text, opts, values, pack, temp, gear |
| itList | `L` | name, items... |

### 1.2 Obfuscated Counts

`itCount` stores values with a random offset:

```
serialize:
    offset = roll(1024) + 1
    stored = value + offset
    output = "{C|name|stored|offset}"

deserialize:
    value = stored - offset
```

### 1.3 Nested Entities

Lists contain nested entities, each wrapped in `{...}`:

```
{L|pack|{C|Food|107|102}|{C|Rope|6|3}|{A|Long Sword|8|2|3|RIGHT|0|}}
```

### 1.4 Hero Serialization

A hero is an `itAgent` containing nested lists:

```
{G|HeroName|50|40|30|5|3|8|Faces/face1.jpg|
    {T|description text}|
    help,backstab,swindle|
    {L|values|{V|social|3}|{V|fame|150}|{V|fight|2}|...}|
    {L|pack|{C|Food|15|10}|...}|
    {L|temp|...}|
    {L|gear|{A|Long Sword|8|2|3|RIGHT|0|}|...}
}
```

---

## 2. Save/Load Flow

### 2.1 Original Flow

```
Save:
    1. Hero data serialized to pipe string
    2. XOR "encrypted" with session key
    3. POSTed to CGI server
    4. Server stores in flat file

Load:
    1. POST name + password to CGI
    2. Server returns XOR'd data
    3. Client decrypts and deserializes
    4. Hero entity reconstructed
```

### 2.2 Target Flow (Multi-player)

```
Save:
    1. Hero data serialized (same pipe format or JSON)
    2. Signed with JWT
    3. PUT /api/hero/save
    4. Server validates + stores in database
    5. Rankings updated

Load:
    1. POST /api/auth/login → JWT
    2. GET /api/hero/load (with JWT)
    3. Server returns hero data
    4. Client deserializes
```

### 2.3 Target Flow (Single-player/Offline)

```
Save:
    1. Hero data serialized
    2. Stored in local SQLite
    3. No network required

Load:
    1. Query local SQLite
    2. Deserialize
```

---

## 3. Security (Original — DO NOT REPLICATE)

### 3.1 XOR "Encryption" (INSECURE)

The original uses XOR with a predictable key derived from hero stats:

```
alterSessionID(hero):
    seed = level + exp + money + age + fame + guildRank
    // Seed used to XOR save data
```

**Vulnerabilities:**
- Key is derived from save data itself (circular)
- XOR is easily reversible
- No authentication beyond name + password
- Password stored/compared in plaintext (assumed)
- No session management

### 3.2 Why It Must Be Replaced

- Any player can read/modify their save data
- No server-side validation
- No protection against replay attacks
- Trivial to forge requests

---

## 4. Security (Target Implementation)

### 4.1 Authentication

| Feature | Implementation |
|---------|---------------|
| Password storage | bcrypt/argon2 hashing |
| Session tokens | JWT with HS256/RS256 |
| Token expiry | 24 hours (configurable) |
| Refresh tokens | Optional, stored server-side |
| Rate limiting | 10 login attempts / minute |

### 4.2 Authorization

| Resource | Rule |
|----------|------|
| Hero data | Owner only (JWT subject match) |
| Rankings | Public read |
| Mail | Recipient only |
| Clan management | Leader only for admin actions |
| Peer viewing | Authenticated + cost check |

### 4.3 Data Integrity

```
Server-side validation on save:
    1. Verify JWT
    2. Decompose hero data
    3. Validate stat ranges (no negative, no overflow)
    4. Validate exp vs level (exp < raise threshold)
    5. Validate items exist in game tables
    6. Validate money consistency (not negative)
    7. Store with server timestamp
    8. Log changes for audit
```

### 4.4 Transport Security

| Deployment | Transport |
|-----------|-----------|
| Web | HTTPS (TLS 1.3) |
| Container | HTTPS (self-signed or Let's Encrypt) |
| Mobile | HTTPS + certificate pinning |
| Offline | N/A (local only) |

### 4.5 OWASP Compliance

| Risk | Mitigation |
|------|-----------|
| Injection | Parameterized queries, input validation |
| Broken Auth | bcrypt, JWT, rate limiting |
| XSS | CSP headers, output encoding |
| CSRF | SameSite cookies, CSRF tokens |
| Insecure Deserialization | Validate before deserialize, schema validation |
| Security Misconfiguration | Hardened defaults, no debug in prod |

---

## 5. Database Schema (Multi-player)

### 5.1 Accounts

```sql
CREATE TABLE accounts (
    id          INTEGER PRIMARY KEY,
    username    TEXT UNIQUE NOT NULL,
    password    TEXT NOT NULL,  -- bcrypt hash
    email       TEXT,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login  TIMESTAMP
);
```

### 5.2 Heroes

```sql
CREATE TABLE heroes (
    id          INTEGER PRIMARY KEY,
    account_id  INTEGER REFERENCES accounts(id),
    name        TEXT UNIQUE NOT NULL,
    level       INTEGER DEFAULT 1,
    data        TEXT NOT NULL,           -- serialized hero
    location    TEXT DEFAULT 'TOWN',
    last_save   TIMESTAMP,
    last_day    DATE,                    -- for new-day detection
    fame        INTEGER DEFAULT 0,
    social      INTEGER DEFAULT 0,
    guild_rank  INTEGER DEFAULT 0,
    clan_id     INTEGER REFERENCES clans(id)
);
```

### 5.3 Rankings

```sql
CREATE TABLE rankings (
    hero_id     INTEGER REFERENCES heroes(id),
    rank_type   TEXT NOT NULL,  -- 'fame','skill','rank','guild','clan'
    value       INTEGER NOT NULL,
    updated_at  TIMESTAMP,
    PRIMARY KEY (hero_id, rank_type)
);
```

### 5.4 Mail

```sql
CREATE TABLE mail (
    id          INTEGER PRIMARY KEY,
    sender      TEXT NOT NULL,
    recipient   TEXT NOT NULL,
    type        TEXT NOT NULL,  -- 'note', 'package'
    content     TEXT NOT NULL,  -- serialized text or items
    sent_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    read        BOOLEAN DEFAULT FALSE
);
```

### 5.5 Clans

```sql
CREATE TABLE clans (
    id          INTEGER PRIMARY KEY,
    name        TEXT UNIQUE NOT NULL,
    leader_id   INTEGER REFERENCES heroes(id),
    created_at  TIMESTAMP
);

CREATE TABLE clan_petitions (
    id          INTEGER PRIMARY KEY,
    clan_id     INTEGER REFERENCES clans(id),
    hero_id     INTEGER REFERENCES heroes(id),
    status      TEXT DEFAULT 'pending',  -- 'pending','granted','denied'
    created_at  TIMESTAMP
);
```

### 5.6 Storage

```sql
CREATE TABLE storage (
    hero_id     INTEGER REFERENCES heroes(id),
    slot        INTEGER NOT NULL,
    item_data   TEXT NOT NULL,  -- serialized item
    PRIMARY KEY (hero_id, slot)
);
```

---

## 6. Offline / Local Schema (SQLite)

Same schema as above but single-user:
- No accounts table (implicit single user)
- No mail table (offline = no multiplayer)
- No clan tables (offline = no clans)
- Rankings stored locally (personal records only)

---

## 7. Save Data Migration

### 7.1 Original → Target

If importing original save data:

1. Parse pipe-delimited format
2. Decode XOR (using derived key)
3. Validate and sanitize all fields
4. Re-serialize to target format (pipe or JSON)
5. Store with proper authentication

### 7.2 Format Options

| Format | Pros | Cons |
|--------|------|------|
| Pipe-delimited (original) | Compact, faithful | Hard to read/debug |
| JSON | Readable, standard | Slightly larger |
| Binary (MessagePack) | Smallest, fast | Opaque |

**Recommendation:** JSON for storage, pipe-delimited as internal wire format for compatibility testing.
