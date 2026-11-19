# Database Setup

The migrations under `Migrations/` target the dedicated APEX GT WORLD Supabase project only. Do not apply them to Qulup or ASIX.

## Migration order
1. `0001_apex_core.sql` — initial schema, RLS, narrow client grants and private session-secret storage.
2. `0002_apex_security_and_region_indexes.sql` — private session-secret deny policy and region foreign-key indexes.
3. `0003_apex_auth_user_bootstrap.sql` — secure Auth trigger provisions profile, zeroed wallet and starter progression; removes direct client profile insertion.
4. `0004_apex_service_role_privileges.sql` — grants trusted server-side access to current and future database objects.
5. `0005_apex_wallet_transactions.sql` — atomic, idempotent wallet adjustment RPC restricted to `service_role`.
6. `0006_apex_race_reward_settlement.sql` — adds a bounded, server-owned Credits payout to race results.

All six migrations are applied to `GTWORLD`.

## Security model
- RLS is enabled on all 11 client-facing `public.apex_*` tables and the private session-secret table.
- Players can update only their own profile `display_name`; Reputation and `home_region` are server-controlled.
- Auth sign-up provisions profile, wallet and progression in the same database transaction through a `SECURITY DEFINER` trigger with an empty search path.
- The trigger function is not an exposed RPC; direct execute permission is revoked from client roles.
- Wallet balances, ledger rows, career progress, vehicle ownership and race results are read-only to clients and owner-scoped where applicable.
- `service_role` has full table permissions for trusted backend code. Never expose its key in a client app.
- The race-session handshake key is stored in non-exposed `apex_private`.
- Wallet mutation is performed by `apex_server_apply_wallet_delta`, a SECURITY DEFINER function with an empty search path, per-player row locking, balance checks and idempotency validation. Only `service_role` can execute it.
- Race result `reward_credits` is bounded to 0..1,000,000,000 and client roles cannot write it.
- The `settle-race-result` Edge Function verifies caller JWT, result ownership, server validation and finished-session state. It accepts only the result ID and invokes the wallet RPC using a stable idempotency key.

## Verification completed
- 12 APEX tables exist: 11 public and one private; RLS enabled on all 12.
- Four world-region seed rows exist.
- Auth trigger transaction test created exactly one profile, wallet and progression row; rollback removed the temporary user and generated rows.
- `authenticated` cannot insert profiles, change Reputation, write wallet/ledger/vehicle/race-result rows, or read private session secrets. It can update only `display_name`.
- `service_role` has SELECT/INSERT/UPDATE/DELETE on all 11 public tables and the private secret table.
- Default privileges grant backend access to future tables created by the postgres role.
- Wallet RPC test: +100 Credits, repeat same key (no duplicate), -30 Credits, insufficient debit rejected, mismatched idempotency payload rejected. Rollback left no test Auth user, profile, wallet or ledger entries.
- Race reward schema migration applied; reward field and range constraint exist. Client roles cannot INSERT/UPDATE race results or reward payout.
- Edge Function `settle-race-result` is ACTIVE with JWT verification enabled.
- Supabase Security Advisor returned no findings after migration 0006.

## Remaining checks
1. Implement trusted race-server validation that writes final placement, `validated=true` and `reward_credits`.
2. Run real Unity Auth, Wallet/Career GET and Edge Function claim smoke tests.
3. Implement trusted APIs for purchases, vehicle grants, property/business transfers and race settlement.
4. Test owner isolation, refresh expiry, network errors, rate limits and transaction idempotency.

Do not place service-role credentials in Unity. The database RPC and Edge Function are trusted-server primitives, not player-controlled amount APIs.
