$ErrorActionPreference = 'Stop'
# 自动定位仓库根：优先脚本所在目录，其次当前目录及其上下级，找含 scripts/build.ps1 的目录。
$scriptPath = $MyInvocation.MyCommand.Path
function Find-ProjectRoot($sp) {
  $cands = @()
  if (-not [string]::IsNullOrEmpty($sp)) { $cands += (Split-Path -Parent $sp) }
  if (-not [string]::IsNullOrEmpty($PSScriptRoot)) { $cands += $PSScriptRoot }
  $cands += $PWD.Path
  $cands += (Split-Path -Parent $PWD.Path)
  foreach ($c in $cands) {
    if (-not [string]::IsNullOrEmpty($c) -and (Test-Path -LiteralPath (Join-Path $c 'scripts\build.ps1'))) { return $c }
  }
  foreach ($d in (Get-ChildItem -LiteralPath $PWD.Path -Directory -ErrorAction SilentlyContinue)) {
    if (Test-Path -LiteralPath (Join-Path $d.FullName 'scripts\build.ps1')) { return $d.FullName }
  }
  return $PWD.Path
}
$projectRoot = Find-ProjectRoot $scriptPath
$srcDir      = Join-Path $projectRoot 'src'
$policyDir   = Join-Path $projectRoot 'policies'
$manifest    = Join-Path $projectRoot 'app.manifest'

$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }

# 产物输出到仓库外的原位置（与整理前一致：仓库父目录/I:\AIstore）。用 ASCII 文件名避免跨环境编码问题。
$outputFile = Join-Path (Split-Path -Parent $projectRoot) 'FocusStation-v2.0.10.exe'

$sources = @('Model.cs','Planner.cs','Sync.cs','Dialogs.cs','MiniTimer.cs','MainForm.cs','Program.cs') | ForEach-Object { Join-Path $srcDir $_ }

& $compiler /nologo "/resource:$(Join-Path $policyDir 'i-have-adhd-policy.md'),LittleFocus.PrimaryAdhdPolicy" "/resource:$(Join-Path $policyDir 'ai-runtime-policy.md'),LittleFocus.RuntimePolicy" "/resource:$(Join-Path $policyDir 'adhd-friendly-policy.md'),LittleFocus.AdhdPolicy" /target:winexe /platform:anycpu /optimize+ "/out:$outputFile" "/win32manifest:$manifest" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Core.dll /reference:System.Xml.dll /reference:System.Web.Extensions.dll /reference:System.Net.Http.dll /reference:System.Security.dll $sources
if ($LASTEXITCODE -ne 0) { throw '编译失败' }
Write-Output "已生成：$outputFile"
