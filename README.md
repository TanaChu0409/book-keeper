# BookKeeper

## 簡介
- BookKeeper 是一個簡單的個人記帳後端 API，用於管理收入 (Incomes)、支出 (Expenditures) 與標籤 (Labels)。

## 主要功能
- 建立 / 查詢 / 更新 / 刪除：`labels`, `incomes`, `expenditures`
- 分頁支援（`page` / `pageSize`）
- 使用 Entity Framework Core + PostgreSQL 儲存資料
- 使用 MediatR 處理 CQRS-like 指令/查詢
- FluentValidation 驗證請求
- OpenTelemetry 可觀測性並支援 OTLP 匯出

## 使用技術
- .NET 8 (ASP.NET Core)
- Entity Framework Core (Npgsql)
- PostgreSQL
- MediatR, FluentValidation
- OpenTelemetry

## 專案結構（重點）
- BookKeeper.Api/: API 程式碼 (Endpoints, Features, Database, Entities)
- docker-compose.yml: Docker 開發環境（包含 PostgreSQL 與 aspire-dashboard）
- BookKeeper.Api/Dockerfile: API Dockerfile

## 快速開始（前提）
- 安裝 .NET 8 SDK
- 安裝 Docker (若使用 docker-compose)

## 直接執行
1. 設定連線字串：編輯 `BookKeeper.Api/appsettings.json` 或在環境變數中設定 `ConnectionStrings__Database`。
   - 範例連線字串 (development 範例已在 `BookKeeper.Api/appsettings.Development.json`):
     `Host=bookkeeper.database;Port=5432;Database=bookkeeper;Username=postgres;Password=postgres`
2. 在 `BookKeeper.Api` 資料夾執行：

```bash
dotnet build BookKeeper.Api/BookKeeper.Api.csproj
dotnet run --project BookKeeper.Api/BookKeeper.Api.csproj
```

3. 開發環境下啟動時會自動套用資料庫遷移（請確認連線字串正確）。

## 使用 Docker（推薦開發）
1. 啟動服務：

```bash
docker compose up --build
```

2. 服務與常用連接
- API (預設對應)：http://localhost:9000 （容器內埠 8080 映射到主機 9000）
- Swagger UI（開發環境）：http://localhost:9000/swagger
- PostgreSQL：localhost:5432（user: postgres / password: postgres）
- Aspire Dashboard (OpenTelemetry)：http://localhost:18888

## 資料庫與遷移
- 專案包含 Migrations 資料夾；在開發環境 (`ASPNETCORE_ENVIRONMENT` = Development) 啟動時，應用程式會自動呼叫 `ApplyMigrationsAsync()`。
- 若想手動操作，請使用 `dotnet ef` 工具於 `BookKeeper.Api` 專案上執行。

## API 範例端點（摘要）
- Labels
  - GET /api/labels
  - GET /api/labels/{id}
  - GET /api/labels/incomes
  - GET /api/labels/expenditures
  - POST /api/labels  (body: { "name": "薪資", "isIncome": true })
  - PUT /api/label/{id}  (body: { "name": "食物", "isIncome": false })
  - DELETE /api/labels/{id}

- Incomes
  - GET /api/incomes
  - GET /api/incomes/{id}
  - POST /api/incomes  (body: { "incomeName": "兼職", "amount": 1000, "incomeDateOnUtc": "2025-12-01", "labelId": "l_..." })
  - PUT /api/incomes/{id}
  - DELETE /api/incomes/{id}

- Expenditures
  - GET /api/expenditures
  - GET /api/expenditures/{id}
  - POST /api/expenditures  (body: { "paymentName":"晚餐", "amount":200, "paymentDateOnUtc":"2025-12-01", "labelId":"l_..." })
  - PUT /api/expenditures/{id}
  - DELETE /api/expenditures/{id}

## 錯誤處理
- 使用 Problem Details + 自訂例外處理器（`BookKeeper.Api/Middleware`）來一致回傳驗證錯誤與內部錯誤。