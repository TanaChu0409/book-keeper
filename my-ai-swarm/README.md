# 📚 BookKeeper Project Memory Hub

> AI Agent 維護的專案決策、模式與文件中樞  
> 版本：v1.0 | 日期：2026-01-06 | 狀態：🟢 Active

---

## 🎯 快速導航

### 🔴 **核心文件（必讀）**

| 文件 | 用途 | 新手 | AI Agent | 開發者 |
|------|------|------|---------|------|
| [project-memory.md](./project-memory.md) | 架構決策、程式碼模式、禁止操作 | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ |
| [QUICK-REFERENCE.md](./QUICK-REFERENCE.md) | 快速指令、決策樹、常見任務 | ⭐⭐ | ⭐⭐ | ⭐⭐⭐ |

### 📋 **工作流程文檔**

| 文件 | 用途 | AI Agent |
|------|------|---------|
| [procedures/WORKFLOW_ROUTES.md](./procedures/WORKFLOW_ROUTES.md) | 3 種流程路由（A/B/C）決策樹 | ⭐⭐⭐ |
| [procedures/WORKFLOW_CHECKLIST.md](./procedures/WORKFLOW_CHECKLIST.md) | 5 角色前置檢查清單 | ⭐⭐⭐ |
| [procedures/WORKFLOW_HANDOFF.md](./procedures/WORKFLOW_HANDOFF.md) | 角色交接驗證程序 | ⭐⭐⭐ |
| [procedures/REQUIREMENT_TEMPLATE.md](./procedures/REQUIREMENT_TEMPLATE.md) | 需求提交模板 | ⭐⭐ |
| [procedures/FEATURE_TEMPLATE.md](./procedures/FEATURE_TEMPLATE.md) | Vertical Slice 功能模板 | ⭐⭐⭐ |

---

## 🚀 快速開始（新開發者）

### **第 1 天：理解架構**

1. **閱讀** [project-memory.md](./project-memory.md) § 專案概述與架構決策（15 分鐘）
2. **閱讀** § 核心程式碼模式（30 分鐘）
   - Vertical Slice Feature 結構
   - Entity Factory Pattern
   - Result Pattern
   - 自動端點註冊
3. **實作練習**：查看 `GetExpenditures.cs` - 最簡單的查詢模式

**時間投資**: 60 分鐘  
**成果**: 理解專案的獨特架構模式

---

### **第 2 天：實作第一個功能**

1. **參照** [project-memory.md § 新增 Feature 完整檢查清單](./project-memory.md#新增-feature-完整檢查清單)
2. **使用模板** [procedures/FEATURE_TEMPLATE.md](./procedures/FEATURE_TEMPLATE.md)
3. **練習任務**: 新增 `GetExpendituresByDateRange` 查詢功能

**時間投資**: 2-3 小時  
**成果**: 完成一個完整的 Vertical Slice 功能

---

### **第 3 天：進階模式**

1. **研究** `CreateExpenditure.cs` - 完整的驗證與錯誤處理
2. **研究** `UpdateExpenditure.cs` - 更新模式與樂觀鎖
3. **練習任務**: 新增 `SoftDeleteExpenditure` 功能

**時間投資**: 3-4 小時  
**成果**: 掌握 CQRS + Result Pattern

---

## 🤖 AI Agent 使用指南

### **Architect Agent**

**準備工作**：
1. 閱讀 [project-memory.md](./project-memory.md) - 理解架構約束
2. 閱讀 [procedures/WORKFLOW_ROUTES.md](./procedures/WORKFLOW_ROUTES.md) - 決定流程類型

**規劃階段**：
1. 使用 [procedures/REQUIREMENT_TEMPLATE.md](./procedures/REQUIREMENT_TEMPLATE.md) 分析需求
2. 檢查 [project-memory.md § 禁止操作與反模式](./project-memory.md#禁止操作與反模式)
3. 參照既有 Feature 檔案作為設計範本
4. 產出詳細的實裝計畫

---

### **Developer Agent**

**實裝階段**：
1. 驗證 Architect 計畫完整性（[procedures/WORKFLOW_CHECKLIST.md](./procedures/WORKFLOW_CHECKLIST.md)）
2. 使用 [procedures/FEATURE_TEMPLATE.md](./procedures/FEATURE_TEMPLATE.md) 作為骨架
3. 遵循 [project-memory.md § 命名約定速查表](./project-memory.md#命名約定速查表)
4. 執行完整檢查清單（[project-memory.md § 新增 Feature 完整檢查清單](./project-memory.md#新增-feature-完整檢查清單)）

**完成標準**：
- ✅ 所有 Phase 1-7 檢查項通過
- ✅ 本地測試成功
- ✅ Swagger 文檔正確

---

### **QA Reviewer Agent**

**審查標準**：
1. 檢查 [procedures/WORKFLOW_CHECKLIST.md § QA Reviewer 的前置檢查](./procedures/WORKFLOW_CHECKLIST.md#4️⃣-qa-reviewer-的前置檢查)
2. 驗證程式碼符合 [project-memory.md § 核心程式碼模式](./project-memory.md#核心程式碼模式)
3. 確認沒有違反 [project-memory.md § 禁止操作](./project-memory.md#禁止操作與反模式)

---

### **Memory Manager Agent**

**記錄任務**：
1. 檢查 [procedures/WORKFLOW_CHECKLIST.md § Memory Manager 的前置檢查](./procedures/WORKFLOW_CHECKLIST.md#5️⃣-memory-manager-的前置檢查)
2. 更新 [project-memory.md § 架構決策日誌](./project-memory.md#架構決策日誌)
3. 同步更新相關文檔

---

## 📊 專案統計

### **當前狀態**（2026-01-06）

| 領域 | 功能數 | 端點數 | 狀態 |
|------|--------|--------|------|
| Labels | 7 | 7 | ✅ 完成 |
| Expenditures | 5 | 5 | ✅ 完成 |
| Incomes | 5 | 5 | ✅ 完成 |
| **總計** | **17** | **17** | ✅ |

### **技術棧**

- **.NET**: 8.0
- **資料庫**: PostgreSQL 17.2
- **ORM**: Entity Framework Core 8.0.21
- **CQRS**: MediatR 12.5.0
- **驗證**: FluentValidation 12.0.0
- **可觀測性**: OpenTelemetry 1.13.1

---

## 🔄 近期更新

| 日期 | 更新 | 類型 |
|------|------|------|
| 2026-01-06 | 建立專案記憶體系 | 🎉 初始化 |
| 2026-01-06 | 創建 AI Swarm 工作流程 | 📋 流程 |
| 2026-01-06 | 完成 Vertical Slice 模式文檔 | 📚 文檔 |

---

## 🎓 學習資源

### **必讀文章**

1. **Vertical Slice Architecture**
   - [Jimmy Bogard - Vertical Slice Architecture](https://jimmybogard.com/vertical-slice-architecture/)
   
2. **Result Pattern**
   - [Vladimir Khorikov - Functional C#](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/)

3. **MediatR & CQRS**
   - [Jimmy Bogard - MediatR](https://github.com/jbogard/MediatR)

### **範例程式碼**

| 模式 | 範例檔案 | 說明 |
|------|---------|------|
| 簡單查詢 | `GetExpenditures.cs` | 分頁查詢 |
| 創建操作 | `CreateExpenditure.cs` | 完整驗證 + Factory |
| 更新操作 | `UpdateExpenditure.cs` | 實體更新方法 |
| 錯誤處理 | `Result.cs` | Result Pattern 實現 |

---

## 📞 支援與問題

### **遇到問題時**

1. **檢查** [project-memory.md § 禁止操作](./project-memory.md#禁止操作與反模式) - 是否違反規則？
2. **搜尋** [project-memory.md § 架構決策日誌](./project-memory.md#架構決策日誌) - 是否有相關決策？
3. **參照** 既有 Feature 檔案 - 有類似實現嗎？
4. **諮詢** Architect Agent - 使用 @dotnet-architect

---

## 🔐 重要提醒

### **絕對禁止**

❌ 在 Handler 中拋出 Exception（使用 Result.Failure 代替）  
❌ 公開 Entity 建構函式（使用私有 + Create() 方法）  
❌ 手動註冊端點（使用 IEndpoint 自動掃描）  
❌ 跳過 FluentValidation（每個 Command 都要有 Validator）  
❌ 分散 Feature 類別到不同檔案（所有類別巢狀在一個檔案）

### **最佳實踐**

✅ 一個功能 = 一個檔案（Vertical Slice）  
✅ 使用 ULID + 前綴作為 Entity Id  
✅ 所有時間使用 UTC  
✅ 使用 Result Pattern 處理錯誤  
✅ EF Configuration 明確配置所有屬性

---

**維護者**: AI Infrastructure Team  
**最後更新**: 2026-01-06  
**版本**: v1.0
