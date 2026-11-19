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
3. `0003_apex_auth_user_bootstrap.sql` — Auth trigger creates profile, zeroed wallet and starter progression; client profile insertion is revoked.
4. `0004_apex_service_role_privileges.sql` — explicit backend role grants and default privileges for future objects.

All four migrations are recorded in Supabase.

## Verified security
- RLS is enabled on all 11 public APEX tables and the private session-secret table.
- Auth trigger transaction test created one profile, wallet and progression row; rollback removed the test Auth user and generated rows.
- `authenticated` cannot insert profiles or update Reputation; it may update only `display_name`.
- `authenticated` cannot write wallets, ledgers, vehicles or race results, and cannot access the private schema.
- `service_role` has read/write access to all 11 public tables and the private session-secret table.
- Default privileges grant those backend rights to future objects created by the postgres role.
- Supabase Security Advisor reports no findings.
- Performance Advisor currently reports unused indexes only; no real gameplay workload has been run.

## Unity integration
`SupabaseRestClient.cs` uses UnityWebRequest and Supabase Auth/PostgREST HTTP endpoints. It supports:
- Email/password sign-in and sign-up with display-name metadata
- In-memory refresh-token flow and sign-out
- Fetch own profile and update own display name
- Fetch configured world regions
- A development-only Supabase debug panel in generated test scenes

Profile editing is an owner-filtered PATCH that sends only `display_name`. Auth registration provisions profile, wallet and progression in a database trigger.

This is a small HTTP adapter, not the full official Supabase SDK. Access and refresh tokens remain in memory only. Before production, implement secure refresh-token storage using Android Keystore and iOS Keychain through a vetted Unity plugin. Never store refresh tokens in plain PlayerPrefs.

## Remaining integration checks
1. Confirm Data API is enabled and configure Auth redirect URLs.
2. Test real Unity sign-up (email confirmation on/off), sign-in, profile PATCH and regions GET.
3. Implement trusted Edge Functions/dedicated-server APIs for wallet operations, purchases, vehicle ownership, property/business transfers and race settlement.
4. Test owner isolation, refresh expiry, network errors, rate limits and idempotency.

Do not apply these migrations to Qulup or ASIX. Keep the service-role key exclusively in server-side secrets.
