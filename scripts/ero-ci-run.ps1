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
        $candidates = @(
            $env:ERO_UNITY_EXE,
            'C:\Program Files\Unity\Hub\Editor\6000.0.67f1\Editor\Unity.exe'
        ) | Where-Object {
            $_ -and
            $_ -match '(?i)(^|[\\/])Unity\.exe$' -and
            (Test-Path -LiteralPath $_ -PathType Leaf)
        }
        if (!$candidates) { throw 'Unity 6000.0.67f1 executable not found. Set ERO_UNITY_EXE to the full path of Unity.exe or install the editor at the standard Unity Hub path.' }
        $unityExe = @($candidates)[0]
        $unityExe = [System.IO.Path]::GetFullPath([string]$unityExe)
        if (-not ($unityExe -match '(?i)Unity\.exe$') -or -not (Test-Path -LiteralPath $unityExe -PathType Leaf)) {
            throw "Resolved Unity path is invalid: $unityExe"
        }
        $productVersion = (Get-Item -LiteralPath $unityExe).VersionInfo.ProductVersion
        if ([string]::IsNullOrWhiteSpace($productVersion) -or -not ($productVersion -match '^6000\.0\.67f1(?:\s|$)')) {
            throw "Unity executable version mismatch. Expected 6000.0.67f1, got '$productVersion' at $unityExe"
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
