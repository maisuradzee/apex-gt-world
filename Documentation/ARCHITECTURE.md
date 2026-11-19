# APEX GT WORLD — Technical Architecture V2

## 1. Product architecture
The delivery order is offline-first: build a complete local single-player driving loop before enabling remote accounts, shared-world networking or online competition. The long-term product still has one player identity spanning driving, motorcycle racing, open-world travel, roleplay, jobs, properties, businesses, clubs and competitive motorsport. The game is designed as one connected world with several independently streamed regions.

## 2. Runtime boundaries

### Offline local runtime (first playable milestone)
- Unity boots directly into a local player profile; no authentication or network request is required.
- `OfflinePlayerProfile` stores local credits, career XP/level, vehicle ownership/selection, unlocked regions, completed events and distance.
- `OfflineSaveStore` writes a versioned JSON save in `Application.persistentDataPath`, maintains a recovery backup and restores the backup if the primary save is invalid.
- `OfflineGameManager` owns local progression, periodic saves, pause/quit saves and local event idempotency.
- Local save data is editable by the device owner and is not authoritative for online rewards, leaderboards, purchases or competitive race results.

### Unity client
Presentation, rendering, audio, input, camera, accessibility, local prediction, UI, asset streaming and client-side convenience logic. The client is not trusted for money, inventory ownership, rewards, race placements or enforcement decisions.

### World servers
Authoritative validation of player motion, active interactions, nearby entity state, NPC/traffic budgets, region occupancy and region transfer. The simulation is spatially partitioned into cells. Clients receive only relevant nearby entities.

### Race servers
Separate authoritative instances for circuit racing, time attack, drift battles, endurance and motorcycle events. The race server controls session phase, timing, checkpoints, penalties, standings and final results. The .NET `RaceSessionCoordinator` now links the local race state to the durable create/start/finalize API, including retry-safe finalization. A .NET 8 Worker Service host is also present for Windows deployment; it validates protected control configuration and holds the process lifecycle, but remains a control-plane shell until simulation and networking adapters are connected. Its `RaceSessionRuntime` provides a tested session state machine: 1–64 entrants, ordered checkpoint progression across configurable 1–100 lap races, monotonic server timestamps, finish-time calculation, DNF/disqualification and deterministic final placements. A trusted server reward policy supplies Credits payouts. This is not yet a complete authoritative simulation: server-side vehicle physics, telemetry validation, movement anti-cheat, networking/replication and reconnect handling remain separate work.

### Backend (later online phase)
- Supabase Auth: identity/session issuance.
- PostgreSQL: durable profiles, progression, ownership, configuration and transaction ledger.
- Trusted Edge Functions / backend services: privileged workflows.
- Dedicated game servers: real-time simulation.
- Realtime: social presence and infrequent events only; not per-frame physics transport.

## 3. Region model
Initial logical regions:
- `metro_city`
- `coastal_city`
- `mountain_district`
- `motorsport_park`

Each region is composed of streaming cells. Handoff protocol:
1. Directory selects destination cell.
2. Destination reserves capacity and returns a short-lived transfer ticket.
3. Source prepares a versioned player snapshot.
4. Destination validates the ticket and snapshot, then acknowledges.
5. Client moves to destination and receives authoritative state.
6. Source releases old session after acknowledgement.
7. Durable state changes use idempotency keys.

No live server handoff implementation is included yet; this is a protocol contract for the next backend milestone.

## 4. Vehicle framework
Shared lifecycle/data interfaces with two independent simulation models:
- Car: WheelCollider proof-of-concept first; later calibrated tire-force model, drivetrain, suspension, differential, aero, brake heat and damage.
- Motorcycle: separate rigid-body and lean/balance model; never reuse car dynamics.
- Shared: ownership, upgrades, damage, camera anchors, audio, spawn validation and network identity.

Camera changes are presentation-only and never change physics.

## 5. Networking
Networking is intentionally not needed for the first playable offline build. The first local source package intentionally has no NGO transport dependency, so it compiles in a fresh URP project without needing network packages. Select and pin the current supported Netcode packages only after the project Editor version is fixed. Server-authoritative design remains mandatory for wallet, race results, ownership and competitive state.

Unity's current docs distinguish Netcode for GameObjects (higher-level GameObject workflows) and Netcode for Entities (high-performance DOTS workflows). Start with GameObjects for the first small test cell; benchmark before selecting Entities for denser cells.

## 6. Mobile performance
Use URP, LODs, occlusion culling, texture compression, addressables, additive scene streaming, object pools, bounded NPC simulation and adaptive quality. Profile CPU, GPU, RAM, thermal load, battery and network usage on real devices. 30/60 FPS are engineering targets, not measured results.

## 7. Security invariants
- Offline local progression is separate from online balances and verified rewards.
- Server validates purchases, rewards, ownership, race results and job payouts.
- No service-role key in client builds.
- RLS on every exposed table.
- Wallet balances are transactionally backed by an append-only ledger.
- Every economic operation has a unique idempotency key.
- Rate limits, audit records, server logs and anti-cheat telemetry for sensitive actions.
