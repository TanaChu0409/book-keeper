# BookKeeper - Project Memory

> **版本**: v1.4.1 | **最後更新**: 2026-01-28 | **專案**: BookKeeper Personal Finance API | **狀態**: ✅ Active Development

---

## 📖 相關文檔

| 文檔 | 用途 | 說明 |
|------|------|------|
| [copilot-instructions.md](../.github/copilot-instructions.md) | Copilot 主配置 | GitHub Copilot 工作流程與指引 |
| [QUICK-REFERENCE.md](./QUICK-REFERENCE.md) | 快速參考 | 命名約定、常見任務速查表 |
| [copilot-service-memory.md](./copilot-service-memory.md) | 服務清單 | Features/Endpoints/Handlers 映射 |
| [WORKFLOW_ROUTES.md](./procedures/WORKFLOW_ROUTES.md) | 流程判斷 | 任務複雜度判斷與角色路由 |
| [WORKFLOW_CHECKLIST.md](./procedures/WORKFLOW_CHECKLIST.md) | 檢查清單 | 各階段驗證標準與前置檢查 |
| [WORKFLOW_HANDOFF.md](./procedures/WORKFLOW_HANDOFF.md) | 交接驗證 | 角色間 Handoff 檢查點 |
| [REQUIREMENT_TEMPLATE.md](./procedures/REQUIREMENT_TEMPLATE.md) | 需求模板 | 標準需求提交格式 |
| [FEATURE_TEMPLATE.md](./procedures/FEATURE_TEMPLATE.md) | 功能模板 | 新功能開發標準流程 |

---

## 🎯 專案概述

**BookKeeper** 是一個使用 .NET 8 構建的現代化個人記帳後端 API，採用 **Vertical Slice Architecture** 與 **CQRS** 模式，專注於管理收入 (Incomes)、支出 (Expenditures) 與標籤 (Labels)。專案採用 PostgreSQL 作為資料存儲、MediatR 處理業務邏輯、FluentValidation 進行請求驗證，並完整整合 OpenTelemetry 實現可觀測性。架構設計清晰、高內聚低耦合，適合個人財務管理場景。

### 技術棧核心

| 類別 | 技術 | 版本 | 用途 |
|------|------|------|------|
| **運行時** | .NET | 8.0 | ASP.NET Core Web API |
| **ORM** | Entity Framework Core | 8.0.21 | 資料存取層 |
| **資料庫** | PostgreSQL | 17.2 | 關聯式資料庫 |
| **資料庫驅動** | Npgsql | 8.0.11 | PostgreSQL 提供者 |
| **CQRS** | MediatR | 12.5.0 | Command/Query 分離 |
| **驗證** | FluentValidation | 12.0.0 | 請求驗證 |
| **可觀測性** | OpenTelemetry | 1.13.1 | Traces/Metrics/Logs |
| **ID 生成** | Ulid | 1.4.1 | 分散式 ID (`l_`/`i_`/`e_` 前綴) |
| **API 文檔** | Swashbuckle | 9.0.6 | Swagger/OpenAPI |
| **認證** | ASP.NET Identity | 8.0.21 | JWT 認證（已配置） |

### 架構特點

1. **Vertical Slice Architecture (VSA)** - 按功能垂直切分，每個 Feature 包含 Command/Query、Validator、Handler、Endpoint
2. **CQRS Pattern** - 使用 MediatR 實現 Command/Query 職責分離
3. **Result Pattern** - 函數式錯誤處理，避免異常作為流程控制
4. **Rich Domain Model** - 實體封裝業務邏輯，私有建構函式 + 靜態工廠方法
5. **Minimal API + IEndpoint** - 自動探索註冊端點，減少樣板代碼

### 核心功能領域

| 模組 | 實體 | ID 前綴 | 功能 |
|------|------|---------|------|
| **Labels** | `Label` | `l_` | 收入/支出分類標籤管理 (CRUD + 軟刪除) |
| **Incomes** | `Income` | `i_` | 收入記錄追蹤 (CRUD + 分頁查詢) |
| **Expenditures** | `Expenditure` | `e_` | 支出記錄追蹤 (CRUD + 分頁查詢) |

---

## 🏗️ 架構決策日誌

### 日誌規則

- **日期格式**: YYYY-MM-DD
- **ID**: 遞增編號 (#001, #002...)
- **決策內容**: 30-50 字簡潔描述
- **理由**: 100+ 字充分說明背景與考量
- **影響範圍**: 列出受影響的模組/檔案

### 決策記錄

| 日期 | ID | 決策 | 理由 | 影響範圍 |
|------|----|----|------|---------|
| 2026-01-06 | #001 | 採用 Vertical Slice Architecture | 功能隔離、易於導航、減少跨層耦合、符合現代 .NET 最佳實踐 | 全專案結構 `/Features/` |
| 2026-01-06 | #002 | Result Pattern 取代 Exception | 明確錯誤處理、更好的控制流、強制調用方處理錯誤 | `/Shared/Result.cs` + 所有 Handler |
| 2026-01-06 | #003 | ULID 作為主鍵 | 時間排序、URL 安全、分散式友好、優於 GUID | 所有 Entity + `l_`/`i_`/`e_` 前綴 |
| 2026-01-06 | #004 | 自動端點發現機制 | 減少樣板代碼、基於 `IEndpoint` 約定、Reflection 自動註冊 | `DependencyInjection.cs` |
| 2026-01-06 | #005 | Entity Factory Pattern | 封裝創建邏輯、確保不變性、私有建構函式 | 所有 Entity (`Label`/`Income`/`Expenditure`) |
| 2026-01-06 | #006 | MediatR for CQRS | 命令/查詢分離、解耦處理邏輯、單一職責原則 | 所有 Feature Handlers |
| 2026-01-06 | #007 | FluentValidation 整合 | 聲明式驗證、與 MediatR Pipeline 整合、清晰的驗證規則 | 所有 Command Validators |
| 2026-01-06 | #008 | Snake_case 資料庫命名 | PostgreSQL 慣例、可讀性、自動轉換 | `EFCore.NamingConventions` 套件 |
| 2026-01-08 | #009 | 重建 my-ai-swarm 記憶系統 | 符合 copilot-instructions.md 標準、建立決策追蹤機制、支援 Workflow 協作 | `my-ai-swarm/` 全目錄結構 |
| 2026-01-12 | #010 | 新增 Auth Register/Login/Refresh | 補齊 JWT 認證流程：新增 Register/Login/Refresh 三個端點，註冊時建立 Identity + Domain User 並預設 Member 角色，登入檢驗帳密並回傳 access/refresh tokens。Refresh Token 採單一活躍策略，簽發前清除舊 token，過期或無效即刪除，避免多重 session 安全風險；沿用 Identity 預設密碼策略，使用 TokenProvider + JwtAuthOptions 控管到期時間，讓後續 API 透過 Authorization header 與 UserContext 能正確解析 Domain User。 | `Features/Auth/*`, `Contracts/Auth/*`, `Tags.cs`, `my-ai-swarm/*` |
| 2026-01-20 | #011 | 強制遵守 .editorconfig 規則 | 新增/更新程式碼時必須先讀取並遵守專案 `.editorconfig` 規範，確保代碼風格一致性。關鍵規則：(1) File-scoped namespace；(2) Using directives 置於 namespace 外；(3) 不使用 `this.` 前綴；(4) 除非類型明顯否則不使用 `var`；(5) 必須使用大括號；(6) Accessors/Properties/Operators 使用 expression body，Methods/Constructors 使用 block body；(7) 所有大括號前換行；(8) 4 空格縮排、CRLF、UTF-8 BOM。此規則適用於所有 C# 檔案修改作業。 | 所有 `BookKeeper.Api/**/*.cs` 檔案 |
| 2026-01-28 | #012 | 新增月度統計背景任務 StatisticOfMonth | 建立每月執行的 Quartz 背景任務，於每月 1 日凌晨 3:00 統計上個月所有用戶的收入與支出總額。Entity 使用 `int Year` + `int Month` 欄位（而非 DateOnly）以簡化查詢與聚合；遍歷所有 Users，從 `Incomes`/`Expenditures` 直接聚合（不依賴 StatisticsOfDates），僅記錄有交易的用戶（totalIncome > 0 或 totalExpend > 0），支援 Upsert 邏輯（查詢現有記錄並更新或建立新記錄）。排程使用 Cron 表達式 `0 0 3 1 * ?`，Job 類別為 `ProcessStatisticOfMonth`，註冊於 `DependencyInjection.AddQuartz()`。ID 前綴使用 `som_` 對稱 `sod_`，唯一索引為 `(UserId, Year, Month)`。 | `Entities/StatisticOfMonth.cs`, `Database/Configurations/StatisticOfMonthConfiguration.cs`, `Features/Statistics/CreateStatisticOfMonth.cs`, `ApplicationDbContext.cs`, `DependencyInjection.cs` |
| 2026-01-28 | #013 | 強制所有檔案使用 CRLF 換行符 | 統一專案內所有文本文件（包含 `.cs`, `.json`, `.md`, `.yml` 等）使用 CRLF (Windows) 作為行尾符 (End of Line)。專案已配置 `.editorconfig` 設定 `end_of_line = crlf`，確保 VS Code/Visual Studio/Rider 等編輯器遵守此規範。AI Copilot 在產生或修改任何檔案時，必須使用 CRLF 換行符，避免混用 LF 導致版本控制衝突。此為 Windows 開發環境標準慣例，所有新建/編輯檔案均需遵守。 | 所有專案文本檔案，`.editorconfig` 第 14 行 |

---

## 📐 核心程式碼模式

### 1️⃣ Vertical Slice Feature 結構

**原則**: 一個功能 = 一個檔案，所有相關類別巢狀在內

```csharp
// File: Features/Expenditures/CreateExpenditure.cs

public static class CreateExpenditure
{
    // ✅ 定義 Command/Query（輸入）
    public class Command : IRequest<Result<string>>
    {
        public string PaymentName { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public DateOnly PaymentDateOnUtc { get; init; }
        public string LabelId { get; init; } = string.Empty;
    }
    
    // ✅ 定義驗證器
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PaymentName).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
            // ...
        }
    }
    
    // ✅ 定義業務邏輯處理器
    internal sealed class Handler(ApplicationDbContext context) 
        : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken ct)
        {
            // 業務邏輯...
            Label? label = await context.Labels.FindAsync([request.LabelId], ct);
            if (label is null)
                return Result.Failure<string>(LabelErrors.NotFound);
            
            Expenditure expenditure = Expenditure.Create(
                request.PaymentName, 
                request.Amount, 
                request.PaymentDateOnUtc, 
                label);
            
            context.Expenditures.Add(expenditure);
            await context.SaveChangesAsync(ct);
            
            return expenditure.Id;
        }
    }
}

// ✅ 定義端點（同一檔案）
public class CreateExpenditureEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("api/expenditures", async (
            CreateExpenditureRequest request, 
            ISender sender) =>
        {
            var command = new CreateExpenditure.Command { /* map request */ };
            Result<string> result = await sender.Send(command);
            
            return result.Match(
                onSuccess: (id) => Results.Created($"api/expenditures/{id}", id),
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.Expenditures);
    }
}
```

**關鍵規則**:
- ✅ 所有類別巢狀在 `CreateExpenditure` 靜態類別內
- ✅ Handler 是 `internal sealed class`
- ✅ 端點類別在同一檔案，實現 `IEndpoint`
- ✅ 使用 `Result<T>` 回傳值

---

### 2️⃣ Entity Factory Pattern

**原則**: 私有建構函式 + 靜態 `Create()` 方法

```csharp
public sealed class Expenditure
{
    // ❌ 私有建構函式（僅供 EF Core 使用）
    private Expenditure() { }
    
    // ✅ 公開靜態工廠方法
    public static Expenditure Create(
        string paymentName,
        decimal amount,
        DateOnly paymentDateOnUtc,
        Label label)
    {
        return new Expenditure
        {
            Id = $"e_{Ulid.NewUlid()}",  // 前綴 + ULID
            PaymentName = paymentName,
            Amount = amount,
            PaymentDateOnUtc = paymentDateOnUtc,
            LabelId = label.Id,
            Label = label,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
    
    // ✅ 更新方法（封裝邏輯）
    public void Update(string paymentName, decimal amount, DateOnly date, Label label)
    {
        PaymentName = paymentName;
        Amount = amount;
        PaymentDateOnUtc = date;
        LabelId = label.Id;
        Label = label;
        UpdatedOnUtc = DateTime.UtcNow;
    }
    
    // Properties...
    public string Id { get; private set; } = string.Empty;
    public string PaymentName { get; private set; } = string.Empty;
    // ...
}
```

**關鍵規則**:
- ✅ Id 格式: `{prefix}_{ULID}` (e.g., `e_01J9KT...`)
- ✅ Prefix: `e_` (Expenditure), `i_` (Income), `l_` (Label)
- ✅ `CreatedOnUtc` / `UpdatedOnUtc` 自動設置
- ❌ **禁止**: 直接使用 `new Expenditure()`

---

### 3️⃣ Result Pattern（錯誤處理）

**原則**: 所有 Handler 回傳 `Result<T>`，不拋例外

```csharp
// ✅ 成功情境
return userId;  // 隱式轉換為 Result<string>

// ✅ 失敗情境
return Result.Failure<string>(new Error("Label.NotFound", "Label not found"));

// ✅ 在 Endpoint 中處理
return result.Match(
    onSuccess: (id) => Results.Created($"api/expenditures/{id}", id),
    onFailure: (error) => Results.BadRequest(error));
```

**錯誤定義範例** (Shared/Errors/):
```csharp
public static class LabelErrors
{
    public static readonly Error NotFound = new(
        "Label.NotFound", 
        "The label with specified ID was not found");
    
    public static readonly Error AlreadyExists = new(
        "Label.AlreadyExists", 
        "A label with the same name already exists");
}
```

**關鍵規則**:
- ✅ 使用 `Result<T>` 回傳成功值或錯誤
- ✅ 在 Shared/Errors/ 定義靜態錯誤常數
- ✅ Endpoint 使用 `.Match()` 處理結果
- ❌ **禁止**: 在 Handler 中拋 `throw new Exception()`

---

### 4️⃣ 自動端點註冊

**原則**: 實現 `IEndpoint`，自動被掃描註冊

```csharp
// ✅ 定義介面（Endpoints/IEndpoint.cs）
public interface IEndpoint
{
    void MapEndpoints(IEndpointRouteBuilder app);
}

// ✅ 實現端點
public class GetExpendituresEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/expenditures", async (...) => { })
            .WithTags(Tags.Expenditures);
    }
}

// ✅ 自動註冊（DependencyInjection.cs）
public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        // 掃描所有 IEndpoint 實現
        services.AddEndpoints(Assembly.GetExecutingAssembly());
        return services;
    }
}
```

**關鍵規則**:
- ✅ 每個 Feature 檔案包含一個 Endpoint 類別
- ✅ 使用 `Tags` 類別的常數進行分類
- ❌ **禁止**: 在 `Program.cs` 手動註冊端點

---

## 📋 命名約定速查表

| 元件 | 模式 | 範例 | 位置 |
|------|------|------|------|
| **Feature 檔案** | `{Action}{Domain}.cs` | `CreateExpenditure.cs` | `Features/{Domain}/` |
| **Command/Query** | `Command` / `Query` (巢狀類別) | `CreateExpenditure.Command` | Feature 檔案內 |
| **Handler** | `Handler` (巢狀 sealed class) | `CreateExpenditure.Handler` | Feature 檔案內 |
| **Validator** | `Validator` (巢狀類別) | `CreateExpenditure.Validator` | Feature 檔案內 |
| **Endpoint** | `{Action}{Domain}Endpoint` | `CreateExpenditureEndpoint` | Feature 檔案內 |
| **Entity** | `{Domain}` | `Expenditure`, `Income`, `Label` | `Entities/` |
| **Entity ID** | `{prefix}_{ULID}` | `e_01J9KT...`, `i_01J9KT...` | 生成於 `Create()` |
| **Request Contract** | `{Action}{Domain}Request` | `CreateExpenditureRequest` | `Contracts/{Domain}/` |
| **Response Contract** | `{Domain}Response` | `ExpenditureResponse` | `Contracts/{Domain}/` |
| **EF Configuration** | `{Entity}Configuration` | `ExpenditureConfiguration` | `Database/Configurations/` |
| **Error Class** | `{Domain}Errors` | `LabelErrors` | `Shared/Errors/` |
| **Extension Class** | `{Feature}Extensions` | `DatabaseExtensions` | `Extensions/` |

---

## 🚫 禁止操作與反模式

| ❌ 禁止 | ✅ 正確做法 | 理由 |
|---------|------------|------|
| `throw new Exception()` 在 Handler | 回傳 `Result.Failure<T>(error)` | Result Pattern 原則 |
| 公開 Entity 建構函式 | 私有建構函式 + `Create()` | 封裝創建邏輯 |
| 在 Endpoint 直接查詢 DbContext | 使用 MediatR `sender.Send(command)` | 分離關注點 |
| 手動註冊端點 | 實現 `IEndpoint` 自動掃描 | 減少樣板代碼 |
| 跳過 FluentValidation | 每個 Command 都要有 `Validator` | 確保輸入驗證 |
| 使用 GUID 作為 Id | 使用 `Ulid` + 前綴 | 時間排序、可讀性 |
| 分散 Feature 類別到不同檔案 | 所有類別巢狀在一個檔案 | Vertical Slice 原則 |
| 使用 Layered Architecture | 使用 Vertical Slice | 功能內聚、減少耦合 |
| DateTime.Now (本地時間) | `DateTime.UtcNow` | 時區一致性 |
| 硬編碼資料庫名稱 | 使用 `appsettings.json` | 環境分離 |

---

## ✅ 新增 Feature 完整檢查清單

### **Phase 1: 規劃階段**
- [ ] 確認 Feature 類型（CRUD? 複雜業務邏輯?）
- [ ] 決定 Domain 分類（Expenditure / Income / Label / 新領域?）
- [ ] 確認是否需要新 Entity（或使用既有）
- [ ] 設計 API 端點路徑與 HTTP 方法

### **Phase 2: Entity 與資料庫**（若需新 Entity）
- [ ] 建立 Entity 類別於 `Entities/{Domain}.cs`
  - [ ] 私有建構函式
  - [ ] 靜態 `Create()` 方法
  - [ ] ULID Id 生成（`{prefix}_{Ulid.NewUlid()}`）
  - [ ] `CreatedOnUtc` / `UpdatedOnUtc` 屬性
- [ ] 建立 EF Configuration 於 `Database/Configurations/{Domain}Configuration.cs`
  - [ ] 配置主鍵
  - [ ] 配置必要欄位、長度限制
  - [ ] 配置關聯（如有）
- [ ] 在 `ApplicationDbContext` 新增 `DbSet<{Domain}>`
- [ ] 建立 Migration: `dotnet ef migrations add Add{Domain} -p BookKeeper.Api`
- [ ] 檢查生成的 Migration SQL 是否正確

### **Phase 3: Contracts（API 介面）**
- [ ] 建立 Request 類別於 `Contracts/{Domain}/{Action}{Domain}Request.cs`
  - [ ] 定義所有輸入欄位
  - [ ] 使用 `init` accessor
- [ ] 建立 Response 類別於 `Contracts/{Domain}/{Domain}Response.cs`（若需要）
  - [ ] 定義所有輸出欄位
  - [ ] 使用 `record` 型別（建議）

### **Phase 4: Feature 實現（核心）**
- [ ] 建立 Feature 檔案於 `Features/{Domain}/{Action}{Domain}.cs`
- [ ] 定義 Command/Query 巢狀類別
  - [ ] 實現 `IRequest<Result<T>>`
  - [ ] 定義所有屬性
- [ ] 定義 Validator 巢狀類別
  - [ ] 繼承 `AbstractValidator<Command>`
  - [ ] 在建構函式中定義所有驗證規則
- [ ] 定義 Handler 巢狀類別
  - [ ] `internal sealed class Handler`
  - [ ] 實現 `IRequestHandler<Command, Result<T>>`
  - [ ] 注入 `ApplicationDbContext` 與其他相依性
  - [ ] 實作業務邏輯
  - [ ] 使用 `Result.Failure<T>(error)` 處理錯誤
  - [ ] 回傳成功結果（直接回傳值，隱式轉換）
- [ ] 定義 Endpoint 類別（同檔案）
  - [ ] 實現 `IEndpoint`
  - [ ] 實作 `MapEndpoints()` 方法
  - [ ] 使用適當的 HTTP 方法（Get/Post/Put/Delete）
  - [ ] 使用 `.WithTags(Tags.{Domain})` 分類
  - [ ] 使用 `result.Match()` 處理結果

### **Phase 5: 錯誤處理**
- [ ] 若需要新錯誤類型，建立於 `Shared/Errors/{Domain}Errors.cs`
  - [ ] 定義為 `public static class`
  - [ ] 定義靜態 `Error` 常數
  - [ ] 格式: `{Domain}.{ErrorType}`

### **Phase 6: 測試與驗證**
- [ ] 本地啟動: `dotnet run --project BookKeeper.Api`
- [ ] 測試端點於 Swagger UI (`http://localhost:9000/swagger`)
- [ ] 測試成功場景（200/201 回應）
- [ ] 測試失敗場景（驗證錯誤、業務邏輯錯誤）
- [ ] 檢查資料庫是否正確寫入（使用 pgAdmin 或 DBeaver）
- [ ] 檢查 OpenTelemetry Traces（Aspire Dashboard: `http://localhost:18888`）

### **Phase 7: 文檔與記憶**
- [ ] 更新此 `project-memory.md` 的決策日誌（若有新決策）
- [ ] 更新 Tags.cs（若新增領域）
- [ ] 若模式變更，更新 `copilot-service-memory.md`

---

## 🛠️ 常用指令速查

### **開發環境**
```bash
# 本地啟動（僅 API，需手動連接外部 DB）
dotnet run --project BookKeeper/BookKeeper/BookKeeper.Api

# Docker 完整環境啟動（推薦）
cd BookKeeper/BookKeeper
docker-compose up --build

# 停止 Docker 環境
docker-compose down

# 查看 Docker 日誌
docker-compose logs -f bookkeeper.api
```

### **資料庫操作**
```bash
# 新增 Migration
dotnet ef migrations add {MigrationName} -p BookKeeper/BookKeeper/BookKeeper.Api

# 套用 Migration（Production，Development 自動套用）
dotnet ef database update -p BookKeeper/BookKeeper/BookKeeper.Api

# 回滾 Migration
dotnet ef database update {PreviousMigrationName} -p BookKeeper/BookKeeper/BookKeeper.Api

# 移除最後一個 Migration（未套用前）
dotnet ef migrations remove -p BookKeeper/BookKeeper/BookKeeper.Api

# 查看 Migration 清單
dotnet ef migrations list -p BookKeeper/BookKeeper/BookKeeper.Api
```

### **程式碼品質**
```bash
# 建置（含 Code Analysis）
dotnet build BookKeeper/BookKeeper/BookKeeper.sln

# 清理建置輸出
dotnet clean BookKeeper/BookKeeper/BookKeeper.sln

# 檢查格式
dotnet format BookKeeper/BookKeeper/BookKeeper.sln
```

### **Docker 資料庫連線**
```bash
# 進入 PostgreSQL 容器
docker exec -it bookkeeper.database psql -U postgres -d bookkeeper

# 查詢所有表格
\dt

# 查詢 Expenditures
SELECT * FROM expenditures;
```

---

## 🔧 設定與環境變數

### **連線字串**

| 環境 | 檔案 | 連線字串 |
|------|------|---------|
| Development (Docker) | `appsettings.Development.json` | `Host=bookkeeper.database;Port=5432;Database=bookkeeper;Username=postgres;Password=postgres` |
| Production | User Secrets / Env | `ConnectionStrings__Database` |

### **OpenTelemetry**

| 設定 | 值 |
|------|-----|
| OTLP Endpoint | `http://bookkeeper.aspire-dashboard:18889` |
| Dashboard UI | `http://localhost:18888` |
| Traces | HTTP, AspNetCore, Npgsql |
| Metrics | Runtime, HTTP, AspNetCore |

### **Docker Compose 服務**

| 服務 | Port | 用途 |
|------|------|------|
| `bookkeeper.api` | 9000 (HTTP), 9001 (HTTPS) | API 端點 |
| `bookkeeper.database` | 5432 | PostgreSQL |
| `bookkeeper.aspire-dashboard` | 18888 | .NET Aspire Dashboard |

---

## 📚 關鍵檔案參考

| 檔案 | 用途 | 何時修改 |
|------|------|---------|
| [Program.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Program.cs) | 應用程式啟動、管線配置 | 極少（僅管線變更） |
| [DependencyInjection.cs](../BookKeeper/BookKeeper/BookKeeper.Api/DependencyInjection.cs) | 服務註冊、端點掃描 | 新增基礎設施服務時 |
| [ApplicationDbContext.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Database/ApplicationDbContext.cs) | EF Core 配置、DbSet 定義 | 新增 Entity 時 |
| [Tags.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Tags.cs) | Swagger 標籤定義 | 新增領域時 |
| [Result.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Shared/Result.cs) | Result Pattern 核心實現 | 不修改 |
| [IEndpoint.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Endpoints/IEndpoint.cs) | 端點介面定義 | 不修改 |

---

## 🎯 工作流程適配 (BookKeeper 專屬)

### 複雜度判斷標準

根據 [WORKFLOW_ROUTES.md](./procedures/WORKFLOW_ROUTES.md) 的流程判斷，BookKeeper 專案的標準：

| 任務類型 | 行數估算 | 涉及檔案 | 推薦流程 | 預計工時 |
|---------|---------|---------|---------|---------|
| **新增簡單 CRUD Feature** | 100-300 | 1-2 個 Feature 檔案 | **流程 B** | 3-5 天 |
| **新增複雜業務邏輯 Feature** | 300-500 | 3-5 個 Feature 檔案 + Entity | **流程 A** | 1-2 週 |
| **新增 Entity + Complete CRUD** | 500+ | Entity + Config + 5 Feature 檔案 | **流程 A** | 1-2 週 |
| **Bug 修復（單一 Feature）** | <100 | 1 個檔案 | **流程 C** | 1-2 天 |
| **重構既有 Feature** | 200-400 | 2-3 個檔案 | **流程 B** | 3-5 天 |
| **資料庫 Schema 變更** | 依複雜度 | Migration + Config | **流程 A/B** | 3-7 天 |
| **新增認證端點** | 300-500 | JWT Service + Auth Feature | **流程 A** | 1-2 週 |

### BookKeeper 專屬流程示例

#### ✅ 流程 C 示例：修復驗證錯誤
```markdown
需求：修復 CreateExpenditure 的金額驗證（允許 0.01）
分類：Bug 修復
影響：Features/Expenditures/CreateExpenditure.cs (1 個檔案)
流程：Developer → QA → Memory
工時：4-8 小時
```

#### ✅ 流程 B 示例：新增查詢端點
```markdown
需求：為 Expenditure 新增「按月份統計」查詢端點
分類：新功能（中等）
影響：Features/Expenditures/GetExpendituresByMonth.cs (1 個新檔案)
流程：Architect → Developer → QA → Memory
工時：3-5 天
```

#### ✅ 流程 A 示例：新增 Category Entity
```markdown
需求：新增 Category 實體與完整 CRUD 端點
分類：新功能（複雜）
影響：
  - Entities/Category.cs
  - Database/Configurations/CategoryConfiguration.cs
  - ApplicationDbContext.cs
  - Features/Categories/ (5 個 Feature 檔案)
  - Contracts/Categories/ (Request/Response)
流程：Architect → Impact Validator → Developer → QA → Memory
工時：1-2 週
```

---

## 🔗 相關資源

### 官方文檔
- [.NET 8 文檔](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR](https://github.com/jbogard/MediatR/wiki)
- [FluentValidation](https://docs.fluentvalidation.net/en/latest/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)

### 架構參考
- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
- [Result Pattern in C#](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)

### 社群資源
- [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)

---

## 📝 版本歷史

| 版本 | 日期 | 變更摘要 | 負責人 |
|------|------|---------|--------|
| v1.0.0 | 2026-01-06 | 初始版本建立，記錄核心架構決策 | AI Architect |
| v1.1.0 | 2026-01-08 | 重建記憶系統，符合 copilot-instructions.md 標準，新增完整命名約定與檢查清單 | GitHub Copilot |

---

**最後更新**: 2026-01-08  
**維護者**: GitHub Copilot + AI Swarm Agents  
**專案狀態**: ✅ Active Development
| `IEndpoint.cs` | 端點約定介面 | 幾乎不修改 |
| `Result.cs` | 錯誤處理模式 | 幾乎不修改 |

---

## 🎓 學習路徑（新開發者）

### **階段 1: 理解架構**
1. 閱讀 `GetExpenditures.cs` - 最簡單的查詢模式
2. 研究 `CreateExpenditure.cs` - 完整的驗證與錯誤處理
3. 檢視 `UpdateExpenditure.cs` - 更新模式
4. 查看 `Result.cs` - 理解錯誤處理模式

### **階段 2: 實作練習**
1. 新增 `GetExpendituresByLabelId` - 練習查詢
2. 新增 `SoftDeleteExpenditure` - 練習軟刪除
3. 新增 `GetMonthlyExpenditureReport` - 練習複雜查詢

### **階段 3: 進階主題**
1. 研究 OpenTelemetry 整合
2. 實作 Unit Test（目前尚未建立）
3. 新增 Authentication/Authorization（未來）

---

## 🔄 版本歷史

| 版本 | 日期 | 變更 |
|------|------|------|
| v1.0.0 | 2026-01-06 | 初始版本：Vertical Slice + CQRS + Result Pattern |

---

**最後更新**: 2026-01-06 by AI Infrastructure Team
