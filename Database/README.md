# Database Setup

The migrations under `Migrations/` target the dedicated APEX GT WORLD Supabase project only. Do not apply them to Qulup or ASIX.

## Migration order
1. `0001_apex_core.sql` — initial schema, RLS, narrow client grants and private session-secret storage.
2. `0002_apex_security_and_region_indexes.sql` — private session-secret deny policy and region foreign-key indexes.
3. `0003_apex_auth_user_bootstrap.sql` — secure Auth trigger provisions a profile, zeroed wallet and starter progression; removes direct client profile insertion.

## Security model
- RLS is enabled on all 11 client-facing `public.apex_*` tables and the private session-secret table.
- Players can update only their own profile `display_name`; Reputation and `home_region` are server-controlled.
- Auth sign-up provisions the profile, wallet and progression row in the same database transaction through a `SECURITY DEFINER` trigger with an empty search path.
- The trigger function is not an exposed RPC; direct execute permission is revoked from client roles.
- Wallet balances and ledger rows are read-only to clients and owner-scoped by RLS.
- Vehicle, motorcycle, property, business and race-result writes are server-only.
- The race-session handshake key is stored in the non-exposed `apex_private` schema.

## Apply and verify
1. Apply migrations in filename order to the dedicated development project.
2. Confirm the trigger exists on `auth.users` and test sign-up with email confirmation both enabled and disabled.
3. Verify each new Auth user gets exactly one profile, wallet and progression row.
4. Run Supabase security and performance advisors.
5. Test that clients cannot insert profiles or update Reputation/`home_region`, and cannot read private session secrets.
6. Add trusted server/Edge Function workflows for wallet posting, purchases, vehicle grants, property/business transfers and race settlement.
7. Test owner isolation, rollback behavior and idempotency.

The migrations do not implement privileged economic functions. Clients must never write wallet balances, ledger entries, race settlements, vehicle ownership or property/business transfers directly.
