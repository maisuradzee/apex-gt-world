# Roadmap and Acceptance Gates

## Gate 0 — Source and backend foundation
- [x] Product choices and architecture outline.
- [x] Unity starter C# source and Editor scene generator.
- [x] Android build menu script.
- [x] Car and motorcycle test-scene setup; preserve existing Build Settings.
- [x] Supabase schema with RLS and least-privilege client grants.
- [x] Dedicated Supabase project provisioned and reachable.
- [x] Migrations 0001–0007 applied and recorded.
- [x] Auth bootstrap trigger verified transactionally.
- [x] Trusted service_role privileges verified.
- [x] Atomic, idempotent wallet transaction RPC implemented and rollback-tested.
- [x] Player Wallet/Career RLS isolation tested.
- [x] JWT-protected race reward Edge Function deployed.
- [x] Authoritative race lifecycle start/finalize RPCs implemented and transaction-tested.
- [x] Unity Wallet/Career reads and race reward claim bridge added.
- [x] Security Advisor clean; player/client privilege boundaries verified.
- [ ] Dedicated race-server process to call lifecycle RPCs and validate race simulation.
- [ ] Final CI after lifecycle documentation update and Unity Editor compilation.
- [ ] Unity 6 LTS URP project opened and exact Editor/package versions pinned.

## Gate 1 — Offline vehicle vertical slice
- [ ] Generate starter scenes in Unity Editor.
- [ ] Verify car acceleration, steering, braking and wheel visuals.
- [ ] Tune suspension, center of mass, tire friction and camera smoothing/collision.
- [ ] Tune motorcycle lean, rider mass shift, braking and tire contact separately.
- [ ] Build and profile on at least two Android device tiers.

## Gate 2 — World vertical slice
- [ ] Build one small Metro City cell with a road loop and drift pad.
- [ ] Add addressable scene streaming and memory-unload tests.
- [ ] Add traffic and pedestrian prototypes with simulation-distance limits.
- [ ] Add garage and vehicle selection.

## Gate 3 — Backend integration
- [ ] Implement dedicated authoritative race server and connect it to start/finalize RPCs.
- [ ] Verify real Unity Auth, profile, Wallet/Career and Edge Function claim flows.
- [ ] Implement trusted purchase, inventory, vehicle ownership and progression APIs.
- [ ] Add secure refresh-token storage.
- [ ] Add integration tests for owner isolation, idempotency and failure handling.

## Gate 4 — Multiplayer
- [ ] Implement dedicated authoritative world server.
- [ ] Add player replication and interest management.
- [ ] Implement disconnect/reconnect recovery.
- [ ] Add region directory and handoff.
- [ ] Integrate race-server results with the deployed settlement endpoint.

## Gate 5 — Game systems
- [ ] Racing career, time attack, drift and endurance.
- [ ] Motorcycle disciplines and championships.
- [ ] Jobs, police/EMS, properties and businesses.
- [ ] Inventory, clubs, friends and social systems.

## Gate 6 — Multi-region and release
- [ ] Coastal City, Mountain District and Motorsport Park.
- [ ] Server load tests and failure recovery.
- [ ] Android compatibility/performance matrix.
- [ ] iOS build, test and signing.
- [ ] Store listing, privacy policy, moderation and live-ops processes.

## Definition of done
A feature is complete only when implementation, integration tests, security review (where relevant), Android profiling, error handling, documented configuration and reproducible builds are available.
