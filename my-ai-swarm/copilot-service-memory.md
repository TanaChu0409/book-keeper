# BookKeeper Service Memory

> **用途**: 詳細記錄 BookKeeper API 的技術細節、程式碼模式、集成方式
>
> **維護者**: AI Infrastructure Team
>
> **最後更新**: 2026-01-06

---

## 🎯 專案概覽

### 技術棧

| 類別 | 技術 | 版本 |
|------|------|------|
| **Runtime** | .NET | 8.0 |
| **API Framework** | ASP.NET Core Minimal API | 8.0 |
| **資料庫** | PostgreSQL | 17.2 |
| **ORM** | Entity Framework Core | 8.0.21 |
| **CQRS** | MediatR | 12.5.0 |
| **驗證** | FluentValidation | 12.0.0 |
| **可觀測性** | OpenTelemetry | 1.13.1 |
| **ID 生成** | Ulid | 1.4.1 |
| **Code Analysis** | SonarAnalyzer.CSharp | 10.15.0 |

---

## 📐 架構模式

### Vertical Slice Architecture

**核心原則**：
- 一個功能 = 一個檔案
- 所有相關類別巢狀在同一個靜態類別內
- 無傳統分層（Controller/Service/Repository）
- 功能內聚、減少跨層耦合

**檔案結構**：
```
Features/
├─ Expenditures/
│  ├─ CreateExpenditure.cs      # Command + Validator + Handler + Endpoint
│  ├─ GetExpenditure.cs          # Query + Handler + Endpoint
│  ├─ GetExpenditures.cs         # Query + Handler + Endpoint (分頁)
│  ├─ UpdateExpenditure.cs       # Command + Validator + Handler + Endpoint
│  └─ DeleteExpenditure.cs       # Command + Handler + Endpoint
├─ Incomes/
│  └─ ...（同 Expenditures 結構）
└─ Labels/
   └─ ...（7 個功能）
```

---

## 🏗️ 程式碼模式詳解

### 1️⃣ CQRS with MediatR

**Command（寫入操作）**：
```csharp
public class Command : IRequest<Result<string>>
{
    public string PropertyName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}
```

**Query（讀取操作）**：
```csharp
public class Query : IRequest<Result<ExpenditureResponse>>
{
    public string Id { get; init; } = string.Empty;
}
```

**Handler**：
```csharp
internal sealed class Handler(ApplicationDbContext context) 
    : IRequestHandler<Command, Result<string>>
{
    public async Task<Result<string>> Handle(Command request, CancellationToken ct)
    {
        // 業務邏輯...
        return entityId;  // 隱式轉換為 Result<string>
    }
}
```

**關鍵特性**：
- Command/Query 實現 `IRequest<Result<T>>`
- Handler 實現 `IRequestHandler<TRequest, TResponse>`
- Handler 是 `internal sealed class`（封裝）
- 使用 Primary Constructor 注入相依性

---

### 2️⃣ Result Pattern

**Result 類別**（Shared/Result.cs）：
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }
    
    // 隱式轉換
    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);
    
    // Match Pattern
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Error!);
    }
}
```

**Error 類別**（Shared/Error.cs）：
```csharp
public sealed record Error(string Code, string Message);
```

**使用範例**：
```csharp
// Handler 中
if (entity is null)
    return Result.Failure<string>(LabelErrors.NotFound);

return entity.Id;  // 成功，隱式轉換

// Endpoint 中
return result.Match(
    onSuccess: (id) => Results.Created($"api/expenditures/{id}", id),
    onFailure: (error) => Results.BadRequest(error));
```

**關鍵特性**：
- 明確的錯誤處理，不拋例外
- 使用 Match Pattern 處理成功/失敗分支
- 隱式轉換簡化語法

---

### 3️⃣ Entity Factory Pattern

**Entity 類別**（Entities/Expenditure.cs）：
```csharp
public sealed class Expenditure
{
    // 私有建構函式（僅 EF Core 使用）
    private Expenditure() { }
    
    // 公開工廠方法
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
    
    // 封裝更新邏輯
    public void Update(string paymentName, decimal amount, DateOnly date, Label label)
    {
        PaymentName = paymentName;
        Amount = amount;
        PaymentDateOnUtc = date;
        LabelId = label.Id;
        Label = label;
        UpdatedOnUtc = DateTime.UtcNow;
    }
    
    // 屬性（private set）
    public string Id { get; private set; } = string.Empty;
    public string PaymentName { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    // ...
}
```

**關鍵特性**：
- 私有建構函式防止直接 `new`
- 靜態 `Create()` 方法封裝創建邏輯
- `Update()` 方法封裝修改邏輯
- ULID + 前綴作為 Id（時間排序、URL 安全）
- 屬性使用 `private set` 確保不變性

---

### 4️⃣ Auto-Endpoint Discovery

**IEndpoint 介面**（Endpoints/IEndpoint.cs）：
```csharp
public interface IEndpoint
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
```

**實現 Endpoint**：
```csharp
public class CreateExpenditureEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("api/expenditures", async (
            CreateExpenditureRequest request,
            ISender sender) =>
        {
            var command = new CreateExpenditure.Command
            {
                PaymentName = request.PaymentName,
                Amount = request.Amount,
                // ...
            };
            
            Result<string> result = await sender.Send(command);
            
            return result.Match(
                onSuccess: (id) => Results.Created($"api/expenditures/{id}", id),
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.Expenditures)
        .WithName("CreateExpenditure")
        .WithSummary("Create a new expenditure");
    }
}
```

**自動註冊**（DependencyInjection.cs）：
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpoints(Assembly.GetExecutingAssembly());
        return services;
    }
    
    private static IServiceCollection AddEndpoints(
        this IServiceCollection services, 
        Assembly assembly)
    {
        // 掃描所有實現 IEndpoint 的類別
        // ...
    }
}
```

**關鍵特性**：
- 無需在 Program.cs 手動註冊
- 實現 IEndpoint 自動被掃描
- 減少樣板代碼

---

## 📊 資料庫模式

### Entity Framework Core Configuration

**DbContext**（Database/ApplicationDbContext.cs）：
```csharp
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : DbContext(options)
{
    public DbSet<Expenditure> Expenditures => Set<Expenditure>();
    public DbSet<Income> Incomes => Set<Income>();
    public DbSet<Label> Labels => Set<Label>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 套用所有 IEntityTypeConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

**EF Configuration**（Database/Configurations/ExpenditureConfiguration.cs）：
```csharp
internal sealed class ExpenditureConfiguration : IEntityTypeConfiguration<Expenditure>
{
    public void Configure(EntityTypeBuilder<Expenditure> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.PaymentName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(x => x.Amount)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.HasOne(x => x.Label)
            .WithMany()
            .HasForeignKey(x => x.LabelId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(x => x.PaymentDateOnUtc);
    }
}
```

**Migration 流程**：
```bash
# 新增 Migration
dotnet ef migrations add {Name} -p BookKeeper.Api

# 檢查 Migration SQL
dotnet ef migrations script -p BookKeeper.Api

# 套用 Migration
dotnet ef database update -p BookKeeper.Api
```

**關鍵特性**：
- Snake_case 命名（via EFCore.NamingConventions）
- 明確的 Configuration 類別
- 自動套用 Configuration（Assembly 掃描）
- Development 環境自動執行 Migration

---

## 🔍 驗證模式

### FluentValidation

**Validator 定義**：
```csharp
public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.PaymentName)
            .NotEmpty()
            .WithMessage("Payment name is required")
            .MaximumLength(200)
            .WithMessage("Payment name must not exceed 200 characters");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");
        
        RuleFor(x => x.PaymentDateOnUtc)
            .NotEmpty()
            .WithMessage("Payment date is required")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Payment date cannot be in the future");
    }
}
```

**自動驗證**：
- MediatR Pipeline Behavior 自動執行驗證
- 驗證失敗自動回傳 400 Bad Request
- ValidationExceptionHandler 處理驗證例外

---

## 🌐 API 端點規範

### 端點模式

| 操作 | HTTP Method | 路徑 | 回應 |
|------|------------|------|------|
| Create | POST | `/api/{resource}` | 201 Created + Id |
| Get One | GET | `/api/{resource}/{id}` | 200 OK + Data |
| Get List | GET | `/api/{resource}` | 200 OK + List |
| Update | PUT | `/api/{resource}/{id}` | 204 No Content |
| Delete | DELETE | `/api/{resource}/{id}` | 204 No Content |

### 分頁查詢

**Query Parameters**：
```
GET /api/expenditures?page=1&pageSize=10
```

**Response**：
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 10,
  "totalCount": 100,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

## 🔧 中介軟體

### Global Exception Handler

**GlobalExceptionHandler.cs**：
```csharp
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) 
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
        
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server error"
        };
        
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        
        return true;
    }
}
```

**ValidationExceptionHandler.cs**：
- 處理 FluentValidation.ValidationException
- 回傳 400 Bad Request + 驗證錯誤詳細資訊

---

## 📈 可觀測性

### OpenTelemetry 整合

**追蹤（Traces）**：
- HTTP 請求
- ASP.NET Core 管線
- EF Core 資料庫查詢（via Npgsql.OpenTelemetry）

**指標（Metrics）**：
- Runtime 指標
- HTTP 客戶端指標
- ASP.NET Core 指標

**日誌（Logs）**：
- 整合到 OpenTelemetry Logging

**Dashboard**：
- .NET Aspire Dashboard (http://localhost:18888)
- OTLP Endpoint: http://bookkeeper.aspire-dashboard:18889

---

## 🎯 ID 生成策略

### ULID (Universally Unique Lexicographically Sortable Identifier)

**優勢**：
- 時間排序（包含時間戳）
- URL 安全（無特殊字元）
- 比 GUID 更短、更可讀
- 支援分散式系統

**格式**：
```
{prefix}_{ULID}

範例：
e_01J9KTQG8YXHZ6PQR7S8T9V0WX  (Expenditure)
i_01J9KTQG8YXHZ6PQR7S8T9V0WX  (Income)
l_01J9KTQG8YXHZ6PQR7S8T9V0WX  (Label)
```

**前綴對照表**：
| Entity | Prefix | 範例 |
|--------|--------|------|
| Expenditure | `e_` | `e_01J9KT...` |
| Income | `i_` | `i_01J9KT...` |
| Label | `l_` | `l_01J9KT...` |

---

## 🚀 啟動流程

### Program.cs 流程

```
1. WebApplication.CreateBuilder()
2. AddPresentation()           # Endpoints, MediatR, FluentValidation
3. AddInfrastructure(config)   # EF Core, Database, Extensions
4. Build()
5. UseExceptionHandler()       # Global + Validation handlers
6. MapEndpoints()              # 掃描並註冊所有 IEndpoint
7. ApplyMigrations()           # 自動套用 Migration (Dev only)
8. Run()
```

---

## 📋 編碼標準

### C# 編碼規範

- **Nullable Reference Types**: 啟用
- **TreatWarningsAsErrors**: 啟用
- **EnforceCodeStyleInBuild**: 啟用
- **Code Analyzer**: SonarAnalyzer.CSharp

### 命名約定

| 元素 | 約定 | 範例 |
|------|------|------|
| 類別 | PascalCase | `Expenditure`, `CreateExpenditure` |
| 方法 | PascalCase | `Create()`, `Update()` |
| 屬性 | PascalCase | `PaymentName`, `Amount` |
| 參數 | camelCase | `paymentName`, `amount` |
| 私有欄位 | camelCase 或 _camelCase | `context`, `_logger` |
| 常數 | PascalCase | `MaxLength`, `DefaultPageSize` |

---

## 🔄 版本歷史

| 版本 | 日期 | 變更 |
|------|------|------|
| v1.0.0 | 2026-01-06 | 初始版本 |

---

**最後更新**: 2026-01-06 by AI Infrastructure Team
