# ERO V3.1 — GitHub + Unity automatic validation

## One-time setup

1. Create a private GitHub repository.
2. Upload the CONTENTS of this project so `Assets/`, `Packages/`, `ProjectSettings/`
   and `.github/` are at repository root.
3. In GitHub: Settings -> Secrets and variables -> Actions.
4. Add repository secrets:
   - `UNITY_EMAIL`
   - `UNITY_PASSWORD`
5. Open Actions -> ERO Unity Validation -> Run workflow.

## What happens automatically

A preflight job checks:
- Assets/Packages/ProjectSettings
- Unity version 6000.0.67f1
- ERO V3 files
- accidental credentials

Then the Unity job:
- opens the project with Unity 6000.0.67f1
- runs all Unity tests
- creates a Linux validation build
- uploads test/build logs

## Security

Never put Unity credentials, Discord tokens, Steam keys or private keys in source files.
Only use GitHub Actions Secrets.

## If the Unity runner cannot obtain the exact editor version

Do not silently change the project version. Use Unity Build Automation or a self-hosted
runner with Unity 6000.0.67f1 installed. The project target must remain exact unless
we deliberately upgrade it.
