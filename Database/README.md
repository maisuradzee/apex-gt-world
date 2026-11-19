# Database Setup

The migrations under `Migrations/` target the dedicated APEX GT WORLD Supabase project only. Do not apply them to Qulup or ASIX.

## Migration order
1. `0001_apex_core.sql` — initial schema, RLS and narrow client grants.
2. `0002_apex_security_and_region_indexes.sql` — private-session deny policy and indexes.
3. `0003_apex_auth_user_bootstrap.sql` — Auth trigger provisions profile, wallet and progression.
4. `0004_apex_service_role_privileges.sql` — trusted backend privileges for current and future objects.
5. `0005_apex_wallet_transactions.sql` — atomic, idempotent wallet RPC.
6. `0006_apex_race_reward_settlement.sql` — bounded server-owned race Credits payout.
7. `0007_apex_authoritative_race_lifecycle.sql` — service-only race start/finalize RPCs.
8. `0008_apex_race_result_integer_validation.sql` — unique placements and whole-number result validation.
9. `0009_apex_idempotent_race_session_creation.sql` — private request-key mapping and retry-safe session creation.

All nine migrations are applied to `GTWORLD`.

## Security model
- RLS enabled on all 11 public APEX tables and the private session-secret table.
- Player can update only own profile `display_name`; Reputation and `home_region` are server-controlled.
- Auth trigger provisions profile, wallet and progression.
- `service_role` has full table privileges; it is never placed in Unity.
- Wallet RPC locks the wallet row and checks idempotency and non-negative balances.
- Race result payout is bounded 0..1,000,000,000; client roles cannot write race results.
- `apex_server_create_race_session`, `apex_server_start_race_session` and `apex_server_finalize_race_session` are executable only by `service_role`.
- Session creation stores a client-generated request UUID in the private schema; repeated requests with the same region/discipline return the original session with `applied=false`, while reusing that key with changed parameters is rejected.
- Finalize validates the complete result set, requires a unique placement and whole-number score/time/payout, writes `validated=true`, sets session `finished` atomically, accepts identical retries and rejects conflicting retries.

## Verified
- 12 tables total: 11 public and one private; RLS on all 12.
- Four world-region seed rows.
- Auth bootstrap transaction tested and rolled back.
- Wallet transaction RPC passed award, duplicate replay, debit, overdraft and payload-mismatch tests.
- Player RLS test confirmed wallet/career owner isolation.
- Race lifecycle transaction test confirmed start, repeated start, finalize, identical retry and one wallet ledger award; rollback left no test data.
- Migration 0008 rejection tests passed for duplicate placements, fractional Credits payout and fractional finish time. Valid two-player results finalized with two unique placements; one 250 Credits wallet ledger row was created. Rollback removed both test users, session, results, wallets and ledgers.
- Service-only permissions verified for wallet and race lifecycle RPCs.
- Race session creation retry test confirms one session is returned for repeated identical request keys and conflicting parameter reuse is rejected.
- Security Advisor returned no findings after migration 0007; migration 0008 adds no new exposed tables or client grants.

## Race control Edge Function
`race-session-control` is deployed ACTIVE with `verify_jwt=false` because it authenticates the dedicated race server itself with a shared secret header. It supports create, start and finalize actions and holds the Supabase service-role key inside the Edge Function only.

Before the control API can execute actions, set `APEX_RACE_SERVER_TOKEN` in Supabase Edge Function secrets and in the dedicated server environment to the same strong random value (at least 32 characters). This token is not stored in Git or Unity. The .NET 8 client reads it from environment variables.

## Remaining
1. Implement the authoritative simulation process that calculates race results and payouts.
2. Configure the shared race-server token.
3. Run end-to-end race-server → Edge Function → DB → player claim tests.
4. Run real Unity Auth/PostgREST/Edge smoke tests and Android device validation.
5. Implement purchase, vehicle, inventory, property/business and progression APIs.
