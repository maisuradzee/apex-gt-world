# Build Status — 2026-11-19

## Implemented
- [x] Android-first multi-region motorsport/RP game foundation and Unity starter source.
- [x] Car and motorcycle input/physics prototypes, camera, region and scene-generation code.
- [x] Editor scene generators preserve existing Build Settings scenes.
- [x] Ten Supabase migrations applied to dedicated GTWORLD project.
- [x] RLS on all 11 public tables and private session-secret table.
- [x] Auth profile/wallet/progression bootstrap.
- [x] Service-only atomic idempotent wallet transaction RPC.
- [x] Service-only race start/finalize RPCs; rollback tests passed for lifecycle and reward.
- [x] Race finalization hardened: unique placements, integer-only scores/times/payouts; rejection and two-player success tests passed.
- [x] Race creation idempotency added through a private request-key table and service-only RPC; create retries return the original session.
- [x] JWT-protected player reward claim function `settle-race-result` deployed ACTIVE.
- [x] Shared-token protected race control function `race-session-control` deployed ACTIVE.
- [x] .NET 8 race-server control client added; uses environment variables, no Supabase service-role key.
- [x] GitHub CI passed source/security validation, Deno type-checks for both Edge Functions, and .NET 8 race-server client build.

## Pending / not verified
- [ ] Set `APEX_RACE_SERVER_TOKEN` in Supabase Edge Function secrets and dedicated server environment.
- [ ] Implement authoritative race simulation, participant state, physics, result calculation and anti-cheat.
- [ ] Run end-to-end race server → DB finalization → player reward claim with configured secret.
- [ ] Live Unity Auth/PostgREST/Edge smoke tests.
- [ ] Unity Editor compilation, Play Mode tests and Android APK/device testing.
- [ ] Secure refresh-token persistence, purchase/vehicle/property/business APIs, world server/replication.
- [ ] Art, audio, tracks, multiplayer load and device performance tests.

## Supabase state
`GTWORLD` (`hzawgrvonsqyqjyzzreg`, eu-west-1) is `ACTIVE_HEALTHY`. Ten migrations recorded. Security Advisor reports no findings after migration 0010; the private race-request mapping has explicit deny RLS policy. Privilege audit confirms only `service_role` can execute race lifecycle RPCs; client roles cannot write results. Strict race validation test passed and all test data rolled back. Edge Functions `settle-race-result` (JWT) and `race-session-control` (custom server token) are ACTIVE. The race control token is not configured through this integration and must be set in Edge Function Secrets plus the server host environment.

This remains a source/backend foundation, not a playable or production-ready game.
