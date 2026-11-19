from pathlib import Path
import json
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
REQUIRED = [
    "README.md",
    "Documentation/ARCHITECTURE.md",
    "Assets/Scripts/VehiclePhysics/VehicleController.cs",
    "Assets/Scripts/MotorcyclePhysics/MotorcyclePrototypeController.cs",
    "Assets/Editor/StarterSceneGenerator.cs",
    "Database/Migrations/0001_apex_core.sql",
    "Database/Migrations/0002_apex_security_and_region_indexes.sql",
    "Database/Migrations/0003_apex_auth_user_bootstrap.sql",
    "Database/Migrations/0004_apex_service_role_privileges.sql",
    "Database/Migrations/0005_apex_wallet_transactions.sql",
    "Database/Migrations/0006_apex_race_reward_settlement.sql",
    "Database/Migrations/0007_apex_authoritative_race_lifecycle.sql",
    "Database/Migrations/0008_apex_race_result_integer_validation.sql",
    "Database/Migrations/0009_apex_idempotent_race_session_creation.sql",
    "Database/Migrations/0010_apex_private_request_policy.sql",
    "supabase/functions/settle-race-result/index.ts",
    "supabase/functions/settle-race-result/deno.json",
]

for relative in REQUIRED:
    if not (ROOT / relative).is_file():
        raise SystemExit(f"Missing required file: {relative}")

with (ROOT / "Packages/manifest.json").open(encoding="utf-8") as f:
    manifest = json.load(f)
if not isinstance(manifest.get("dependencies"), dict):
    raise SystemExit("Packages/manifest.json must contain a dependencies object")

sql = (ROOT / "Database/Migrations/0001_apex_core.sql").read_text(encoding="utf-8")
tables = re.findall(r"create\s+table\s+if\s+not\s+exists\s+public\.(apex_[a-z0-9_]+)", sql, re.I)
rls_tables = re.findall(r"alter\s+table\s+public\.(apex_[a-z0-9_]+)\s+enable\s+row\s+level\s+security", sql, re.I)
if not tables:
    raise SystemExit("No APEX tables found in SQL migration")
missing_rls = sorted(set(tables) - set(rls_tables))
if missing_rls:
    raise SystemExit("RLS not enabled on: " + ", ".join(missing_rls))
if "grant select" not in sql.lower() or "to authenticated" not in sql.lower():
    raise SystemExit("Expected explicit authenticated Data API grants")

# The server-session handshake secret must never be in the exposed race table.
race_prefix = "create table if not exists public.apex_race_sessions"
if race_prefix not in sql.lower():
    raise SystemExit("Public race sessions table is missing")
race_definition = sql.lower().split(race_prefix, 1)[1].split(");", 1)[0]
if "server_session_key" in race_definition:
    raise SystemExit("server_session_key must not be stored in the exposed race sessions table")
if "create table if not exists apex_private.apex_race_session_secrets" not in sql.lower():
    raise SystemExit("Private race-session secret table is missing")
if "alter table apex_private.apex_race_session_secrets enable row level security" not in sql.lower():
    raise SystemExit("RLS must be enabled on the private race-session secret table")
for table in tables:
    expected_revoke = f"revoke all on public.{table} from public, anon, authenticated;"
    if expected_revoke not in sql.lower():
        raise SystemExit(f"Client grants must be reset before explicit grants on {table}")
if "grant update (display_name) on public.apex_profiles to authenticated;" not in sql.lower():
    raise SystemExit("Authenticated should have the intended profile display-name update grant")
if "grant select, insert, update on public.apex_profiles to authenticated;" in sql.lower():
    raise SystemExit("Broad profile table write grants are forbidden")
if "grant select, insert, update, delete on table apex_private.apex_race_session_secrets to service_role;" not in sql.lower():
    raise SystemExit("Trusted service role is missing private session-secret privileges")

followup_sql = (ROOT / "Database/Migrations/0002_apex_security_and_region_indexes.sql").read_text(encoding="utf-8").lower()
required_followup = [
    "create index if not exists apex_profiles_home_region_idx",
    "create index if not exists apex_businesses_region_idx",
    "create index if not exists apex_properties_region_idx",
    "create index if not exists apex_race_sessions_region_idx",
    "create policy apex_race_session_secrets_deny_clients",
    "using (false)",
    "with check (false)",
]
for fragment in required_followup:
    if fragment not in followup_sql:
        raise SystemExit("Missing security/performance follow-up migration statement: " + fragment)

# Auth bootstrap is a trusted, idempotent trigger and is not exposed as an RPC.
auth_sql = (ROOT / "Database/Migrations/0003_apex_auth_user_bootstrap.sql").read_text(encoding="utf-8").lower()
required_auth = [
    "create or replace function public.apex_handle_new_user()",
    "security definer",
    "set search_path = ''",
    "after insert on auth.users",
    "insert into public.apex_profiles",
    "insert into public.apex_wallets",
    "insert into public.apex_player_progress",
    "drop policy if exists apex_profiles_insert_self",
    "revoke insert (user_id, display_name) on table public.apex_profiles from authenticated",
    "revoke all on function public.apex_handle_new_user() from public, anon, authenticated",
]
for fragment in required_auth:
    if fragment not in auth_sql:
        raise SystemExit("Missing Auth bootstrap security statement: " + fragment)

# Match the client request shape to the server-controlled signup and profile grants.
dto_source = (ROOT / "Assets/Scripts/Backend/SupabaseDtos.cs").read_text(encoding="utf-8")
update_match = re.search(r"class ApexProfileUpdate\s*\{([^}]+)\}", dto_source, re.S)
if not update_match or "display_name" not in update_match.group(1) or "user_id" in update_match.group(1) or "home_region" in update_match.group(1):
    raise SystemExit("ApexProfileUpdate must contain only display_name")

client_source = (ROOT / "Assets/Scripts/Backend/SupabaseRestClient.cs").read_text(encoding="utf-8")
save_start = client_source.find("public IEnumerator SaveMyProfile")
save_end = client_source.find("public IEnumerator FetchRegions", save_start)
if save_start < 0 or save_end < 0:
    raise SystemExit("SaveMyProfile method not found")
save_block = client_source[save_start:save_end]
if '"PATCH"' not in save_block or "user_id=eq." not in save_block or "ApexProfileUpdate" not in save_block:
    raise SystemExit("SaveMyProfile must use owner-filtered PATCH with a display-name-only payload")
if '"POST"' in save_block or "on_conflict" in save_block or "ApexProfileUpsert" in save_block:
    raise SystemExit("Client profile path must not insert or upsert profile rows")

signup_start = client_source.find("public IEnumerator SignUpWithPassword")
signup_end = client_source.find("public IEnumerator RefreshSession", signup_start)
if signup_start < 0 or signup_end < 0 or "SupabaseSignUpRequest" not in client_source[signup_start:signup_end]:
    raise SystemExit("Sign-up must send display_name metadata for the Auth bootstrap trigger")
if "SupabaseSignUpMetadata" not in dto_source:
    raise SystemExit("Sign-up metadata DTO is missing")

# Trusted backend access must be granted explicitly; client grants remain narrow.
service_sql = (ROOT / "Database/Migrations/0004_apex_service_role_privileges.sql").read_text(encoding="utf-8").lower()
required_service_grants = [
    "grant usage on schema public to service_role",
    "grant all privileges on all tables in schema public to service_role",
    "grant all privileges on all sequences in schema public to service_role",
    "grant execute on all functions in schema public to service_role",
    "grant usage on schema apex_private to service_role",
    "grant all privileges on all tables in schema apex_private to service_role",
    "alter default privileges for role postgres in schema public",
    "grant all privileges on tables to service_role",
    "alter default privileges for role postgres in schema apex_private",
]
for fragment in required_service_grants:
    if fragment not in service_sql:
        raise SystemExit("Missing trusted backend privilege statement: " + fragment)

# Scene generators must add their scene without discarding other enabled build scenes.
scene_source = (ROOT / "Assets/Editor/StarterSceneGenerator.cs").read_text(encoding="utf-8")
if scene_source.count("RegisterSceneInBuildSettings(") < 3:
    raise SystemExit("Both starter scene generators must register scenes through the preserving helper")
if "EditorBuildSettings.scenes = new[]" in scene_source:
    raise SystemExit("Scene generation must not replace the existing Build Settings scene list")

# Prevent cross-account debug flows from retaining an unrelated in-memory session.
if "Sign out before switching accounts." not in client_source:
    raise SystemExit("Sign-in must reject account switching while a session is active")
if "Sign out before creating a new account." not in client_source:
    raise SystemExit("Sign-up must reject account creation while another session is active")
refresh_block = client_source[client_source.find("public IEnumerator RefreshSession"):client_source.find("public IEnumerator SignOut")]
if "invalid refreshed session" not in refresh_block:
    raise SystemExit("RefreshSession must reject invalid Auth responses")

# Wallet settlement must be atomic, idempotent, and callable only by trusted backend code.
wallet_sql = (ROOT / "Database/Migrations/0005_apex_wallet_transactions.sql").read_text(encoding="utf-8").lower()
required_wallet = [
    "create or replace function public.apex_server_apply_wallet_delta(",
    "security definer",
    "set search_path = ''",
    "for update",
    "from public.apex_wallet_ledger",
    "idempotency_key = v_key",
    "insufficient credits balance",
    "insufficient premium tokens balance",
    "revoke all on function public.apex_server_apply_wallet_delta(uuid, text, bigint, text, text, jsonb)",
    "from public, anon, authenticated",
    "grant execute on function public.apex_server_apply_wallet_delta(uuid, text, bigint, text, text, jsonb)",
    "to service_role",
]
for fragment in required_wallet:
    if fragment not in wallet_sql:
        raise SystemExit("Missing wallet transaction safety statement: " + fragment)
if "grant execute on function public.apex_server_apply_wallet_delta(uuid, text, bigint, text, text, jsonb) to authenticated" in wallet_sql:
    raise SystemExit("Wallet transaction RPC must never be executable by authenticated clients")

# Wallet and career read requests are owner-scoped GET-only operations.
wallet_start = client_source.find("public IEnumerator FetchMyWallet")
wallet_end = client_source.find("public IEnumerator FetchMyProgress", wallet_start)
if wallet_start < 0 or wallet_end < 0:
    raise SystemExit("FetchMyWallet method is missing")
wallet_block = client_source[wallet_start:wallet_end]
if '"/rest/v1/apex_wallets?select=user_id,credits,premium_tokens,updated_at&user_id=eq."' not in wallet_block:
    raise SystemExit("Wallet read must select only safe balance fields and filter by signed-in user_id")
if "UnityWebRequest.kHttpVerbGET" not in wallet_block or '"POST"' in wallet_block or '"PATCH"' in wallet_block:
    raise SystemExit("Wallet client operation must remain read-only")

progress_start = wallet_end
progress_end = client_source.find("public IEnumerator FetchRegions", progress_start)
if progress_end < 0:
    raise SystemExit("FetchRegions method not found after player progress reader")
progress_block = client_source[progress_start:progress_end]
if '"/rest/v1/apex_player_progress?select=user_id,career_level,career_xp,unlocked_content,updated_at&user_id=eq."' not in progress_block:
    raise SystemExit("Career progress read must be owner-filtered")
if "UnityWebRequest.kHttpVerbGET" not in progress_block or '"POST"' in progress_block or '"PATCH"' in progress_block:
    raise SystemExit("Career progress client operation must remain read-only")

if "public sealed class ApexWallet" not in dto_source or "public long credits;" not in dto_source or "public long premium_tokens;" not in dto_source:
    raise SystemExit("ApexWallet DTO is incomplete")
if "public sealed class ApexPlayerProgress" not in dto_source or "public int career_level;" not in dto_source or "public long career_xp;" not in dto_source or "public string[] unlocked_content;" not in dto_source:
    raise SystemExit("ApexPlayerProgress DTO is incomplete")

# Race reward settlement trusts only server-validated result data, never client-supplied amounts.
race_sql = (ROOT / "Database/Migrations/0006_apex_race_reward_settlement.sql").read_text(encoding="utf-8").lower()
for fragment in [
    "add column if not exists reward_credits bigint not null default 0",
    "check (reward_credits between 0 and 1000000000)",
    "trusted race server",
]:
    if fragment not in race_sql:
        raise SystemExit("Missing server-owned race reward database rule: " + fragment)

race_function = (ROOT / "supabase/functions/settle-race-result/index.ts").read_text(encoding="utf-8")
required_race_function = [
    'request.headers.get("Authorization")',
    "auth.getUser(jwt)",
    '.eq("user_id", userData.user.id)',
    "raceResult.validated !== true",
    'session.status !== "finished"',
    'body.race_result_id',
    'Object.keys(body).some((key) => key !== "race_result_id")',
    'admin.rpc(',
    '"apex_server_apply_wallet_delta"',
    'p_amount: rewardCredits',
    'p_idempotency_key: idempotencyKey',
    'race-result:${raceResult.id}:credits',
]
for fragment in required_race_function:
    if fragment not in race_function:
        raise SystemExit("Missing race-reward settlement security check: " + fragment)
if "body.amount" in race_function or "body.user_id" in race_function or "body.currency" in race_function:
    raise SystemExit("Race reward endpoint must not accept amount, currency, or player identity from the client")

# Race reward claim client sends only a result ID to the JWT-protected Edge Function.
claim_start = client_source.find("public IEnumerator ClaimRaceReward")
claim_end = client_source.find("public IEnumerator FetchRegions", claim_start)
if claim_start < 0 or claim_end < 0:
    raise SystemExit("ClaimRaceReward method is missing")
claim_block = client_source[claim_start:claim_end]
if '"/functions/v1/settle-race-result"' not in claim_block or "UnityWebRequest.kHttpVerbPOST" not in claim_block:
    raise SystemExit("Race reward claim must call the deployed Edge Function with POST")
if "race_result_id" not in claim_block or "body.amount" in claim_block or "p_amount" in claim_block:
    raise SystemExit("Race reward client may send only the result ID, never a payout amount")
if "client.ClaimRaceReward" not in (ROOT / "Assets/Scripts/Backend/SupabaseDebugPanel.cs").read_text(encoding="utf-8"):
    raise SystemExit("Supabase debug panel must expose the race reward test action")

# Race lifecycle RPCs are authoritative, atomic, idempotent and server-only.
race_lifecycle_sql = (ROOT / "Database/Migrations/0007_apex_authoritative_race_lifecycle.sql").read_text(encoding="utf-8").lower()
required_lifecycle = [
    "create or replace function public.apex_server_start_race_session",
    "for update",
    "set status = 'running'",
    "create or replace function public.apex_server_finalize_race_session",
    "jsonb_typeof(p_results) <> 'array'",
    "v_session.status = 'finished'",
    "v_existing.reward_credits <> v_reward_credits",
    "validated, reward_credits",
    "set status = 'finished'",
    "from public, anon, authenticated",
    "to service_role",
]
for fragment in required_lifecycle:
    if fragment not in race_lifecycle_sql:
        raise SystemExit("Missing authoritative race lifecycle safety statement: " + fragment)
if "grant execute on function public.apex_server_finalize_race_session(uuid, jsonb) to authenticated" in race_lifecycle_sql:
    raise SystemExit("Race finalization must never be executable by client roles")

# The standalone race-server helper must keep its shared secret outside the Unity client.
race_client = (ROOT / "Server/RaceServer/RaceControlClient.cs").read_text(encoding="utf-8")
for fragment in [
    'Environment.GetEnvironmentVariable("APEX_RACE_CONTROL_URL")',
    'Environment.GetEnvironmentVariable("APEX_RACE_SERVER_TOKEN")',
    '"x-apex-race-server-token"',
    "CreateSessionAsync",
    "StartSessionAsync",
    "FinalizeSessionAsync",
    'JsonPropertyName("reward_credits")',
]:
    if fragment not in race_client:
        raise SystemExit("Missing dedicated race-server control client statement: " + fragment)
if "SUPABASE_SERVICE_ROLE_KEY" in race_client or "sb_secret_" in race_client:
    raise SystemExit("Service-role or secret API keys must not be embedded in the race-server helper source")

# Race control Edge Function is protected by the separate trusted-server token.
race_control_source = (ROOT / "supabase/functions/race-session-control/index.ts").read_text(encoding="utf-8")
required_control = [
    'Deno.env.get("APEX_RACE_SERVER_TOKEN")',
    'request.headers.get("x-apex-race-server-token")',
    "crypto.subtle.digest",
    'operation === "create"',
    'operation === "start"',
    'operation === "finalize"',
    "apex_server_start_race_session",
    "apex_server_finalize_race_session",
    "SUPABASE_SERVICE_ROLE_KEY",
]
for fragment in required_control:
    if fragment not in race_control_source:
        raise SystemExit("Missing protected race control operation: " + fragment)
if "auth.getUser(" in race_control_source:
    raise SystemExit("Race control must not use player JWT as its server authentication mechanism")

# Race finalization rejects fractional results and duplicate player placements.
race_hardening_sql = (ROOT / "Database/Migrations/0008_apex_race_result_integer_validation.sql").read_text(encoding="utf-8").lower()
for fragment in [
    "create unique index if not exists apex_race_results_session_placement_uidx",
    "v_distinct_placements",
    "each player must have a unique race placement",
    "placement, score and reward_credits must be whole numbers",
    "finish_time_ms must be a whole number",
]:
    if fragment not in race_hardening_sql:
        raise SystemExit("Missing strict race result validation statement: " + fragment)

# Session creation is idempotent on a stable request UUID and protected by the service role.
creation_sql = (ROOT / "Database/Migrations/0009_apex_idempotent_race_session_creation.sql").read_text(encoding="utf-8").lower()
for fragment in [
    "create table if not exists apex_private.apex_race_session_requests",
    "enable row level security",
    "pg_advisory_xact_lock",
    "hashtextextended",
    "apex_server_create_race_session",
    "race creation request key was reused with different parameters",
    "from public, anon, authenticated",
    "to service_role",
]:
    if fragment not in creation_sql:
        raise SystemExit("Missing idempotent race-session creation safety statement: " + fragment)

control_for_create = (ROOT / "supabase/functions/race-session-control/index.ts").read_text(encoding="utf-8")
if '"request_key"' not in control_for_create or '"apex_server_create_race_session"' not in control_for_create:
    raise SystemExit("Race control create operation must call the idempotent create-session RPC")

race_client_for_create = (ROOT / "Server/RaceServer/RaceControlClient.cs").read_text(encoding="utf-8")
if "Guid requestKey" not in race_client_for_create or 'JsonPropertyName("request_key")' not in race_client_for_create:
    raise SystemExit("Race-server create API must send a stable idempotency request key")

# Private race request mapping is explicitly denied to ordinary roles under RLS.
private_policy_sql = (ROOT / "Database/Migrations/0010_apex_private_request_policy.sql").read_text(encoding="utf-8").lower()
for fragment in [
    "create policy apex_race_session_requests_deny_clients",
    "on apex_private.apex_race_session_requests",
    "for all",
    "to public",
    "using (false)",
    "with check (false)",
    "revoke all on table apex_private.apex_race_session_requests",
]:
    if fragment not in private_policy_sql:
        raise SystemExit("Missing explicit private race-request deny policy: " + fragment)

# Reject actual credential values in code/config. Public publishable keys are allowed.
for base in (ROOT / "Assets", ROOT / "Database", ROOT / ".github", ROOT / "Server", ROOT / "supabase"):
    for path in base.rglob("*"):
        if not path.is_file() or path.suffix.lower() not in {".cs", ".ts", ".sql", ".yml", ".yaml", ".json"}:
            continue
        content = path.read_text(encoding="utf-8", errors="ignore")
        if re.search(r"sb_secret_[A-Za-z0-9_-]{20,}|service_role\s*[:=]\s*['\"][^'\"]{20,}|-----BEGIN (?:RSA |EC )?PRIVATE KEY-----", content, re.I):
            raise SystemExit(f"Potential secret credential found in {path.relative_to(ROOT)}")

print(f"Source checks passed: {len(REQUIRED)} required files, {len(tables)} public RLS tables, secure Auth bootstrap, valid package JSON.")
