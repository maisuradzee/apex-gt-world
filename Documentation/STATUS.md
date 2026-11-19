# Build Status — 2026-11-19

## Implemented in source and services
- [x] Product/architecture baseline: Android-first, connected-region world, persistent online model, dual camera.
- [x] Unity WheelCollider car prototype and separate motorcycle lean prototype.
- [x] Car and motorcycle mobile touch-input adapters.
- [x] Editor scene generators for car and motorcycle test scenes.
- [x] Android IL2CPP/ARM64 APK build menu.
- [x] Supabase UnityWebRequest Auth/PostgREST client foundation and development debug panel.
- [x] Dedicated Supabase project `GTWORLD` reachable and healthy.
- [x] Migrations 0001 and 0002 applied: 11 public tables, RLS, narrow client grants, private race-session secrets and region indexes.
- [x] RLS verified on all 11 public tables and the private secrets table.
- [x] SQL privilege audit: authenticated cannot change Reputation or read private secrets; service_role can read private secrets.
- [x] Supabase Security Advisor returned no findings after migration 0002.
- [x] GitHub source tree committed to `main`; GitHub Actions source expansion completed.
- [x] GitHub Actions source validator passes on the current branch.
- [x] Unity profile-save client changed to owner-filtered PATCH of display_name only.
- [x] Auth bootstrap migration 0003 and matching Unity sign-up/profile flow prepared; pending database deployment and runtime validation.

## Not completed or not verified
- [ ] Apply migration 0003 and validate the Auth trigger in a rollback-safe test.
- [ ] Unity Editor compilation (Unity Editor is not available in this execution environment).
- [ ] Play-mode physics, camera and input validation.
- [ ] Android APK build and physical-device installation.
- [ ] Live Unity Auth/PostgREST smoke tests (sign-up, sign-in, profile save, region fetch).
- [ ] Secure refresh-token persistence using Android Keystore/iOS Keychain.
- [ ] Trusted Edge Function/dedicated-server APIs for economy, inventory, purchases and race settlement.
- [ ] Dedicated persistent-world/race server implementation and hosting.
- [ ] Vehicle/motorcycle art, animation, audio, final tracks and world content.
- [ ] Multiplayer load, anti-cheat, recovery and device performance tests.

## Supabase state
Target: `hzawgrvonsqyqjyzzreg` (`GTWORLD`, `eu-west-1`), `ACTIVE_HEALTHY`. Migrations 0001 and 0002 are recorded. The database has 11 public APEX tables and one private session-secret table, all with RLS enabled; four region seed rows are present. Security Advisor had no findings after migration 0002. Migration 0003 is pending. Qulup and ASIX projects were not modified.

## Scope note
This remains a starter source foundation, not a playable or production-ready open-world game. Build and device claims require actual Unity Editor and Android hardware validation.
