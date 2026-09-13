# ERO GitHub Automation V3

This version is designed for the persistent self-hosted `ERO-PC` runner.

## Changes from V2
- `actions/checkout` uses `clean: false` so GitHub does not delete Unity's persistent `Library` cache on every run.
- First-time Unity import has a 60-minute step timeout.
- CI job timeout is 180 minutes; build job timeout is 240 minutes.
- Existing `Library` is reused when present and non-empty.
- `-accept-apiupdate` remains enabled for Unity batchmode.
- Unity logs are retained as workflow artifacts.

## Expected first run
The first run can take a long time because Unity must import the project and build its Asset Database. Subsequent runs should be much faster because the self-hosted runner preserves `Library`.

## Install
Copy the contents of this archive into the root of the GitHub Desktop clone, preserving `Assets`, `Packages`, `ProjectSettings`, `.github`, and `Tools` at the repository root. Commit and push to `main`.
