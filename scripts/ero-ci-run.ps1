param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('preflight','secrets','resolve-unity','stop-processes','validate','cleanup')]
    [string]$Action
)
$ErrorActionPreference = 'Stop'

$UnityVersion = '6000.0.67f1'

switch ($Action) {
    'preflight' {
        $required = @('Assets','Packages','ProjectSettings','ProjectSettings/ProjectVersion.txt','Assets/ERO/Legal/ERO_Asset_License_Registry.md')
        foreach ($path in $required) {
            if (!(Test-Path -LiteralPath $path)) { throw "Required ERO path missing: $path" }
        }
        if (-not (Select-String -Path ProjectSettings/ProjectVersion.txt -Pattern "^m_EditorVersion:\s*$([regex]::Escape($UnityVersion))\s*$" -Quiet)) {
            throw "Unity version is not exactly $UnityVersion"
        }
        Write-Host 'ERO structure: OK'
        Write-Host "Pinned Unity: $UnityVersion"
        Get-Content ProjectSettings/ProjectVersion.txt
    }
    'secrets' {
        $patterns = @(
            'UNITY_PASSWORD=',
            'DISCORD_TOKEN=',
            'BEGIN RSA PRIVATE KEY',
            'BEGIN OPENSSH PRIVATE KEY',
            'BEGIN PRIVATE KEY'
        )
        # Keep this gate focused on source/config text. Scanning every Unity binary/imported asset
        # recursively was unnecessarily expensive on the self-hosted runner and could prevent the
        # actual Unity compile job from ever starting.
        $extensions = @('.cs','.json','.yaml','.yml','.txt','.xml','.asset','.prefab','.unity','.shader','.hlsl','.asmdef','.md','.ps1','.bat','.cmd','.ini','.cfg','.config')
        $roots = @('Assets','Packages','ProjectSettings')
        $files = Get-ChildItem $roots -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object {
                $_.Extension -in $extensions -and
                $_.Length -le 2097152 -and
                $_.Extension -ne '.meta'
            }
        $scanned = 0
        foreach ($file in $files) {
            $scanned++
            $hit = Select-String -LiteralPath $file.FullName -Pattern $patterns -SimpleMatch -ErrorAction SilentlyContinue
            if ($hit) { throw "Possible secret found in project file: $($file.FullName)" }
        }
        Write-Host "Secret scan: OK ($scanned text/config files scanned)"
    }
    'resolve-unity' {
        $version = $UnityVersion
        $candidates = New-Object System.Collections.Generic.List[string]
        if ($env:ERO_UNITY_EXE) { [void]$candidates.Add($env:ERO_UNITY_EXE) }
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
        $productVersion = (Get-Item -LiteralPath $unityExe).VersionInfo.ProductVersion
        if ([string]::IsNullOrWhiteSpace($productVersion) -or -not ($productVersion -match "^$version(?:\s|_|$)")) {
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
