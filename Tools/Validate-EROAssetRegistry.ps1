$ErrorActionPreference = "Stop"

$repoRoot = if ($env:GITHUB_WORKSPACE) { $env:GITHUB_WORKSPACE } else { Split-Path -Parent (Split-Path -Parent $PSScriptRoot) }
$registry = Join-Path $repoRoot "Assets\ERO\Legal\ERO_Asset_License_Registry.md"
if (-not (Test-Path -LiteralPath $registry)) {
    throw "Missing legal asset registry: $registry"
}

$text = Get-Content -LiteralPath $registry -Raw
$requiredSections = @(
    "# ERO Asset & License Registry",
    "## Policy",
    "## APPROVED",
    "## Integration quality gate",
    "## REVIEW",
    "## REJECTED",
    "## Release rule"
)
foreach ($section in $requiredSections) {
    if (-not $text.Contains($section)) { throw "Legal registry missing required section: $section" }
}

$approvedStart = $text.IndexOf("## APPROVED")
$reviewStart = $text.IndexOf("## REVIEW")
if ($approvedStart -lt 0 -or $reviewStart -le $approvedStart) {
    throw "Unable to isolate APPROVED registry entries"
}
$approved = $text.Substring($approvedStart, $reviewStart - $approvedStart)

# Keep this script ASCII-only because the self-hosted Windows runner executes Windows PowerShell 5.1.
# Approved rows require an HTTPS source, a permissive/commercial license marker, and explicit commercial use.
$rowPattern = '(?m)^\|\s*([^|]+?)\s*\|\s*(https://[^|]+?)\s*\|\s*([^|]+?)\s*\|\s*(Yes|yes|commercial)\s*\|'
$matches = [regex]::Matches($approved, $rowPattern)
if ($matches.Count -eq 0) { throw "No approved commercial asset rows were found" }

foreach ($match in $matches) {
    $asset = $match.Groups[1].Value.Trim()
    $source = $match.Groups[2].Value.Trim()
    $license = $match.Groups[3].Value.Trim()
    $commercial = $match.Groups[4].Value.Trim()
    if ([string]::IsNullOrWhiteSpace($asset)) { throw "Approved asset row has an empty asset name" }
    if ($source -notmatch '^https://') { throw "Approved asset '$asset' has no HTTPS source URL" }
    if ($license -notmatch '(?i)CC0|public-domain|commercial') { throw "Approved asset '$asset' has no recognized permissive/commercial license marker: $license" }
    if ($commercial -notmatch '(?i)^yes$|commercial') { throw "Approved asset '$asset' is not explicitly marked for commercial use" }
}

$forbiddenApproved = @(
    'Ragnarok',
    'Lost Ark',
    'Throne and Liberty'
)
foreach ($name in $forbiddenApproved) {
    if ($approved -match [regex]::Escape($name)) { throw "Proprietary game reference '$name' found inside APPROVED registry entries" }
}

Write-Host "ERO legal asset registry validation: OK"
Write-Host "Approved commercial asset rows checked: $($matches.Count)"
