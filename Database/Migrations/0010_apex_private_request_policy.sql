-- APEX GT WORLD — explicit deny policy for the private race request map.
-- service_role and the function owner retain their trusted server-side access.

drop policy if exists apex_race_session_requests_deny_clients
    on apex_private.apex_race_session_requests;

create policy apex_race_session_requests_deny_clients
on apex_private.apex_race_session_requests
for all
to public
using (false)
with check (false);

revoke all on table apex_private.apex_race_session_requests
    from public, anon, authenticated;
