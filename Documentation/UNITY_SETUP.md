# Unity Editor Setup

1. Install Unity Hub and a supported Unity 6 LTS Editor.
2. Install Android Build Support, Android SDK & NDK Tools, and OpenJDK from the Editor's Add Modules menu.
3. Create a new `3D (URP)` project named `APEX_GT_WORLD`.
4. Copy the provided `Assets/` folder and `.gitignore` into the project. Keep generated `ProjectSettings/ProjectVersion.txt`, URP assets and `Packages/manifest.json` from the new Editor project.
5. Open the project and wait for import/compile.
6. In the Unity menu select `APEX GT WORLD > Setup > Generate Starter Car Scene` or `Generate Motorcycle Test Scene`.
7. The generator adds the created scene to Build Settings automatically.
8. Select `APEX GT WORLD > Build > Build Android APK`.

Do not use an APK from this starter without testing it on the actual target phone. The Editor scene generator creates placeholder geometry only.

9. For Editor keyboard testing, set **Active Input Handling** to `Input Manager (Old)` in Project Settings > Player > Other Settings. Mobile touch controls use uGUI EventSystem. Do not select `Both` for Android; Unity documents that mode as unsupported on Android.


## Supabase test setup
10. The supplied project URL and publishable key are in `Assets/Scripts/Backend/SupabaseProjectSettings.cs`.
11. After the migration is applied and the `public` schema is exposed through Data API settings, generate either starter scene. A temporary Supabase development console appears on screen for Auth/profile/region smoke tests.
12. In Player Settings, set Internet Access to Require for Android.
13. The sample stores tokens in memory only. Do not ship it as a production login flow until refresh-token persistence is implemented using Android Keystore and iOS Keychain.
