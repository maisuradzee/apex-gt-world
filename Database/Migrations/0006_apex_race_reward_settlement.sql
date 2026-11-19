-- APEX GT WORLD — server-owned race reward amount.
-- The trusted race server writes reward_credits together with validated=true.
-- Client roles have no INSERT/UPDATE privilege on apex_race_results.

alter table public.apex_race_results
    add column if not exists reward_credits bigint not null default 0;

do $$
begin
    if not exists (
        select 1
        from pg_constraint
        where conname = 'apex_race_results_reward_credits_range'
          and conrelid = 'public.apex_race_results'::regclass
    ) then
        alter table public.apex_race_results
            add constraint apex_race_results_reward_credits_range
            check (reward_credits between 0 and 1000000000);
    end if;
end
$$;

comment on column public.apex_race_results.reward_credits is
    'Server-calculated Credits payout. Never supplied by a player client; set only by a trusted race server when validating the result.';
