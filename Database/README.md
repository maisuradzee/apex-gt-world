# Database Setup

The migrations under `Migrations/` target the dedicated APEX GT WORLD Supabase project only. Do not apply them to Qulup or ASIX.

## Migration order
1. `0001_apex_core.sql` — initial schema, RLS, narrow client grants and private session-secret storage.
2. `0002_apex_security_and_region_indexes.sql` — private session-secret deny policy and region foreign-key indexes.
3. `0003_apex_auth_user_bootstrap.sql` — Auth trigger provisions profile, zeroed wallet and starter progression; removes direct client profile insertion.
4. `0004_apex_service_role_privileges.sql` — grants trusted server-side access to current and future database objects.
5. `0005_apex_wallet_transactions.sql` — atomic, idempotent wallet adjustment RPC restricted to `service_role`.

All five migrations are applied to `GTWORLD`.

## Security model
- RLS is enabled on all 11 client-facing `public.apex_*` tables and the private session-secret table.
- Players can update only their own profile `display_name`; Reputation and `home_region` are server-controlled.
- Auth sign-up provisions profile, wallet and progression in the same database transaction through a `SECURITY DEFINER` trigger with an empty search path.
- The trigger function is not an exposed RPC; direct execute permission is revoked from client roles.
- Wallet balances and ledger rows are read-only to clients and owner-scoped by RLS.
- Vehicle, motorcycle, property, business and race-result writes are server-only.
- `service_role` has full table permissions for trusted backend code. Never expose its key in a client app.
- The race-session handshake key is stored in non-exposed `apex_private`.
- Wallet mutation is performed by `apex_server_apply_wallet_delta`, a SECURITY DEFINER function with an empty search path, per-player row locking, balance checks and idempotency validation. Only `service_role` can execute it.

## Verification completed
- 12 APEX tables exist: 11 public and one private; RLS enabled on all 12.
- Four world-region seed rows exist.
- Auth trigger transaction test created exactly one profile, wallet and progression row; rollback removed the temporary user and generated rows.
- `authenticated` cannot insert profiles, change Reputation, write wallet/ledger/vehicle/race-result rows, or read private session secrets. It can update only `display_name`.
- `service_role` has SELECT/INSERT/UPDATE/DELETE on all 11 public tables and the private secret table.
- Default privileges grant backend access to future tables created by the postgres role.
- Wallet RPC test: +100 Credits, repeat same key (no duplicate), -30 Credits, insufficient debit rejected, mismatched idempotency payload rejected. Test wallet ended at 70 Credits with two ledger rows inside the transaction; rollback left no test Auth user, profile, wallet or ledger entries.
- `service_role` can execute the wallet RPC; `authenticated` and `anon` cannot.
- Supabase Security Advisor returned no findings. Performance Advisor reports unused indexes only; no gameplay workload exists yet.

## Remaining checks
1. Confirm Data API is enabled for the project and configure Auth redirect URLs.
2. Test real Unity Auth sign-up/sign-in and profile/region REST calls.
3. Implement and deploy trusted Edge Function/dedicated-server endpoints that call the wallet RPC after validating authoritative game events.
4. Add trusted APIs for purchases, vehicle grants, property/business transfers and race settlement.
5. Test owner isolation, refresh expiry, network errors, rate limits and transaction idempotency.

Do not place service-role credentials in Unity. The database RPC is a trusted server primitive, not a player-facing endpoint.
