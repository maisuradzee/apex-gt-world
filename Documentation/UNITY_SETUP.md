# Unity Editor Setup

1. Install Unity Hub and **Unity 6.3 LTS `6000.3.28f1`**. Install Android Build Support, Android SDK & NDK Tools, and OpenJDK if building for Android.
2. If you do not already have a URP starter project, create a new `3D (URP)` project in Unity Hub/Editor first so Unity generates compatible URP assets and Graphics settings.
3. Use the `feature/offline-first-foundation` branch of the repository while the offline changes are under review. Copy the repository's `Assets/`, `Packages/manifest.json` and `ProjectSettings/ProjectVersion.txt` into the URP project, preserving the generated `Assets/Settings` URP pipeline assets and the rest of the generated `ProjectSettings`.
4. Copy the provided `Assets/` folder and `.gitignore` into the project. Keep generated `ProjectSettings/ProjectVersion.txt`, URP assets and `Packages/manifest.json` from the Unity Editor project.
5. Open the project and wait for import/compile. Resolve any package/API issues in the Console before continuing.
6. In the Unity menu select `APEX GT WORLD > Setup > Generate Starter Car Scene`. This creates `Assets/Scenes/Bootstrap.unity` and adds it to Build Settings. It overwrites that specific generated scene if it already exists.
7. Press Play. The Offline menu should appear. No account, Supabase setup, server process or internet connection is needed for the local driving prototype.
8. Select Free Drive or Career. Career starts the two-lap Metro Circuit Club Cup with four checkpoint gates and three simple AI opponents. Finishing gives a one-time local reward. Garage upgrades cost 250 local Credits each.
9. Select `APEX GT WORLD > Build > Build Android APK` only after the Editor smoke test passes.

Do not use an APK from this starter without testing it on the actual target phone. The Editor scene generator creates placeholder geometry only.

9. For Editor keyboard testing, set **Active Input Handling** to `Input Manager (Old)` in Project Settings > Player > Other Settings. Mobile touch controls use uGUI EventSystem. Do not select `Both` for Android; Unity documents that mode as unsupported on Android.


## Offline save behavior
10. The local profile is created automatically on first launch and stored under Unity `Application.persistentDataPath`.
11. The save store keeps a backup and attempts recovery when the main file is damaged.
12. Progress saves periodically and when the app is paused or closed. Test persistence by finishing the local event, purchasing a Garage upgrade, closing the app and launching again on the same device.
13. The current Offline build does not need the Supabase project URL, API keys, Auth, Windows race server or Android Internet Access permission. Set Internet Access to Not Required for the offline-only player build.

## Optional Supabase integration (later online phase)
14. The supplied project URL and publishable key remain in `Assets/Scripts/Backend/SupabaseProjectSettings.cs`.
15. Supabase Auth, cloud profiles and race rewards are optional online features. Do not make them a dependency of offline startup.
16. The sample stores tokens in memory only. Do not enable production login until refresh-token persistence is implemented using a vetted Android Keystore / iOS Keychain storage plugin.
