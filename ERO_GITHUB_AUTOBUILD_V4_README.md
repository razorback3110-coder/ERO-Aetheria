# ERO GitHub Automation V4

Purpose: robust Unity 6000.0.67f1 CI/build pipeline for the persistent ERO self-hosted runner.

## Changes from V3
- Explicit per-step timeout for Unity initialization (60 min).
- Explicit per-step timeout for compile validation (60 min).
- Explicit per-step timeouts for Windows/Linux builds (120 min each).
- Persistent Library reuse only when `Library/SourceAssetDB` exists.
- `actions/checkout` keeps `clean: false` so the self-hosted runner can retain its Unity Library cache.
- Unity logs are uploaded on failure.

## Install
Copy the archive contents into the root of the local `ERO-Aetheria` clone, commit, and push.
Recommended commit: `Fix Unity CI V4 persistent Library and build timeouts`
