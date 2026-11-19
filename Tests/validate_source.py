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

# Reject actual credential values in code/config. Public publishable keys are allowed.
for base in (ROOT / "Assets", ROOT / "Database", ROOT / ".github"):
    for path in base.rglob("*"):
        if not path.is_file() or path.suffix.lower() not in {".cs", ".sql", ".yml", ".yaml", ".json"}:
            continue
        content = path.read_text(encoding="utf-8", errors="ignore")
        if re.search(r"sb_secret_[A-Za-z0-9_-]{20,}|service_role\s*[:=]\s*['\"][^'\"]{20,}|-----BEGIN (?:RSA |EC )?PRIVATE KEY-----", content, re.I):
            raise SystemExit(f"Potential secret credential found in {path.relative_to(ROOT)}")

print(f"Source checks passed: {len(REQUIRED)} required files, {len(tables)} public RLS tables, secure Auth bootstrap, valid package JSON.")
