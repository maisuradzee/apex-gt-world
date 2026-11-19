# APEX GT WORLD — Unity Source Starter

Mobile-first persistent open-world motorsport + roleplay project.

## Project status
This repository contains the initial APEX GT WORLD Unity source package. It includes C# vehicle and motorcycle prototypes, touch controls, Editor scene generators, Android build tooling, architecture documentation, static validation tests, and a first Supabase schema migration. It is a **starter vertical slice, not a finished or currently playable commercial game**. Unity compilation, Play-mode testing, Android APK creation, and live multiplayer have not yet been validated.

## Product decisions
- Android first; iOS-compatible architecture.
- Unity 6 LTS + URP.
- Connected regions: Metro City, Coastal City, Mountain District, and Motorsport Park.
- First-person and third-person cameras.
- Persistent shared world with separate authoritative race sessions.
- Mobile-optimized graphics; 30 FPS baseline / 60 FPS capable-device goals remain unverified targets.
- Original code, maps, UI, missions, and original/licensed vehicle assets only.

## Unity setup
1. Install Unity Hub and a supported Unity 6 LTS Editor.
2. Create a **3D (URP)** project named `APEX_GT_WORLD`.
3. Add Android Build Support, Android SDK & NDK Tools, and OpenJDK.
4. Copy this repository's `Assets/` folder into the project; retain Unity-generated URP and `ProjectSettings` files.
5. Merge `com.unity.ugui` from `Packages/manifest.json` if needed; do not replace URP template dependencies.
6. Reopen Unity, wait for compilation, then run `APEX GT WORLD > Setup` to generate a car or motorcycle test scene.
7. Test the scenes before using `APEX GT WORLD > Build > Build Android APK`.

## Supabase
`Database/Migrations/0001_apex_core.sql` targets a **new dedicated APEX GT WORLD project only**. The project `hzawgrvonsqyqjyzzreg` is reachable and active, but the migration has not yet been applied. Do not run it on Qulup or ASIX. RLS is enabled in the migration; wallet and authoritative economy writes are server-only.

The Unity client uses a public publishable key. A publishable key is not a substitute for user authentication or authorization. Never put a Supabase secret/service-role key, database password, signing key, or server secret in Unity assets or Git.

## GitHub
The source package is committed to `main`. The ChatGPT Codex Connector installation is active on the `maisuradzee` account and the Git Data write test succeeded. The `unpack-source.yml` workflow expands the source archive into the repository and commits the extracted project files when GitHub Actions is enabled and permitted to push to `main`.

## Validation
The included source validation checks required files, package JSON, and SQL/RLS structure. It passed in the preparation environment. These checks do not replace Unity Editor compilation, Play-mode physics tests, Android device profiling, or server-side integration tests. See `Documentation/STATUS.md` and `Documentation/ROADMAP.md`.
