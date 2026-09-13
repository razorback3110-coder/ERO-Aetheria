# ERO GitHub Automation V2

This revision hardens the self-hosted Unity 6000.0.67f1 pipeline for ERO-PC.

## Changes
- Adds a dedicated Unity project initialization step before compile validation.
- Uses `-accept-apiupdate` for batch-mode Unity runs.
- Removes `-nographics` from Windows editor/compile initialization so the local GPU/editor environment can initialize normally; Linux Dedicated Server keeps `-nographics`.
- Uses PowerShell `Start-Process -Wait -PassThru` so Unity's process exit code is captured reliably.
- Prints the tail of Unity logs directly when a step fails.
- Explicitly calls `EditorApplication.Exit(0)` after successful ERO CI/build methods.
- Keeps Unity logs as GitHub artifacts.

## Expected flow
Checkout -> project validation -> secret scan -> Unity discovery -> Unity Library/import initialization -> compile validation -> Windows build -> Linux server build -> artifacts.

The first clean run may take longer because Unity must reconstruct its Library/Asset Database. The self-hosted runner is intentionally used so this initialized state can be reused between jobs where the workspace is retained.

This package has been structurally checked here, but Unity 6000.0.67f1 cannot be executed in this environment. Final validation must run on ERO-PC.
