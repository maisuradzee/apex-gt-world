# Build Status — 2026-11-19

## Implemented
- [x] Android-first multi-region motorsport/RP game foundation and Unity starter source.
- [x] Car and motorcycle input/physics prototypes, camera, region and scene-generation code.
- [x] Editor scene generators preserve existing Build Settings scenes.
- [x] Eight Supabase migrations applied to dedicated GTWORLD project.
- [x] RLS on all 11 public tables and private session-secret table.
- [x] Auth profile/wallet/progression bootstrap.
- [x] Service-only atomic idempotent wallet transaction RPC.
- [x] Service-only race start/finalize RPCs; rollback tests passed for lifecycle and reward.
- [x] JWT-protected player reward claim function `settle-race-result` deployed ACTIVE.
- [x] Shared-token protected race control function `race-session-control` deployed ACTIVE.
- [x] .NET 8 race-server control client added; uses environment variables, no Supabase service-role key.
- [x] GitHub CI validates source/security, Deno functions and .NET client build (workflow updated; latest CI result pending).

## Pending / not verified
- [ ] Set `APEX_RACE_SERVER_TOKEN` in Supabase Edge Function secrets and dedicated server environment.
- [ ] Implement authoritative race simulation, participant state, physics, result calculation and anti-cheat.
- [ ] Run end-to-end race server → DB finalization → player reward claim with configured secret.
- [ ] Live Unity Auth/PostgREST/Edge smoke tests.
- [ ] Unity Editor compilation, Play Mode tests and Android APK/device testing.
- [ ] Secure refresh-token persistence, purchase/vehicle/property/business APIs, world server/replication.
- [ ] Art, audio, tracks, multiplayer load and device performance tests.

## Supabase state
`GTWORLD` (`hzawgrvonsqyqjyzzreg`, eu-west-1) is `ACTIVE_HEALTHY`. Seven migrations recorded. Security Advisor reports no findings after migration 0007. Race and wallet DB transaction tests rolled back test data. Migration 0008 still needs a post-apply security review. Edge Functions `settle-race-result` (JWT) and `race-session-control` (custom server token) are ACTIVE. The race control token is not configured through this integration and must be set in Edge Function Secrets plus the server host environment.

This remains a source/backend foundation, not a playable or production-ready game.
