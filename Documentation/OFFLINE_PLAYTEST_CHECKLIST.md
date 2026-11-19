# APEX GT WORLD — Offline Playtest Checklist

## Scope
This checklist covers the first offline-only driving vertical slice. It does not certify the long-term open world, production vehicle physics, Android performance or online backend.

## Editor setup
- Unity 6 LTS with a 3D URP project.
- Copy this branch's `Assets/` folder into the project and keep Unity's generated URP assets, Packages manifest and ProjectVersion.
- Set Player > Other Settings > Active Input Handling to `Input Manager (Old)` for the current keyboard adapter and legacy EventSystem module.
- Select `APEX GT WORLD > Setup > Generate Starter Car Scene`.
- Confirm `Assets/Scenes/Bootstrap.unity` is included in Build Settings.
- Wait for compilation. Resolve every red Console error before pressing Play.

## Offline boot test
1. Disconnect the PC from the internet or disable the network adapter.
2. Enter Play Mode.
3. Confirm the APEX GT WORLD Offline Single Player menu appears without Auth, sign-in, Supabase or race-server requests.
4. Confirm the profile panel shows the local player name, career level and local Credits.
5. Stop Play Mode and relaunch; confirm profile data remains.

## Free-drive test
- Select Start Free Drive.
- WASD / arrow keys steer and accelerate; Space brakes; Left Shift applies handbrake; V toggles camera.
- Press Escape or Pause and confirm the car and simulation stop.
- Resume and confirm controls work again.
- Return to Main Menu and confirm the vehicle cannot move behind the menu.

## Career race test
1. From Main Menu select Career, then Start Career Race.
2. Confirm three AI vehicles spawn with the player on the local circuit.
3. Confirm the HUD shows speed, lap and next checkpoint.
4. Drive four gates in order and repeat for two laps.
5. Confirm out-of-order gates do not advance the lap.
6. Finish and confirm the race result screen shows elapsed time.
7. Confirm the local profile has received 500 Credits and 250 XP once.
8. Restart the same event and confirm the one-time reward is not duplicated.

## Garage and persistence test
- Open Garage.
- Purchase one Engine, Handling or Brakes upgrade for 250 Credits.
- Confirm the selected level increases and vehicle tuning is reapplied.
- Close the application and launch it again.
- Confirm Credits, XP, completed event, owned vehicle and upgrade levels persist.
- Verify a save recovery scenario only on a disposable test profile: back up the save files, damage the primary JSON, launch and confirm backup recovery.

## Android smoke test
- Switch Build Platform to Android and install Android Build Support / SDK / NDK / OpenJDK.
- Set Internet Access to Not Required for this offline-only build.
- Build and install on a real Android device.
- Verify touch buttons, pause/resume, camera toggle, heat/battery behavior and save persistence after app suspension.
- Do not claim a performance target until CPU/GPU frame time, memory and thermals are profiled on the actual device.

## Current known limitations
- The scene is a primitive-geometry practice circuit, not the finished Metro City or a complete open world.
- The AI uses simple waypoint movement, not full racing tactics or collision avoidance.
- The car uses an early WheelCollider prototype, not production-calibrated tire/suspension/drivetrain physics.
- The one initial Career event grants a one-time local reward.
- The Garage currently tunes one starter vehicle; licensed car models, paint, parts inventory and full vehicle roster are not included.
- Audio, weather, day/night, traffic, pedestrians, jobs, property/business systems, mission chains, motorcycle career and region streaming are future milestones.
- Unity Editor compilation, scene playtesting and real-device APK testing must pass before calling this a playable release.
- Local save data can be modified by the device owner; it must never be trusted for online competitive rewards or leaderboards.
