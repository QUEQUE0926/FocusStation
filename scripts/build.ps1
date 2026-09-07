$ErrorActionPreference = 'Stop'
$scriptPath = $MyInvocation.MyCommand.Path

# Fallback repo root; derive from script path when possible.
$projectRoot = 'I:\AIstore\9.FocusStation'
if (-not [string]::IsNullOrEmpty($scriptPath)) {
    $idx = $scriptPath.LastIndexOf('\scripts\')
    if ($idx -ge 0) { $projectRoot = $scriptPath.Substring(0, $idx) }
}

$srcDir    = $projectRoot + '\src'
$policyDir = $projectRoot + '\policies'
$manifest  = $projectRoot + '\app.manifest'

$compiler = $env:WINDIR + '\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = $env:WINDIR + '\Microsoft.NET\Framework\v4.0.30319\csc.exe' }

$artifactsDir = $projectRoot + '\artifacts'
New-Item -ItemType Directory -Path $artifactsDir -Force | Out-Null

# Branch-aware output: dev/other branches get a distinct name so they never
# overwrite the main build (insurance against cross-branch clobbering).
# Read .git/HEAD directly to avoid depending on the git CLI being on PATH.
$branch = 'unknown'
try {
    $headFile = $projectRoot + '\.git\HEAD'
    if (Test-Path -LiteralPath $headFile) {
        $line = (Get-Content -LiteralPath $headFile -Raw).Trim()
        if ($line -like 'ref:*') {
            $branch = $line.Substring($line.LastIndexOf('/') + 1)
        } else {
            $branch = 'detached'
        }
    }
} catch { }
$suffix = ''
if ($branch -ne 'main') {
    $s = $branch -replace '[^A-Za-z0-9._-]', '-'
    $suffix = '-' + $s
}
$outputFile = $artifactsDir + '\FocusStation-v2.0.10' + $suffix + '.exe'

# Branch-aware data isolation: non-main builds compile with the DEV symbol so
# they read/write LittleFocusDesktop-dev instead of the production folder.
# That keeps data.xml AND sync.xml (WebDAV endpoint + password) separate, so
# testing sync on dev can never merge test rows into real data.
$defineArg = ''
if ($branch -ne 'main') { $defineArg = '/define:DEV' }

$sources = @('Model.cs','Planner.cs','Sync.cs','Dialogs.cs','MiniTimer.cs','MainForm.cs','Program.cs') | ForEach-Object { $srcDir + '\' + $_ }

& $compiler /nologo "/resource:$($policyDir + '\i-have-adhd-policy.md'),LittleFocus.PrimaryAdhdPolicy" "/resource:$($policyDir + '\ai-runtime-policy.md'),LittleFocus.RuntimePolicy" "/resource:$($policyDir + '\adhd-friendly-policy.md'),LittleFocus.AdhdPolicy" /target:winexe /platform:anycpu /optimize+ "/out:$outputFile" "/win32manifest:$manifest" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Core.dll /reference:System.Xml.dll /reference:System.Web.Extensions.dll /reference:System.Net.Http.dll /reference:System.Security.dll $defineArg $sources
if ($LASTEXITCODE -ne 0) { throw 'compile failed' }
Write-Output "built: $outputFile (branch=$branch define=$defineArg)"
