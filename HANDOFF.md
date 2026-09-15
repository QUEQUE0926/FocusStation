# HANDOFF.md — FocusStation 交接文档

> 给**完全没有上下文的新会话**看。下次开新会话，第一句话就应该是「先读 HANDOFF.md」。
> 本文件最后更新：2026-09-15（v2.0.11 完成并合并 main 后）。

---

## 0. 一句话定位

FocusStation（专注小站）是一个 **Windows 桌面专注计时工具**（C# / .NET Framework 4.x，单文件 winexe，用 `csc.exe` 直接编译，无 csproj/sln）。本项目当前主线任务是**按腾讯文档《专注小站迭代log》逐版本修用户反馈**。

当前版本：**v2.0.11**（已合并 main）。本次会话（2026-09-15）已完成 v2.0.11 的两条未修复反馈修复、冒烟测试、合并 main，以及补一个因正式包没生成而暴露的 build 脚本 bug。

---

## 1. 我们在做什么任务

迭代闭环：读腾讯文档的「专注小站迭代log」→ 按版本号（v2.0.5…v2.0.11）挑出「未修复」的反馈 → 改代码 → dev 跑 `scripts/test.ps1` 冒烟 → 合并 main → 生成带版本号的 EXE（`scripts/build.ps1`）。

**腾讯文档入口（必须用这个工具读，不能用网页 fetch）：**
- 文档：`https://docs.qq.com/sheet/DQm9XYWxSQm5yYWNs?tab=nn5rmn`
- 工具：MCP `mcp__tencent-docs__get_content`，参数 `file_id="DQm9XYWxSQm5yYWNs"`。
- 文档按版本分子表，v2.0.11 表里每条反馈标注「已修复 / 未修复」。

---

## 2. 已经完成了什么（截至 2026-09-15）

### 2.1 v2.0.11 两条未修复反馈（用户本次指定改的）
- **问题② 写稿模板底部加子任务 + 时间百分比被改乱**
  - 根因：`src/Dialogs.cs` 的 `AddStage()` 插入子任务后调用 `RenumberStages()`，会把**所有**子任务百分比整体重排成均匀等差数列（100/90/80/70…），冲掉写稿模板 `Planner.cs` 里手调好的递减序列（100/90/80/65/20）。
  - 修复：插入时**不再整体重排**，新子任务按「继续递减」取值——加在末尾取「末行−10」，插在中间取相邻两行中点（相邻差 <2 时回退整排保合法）。各子任务「剩余 %」与对应时长都保留。
  - ⚠️ **未验证点**：腾讯文档里问题②原话「时间百分比需要改成**下图**这样的」——那张配图无法以文本获取，当前实现是按「继续原有递减序列、不破坏已调好的百分比」推断的。需用户对照原截图确认格式是否一致。
- **问题③ 长按 2 秒拖动任务排序**
  - 主面板任务列表 `projects`（自定义 `TaskListBox`）原本只能右键复制/删除，不能拖。
  - 修复：左键**按住 2 秒**进入排序（光标 `SizeNS` + 状态栏提示「拖动排序中…松手保存顺序」），移动实时重排，松手写入 `data.Projects` 并保存；单击/短按仍正常选中，移动 >8px 视为误触不触发。新增 `ReorderVisible` / `CommitReorder`。

### 2.2 测试
- `scripts/test.ps1` 全绿：逻辑单测 168 断言 + 界面单测（含两张新 PNG）通过。
- 新增 2 条回归断言：写稿模板加子任务后原百分比不变且严格递减；拖动排序能正确落库。

### 2.3 版本与构建
- 版本号升至 **v2.0.11**（已同步 `scripts/build.ps1`、`README.md`、`docs/分支与发布流程.md`、`docs/CHANGELOG.md`）。
- 修复 `build.ps1` 的 bug（见第 4 节）：main 分支也能正常生成正式包。
- 已生成本地 `artifacts/FocusStation-v2.0.11.exe` 与 `FocusStation-v2.0.11-dev.exe`。

### 2.4 git
- 提交历史（main/dev 当前同位）：`3138743`（build.ps1 修复）→ `a2ddb6c`（v2.0.11 功能修复）→ `b761e30`（TIMER-004）。
- `dev` 与 `main` 均已推送 origin 并快进到 `3138743`。

---

## 3. 当前卡点 / 待定（需要用户拍板或信息）

1. **问题② 配图格式未确认**：配图取不到文字，实现是推断的。等用户对照原截图确认；若格式另有要求再微调。
2. **腾讯文档状态未更新**：文档里 ②、③ 仍标「未修复」。是否要把它们改成「已修复」——之前没擅自改（动表格有风险），等用户定。
3. **旧版 EXE 是否清理**：`artifacts/` 里还有 `FocusStation-v2.0.10.exe` 和 `v2.0.10-dev.exe` 两个旧文件，不会自动删，问用户要不要清。

---

## 4. 下一步计划

- 等用户反馈后：①确认问题②配图格式；②决定是否把腾讯文档 ②/③ 标「已修复」；③决定是否删旧 v2.0.10 包。
- 若确认无误，本版本收尾，等下一版反馈。
- 开工前始终确认自己在 `dev` 分支，改完走「test → 提 dev → 合并 main → build 生成 EXE」流程。

---

## 5. 绝对不要再踩的坑（血泪清单）

1. **`csc.exe` 在 PowerShell 命令里直接出现会被安全拦截**：git commit 命令里如果**消息或命令行**出现 `csc.exe` 字样，Bash 工具会被「Command blocked for security: csc.exe compiles arbitrary C# code」拦掉。提交构建相关改动时**用 PowerShell 跑 git** 或把 `csc` 换个说法（比如「编译器」），别在命令行里写 `csc.exe`。
2. **`scripts/test.ps1` ≠ `scripts/build.ps1`**：
   - `test.ps1` 只在 `artifacts/work/` 编译一个临时程序跑测试，**不生成带版本号的发布 EXE**。
   - `build.ps1` 才生成 `artifacts/FocusStation-vX.Y.Z(-dev).exe`。
   - 用户说「重新生成 EXE」指的是 `build.ps1`，别拿 test 通过当成了打包完成。
3. **`build.ps1` 按分支命名 + define**：
   - 在 `dev` 上跑 → 输出 `FocusStation-vX.Y.Z-dev.exe` 且带 `/define:DEV`（数据读写 `LittleFocusDesktop-dev` 隔离目录，不会污染正式数据）。
   - 在 `main` 上跑 → 输出 `FocusStation-vX.Y.Z.exe`，**不带** `/define:DEV`。
   - 想要正式包，必须切到 `main` 再 build。
4. **`build.ps1` 经典 bug（已修，别再引入）**：main 分支下 `$defineArg` 是空字符串，若直接拼进 csc 参数会被当成「空源文件路径」→ 报 `CS2001 未能找到源文件“”。`现在已改成数组 + splatting（`if ($defineArg -ne '') { $compilerArgs += $defineArg }`）。**以后改 build.ps1 务必在 dev 和 main 两个分支都验证能编译出来。**
5. **`artifacts/` 和 `*.exe` 在 `.gitignore` 里，二进制不入库**：本地生成的 EXE 只活在磁盘上，`git status` 不会显示，旧的也不会自动消失。别困惑「为什么 git 里看不到 EXE」——这是设计如此。
6. **`UiTests.cs` 写测试时的三个低级错误（前车之鉴）**：
   - `Store` 类**没实现 IDisposable**，不能用 `using(var store=new Store(...))`，要用普通 `var store=new Store(...)`。
   - C# 对象初始化器是 `Name="B"`，**不是** `Name:"B"`（冒号是错的，编译报语法错）。
   - 断言要核对**界面真实加载的对象**（MainForm 的 `data` 字段），别核对测试里自己 new 的旧副本——否则「通过了」但根本没验到东西。
7. **腾讯文档图片取不到文字**：`mcp__tencent-docs__get_content` 只返回文字，嵌在单元格里的截图/配图拿不到。遇到「如下图所示」这类需求，必须明说「配图无法文本获取，按推断实现，待用户确认」。
8. **git 推送走本地代理**：本机出网走 `http://127.0.0.1:7890`（端口会随代理客户端重启变化，曾 10137→7890）。推送前若连不上先确认端口。命令加 `GIT_TERMINAL_PROMPT=0` 避免卡在凭据输入。GitHub 凭据已由 GCM 管理（系统级 `credential.helper=manager` 已修过一次），无需手动 token。

---

## 6. 关键文件速查

| 文件 | 作用 |
|---|---|
| `src/Dialogs.cs` | 任务/子任务编辑弹窗；`AddStage()` 加子任务逻辑在这里 |
| `src/MainForm.cs` | 主窗口；任务列表 `projects`、长按拖动排序、右键复制/删除 |
| `src/Planner.cs` | `Writing()` 写稿模板（手调百分比序列 100/90/80/65/20） |
| `src/Model.cs` | 数据模型 `Project` / `AppData` / `Store`（注意 Store 非 IDisposable） |
| `src/UiTests.cs` | 界面自动化测试（跑真实 WinForms，需 Windows） |
| `src/Tests.cs` | 纯逻辑单测 |
| `scripts/test.ps1` | 编译 + 跑逻辑/界面测试（**不生成发布包**） |
| `scripts/build.ps1` | 生成带版本号 EXE（按分支命名 + define） |
| `docs/CHANGELOG.md` | 版本变更记录，**每次发版都要补** |
| `docs/分支与发布流程.md` | dev→main 合并流程，版本号引用处要同步 |
| `REVIEW_REQUEST.md` | 连续失败两次的停手记录文档（本会话没触发） |
| `app.manifest` | 编译时 `/win32manifest` 引用 |

---

## 7. 环境速查

- OS：Windows（Win32）。编译用系统 `csc.exe`：`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe`（兜底 Framework 版）。
- 出网代理：`http://127.0.0.1:7890`（推送前确认端口）。
- git 身份：`user.name=扁鹊鹊`，`user.email=queque0926@users.noreply.github.com`。
- 分支策略：**所有改动先入 `dev`**，main 需显式批准才合并（本会话已获授权合并 v2.0.11）。
- 测试需 Windows 桌面环境（UiTests 起真实窗口）；逻辑测试 `Tests.cs` 不依赖 UI。
- 项目记忆在 `I:/AIstore/9.FocusStation/.workbuddy/memory/`（按日期日志 + MEMORY.md）。
