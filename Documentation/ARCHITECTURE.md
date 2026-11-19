# APEX GT WORLD — Technical Architecture V2

## 1. Product architecture
One persistent player identity spans driving, motorcycle racing, open-world travel, roleplay, jobs, properties, businesses, clubs and competitive motorsport. The game is designed as one connected world with several independently streamed regions.

## 2. Runtime boundaries

### Unity client
Presentation, rendering, audio, input, camera, accessibility, local prediction, UI, asset streaming and client-side convenience logic. The client is not trusted for money, inventory ownership, rewards, race placements or enforcement decisions.

### World servers
Authoritative validation of player motion, active interactions, nearby entity state, NPC/traffic budgets, region occupancy and region transfer. The simulation is spatially partitioned into cells. Clients receive only relevant nearby entities.

### Race servers
Separate authoritative instances for circuit racing, time attack, drift battles, endurance and motorcycle events. The race server controls session phase, timing, checkpoints, penalties, standings and final results.

### Backend
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
The first local source package intentionally has no NGO transport dependency, so it compiles in a fresh URP project without needing network packages. Select and pin the current supported Netcode packages only after the project Editor version is fixed. Server-authoritative design remains mandatory for wallet, race results, ownership and competitive state.

Unity's current docs distinguish Netcode for GameObjects (higher-level GameObject workflows) and Netcode for Entities (high-performance DOTS workflows). Start with GameObjects for the first small test cell; benchmark before selecting Entities for denser cells.

## 6. Mobile performance
Use URP, LODs, occlusion culling, texture compression, addressables, additive scene streaming, object pools, bounded NPC simulation and adaptive quality. Profile CPU, GPU, RAM, thermal load, battery and network usage on real devices. 30/60 FPS are engineering targets, not measured results.

## 7. Security invariants
- Server validates purchases, rewards, ownership, race results and job payouts.
- No service-role key in client builds.
- RLS on every exposed table.
- Wallet balances are transactionally backed by an append-only ledger.
- Every economic operation has a unique idempotency key.
- Rate limits, audit records, server logs and anti-cheat telemetry for sensitive actions.
