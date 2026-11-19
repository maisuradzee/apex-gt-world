# Unity Editor Setup

1. Install Unity Hub and a supported Unity 6 LTS Editor.
2. Install Android Build Support, Android SDK & NDK Tools, and OpenJDK from the Editor's Add Modules menu.
3. Create a new `3D (URP)` project named `APEX_GT_WORLD`.
4. Copy the provided `Assets/` folder and `.gitignore` into the project. Keep generated `ProjectSettings/ProjectVersion.txt`, URP assets and `Packages/manifest.json` from the new Editor project.
5. Open the project and wait for import/compile.
6. In the Unity menu select `APEX GT WORLD > Setup > Generate Starter Car Scene` or `Generate Motorcycle Test Scene`. Both scenes now start with a local offline profile; no account login or internet connection is required.
7. The generator adds the created scene to Build Settings automatically.
8. Select `APEX GT WORLD > Build > Build Android APK`.

Do not use an APK from this starter without testing it on the actual target phone. The Editor scene generator creates placeholder geometry only.

9. For Editor keyboard testing, set **Active Input Handling** to `Input Manager (Old)` in Project Settings > Player > Other Settings. Mobile touch controls use uGUI EventSystem. Do not select `Both` for Android; Unity documents that mode as unsupported on Android.


## Offline save behavior
10. The local profile is created automatically on first launch and stored under Unity `Application.persistentDataPath`.
11. The save store keeps a backup and attempts recovery when the main file is damaged or has an unsupported schema.
12. Progress saves periodically and when the app is paused or closed. Test persistence by changing scene, stopping Play Mode, and launching again on the same device.

## Optional Supabase integration (later online phase)
13. The supplied project URL and publishable key remain in `Assets/Scripts/Backend/SupabaseProjectSettings.cs`.
14. Supabase Auth, cloud profiles and race rewards are optional online features. Do not make them a dependency of offline startup.
15. The sample stores tokens in memory only. Do not enable production login until refresh-token persistence is implemented using a vetted Android Keystore / iOS Keychain storage plugin.
