# Supabase Smoke Test Checklist

Run these tests against the dedicated APEX GT WORLD project only.

## Auth and player bootstrap
- [ ] Confirm project ref and Data API exposure.
- [ ] Confirm all 11 public APEX tables have RLS enabled and private schema is not exposed.
- [ ] Sign up; verify Auth trigger creates exactly one profile, wallet and progression row.
- [ ] Fetch own profile, regions, Wallet and Career.
- [ ] Verify Player A cannot read Player B's private Wallet/Career.
- [ ] Verify client cannot write wallet, career or race-result rows.
- [ ] Confirm refresh/sign-out.

## Race control (dedicated server only)
- [ ] Set the same strong `APEX_RACE_SERVER_TOKEN` in GTWORLD Edge Function Secrets and the dedicated race-server host environment.
- [ ] Configure `APEX_RACE_CONTROL_URL`.
- [ ] Call create; verify session status `created`.
- [ ] Call start; verify session status `running`; repeat start safely.
- [ ] Run the authoritative simulation and calculate results/payout on the server.
- [ ] Call finalize; verify `validated=true` result rows and session `finished`.
- [ ] Retry identical finalize; verify no duplicate result.
- [ ] Try changed placement/score/payout after finish; verify rejection.
- [ ] Verify no player JWT, anon or authenticated role can call lifecycle RPCs.

## Player reward claim
- [ ] Signed-in player calls `POST /functions/v1/settle-race-result` with only `race_result_id`.
- [ ] Confirm Credits increase by server-approved payout.
- [ ] Repeat claim; verify `applied=false` and no duplicate ledger credit.
- [ ] Test another player's result, unvalidated result, unfinished session and extra amount field; all must be rejected.

Do not use publishable client credentials for economy writes or race lifecycle controls.
