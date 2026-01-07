# Vertical Slice Feature Template

> **用途**: 新增功能時使用的完整模板
>
> **架構**: Vertical Slice Architecture + CQRS + Result Pattern
>
> **最後更新**: 2026-01-06

---

## 📋 使用說明

1. 複製此模板到對應的 Feature 檔案
2. 替換 `{Domain}` 為領域名稱（如 `Expenditure`, `Income`）
3. 替換 `{Action}` 為操作名稱（如 `Create`, `Get`, `Update`, `Delete`）
4. 實作業務邏輯
5. 建立對應的 Request/Response Contracts

---

## 🎯 完整 Feature 檔案模板

```csharp
// File: Features/{Domain}/{Action}{Domain}.cs

using BookKeeper.Api.Contracts.{Domain};
using BookKeeper.Api.Database;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Entities;
using BookKeeper.Api.Shared;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BookKeeper.Api.Features.{Domain};

public static class {Action}{Domain}
{
    // ═══════════════════════════════════════════════════════════
    // Command/Query Definition
    // ═══════════════════════════════════════════════════════════
    
    public class Command : IRequest<Result<string>>
    {
        // ✅ 定義輸入屬性（使用 init accessor）
        public string PropertyName { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public DateOnly Date { get; init; }
        public string RelatedId { get; init; } = string.Empty;
    }
    
    // 或使用 Query（僅查詢時）
    // public class Query : IRequest<Result<{Domain}Response>>
    // {
    //     public string Id { get; init; } = string.Empty;
    // }
    
    // ═══════════════════════════════════════════════════════════
    // Validation Rules
    // ═══════════════════════════════════════════════════════════
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // ✅ 定義驗證規則
            RuleFor(x => x.PropertyName)
                .NotEmpty()
                .WithMessage("Property name is required")
                .MaximumLength(200)
                .WithMessage("Property name must not exceed 200 characters");
                
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0");
                
            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("Date is required");
                
            RuleFor(x => x.RelatedId)
                .NotEmpty()
                .WithMessage("Related ID is required");
        }
    }
    
    // ═══════════════════════════════════════════════════════════
    // Business Logic Handler
    // ═══════════════════════════════════════════════════════════
    
    internal sealed class Handler(ApplicationDbContext context) 
        : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(
            Command request, 
            CancellationToken cancellationToken)
        {
            // ══════════════════════════════════════════════════
            // Step 1: 驗證相依實體是否存在
            // ══════════════════════════════════════════════════
            
            // ✅ 檢查關聯實體（如 Label）
            var relatedEntity = await context.RelatedEntities
                .FindAsync([request.RelatedId], cancellationToken);
                
            if (relatedEntity is null)
            {
                return Result.Failure<string>(
                    RelatedEntityErrors.NotFound);
            }
            
            // ══════════════════════════════════════════════════
            // Step 2: 業務邏輯驗證
            // ══════════════════════════════════════════════════
            
            // ✅ 檢查業務規則（如重複性）
            bool isDuplicate = await context.{Domain}s
                .AnyAsync(x => 
                    x.PropertyName == request.PropertyName && 
                    x.Date == request.Date,
                    cancellationToken);
                    
            if (isDuplicate)
            {
                return Result.Failure<string>(
                    {Domain}Errors.AlreadyExists);
            }
            
            // ══════════════════════════════════════════════════
            // Step 3: 建立/更新實體
            // ══════════════════════════════════════════════════
            
            // ✅ 使用 Entity Factory Pattern
            var entity = {Domain}.Create(
                request.PropertyName,
                request.Amount,
                request.Date,
                relatedEntity);
            
            // 或更新既有實體
            // entity.Update(request.PropertyName, request.Amount, ...);
            
            // ══════════════════════════════════════════════════
            // Step 4: 持久化到資料庫
            // ══════════════════════════════════════════════════
            
            context.{Domain}s.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            
            // ══════════════════════════════════════════════════
            // Step 5: 回傳結果
            // ══════════════════════════════════════════════════
            
            return entity.Id;  // 隱式轉換為 Result<string>
        }
    }
}

// ═══════════════════════════════════════════════════════════
// Endpoint Registration
// ═══════════════════════════════════════════════════════════

public class {Action}{Domain}Endpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // ✅ 選擇適當的 HTTP 方法
        app.MapPost("api/{domains}", async (
            {Action}{Domain}Request request,
            ISender sender) =>
        {
            // ══════════════════════════════════════════════════
            // Step 1: 映射 Request 到 Command
            // ══════════════════════════════════════════════════
            
            var command = new {Action}{Domain}.Command
            {
                PropertyName = request.PropertyName,
                Amount = request.Amount,
                Date = request.Date,
                RelatedId = request.RelatedId
            };
            
            // ══════════════════════════════════════════════════
            // Step 2: 發送 Command 並取得結果
            // ══════════════════════════════════════════════════
            
            Result<string> result = await sender.Send(command);
            
            // ══════════════════════════════════════════════════
            // Step 3: 根據結果回傳適當的 HTTP Response
            // ══════════════════════════════════════════════════
            
            return result.Match(
                onSuccess: (id) => Results.Created($"api/{domains}/{id}", id),
                onFailure: (error) => Results.BadRequest(error));
        })
        .WithTags(Tags.{Domain}s)
        .WithName("{Action}{Domain}")
        .WithSummary("{Action} a {domain}")
        .WithDescription("Creates/Updates/Deletes a {domain} with the provided information");
    }
}
```

---

## 📐 Contracts 模板

### **Request Contract**

```csharp
// File: Contracts/{Domain}/{Action}{Domain}Request.cs

namespace BookKeeper.Api.Contracts.{Domain};

public sealed record {Action}{Domain}Request
{
    public string PropertyName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateOnly Date { get; init; }
    public string RelatedId { get; init; } = string.Empty;
}
```

### **Response Contract**

```csharp
// File: Contracts/{Domain}/{Domain}Response.cs

namespace BookKeeper.Api.Contracts.{Domain};

public sealed record {Domain}Response
{
    public required string Id { get; init; }
    public required string PropertyName { get; init; }
    public required decimal Amount { get; init; }
    public required DateOnly Date { get; init; }
    public required string RelatedId { get; init; }
    public required string RelatedName { get; init; }
    public required DateTime CreatedOnUtc { get; init; }
    public DateTime? UpdatedOnUtc { get; init; }
}
```

---

## 🏗️ Entity 模板

```csharp
// File: Entities/{Domain}.cs

namespace BookKeeper.Api.Entities;

public sealed class {Domain}
{
    // ═══════════════════════════════════════════════════════════
    // Private Constructor (for EF Core only)
    // ═══════════════════════════════════════════════════════════
    
    private {Domain}() { }
    
    // ═══════════════════════════════════════════════════════════
    // Factory Method (Public API for creation)
    // ═══════════════════════════════════════════════════════════
    
    public static {Domain} Create(
        string propertyName,
        decimal amount,
        DateOnly date,
        RelatedEntity relatedEntity)
    {
        return new {Domain}
        {
            Id = $"{prefix}_{Ulid.NewUlid()}",  // e.g., "e_01J9KT..."
            PropertyName = propertyName,
            Amount = amount,
            Date = date,
            RelatedEntityId = relatedEntity.Id,
            RelatedEntity = relatedEntity,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
    
    // ═══════════════════════════════════════════════════════════
    // Update Method (Encapsulate mutation logic)
    // ═══════════════════════════════════════════════════════════
    
    public void Update(
        string propertyName, 
        decimal amount, 
        DateOnly date, 
        RelatedEntity relatedEntity)
    {
        PropertyName = propertyName;
        Amount = amount;
        Date = date;
        RelatedEntityId = relatedEntity.Id;
        RelatedEntity = relatedEntity;
        UpdatedOnUtc = DateTime.UtcNow;
    }
    
    // ═══════════════════════════════════════════════════════════
    // Properties (private set for encapsulation)
    // ═══════════════════════════════════════════════════════════
    
    public string Id { get; private set; } = string.Empty;
    public string PropertyName { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public string RelatedEntityId { get; private set; } = string.Empty;
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }
    
    // ═══════════════════════════════════════════════════════════
    // Navigation Properties
    // ═══════════════════════════════════════════════════════════
    
    public RelatedEntity RelatedEntity { get; private set; } = null!;
}
```

---

## 🔧 EF Configuration 模板

```csharp
// File: Database/Configurations/{Domain}Configuration.cs

using BookKeeper.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeper.Api.Database.Configurations;

internal sealed class {Domain}Configuration : IEntityTypeConfiguration<{Domain}>
{
    public void Configure(EntityTypeBuilder<{Domain}> builder)
    {
        // ═══════════════════════════════════════════════════════════
        // Primary Key
        // ═══════════════════════════════════════════════════════════
        
        builder.HasKey(x => x.Id);
        
        // ═══════════════════════════════════════════════════════════
        // Properties Configuration
        // ═══════════════════════════════════════════════════════════
        
        builder.Property(x => x.PropertyName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(x => x.Amount)
            .IsRequired()
            .HasPrecision(18, 2);  // For decimal
        
        builder.Property(x => x.Date)
            .IsRequired();
        
        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();
        
        builder.Property(x => x.UpdatedOnUtc)
            .IsRequired(false);
        
        // ═══════════════════════════════════════════════════════════
        // Foreign Keys & Relationships
        // ═══════════════════════════════════════════════════════════
        
        builder.HasOne(x => x.RelatedEntity)
            .WithMany()  // or WithMany(x => x.{Domain}s) if bidirectional
            .HasForeignKey(x => x.RelatedEntityId)
            .OnDelete(DeleteBehavior.Restrict);  // or Cascade
        
        // ═══════════════════════════════════════════════════════════
        // Indexes (Optional)
        // ═══════════════════════════════════════════════════════════
        
        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.RelatedEntityId);
    }
}
```

---

## 🚨 Errors 模板

```csharp
// File: Shared/Errors/{Domain}Errors.cs

using BookKeeper.Api.Shared;

namespace BookKeeper.Api.Shared.Errors;

public static class {Domain}Errors
{
    public static readonly Error NotFound = new(
        "{Domain}.NotFound",
        "The {domain} with the specified ID was not found");
    
    public static readonly Error AlreadyExists = new(
        "{Domain}.AlreadyExists",
        "A {domain} with the same properties already exists");
    
    public static readonly Error InvalidAmount = new(
        "{Domain}.InvalidAmount",
        "The amount must be greater than zero");
    
    public static readonly Error InvalidDate = new(
        "{Domain}.InvalidDate",
        "The date cannot be in the future");
}
```

---

## 📝 完整檢查清單

### **Phase 1: 建立檔案結構**
```
□ Features/{Domain}/{Action}{Domain}.cs
□ Contracts/{Domain}/{Action}{Domain}Request.cs
□ Contracts/{Domain}/{Domain}Response.cs
□ Entities/{Domain}.cs (若需新 Entity)
□ Database/Configurations/{Domain}Configuration.cs (若需新 Entity)
□ Shared/Errors/{Domain}Errors.cs (若需新錯誤)
```

### **Phase 2: 實作 Feature**
```
□ Command/Query 定義完整
□ Validator 包含所有必要規則
□ Handler 業務邏輯實作完成
□ Endpoint 註冊與路由正確
□ 錯誤處理使用 Result Pattern
```

### **Phase 3: 資料庫（若需新 Entity）**
```
□ Entity 使用 Factory Pattern
□ EF Configuration 完整配置
□ ApplicationDbContext 新增 DbSet
□ Migration 已建立且檢查
```

### **Phase 4: 測試**
```
□ 建置成功（無警告）
□ Migration 套用成功
□ 本地啟動成功
□ Swagger 顯示端點
□ 成功場景測試通過
□ 失敗場景測試通過
□ 資料庫資料正確
```

---

## 🎓 HTTP 方法選擇指南

| 操作 | HTTP Method | 端點範例 | 回傳 |
|------|------------|---------|------|
| Create | POST | `POST /api/expenditures` | 201 Created + Id |
| Get One | GET | `GET /api/expenditures/{id}` | 200 OK + Data |
| Get List | GET | `GET /api/expenditures` | 200 OK + List |
| Update | PUT | `PUT /api/expenditures/{id}` | 204 No Content |
| Delete | DELETE | `DELETE /api/expenditures/{id}` | 204 No Content |

---

**最後更新**: 2026-01-06 by AI Infrastructure Team
