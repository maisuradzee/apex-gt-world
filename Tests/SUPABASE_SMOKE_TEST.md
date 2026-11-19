# Supabase Smoke Test Checklist

Run these tests against the dedicated APEX GT WORLD project only.

## Auth and player bootstrap
- [ ] Confirm project ref is `hzawgrvonsqyqjyzzreg`.
- [ ] Confirm `public` is exposed in Data API settings.
- [ ] Confirm all 11 public APEX tables have RLS enabled and the private schema is not exposed.
- [ ] Sign up with `SupabaseRestClient.SignUpWithPassword` and a valid display name; complete email confirmation if enabled.
- [ ] Verify the Auth trigger created exactly one profile, wallet and progression row. Do not INSERT a profile from the client.
- [ ] Fetch own profile, world regions, Wallet and Career Progress.
- [ ] Attempt to read a second account's wallet/progress and confirm RLS returns no rows.
- [ ] Attempt client writes to wallet, ledger, career XP and race results and confirm they are denied.
- [ ] Confirm refresh/sign-out behavior.

## Authoritative race lifecycle (trusted race server only)
- [ ] Create a race session in `created` state using trusted backend credentials.
- [ ] Call `apex_server_start_race_session(session_id)`; verify state becomes `running`.
- [ ] Send the authoritative result set to `apex_server_finalize_race_session(session_id, results_json)`.
- [ ] Verify result rows have `validated=true`, server-calculated `reward_credits` and the session is `finished`.
- [ ] Retry the exact same finalization payload; verify it returns existing rows with `applied=false` and no duplicates.
- [ ] Retry with a changed placement, score or payout; verify the finished result set cannot be overwritten.
- [ ] Confirm `anon` and `authenticated` cannot execute either lifecycle RPC.

## Race reward settlement (player client)
- [ ] Signed-in player calls `POST /functions/v1/settle-race-result` with only `race_result_id`.
- [ ] Confirm Credits increase by the stored server-approved reward.
- [ ] Repeat the same claim and confirm `applied=false` and no second ledger credit.
- [ ] Try another player's result ID, an unvalidated result, an unfinished session, a zero reward and an extra `amount` field; each must be rejected.

Do not test economy writes using publishable client credentials. Wallet changes and race-result validation remain trusted-server operations.
