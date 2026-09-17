param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('preflight','secrets','resolve-unity','stop-processes','validate','cleanup')]
    [string]$Action
)
$ErrorActionPreference = 'Stop'

switch ($Action) {
    'preflight' {
        if (!(Test-Path Assets)) { throw 'Assets missing' }
        if (!(Test-Path Packages)) { throw 'Packages missing' }
        if (!(Test-Path ProjectSettings)) { throw 'ProjectSettings missing' }
        if (!(Test-Path ProjectSettings/ProjectVersion.txt)) { throw 'ProjectVersion.txt missing' }
        if (-not (Select-String -Path ProjectSettings/ProjectVersion.txt -Pattern '^m_EditorVersion:\s*6000\.0\.67f1\s*$' -Quiet)) { throw 'Unity version is not exactly 6000.0.67f1' }
        Write-Host 'ERO structure: OK'
        Get-Content ProjectSettings/ProjectVersion.txt
    }
    'secrets' {
        $patterns = @('UNITY_' + 'PASSWORD=','DISCORD_' + 'TOKEN=','BEGIN RSA PRIVATE KEY','BEGIN OPENSSH PRIVATE KEY','BEGIN PRIVATE KEY')
        $files = Get-ChildItem Assets,Packages,ProjectSettings -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.Extension -ne '.meta' }
        foreach ($file in $files) {
            $hit = Select-String -Path $file.FullName -Pattern $patterns -SimpleMatch -ErrorAction SilentlyContinue
            if ($hit) { throw "Possible secret found in project file: $($file.FullName)" }
        }
        Write-Host 'Secret scan: OK'
    }
    'resolve-unity' {
        $version = '6000.0.67f1'
        $candidates = New-Object System.Collections.Generic.List[string]
        if ($env:ERO_UNITY_EXE) { [void]$candidates.Add($env:ERO_UNITY_EXE) }

        # Prefer the explicit versioned Hub locations used by the hosted/self-hosted runners.
        $knownRoots = @(
            (Join-Path ${env:ProgramFiles} "Unity\Hub\Editor\$version\Editor\Unity.exe"),
            (Join-Path ${env:ProgramFiles} "Unity Hub\Editor\$version\Editor\Unity.exe"),
            (Join-Path ${env:ProgramFiles(x86)} "Unity\Hub\Editor\$version\Editor\Unity.exe"),
            (Join-Path ${env:ProgramFiles(x86)} "Unity Hub\Editor\$version\Editor\Unity.exe"),
            "C:\Unity\Hub\Editor\$version\Editor\Unity.exe",
            "D:\Unity\Hub\Editor\$version\Editor\Unity.exe",
            "D:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
        )
        foreach ($candidate in $knownRoots) { if ($candidate) { [void]$candidates.Add($candidate) } }

        # Unity Hub records installed editors in editors.json; this handles custom Hub install roots.
        $hubFiles = @(
            (Join-Path ${env:APPDATA} 'UnityHub\editors.json'),
            (Join-Path ${env:LOCALAPPDATA} 'UnityHub\editors.json')
        ) | Where-Object { $_ -and (Test-Path -LiteralPath $_ -PathType Leaf) }
        foreach ($hubFile in $hubFiles) {
            try {
                $hubEditors = Get-Content -LiteralPath $hubFile -Raw | ConvertFrom-Json
                foreach ($editor in @($hubEditors)) {
                    $path = $null
                    if ($editor.version -eq $version) { $path = $editor.path }
                    elseif ($editor.version -and ($editor.version -match "^$version(?:\s|$)")) { $path = $editor.path }
                    if ($path) {
                        if ($path -match '(?i)Unity\.exe$') { [void]$candidates.Add($path) }
                        else { [void]$candidates.Add((Join-Path $path 'Unity.exe')); [void]$candidates.Add((Join-Path $path 'Editor\Unity.exe')) }
                    }
                }
            } catch { Write-Warning "Unable to parse Unity Hub editor registry '$hubFile': $($_.Exception.Message)" }
        }

        # Unity installer registry entries can point at non-standard installation roots.
        $registryPaths = @(
            'HKLM:\SOFTWARE\Unity Technologies\Installer',
            'HKLM:\SOFTWARE\WOW6432Node\Unity Technologies\Installer'
        )
        foreach ($registryPath in $registryPaths) {
            if (Test-Path $registryPath) {
                try {
                    $props = Get-ItemProperty -Path $registryPath -ErrorAction Stop
                    foreach ($property in $props.PSObject.Properties) {
                        if ($property.Value -is [string] -and $property.Value -match [regex]::Escape($version)) {
                            [void]$candidates.Add($property.Value)
                        }
                    }
                } catch { Write-Warning "Unable to inspect Unity installer registry '$registryPath': $($_.Exception.Message)" }
            }
        }

        # Finally accept Unity.exe exposed on PATH, but still require the exact editor version below.
        try {
            $pathUnity = Get-Command Unity.exe -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty Source
            if ($pathUnity) { [void]$candidates.Add($pathUnity) }
        } catch { }

        $unityExe = $null
        foreach ($candidate in ($candidates | Select-Object -Unique)) {
            if ((Test-Path -LiteralPath $candidate -PathType Leaf) -and ($candidate -match '(?i)Unity\.exe$')) {
                $unityExe = [System.IO.Path]::GetFullPath([string]$candidate)
                break
            }
        }

        if (-not $unityExe) {
            $searchRoots = @("C:\Program Files\Unity", "C:\Program Files\Unity Hub", "C:\Unity", "D:\Unity", "D:\Program Files\Unity") | Where-Object { Test-Path -LiteralPath $_ -PathType Container }
            foreach ($root in $searchRoots) {
                $found = Get-ChildItem -LiteralPath $root -Filter 'Unity.exe' -File -Recurse -ErrorAction SilentlyContinue |
                    Where-Object { $_.FullName -match "(?i)[\\/]$version[\\/]Editor[\\/]Unity\.exe$" } |
                    Select-Object -First 1
                if ($found) { $unityExe = $found.FullName; break }
            }
        }

        if (-not $unityExe) { throw "Unity $version executable not found. Set ERO_UNITY_EXE to the full path of Unity.exe or install the editor." }
        if (-not (Test-Path -LiteralPath $unityExe -PathType Leaf)) { throw "Resolved Unity path is invalid: $unityExe" }
        $productVersion = (Get-Item -LiteralPath $unityExe).VersionInfo.ProductVersion
        if ([string]::IsNullOrWhiteSpace($productVersion) -or -not ($productVersion -match "^$version(?:\s|$)")) {
            throw "Unity executable version mismatch. Expected $version, got '$productVersion' at $unityExe"
        }
        "UNITY_EXE=$unityExe" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
        "UNITY_VERSION=$productVersion" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
        Write-Host "Using Unity: $unityExe"
        Write-Host "Unity ProductVersion: $productVersion"
    }
    'stop-processes' {
        Get-Process Unity,UnityHub,BeeBackend -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    }
    'validate' {
        New-Item -ItemType Directory -Force -Path Logs | Out-Null
        $log = Join-Path $PWD 'Logs/ERO_Unity_Validation.log'
        Write-Host "Launching Unity validation: $env:UNITY_EXE"
        $arguments = '-batchmode -nographics -quit -accept-apiupdate -projectPath "{0}" -buildTarget StandaloneWindows64 -executeMethod EternalRealmsOnline.CI.EROBuildAutomation.ValidateCompile -logFile "{1}"' -f $PWD, $log
        $process = Start-Process -FilePath $env:UNITY_EXE -ArgumentList $arguments -WorkingDirectory $PWD -Wait -PassThru -NoNewWindow
        $code = $process.ExitCode
        Write-Host "Unity process exit code: $code"
        if (Test-Path $log) {
            Write-Host '--- Unity validation log (tail) ---'
            Get-Content $log -Tail 300
            Write-Host '--- End Unity validation log ---'
        } else {
            Write-Host "Unity validation log was not created: $log"
        }
        if ($code -ne 0) { throw "Unity compile validation failed with exit code $code" }
    }
    'cleanup' {
        Remove-Item -Recurse -Force Library/Bee -ErrorAction SilentlyContinue
        Remove-Item -Recurse -Force Temp -ErrorAction SilentlyContinue
        Remove-Item -Recurse -Force obj -ErrorAction SilentlyContinue
    }
}
