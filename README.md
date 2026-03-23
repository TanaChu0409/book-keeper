# BookKeeper

## 簡介
BookKeeper 是一個使用 .NET 8 構建的現代化個人記帳後端 API，採用 **Vertical Slice Architecture** 與 **CQRS** 模式，專注於管理收入 (Incomes)、支出 (Expenditures) 與標籤 (Labels)。專案採用 PostgreSQL 作為資料存儲、MediatR 處理業務邏輯、FluentValidation 進行請求驗證，並完整整合 OpenTelemetry 實現可觀測性。

## 主要功能
- **完整 CRUD 操作**: `labels`（標籤）、`incomes`（收入）、`expenditures`（支出）
- **分頁支援**: 所有列表查詢支援 `page` / `pageSize` 參數
- **Result Pattern**: 函數式錯誤處理，避免異常作為流程控制
- **自動端點探索**: 基於 `IEndpoint` 介面的自動註冊機制
- **Rich Domain Model**: 實體封裝業務邏輯，私有建構函式 + 靜態工廠方法
- **OpenTelemetry**: 完整追蹤與指標收集，支援 OTLP 匯出

## 架構特點
- **Vertical Slice Architecture (VSA)**: 按功能垂直切分，每個 Feature 包含 Command/Query、Validator、Handler、Endpoint
- **CQRS Pattern**: 使用 MediatR 實現 Command/Query 職責分離
- **Minimal API**: ASP.NET Core Minimal API 模式
- **Entity Framework Core**: PostgreSQL + Snake_case 命名轉換

## 使用技術
| 類別 | 技術 | 版本 |
|------|------|------|
| 運行時 | .NET | 8.0 |
| ORM | Entity Framework Core | 8.0.21 |
| 資料庫 | PostgreSQL | 17.2 |
| CQRS | MediatR | 12.5.0 |
| 驗證 | FluentValidation | 12.0.0 |
| 可觀測性 | OpenTelemetry | 1.13.1 |
| ID 生成 | Ulid | 1.4.1 |
| API 文檔 | Swashbuckle | 9.0.6 |

## 專案結構
```
BookKeeper/BookKeeper/
├── BookKeeper.Api/          # API 主專案
│   ├── Features/            # 功能切片（VSA）
│   │   ├── Labels/          # 標籤管理 CRUD
│   │   ├── Incomes/         # 收入管理 CRUD
│   │   └── Expenditures/    # 支出管理 CRUD
│   ├── Entities/            # 領域實體
│   ├── Database/            # EF Core DbContext + Configurations
│   ├── Contracts/           # API Request/Response DTO
│   ├── Endpoints/           # IEndpoint 介面定義
│   ├── Shared/              # Result Pattern + Error
│   ├── Middleware/          # 全域異常處理
│   └── Migrations/          # EF Core Migrations
├── docker-compose.yml       # Docker Compose 配置
└── BookKeeper.sln           # 解決方案檔

my-ai-swarm/                 # AI Copilot 記憶系統
├── project-memory.md        # 專案決策日誌與架構約束
├── copilot-service-memory.md# Features/Endpoints/Handlers 映射
├── QUICK-REFERENCE.md       # 快速參考指南
├── procedures/              # 工作流程文檔
└── decisions/               # 架構決策記錄
```

## 快速開始

### 前提需求
- .NET 8 SDK
- Docker Desktop（推薦）
- PostgreSQL Client（可選）

### 使用 Docker（推薦）
1. 啟動完整開發環境：

```bash
cd BookKeeper/BookKeeper
docker compose up --build
```

2. 訪問服務：
   - **API**: http://localhost:9000
   - **Swagger UI**: http://localhost:9000/swagger
   - **Aspire Dashboard** (OpenTelemetry): http://localhost:18888
   - **PostgreSQL**: localhost:5432 (user: postgres / password: postgres)

### 本地執行（不使用 Docker）
1. 設定連線字串：
   - 編輯 `BookKeeper.Api/appsettings.Development.json`
   - 或設定環境變數 `ConnectionStrings__Database`

2. 執行專案：

```bash
cd BookKeeper/BookKeeper
dotnet build BookKeeper.Api/BookKeeper.Api.csproj
dotnet run --project BookKeeper.Api/BookKeeper.Api.csproj
```

3. 開發環境會自動套用資料庫 Migrations

## 資料庫管理

### Migration 操作

```bash
# 新增 Migration
dotnet ef migrations add {MigrationName} -p BookKeeper/BookKeeper/BookKeeper.Api

# 套用 Migration（Development 自動套用）
dotnet ef database update -p BookKeeper/BookKeeper/BookKeeper.Api

# 查看 Migration 清單
dotnet ef migrations list -p BookKeeper/BookKeeper/BookKeeper.Api

# 移除最後一個 Migration（未套用前）
dotnet ef migrations remove -p BookKeeper/BookKeeper/BookKeeper.Api
```

### 連接 PostgreSQL 容器

```bash
docker exec -it bookkeeper.database psql -U postgres -d bookkeeper
```

## API 端點參考

### Auth（認證）
| HTTP | 端點 | 功能 |
|------|------|------|
| POST | `/api/auth/register` | 註冊帳號、指派 Member 角色並回傳 access/refresh token |
| POST | `/api/auth/login` | 驗證帳密並回傳新的 token pair |
| POST | `/api/auth/refresh` | 驗證 refresh token，過期/無效即清除並輪替新 token |

**說明**:
- JWT Bearer 驗證，Access Token 需放在 `Authorization: Bearer {token}`。
- Refresh Token 採單一活躍策略，新簽發會清除舊值。

### Labels（標籤管理）
| HTTP | 端點 | 功能 |
|------|------|------|
| GET | `/api/labels` | 分頁查詢所有標籤 |
| GET | `/api/labels/{id}` | 查詢單個標籤 |
| GET | `/api/labels/incomes` | 查詢收入類標籤 |
| GET | `/api/labels/expenditures` | 查詢支出類標籤 |
| POST | `/api/labels` | 建立標籤 |
| PUT | `/api/labels/{id}` | 更新標籤 |
| DELETE | `/api/labels/{id}` | 軟刪除標籤 |

**Request 範例** (POST):
```json
{
  "name": "薪資",
  "isIncome": true
}
```

### Incomes（收入管理）
| HTTP | 端點 | 功能 |
|------|------|------|
| GET | `/api/incomes` | 分頁查詢收入記錄 |
| GET | `/api/incomes/{id}` | 查詢單筆收入 |
| POST | `/api/incomes` | 建立收入記錄 |
| PUT | `/api/incomes/{id}` | 更新收入記錄 |
| DELETE | `/api/incomes/{id}` | 刪除收入記錄 |

**Request 範例** (POST):
```json
{
  "incomeName": "兼職收入",
  "amount": 1000.00,
  "incomeDateOnUtc": "2025-12-01",
  "labelId": "l_01J9KT..."
}
```

### Expenditures（支出管理）
| HTTP | 端點 | 功能 |
|------|------|------|
| GET | `/api/expenditures` | 分頁查詢支出記錄 |
| GET | `/api/expenditures/{id}` | 查詢單筆支出 |
| POST | `/api/expenditures` | 建立支出記錄 |
| PUT | `/api/expenditures/{id}` | 更新支出記錄 |
| DELETE | `/api/expenditures/{id}` | 刪除支出記錄 |

**Request 範例** (POST):
```json
{
  "paymentName": "晚餐",
  "amount": 200.00,
  "paymentDateOnUtc": "2025-12-01",
  "labelId": "l_01J9KT..."
}
```

### Statistics（統計查詢）

> 所有統計端點需要 JWT Bearer 驗證，使用者只能查詢自己的統計資料。

| HTTP | 端點 | 功能 |
|------|------|------|
| GET | `/api/statistics/daily` | 查詢每日收支統計（支援 `date` 或 `startDate`+`endDate` 過濾） |
| GET | `/api/statistics/weekly` | 查詢每週收支統計（必填 `year`、`month`，可選 `weekOfMonth`） |
| GET | `/api/statistics/monthly` | 查詢每月收支統計（必填 `year`，可選 `month`） |
| GET | `/api/statistics/yearly` | 查詢每年收支統計（可選 `year`，不填則返回所有年份） |

**Query Parameters（共用）:**
- `page` (optional, default: 1): 頁碼，需 ≥ 1
- `pageSize` (optional, default: 10): 每頁筆數，範圍 1-100

**每日統計 Query Parameters:**
- `date` (optional): 指定日期（格式 `YYYY-MM-DD`），與 `startDate`/`endDate` 互斥
- `startDate` (optional): 起始日期；若提供則 `endDate` 必填
- `endDate` (optional): 結束日期；若提供則 `startDate` 必填

**每週統計 Query Parameters:**
- `year` (required): 年份（1900–2100）
- `month` (required): 月份（1–12）
- `weekOfMonth` (optional): 月中第幾週（1–5）

**每月統計 Query Parameters:**
- `year` (required): 年份（1900–2100）
- `month` (optional): 月份（1–12）

**每年統計 Query Parameters:**
- `year` (optional): 年份（1900–2100），不填則返回所有年份

**Response 範例** (GET `/api/statistics/daily`):
```json
{
  "items": [
    {
      "date": "2025-12-01",
      "totalExpendAmount": 1500.00,
      "totalIncomeAmount": 5000.00,
      "sumAmount": 3500.00
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

## 錯誤處理
專案使用 **Problem Details** 標準與自訂例外處理器（`BookKeeper.Api/Middleware`）來一致回傳驗證錯誤與內部錯誤。

### 錯誤回應格式
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Amount": ["Amount must be greater than 0"]
  }
}
```

## AI Copilot 工作流程

BookKeeper 專案配置了完整的 AI Copilot 協作系統，支援 GitHub Copilot 與 Subagent 工作流程。

### 記憶系統文檔

| 文檔 | 用途 |
|------|------|
| [project-memory.md](./my-ai-swarm/project-memory.md) | 專案決策日誌、架構約束、命名約定 |
| [copilot-service-memory.md](./my-ai-swarm/copilot-service-memory.md) | Features/Endpoints/Handlers 完整映射 |
| [QUICK-REFERENCE.md](./my-ai-swarm/QUICK-REFERENCE.md) | 常用指令、快速檢查清單 |
| [WORKFLOW_ROUTES.md](./my-ai-swarm/procedures/WORKFLOW_ROUTES.md) | 任務複雜度判斷與角色路由 |

### 工作流程

根據任務複雜度，系統會自動選擇適當的流程：

| 任務類型 | 流程 | 參與角色 | 預計工時 |
|---------|------|---------|---------|
| Bug 修復 | 流程 C | Developer → QA → Memory | 1-2 天 |
| 新增 Feature | 流程 B | Architect → Developer → QA → Memory | 3-5 天 |
| 新增 Entity + CRUD | 流程 A | Architect → Impact → Developer → QA → Memory | 1-2 週 |

### 開始開發

1. 查閱 [project-memory.md](./my-ai-swarm/project-memory.md) 了解架構約束
2. 使用 [FEATURE_TEMPLATE.md](./my-ai-swarm/procedures/FEATURE_TEMPLATE.md) 規劃新功能
3. 遵循 [QUICK-REFERENCE.md](./my-ai-swarm/QUICK-REFERENCE.md) 的檢查清單
4. 完成後更新決策日誌

## 開發指南

### 新增 Feature 標準流程

1. **規劃階段**（Architect）
   - 確認需求清晰
   - 決定 Domain 分類
   - 設計 API 端點

2. **實作階段**（Developer）
   - 建立 Entity（若需要）+ EF Configuration
   - 建立 Feature 檔案（Command/Query + Validator + Handler + Endpoint）
   - 建立 Contracts（Request/Response）
   - 執行 Migration

3. **測試階段**（QA Reviewer）
   - 本地測試成功場景
   - 驗證錯誤處理
   - 檢查 OpenTelemetry Traces

4. **記錄階段**（Memory Manager）
   - 更新 `project-memory.md` 決策日誌
   - 更新 `copilot-service-memory.md` Feature 映射

詳細流程請參考 [FEATURE_TEMPLATE.md](./my-ai-swarm/procedures/FEATURE_TEMPLATE.md)

## 參考資源

- [.NET 8 文檔](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR](https://github.com/jbogard/MediatR/wiki)
- [FluentValidation](https://docs.fluentvalidation.net/en/latest/)
- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)

## 授權
（請根據專案需求補充授權信息）

---

**最後更新**: 2026-01-08  
**專案狀態**: ✅ Active Development  
**維護者**: GitHub Copilot + AI Swarm Agents