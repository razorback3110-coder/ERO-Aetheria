# ERO GitHub Full Project Ready

This package contains the complete Unity project at repository root, prepared for the `razorback3110-coder/ERO-Aetheria` repository.

## Root must contain
- Assets/
- Packages/
- ProjectSettings/
- .github/workflows/ero-ci.yml
- Tools/Validate-ERO.ps1

## GitHub Actions
The workflow targets the existing self-hosted runner labels:
`self-hosted`, `Windows`, `X64`, `ERO-Unity`.

## Upload
Use GitHub Desktop or Git to replace/add the repository contents from this folder. Do not upload `Library`, `Temp`, `Logs`, `obj`, or local build output.

## Unity
Target version: Unity 6000.0.67f1.
The CI first validates the project structure and then detects Unity on the self-hosted machine.
