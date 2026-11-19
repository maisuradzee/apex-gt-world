# Build Status — 2026-11-19

## Implemented in source and services
- [x] Product/architecture baseline: Android-first, connected-region world, persistent online model, dual camera.
- [x] Unity WheelCollider car prototype and separate motorcycle lean prototype.
- [x] Car and motorcycle mobile touch-input adapters.
- [x] Editor scene generators for car and motorcycle test scenes.
- [x] Android IL2CPP/ARM64 APK build menu.
- [x] Supabase UnityWebRequest Auth/PostgREST client foundation and development debug panel.
- [x] Dedicated Supabase project `GTWORLD` reachable and healthy.
- [x] Migration 0001 applied: 11 public tables, RLS, profile grants, private race-session secret storage.
- [x] Migration 0002 applied: private-schema deny policy and region foreign-key indexes.
- [x] RLS verified on all 11 public tables and the private secrets table.
- [x] SQL privilege audit: authenticated cannot change Reputation or read private secrets; service_role can read private secrets.
- [x] Supabase Security Advisor returned no findings.
- [x] GitHub source tree committed to `main`; GitHub Actions source expansion completed.
- [x] GitHub Actions source validator passes on the current branch.

## Not completed or not verified
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
Target: `hzawgrvonsqyqjyzzreg` (`GTWORLD`, `eu-west-1`), `ACTIVE_HEALTHY`. The database has 11 public APEX tables and one private session-secret table, all with RLS enabled. Four region seed rows are present. Two migrations are recorded. Security Advisor has no findings. Performance Advisor currently shows unused indexes only; there is not yet a production workload. Qulup and ASIX projects were not modified.

## Scope note
This remains a starter source foundation, not a playable or production-ready open-world game. Build and device claims require actual Unity Editor and Android hardware validation.
