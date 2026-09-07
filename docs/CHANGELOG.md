# 修订记录（CHANGELOG）

> 对应手册 3.3：文档末尾保留按日期的修订记录，写清每次逻辑定型的原因（哪些方案被实测推翻）。
> 新条目写在最上面。

---

## 2026-09-08 · dev 数据隔离（数据目录与同步配置分离）

- **问题**：`src/Program.cs` 把数据目录写死为 `%LocalAppData%\LittleFocusDesktop`，而 `build.ps1` 只做了 exe 的分支命名。结果是 dev 版与正式版读写同一份 `data.xml` 和 `sync.xml`；在 dev 上测试 WebDAV 同步，会通过 `DataMerge` 把测试记录双向合并进正式数据。
- **改动**：
  - `scripts/build.ps1`：非 main 分支编译时注入 `/define:DEV`（与已有的分支命名同源判定，读 `.git/HEAD`，不依赖 git CLI）。
  - `src/Program.cs`：`#if UI_TEST` > `#elif DEV` > 默认正式目录；dev 走 `%LocalAppData%\LittleFocusDesktop-dev`，`sync.xml` 随目录一并隔离。
  - `src/MainForm.cs` / `src/MiniTimer.cs`：窗口标题、托盘提示、迷你计时条改用 `Program.AppName`，dev 版显示 `专注小站 (dev)`。
- **原因**：exe 分开但数据不分，等于「两个门牌、一个房间」——测试动作会直接落在真实数据上。
- **附带清理**：删除临时脚本 `_fix_garbled.py`。该脚本针对一个并不存在的「乱码文件名」：已扫描全仓库 6 个提交、所有分支、索引与磁盘，非 UTF-8 路径数为 0；git 显示的 `\345\210\206` 是 `core.quotepath` 的八进制转义，属显示转义而非存储乱码。
- **验证**：dev 构建 `FocusStation-v2.0.10-dev.exe` 内含 `LittleFocusDesktop-dev` 与 `专注小站 (dev)`；以 main 分支配置构建仍产出 `FocusStation-v2.0.10.exe`，数据目录保持正式路径，未受影响。
- **注意**：dev 版首次启动数据为空，不会自动复制正式数据；需要带真实数据复现时手动复制 `data.xml` / `sync.xml` 到 dev 目录。

---

## 2026-09-08 · 分支治理补齐（main/dev 双分支模型）

- **新增分支治理资产**（对应手册 2.2 / 2.3 / 2.4 / 3.4）：
  - `.github/workflows/ci.yml`：push dev 与 PR main 时自动 `build.ps1` → `test.ps1`；`concurrency` group 设 `cancel-in-progress: false`，`checkout` 用 `fetch-depth: 0` 保留完整历史；非生产环境自动注入 `PUSH_TEST=1`。
  - `.github/workflows/ops.yml`：`workflow_dispatch` 手动运维入口（`mode: build/test/build+test`），带参数校验，且守卫 `main` 禁止手动触发（生产守卫）。
  - `.github/PULL_REQUEST_TEMPLATE.md`：合并前检查清单，逐项对应手册（构建/测试/rebase/文档互锁/失效行为/回归用例/兼容旧数据）。
  - `docs/分支与发布流程.md`：分支模型、环境标记、CI/运维入口、合并流程、rebase 纪律、本地代理/凭据约定。
- **原因**：仓库已有 main+dev 分支，但缺"存在分支时应有的验证与运维入口"。本次按手册补齐，使 dev 的任何改动在合并前都有自动构建+测试把关，且生产分支有手动触发守卫。
- **未做（待实测）**：UI 测试在 CI 无显示器 runner 上的可行性待验证；若失败再据手册调整（如改为非阻塞或自托管 runner）。

---

## 2026-09-08 · 治理资产补齐 + 目录结构化（main 基线）

- **目录整理**：源码归入 `src/`，策略文档归入 `policies/`，脚本归入 `scripts/`，治理文档归入 `docs/`；同步更新 `build.ps1`/`test.ps1` 的资源与源码路径。`app.manifest` 保留在仓库根。
- **新增治理文档**（均不改动业务代码，低风险）：
  - `README.md`：项目概览、构建/运行、文档索引、分支约定（手册 3.2 文档与代码互锁）。
  - `docs/项目方案.md`：产品定位、架构、功能矩阵、成熟度、风险与路线图。
  - `docs/假设清单.md`：假设/验证/出错表现三列表 = 风险登记表（手册 0.2）。
  - `docs/工程实践对照.md`：对照手册逐条体检表。
  - `docs/CHANGELOG.md`：本文件。
- **`.gitignore`** 增补 `.workbuddy/`（本地记忆不入库）。
- **原因**：此前代码已满足手册 MVP 级要求，但缺"写下来的治理资产"。本次补齐文档层，使 main 成为可审计、可交接的稳定基线。
- **未做（明确排除）**：手册中调度双保险 / heartbeat / 多平台注册表对本机应用不适用，不补。

---

## 基线说明（v2.0.10）

以下为整理前已存在的核心能力，记于此以便追溯（非本次变更）：

- 凭据卫生：Windows 凭据加密，绝不入库，备份不含密钥（手册 1.1 ✅）。
- 状态版本：`AppData.Version == 1` + `AiDefaults.Migrate` 迁移（手册 1.2 ✅）。
- 幂等：RunId 去重、原子写、合并幂等，均有测试（手册 1.3 ✅）。
- fail-open：AI/同步失败不影响主计时（手册 1.4 ✅）。
- 测试隔离：`UI_TEST` 落 `testdata/`，不碰真实数据（手册 1.5 ✅）。
- 单调性闸门：合并取 `UpdatedUtc` 最新（手册 2.2 ✅）。
- 假成功检查：校验响应体结构（手册 2.3 ✅）。
- 状态可审计：`.bak`/`.damaged-*` 备份（手册 2.4 ✅ 部分）。
- **缺口（留给 dev 分支）**：本地结构化事件日志（手册 1.6）、事故→回归用例映射（手册 3.1）。
