-- APEX GT WORLD — atomic, idempotent wallet transactions.
-- Execute only from trusted server-side services using the service_role.

create or replace function public.apex_server_apply_wallet_delta(
    p_user_id uuid,
    p_currency text,
    p_amount bigint,
    p_reason text,
    p_idempotency_key text,
    p_metadata jsonb default '{}'::jsonb
)
returns table (
    ledger_id uuid,
    credits bigint,
    premium_tokens bigint,
    applied boolean
)
language plpgsql
security definer
set search_path = ''
as $$
declare
    v_wallet public.apex_wallets%rowtype;
    v_existing public.apex_wallet_ledger%rowtype;
    v_ledger_id uuid;
    v_reason text;
    v_key text;
    v_metadata jsonb;
begin
    if p_user_id is null or not exists (
        select 1 from auth.users u where u.id = p_user_id
    ) then
        raise exception using errcode = '22023', message = 'Unknown player account';
    end if;

    if p_currency is null or p_currency not in ('credits', 'premium_tokens') then
        raise exception using errcode = '22023', message = 'Unsupported wallet currency';
    end if;

    if p_amount is null or p_amount = 0 then
        raise exception using errcode = '22023', message = 'Wallet delta must be non-zero';
    end if;

    v_reason := btrim(coalesce(p_reason, ''));
    if char_length(v_reason) < 1 or char_length(v_reason) > 120 then
        raise exception using errcode = '22023', message = 'Wallet reason must contain 1–120 characters';
    end if;

    v_key := btrim(coalesce(p_idempotency_key, ''));
    if char_length(v_key) < 8 or char_length(v_key) > 128
       or v_key <> p_idempotency_key then
        raise exception using errcode = '22023', message = 'Idempotency key must contain 8–128 non-padded characters';
    end if;

    v_metadata := coalesce(p_metadata, '{}'::jsonb);
    if jsonb_typeof(v_metadata) <> 'object' then
        raise exception using errcode = '22023', message = 'Wallet metadata must be a JSON object';
    end if;

    insert into public.apex_wallets(user_id)
    values (p_user_id)
    on conflict (user_id) do nothing;

    -- Serialize all transactions for this player before checking the idempotency key.
    select * into v_wallet
    from public.apex_wallets
    where user_id = p_user_id
    for update;

    select * into v_existing
    from public.apex_wallet_ledger
    where user_id = p_user_id
      and idempotency_key = v_key;

    if found then
        if v_existing.currency <> p_currency
           or v_existing.amount <> p_amount
           or v_existing.reason <> v_reason
           or v_existing.metadata is distinct from v_metadata then
            raise exception using errcode = '23505', message = 'Idempotency key was reused with a different transaction payload';
        end if;

        return query
        select v_existing.id, v_wallet.credits, v_wallet.premium_tokens, false;
        return;
    end if;

    if p_currency = 'credits' then
        v_wallet.credits := v_wallet.credits + p_amount;
        if v_wallet.credits < 0 then
            raise exception using errcode = 'P0001', message = 'Insufficient Credits balance';
        end if;
        update public.apex_wallets
        set credits = v_wallet.credits, updated_at = now()
        where user_id = p_user_id;
    else
        v_wallet.premium_tokens := v_wallet.premium_tokens + p_amount;
        if v_wallet.premium_tokens < 0 then
            raise exception using errcode = 'P0001', message = 'Insufficient Premium Tokens balance';
        end if;
        update public.apex_wallets
        set premium_tokens = v_wallet.premium_tokens, updated_at = now()
        where user_id = p_user_id;
    end if;

    insert into public.apex_wallet_ledger(
        user_id, currency, amount, reason, idempotency_key, metadata
    ) values (
        p_user_id, p_currency, p_amount, v_reason, v_key, v_metadata
    )
    returning id into v_ledger_id;

    return query
    select v_ledger_id, v_wallet.credits, v_wallet.premium_tokens, true;
end;
$$;

revoke all on function public.apex_server_apply_wallet_delta(uuid, text, bigint, text, text, jsonb)
    from public, anon, authenticated;
grant execute on function public.apex_server_apply_wallet_delta(uuid, text, bigint, text, text, jsonb)
    to service_role;
