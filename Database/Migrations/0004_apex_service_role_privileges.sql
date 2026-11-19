-- APEX GT WORLD — trusted backend grants.
-- Clients remain restricted to the narrow grants established by migrations 0001-0003.
-- The service_role is used only by trusted Edge Functions / server-side services.

grant usage on schema public to service_role;
grant all privileges on all tables in schema public to service_role;
grant all privileges on all sequences in schema public to service_role;
grant execute on all functions in schema public to service_role;

grant usage on schema apex_private to service_role;
grant all privileges on all tables in schema apex_private to service_role;

-- Keep trusted backend access when future migrations add new database objects.
alter default privileges for role postgres in schema public
    grant all privileges on tables to service_role;
alter default privileges for role postgres in schema public
    grant all privileges on sequences to service_role;
alter default privileges for role postgres in schema public
    grant execute on functions to service_role;

alter default privileges for role postgres in schema apex_private
    grant all privileges on tables to service_role;
alter default privileges for role postgres in schema apex_private
    grant all privileges on sequences to service_role;
alter default privileges for role postgres in schema apex_private
    grant execute on functions to service_role;
