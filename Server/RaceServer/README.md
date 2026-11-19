# APEX GT WORLD Race Server Control Client

This standalone .NET 8 helper talks to the deployed Supabase `race-session-control` Edge Function. It is server-only and must not be included in the Unity player build.

## Required environment variables
Set these only on the dedicated race-server host:
- `APEX_RACE_CONTROL_URL=https://hzawgrvonsqyqjyzzreg.supabase.co/functions/v1/race-session-control`
- `APEX_RACE_SERVER_TOKEN=<strong random secret, at least 32 characters>`

In the GTWORLD Supabase Dashboard, add the same token as an Edge Function secret named `APEX_RACE_SERVER_TOKEN`. The function will return 503 until it is configured. Keep the value out of Git, issue logs, Unity assets, Android/iOS builds and player clients. Rotate it in both environments if exposed.

The Supabase-managed `SUPABASE_SERVICE_ROLE_KEY` stays inside the Edge Function. The race server does not receive or store it.

## API lifecycle
1. `CreateSessionAsync(regionId, discipline, requestKey)` creates a session in `created`. Supply one stable non-empty GUID per logical create request; reuse it for network retries. The same key/parameters return the same session; changing region or discipline while reusing the key is rejected.
2. `StartSessionAsync(sessionId)` changes it to `running`; repeated start while running is safe.
3. The race runtime tracks entrants, server timestamps, ordered checkpoint progress and terminal states; the host's authoritative vehicle physics and movement validation still need to be integrated.
4. `FinalizeSessionAsync(sessionId, results)` submits validated placements, finish times, scores and server-calculated Credits payouts atomically.
5. The player separately claims their own result through the JWT-protected `settle-race-result` function.

The create RPC and finalize RPC are idempotent. Create uses a private request-key table; finalize accepts identical retries and rejects a changed result set.

## Authoritative race-session runtime

`RaceSessionRuntime.cs` is an engine-independent server-side state machine for a single race instance. It:
- accepts 1–64 registered entrants before the race starts;
- enforces a single ordered checkpoint sequence per course;
- uses server-supplied monotonic timestamps and derives finish time on the server;
- records Finished, DNF and disqualified entrant states;
- creates deterministic placements (finishers by finish time, then non-finishers by progress and stable tie-break);
- requires a trusted server reward-calculator delegate and caches final submissions so retries cannot silently recalculate payouts.

The host must call checkpoint methods only after authoritative server-side position/trigger validation. The runtime does **not** yet simulate vehicle physics, validate movement/telemetry, replicate players, manage reconnects or prove anti-cheat integrity. Reward amounts remain the responsibility of a server-owned reward schedule; no economy rates are hard-coded here.

## Build
```bash
dotnet build Server/RaceServer/ApexGtWorld.RaceServer.csproj
```

The GitHub Actions workflow builds the .NET server library and runs both HTTP contract tests (with a fake handler) and race-session domain tests. The current runtime is a lifecycle/checkpoint domain component only; it does not implement simulation ticks, vehicle physics, network replication, reconnect handling, telemetry validation or anti-cheat.


## Tests
```bash
dotnet run --project Server/RaceServer.Tests/ApexGTWorld.RaceServer.Tests.csproj
```
