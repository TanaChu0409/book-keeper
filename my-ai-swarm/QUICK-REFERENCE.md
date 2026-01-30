# BookKeeper - Quick Reference (快速參考指南)

> **版本**: v1.2.0 | **最後更新**: 2026-01-30 | **用途**: AI Agent 與開發者的快速查閱指南

---

## 🚦 新需求快速決策樹

```
收到新需求
    ↓
【第 1 步】需求分類
    ├─ 新增 CRUD 功能？ → 使用 FEATURE_TEMPLATE.md（流程 B）
    ├─ 新增 Entity + CRUD？ → 使用 FEATURE_TEMPLATE.md（流程 A）
    ├─ 修改既有功能？ → 評估影響範圍（流程 B/C）
    ├─ Bug 修復？ → 直接修復（流程 C）
    └─ 架構變更？ → 啟動 Architect Review（流程 A）

【第 2 步】複雜度評估
    ├─ 新增 Entity？ → 需要 Migration + Config（預估 1-2 週，流程 A）
    ├─ 修改 Entity？ → 需要 Migration（預估 3-5 天，流程 B）
    ├─ 僅業務邏輯？ → 快速實裝（預估 1-2 天，流程 C）
    ├─ 新增背景任務？ → 需要 Quartz Job + DI 註冊（預估 3-5 天，流程 B）
    └─ 跨 Domain？ → 需要評估相依性（預估 1-2 週，流程 A）

【第 3 步】工作流程選擇
    ├─ 複雜度高（>500 行或新 Entity）？ → 流程 A（Architect → Impact → Developer → QA → Memory）
    ├─ 複雜度中（200-500 行）？ → 流程 B（Architect → Developer → QA → Memory）
    └─ 複雜度低（<100 行或 Bug）？ → 流程 C（Developer → QA → Memory）
```

---

## ⚡ 常用指令速查

### **專案啟動**

```bash
# Docker 完整環境（推薦）
cd BookKeeper/BookKeeper
docker-compose up --build

# 僅 API（需外部 DB）
dotnet run --project BookKeeper/BookKeeper/BookKeeper.Api

# 查看服務
# - Swagger: http://localhost:9000/swagger
# - Aspire Dashboard: http://localhost:18888
# - PostgreSQL: localhost:5432
```


### **資料庫操作**

```bash
# 新增 Migration
dotnet ef migrations add {Name} -p BookKeeper/BookKeeper/BookKeeper.Api

# 套用 Migration（Development 自動套用）
dotnet ef database update -p BookKeeper/BookKeeper/BookKeeper.Api

# 查看 Migration 清單
dotnet ef migrations list -p BookKeeper/BookKeeper/BookKeeper.Api

# 移除最後一個 Migration（未套用前）
dotnet ef migrations remove -p BookKeeper/BookKeeper/BookKeeper.Api

# 連接 PostgreSQL 容器
docker exec -it bookkeeper.database psql -U postgres -d bookkeeper
```

---

## 📐 命名約定速查表

| 元件 | 模式 | 範例 | 位置 |
|------|------|------|------|
| **Feature 檔案** | `{Action}{Domain}.cs` | `CreateExpenditure.cs` | `Features/{Domain}/` |
| **Command/Query** | `Command` / `Query` (巢狀) | `CreateExpenditure.Command` | Feature 檔案內 |
| **Handler** | `Handler` (巢狀 sealed) | `CreateExpenditure.Handler` | Feature 檔案內 |
| **Validator** | `Validator` (巢狀) | `CreateExpenditure.Validator` | Feature 檔案內 |
| **Endpoint** | `{Action}{Domain}Endpoint` | `CreateExpenditureEndpoint` | Feature 檔案內 |
| **Entity** | `{Domain}` | `Expenditure`, `Label`, `StatisticOfDate` | `Entities/` |
| **Entity ID** | `{prefix}_{ULID}` | `e_01J9KT...`, `l_01J9KT...`, `sod_01J9KT...`, `sow_01J9KT...`, `som_01J9KT...`, `soy_01J9KT...` | 生成於 `Create()` |
| **Request** | `{Action}{Domain}Request` | `CreateExpenditureRequest` | `Contracts/{Domain}/` |
| **Response** | `{Domain}Response` | `ExpenditureResponse` | `Contracts/{Domain}/` |
| **Error** | `{Domain}Errors` | `LabelErrors` | `Shared/Errors/` |

---

## 📋 新增 Feature 快速檢查清單

### **Phase 1: 規劃（Architect）**
- [ ] 確認需求清晰？
- [ ] 決定 Domain 分類（Expenditure/Income/Label/新？）
- [ ] 是否需要新 Entity？
- [ ] 設計 API 路徑與 HTTP 方法

### **Phase 2: Entity & 資料庫（若需要）**
- [ ] 建立 `Entities/{Domain}.cs`（私有建構函式 + `Create()`）
- [ ] 建立 `Database/Configurations/{Domain}Configuration.cs`
- [ ] 更新 `ApplicationDbContext.cs` 新增 `DbSet<{Domain}>`
- [ ] 執行 `dotnet ef migrations add Add{Domain}`
- [ ] 檢查生成的 Migration SQL

### **Phase 3: Feature 實作**
- [ ] 建立 `Features/{Domain}/{Action}{Domain}.cs`
- [ ] 定義 Command/Query 巢狀類別（實現 `IRequest<Result<T>>`）
- [ ] 定義 Validator 巢狀類別（繼承 `AbstractValidator<Command>`）
- [ ] 定義 Handler 巢狀類別（`internal sealed class Handler`）
- [ ] 定義 Endpoint 類別（實現 `IEndpoint`）
- [ ] 建立 Request/Response Contracts（若需要）

### **Phase 4: 測試**
- [ ] 本地啟動成功？（`dotnet run`）
- [ ] Swagger 顯示端點？（http://localhost:9000/swagger）
- [ ] 測試成功場景（200/201）
- [ ] 測試失敗場景（驗證錯誤）
- [ ] 檢查資料庫寫入正確？
- [ ] OpenTelemetry Traces 正常？（http://localhost:18888）

### **Phase 5: 記憶更新**
- [ ] 更新 `project-memory.md` 決策日誌（若有新決策）
- [ ] 更新 `Tags.cs`（若新增 Domain）
- [ ] 更新 `copilot-service-memory.md`（新增 Feature 映射）

---

## 🎯 常見任務快速模板

### **1. 新增簡單查詢端點**

**需求**: 為 Expenditure 新增「按月份統計」查詢

**步驟**:
```bash
# 1. 建立 Feature 檔案
touch BookKeeper.Api/Features/Expenditures/GetExpendituresByMonth.cs
```

**模板**:
```csharp
public static class GetExpendituresByMonth
{
    public record Query(int Year, int Month) : IRequest<Result<MonthlyStatisticsResponse>>;
    
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
            RuleFor(x => x.Month).InclusiveBetween(1, 12);
        }
    }
    
    internal sealed class Handler(ApplicationDbContext context) 
        : IRequestHandler<Query, Result<MonthlyStatisticsResponse>>
    {
        public async Task<Result<MonthlyStatisticsResponse>> Handle(Query request, CancellationToken ct)
        {
            var stats = await context.Expenditures
                .Where(e => e.PaymentDateOnUtc.Year == request.Year && 
                            e.PaymentDateOnUtc.Month == request.Month)
                .GroupBy(e => 1)
                .Select(g => new MonthlyStatisticsResponse(
                    g.Sum(e => e.Amount),
                    g.Count()))
                .FirstOrDefaultAsync(ct);
            
            return stats ?? new MonthlyStatisticsResponse(0, 0);
        }
    }
}

public class GetExpendituresByMonthEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/expenditures/statistics/{year}/{month}", async (
            int year, 
            int month, 
            ISender sender) =>
        {
            var query = new GetExpendituresByMonth.Query(year, month);
            var result = await sender.Send(query);
            
            return result.Match(
                onSuccess: Results.Ok,
                onFailure: error => Results.BadRequest(error));
        })
        .WithTags(Tags.Expenditures);
    }
}
```

**工時**: 3-5 天（流程 B）

---

### **2. 修復驗證錯誤**

**需求**: CreateExpenditure 允許金額為 0.01

**步驟**:
```csharp
// Features/Expenditures/CreateExpenditure.cs
public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        // 修改前: RuleFor(x => x.Amount).GreaterThan(0);
        // 修改後:
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0.01m)
            .WithMessage("Amount must be at least 0.01");
    }
}
```

**工時**: 1-2 天（流程 C）

---

### **3. 新增 Category Entity + CRUD**

**需求**: 新增 Category 實體與完整 CRUD 端點

**影響範圍**:
- `Entities/Category.cs`
- `Database/Configurations/CategoryConfiguration.cs`
- `ApplicationDbContext.cs`
- `Features/Categories/` (5 個 Feature 檔案)
- `Contracts/Categories/` (Request/Response)
- Migration

**預估**: 1-2 週（流程 A）

**步驟**: 參考 [FEATURE_TEMPLATE.md](./procedures/FEATURE_TEMPLATE.md)

---

## 🚫 常見錯誤與防止

| ❌ 錯誤 | ✅ 正確 | 後果 |
|--------|--------|------|
| `throw new Exception()` | 回傳 `Result.Failure<T>(error)` | 違反 Result Pattern |
| 公開 Entity 建構函式 | 私有 + `Create()` | 無法控制創建邏輯 |
| 直接查詢 DbContext in Endpoint | 使用 MediatR `sender.Send()` | 違反關注點分離 |
| 手動註冊端點 | 實現 `IEndpoint` | 破壞自動探索 |
| 跳過 FluentValidation | 每個 Command 都要 `Validator` | 缺少輸入驗證 |
| 使用 `DateTime.Now` | 使用 `DateTime.UtcNow` | 時區問題 |
| 分散 Feature 類別到多檔案 | 所有類別在一個檔案 | 違反 VSA 原則 |

---

## 🔧 故障排除速查

### **問題: Migration 無法執行**
```bash
# 解法 1: 刪除 Migration 並重新生成
dotnet ef migrations remove -p BookKeeper.Api
dotnet ef migrations add {Name} -p BookKeeper.Api

# 解法 2: 手動修改 Migration 檔案
# 編輯 Migrations/Application/{Timestamp}_{Name}.cs
```

### **問題: Endpoint 未顯示在 Swagger**
```
檢查清單:
□ 是否實現 IEndpoint？
□ 是否在 Program.cs 調用 MapEndpoints()？
□ 是否使用 .WithTags() 分類？
□ 環境是否為 Development？（Swagger 僅 Dev 啟用）
```

### **問題: Handler 找不到依賴**
```
檢查清單:
□ ApplicationDbContext 是否已註冊？
□ 建構函式參數是否正確？
□ Handler 是否為 internal sealed class？
□ 是否使用 Primary Constructor？
```

---

## 📊 複雜度評估指標

| 指標 | 流程 C | 流程 B | 流程 A |
|------|--------|--------|--------|
| **行數** | <100 | 100-500 | >500 |
| **涉及檔案** | 1 | 2-3 | 4+ |
| **新增 Entity** | 無 | 無 | 有 |
| **Migration** | 無 | 可能 | 必定 |
| **跨 Domain** | 無 | 無 | 可能 |
| **工時** | 1-2 天 | 3-5 天 | 1-2 週 |
| **角色** | Dev→QA→Mem | Arc→Dev→QA→Mem | Arc→Impact→Dev→QA→Mem |

---

## 🔗 相關文檔快速連結

| 文檔 | 用途 | 何時查閱 |
|------|------|---------|
| [project-memory.md](./project-memory.md) | 決策日誌與架構約束 | 開始開發前 |
| [copilot-service-memory.md](./copilot-service-memory.md) | Feature/Endpoint 映射 | 查找既有 API |
| [WORKFLOW_ROUTES.md](./procedures/WORKFLOW_ROUTES.md) | 流程判斷樹 | 評估任務複雜度 |
| [FEATURE_TEMPLATE.md](./procedures/FEATURE_TEMPLATE.md) | 新功能模板 | 新增 Feature 時 |
| [README.md](../README.md) | 專案概覽 | 初次接觸專案 |

---

## 📝 版本歷史

| 版本 | 日期 | 變更摘要 |
|------|------|---------|
| v1.0.0 | 2026-01-06 | 初始版本，記錄常用指令與決策樹 |
| v1.1.0 | 2026-01-08 | 完整重建，新增命名約定速查表、常見任務模板、複雜度指標 |

---

**最後更新**: 2026-01-08  
**維護者**: GitHub Copilot

```csharp
// File: Features/Expenditures/GetExpendituresByLabel.cs

public static class GetExpendituresByLabel
{
    public class Query : IRequest<Result<List<ExpenditureResponse>>>
    {
        public string LabelId { get; init; } = string.Empty;
    }
    
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.LabelId).NotEmpty();
        }
    }
    
    internal sealed class Handler(ApplicationDbContext context) 
        : IRequestHandler<Query, Result<List<ExpenditureResponse>>>
    {
        public async Task<Result<List<ExpenditureResponse>>> Handle(
            Query request, CancellationToken ct)
        {
            var expenditures = await context.Expenditures
                .Where(e => e.LabelId == request.LabelId)
                .Select(e => new ExpenditureResponse
                {
                    Id = e.Id,
                    PaymentName = e.PaymentName,
                    Amount = e.Amount,
                    // ...
                })
                .ToListAsync(ct);
                
            return expenditures;
        }
    }
}

public class GetExpendituresByLabelEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/expenditures/by-label/{labelId}", 
            async (string labelId, ISender sender) =>
        {
            var query = new GetExpendituresByLabel.Query { LabelId = labelId };
            var result = await sender.Send(query);
            
            return result.Match(
                onSuccess: Results.Ok,
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.Expenditures);
    }
}
```

---

### **2. 新增 Entity 與 Configuration**

```csharp
// File: Entities/Category.cs

public sealed class Category
{
    private Category() { }  // EF Core only
    
    public static Category Create(string name, string description)
    {
        return new Category
        {
            Id = $"c_{Ulid.NewUlid()}",
            Name = name,
            Description = description,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
    
    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
        UpdatedOnUtc = DateTime.UtcNow;
    }
    
    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }
}

// File: Database/Configurations/CategoryConfiguration.cs

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Description)
            .HasMaxLength(500);
            
        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();
    }
}

// File: Database/ApplicationDbContext.cs

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : DbContext(options)
{
    // ... existing DbSets
    
    public DbSet<Category> Categories => Set<Category>();  // 新增這行
}
```

---

### **3. 新增錯誤定義**

```csharp
// File: Shared/Errors/CategoryErrors.cs

public static class CategoryErrors
{
    public static readonly Error NotFound = new(
        "Category.NotFound",
        "The category with the specified ID was not found");
        
    public static readonly Error AlreadyExists = new(
        "Category.AlreadyExists",
        "A category with the same name already exists");
        
    public static readonly Error InUse = new(
        "Category.InUse",
        "Cannot delete category because it is in use by existing records");
}
```

---

## 🔍 除錯技巧

### **檢查端點是否註冊**
```bash
# 查看所有端點
curl http://localhost:9000/swagger/v1/swagger.json | jq '.paths | keys'
```

### **檢查 Migration 狀態**
```bash
# 查看套用狀態
dotnet ef migrations list -p BookKeeper.Api

# 查看 Migration SQL
dotnet ef migrations script -p BookKeeper.Api
```

### **檢查資料庫**
```sql
-- 連接 PostgreSQL
docker exec -it bookkeeper.database psql -U postgres -d bookkeeper

-- 查看所有表格
\dt

-- 查看表格結構
\d+ expenditures

-- 查詢資料
SELECT * FROM expenditures ORDER BY created_on_utc DESC LIMIT 10;
```

### **檢查 OpenTelemetry Traces**
```
1. 開啟 http://localhost:18888
2. 進入 "Traces" 頁籤
3. 搜尋端點名稱或 HTTP 方法
4. 查看 Trace 詳細資訊
```

---

## 📊 狀態碼對照表

| 狀態碼 | 情境 | 使用方式 |
|--------|------|---------|
| 200 OK | 查詢成功 | `Results.Ok(data)` |
| 201 Created | 創建成功 | `Results.Created($"api/{resource}/{id}", id)` |
| 204 No Content | 刪除/更新成功（無回傳） | `Results.NoContent()` |
| 400 Bad Request | 驗證失敗、業務邏輯錯誤 | `Results.BadRequest(error)` |
| 404 Not Found | 資源不存在 | `Results.NotFound()` 或 `BadRequest(Error)` |
| 500 Internal Server Error | 未預期的例外 | GlobalExceptionHandler 自動處理 |

---

## 🎓 學習路徑

### **第 1 週：理解架構**
```
Day 1: 閱讀 project-memory.md § 核心程式碼模式
Day 2: 研究 GetExpenditures.cs（查詢模式）
Day 3: 研究 CreateExpenditure.cs（創建模式）
Day 4: 研究 UpdateExpenditure.cs（更新模式）
Day 5: 研究 Result.cs（錯誤處理）
```

### **第 2 週：實作練習**
```
Task 1: 新增 GetExpendituresByLabelId（練習查詢）
Task 2: 新增 GetMonthlyExpenditureReport（練習複雜查詢）
Task 3: 新增 SoftDeleteExpenditure（練習軟刪除）
Task 4: 新增完整的 Category CRUD（綜合練習）
```

---

## � Statistics Entity ID 前綴速查

| Entity | ID 前綴 | 唯一索引 | 排程時間 | 零值策略 |
|--------|---------|----------|----------|----------|
| `StatisticOfDate` | `sod_` | `(UserId, DateOnUtc)` | 每日 03:00 | 寫入所有用戶 |
| `StatisticOfWeek` | `sow_` | `(UserId, Year, Month, WeekOfMonth)` | 每週一 03:00 | 寫入所有用戶 |
| `StatisticOfMonth` | `som_` | `(UserId, Year, Month)` | 每月 1 日 03:00 | 僅有交易用戶 |
| `StatisticOfYear` | `soy_` | `(UserId, Year)` | 每年 1/1 03:00 | 寫入所有用戶 |

> **注意**: 所有時間皆為台灣時區 (UTC+8)，使用 `TaipeiNow`/`TaipeiToday` 計算統計邊界。

---

## 🔗 外部資源

### **官方文檔**
- [.NET 8 文檔](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [EF Core 8 文檔](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [FluentValidation 文檔](https://docs.fluentvalidation.net/)
- [Quartz.NET 文檔](https://www.quartz-scheduler.net/documentation/)

### **學習資源**
- [Vertical Slice Architecture](https://jimmybogard.com/vertical-slice-architecture/)
- [Result Pattern in C#](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/)
- [ULID Spec](https://github.com/ulid/spec)

---

**最後更新**: 2026-01-30 by GitHub Copilot
