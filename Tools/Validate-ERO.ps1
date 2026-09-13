param([switch]$Strict)
$ErrorActionPreference = "Stop"
$required = @("Assets","Packages","ProjectSettings","ProjectSettings/ProjectVersion.txt")
foreach($p in $required){ if(!(Test-Path $p)){ throw "Missing required path: $p" }; Write-Host "OK: $p" }
$v=(Get-Content "ProjectSettings/ProjectVersion.txt" -Raw).Trim(); Write-Host "Unity: $v"
if($v -notmatch "6000\.0\.67f1"){ throw "Expected Unity 6000.0.67f1" }
$s=@(Get-ChildItem Assets/ERO -Recurse -Filter *.cs -File); Write-Host "ERO C# scripts: $($s.Count)"
if($s.Count -eq 0){ throw "No ERO C# scripts found" }
Write-Host "ERO project validation OK."
