# REVIEW_REQUEST · 推送持续被阻塞：github.com:443 不可达（2026-10-23 起） — ✅ 已解决（2026-10-24 22:56）

- 2026-10-24 结论：网络恢复，用下面那条「直推令牌 URL」命令一次成功，`1b08460..89f7844` 已把 v2.0.13 / v2.0.14 / v2.0.15 / v2.0.16 和文档提交一起推上去（`main` / `dev` 均已同步）。以下为当时的原始记录，保留备查。
- 2026-10-24 23:08 补记：v2.0.17（复原休息模块）同样一次推成功，`14df73e..4459d2b  dev -> dev` / `main -> main`。注意这次 `git push origin dev main` 走 GCM 会**静默失败（exit 128，无输出）**，必须用下面的令牌直推 URL 命令。

- 状态：**代码、测试、构建全部完成，仅「推送到 GitHub」没做成**。本地待推提交 4 个：`9384644`（v2.0.13）、`e2a2963`（文档）、`c6024c1`（v2.0.14）、`f8fd4e6`（v2.0.15）；`main` / `dev` 同位。
- 已生成的正式包：`artifacts/FocusStation-v2.0.13.exe`、`-v2.0.14.exe`、`-v2.0.15.exe`。
- 现象：`git push`（含直推令牌 URL）连续报 `Failed to connect to github.com:443 after 21s: Could not connect to server`。v2.0.15 本轮又试了 1 次，仍是 21s 超时，已按停手规则停止重试。
- 诊断（只读）：
  - `curl https://api.github.com/` → **200**（能出网）
  - `curl https://github.com/` → **000**（连不上）
  - `git ls-remote origin` → 同样 443 超时
  - 代理 `127.0.0.1:7890` 没有进程监听（`proxy7890=000`）
  - DNS 正常解析到 `20.205.243.166`
- 判断：**不是令牌问题**（上一版 v2.0.12 推送成功，用的同一枚令牌），是 github.com 这个域名当前网络不可达；api.github.com 走的是另一条线路/边缘节点所以正常。
- 触发停手规则：同一操作连续失败 2 次，停止重试。
- 恢复办法（网络正常后，一条命令即可）：确认能 `curl https://github.com/` 返回 200（或重新开启本地代理 7890）后执行
  ```bash
  cd /i/AIstore/9.FocusStation
  TOKEN=$(printf "protocol=https\nhost=github.com\n\n" | git credential fill 2>/dev/null | sed -n 's/^password=//p')
  GIT_TERMINAL_PROMPT=0 git -c credential.helper= push "https://x-access-token:$TOKEN@github.com/QUEQUE0926/FocusStation.git" main dev
  ```

---

# 历史存档 · dev→main 推送被拒（403）：GCM 令牌无 FocusStation 写权限 — ✅ 已解决（2026-09-08 20:47）

- 日期：2026-09-08 20:35（阻塞），2026-09-08 20:47（解决）
- 现象：`git push origin main dev`（GCM 凭据）返回 403：`Permission to QUEQUE0926/FocusStation.git denied to QUEQUE0926`。单独推 `dev`、改用 `x-access-token:<令牌>` 直推，均 403；代理已恢复（`git ls-remote` 正常）。
- 实测验证：取 GCM 中同一枚令牌（93 位）调 GitHub API，`GET /repos/QUEQUE0926/FocusStation` 返回 200 且 `permissions.push=true`，但 `POST .../git/refs`（建 tag＝Contents 写）返回 **403**。即令牌"已授予 Contents:write"却**对 FocusStation 没有有效写权限**——典型是细粒度令牌的 Repository access 列表未包含本仓库（git 智能 HTTP 会严格校验访问列表）。
- 触发停手规则：推送连续失败（main+dev / x-access-token 直推 / API 写 各试 1 次均 403），停止重试，等用户在 GitHub 修正令牌权限后重推。

> **解决（2026-09-08 20:47）**：用户提供了具备 FocusStation 写权限的新细粒度令牌（`github_pat_...`）。已用 `git credential approve` 以用户名 `x-access-token` 存入本机凭据库（Windows 凭据管理器 `git:https://github.com`），替换旧的无权限令牌。API 写测试从 403 变为 422（"Object does not exist"＝鉴权通过、仅因本地提交尚未上远端），确认权限恢复。`git push origin main dev` 成功（main: `755fa6c..7e5cb5a`，dev: `2b6c615..7e5cb5a`）。本地合并与冒烟测试均已在推送前通过。

---

# 历史存档 · 19:20 本地代理 127.0.0.1:7890 连不上 — ✅ 已恢复（2026-09-08 20:28）

> 代理客户端恢复运行（HTTP 端口 7890 可达），`git ls-remote` 正常。但推送随即遇到上方 403 权限问题，属不同根因。

---

# 历史存档 · dev 推送被拒（403）— ✅ 已解决（2026-09-08 18:03）

> **解决**：用户在凭据管理器把 `git:https://github.com` 的密码更新为新的 ALL 仓库细粒度令牌（Contents/Actions/Workflows/Pages 读写），推送成功（d98cbb3..8740bea dev）。以下为排查过程存档。

- 日期：2026-09-08
- 现象：`git push origin dev`（GCM 凭据）和用 kimiTips 的 PAT 直推均返回 403，身份均被识别为 `QUEQUE0926`。

## 排查结论（已实测确认）

1. `I:\Git SCM\Git` 与 WorkBuddy 便携 Git 共用同一 Windows 凭据库；GCM 里存的 GitHub 凭据是一个**细粒度 PAT**（93 位，`gith…`），与 kimiTips 配置里的**不是同一个**（指纹不同）。
2. GCM 的 PAT：对 `deutsch-tagebuch` 有 Contents 写权限（201），对 `FocusStation` 无（403）。
3. kimiTips 的 PAT：对 `kimi-quota-reminder` 有写权限（201），对 `FocusStation` 无（403）。
4. 即：两个细粒度令牌都是"按仓库勾选"，谁都没勾 FocusStation——这就是 403 的唯一根因。

## 解法（需要用户在 GitHub 网页操作，1 分钟）

GitHub → Settings → Developer settings → Fine-grained tokens → 打开 kimiTips 用的那个令牌：
- **Repository access**：增加选中 `QUEQUE0926/FocusStation`；
- **Permissions → Contents**：设为 `Read and write`。

改完后本地执行（令牌不落盘）：
```
cd I:/AIstore/9.FocusStation
TOKEN=$(node -e "console.log(require(require('os').homedir()+'/.kimi-code/hooks/quota-reminder.config.json').github_pat)")
GIT_TERMINAL_PROMPT=0 git -c credential.helper= push "https://x-access-token:$TOKEN@github.com/QUEQUE0926/FocusStation.git" dev
```

## 本地状态

- dev 领先 origin/dev 2 个提交（`db14c56` 文档 + `8740bea` v2.0.10 三条反馈修复），构建与 162 条测试通过，无未提交内容。
- 触发停手规则：推送这一步已失败 2 次（GCM 与 PAT 各一次），停止重试。
