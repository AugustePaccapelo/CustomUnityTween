<#
.SYNOPSIS
    Runs the TweenCore test suite headlessly and reports the result.

.DESCRIPTION
    Wraps Unity's batchmode test runner and deals with its two awkward habits:

      * Unity.exe returns to the shell immediately with an empty exit code,
        long before the run has finished. This waits for the process properly.
      * A missing results file means the run crashed or failed to compile. That
        is reported as a failure rather than as an empty pass.

    Exits 0 when every test passed, 1 otherwise, so it can be used in CI.

.PARAMETER Platform
    EditMode (default) or PlayMode. Use Both to run the two in sequence.

.PARAMETER EditorPath
    Override the editor. By default the version pinned in
    ProjectSettings/ProjectVersion.txt is used, so the project is never opened
    by an editor that would silently upgrade it.

.EXAMPLE
    .\run-tests.ps1
    .\run-tests.ps1 -Platform PlayMode
    .\run-tests.ps1 -Platform Both
#>

[CmdletBinding()]
param(
    [ValidateSet("EditMode", "PlayMode", "Both")]
    [string]$Platform = "EditMode",

    [string]$EditorPath,

    [string]$ResultsDir
)

$ErrorActionPreference = "Stop"

$repoRoot   = $PSScriptRoot
$projectDir = Join-Path $repoRoot "TweensProject"

if (-not $ResultsDir) { $ResultsDir = Join-Path $repoRoot "TestResults" }
if (-not (Test-Path $ResultsDir)) { New-Item -ItemType Directory -Path $ResultsDir | Out-Null }

# ---------- Locate the editor ---------- #

function Resolve-Editor {
    if ($EditorPath) {
        if (-not (Test-Path $EditorPath)) { throw "No editor at '$EditorPath'." }
        return $EditorPath
    }

    $versionFile = Join-Path $projectDir "ProjectSettings\ProjectVersion.txt"
    if (-not (Test-Path $versionFile)) { throw "Cannot find $versionFile - is this the repository root?" }

    $line = Select-String -Path $versionFile -Pattern '^m_EditorVersion:\s*(\S+)' | Select-Object -First 1
    if (-not $line) { throw "Could not read the editor version from $versionFile." }
    $version = $line.Matches[0].Groups[1].Value

    $candidate = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
    if (Test-Path $candidate) { return $candidate }

    throw @"
Unity $version is not installed at:
  $candidate

The project pins that version, so opening it with another one would upgrade it.
Install it from Unity Hub, or pass -EditorPath to use a different editor
deliberately.
"@
}

# ---------- Run one platform ---------- #

function Invoke-Suite([string]$mode, [string]$unity) {
    $results = Join-Path $ResultsDir "results-$mode.xml"
    $log     = Join-Path $ResultsDir "unity-$mode.log"

    if (Test-Path $results) { [System.IO.File]::Delete($results) }

    Write-Host "Running $mode tests..." -ForegroundColor Cyan
    $started = Get-Date

    $process = Start-Process -FilePath $unity -PassThru -ArgumentList @(
        "-runTests", "-batchmode",
        "-projectPath", $projectDir,
        "-testPlatform", $mode,
        "-testResults", $results,
        "-logFile", $log
    )

    # Unity detaches, so neither the call operator nor the returned exit code can
    # be trusted. Wait on the process, then make sure nothing is still holding on.
    $process.WaitForExit()
    while (Get-Process -Name "Unity" -ErrorAction SilentlyContinue) { Start-Sleep -Seconds 2 }

    $elapsed = "{0:N1}" -f ((Get-Date) - $started).TotalSeconds

    if (-not (Test-Path $results)) {
        Write-Host "  $mode FAILED after $elapsed s - no results file was produced." -ForegroundColor Red
        Write-Host "  That usually means a compile error or a crash. Last lines of the log:" -ForegroundColor Red
        if (Test-Path $log) { Get-Content $log -Tail 30 | ForEach-Object { "    $_" } }
        return $false
    }

    [xml]$xml = Get-Content $results
    $run = $xml.'test-run'
    $failed = [int]$run.failed

    $colour = if ($failed -eq 0) { "Green" } else { "Red" }
    Write-Host ("  {0}: {1} passed, {2} failed, {3} skipped, of {4} in {5} s" -f `
        $mode, $run.passed, $run.failed, $run.skipped, $run.total, $elapsed) -ForegroundColor $colour

    if ($failed -gt 0) {
        foreach ($case in $xml.SelectNodes("//test-case[@result='Failed']")) {
            Write-Host ""
            Write-Host "  FAILED  $($case.fullname)" -ForegroundColor Red
            $message = $case.failure.message.InnerText
            if ($message) { Write-Host ("          " + ($message -replace "\s+", " ").Trim()) }
        }
        Write-Host ""
        Write-Host "  Full log: $log"
    }

    return ($failed -eq 0)
}

# ---------- Go ---------- #

$unity = Resolve-Editor
Write-Host "Editor:  $unity"
Write-Host "Project: $projectDir"

$lockfile = Join-Path $projectDir "Temp\UnityLockfile"
if (Test-Path $lockfile) {
    Write-Host ""
    Write-Host "The project appears to be open in the Unity editor." -ForegroundColor Yellow
    Write-Host "A batchmode run takes an exclusive lock, so close it first." -ForegroundColor Yellow
    exit 1
}

$modes = if ($Platform -eq "Both") { @("EditMode", "PlayMode") } else { @($Platform) }
$allPassed = $true

foreach ($mode in $modes) {
    if (-not (Invoke-Suite $mode $unity)) { $allPassed = $false }
}

Write-Host ""
if ($allPassed) {
    Write-Host "All tests passed." -ForegroundColor Green
    exit 0
}

Write-Host "There were test failures." -ForegroundColor Red
exit 1
