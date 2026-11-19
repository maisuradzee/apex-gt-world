# Roadmap and Acceptance Gates

## Gate 0 — Source and backend foundation
- [x] Product and architecture baseline; Unity starter source and Android build menu.
- [x] Car/motorcycle prototype and region/scene foundation.
- [x] Eight Supabase migrations applied and security reviewed.
- [x] Auth, wallet and race lifecycle transaction tests passed with rollback.
- [x] Player reward claim Edge Function deployed with JWT verification.
- [x] Race control Edge Function deployed with shared-token authentication.
- [x] .NET 8 race-server control client prepared.
- [ ] Configure `APEX_RACE_SERVER_TOKEN` securely in Supabase and server environment.
- [x] Pass Deno type checks and .NET 8 build in GitHub Actions.

## Gate 1 — Offline vehicle vertical slice
- [ ] Open Unity 6 LTS URP project and compile.
- [ ] Generate car and motorcycle scenes in Editor.
- [ ] Verify car movement, steering, braking and wheel visuals.
- [ ] Tune suspension, tires, center of mass and camera.
- [ ] Tune motorcycle lean, braking and rider mass shift.
- [ ] Build and profile on low/mid and high-tier Android devices.

## Gate 2 — World vertical slice
- [ ] Build a Metro City cell with road loop and drift pad.
- [ ] Add addressable streaming and memory unload.
- [ ] Traffic, pedestrians, garage and vehicle selection.

## Gate 3 — Backend and race server
- [ ] Implement authoritative race simulation and participant management.
- [ ] Connect race server to create/start/finalize control operations.
- [ ] Add simulation-based score/reward calculation and anti-cheat checks.
- [ ] Run end-to-end race settlement with the shared secret configured.
- [ ] Implement purchase, inventory, vehicle ownership, progression and property APIs.
- [ ] Secure refresh-token storage; test real Unity Auth/PostgREST flows.

## Gate 4 — Multiplayer
- [ ] Persistent authoritative world server.
- [ ] Player replication, interest management, reconnect recovery.
- [ ] Region routing/handoff and dedicated race session hosting.
- [ ] Load, latency, packet-loss and abuse tests.

## Gate 5 — Game systems and release
- [ ] Car/motorcycle careers, championships, drift and endurance.
- [ ] RP jobs, police/EMS, properties, businesses, clubs and social.
- [ ] More regions, art/audio/content, Android performance matrix and iOS build.
- [ ] Moderation, privacy policy, store release and live operations.

## Definition of done
A feature is complete only when implementation, integration tests, security review where relevant, Android profiling, error handling, documentation and reproducible builds are available.
