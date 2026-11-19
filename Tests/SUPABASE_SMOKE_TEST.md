# Supabase Smoke Test Checklist

Run these tests against the dedicated APEX GT WORLD project only.

## Auth and player bootstrap
- [ ] Confirm project ref is `hzawgrvonsqyqjyzzreg`.
- [ ] Confirm `public` is exposed in Data API settings.
- [ ] Confirm the 11 public APEX tables have RLS enabled and the private schema is not exposed.
- [ ] Sign up using `SupabaseRestClient.SignUpWithPassword` with a valid display name.
- [ ] Complete email confirmation if enabled, then sign in.
- [ ] Verify the Auth trigger created exactly one profile, wallet and progression row. Do not INSERT a profile from the client.
- [ ] Fetch own profile and world-region catalogue.
- [ ] Fetch own wallet and career progress; verify balances and XP.
- [ ] Attempt to read a second account's wallet/progress and confirm RLS returns no rows.
- [ ] Attempt client writes to wallet, ledger, career XP and race results and confirm they are denied.
- [ ] Confirm refresh/sign-out behavior.

## Race reward settlement
- [ ] A trusted race server creates the race result, sets `validated=true`, writes server-calculated `reward_credits`, and finishes the session.
- [ ] From the signed-in player's client call `POST /functions/v1/settle-race-result` with only `race_result_id`.
- [ ] Confirm the player's Credits balance increases by the stored reward amount.
- [ ] Repeat the same claim and confirm `applied=false` and no second ledger credit.
- [ ] Try another player's result ID and confirm the endpoint returns not found.
- [ ] Try an unvalidated result, an unfinished session, a zero reward, and an extra `amount` request field; each must be rejected.
- [ ] Verify no client role can INSERT/UPDATE race results or call the wallet RPC.

Do not test economy writes using publishable client credentials. Wallet changes and race-result validation remain trusted-server operations.
