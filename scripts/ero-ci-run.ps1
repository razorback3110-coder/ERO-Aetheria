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
        if (-not (Select-String -Path ProjectSettings/ProjectVersion.txt -Pattern '6000.0.67f1' -Quiet)) { throw 'Unity version is not 6000.0.67f1' }
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
        ) | Where-Object { $_ -and (Test-Path $_) }
        if (!$candidates) { throw 'Unity 6000.0.67f1 executable not found. Set ERO_UNITY_EXE or install the editor at the standard Unity Hub path.' }
        "UNITY_EXE=$($candidates[0])" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
        Write-Host "Using Unity: $($candidates[0])"
    }
    'stop-processes' {
        Get-Process Unity,UnityHub,BeeBackend -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    }
    'validate' {
        New-Item -ItemType Directory -Force -Path Logs | Out-Null
        $log = Join-Path $PWD 'Logs/ERO_Unity_Validation.log'
        & $env:UNITY_EXE -batchmode -nographics -quit -accept-apiupdate -projectPath $PWD -buildTarget StandaloneWindows64 -executeMethod EternalRealmsOnline.CI.EROBuildAutomation.ValidateCompile -logFile $log
        $code = $LASTEXITCODE
        if (Test-Path $log) { Get-Content $log -Tail 250 }
        if ($code -ne 0) { throw "Unity compile validation failed with exit code $code" }
    }
    'cleanup' {
        Remove-Item -Recurse -Force Library/Bee -ErrorAction SilentlyContinue
        Remove-Item -Recurse -Force Temp -ErrorAction SilentlyContinue
        Remove-Item -Recurse -Force obj -ErrorAction SilentlyContinue
    }
}
