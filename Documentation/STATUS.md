# Build Status — 2026-11-19

## Implemented in source and services
- [x] Product/architecture baseline: Android-first, connected-region world, persistent online model, dual camera.
- [x] Unity WheelCollider car prototype and separate motorcycle lean prototype.
- [x] Car and motorcycle mobile touch-input adapters.
- [x] Editor scene generators for car and motorcycle test scenes.
- [x] Android IL2CPP/ARM64 build menu.
- [x] Scene generators preserve existing enabled Build Settings scenes.
- [x] Supabase UnityWebRequest Auth/PostgREST client foundation and development debug panel.
- [x] Dedicated Supabase project `GTWORLD` reachable and healthy.
- [x] Migrations 0001–0005 applied: 11 public tables, RLS, restricted client grants, private session secrets, Auth bootstrap, trusted backend privileges and wallet transaction RPC.
- [x] RLS verified on all 11 public tables and the private secrets table.
- [x] Auth bootstrap trigger transaction test passed and rollback left no test rows.
- [x] Privilege audit: service_role has full backend access; authenticated can update only its own display_name and cannot write economy/inventory/race state.
- [x] Wallet RPC transaction test: idempotent award, debit, overdraft rejection and idempotency-payload mismatch rejection.
- [x] Supabase Security Advisor returned no findings after migration 0005.
- [x] GitHub source tree committed to `main`; source expansion and CI validator pass.
- [x] Unity profile-save uses owner-filtered PATCH; sign-up sends display-name metadata.
- [x] Auth session guard prevents account switching without sign-out; refresh responses are validated.

## Not completed or not verified
- [ ] Unity Editor compilation (Unity Editor is not available in this execution environment).
- [ ] Play-mode physics, camera and input validation.
- [ ] Android APK build and physical-device installation.
- [ ] Live Unity Auth/PostgREST smoke tests (real sign-up, sign-in, profile save, region fetch).
- [ ] Confirm Data API is enabled and configure Auth redirect URLs.
- [ ] Secure refresh-token persistence using Android Keystore/iOS Keychain.
- [ ] Trusted Edge Function/dedicated-server endpoint that validates authoritative game events before calling wallet RPC.
- [ ] Purchase, vehicle ownership, property/business and race-settlement server APIs.
- [ ] Dedicated persistent-world/race server implementation and hosting.
- [ ] Vehicle/motorcycle art, animation, audio, final tracks and world content.
- [ ] Multiplayer load, anti-cheat, recovery and device performance tests.

## Supabase state
Target: `hzawgrvonsqyqjyzzreg` (`GTWORLD`, `eu-west-1`), `ACTIVE_HEALTHY`. Five migrations are recorded. There are 11 public APEX tables and one private session-secret table, RLS enabled on all 12; four region seed rows exist. Security Advisor has no findings. Performance Advisor reports unused indexes only; no gameplay workload has been run. The Auth and wallet rollback tests left no test users or records. Qulup and ASIX were not modified.

## Scope note
This remains a starter source foundation, not a playable or production-ready game. Build and device claims require Unity Editor and Android hardware validation.
