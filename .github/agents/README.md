# AI Agent Swarm 系統指南

> **版本**: v1.0.0 | **最後更新**: 2026-01-08 | **環境**: VS Code + GitHub Copilot

---

## 📋 概述

此目錄包含 5 個專用角色配置文件，用於實現 VS Code + GitHub Copilot 環境下的標準化工作流程。

## 🎭 5 個核心角色

### 1️⃣ [Architect](./architect.md) - 架構師
**職責**: 需求分析、設計規劃、生成實裝計畫

**調用時機**:
- 新功能開發前
- 重大重構前
- 需要技術方案設計時

**工作流程**:
```
Architect 收到需求
  ↓
分析現有代碼 (search)
  ↓
設計解決方案
  ↓
產出實裝計畫
  ↓
交由 Developer 實裝
```

---

### 2️⃣ [Impact Validator](./impact-validator.md) - 風險評估師
**職責**: 評估跨系統影響、識別風險、提供緩解策略

**調用時機**:
- 高複雜度變更（>500 行或涉及 2+ 服務）
- 涉及 API 簽名變更
- 涉及數據庫 Schema 變更
- 依賴庫升級

**工作流程**:
```
Impact Validator 收到 Architect 設計
  ↓
掃描依賴圖
  ↓
識別風險
  ↓
提供緩解策略
  ↓
批准或提出修改建議
  ↓
交由 Developer 實裝
```

---

### 3️⃣ [Developer](./developer.md) - 開發工程師
**職責**: 嚴格執行計畫、實裝代碼、本地測試

**調用時機**:
- Architect 計畫已批准
- Impact 評估已完成（高風險需求）
- 準備開始代碼實裝

**工作流程**:
```
Developer 收到 Architect 計畫
  ↓
驗證計畫清晰性
  ↓
按步驟實裝代碼
  ↓
本地測試
  ↓
提交 QA Reviewer
```

---

### 4️⃣ [QA Reviewer](./qa-reviewer.md) - 品質審查員
**職責**: 代碼質量檢查、設計合規驗證、測試覆蓋審查

**調用時機**:
- Developer 代碼實裝完成
- 需要品質把關
- 準備上線前

**工作流程**:
```
QA Reviewer 收到 Developer 代碼
  ↓
驗證前置條件
  ↓
進行代碼審查
  ↓
檢查測試覆蓋
  ↓
批准或要求修改
  ↓
交由 Memory Manager 記錄
```

---

### 5️⃣ [Memory Manager](./memory-manager.md) - 記憶管理員
**職責**: 決策日誌記錄、版本維護、跨檔案同期

**調用時機**:
- QA Reviewer 批准代碼
- 需要更新 project-memory.md
- 需要版本升級

**工作流程**:
```
Memory Manager 收到 QA 批准
  ↓
驗證所有前置條件
  ↓
格式化決策記錄
  ↓
驗證代碼位置
  ↓
記錄到 project-memory.md
  ↓
同期更新相關文檔
  ↓
完成
```

---

## 🚀 快速使用

### 場景 1：簡單 Bug 修復（流程 C）
```
User: 支付功能返回 500 錯誤

Copilot 自動判斷: 低複雜度
  → 調用 Developer（直接修復）
  → 調用 QA Reviewer（快速審查）
  → 調用 Memory Manager（記錄決策）

預期工時: 4-8 小時
```

### 場景 2：中等功能優化（流程 B）
```
User: 優化 LifePaymentRecord 查詢性能

Copilot 自動判斷: 中複雜度
  → 調用 Architect（設計索引優化方案）
  → 調用 Developer（實裝優化）
  → 調用 QA Reviewer（驗證效能改善）
  → 調用 Memory Manager（記錄決策）

預期工時: 3-5 天
```

### 場景 3：新功能開發（流程 A）
```
User: 新增批量用戶導入功能

Copilot 自動判斷: 高複雜度
  → 調用 Architect（完整設計）
  → 調用 Impact Validator（風險評估）
  → 調用 Developer（實裝功能）
  → 調用 QA Reviewer（品質檢查）
  → 調用 Memory Manager（記錄決策）

預期工時: 1-2 週
```

---

## 📚 工作流程文檔

| 文檔 | 用途 |
|------|------|
| [WORKFLOW_ROUTES.md](../my-ai-swarm/procedures/WORKFLOW_ROUTES.md) | 流程判斷樹 (A/B/C)、角色職責 |
| [WORKFLOW_CHECKLIST.md](../my-ai-swarm/procedures/WORKFLOW_CHECKLIST.md) | 各角色前置檢查清單 |
| [WORKFLOW_HANDOFF.md](../my-ai-swarm/procedures/WORKFLOW_HANDOFF.md) | Handoff 交接標準 |
| [REQUIREMENT_TEMPLATE.md](../my-ai-swarm/procedures/REQUIREMENT_TEMPLATE.md) | 用戶需求提交模板 |

---

## 🎯 核心設計原則

### 1. 自動化檢查
每個角色都有前置檢查清單，不符合條件的任務會被**自動拒絕**，無法跳過驗證。

### 2. 完全追蹤
所有決策都記錄在 `project-memory.md`，包含代碼位置（文件 + 行號）。

### 3. 無缝流轉
角色間的 Handoff 點有明確的驗證標準，確保信息完整無遺漏。

### 4. 語言統一
- **說明文字**: 繁體中文 (zh-TW)
- **代碼與技術術語**: 英文
- **命名約定**: 遵循 C# PascalCase / camelCase

---

## 📞 常見問題

**Q1: 如何手動調用特定 Agent?**

A: 在對話中明確指定，例如：
```
@Architect 請幫我設計批量導入功能
```

**Q2: 如果流程中途發現問題怎麼辦?**

A: 對應角色會拒絕並回報缺失項，由責任人補充後重新提交。

**Q3: 是否可以跳過某個角色?**

A: 不可以。前置檢查清單會強制執行完整流程。

**Q4: 決策記錄的代碼位置如何取得?**

A: 使用 VS Code 在代碼上右鍵 → 「複製相對路徑」，然後添加行號即可。

---

## 📊 工作流程決策表

| 需求特徵 | 複雜度 | 流程 | 參與角色 | 工時 |
|---------|--------|------|---------|------|
| 新功能 + 高複雜 | 高 | A | Architect → Impact → Developer → QA → Memory | 1-2 週 |
| 新功能 + 低複雜 | 低 | B | Architect → Developer → QA → Memory | 3-5 天 |
| 重構 >500 行 | 高 | A | 同上 | 1-2 週 |
| 重構 200-500 行 | 中 | B | 同上 | 3-5 天 |
| 優化 <200 行 | 低 | B | Architect → Developer → QA → Memory | 3-5 天 |
| Bug 修復 | 低 | C | Developer → QA → Memory | 4-8 小時 |
| 緊急修復 | 低 | C | 同上 | <4 小時 |

---

## 🔧 技術參考

### 環境配置
- **IDE**: Visual Studio Code
- **AI**: GitHub Copilot (Claude Sonnet 4.5)
- **Language**: C# .NET 8
- **Database**: SQL Server
- **Framework**: ASP.NET Core Web API

### 相關文件
- `.github/copilot-instructions.md` - 主配置文件
- `my-ai-swarm/project-memory.md` - 決策日誌
- `my-ai-swarm/procedures/` - 工作流程文檔

---

## 📢 版本歷程

| 版本 | 日期 | 內容 |
|------|------|------|
| v1.0.0 | 2026-01-08 | 初始發布，包含 5 角色系統與 3 流程架構 |

---

**維護者**: AI Infrastructure Team | **狀態**: ✅ 已驗證並活躍
