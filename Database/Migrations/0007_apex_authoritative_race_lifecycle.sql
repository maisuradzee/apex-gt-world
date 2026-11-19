-- APEX GT WORLD — authoritative race lifecycle primitives.
-- Only trusted race-server code using service_role can start/finalize sessions.

create or replace function public.apex_server_start_race_session(p_session_id uuid)
returns table (
    race_session_id uuid,
    race_status text,
    race_started_at timestamptz,
    was_started boolean
)
language plpgsql
security definer
set search_path = ''
as $$
declare
    v_session public.apex_race_sessions%rowtype;
    v_started boolean := false;
begin
    select * into v_session
    from public.apex_race_sessions
    where id = p_session_id
    for update;

    if not found then
        raise exception using errcode = '22023', message = 'Race session not found';
    end if;

    if v_session.status = 'created' then
        update public.apex_race_sessions
        set status = 'running',
            started_at = coalesce(started_at, now())
        where id = p_session_id
        returning * into v_session;
        v_started := true;
    elsif v_session.status = 'running' then
        v_started := false;
    else
        raise exception using errcode = '22023', message = 'Race session cannot be started from its current state';
    end if;

    return query
    select v_session.id, v_session.status, v_session.started_at, v_started;
end;
$$;

revoke all on function public.apex_server_start_race_session(uuid)
    from public, anon, authenticated;
grant execute on function public.apex_server_start_race_session(uuid)
    to service_role;


create or replace function public.apex_server_finalize_race_session(
    p_session_id uuid,
    p_results jsonb
)
returns table (
    result_id uuid,
    user_id uuid,
    placement integer,
    reward_credits bigint,
    applied boolean
)
language plpgsql
security definer
set search_path = ''
as $$
declare
    v_session public.apex_race_sessions%rowtype;
    v_item jsonb;
    v_user_id uuid;
    v_placement integer;
    v_finish_time_ms bigint;
    v_score integer;
    v_reward_credits bigint;
    v_existing public.apex_race_results%rowtype;
    v_result_id uuid;
    v_count integer;
    v_distinct_users integer;
    v_extra_keys integer;
begin
    if p_results is null or jsonb_typeof(p_results) <> 'array' then
        raise exception using errcode = '22023', message = 'Race results must be a JSON array';
    end if;

    v_count := jsonb_array_length(p_results);
    if v_count < 1 or v_count > 64 then
        raise exception using errcode = '22023', message = 'Race result count must be between 1 and 64';
    end if;

    select count(distinct value ->> 'user_id')
    into v_distinct_users
    from jsonb_array_elements(p_results);

    if v_distinct_users <> v_count then
        raise exception using errcode = '22023', message = 'Each player may appear only once in a race result set';
    end if;

    select * into v_session
    from public.apex_race_sessions
    where id = p_session_id
    for update;

    if not found then
        raise exception using errcode = '22023', message = 'Race session not found';
    end if;

    if v_session.status not in ('running', 'finished') then
        raise exception using errcode = '22023', message = 'Only a running or previously finished session can be finalized';
    end if;

    for v_item in
        select value from jsonb_array_elements(p_results)
    loop
        if jsonb_typeof(v_item) <> 'object'
           or not (v_item ?& array['user_id','placement','score','reward_credits']) then
            raise exception using errcode = '22023', message = 'Each result requires user_id, placement, score and reward_credits';
        end if;

        select count(*) into v_extra_keys
        from jsonb_object_keys(v_item) as item_key(key)
        where key not in ('user_id','placement','finish_time_ms','score','reward_credits');

        if v_extra_keys > 0 then
            raise exception using errcode = '22023', message = 'Race result contains unsupported fields';
        end if;

        if jsonb_typeof(v_item -> 'user_id') <> 'string'
           or jsonb_typeof(v_item -> 'placement') <> 'number'
           or jsonb_typeof(v_item -> 'score') <> 'number'
           or jsonb_typeof(v_item -> 'reward_credits') <> 'number' then
            raise exception using errcode = '22023', message = 'Race result fields have invalid types';
        end if;

        begin
            v_user_id := (v_item ->> 'user_id')::uuid;
            v_placement := (v_item ->> 'placement')::integer;
            v_score := (v_item ->> 'score')::integer;
            v_reward_credits := (v_item ->> 'reward_credits')::bigint;

            if jsonb_typeof(coalesce(v_item -> 'finish_time_ms', 'null'::jsonb)) = 'number' then
                v_finish_time_ms := (v_item ->> 'finish_time_ms')::bigint;
            elsif jsonb_typeof(coalesce(v_item -> 'finish_time_ms', 'null'::jsonb)) = 'null' then
                v_finish_time_ms := null;
            else
                raise exception using errcode = '22023', message = 'finish_time_ms must be a number or null';
            end if;
        exception
            when invalid_text_representation or numeric_value_out_of_range then
                raise exception using errcode = '22023', message = 'Race result contains an invalid numeric or UUID value';
        end;

        if v_placement < 1 or v_score < 0
           or v_reward_credits < 0 or v_reward_credits > 1000000000
           or (v_finish_time_ms is not null and v_finish_time_ms < 1) then
            raise exception using errcode = '22023', message = 'Race result values are outside allowed bounds';
        end if;

        if not exists (select 1 from auth.users u where u.id = v_user_id) then
            raise exception using errcode = '22023', message = 'Race result references an unknown player';
        end if;

        if v_session.status = 'finished' then
            select * into v_existing
            from public.apex_race_results
            where session_id = p_session_id and apex_race_results.user_id = v_user_id;

            if not found
               or v_existing.placement <> v_placement
               or v_existing.finish_time_ms is distinct from v_finish_time_ms
               or v_existing.score <> v_score
               or v_existing.reward_credits <> v_reward_credits
               or v_existing.validated is distinct from true then
                raise exception using errcode = '23505', message = 'Finished race session cannot be finalized with different results';
            end if;
        else
            insert into public.apex_race_results (
                session_id, user_id, placement, finish_time_ms, score, validated, reward_credits
            ) values (
                p_session_id, v_user_id, v_placement, v_finish_time_ms, v_score, true, v_reward_credits
            )
            returning id into v_result_id;
        end if;
    end loop;

    if v_session.status = 'finished' then
        if (select count(*) from public.apex_race_results where session_id = p_session_id) <> v_count then
            raise exception using errcode = '23505', message = 'Finished race session result count does not match the retry payload';
        end if;

        return query
        select r.id, r.user_id, r.placement, r.reward_credits, false
        from public.apex_race_results r
        where r.session_id = p_session_id
        order by r.placement, r.user_id;
        return;
    end if;

    update public.apex_race_sessions
    set status = 'finished',
        finished_at = coalesce(finished_at, now())
    where id = p_session_id;

    return query
    select r.id, r.user_id, r.placement, r.reward_credits, true
    from public.apex_race_results r
    where r.session_id = p_session_id
    order by r.placement, r.user_id;
end;
$$;

revoke all on function public.apex_server_finalize_race_session(uuid, jsonb)
    from public, anon, authenticated;
grant execute on function public.apex_server_finalize_race_session(uuid, jsonb)
    to service_role;
