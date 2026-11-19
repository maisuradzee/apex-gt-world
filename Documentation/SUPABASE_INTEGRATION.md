# Supabase Integration — APEX GT WORLD

## Project
- `GTWORLD`, ref `hzawgrvonsqyqjyzzreg`
- URL `https://hzawgrvonsqyqjyzzreg.supabase.co`
- Region `eu-west-1`
- Status `ACTIVE_HEALTHY`

Unity uses the supplied publishable key. Never store `service_role`, `sb_secret_...`, database password or server token in Unity or source control.

## Applied migrations
All ten migrations 0001–0010 are applied. They establish 11 public RLS tables, a private secret table, Auth bootstrap, service-role grants, idempotent wallet settlement, server-owned race reward amount and atomic race start/finalize RPCs.

## Verified DB boundaries
- Client roles cannot write wallet balances, ledger, career progress or race-result rows.
- Player reads are owner-scoped for wallet and career.
- Profile client update is restricted to display_name.
- Wallet RPC uses row locking, balance checks and an idempotency key.
- Race lifecycle functions validate result payloads, require unique placements and whole-number score/time/reward values, set validated=true, finalize the session atomically and accept only identical retries.
- Wallet and race lifecycle functions are callable only by service_role.
- Auth, wallet, race lifecycle and strict numeric validation transaction tests passed and rolled back all generated test data.
- Supabase Security Advisor reports no findings; migration 0008 was followed by a new privilege/index/rollback audit.

## Edge Functions
### settle-race-result
- ACTIVE, JWT verification enabled.
- Player submits only `race_result_id`.
- Function checks the player's JWT, result ownership, validated flag, finished session and server-stored payout.
- Calls wallet RPC with a stable idempotency key.

### race-session-control
- ACTIVE, platform JWT verification disabled by design; function performs its own shared-secret authentication.
- Header: `x-apex-race-server-token`.
- Operations: `create`, `start`, `finalize`.
- Create requires a stable `request_key` UUID and calls the private idempotent create-session RPC; the same key/parameters return the same session.
- Function calls service-only SQL RPCs or creates a session using its internal service-role client.
- It never accepts player JWTs or client game requests as trusted server authority.
- The private request-key mapping is protected with RLS, an explicit deny policy and no client table grants.

Set `APEX_RACE_SERVER_TOKEN` under GTWORLD Supabase Edge Function secrets, and set the exact same strong random token only in the dedicated race-server host environment. Use at least 32 characters. Without this secret, the function deliberately returns 503 and performs no operation. Do not put the token in GitHub, Unity or Android/iOS packages.

## Dedicated server client
`Server/RaceServer/RaceControlClient.cs` is a standalone .NET 8 server-only helper. It reads `APEX_RACE_CONTROL_URL` and `APEX_RACE_SERVER_TOKEN` from environment variables and exposes create/start/finalize calls. Create requires a stable caller-supplied GUID, reused across retries. It does not contain or require the Supabase service-role key.

Build:
```bash
dotnet build Server/RaceServer/ApexGtWorld.RaceServer.csproj
```

This is the control-plane client only. The authoritative physics/simulation loop, participant management, result calculation and anti-cheat are not yet implemented.

## Unity integration
Unity client supports Auth, profile update, region read, Wallet/Career GET and race reward claim by result ID. The debug UI is prototype-only. Refresh tokens are memory-only; production needs secure Android Keystore/iOS Keychain persistence.

CI now type-checks both Edge Functions, builds this .NET helper and runs isolated HTTP contract tests for create/start/finalize, JSON fields, token header and input validation.

## Still pending
1. Configure the race-server shared token.
2. Implement and host the authoritative race simulation process.
3. Run end-to-end race control and player claim tests.
4. Run real Unity Auth/PostgREST/Edge Function tests.
5. Implement purchase, vehicle, inventory, property/business and progression APIs.
6. Complete Unity Editor compile, Android APK and physical-device performance validation.
