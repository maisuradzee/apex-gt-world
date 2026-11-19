# APEX GT WORLD Race Server Control Client

This standalone .NET 8 helper talks to the deployed Supabase `race-session-control` Edge Function. It is server-only and must not be included in the Unity player build.

## Required environment variables
Set these only on the dedicated race-server host:
- `APEX_RACE_CONTROL_URL=https://hzawgrvonsqyqjyzzreg.supabase.co/functions/v1/race-session-control`
- `APEX_RACE_SERVER_TOKEN=<strong random secret, at least 32 characters>`

In the GTWORLD Supabase Dashboard, add the same token as an Edge Function secret named `APEX_RACE_SERVER_TOKEN`. The function will return 503 until it is configured. Keep the value out of Git, issue logs, Unity assets, Android/iOS builds and player clients. Rotate it in both environments if exposed.

The Supabase-managed `SUPABASE_SERVICE_ROLE_KEY` stays inside the Edge Function. The race server does not receive or store it.

## API lifecycle
1. `CreateSessionAsync(regionId, discipline)` creates a session in `created`.
2. `StartSessionAsync(sessionId)` changes it to `running`; repeated start while running is safe.
3. Dedicated authoritative simulation runs on the race server.
4. `FinalizeSessionAsync(sessionId, results)` submits validated placements, finish times, scores and server-calculated Credits payouts atomically.
5. The player separately claims their own result through the JWT-protected `settle-race-result` function.

The SQL RPC validates the result set and ensures identical retries do not duplicate or overwrite results.

## Build
```bash
dotnet build Server/RaceServer/ApexGtWorld.RaceServer.csproj
```

This is the control-plane client only. It does not yet implement simulation ticks, physics, replication, participant lifecycle, result calculation or anti-cheat.
