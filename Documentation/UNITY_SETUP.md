# Unity Editor Setup — APEX GT WORLD Offline Prototype

## 1. Required tools
1. Install Unity Hub and **Unity 6.3 LTS `6000.3.28f1`**.
2. In Unity Hub's Installs > Add Modules, install Android Build Support, Android SDK & NDK Tools, and OpenJDK if you plan to build Android.
3. The render pipeline is URP. Create a new `3D (URP)` project in Unity Hub/Editor so Unity generates the matching URP pipeline asset, Renderer asset, Graphics settings and default scene.
4. Check out the repository branch `feature/offline-first-foundation`. Copy this branch's `Assets/` folder, `.gitignore`, `Packages/manifest.json` and `ProjectSettings/ProjectVersion.txt` into the newly created URP project. Preserve the URP template's `Assets/Settings` and generated `ProjectSettings` files.
5. Open the project and wait for Package Manager resolution and script import. Resolve all red Console errors before continuing.
6. In Project Settings > Player > Other Settings, set **Active Input Handling** to `Input Manager (Old)` for the current keyboard adapter and legacy uGUI EventSystem. Do not select `Both` for Android.
7. In the Unity menu select `APEX GT WORLD > Setup > Generate Starter Car Scene`. This creates `Assets/Scenes/Bootstrap.unity` and adds it to Build Settings. It overwrites that generated scene if it already exists.
8. Press Play. The Offline menu should appear. No login, Supabase setup, Windows server process or internet connection is needed.
9. Select **Free Drive** or **Career**. Career launches the two-lap Metro Circuit Club Cup with four ordered checkpoint gates and three simple AI opponents. Finishing earns a one-time local reward of 500 Credits and 250 XP.
10. Open **Garage**. Engine, Handling and Brakes upgrades cost 250 local Credits each, persist on the device and change the prototype vehicle's tuning.
11. Select `APEX GT WORLD > Build > Build Android APK` only after the Editor smoke tests pass.

The setup script requires Windows PowerShell and `robocopy` (included with Windows). Create the target project from the 3D (URP) template first.

## 2. Controls
- Keyboard: WASD / arrow keys to steer and accelerate.
- Space: brake.
- Left Shift: handbrake.
- V: switch camera.
- Escape or the on-screen PAUSE button: pause.
- Android: generated touch controls for steering, throttle, brake, handbrake and camera.

## 3. Offline save behavior
- The local profile is created automatically and stored under Unity `Application.persistentDataPath`.
- The save store uses a versioned JSON file, a temporary write and a backup recovery file.
- Progress saves periodically and when the app is paused or closed.
- Test persistence by completing the career event, purchasing a Garage upgrade, closing the app and launching again on the same device.
- For an offline-only Android build, set Player Settings > Publishing Settings / Internet Access to **Not Required**. Offline startup must not call Supabase or the Windows race server.

## 4. Package and Editor versions
- Project Editor: Unity 6.3 LTS `6000.3.28f1`.
- URP: `17.3.0`.
- uGUI: `2.0.0`.
- The 3D URP template must supply the actual URP pipeline and Renderer assets; the repository pins code/package dependencies, not machine-specific graphics project state.

## 5. Optional Supabase integration — later online phase
The Supabase URL and publishable key remain in `Assets/Scripts/Backend/SupabaseProjectSettings.cs`. Auth, cloud profiles and race rewards are optional later features and must not become a dependency of Offline startup. The sample login currently keeps tokens in memory only; do not enable production login until refresh-token persistence uses a vetted Android Keystore / iOS Keychain storage plugin.

## 6. Important limitations
The generated scene is a small primitive-geometry practice circuit, not Metro City or the finished open world. The AI uses basic waypoint movement. The car uses an early WheelCollider prototype, not production-calibrated tire, suspension or drivetrain physics. The current Garage modifies one starter car only. Audio, weather, day/night, traffic, pedestrians, jobs, properties, businesses, full event chains, production vehicle roster, region streaming, editor playtesting and real-device Android profiling remain future work. Do not call this a playable release until Unity compilation, Editor tests and an Android device smoke test pass.
