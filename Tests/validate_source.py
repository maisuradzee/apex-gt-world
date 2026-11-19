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

# Reject actual credential values in code/config. Placeholders and public publishable keys are permitted.
for base in (ROOT / "Assets", ROOT / "Database", ROOT / ".github"):
    for path in base.rglob("*"):
        if not path.is_file() or path.suffix.lower() not in {".cs", ".sql", ".yml", ".yaml", ".json"}:
            continue
        content = path.read_text(encoding="utf-8", errors="ignore")
        if re.search(r"sb_secret_[A-Za-z0-9_-]{20,}|service_role\s*[:=]\s*['\"][^'\"]{20,}|-----BEGIN (?:RSA |EC )?PRIVATE KEY-----", content, re.I):
            raise SystemExit(f"Potential secret credential found in {path.relative_to(ROOT)}")

print(f"Source checks passed: {len(REQUIRED)} required files, {len(tables)} RLS-enabled tables, valid package JSON.")
