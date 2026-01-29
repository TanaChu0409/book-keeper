---
name: skill-builder
description: |
  建立或審查 Agent Skills（SKILL.md）並確保符合 agentskills.io 官方規格。
  何時使用：(1) 為專案新增 Agent Skill；(2) 驗證既有 SKILL.md 合規性；(3) 指導團隊編寫技能。
  包含命名規範、Frontmatter 要求、內容結構、安全檢查清單、驗證工具與實例。
license: MIT
compatibility: |
  - GitHub Copilot Chat（Web、VS Code 擴充套件）
  - 支援 Markdown 格式的編輯器
  - GitHub 存取權限（用於存放 .github/skills/）
metadata:
  category: governance
  requires-review: true
  version: "1.0"
  spec-version: "agentskills.io"
  tags:
    - formatting
    - naming-convention
    - security
    - compliance
    - best-practices
allowed-tools: |
  semantic_search
  read_file
  grep_search
  list_dir
  get_errors
  file_search
---

## 觸發情境
- 需要為專案新增新的 Agent Skill（專家角色）時
- 審查既有 SKILL.md 是否合規、可直接套用時
- 指導團隊如何撰寫/維護技能，避免使用非官方語法（例如 `@skill`）

## 操作步驟
1. **命名與位置**：
   - 目錄：`.github/skills/<skill-name>/`（或 `~/.copilot/skills/<skill-name>/` 供個人使用）。
   - `<skill-name>` 全小寫、使用連字號，與 `SKILL.md` frontmatter 的 `name` 一致。
2. **Frontmatter 必填**：
   - `name`: `<skill-name>`
   - `description`: 說明做什麼、何時載入（觸發情境）。
   - `license`（選填）：如 MIT / Apache-2.0 / 專案內部授權。
3. **主體內容需包含**：
   - 觸發情境：何時載入此技能。
   - 操作步驟：明確、可重複的流程，可標註建議工具（例如 `semantic_search`, `read_file`, `grep_search`）。
   - 檢查清單 / 輸出要求：行號、風險分級 (HIGH/MEDIUM/LOW)、DO/DON'T、需標記「人工審核」的條件。
   - 範例提示：1–3 個自然語言示例，供使用者直接貼給 Copilot。
   - （選填）附檔：scripts / examples / templates，可在同目錄新增。
4. **驗證**：
   - 檢查 `name` 與資料夾一致；禁止含空白或大寫。
   - `description` 清楚列「何時使用」與「做什麼」。
   - 無遺漏核心段落（觸發情境、步驟、檢查清單、範例）。
   - 內容無 `@skill` 指令式語法；全部使用自然語言。
   - 若使用工具/腳本，已標註用途與安全注意事項。
5. **安全與合規**：
   - 不得在技能中硬編碼密鑰、Token 或憑證。
   - 如需外部腳本，標註來源與用途，並避免觸發危險指令。

## 檢查清單 / 輸出要求
- [ ] 路徑：`.github/skills/<skill-name>/SKILL.md` 或 `~/.copilot/skills/<skill-name>/SKILL.md`
- [ ] `name`=目錄名（小寫、連字號）；`description` 說明何時用與做什麼
- [ ] 主體包含：觸發情境、操作步驟、檢查清單/輸出要求、範例提示
- [ ] 內容採自然語言，無 `@skill` 語法
- [ ] 若引用工具/腳本，已標註用途與安全注意事項

## 範例提示
- 「請依 skill-builder 步驟，為支付狀態機審查建立一個技能草稿」
- 「審查這個 SKILL.md 是否符合命名、frontmatter、檢查清單的規範」
- 「幫我生成一個新的 caching-review 技能，包含觸發情境、步驟、檢查清單與範例提示」

## 相關資源
- [常見錯誤與邊界情況](references/COMMON_PITFALLS.md) - 應避免的 10 項常見問題
- [官方規格](https://agentskills.io/specification) - agentskills.io 完整規範  
- [驗證腳本](scripts/validate-skill.ps1) - PowerShell 自動化驗證工具
- [Skill 範本](assets/SKILL_TEMPLATE.md) - 快速建立新 Skill 的標準範本
- 相關 Skill：
  - [writing-skills](../writing-skills/SKILL.md) - 文件撰寫最佳實踐
  - [test-driven-development](../test-driven-development/SKILL.md) - TDD 開發流程
