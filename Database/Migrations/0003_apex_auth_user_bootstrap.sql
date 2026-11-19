-- APEX GT WORLD — Auth user bootstrap.
-- Apply after migrations 0001 and 0002 in the dedicated APEX project only.

-- A newly registered Auth user gets a server-created profile, zeroed wallet,
-- and starter progression row in the same transaction as the Auth user insert.
create or replace function public.apex_handle_new_user()
returns trigger
language plpgsql
security definer
set search_path = ''
as $$
declare
    v_display_name text;
begin
    v_display_name := nullif(btrim(coalesce(new.raw_user_meta_data ->> 'display_name', '')), '');

    if v_display_name is null or char_length(v_display_name) not between 3 and 24 then
        v_display_name := 'Driver-' || right(replace(new.id::text, '-', ''), 8);
    end if;

    insert into public.apex_profiles (user_id, display_name)
    values (new.id, v_display_name)
    on conflict (user_id) do nothing;

    insert into public.apex_wallets (user_id)
    values (new.id)
    on conflict (user_id) do nothing;

    insert into public.apex_player_progress (user_id)
    values (new.id)
    on conflict (user_id) do nothing;

    return new;
end;
$$;

-- The function is called by the Auth trigger; do not expose it as an RPC.
revoke all on function public.apex_handle_new_user() from public, anon, authenticated;

drop trigger if exists apex_on_auth_user_created on auth.users;
create trigger apex_on_auth_user_created
after insert on auth.users
for each row execute function public.apex_handle_new_user();

-- Profile creation is server-controlled. Clients can only update display_name.
drop policy if exists apex_profiles_insert_self on public.apex_profiles;
revoke insert (user_id, display_name) on table public.apex_profiles from authenticated;
revoke insert on table public.apex_profiles from authenticated;

-- Backfill Auth users created before this trigger was installed.
insert into public.apex_profiles (user_id, display_name)
select
    u.id,
    case
        when char_length(btrim(coalesce(u.raw_user_meta_data ->> 'display_name', ''))) between 3 and 24
            then btrim(u.raw_user_meta_data ->> 'display_name')
        else 'Driver-' || right(replace(u.id::text, '-', ''), 8)
    end
from auth.users u
on conflict (user_id) do nothing;

insert into public.apex_wallets (user_id)
select u.id from auth.users u
on conflict (user_id) do nothing;

insert into public.apex_player_progress (user_id)
select u.id from auth.users u
on conflict (user_id) do nothing;
