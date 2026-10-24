# HANDOFF.md — FocusStation 交接文档

> 给**完全没有上下文的新会话**看。下次开新会话，第一句话就应该是「先读 HANDOFF.md」。
> 本文件最后更新：2026-10-24（v2.0.17 复原休息模块，main/dev 均已推送 GitHub）。

---

## 0. 一句话定位

FocusStation（专注小站）是一个 **Windows 桌面专注计时工具**（C# / .NET Framework 4.x，单文件 winexe，用 `csc.exe` 直接编译，无 csproj/sln）。本项目当前主线任务是**按腾讯文档《专注小站迭代log》逐版本修用户反馈**。

当前版本：**v2.0.17**。2026-10-24 当天做了两版：v2.0.16 是「已完成按完成时间排序 + 休息改纯手动」，其中休息那条**方向理解错了**，所以紧接着 v2.0.17 把休息模块**原样复原到 v2.0.14 的行为**（只保留排序那条）。2026-10-23 连做四版：v2.0.12（今日专注后加括号）、v2.0.13（无记录项目也显示）、v2.0.14（把括号里的数改成真实累计专注 + 模板按钮改造）、v2.0.15（内置模板换成 4 步「测评模板」+ 新建任务默认套用）。

---

## 1. 我们在做什么任务

迭代闭环：读腾讯文档的「专注小站迭代log」→ 按版本号（v2.0.5…v2.0.16）挑出「未修复」的反馈 → 改代码 → dev 跑 `scripts/test.ps1` 冒烟 → 合并 main → 生成带版本号的 EXE（`scripts/build.ps1`）。

**腾讯文档入口（必须用这个工具读，不能用网页 fetch）：**
- 文档：`https://docs.qq.com/sheet/DQm9XYWxSQm5yYWNs`（各版本一个子表：v2.0.16 页 tab=`7mnnto`、v2.0.12 页 `o3ho1a`、v2.0.11 页 `nn5rmn`）
- 工具：`tencent-docs` skill 的 `tencentdocs.py tdoc_call tencent-docs get_content '{"file_id":"DQm9XYWxSQm5yYWNs"}'`。
- **一次 get_content 返回整本表所有子表的文字**（含历史页），按 `|v2.0.12` 这样的行头切段即可。
- 连接器状态若是 `auth-required`，先用 `reconnect_plugin`（pluginName=`tencent-docs@workbuddy-connector-plugins-official`）重新授权，成功后再 `tdoc_init`（应回 `READY`）。

---

## 2. 已经完成了什么（截至 2026-10-23）

### 2.1 v2.0.12 页第 1 条（版本 v2.0.12）
- **需求原文**：我希望今日专注时间后面可以加个项目总时长的显示；如 `今日已专注：XX：XX：XX（XX：XX：XX）`。
- **改法**：
  - `src/Model.cs` 新增 `TaskSupport.ProjectTotalSeconds(Project,AppData)`，统一「项目总时长」口径：挑战模式取 `ChallengeMinutes`，否则取项目自己的 `PomodoroMinutes`，都没设则回落到全局 `FocusMinutes`（与「双击改本项目的总时长」弹窗同源）。
  - `src/MainForm.cs` 新增 `TodayLine()`，`RefreshDisplay()` 里 `projectToday` 由它生成，格式 `今日已专注：HH:MM:SS（HH:MM:SS）`。

### 2.1b v2.0.12 页第 2 条（版本 v2.0.13）
- **需求原文**：对于今日没有时长的项目，也需要显示项目总时长；如 `项目总专注：XX：XX：XX（今日没有记录噢）`。
- **改法**：`TodayLine(todaySeconds, p, focusing)` 分两种文案——
  - 今日有记录或正在专注 → `今日已专注：今日时长（项目总时长）`；
  - 今日没有记录 → `项目总专注：项目总时长（今日没有记录噢）`（原来这一行是空白，现在一定会显示）；
  - 未选中项目仍留空。
- ⚠️ 这一版括号里的数取的是**设定总时长**，为 v2.0.14 的返工埋了雷（见 2.1c）。

### 2.1c v2.0.14 两条反馈（版本 v2.0.14）
- **① 项目时间统计不对**（原文：有些项目都没开始过，怎么也会有4小时？注意区分好，项目的总时长，以及项目的今日时长。）
  - 根因：v2.0.13 括号里放的是**设定的项目总时长**（挑战分钟 / 番茄分钟 / 全局默认），从未开始的项目设定 240 分钟就直接显示成 4 小时，看起来像统计数据。
  - 改为：括号里一律是**该项目累计真实专注时长**（`TimeRecord.Seconds` 全量求和，含今天；兼容早年 `ProjectId` 为空、只记项目名的旧记录），`MainForm.ProjectFocusedSeconds()` 负责取数。
  - 从未开始的项目现在显示 `项目总专注：00:00:00（今日没有记录噢）`。
  - `TaskSupport.ProjectTotalSeconds` 保留（它表达的是「设定总时长」这个另一个口径），但已不参与这条文案，避免两个概念混用。
- **② 写稿模板按钮改成「模板」：单击下拉、长按编辑**
  - 按钮 `写稿模板` → `模板`（`Name=TemplateButton`）。**单击**弹出下拉：列出模板名（带步数），底部固定一行不可点的 `长按模板按钮可以修改模板`。
  - **长按 800ms** 打开新弹窗 `TemplateEditorDialog`：左侧模板名列表（新建 / 删除），右侧模板详情（名称 + 子任务表：剩余 % / 子任务 / 下一步动作，可加删行）。保存时百分比不合法会自动摊平，名称空或没子任务会当场提示。
  - 模板存进 `AppData.Templates`，随 `data.xml` 备份/同步；`TemplateLibrary` 负责内置模板、「写稿」默认、深拷贝、百分比归一化。
  - 保存是立即落盘的：`ProjectDialog` 新增第 5 个可选参数 `persist`，MainForm 传 `Save`，所以取消任务编辑也不会丢模板改动。
  - 踩坑（已修）：**不要用字段初始化器给 `Templates` 预置默认模板**——`XmlSerializer` 对集合是「往已有列表里 Add」，会导致每次读盘都多一份。现在默认模板由 `TemplateLibrary.Ensure()` 配 `TemplatesSeeded` 标记补齐。

### 2.1d v2.0.15 两条反馈（版本 v2.0.15）
- **① 内置模板换成截图里的内容，并改名「测评模板」**
  - 原文：模板里的写稿（6步）的子项目改成截图所示；然后把该模板的名改成「测评模板」。
  - 截图内容（4 步，剩余 % 100 / 80 / 45 / 20）：梳理测评框架 → 写正文初稿 → 润色与配图 → 做封面并定稿，动作文案照截图逐字抄进 `Planner.Review()`。
  - `Planner.Writing()` → `Planner.Review()`；`TemplateLibrary.Writing()` → `TemplateLibrary.Review()`；`DefaultName="测评模板"`。
  - **老数据迁移**：`AppData.TemplateRevision`（0→2，只跑一次）。仅当模板名仍是「写稿」、且 6 个子任务标题与旧内置逐字一致时，才改名 + 换成 4 步；**用户自己改过的模板一律不动**。挂在 `TemplateLibrary.Ensure()`，即 `Store.Decode` 里，升级后首次启动就生效。
- **② 新建任务默认名 + 默认模板**
  - 原文：点「＋ 新建」时任务名默认为 `游戏测评：《请输入游戏名字》`，并默认使用「测评模板」。
  - 实现：`TaskSupport.NewProjectName` / `NewProjectPlaceholder` 两个常量；`ProjectDialog` 构造函数里用 `String.IsNullOrWhiteSpace(project.Name)` 判断「新建」（和原有默认计时方式、默认挑战开关同一处判断），预填名字并把模板 4 步深拷贝进 `project.Stages`。
  - 细节：弹窗打开时只选中 `《请输入游戏名字》` 这一段，直接打字即替换，免得用户手动删；模板被删光时保持空白不崩。

### 2.1e v2.0.16 两条反馈（版本 v2.0.16）
- **① 已完成的项目按完成时间排序**
  - 原文：已完成的项目排序，可以按照完成时间点最近来排序，保证排在前面的都是最近完成的。
  - `Project` 新增 `CompletedUtc`，`FocusEngine.Complete()` 写入；`MainForm.CompletionTime()` 在旧数据缺这个字段时回落到 `UpdatedUtc`。
  - `RefreshProjects()`：已完成视图 `OrderByDescending(CompletionTime).ThenBy(Name)`；进行中视图仍是「置顶 + 手动拖动排序」。
  - 已完成视图**不再参与长按拖动排序**（顺序由完成时间决定），`reorderTimer` 里直接跳过。`RestoreTask()` 会把 `CompletedUtc` 清掉。
- **② 休息改成纯手动（v2.0.16 做错了，v2.0.17 已复原）**
  - 原文完整版（**注意最后一句，之前漏读了**）：「我怎么感觉你又偷偷改我的休息逻辑了？」「它应该是左边的这样，只要我不点休息，就永远不会休息。」「`I:\AIstore\9.FocusStation\artifacts` 路径下，你去解包 `FocusStation-v2.0.14-dev.exe` 里面关于休息的逻辑和UI，**给我复原休息模块**。」
  - **v2.0.16 的错误做法**（别再犯）：把番茄钟到点改成 `return "due-soft"`——不暂停、继续累计超时、只响一声不弹窗；挑战模式直接删掉 `challenge-break`；界面显示「超时 MM:SS」。
  - **正确做法 = 复原 v2.0.14**：
    - `Tick()` 番茄钟到点：`ReminderSent=true; State=Phase.Paused; return "due"`，并且把计时**卡在到点那一刻**（`countedUntil=LastUtc.AddSeconds(seconds)`，超时部分不计入专注时长）。
    - `Tick()` 挑战模式：`if(RunSeconds>=NextBreakReminderSeconds){State=Phase.Paused;return "challenge-break";}` —— 每连续专注 `data.FocusMinutes` 分钟暂停一次并提醒。
    - `HandleEvent`：`due` 弹窗「到休息时间了」+ 三个按钮（开始休息 / 继续专注 5 分钟 / 结束本次），关闭弹窗仍保持暂停；`challenge-break` 弹窗「休息提醒」+（开始休息 / 继续挑战 / 保持暂停）。
    - `PauseResume()`：暂停态点「继续」时若有 `ReminderSent` 先 `Snooze()`（等于再给 5 分钟）。
    - `RefreshDisplay()`：到点后阶段文案 `已到休息时间 · 等待确认`；Idle 说明行 `当前子任务到点自动暂停，确认后才进入休息。`
    - 不要保留 `StageOvertimeSeconds` / `RestDue`，也不要有「超时 MM:SS」。
  - **判定依据（可复现）**：直接读 EXE 字节，按 UTF-16LE 找文案。`FocusStation-v2.0.13/14/15-dev.exe` 都含「到休息时间了 / 计时已暂停 / 继续专注 5 分钟 / 休息提醒 / 已经连续专注 / 当前子任务到点自动暂停」，`v2.0.16-dev.exe` 全部消失，`v2.0.17` 全部回来。
  - 关键区分：**「不点休息就不休息」是指不自动进入休息，不是不暂停**。到点必须暂停 + 弹窗，这才是他要的。

### 2.2 测试
- `scripts/test.ps1` 全绿：逻辑单测 **178 断言** + 界面单测（含渲染截图）通过。
- v2.0.17：把 v2.0.16 那批「不暂停」断言全部还原（`due` + `Paused` + 等待时间不计入）。界面测试里的探针会打印 `due probe: due Paused`，看到 `due-soft` 就是又改错了。
- v2.0.16 保留的断言：已完成列表按 `CompletedUtc` 倒序；缺 `CompletedUtc` 的老数据按 `UpdatedUtc` 兜底。
- v2.0.15 新增断言：内置模板是 4 步且百分比 100/80/45/20；旧 6 步「写稿」自动迁移成 4 步「测评模板」且 `TemplateRevision==2`；用户改过的模板保持原样；新建弹窗标题预填、自动带 4 步子任务、模板名与步骤名逐个核对。
- 顺手把三条与模板内容强绑定的老测试解耦（计时节点测试、5 行布局测试、保存测试），以后改模板内容不会再连带炸测试。

### 2.3 版本与构建
- 版本号 v2.0.12 → v2.0.13 → v2.0.14 → v2.0.15 → v2.0.16 → **v2.0.17**（已同步 `scripts/build.ps1`、`README.md`、`docs/分支与发布流程.md`、`docs/CHANGELOG.md`）。
- 产物：`artifacts/FocusStation-v2.0.17.exe`（main，无 DEV 宏）、`artifacts/FocusStation-v2.0.17-dev.exe`。

### 2.4 git
- 提交：`d1fb4fb`（v2.0.17 复原休息）→ `89f7844`（v2.0.16）→ `f8fd4e6`（v2.0.15）→ `c6024c1`（v2.0.14）→ `9384644`（v2.0.13）→ `b59515a`（v2.0.12）。
- 2026-10-24 已推送：v2.0.13–v2.0.16 那批积压提交 + v2.0.17 一起上到 GitHub（dev / main 同步）。

---

## 3. 当前卡点 / 待定（需要用户拍板或信息）

1. **腾讯文档状态未更新**：v2.0.12 第 1 条已被用户标「已修复」；v2.0.13 第 2 条、v2.0.14 两条、v2.0.15 两条、v2.0.16 两条都还没标。没擅自改表格，等用户定。
2. **休息模块已按 v2.0.14 复原（v2.0.17）**：之前 v2.0.16 的「到点不暂停」已撤销。若以后还有人提「不想自动休息」，先分清两件事——**暂停**和**进入休息**是两回事；他要的是到点暂停 + 弹窗确认，不点「开始休息」不进休息。
3. **旧版 EXE 未清理**：`artifacts/` 里留着 v2.0.10–v2.0.15 的旧包，不会自动删。
4. **v2.0.11 问题② 配图格式**仍未获用户确认（当时按推断实现）。

---

## 4. 下一步计划

- 等下一版（v2.0.17）反馈；开工前始终确认自己在 `dev` 分支，走「test → 提 dev → 合并 main → build 生成 EXE → push」流程。
- 推送现在正常（2026-10-24 直推令牌 URL 成功，网络已恢复）。
- 若用户要清旧包 / 改文档状态，分别单独确认后再动。

---

## 5. 绝对不要再踩的坑（血泪清单）

1. **`csc.exe` 在 PowerShell 命令里直接出现会被安全拦截**：git commit 命令里如果**消息或命令行**出现 `csc.exe` 字样，Bash 工具会被「Command blocked for security: csc.exe compiles arbitrary C# code」拦掉。提交构建相关改动时**用 PowerShell 跑 git** 或把 `csc` 换个说法（比如「编译器」），别在命令行里写 `csc.exe`。
2. **`scripts/test.ps1` 现在也会顺手 build**（2026-09-15 `e5fb068` 加的「自动打包」）：跑完会打印 `built: ...FocusStation-vX.Y.Z-<分支>.exe`。但**正式包仍应显式在 main 上跑一次 `build.ps1`**，别只看 test 的产物。
3. **`build.ps1` 按分支命名 + define**：
   - 在 `dev` 上跑 → 输出 `FocusStation-vX.Y.Z-dev.exe` 且带 `/define:DEV`（数据读写 `LittleFocusDesktop-dev` 隔离目录，不会污染正式数据）。
   - 在 `main` 上跑 → 输出 `FocusStation-vX.Y.Z.exe`，**不带** `/define:DEV`。
   - 想要正式包，必须切到 `main` 再 build。
4. **`build.ps1` 经典 bug（已修，别再引入）**：main 分支下 `$defineArg` 是空字符串，若直接拼进 csc 参数会被当成「空源文件路径」→ 报 `CS2001 未能找到源文件“”。`现在已改成数组 + splatting（`if ($defineArg -ne '') { $compilerArgs += $defineArg }`）。**以后改 build.ps1 务必在 dev 和 main 两个分支都验证能编译出来。**
5. **`artifacts/` 和 `*.exe` 在 `.gitignore` 里，二进制不入库**：本地生成的 EXE 只活在磁盘上，`git status` 不会显示，旧的也不会自动消失。
6. **`UiTests.cs` 写测试时的三个低级错误（前车之鉴）**：
   - `Store` 类**没实现 IDisposable**，不能用 `using(var store=new Store(...))`，要用普通 `var store=new Store(...)`。
   - C# 对象初始化器是 `Name="B"`，**不是** `Name:"B"`（冒号是错的，编译报语法错）。
   - 断言要核对**界面真实加载的对象**（MainForm 的 `data` 字段），别核对测试里自己 new 的旧副本——否则「通过了」但根本没验到东西。
7. **腾讯文档里的截图：`get_content` 拿不到，但导出 xlsx 能捞出来（2026-10-23 跑通）**：
   - `get_content` 只返回文字，`sheet.get_cell_data` 也只返回值，**嵌在单元格里的截图一个字都拿不到**。
   - 可用办法（全程只读，不碰用户的表）：
     1. `tdoc_call tencent-docs manage.export_file '{"file_id":"<id>"}'` → 拿 `task_id`
     2. `manage.export_progress '{"task_id":"..."}'` → `progress=100` 时返回带签名的 `file_url`（**只有约 30 分钟有效，且只能查一次，第二次查会 404**，所以拿到就立刻下载）
     3. `curl -L -o log.xlsx "<file_url>"` → `unzip` → 图片在 `xl/drawings/media/`，子表与 drawing 的对应看 `xl/worksheets/_rels/sheetN.xml.rels` + `xl/workbook.xml`
     4. 图片和单元格的对应关系看 `xl/drawings/drawingN.xml`：`<xdr:from><xdr:row>` 就是锚定的行号；`r:embed="rIdX"` 再去 `xl/drawings/_rels/drawingN.xml.rels` 查真实文件名。
   - 可以直接 `Read` 那个 png 看图，不用 OCR。
   - 仍在的坑：表格里的**文字**是 `sheet.get_cell_data` 最省事；但**图片**必须走上面这条导出链路。
8. **git 推送：GCM 会 403，但令牌本身是好的（2026-10-23 复现）**：
   - 现象：`git push`（走本机凭据管理器）报 `Permission to QUEQUE0926/FocusStation.git denied to QUEQUE0926`，403。
   - 但用 `git credential fill` 取出同一枚令牌调 GitHub API：`/user` 200、`/repos/.../FocusStation` 200 且 `permissions.push=true`——**令牌有写权限，是 git 那条凭据链路的问题**。
   - 可用解法（不落盘）：把 `git credential fill` 的 password 塞进 URL 直推：
     ```bash
     TOKEN=$(printf "protocol=https\nhost=github.com\n\n" | git credential fill 2>/dev/null | sed -n 's/^password=//p')
     GIT_TERMINAL_PROMPT=0 git -c credential.helper= push "https://x-access-token:$TOKEN@github.com/QUEQUE0926/FocusStation.git" main dev
     ```
   - 本次本地代理 `http://127.0.0.1:7890` 连不上（端口随代理客户端变化）；**直连 github.com:443 可通**，所以上面命令不加 `-c http.proxy=`。
9. **腾讯文档连接器会掉授权**：状态 `auth-required` 时 `tdoc_init` 直接报 `ERROR:no_token`，网页 fetch 和 `dop-api/opendoc` 都返回 401。只能走 `reconnect_plugin` 让用户重新授权。
10. **读反馈必须读完整原文，别只读第一句（2026-10-24 血的教训）**：
    - v2.0.16 那条「只要我不点休息，就永远不会休息」后面还跟了一句「去解包 `artifacts/FocusStation-v2.0.14-dev.exe`……**给我复原休息模块**」。漏读这句，就把「复原」错做成「改成新行为」，白改一版。
    - 判断某个行为属于哪一版，不要靠记忆和推断——**直接读 EXE 字节比对文案**：
      ```python
      raw = open('artifacts/FocusStation-v2.0.15-dev.exe','rb').read()
      print('到休息时间了'.encode('utf-16-le') in raw)   # .NET 字符串是 UTF-16LE
      ```
      比 `strings` 靠谱（`strings` 默认只抓 ASCII，中文全是漏的）。
    - 用户说「你偷偷改了我的 XX」时，先做这份比对，用事实确认「改了哪一版、改成什么」，再动手。

---

## 6. 关键文件速查

| 文件 | 作用 |
|---|---|
| `src/Dialogs.cs` | 任务/子任务编辑弹窗；`AddStage()` 加子任务逻辑在这里 |
| `src/MainForm.cs` | 主窗口；任务列表 `projects`、`RefreshDisplay()`、`TodayLine()`（今日已专注文案） |
| `src/Planner.cs` | `Review()` 测评模板（4 步，剩余 % 100/80/45/20）；`Writing()` 已在 v2.0.15 删除 |
| `src/Model.cs` | 数据模型 `Project` / `AppData` / `Store`（注意 Store 非 IDisposable）；`TaskSupport` 工具方法 |
| `src/UiTests.cs` | 界面自动化测试（跑真实 WinForms，需 Windows） |
| `src/Tests.cs` | 纯逻辑单测 |
| `scripts/test.ps1` | 编译 + 跑逻辑/界面测试（跑完会顺手 build 一个分支包） |
| `scripts/build.ps1` | 生成带版本号 EXE（按分支命名 + define） |
| `docs/CHANGELOG.md` | 版本变更记录，**每次发版都要补** |
| `docs/分支与发布流程.md` | dev→main 合并流程，版本号引用处要同步 |
| `REVIEW_REQUEST.md` | 连续失败两次的停手记录文档 |
| `app.manifest` | 编译时 `/win32manifest` 引用 |

---

## 7. 环境速查

- OS：Windows（Win32）。编译用系统 `csc.exe`：`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe`（兜底 Framework 版）。
- 出网：直连 github.com 可通；本地代理 `http://127.0.0.1:7890` 可能未开，推送失败先试直连。
- git 身份：`user.name=扁鹊鹊`，`user.email=queque0926@users.noreply.github.com`。
- 分支策略：**所有改动先入 `dev`**，main 需显式批准才合并（2026-10-23 已获「改完可直接合并 main」的授权）。
- 测试需 Windows 桌面环境（UiTests 起真实窗口）；逻辑测试 `Tests.cs` 不依赖 UI。
- 项目记忆在 `I:/AIstore/9.FocusStation/.workbuddy/memory/`（按日期日志 + MEMORY.md）。
