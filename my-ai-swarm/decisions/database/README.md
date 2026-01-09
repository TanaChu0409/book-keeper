# 資料庫變更記錄 (Database Change Records)

> **版本**: v1.0.0 | **最後更新**: 2026-01-08 | **用途**: 記錄重要資料庫 Schema 變更與 Migration 細節

---

## 📋 概述

此目錄存放 BookKeeper 專案的資料庫 Schema 變更記錄。每個記錄應包含：

- **變更目的** (Purpose): 為什麼需要變更 Schema？
- **變更內容** (Changes): 新增/修改/刪除的表格、欄位、索引
- **Migration 編號** (Migration ID): 對應的 EF Core Migration 名稱
- **資料遷移策略** (Data Migration): 如何處理既有資料（若有）
- **回滾計畫** (Rollback Plan): 若變更失敗如何回滾
- **效能影響** (Performance Impact): 變更對查詢效能的影響

---

## 📂 Migration 清單

| Migration | 日期 | 變更內容 | 檔案 |
|-----------|------|---------|------|
| `InitialCreate` | 2026-01-06 | 建立 Labels, Incomes, Expenditures 表格 | `Migrations/Application/` |

---

## 🗄️ 當前 Schema 概覽

### Schema: `application`

| 表格 | 主鍵 | 索引 | 外鍵 |
|------|------|------|------|
| **labels** | `id` (varchar) | - | - |
| **incomes** | `id` (varchar) | `label_id` | → `labels(id)` |
| **expenditures** | `id` (varchar) | `label_id` | → `labels(id)` |

### Schema: `identity`

| 表格 | 主鍵 | 用途 |
|------|------|------|
| **AspNetUsers** | `id` | ASP.NET Identity 用戶 |
| **AspNetRoles** | `id` | ASP.NET Identity 角色 |
| **RefreshTokens** | `id` | JWT Refresh Token |

---

## 📝 未來變更範例

待專案發展時，可記錄的資料庫變更：

- **新增 Categories 表格**（標籤階層）
- **新增 Attachments 表格**（檔案附件）
- **新增 RecurringTransactions 表格**（週期性交易）
- **新增索引優化**（提升查詢效能）
- **新增全文搜尋**（支援名稱搜尋）

---

## 🔧 Migration 命名規則

```bash
# 新增表格
dotnet ef migrations add AddCategoriesTable

# 修改欄位
dotnet ef migrations add AddAmountDecimalPrecision

# 新增索引
dotnet ef migrations add AddIndexOnExpenditureDate

# 資料遷移
dotnet ef migrations add MigrateOldLabelFormat
```

---

**最後更新**: 2026-01-08  
**維護者**: GitHub Copilot
