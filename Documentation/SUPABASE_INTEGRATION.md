# Supabase Integration — APEX GT WORLD

## Connected project
- Project: `GTWORLD`
- Ref: `hzawgrvonsqyqjyzzreg`
- URL: `https://hzawgrvonsqyqjyzzreg.supabase.co`
- Region: `eu-west-1`
- Status at last check: `ACTIVE_HEALTHY`

The Unity client uses the supplied `sb_publishable_...` key. Publishable keys are designed for client use when RLS and least-privilege grants are enforced. Never add `sb_secret_...`, `service_role`, database passwords, or server shared secrets to Unity assets or source control.

## Applied database migrations
1. `0001_apex_core.sql` — 11 public tables, RLS, explicit grants and `apex_private.apex_race_session_secrets`.
2. `0002_apex_security_and_region_indexes.sql` — private-session deny policy and region foreign-key indexes.
3. `0003_apex_auth_user_bootstrap.sql` — Auth trigger creates profile, zeroed wallet and starter progression; direct client profile insertion is revoked.
4. `0004_apex_service_role_privileges.sql` — trusted backend access to existing/future tables, sequences and functions.
5. `0005_apex_wallet_transactions.sql` — atomic and idempotent Credits/Premium Tokens adjustment RPC restricted to service_role.
6. `0006_apex_race_reward_settlement.sql` — server-owned, bounded `reward_credits` on race results.

All six migrations are recorded in Supabase.

## Verified security and database behavior
- RLS is enabled on all 11 public APEX tables and the private session-secret table.
- Auth trigger transaction test created one profile, wallet and progression row; rollback removed the test Auth user and generated rows.
- `authenticated` cannot insert profiles or update Reputation; it may update only `display_name`.
- `authenticated` cannot write wallets, ledgers, vehicles, progress or race results; cannot access private schema; cannot execute wallet RPC.
- `service_role` has read/write access to all 11 public tables and private session-secret table, and can execute wallet RPC.
- Wallet transaction test verified award, idempotent replay, debit, overdraft rejection and idempotency-payload mismatch rejection.
- Player RLS test confirmed user A can read own Wallet/Career and cannot read user B's Wallet/Career.
- Race result payout field and 0..1,000,000,000 constraint are applied; client roles cannot write race-result rows or payout.
- Security Advisor reports no findings.

## Deployed Edge Function
Function: `settle-race-result`  
Status: `ACTIVE`  
JWT verification: enabled.

Endpoint:
```
POST /functions/v1/settle-race-result
Authorization: Bearer <player access token>
Content-Type: application/json

{"race_result_id":"<UUID>"}
```

The function verifies the caller's JWT, loads the result by both result ID and authenticated user ID, requires `validated=true`, a finished race session and a bounded positive server-approved `reward_credits`, then calls `apex_server_apply_wallet_delta` with a stable per-result idempotency key. The request does not accept a payout amount, currency or player ID. The race server that calculates/validates result payouts is not yet implemented, so end-to-end reward processing is not yet tested.

## Unity integration
`SupabaseRestClient.cs` uses UnityWebRequest. It supports:
- Email/password sign-in and sign-up with display-name metadata
- In-memory refresh-token flow and sign-out
- Fetch/update own profile display name
- Fetch configured world regions
- Owner-filtered Wallet and Career Progress reads
- Claim a validated race reward by sending only the result ID to the Edge Function
- A development-only Supabase debug panel

Profile editing uses owner-filtered PATCH with only `display_name`. Auth registration provisions profile, wallet and progression through a database trigger. Wallet/Career methods are GET-only. Race reward claim uses the player's JWT and cannot choose the payout.

This is a lightweight HTTP adapter, not the full official Supabase SDK. Access and refresh tokens remain in memory only. Before production, implement secure refresh-token persistence using Android Keystore and iOS Keychain through a vetted Unity plugin. Never store refresh tokens in plain PlayerPrefs.

## Remaining integration checks
1. Implement the dedicated race server's result validation and server-owned payout calculation.
2. Confirm Data API and Auth redirect settings; run real Unity Auth, Wallet/Career GET and Edge Function claim tests.
3. Implement trusted APIs for purchases, vehicle grants, property/business transfers and race settlement.
4. Test refresh expiry, network errors, rate limits, replay and transaction idempotency.

Do not apply these migrations to Qulup or ASIX. Keep the service-role key exclusively in server-side secrets.
