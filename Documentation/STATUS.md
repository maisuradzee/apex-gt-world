# Build Status — 2026-11-19

## Implemented in source and services
- [x] Product/architecture baseline: Android-first, connected-region world, persistent online model, dual camera.
- [x] Unity WheelCollider car prototype and separate motorcycle lean prototype.
- [x] Car and motorcycle mobile touch-input adapters.
- [x] Editor scene generators preserve existing Build Settings scenes.
- [x] Android IL2CPP/ARM64 build menu.
- [x] Supabase UnityWebRequest Auth/PostgREST adapter and development debug panel.
- [x] Dedicated Supabase project `GTWORLD` reachable and healthy.
- [x] Migrations 0001–0006 applied: 11 public tables, RLS, restricted grants, private session secrets, Auth bootstrap, service grants, wallet RPC and race reward field.
- [x] RLS verified on all 11 public tables and the private secrets table.
- [x] Auth bootstrap trigger transaction test passed; rollback left no test rows.
- [x] Wallet RPC test passed: idempotent award, debit, overdraft rejection and mismatch rejection.
- [x] Player RLS test confirmed Wallet/Career owner isolation.
- [x] Service role has backend access; client roles cannot write wallet/race-result state or execute wallet RPC.
- [x] `settle-race-result` Edge Function deployed ACTIVE with JWT verification enabled.
- [x] Function verifies user, race-result owner, server validation, finished session and server-stored payout before settling idempotently.
- [x] Unity Wallet/Career GET methods and race-reward claim bridge added to source.
- [x] Supabase Security Advisor returned no findings after migration 0006.

## Not completed or not verified
- [ ] Final CI for the Unity race-claim bridge and documentation update.
- [ ] Unity Editor compilation (not available in this execution environment).
- [ ] Play-mode physics, camera and input validation.
- [ ] Android APK build and physical-device installation.
- [ ] Live Unity Auth/PostgREST/Edge Function smoke tests.
- [ ] Implement dedicated authoritative race server that writes validated placements and server-calculated reward_credits.
- [ ] Secure refresh-token persistence using Android Keystore/iOS Keychain.
- [ ] Purchase, vehicle ownership, property/business and race-server APIs.
- [ ] Dedicated persistent-world/race server implementation and hosting.
- [ ] Vehicle/motorcycle art, animation, audio, final tracks and world content.
- [ ] Multiplayer load, anti-cheat, recovery and device performance tests.

## Supabase state
Target: `hzawgrvonsqyqjyzzreg` (`GTWORLD`, `eu-west-1`), `ACTIVE_HEALTHY`. Six migrations are recorded. There are 11 public APEX tables and one private session-secret table, RLS enabled on all 12; four region seed rows exist. Security Advisor has no findings. `settle-race-result` is ACTIVE with JWT verification. Auth and wallet rollback tests left no test users or records. Qulup and ASIX were not modified.

## Scope note
This is a starter source foundation with a deployed backend reward-settlement boundary, not yet a playable or production-ready game. Complete race-server validation, Unity compilation and device testing remain required.
