-- APEX GT WORLD — initial schema for a NEW dedicated Supabase project.
-- Do not apply to Qulup, ASIX, or any existing production database.
-- Client-facing tables use RLS. Economy writes are intentionally server-only.

create extension if not exists pgcrypto;

create table if not exists public.apex_regions (
    region_id text primary key check (region_id in ('metro_city','coastal_city','mountain_district','motorsport_park')),
    display_name text not null,
    enabled boolean not null default false,
    created_at timestamptz not null default now()
);

insert into public.apex_regions(region_id, display_name, enabled) values
('metro_city','Metro City',true),
('coastal_city','Coastal City',false),
('mountain_district','Mountain District',false),
('motorsport_park','Motorsport Park',false)
on conflict (region_id) do nothing;

create table if not exists public.apex_profiles (
    user_id uuid primary key references auth.users(id) on delete cascade,
    display_name text not null check (char_length(trim(display_name)) between 3 and 24),
    reputation bigint not null default 0 check (reputation >= 0),
    home_region text not null default 'metro_city' references public.apex_regions(region_id),
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create table if not exists public.apex_player_vehicles (
    id uuid primary key default gen_random_uuid(),
    owner_id uuid not null references auth.users(id) on delete cascade,
    vehicle_code text not null,
    display_name text not null,
    drivetrain text not null default 'RWD' check (drivetrain in ('FWD','RWD','AWD')),
    condition_percent numeric(5,2) not null default 100 check (condition_percent between 0 and 100),
    upgrade_data jsonb not null default '{}'::jsonb,
    created_at timestamptz not null default now()
);
create index if not exists apex_player_vehicles_owner_idx on public.apex_player_vehicles(owner_id);

create table if not exists public.apex_player_motorcycles (
    id uuid primary key default gen_random_uuid(),
    owner_id uuid not null references auth.users(id) on delete cascade,
    motorcycle_code text not null,
    display_name text not null,
    condition_percent numeric(5,2) not null default 100 check (condition_percent between 0 and 100),
    upgrade_data jsonb not null default '{}'::jsonb,
    created_at timestamptz not null default now()
);
create index if not exists apex_player_motorcycles_owner_idx on public.apex_player_motorcycles(owner_id);

create table if not exists public.apex_wallets (
    user_id uuid primary key references auth.users(id) on delete cascade,
    credits bigint not null default 0 check (credits >= 0),
    premium_tokens bigint not null default 0 check (premium_tokens >= 0),
    updated_at timestamptz not null default now()
);

create table if not exists public.apex_wallet_ledger (
    id uuid primary key default gen_random_uuid(),
    user_id uuid not null references auth.users(id) on delete cascade,
    currency text not null check (currency in ('credits','premium_tokens')),
    amount bigint not null check (amount <> 0),
    reason text not null check (char_length(trim(reason)) between 1 and 120),
    idempotency_key text not null,
    metadata jsonb not null default '{}'::jsonb,
    created_at timestamptz not null default now(),
    unique (user_id, idempotency_key)
);
create index if not exists apex_wallet_ledger_user_created_idx on public.apex_wallet_ledger(user_id, created_at desc);

create table if not exists public.apex_properties (
    id uuid primary key default gen_random_uuid(),
    owner_id uuid references auth.users(id) on delete set null,
    property_code text not null unique,
    region_id text not null references public.apex_regions(region_id),
    property_type text not null check (property_type in ('apartment','house','garage','commercial')),
    state jsonb not null default '{}'::jsonb,
    created_at timestamptz not null default now()
);
create index if not exists apex_properties_owner_idx on public.apex_properties(owner_id);

create table if not exists public.apex_businesses (
    id uuid primary key default gen_random_uuid(),
    owner_id uuid references auth.users(id) on delete set null,
    business_code text not null unique,
    region_id text not null references public.apex_regions(region_id),
    business_type text not null,
    state jsonb not null default '{}'::jsonb,
    created_at timestamptz not null default now()
);
create index if not exists apex_businesses_owner_idx on public.apex_businesses(owner_id);

create table if not exists public.apex_race_sessions (
    id uuid primary key default gen_random_uuid(),
    region_id text not null references public.apex_regions(region_id),
    discipline text not null check (discipline in ('circuit','time_attack','drift','endurance','motorcycle')),
    status text not null default 'created' check (status in ('created','running','finished','cancelled')),
    started_at timestamptz,
    finished_at timestamptz,
    created_at timestamptz not null default now()
);

-- Keep the session handshake secret outside exposed schemas and away from client roles.
create schema if not exists apex_private;
revoke all on schema apex_private from public, anon, authenticated;

create table if not exists apex_private.apex_race_session_secrets (
    session_id uuid primary key references public.apex_race_sessions(id) on delete cascade,
    server_session_key text not null unique,
    created_at timestamptz not null default now()
);
alter table apex_private.apex_race_session_secrets enable row level security;
revoke all on table apex_private.apex_race_session_secrets from public, anon, authenticated;
grant usage on schema apex_private to service_role;
grant select, insert, update, delete on table apex_private.apex_race_session_secrets to service_role;

create table if not exists public.apex_race_results (
    id uuid primary key default gen_random_uuid(),
    session_id uuid not null references public.apex_race_sessions(id) on delete cascade,
    user_id uuid not null references auth.users(id) on delete cascade,
    placement integer not null check (placement > 0),
    finish_time_ms bigint check (finish_time_ms is null or finish_time_ms > 0),
    score integer not null default 0,
    validated boolean not null default false,
    created_at timestamptz not null default now(),
    unique(session_id, user_id)
);
create index if not exists apex_race_results_user_created_idx on public.apex_race_results(user_id, created_at desc);

create table if not exists public.apex_player_progress (
    user_id uuid primary key references auth.users(id) on delete cascade,
    career_level integer not null default 1 check (career_level > 0),
    career_xp bigint not null default 0 check (career_xp >= 0),
    unlocked_content jsonb not null default '[]'::jsonb,
    updated_at timestamptz not null default now()
);

-- RLS on every exposed public table.
alter table public.apex_regions enable row level security;
alter table public.apex_profiles enable row level security;
alter table public.apex_player_vehicles enable row level security;
alter table public.apex_player_motorcycles enable row level security;
alter table public.apex_wallets enable row level security;
alter table public.apex_wallet_ledger enable row level security;
alter table public.apex_properties enable row level security;
alter table public.apex_businesses enable row level security;
alter table public.apex_race_sessions enable row level security;
alter table public.apex_race_results enable row level security;
alter table public.apex_player_progress enable row level security;

create policy apex_regions_read_authenticated on public.apex_regions
for select to authenticated using (true);

create policy apex_profiles_read_authenticated on public.apex_profiles
for select to authenticated using (true);
create policy apex_profiles_insert_self on public.apex_profiles
for insert to authenticated with check ((select auth.uid()) = user_id);
create policy apex_profiles_update_self on public.apex_profiles
for update to authenticated using ((select auth.uid()) = user_id)
with check ((select auth.uid()) = user_id);

create policy apex_vehicles_read_owner on public.apex_player_vehicles
for select to authenticated using ((select auth.uid()) = owner_id);
create policy apex_motorcycles_read_owner on public.apex_player_motorcycles
for select to authenticated using ((select auth.uid()) = owner_id);
create policy apex_wallet_read_owner on public.apex_wallets
for select to authenticated using ((select auth.uid()) = user_id);
create policy apex_ledger_read_owner on public.apex_wallet_ledger
for select to authenticated using ((select auth.uid()) = user_id);
create policy apex_properties_read_owner on public.apex_properties
for select to authenticated using ((select auth.uid()) = owner_id);
create policy apex_businesses_read_owner on public.apex_businesses
for select to authenticated using ((select auth.uid()) = owner_id);
create policy apex_race_sessions_read_authenticated on public.apex_race_sessions
for select to authenticated using (true);
create policy apex_race_results_read_authenticated on public.apex_race_results
for select to authenticated using (true);
create policy apex_progress_read_owner on public.apex_player_progress
for select to authenticated using ((select auth.uid()) = user_id);

-- Start from zero client privileges for each table. The grants below expose only
-- intended reads and profile display-name changes; authoritative state stays server-controlled.
revoke all on public.apex_regions from public, anon, authenticated;
revoke all on public.apex_profiles from public, anon, authenticated;
revoke all on public.apex_player_vehicles from public, anon, authenticated;
revoke all on public.apex_player_motorcycles from public, anon, authenticated;
revoke all on public.apex_wallets from public, anon, authenticated;
revoke all on public.apex_wallet_ledger from public, anon, authenticated;
revoke all on public.apex_properties from public, anon, authenticated;
revoke all on public.apex_businesses from public, anon, authenticated;
revoke all on public.apex_race_sessions from public, anon, authenticated;
revoke all on public.apex_race_results from public, anon, authenticated;
revoke all on public.apex_player_progress from public, anon, authenticated;


-- Explicit Data API privileges. RLS remains the row-level gate.
grant select on public.apex_regions to authenticated;
grant select on public.apex_profiles to authenticated;
grant insert (user_id, display_name) on public.apex_profiles to authenticated;
grant update (display_name) on public.apex_profiles to authenticated;
grant select on public.apex_player_vehicles to authenticated;
grant select on public.apex_player_motorcycles to authenticated;
grant select on public.apex_wallets to authenticated;
grant select on public.apex_wallet_ledger to authenticated;
grant select on public.apex_properties to authenticated;
grant select on public.apex_businesses to authenticated;
grant select on public.apex_race_sessions to authenticated;
grant select on public.apex_race_results to authenticated;
grant select on public.apex_player_progress to authenticated;

-- No client role receives write privileges for wallet, inventory, ownership,
-- property/business transfers or race settlement. Trusted server workflows
-- must validate identity and business rules before privileged mutations.
