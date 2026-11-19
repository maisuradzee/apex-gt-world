# Roadmap and Acceptance Gates

## Gate 0 — Source and backend foundation
- [x] Product choices and architecture outline.
- [x] Unity starter C# source and Editor scene generator.
- [x] Android build menu script.
- [x] Supabase initial schema with RLS and least-privilege grants.
- [x] Dedicated Supabase project provisioned and reachable.
- [x] Migrations 0001 and 0002 applied and recorded.
- [x] Security Advisor clean; column privileges and private-secret isolation verified.
- [x] GitHub source tree committed to `main`; CI source validation passes.
- [ ] Create/open the Unity 6 LTS URP project and pin exact Editor/package versions.

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
- [ ] Confirm Data API exposure and Auth redirect settings.
- [ ] Verify Auth sign-up/sign-in, profile save and region fetch from Unity.
- [ ] Implement trusted server-side wallet, inventory and ownership APIs.
- [ ] Add secure refresh-token storage.
- [ ] Add integration tests for owner isolation, idempotency and failure handling.

## Gate 4 — Multiplayer
- [ ] Implement dedicated authoritative world server.
- [ ] Add player replication and interest management.
- [ ] Implement disconnect/reconnect recovery.
- [ ] Add region directory and handoff.
- [ ] Add dedicated race server and server-validated results.

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
