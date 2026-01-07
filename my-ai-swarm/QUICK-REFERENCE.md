# BookKeeper Quick Reference

> **目的**: 為 AI Agent 提供快速查閱的決策樹與常用操作指南
>
> **專案**: BookKeeper Personal Finance API
>
> **最後更新**: 2026-01-06

---

## 🚦 新需求快速決策樹

```
收到新需求
    ↓
【第 1 步】需求分類
    ├─ 新增 CRUD 功能？ → 使用 FEATURE_TEMPLATE.md
    ├─ 修改既有功能？ → 評估影響範圍
    ├─ Bug 修復？ → 直接修復 + 更新測試
    └─ 架構變更？ → 啟動 Architect Review

【第 2 步】複雜度評估
    ├─ 新增 Entity？ → 需要 Migration（預估 +2h）
    ├─ 修改 Entity？ → 需要 Migration（預估 +1h）
    ├─ 僅業務邏輯？ → 快速實裝（預估 1-2h）
    └─ 跨 Domain？ → 需要評估相依性（預估 +3h）

【第 3 步】工作流程選擇
    ├─ 複雜度高？ → 流程 A（Architect → Impact → Developer → QA）
    ├─ 複雜度中？ → 流程 B（Architect → Developer → QA）
    └─ 複雜度低？ → 流程 C（Developer → QA）
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

# 查看 Swagger
# http://localhost:9000/swagger

# 查看 Aspire Dashboard（OpenTelemetry）
# http://localhost:18888
```

### **資料庫操作**

```bash
# 新增 Migration
dotnet ef migrations add {Name} -p BookKeeper/BookKeeper/BookKeeper.Api

# 套用 Migration
dotnet ef database update -p BookKeeper/BookKeeper/BookKeeper.Api

# 查看 Migration 清單
dotnet ef migrations list -p BookKeeper/BookKeeper/BookKeeper.Api

# 移除最後一個 Migration（未套用前）
dotnet ef migrations remove -p BookKeeper/BookKeeper/BookKeeper.Api

# 連接 PostgreSQL 容器
docker exec -it bookkeeper.database psql -U postgres -d bookkeeper
```

### **程式碼品質**

```bash
# 建置（含 Code Analysis）
dotnet build BookKeeper/BookKeeper/BookKeeper.sln

# 清理
dotnet clean BookKeeper/BookKeeper/BookKeeper.sln

# 格式化
dotnet format BookKeeper/BookKeeper/BookKeeper.sln

# 查看警告（應該為 0）
dotnet build BookKeeper/BookKeeper/BookKeeper.sln --no-incremental
```

---

## 📋 新增 Feature 快速清單

### **Phase 1: 規劃（Architect）**
```
□ 確認需求清晰？
□ 決定 Domain 分類（Expenditure/Income/Label/新？）
□ 是否需要新 Entity？
□ 設計 API 路徑與 HTTP 方法
```

### **Phase 2: 實作（Developer）**

**若需新 Entity**:
```bash
# 1. 建立 Entity
touch BookKeeper.Api/Entities/{Domain}.cs

# 2. 建立 EF Configuration
touch BookKeeper.Api/Database/Configurations/{Domain}Configuration.cs

# 3. 更新 DbContext
# 編輯 ApplicationDbContext.cs 新增 DbSet

# 4. 建立 Migration
dotnet ef migrations add Add{Domain} -p BookKeeper.Api

# 5. 檢查 Migration
# 查看 Migrations/Application/ 資料夾
```

**建立 Feature 檔案**:
```bash
# 建立主 Feature 檔案
touch BookKeeper.Api/Features/{Domain}/{Action}{Domain}.cs

# 建立 Contracts
touch BookKeeper.Api/Contracts/{Domain}/{Action}{Domain}Request.cs
touch BookKeeper.Api/Contracts/{Domain}/{Domain}Response.cs

# 建立 Errors（若需要）
touch BookKeeper.Api/Shared/Errors/{Domain}Errors.cs
```

### **Phase 3: 測試**
```
□ 本地啟動成功？
□ Swagger 顯示端點？
□ 測試成功場景（200/201）
□ 測試失敗場景（驗證錯誤、業務錯誤）
□ 檢查資料庫寫入正確？
□ OpenTelemetry Traces 正常？
```

---

## 🎯 常見任務模板

### **1. 新增簡單查詢**

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

## 🔗 外部資源

### **官方文檔**
- [.NET 8 文檔](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [EF Core 8 文檔](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [FluentValidation 文檔](https://docs.fluentvalidation.net/)

### **學習資源**
- [Vertical Slice Architecture](https://jimmybogard.com/vertical-slice-architecture/)
- [Result Pattern in C#](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/)
- [ULID Spec](https://github.com/ulid/spec)

---

**最後更新**: 2026-01-06 by AI Infrastructure Team
