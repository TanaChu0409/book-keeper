# Common Pitfalls & Edge Cases

當撰寫或審查 Agent Skills 時，應避免的常見錯誤與邊界情況。

## 🔴 Critical（必須避免）

### 1. Frontmatter 缺失必填字段
**問題**：缺少 `name` 或 `description`，導致 Skill 無法被識別。

```yaml
# ❌ 錯誤
---
description: Some skill
---

# ✅ 正確
---
name: my-skill
description: What this skill does and when to use it
---
```

**修復**：檢查所有必填字段（name、description）必須存在且非空。

---

### 2. `name` 不符合命名規範
**問題**：`name` 包含大寫、空白、連字號不正確，違反 agentskills.io 規格。

```yaml
# ❌ 錯誤
name: MySkill              # 大寫不允許
name: my skill             # 空白不允許
name: -my-skill            # 以連字號開頭
name: my--skill            # 連續連字號
name: my-skill-           # 以連字號結尾

# ✅ 正確
name: my-skill             # 全小寫、連字號分隔
name: pdf-processing       # 字母、數字、連字號
name: data-analysis        # 簡短、清晰
```

**修復**：使用正規表達式驗證：`^[a-z0-9]+([a-z0-9\-]*[a-z0-9]+)?$`

---

### 3. `name` 與資料夾名稱不一致
**問題**：`SKILL.md` 中的 `name: pdf-processing` 但資料夾為 `pdf_processing/` 或 `PDFProcessing/`。

```
❌ 錯誤結構：
skills/
└── pdf_processing/
    └── SKILL.md (name: pdf-processing)

✅ 正確結構：
skills/
└── pdf-processing/
    └── SKILL.md (name: pdf-processing)
```

**修復**：確保資料夾名稱與 `name` 字段完全相同。

---

### 4. 技能中硬編碼密鑰、Token、憑證
**問題**：將 API Key、密碼、Session Token 直接寫入 SKILL.md，造成安全洩漏。

```markdown
# ❌ 危險
To authenticate, use token: `sk_live_abc123xyz`
API_KEY=your-secret-key

# ✅ 正確
To authenticate, use environment variable: `API_KEY`
Store credentials in `.env` or secure vault, never in skill files
```

**修復**：
1. 移除所有硬編碼的敏感資訊
2. 使用環境變數或安全保管庫參考
3. 在 COMMON_PITFALLS 文檔中明確警告

---

### 5. 技能內容直接複製外部專案
**問題**：將他人專案或文檔直接複製粘貼到 SKILL.md，觸發版權/合規風險。

```markdown
# ❌ 違反版權
[直接複製 GitHub/StackOverflow 內容]

# ✅ 正確做法
1. 理解原始內容概念
2. 用自己的文字重新撰寫
3. 明確標註來源與授權
4. 根據授權條款標註 license 字段
```

**修復**：始終編寫原創內容或明確標註來源與授權。

---

## 🟡 Warning（需要特別留意）

### 6. `description` 未說明「何時用」
**問題**：Description 只說功能，不說使用場景，AI 難以判斷何時激活此技能。

```yaml
# ❌ 不夠好
description: Processes PDF files

# ✅ 好
description: |
  Extract text and tables from PDFs, fill forms, merge documents.
  Use when working with PDF files or when user mentions PDFs, forms, or document extraction.
```

**修復**：確保 description 包含：
1. **功能**（做什麼）：Extract, merge, validate...
2. **觸發場景**（何時用）：When user mentions, if working with...
3. **關鍵字**：特定術語提升可發現性

---

### 7. Skill 內容超過 500 行
**問題**：單一 SKILL.md 超過 500 行，導致 Agent 加載時消耗大量 context token。

```markdown
# ❌ 問題結構
SKILL.md (1200 行，包含所有詳細文檔)

# ✅ 正確結構
SKILL.md (150 行，核心指導)
├── references/DETAILED_GUIDE.md (400 行)
├── references/EDGE_CASES.md (200 行)
└── assets/TEMPLATE.md (100 行)
```

**修復**：
- 保持 SKILL.md < 500 行（推薦 < 300 行）
- 將詳細內容移至 `references/` 目錄
- 使用相對路徑參考：`See [detailed guide](references/DETAILED_GUIDE.md)`

---

### 8. 包含 `@skill` 指令式語法
**問題**：在 Skill 中使用 `@skill` 或其他專有指令，違反自然語言原則。

```markdown
# ❌ 錯誤
@skill payment-flow-review
Please check @skill transaction-security

# ✅ 正確
When reviewing payment flows, ensure transaction security is checked (see related skill: transaction-security-audit)
For details, refer to payment-flow-review skill
```

**修復**：
- 移除所有 `@skill` 指令
- 使用自然語言描述關聯技能
- 在相關資源中列舉參考

---

### 9. 無 `metadata` 或 `allowed-tools` 字段
**問題**：缺少可選字段，影響自動化分類與工具預批准。

```yaml
# ❌ 最小化
---
name: skill
description: ...
---

# ✅ 推薦
---
name: skill
description: ...
metadata:
  category: review
  version: "1.0"
  tags: [compliance, security]
allowed-tools: read_file grep_search
---
```

**修復**：新增推薦的可選字段，支援工具自動化與元數據分類。

---

### 10. 缺少 `compatibility` 說明
**問題**：未明確指定環境/產品要求，用戶不知道是否適用。

```yaml
# ❌ 不清楚
description: A skill for Copilot

# ✅ 清楚
compatibility: |
  - GitHub Copilot Chat (VS Code, Web)
  - Requires internet access
  - Supports Linux, macOS, Windows
```

**修復**：根據實際環境要求補充 `compatibility` 字段。

---

## 📋 快速檢查清單

在提交 Skill 前，確認：

- [ ] `name` 與資料夾名稱完全一致（全小寫、連字號分隔）
- [ ] `name` 不含大寫、空白、連續連字號、首尾連字號
- [ ] `description` 1-1024 字符，包含「做什麼」+「何時用」+關鍵字
- [ ] 無硬編碼密鑰、Token、憑證
- [ ] 無直接複製的外部內容（版權風險）
- [ ] 無 `@skill` 指令式語法
- [ ] SKILL.md < 500 行（詳細內容移至 references/）
- [ ] 補充 `metadata` 與 `allowed-tools`（推薦）
- [ ] 補充 `compatibility`（如有特定環境要求）
- [ ] 包含 4 個核心段落：觸發情境、操作步驟、檢查清單、範例提示

---

## 🔗 參考資源

- 官方規格：https://agentskills.io/specification
- Skill 名稱驗證工具：使用 `skills-ref validate ./skill-name` 命令
- 許可協議查詢：https://spdx.org/licenses/
