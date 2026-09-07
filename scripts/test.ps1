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

$artifactsDir = $projectRoot + '\artifacts'
$workDir = $artifactsDir + '\work'
New-Item -ItemType Directory -Path $workDir -Force | Out-Null

$compiler = $env:WINDIR + '\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = $env:WINDIR + '\Microsoft.NET\Framework\v4.0.30319\csc.exe' }

# 1) logic unit tests (console, no GUI)
$testExe = $workDir + '\FocusTests.exe'
$testSources = @('Model.cs','Planner.cs','Sync.cs','Tests.cs') | ForEach-Object { $srcDir + '\' + $_ }
& $compiler /nologo "/resource:$($policyDir + '\i-have-adhd-policy.md'),LittleFocus.PrimaryAdhdPolicy" "/resource:$($policyDir + '\ai-runtime-policy.md'),LittleFocus.RuntimePolicy" "/resource:$($policyDir + '\adhd-friendly-policy.md'),LittleFocus.AdhdPolicy" /target:exe "/out:$testExe" /reference:System.Core.dll /reference:System.Xml.dll /reference:System.Web.Extensions.dll /reference:System.Net.Http.dll /reference:System.Security.dll $testSources
if ($LASTEXITCODE -ne 0) { throw 'test compile failed' }
& $testExe ($workDir + '\test-results')
if ($LASTEXITCODE -ne 0) { throw 'tests failed' }

# 2) UI test build (UI_TEST define, isolated data dir)
$uiExe = $workDir + '\FocusUITest.exe'
$uiSources = @('Model.cs','Planner.cs','Sync.cs','Dialogs.cs','MiniTimer.cs','MainForm.cs','Program.cs') | ForEach-Object { $srcDir + '\' + $_ }
& $compiler /nologo "/resource:$($policyDir + '\i-have-adhd-policy.md'),LittleFocus.PrimaryAdhdPolicy" "/resource:$($policyDir + '\ai-runtime-policy.md'),LittleFocus.RuntimePolicy" "/resource:$($policyDir + '\adhd-friendly-policy.md'),LittleFocus.AdhdPolicy" /target:winexe /define:UI_TEST "/out:$uiExe" "/win32manifest:$manifest" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Core.dll /reference:System.Xml.dll /reference:System.Web.Extensions.dll /reference:System.Net.Http.dll /reference:System.Security.dll $uiSources
if ($LASTEXITCODE -ne 0) { throw 'ui test build failed' }

# 3) UI unit tests (UiTests.cs has its own Main, isolated)
$uiUnitExe = $workDir + '\FocusUiUnitTests.exe'
$uiUnitSources = @('Model.cs','Planner.cs','Sync.cs','Dialogs.cs','MiniTimer.cs','MainForm.cs','UiTests.cs') | ForEach-Object { $srcDir + '\' + $_ }
& $compiler /nologo "/resource:$($policyDir + '\i-have-adhd-policy.md'),LittleFocus.PrimaryAdhdPolicy" "/resource:$($policyDir + '\ai-runtime-policy.md'),LittleFocus.RuntimePolicy" "/resource:$($policyDir + '\adhd-friendly-policy.md'),LittleFocus.AdhdPolicy" /target:exe "/out:$uiUnitExe" "/win32manifest:$manifest" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Core.dll /reference:System.Xml.dll /reference:System.Web.Extensions.dll /reference:System.Net.Http.dll /reference:System.Security.dll $uiUnitSources
if ($LASTEXITCODE -ne 0) { throw 'ui unit test build failed' }
& $uiUnitExe ($workDir + '\ui-unit-data')
if ($LASTEXITCODE -ne 0) { throw 'ui unit tests failed' }
