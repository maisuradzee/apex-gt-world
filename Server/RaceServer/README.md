# APEX GT WORLD Race Server Control Client

This is a server-only .NET 8 helper. It is not part of the Unity player build.

## Configuration
Set these environment variables on the dedicated race-server host:

- `APEX_RACE_CONTROL_URL=https://hzawgrvonsqyqjyzzreg.supabase.co/functions/v1/race-session-control`
- `APEX_RACE_SERVER_TOKEN=<strong random secret, at least 32 characters>`

Set the exact same token as an Edge Function secret named `APEX_RACE_SERVER_TOKEN` in the GTWORLD Supabase project. Do not put it in Git, Unity assets, Android/iOS builds, client logs or the player app. Rotate it in both places if it is exposed.

The Supabase Edge Function uses its server-managed `SUPABASE_SERVICE_ROLE_KEY` internally. Do not copy that key into the race server if the Edge Function is the only database control boundary.

## Lifecycle
1. `CreateSessionAsync(regionId, discipline)` creates a `created` session.
2. `StartSessionAsync(sessionId)` calls the service-only start RPC; repeat calls are safe while the session is already running.
3. The authoritative simulation runs on the dedicated server.
4. `FinalizeSessionAsync(sessionId, results)` submits the server-calculated player placement, finish time, score and Credits reward in one atomic database operation.
5. The player claims their own reward using the separate JWT-protected `settle-race-result` function.

The Edge Function accepts only the operations and fields required for each step. The SQL RPC performs final validation and is restricted to `service_role`.

## Build
```bash
dotnet build Server/RaceServer/ApexGTWorld.RaceServer.csproj
```

This helper supplies the backend control client; it is not yet the game simulation, physics loop, player replication, anti-cheat or region server.
