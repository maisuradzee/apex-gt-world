# Supabase Integration — APEX GT WORLD

## Connected project
- Project: `GTWORLD`
- Ref: `hzawgrvonsqyqjyzzreg`
- URL: `https://hzawgrvonsqyqjyzzreg.supabase.co`
- Region: `eu-west-1`
- Status at last check: `ACTIVE_HEALTHY`

The Unity client uses the supplied `sb_publishable_...` key. Publishable keys are designed for client use when RLS and least-privilege grants are enforced. Never add `sb_secret_...`, `service_role`, database passwords, or server shared secrets to Unity assets or source control.

## Database migrations
1. `0001_apex_core.sql` — 11 public tables, RLS, explicit grants and `apex_private.apex_race_session_secrets`.
2. `0002_apex_security_and_region_indexes.sql` — explicit deny policy on private session secrets and region foreign-key indexes.
3. `0003_apex_auth_user_bootstrap.sql` — Auth trigger provisions a profile, zeroed wallet and starter progression; direct client profile insertion is revoked.

Migrations 0001 and 0002 are applied and verified. Migration 0003 is included in the repository and awaits deployment after CI validation. Do not run any migration on Qulup or ASIX.

## Security model
- RLS is enabled on all public APEX tables and the private session-secret table.
- Players can update only their own profile `display_name`; Reputation and `home_region` are server-controlled.
- Auth registration provisions profile, wallet and progression in a database trigger.
- Client profile editing uses an owner-filtered PATCH and sends only `display_name`.
- Wallet, inventory, property, business and race-result mutations remain server-only.
- The private race-session key is not exposed through the public schema or client roles.

## Unity integration
`SupabaseRestClient.cs` uses UnityWebRequest and Supabase Auth/PostgREST HTTP endpoints. It supports:
- Email/password sign-in and sign-up with display-name metadata
- In-memory refresh-token flow and sign-out
- Fetch own profile and update own display name
- Fetch configured world regions
- A development-only Supabase debug panel in generated test scenes

This is a small HTTP adapter, not the full official Supabase SDK. Access and refresh tokens remain in memory only. Before production, implement secure refresh-token storage using Android Keystore and iOS Keychain through a vetted Unity plugin. Never store refresh tokens in plain PlayerPrefs.

## Remaining integration checks
1. Apply migration 0003 to the dedicated development project and verify trigger creation.
2. Test Auth sign-up (with email confirmation enabled and disabled) and confirm the profile, wallet and progression rows are provisioned once.
3. Confirm `public` is enabled as a Data API exposed schema and configure Auth redirect URLs.
4. Run Unity sign-in, profile PATCH and regions GET smoke tests.
5. Add trusted Edge Function or dedicated server workflows for wallet posting, purchases, vehicle grants, property/business transfers and race settlement.
6. Test owner isolation, refresh expiry, network errors, rate limits and idempotency.

Do not apply these migrations to Qulup or ASIX.
