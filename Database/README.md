# Database Setup

The migration under `Migrations/0001_apex_core.sql` is written for a dedicated, newly provisioned APEX GT WORLD Supabase project. It has not been applied remotely. Do not apply it to existing Qulup or ASIX projects.

After the dedicated project exists:
1. Review the migration.
2. Apply it to a development database.
3. Inspect RLS and Data API grants.
4. Run Supabase security and performance advisors.
5. Add trusted server/Edge Function workflows for wallet posting, purchases, vehicle grants, property/business transfers and race result settlement.
6. Test owner isolation and duplicate idempotency behavior.

The migration does not implement privileged economic functions. That is intentional: a client must never write wallet balances or ledger entries directly.
