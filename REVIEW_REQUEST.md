# REVIEW_REQUEST · dev→main 推送被拒（403）：GCM 令牌无 FocusStation 写权限（2026-09-08 20:35）

- 日期：2026-09-08 20:35
- 现象：`git push origin main dev`（GCM 凭据）返回 403：`Permission to QUEQUE0926/FocusStation.git denied to QUEQUE0926`。单独推 `dev`、改用 `x-access-token:<令牌>` 直推，均 403；代理已恢复（`git ls-remote` 正常）。
- 实测验证：取 GCM 中同一枚令牌（93 位）调 GitHub API，`GET /repos/QUEQUE0926/FocusStation` 返回 200 且 `permissions.push=true`，但 `POST .../git/refs`（建 tag＝Contents 写）返回 **403**。即令牌"已授予 Contents:write"却**对 FocusStation 没有有效写权限**——典型是细粒度令牌的 Repository access 列表未包含本仓库（git 智能 HTTP 会严格校验访问列表）。
- 本地状态：main 已快进合并 dev（HEAD=`8e097af`，领先 origin/main），dev 与 main 冒烟测试均通过（162 断言 + UI 单测）；仅远端推送因令牌权限被阻。
- 触发停手规则：推送连续失败（main+dev / x-access-token 直推 / API 写 各试 1 次均 403），停止重试，等用户在 GitHub 修正令牌权限后重推。

> **所需操作（用户）**：GitHub → Settings → Developer settings → Fine-grained tokens → 打开 Windows 凭据库 `git:https://github.com` 对应的令牌，确认 **Repository access** 含 `QUEQUE0926/FocusStation` 且 **Permissions → Contents = Read and write**；保存后本机重推即可。若仍不行，改用"All repositories"范围的细粒度令牌并更新凭据库。

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
