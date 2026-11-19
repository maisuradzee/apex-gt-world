# GitHub repository status

Repository: https://github.com/maisuradzee/apex-gt-world  
Default branch: `main`  
Connected write account: `maisuradzee`

The ChatGPT Codex Connector is installed on the `maisuradzee` account with selected-repository access including `apex-gt-world`. Repository metadata confirms admin and push permission. Git Data blob write and branch update operations were verified.

## Source upload
The initial Unity source package was committed and expanded into the repository root. GitHub Actions assembled the four-part compressed archive, extracted the files, ran `Tests/validate_source.py`, and committed the expanded tree successfully.

## CI
- `.github/workflows/expand-chunked-source.yml` is the bootstrap archive-expansion workflow.
- `.github/workflows/validate-source.yml` validates required source structure, package JSON, and the SQL/RLS baseline on pushes and pull requests.

## Local checks
```bash
python3 Tests/validate_source.py
```

## Credentials
Do not commit Supabase secret/service-role keys, database passwords, signing keys, refresh tokens, production credentials, or server secrets. Store server-only values in a trusted secret manager.
