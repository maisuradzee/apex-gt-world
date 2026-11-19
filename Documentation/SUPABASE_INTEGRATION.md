# Supabase Integration — APEX GT WORLD

## Client configuration
The Unity client is configured for the project URL/ref supplied by the project owner. Its `sb_publishable_...` key is a client-safe publishable key. The key is included in `SupabaseProjectSettings.cs` because it is intended for use in mobile clients. It does not grant elevated database privileges by itself.

Never add `sb_secret_...`, `service_role`, database passwords, or server shared secrets to Unity assets or source control.

## Unity integration
`SupabaseRestClient.cs` uses UnityWebRequest and the Supabase Auth/PostgREST HTTP endpoints. It supports:
- Email/password sign-in
- Email/password sign-up
- Refresh token while app session is in memory
- Sign-out
- Fetch own APEX profile
- Upsert own APEX profile
- Fetch configured world regions
- A temporary development console is added to the generated car and motorcycle scenes for on-device smoke testing.

This is a small HTTP adapter, not the full official Supabase SDK. The SDK's C# project documents .NET Standard 2.1 support including Unity, but this starter avoids an extra NuGet dependency until the Unity package workflow is pinned and validated.

## Required dashboard setup
1. In Supabase Dashboard, confirm the project ref is `hzawgrvonsqyqjyzzreg` and its URL is `https://hzawgrvonsqyqjyzzreg.supabase.co`.
2. In Project Settings / API / Data API, make sure `public` is an exposed schema if the app will use PostgREST.
3. Run `Database/Migrations/0001_apex_core.sql` in the SQL Editor or through an authorized migration tool.
4. Confirm every APEX table has RLS enabled and the intended policies/grants are installed.
5. Configure Auth providers, email confirmation and redirect URLs for Android/iOS.
6. In Unity Player Settings, set Internet Access to Require for Android.

## Current access limitation
The supplied project ref is not present in the Supabase projects returned by the currently connected Supabase account, and direct project inspection is denied. Therefore the migration has NOT been applied and live REST/Auth connectivity has NOT been verified. Reconnect/authorize the Supabase integration with the account/organization that owns this project, or add this project to the connected account's accessible organization.

## Session storage
The sample client keeps Auth tokens in memory only. Before production, implement secure refresh-token storage using Android Keystore and iOS Keychain via a vetted Unity plugin. Do not store refresh tokens in plain PlayerPrefs.

## Important 2026 Data API behavior
New `public` tables are not necessarily exposed to the Data API automatically. Explicitly verify the exposed-schema setting in the project dashboard, in addition to grants and RLS. RLS protects rows; Data API exposure and SQL privileges are separate controls.
