# Roadmap and Acceptance Gates

## Gate 0 — Source foundation
- [x] Product choices and architecture outline.
- [x] Unity starter C# source and Editor scene generator.
- [x] Android build menu script.
- [x] Supabase initial schema draft with RLS and explicit client grants.
- [x] GitHub write access verified; complete source tree committed to `main`.
- [x] GitHub Actions source extraction and static validation passed.
- [ ] Create/open the Unity 6 LTS URP project and pin exact Editor/package versions.
- [ ] Apply the initial Supabase migration to the dedicated development project after review.

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

## Gate 3 — Backend foundation
- [ ] Apply migrations in the dedicated development Supabase project.
- [ ] Verify Auth, profile provisioning and secure session refresh.
- [ ] Implement trusted server-side wallet and ownership APIs.
- [ ] Complete RLS, grants and security-advisor review.

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
