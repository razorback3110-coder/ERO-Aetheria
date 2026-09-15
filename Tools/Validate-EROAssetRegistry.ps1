$ErrorActionPreference = "Stop"

$registry = Join-Path $env:GITHUB_WORKSPACE "Assets\ERO\Legal\ERO_Asset_License_Registry.md"
if (-not (Test-Path -LiteralPath $registry)) {
    throw "Missing legal asset registry: $registry"
}

$text = Get-Content -LiteralPath $registry -Raw
$requiredSections = @(
    "# ERO Asset & License Registry",
    "## Policy",
    "## APPROVED — environment / props / nature",
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

# Parse only Markdown table rows whose header explicitly declares the commercial-use field.
# This avoids treating prose, section dividers, or unrelated tables as asset records.
$lines = $approved -split "`r?`n"
$rows = @()
$inAssetTable = $false
foreach ($line in $lines) {
    if ($line -match '^\|\s*Asset\s*\|\s*Source\s*\|\s*License\s*\|\s*Commercial game\s*\|') {
        $inAssetTable = $true
        continue
    }
    if ($line -match '^\|\s*Asset\s*\|') {
        $inAssetTable = $false
        continue
    }
    if ($inAssetTable -and $line -match '^\|.*\|$' -and $line -notmatch '^\|\s*-+') {
        $rows += $line
    }
}

if ($rows.Count -eq 0) { throw "No approved commercial asset rows were found" }

foreach ($row in $rows) {
    $cells = $row.Trim('|').Split('|') | ForEach-Object { $_.Trim() }
    if ($cells.Count -lt 4) { throw "Malformed approved asset row: $row" }
    $asset = $cells[0]
    $source = $cells[1]
    $license = $cells[2]
    $commercial = $cells[3]
    if ([string]::IsNullOrWhiteSpace($asset) -or [string]::IsNullOrWhiteSpace($source) -or [string]::IsNullOrWhiteSpace($license)) {
        throw "Approved asset row has empty required metadata: $row"
    }
    if ($source -notmatch '^https://') { throw "Approved asset '$asset' has no HTTPS source URL" }
    if ($license -notmatch 'CC0|public-domain|commercial') { throw "Approved asset '$asset' has no recognized permissive/commercial license marker" }
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
Write-Host "Approved commercial asset rows checked: $($rows.Count)"
