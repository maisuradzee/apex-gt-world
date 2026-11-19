# Database Setup

The migrations under `Migrations/` target the dedicated APEX GT WORLD Supabase project only. Do not apply them to Qulup or ASIX.

## Migration order
1. `0001_apex_core.sql` — initial schema, row-level security, least-privilege client grants, and private session-secret storage.
2. `0002_apex_security_and_region_indexes.sql` — explicit client-deny policy for private session secrets and covering indexes for region foreign keys.

## Security model
- RLS is enabled on all 11 client-facing `public.apex_*` tables and on the private session-secret table.
- Client privileges are revoked first, then granted narrowly.
- Players can create their own profile and update only `display_name`; Reputation and home-region changes remain server-controlled.
- Wallet balances and ledger rows are read-only to clients and owner-scoped by RLS.
- Vehicle, motorcycle, property, business and race-result writes are server-only.
- The race-session handshake key is stored in the non-exposed `apex_private` schema, with access limited to the trusted `service_role`.

## Apply and verify
1. Apply migrations in filename order to the dedicated development project.
2. Confirm the expected tables and migration history.
3. Run Supabase security and performance advisors.
4. Test that anonymous users have no access and authenticated users can read only permitted rows.
5. Test that profile clients cannot write Reputation/home_region and cannot read the private session key.
6. Add trusted server/Edge Function workflows for wallet posting, purchases, vehicle grants, property/business transfers and race settlement.
7. Test owner isolation and duplicate/idempotency behavior.

The migrations do not implement privileged economic functions. Clients must never write wallet balances, ledger entries, race settlements, vehicle ownership or property/business transfers directly.
