-- APEX GT WORLD — idempotent race session creation.
-- Request keys and their mapping are kept in the non-exposed private schema.

create table if not exists apex_private.apex_race_session_requests (
    request_key uuid primary key,
    session_id uuid not null unique
        references public.apex_race_sessions(id) on delete cascade,
    region_id text not null references public.apex_regions(region_id),
    discipline text not null
        check (discipline in ('circuit','time_attack','drift','endurance','motorcycle')),
    created_at timestamptz not null default now()
);

alter table apex_private.apex_race_session_requests enable row level security;
revoke all on table apex_private.apex_race_session_requests from public, anon, authenticated;
grant usage on schema apex_private to service_role;
grant select, insert, update, delete on table apex_private.apex_race_session_requests to service_role;

create or replace function public.apex_server_create_race_session(
    p_region_id text,
    p_discipline text,
    p_request_key uuid
)
returns table (
    race_session_id uuid,
    region_id text,
    discipline text,
    race_status text,
    created_at timestamptz,
    applied boolean
)
language plpgsql
security definer
set search_path = ''
as $$
declare
    v_existing apex_private.apex_race_session_requests%rowtype;
    v_session public.apex_race_sessions%rowtype;
begin
    if p_request_key is null then
        raise exception using errcode = '22023', message = 'Race creation request key is required';
    end if;

    if p_region_id is null or char_length(btrim(p_region_id)) < 1
       or char_length(p_region_id) > 80 then
        raise exception using errcode = '22023', message = 'A valid race region is required';
    end if;

    if p_discipline is null
       or p_discipline not in ('circuit','time_attack','drift','endurance','motorcycle') then
        raise exception using errcode = '22023', message = 'Unsupported race discipline';
    end if;

    if not exists (
        select 1 from public.apex_regions r
        where r.region_id = p_region_id and r.enabled = true
    ) then
        raise exception using errcode = '22023', message = 'Race region is not enabled';
    end if;

    -- Serialize only calls using the same client-generated request key.
    perform pg_catalog.pg_advisory_xact_lock(
        pg_catalog.hashtextextended(p_request_key::text, 0)
    );

    select * into v_existing
    from apex_private.apex_race_session_requests
    where request_key = p_request_key
    for update;

    if found then
        if v_existing.region_id <> p_region_id
           or v_existing.discipline <> p_discipline then
            raise exception using errcode = '23505', message = 'Race creation request key was reused with different parameters';
        end if;

        select * into v_session
        from public.apex_race_sessions
        where id = v_existing.session_id;

        if not found then
            raise exception using errcode = 'P0001', message = 'Race session request mapping is inconsistent';
        end if;

        return query
        select v_session.id, v_session.region_id, v_session.discipline,
               v_session.status, v_session.created_at, false;
        return;
    end if;

    insert into public.apex_race_sessions(region_id, discipline, status)
    values (p_region_id, p_discipline, 'created')
    returning * into v_session;

    insert into apex_private.apex_race_session_requests(
        request_key, session_id, region_id, discipline
    ) values (
        p_request_key, v_session.id, p_region_id, p_discipline
    );

    return query
    select v_session.id, v_session.region_id, v_session.discipline,
           v_session.status, v_session.created_at, true;
end;
$$;

revoke all on function public.apex_server_create_race_session(text, text, uuid)
    from public, anon, authenticated;
grant execute on function public.apex_server_create_race_session(text, text, uuid)
    to service_role;
