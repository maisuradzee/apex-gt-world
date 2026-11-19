# Acceptance Tests

## Unity Editor
- [ ] Open as a Unity 6 LTS URP project without compiler errors.
- [ ] Generate Bootstrap scene from APEX GT WORLD menu.
- [ ] Verify generated car has Rigidbody and four WheelColliders.
- [ ] Verify steering, throttle, brake and handbrake values are clamped.
- [ ] Verify camera toggle changes view only, not rigidbody state.
- [ ] Check no duplicate AudioListeners are active.
- [ ] Tune wheel collider friction and suspension; the generated scene is not calibrated.

## Android
- [ ] Android Build Support + SDK + NDK + OpenJDK installed through Unity Hub.
- [ ] APK builds successfully with IL2CPP and ARM64.
- [ ] Test at least one low/mid and one high-tier Android device.
- [ ] Measure FPS, frame time, RAM, temperature, battery, load time and crashes.
- [ ] Verify touch controls and camera switching on device.

## Backend
- [ ] Apply migrations only to dedicated APEX project.
- [ ] RLS enabled for every public/private APEX table.
- [ ] New Auth user receives a profile, wallet and progression row from the database trigger.
- [ ] Player A cannot read Player B's private vehicles, wallet or career progress.
- [ ] Client cannot write wallet, ledger, career progress or race-result rows.
- [ ] Duplicate wallet idempotency key cannot double-credit.
- [ ] Race reward endpoint verifies JWT, owner, validated result and finished session.
- [ ] Repeated race reward claim does not create another ledger credit.
- [ ] Race reward amount is read from the server-owned result row, never the request body.

## Multiplayer
- [ ] Client cannot submit arbitrary final position/reward.
- [ ] Disconnect/reconnect restores last durable checkpoint.
- [ ] Region transfer rejects duplicate tickets and stale snapshots.
- [ ] Load test records tick duration, CPU, memory, bandwidth and packet loss.
