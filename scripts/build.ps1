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
$outputFile = $artifactsDir + '\FocusStation-v2.0.10.exe'

$sources = @('Model.cs','Planner.cs','Sync.cs','Dialogs.cs','MiniTimer.cs','MainForm.cs','Program.cs') | ForEach-Object { $srcDir + '\' + $_ }

& $compiler /nologo "/resource:$($policyDir + '\i-have-adhd-policy.md'),LittleFocus.PrimaryAdhdPolicy" "/resource:$($policyDir + '\ai-runtime-policy.md'),LittleFocus.RuntimePolicy" "/resource:$($policyDir + '\adhd-friendly-policy.md'),LittleFocus.AdhdPolicy" /target:winexe /platform:anycpu /optimize+ "/out:$outputFile" "/win32manifest:$manifest" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Core.dll /reference:System.Xml.dll /reference:System.Web.Extensions.dll /reference:System.Net.Http.dll /reference:System.Security.dll $sources
if ($LASTEXITCODE -ne 0) { throw 'compile failed' }
Write-Output "built: $outputFile"
