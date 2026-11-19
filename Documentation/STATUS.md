# Build Status — 2026-11-19

## Implemented in source
- [x] Product/architecture baseline: Android-first, connected-region world, persistent online model, dual camera.
- [x] Unity WheelCollider car prototype and separate motorcycle lean prototype.
- [x] Car and motorcycle mobile touch-input adapters.
- [x] Editor scene generators for car and motorcycle test scenes.
- [x] Android IL2CPP/ARM64 APK build menu.
- [x] Initial Supabase schema migration with RLS and explicit client grants.
- [x] UnityWebRequest Supabase Auth and profile/region REST client foundation.
- [x] On-screen Supabase debug panel for prototype smoke tests.
- [x] GitHub Codex Connector installation on `maisuradzee`; Git Data write test passed.
- [x] GitHub source archive upload planned in initial repository commit; workflow configured to extract and commit project files.
- [x] GitHub Actions source-structure, JSON and SQL/RLS static checks configured.

## Not completed or not verified
- [ ] Unity Editor compilation (Unity Editor is not installed in this environment).
- [ ] Play-mode physics, camera and input validation.
- [ ] Android APK build and physical-device installation.
- [ ] Vehicle/motorcycle art, animation, audio, final tracks and world content.
- [ ] Production tire-force and motorcycle dynamics calibration.
- [ ] Dedicated persistent-world/race server implementation and hosting.
- [ ] Supabase migration application and live Auth/Data API tests.
- [ ] Multiplayer load, anti-cheat, security and recovery tests.

## Supabase connection status
The dedicated target `hzawgrvonsqyqjyzzreg` (`GTWORLD`, `eu-west-1`) is reachable and `ACTIVE_HEALTHY`. Its `public` schema currently has no tables and no migration history. The APEX migration has not been applied. Existing Qulup and ASIX projects were not modified.

## Scope note
This is a starter vertical slice and source foundation, not a playable or production-ready open-world game. Build and device claims require actual Unity Editor and Android hardware validation.
