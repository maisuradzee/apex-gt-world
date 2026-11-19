# Supabase Smoke Test Checklist

Run only after the target project is visible to the authorized Supabase connection and migration has been applied.

- [ ] Confirm project ref and URL match the intended APEX GT WORLD project.
- [ ] Confirm `public` is exposed in Data API settings.
- [ ] Confirm all 11 APEX tables have RLS enabled.
- [ ] Register a test account through `SupabaseRestClient.SignUpWithPassword`.
- [ ] Complete email confirmation if enabled, then sign in.
- [ ] Create a profile using the authenticated user's UUID.
- [ ] Fetch the profile and verify the returned user ID matches the session user.
- [ ] Fetch the region catalogue.
- [ ] Attempt to read another user's vehicle/wallet data and confirm RLS blocks it.
- [ ] Attempt a client wallet write and confirm it is denied.
- [ ] Confirm refresh/sign-out behavior.

Do not test economy writes using publishable client credentials. Those are server-only operations.
