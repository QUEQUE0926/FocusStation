# 专注小站 · FocusStation（LittleFocus Desktop）

一款 **本机运行的 Windows 桌面专注计时工具**，面向容易分心、启动困难的人：把"想做的事"拆成小步、计时、更易开始，也更易在中断后接上。

- **不是联网服务**：所有业务数据存在本机 `data.xml`，不上云、不依赖账号。
- **两个可选外部依赖**：①可替换的 AI 接口（任务拆解 / 鼓励）；②可选的 WebDAV 同步（多设备共享同一份数据）。两者都**失败不连累主功能**（fail-open）。
- **当前版本**：v2.0.10（见 `scripts/build.ps1` 输出名）。

---

## 目录结构

```
FocusStation/
├── app.manifest              # Windows 运行清单（UAC / DPI 等）
├── README.md                 # 本文件
├── src/                      # C# 源码（直接用 csc 编译，无 .csproj/.sln）
│   ├── Model.cs              # 数据模型、Store 存储、FocusEngine 引擎
│   ├── Planner.cs            # AI 拆解 / 分析逻辑
│   ├── Sync.cs               # WebDAV 同步（ETag 幂等）
│   ├── Dialogs.cs            # 各类设置/接续卡对话框
│   ├── MiniTimer.cs          # 迷你计时窗
│   ├── MainForm.cs           # 主窗口与编排
│   ├── Program.cs            # 入口、数据目录、单实例锁
│   ├── Tests.cs              # 逻辑单元测试（自带 Main）
│   └── UiTests.cs            # 界面单元测试（自带 Main）
├── policies/                 # 作为资源嵌入的三个策略文档
│   ├── i-have-adhd-policy.md
│   ├── ai-runtime-policy.md
│   └── adhd-friendly-policy.md
├── scripts/
│   ├── build.ps1             # 编译为 winexe
│   └── test.ps1              # 编译并运行三套测试
└── docs/                     # 工程治理文档（详见下方索引）
```

> 用户数据路径在仓库**之外**：`%LocalAppData%\LittleFocusDesktop\data.xml`，整理文件夹时**绝不触碰**。

---

## 构建与运行

需要 Windows + .NET Framework 4.x（构建脚本会自动探测 `csc.exe`）。

```powershell
# 编译（产物输出到仓库外的 I:\AIstore\专注小站-v2.0.10.exe）
powershell -File scripts/build.ps1

# 运行全部测试（逻辑单测 + 界面测试版 + 界面单测，产物在仓库外的 work/）
powershell -File scripts/test.ps1
```

测试在 `UI_TEST` 定义下会把数据落到隔离的 `testdata/` 目录，不会污染真实用户数据。

---

## 文档索引（文档与代码互锁）

| 文档 | 对应手册条目 | 作用 |
|---|---|---|
| [docs/项目方案.md](docs/项目方案.md) | 全局 | 产品定位、架构、路线、成熟度与风险 |
| [docs/工程实践对照.md](docs/工程实践对照.md) | 手册全文 | 对照手册逐条体检，标出已做/缺口 |
| [docs/假设清单.md](docs/假设清单.md) | 0.2 | 假设 / 验证方式 / 出错表现 = 风险登记表 |
| [docs/CHANGELOG.md](docs/CHANGELOG.md) | 3.3 | 按日期的修订记录，写清每次逻辑定型原因 |

---

## 分支约定

- `main`：稳定基线，只接收已定型、已验证的改动（含本次治理文档与目录整理）。
- `dev`：工作分支，新功能 / 实验在此进行，稳定后合回 `main`（需显式确认）。

---

## 凭据与安全

- API 密钥仅以 **Windows 凭据加密**存放在本机，绝不进版本库、不进备份。
- 同步密码仅用于 WebDAV 认证，不写入数据文件。
- 详见 [docs/假设清单.md](docs/假设清单.md) 与 [docs/工程实践对照.md](docs/工程实践对照.md)。
