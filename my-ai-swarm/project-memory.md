# BookKeeper - Project Memory

> **版本**: v1.0.0 | **最後更新**: 2026-01-06 | **專案**: BookKeeper Personal Finance API | **狀態**: ✅ Active Development

---

## 📖 相關文檔

- **Copilot 入門指南**: [.github/copilot-instructions.md](../.github/copilot-instructions.md)
- **快速參考**: [QUICK-REFERENCE.md](./QUICK-REFERENCE.md)
- **服務記憶**: [copilot-service-memory.md](./copilot-service-memory.md)
- **工作流程**: [procedures/](./procedures/)

---

## 🎯 專案概述

**BookKeeper** 是一個個人財務追蹤 API，採用現代 .NET 8 架構模式：

- **架構**: Vertical Slice Architecture (VSA)
- **API 風格**: ASP.NET Core Minimal API
- **資料庫**: PostgreSQL 17.2 + Entity Framework Core 8
- **模式**: CQRS (MediatR) + Result Pattern
- **容器化**: Docker Compose (API + DB + Aspire Dashboard)
- **可觀測性**: OpenTelemetry (Traces/Metrics/Logs)

### 核心功能領域

1. **Labels（標籤）**: 收入/支出分類管理
2. **Expenditures（支出）**: 支出記錄追蹤
3. **Incomes（收入）**: 收入記錄追蹤

---

## 🏗️ 架構決策日誌

| 日期 | ID | 決策 | 理由 | 影響範圍 |
|------|----|----|------|---------|
| 2026-01-06 | #001 | 採用 Vertical Slice Architecture | 功能隔離、易於導航、減少跨層耦合 | 全專案結構 |
| 2026-01-06 | #002 | Result Pattern 取代 Exception | 明確錯誤處理、更好的控制流 | 所有 Handler |
| 2026-01-06 | #003 | ULID 作為主鍵 | 時間排序、URL 安全、優於 GUID | 所有 Entity |
| 2026-01-06 | #004 | 自動端點發現 | 減少樣板代碼、基於約定 | DependencyInjection |
| 2026-01-06 | #005 | Entity Factory Pattern | 封裝創建邏輯、確保不變性 | 所有 Entity |
| 2026-01-06 | #006 | MediatR for CQRS | 命令/查詢分離、解耦處理邏輯 | 所有 Feature |
| 2026-01-06 | #007 | FluentValidation | 聲明式驗證、與 MediatR 整合 | 所有 Command |
| 2026-01-06 | #008 | Snake_case 資料庫命名 | PostgreSQL 慣例、可讀性 | EF Core 配置 |

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
| `Program.cs` | 應用程式啟動、管線配置 | 極少（僅管線變更） |
| `DependencyInjection.cs` | 服務註冊、端點掃描 | 新增基礎設施服務時 |
| `ApplicationDbContext.cs` | EF Core 配置、DbSet 定義 | 新增 Entity 時 |
| `Tags.cs` | Swagger 標籤定義 | 新增領域時 |
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
