# GitHub repository status

Repository: https://github.com/maisuradzee/apex-gt-world  
Default branch: `main`  
Connected write account: `maisuradzee`

The ChatGPT Codex Connector GitHub App is installed for the `maisuradzee` account with selected-repository access that includes `apex-gt-world`. Repository metadata reports admin and push permissions, and a Git Data blob write test succeeded.

The initial source package is stored as `APEX_GT_WORLD_Source.tar.xz`. The workflow `.github/workflows/unpack-source.yml` expands the package into the root repository and pushes a second commit. It requires GitHub Actions to be enabled and the repository Actions setting to allow the workflow's `contents: write` permission. If automatic extraction is blocked, download the archive and extract it into a local clone without replacing the `.git` directory.

## Local workflow
```bash
git clone https://github.com/maisuradzee/apex-gt-world.git
cd apex-gt-world
tar -xJf APEX_GT_WORLD_Source.tar.xz
python3 Tests/validate_source.py
```

## Credentials
Do not commit Supabase secret/service-role keys, database passwords, signing keys, refresh tokens, production credentials, or server secrets. Store server-only values in a trusted secret manager.
