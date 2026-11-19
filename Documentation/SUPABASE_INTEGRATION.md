# Supabase Integration — APEX GT WORLD

## Connected project
- Project: `GTWORLD`
- Ref: `hzawgrvonsqyqjyzzreg`
- URL: `https://hzawgrvonsqyqjyzzreg.supabase.co`
- Region: `eu-west-1`
- Status at last check: `ACTIVE_HEALTHY`

The Unity client uses the supplied `sb_publishable_...` key. Publishable keys are designed for client use when RLS and least-privilege grants are enforced. Never add `sb_secret_...`, `service_role`, database passwords, or server shared secrets to Unity assets or source control.

## Applied database migrations
1. `0001_apex_core.sql` — 11 public tables, RLS, explicit grants, and `apex_private.apex_race_session_secrets`.
2. `0002_apex_security_and_region_indexes.sql` — explicit deny policy on private session secrets and region foreign-key indexes.

Verified state:
- 11 public APEX tables and one private table exist.
- RLS is enabled on all 12 tables.
- Four region seed records exist.
- `authenticated` cannot use the `apex_private` schema or read private session secrets.
- `authenticated` cannot update profile `reputation`; it can update only `display_name`.
- `service_role` can access private session secrets.
- Supabase Security Advisor returned no findings after migration 0002.
- Performance Advisor reports only currently unused indexes; there is no real gameplay workload yet.

## Unity integration
`SupabaseRestClient.cs` uses UnityWebRequest and Supabase Auth/PostgREST HTTP endpoints. It currently provides:
- Email/password sign-in and sign-up
- In-memory refresh-token flow and sign-out
- Fetch own profile and save own display name
- Fetch configured world regions
- A development-only Supabase debug panel in generated test scenes

The profile save request sends only `user_id` and `display_name`. `home_region` and `reputation` are server-controlled and cannot be changed by the client. The client does not write wallet, inventory, property, business, or race-result records.

This is a small HTTP adapter, not the full official Supabase SDK. Access tokens remain in memory only. Before production, implement secure refresh-token storage using Android Keystore and iOS Keychain through a vetted Unity plugin. Never store refresh tokens in plain PlayerPrefs.

## Remaining integration checks
1. Confirm `public` is enabled as a Data API exposed schema in the Supabase dashboard.
2. Configure Auth email confirmation and Android/iOS redirect URLs.
3. Run real sign-up/sign-in/profile/regions tests from Unity Editor.
4. Add trusted Edge Function or dedicated server workflows for wallet posting, purchases, vehicle grants, property/business transfers and race settlement.
5. Test owner isolation, refresh expiry, network errors, rate limits and idempotency.
6. Keep the service-role key exclusively in trusted server-side secrets.

Do not run these migrations on Qulup or ASIX.
