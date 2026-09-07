$ErrorActionPreference = 'Stop'
# 自动定位仓库根：优先脚本所在目录，其次当前目录及其上下级，找含 scripts/test.ps1 的目录。
$scriptPath = $MyInvocation.MyCommand.Path
function Find-ProjectRoot($sp) {
  $cands = @()
  if (-not [string]::IsNullOrEmpty($sp)) { $cands += (Split-Path -Parent $sp) }
  if (-not [string]::IsNullOrEmpty($PSScriptRoot)) { $cands += $PSScriptRoot }
  $cands += $PWD.Path
  $cands += (Split-Path -Parent $PWD.Path)
  foreach ($c in $cands) {
    if (-not [string]::IsNullOrEmpty($c) -and (Test-Path -LiteralPath (Join-Path $c 'scripts\test.ps1'))) { return $c }
  }
  foreach ($d in (Get-ChildItem -LiteralPath $PWD.Path -Directory -ErrorAction SilentlyContinue)) {
    if (Test-Path -LiteralPath (Join-Path $d.FullName 'scripts\test.ps1')) { return $d.FullName }
  }
  return $PWD.Path
}
$projectRoot = Find-ProjectRoot $scriptPath
$srcDir      = Join-Path $projectRoot 'src'
$policyDir   = Join-Path $projectRoot 'policies'
$manifest    = Join-Path $projectRoot 'app.manifest'

$workspace = Split-Path -Parent $projectRoot
$workDir   = Join-Path $workspace 'work'
New-Item -ItemType Directory -Path $workDir -Force | Out-Null

$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }

# 1) 逻辑单元测试（控制台，不依赖 GUI）
$testExe = Join-Path $workDir 'FocusTests.exe'
$testSources = @('Model.cs','Planner.cs','Sync.cs','Tests.cs') | ForEach-Object { Join-Path $srcDir $_ }
& $compiler /nologo "/resource:$(Join-Path $policyDir 'i-have-adhd-policy.md'),LittleFocus.PrimaryAdhdPolicy" "/resource:$(Join-Path $policyDir 'ai-runtime-policy.md'),LittleFocus.RuntimePolicy" "/resource:$(Join-Path $policyDir 'adhd-friendly-policy.md'),LittleFocus.AdhdPolicy" /target:exe "/out:$testExe" /reference:System.Core.dll /reference:System.Xml.dll /reference:System.Web.Extensions.dll /reference:System.Net.Http.dll /reference:System.Security.dll $testSources
if ($LASTEXITCODE -ne 0) { throw '测试编译失败' }
& $testExe (Join-Path $workDir 'test-results')
if ($LASTEXITCODE -ne 0) { throw '测试失败' }

# 2) 界面测试版（UI_TEST 定义，数据落到 testdata/ 隔离目录，不碰真实用户数据）
$uiExe = Join-Path $workDir 'FocusUITest.exe'
$uiSources = @('Model.cs','Planner.cs','Sync.cs','Dialogs.cs','MiniTimer.cs','MainForm.cs','Program.cs') | ForEach-Object { Join-Path $srcDir $_ }
& $compiler /nologo "/resource:$(Join-Path $policyDir 'i-have-adhd-policy.md'),LittleFocus.PrimaryAdhdPolicy" "/resource:$(Join-Path $policyDir 'ai-runtime-policy.md'),LittleFocus.RuntimePolicy" "/resource:$(Join-Path $policyDir 'adhd-friendly-policy.md'),LittleFocus.AdhdPolicy" /target:winexe /define:UI_TEST "/out:$uiExe" "/win32manifest:$manifest" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Core.dll /reference:System.Xml.dll /reference:System.Web.Extensions.dll /reference:System.Net.Http.dll /reference:System.Security.dll $uiSources
if ($LASTEXITCODE -ne 0) { throw '界面测试版编译失败' }

# 3) 界面单元测试（UiTests.cs 自带 Main，同样隔离）
$uiUnitExe = Join-Path $workDir 'FocusUiUnitTests.exe'
$uiUnitSources = @('Model.cs','Planner.cs','Sync.cs','Dialogs.cs','MiniTimer.cs','MainForm.cs','UiTests.cs') | ForEach-Object { Join-Path $srcDir $_ }
& $compiler /nologo "/resource:$(Join-Path $policyDir 'i-have-adhd-policy.md'),LittleFocus.PrimaryAdhdPolicy" "/resource:$(Join-Path $policyDir 'ai-runtime-policy.md'),LittleFocus.RuntimePolicy" "/resource:$(Join-Path $policyDir 'adhd-friendly-policy.md'),LittleFocus.AdhdPolicy" /target:exe "/out:$uiUnitExe" "/win32manifest:$manifest" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Core.dll /reference:System.Xml.dll /reference:System.Web.Extensions.dll /reference:System.Net.Http.dll /reference:System.Security.dll $uiUnitSources
if ($LASTEXITCODE -ne 0) { throw '界面单元测试编译失败' }
& $uiUnitExe (Join-Path $workDir 'ui-unit-data')
if ($LASTEXITCODE -ne 0) { throw '界面单元测试失败' }
