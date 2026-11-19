# Database Setup

The migrations under `Migrations/` target the dedicated APEX GT WORLD Supabase project only. Do not apply them to Qulup or ASIX.

## Migration order
1. `0001_apex_core.sql` — initial schema, RLS, narrow client grants and private session-secret storage.
2. `0002_apex_security_and_region_indexes.sql` — private session-secret deny policy and region foreign-key indexes.
3. `0003_apex_auth_user_bootstrap.sql` — secure Auth trigger provisions profile, zeroed wallet and starter progression; removes direct client profile insertion.
4. `0004_apex_service_role_privileges.sql` — grants trusted server-side access to existing and future database objects.

All four migrations are applied to `GTWORLD`.

## Security model
- RLS is enabled on all 11 client-facing `public.apex_*` tables and the private session-secret table.
- Players can update only their own profile `display_name`; Reputation and `home_region` are server-controlled.
- Auth sign-up provisions profile, wallet and progression in the same database transaction through a `SECURITY DEFINER` trigger with an empty search path.
- Trigger function is not an exposed RPC; direct execute permission is revoked from client roles.
- Wallet balances and ledger rows are read-only to clients and owner-scoped by RLS.
- Vehicle, motorcycle, property, business and race-result writes are server-only.
- `service_role` has full table permissions for trusted backend code. Never expose its key in a client app.
- The race-session handshake key is stored in non-exposed `apex_private`.

## Verification completed
- 12 APEX tables exist: 11 public and one private; RLS enabled on all 12.
- Four world-region seed rows exist.
- Auth trigger transaction test created exactly one profile, wallet and progression row; rollback removed the temporary user and generated rows.
- `authenticated` cannot insert profiles, change Reputation, write wallet/ledger/vehicle/race-result rows, or read private session secrets. It can update only `display_name`.
- `service_role` has SELECT/INSERT/UPDATE/DELETE on all 11 public tables and the private secret table.
- Default privileges grant backend access to future tables created by the postgres role.
- Supabase Security Advisor returned no findings. Performance Advisor reports unused indexes only; no gameplay workload exists yet.

## Remaining checks
1. Confirm Data API is enabled for the project and configure Auth redirect URLs.
2. Test real Unity Auth sign-up/sign-in and profile/region REST calls.
3. Add trusted Edge Function/dedicated-server endpoints for wallet posting, purchases, vehicle grants, property/business transfers and race settlement.
4. Test owner isolation, refresh expiry, network errors, rate limits, and transaction idempotency.

Do not place service-role credentials in Unity. The database migrations do not yet implement production economy or race-settlement functions.
