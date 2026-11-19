-- APEX GT WORLD — security policy and foreign-key index follow-up.
-- Apply after 0001_apex_core.sql in the dedicated APEX GT WORLD project.

-- Explicit deny policy is defense in depth for client roles. The schema and table
-- privileges are also revoked; the trusted service_role bypasses RLS.
create policy apex_race_session_secrets_deny_clients
on apex_private.apex_race_session_secrets
for all
to anon, authenticated
using (false)
with check (false);

-- Cover foreign keys used for region filtering and referential operations.
create index if not exists apex_profiles_home_region_idx
    on public.apex_profiles(home_region);

create index if not exists apex_businesses_region_idx
    on public.apex_businesses(region_id);

create index if not exists apex_properties_region_idx
    on public.apex_properties(region_id);

create index if not exists apex_race_sessions_region_idx
    on public.apex_race_sessions(region_id);
