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

# Enforce least-privilege profile writes and keep server session credentials private.
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
required_column_grants = [
    "grant insert (user_id, display_name) on public.apex_profiles to authenticated;",
    "grant update (display_name) on public.apex_profiles to authenticated;",
]
for grant in required_column_grants:
    if grant not in sql.lower():
        raise SystemExit("Missing restricted profile column grant: " + grant)
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

# Keep profile writes aligned with the database's column-level grants.
dto_source = (ROOT / "Assets/Scripts/Backend/SupabaseDtos.cs").read_text(encoding="utf-8")
upsert_match = re.search(r"class ApexProfileUpsert\s*\{([^}]+)\}", dto_source, re.S)
if not upsert_match:
    raise SystemExit("ApexProfileUpsert DTO is missing")
if "home_region" in upsert_match.group(1):
    raise SystemExit("Profile upsert must not send home_region; region changes are server-controlled")
client_source = (ROOT / "Assets/Scripts/Backend/SupabaseRestClient.cs").read_text(encoding="utf-8")
if re.search(r"SaveMyProfile\s*\([^)]*homeRegion", client_source):
    raise SystemExit("SaveMyProfile must not accept a client-controlled home region")
if "home_region = string.IsNullOrEmpty(homeRegion)" in client_source:
    raise SystemExit("Profile payload must not attempt to write the server-controlled home_region field")

# Reject actual credential values in code/config. Placeholders and public publishable keys are permitted.
for base in (ROOT / "Assets", ROOT / "Database", ROOT / ".github"):
    for path in base.rglob("*"):
        if not path.is_file() or path.suffix.lower() not in {".cs", ".sql", ".yml", ".yaml", ".json"}:
            continue
        content = path.read_text(encoding="utf-8", errors="ignore")
        if re.search(r"sb_secret_[A-Za-z0-9_-]{20,}|service_role\s*[:=]\s*['\"][^'\"]{20,}|-----BEGIN (?:RSA |EC )?PRIVATE KEY-----", content, re.I):
            raise SystemExit(f"Potential secret credential found in {path.relative_to(ROOT)}")

print(f"Source checks passed: {len(REQUIRED)} required files, {len(tables)} RLS-enabled tables, valid package JSON.")
