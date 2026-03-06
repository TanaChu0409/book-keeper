---
name: dotnet-writing-skills
description: |
  為 .NET C# 專案提供編碼最佳實踐與風格指引，結合 Microsoft 官方 Coding Conventions 與 Milan Jovanovic 的現代 .NET 技巧。
  何時使用：(1) 編寫新的 C# 代碼；(2) 重構既有代碼以符合最佳實踐；(3) 代碼審查時檢查編碼風格；(4) 採用現代 C# 語言特性。
  涵蓋命名規範、語言特性、架構模式、效能優化、CQRS/VSA 實踐。
license: MIT
compatibility: |
  - .NET 8+ / C# 12+
  - GitHub Copilot Chat
  - Visual Studio / VS Code
  - 適用於 Clean Architecture、Vertical Slice Architecture、CQRS 專案
metadata:
  category: coding-standards
  requires-review: false
  version: "1.0"
  spec-version: "agentskills.io"
  tags:
    - dotnet
    - csharp
    - clean-code
    - best-practices
    - vertical-slice-architecture
    - cqrs
    - naming-conventions
    - performance
allowed-tools: |
  semantic_search
  read_file
  grep_search
  list_dir
  get_errors
  file_search
  replace_string_in_file
  multi_replace_string_in_file
---

## 觸發情境

本技能適用於以下場景：

1. **新代碼編寫**：建立新的 Controller、Service、Entity、Feature 時
2. **代碼重構**：改進既有代碼品質，採用現代 C# 特性
3. **代碼審查**：驗證代碼是否符合團隊標準與最佳實踐
4. **架構決策**：選擇適當的架構模式（VSA vs Layered）
5. **效能優化**：識別並改善效能瓶頸
6. **LINQ 查詢**：撰寫高效且可讀的查詢
7. **錯誤處理**：實作 Result Pattern 與適當的例外處理

## 核心原則

### 1. 命名規範（Microsoft Official Guidelines）

#### **PascalCase**
- **類別、記錄、結構**: `CustomerService`, `OrderRequest`, `PaymentResult`
- **介面**: `IPaymentService`, `IRepository<T>`（以 `I` 開頭）
- **方法**: `GetCustomerById`, `ProcessPayment`, `ValidateInput`
- **屬性**: `FirstName`, `OrderTotal`, `IsActive`
- **公開欄位**: `MaxRetryCount`（極少使用）
- **Namespace**: `BookKeeper.Api.Features.Payments`
- **記錄主要建構函式參數**: `public record Person(string FirstName, string LastName);`

#### **camelCase**
- **私有欄位**: `_customerRepository`, `_logger`（以底線開頭）
- **區域變數**: `customerName`, `totalAmount`, `isValid`
- **方法參數**: `public void Process(int orderId, string userName)`
- **類別/結構主要建構函式參數**: `public class Container(string label) { }`

#### **UPPERCASE_SNAKE_CASE**
- **常數**: `const int MAX_RETRY_COUNT = 3;`
- **枚舉值**（舊風格，現代傾向 PascalCase）

### 2. 現代 C# 語言特性（來自 Milan Jovanovic）

#### **Sealed Classes（封閉類別）**
```csharp
// ✅ 推薦：不打算被繼承的類別應使用 sealed
public sealed class PaymentService(
    IPaymentRepository repository,
    ILogger<PaymentService> logger)
{
    public async Task ProcessAsync(Payment payment)
    {
        logger.LogInformation("Processing payment {Id}", payment.Id);
        await repository.SaveAsync(payment);
    }
}

// ✅ Service、Handler、Repository 通常應該是 sealed
public sealed class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, Result>
{
    // Implementation
}

// ❌ 避免：非必要的繼承可能性
public class PaymentService { } // 可被繼承，但通常不需要

// ✅ 僅在明確設計為可繼承時才不用 sealed
public abstract class BaseRepository<T> { } // 設計為基底類別
public class PaymentRepository : BaseRepository<Payment> { } // 繼承，但仍可 sealed
```

**Sealed 的好處**：
- **效能優化**：編譯器可進行虛擬方法調用的去虛擬化（devirtualization）
- **設計意圖**：明確表達「此類不應被繼承」
- **安全性**：防止非預期的繼承導致的行為變更

**何時使用 sealed**：
- ✅ Services、Handlers、Repositories
- ✅ DTOs、Requests、Responses
- ✅ Validators（FluentValidation）
- ✅ Endpoints（Minimal API）
- ❌ 設計為基底類別的抽象類別
- ❌ 明確需要被繼承的類別

#### **Primary Constructors**
```csharp
// ✅ 推薦（C# 12+）：結合 sealed 與 Primary Constructor
public sealed class PaymentService(
    IPaymentRepository repository,
    ILogger<PaymentService> logger)
{
    public async Task ProcessAsync(Payment payment)
    {
        logger.LogInformation("Processing payment {Id}", payment.Id);
        await repository.SaveAsync(payment);
    }
}

// ❌ 避免（舊風格）
public class PaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<PaymentService> _logger;
    
    public PaymentService(IPaymentRepository repository, ILogger<PaymentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}
```

#### **Required Properties**
```csharp
// ✅ 推薦：使用 required 屬性而非建構函式強制
public class CreateOrderRequest
{
    public required string CustomerId { get; init; }
    public required decimal Amount { get; init; }
    public DateTime? OrderDate { get; init; }
}

// 使用
var request = new CreateOrderRequest
{
    CustomerId = "C123",
    Amount = 100.50m
    // OrderDate 可選
};
```

#### **Collection Expressions (C# 12+)**
```csharp
// ✅ 推薦
string[] vowels = ["a", "e", "i", "o", "u"];
List<int> numbers = [1, 2, 3, 4, 5];
int[] combined = [..array1, ..array2];

// ❌ 避免
string[] vowels = new[] { "a", "e", "i", "o", "u" };
```

#### **File-Scoped Namespaces**
```csharp
// ✅ 推薦
namespace BookKeeper.Api.Features.Payments;

public class PaymentService { }

// ❌ 避免（浪費縮排層級）
namespace BookKeeper.Api.Features.Payments
{
    public class PaymentService { }
}
```

#### **Raw String Literals**
```csharp
// ✅ 推薦：多行字串、JSON、SQL
var sql = """
    SELECT *
    FROM orders
    WHERE customer_id = @customerId
        AND status = 'Pending'
    """;

// ❌ 避免：使用跳脫字元
var sql = "SELECT *\nFROM orders\nWHERE customer_id = @customerId";
```

### 3. String 處理最佳實踐

```csharp
// ✅ 短字串串連：使用 String Interpolation
string displayName = $"{user.LastName}, {user.FirstName}";

// ✅ 迴圈中大量字串：使用 StringBuilder
var builder = new StringBuilder();
for (var i = 0; i < 10000; i++)
{
    builder.Append(phrase);
}

// ✅ 表達式字串插值
Console.WriteLine($"{student.Last} Score: {student.Score}");
```

### 4. 例外處理與錯誤管理

#### **Result Pattern（Milan Jovanovic 推薦）**
```csharp
// ✅ 推薦：Result Pattern 用於業務邏輯錯誤
public record Result<T>
{
    public T? Value { get; init; }
    public Error? Error { get; init; }
    public bool IsSuccess => Error is null;
    
    public static Result<T> Success(T value) => new() { Value = value };
    public static Result<T> Failure(Error error) => new() { Error = error };
}

// 使用
public async Task<Result<Payment>> ProcessPaymentAsync(PaymentRequest request)
{
    if (request.Amount <= 0)
        return Result<Payment>.Failure(new Error("Payment.InvalidAmount", "Amount must be greater than 0"));
    
    var payment = await _repository.SaveAsync(request);
    return Result<Payment>.Success(payment);
}

// ❌ 避免：使用例外處理業務邏輯錯誤
public async Task<Payment> ProcessPaymentAsync(PaymentRequest request)
{
    if (request.Amount <= 0)
        throw new InvalidOperationException("Invalid amount"); // 不應該用例外
}
```

#### **try-catch 使用規範**
```csharp
// ✅ 僅捕捉可以處理的例外
try
{
    return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
}
catch (ArithmeticException ex)
{
    _logger.LogError(ex, "Arithmetic overflow");
    throw; // 重新拋出
}

// ✅ 使用 using 簡化 Dispose
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

// ❌ 避免：捕捉一般例外
catch (Exception ex) { } // 過於寬泛
```

### 5. LINQ 查詢最佳實踐

```csharp
// ✅ 使用有意義的查詢變數名稱
var seattleCustomers = from customer in Customers
                       where customer.City == "Seattle"
                       select customer.Name;

// ✅ 使用別名確保 Pascal Case
var localDistributors =
    from customer in Customers
    join distributor in Distributors on customer.City equals distributor.City
    select new { Customer = customer, Distributor = distributor };

// ✅ where 子句優先（減少資料集）
var seattleCustomers = from customer in Customers
                       where customer.City == "Seattle"
                       orderby customer.Name
                       select customer;

// ✅ 使用多個 from 存取內部集合
var highScores = from student in students
                 from score in student.Scores
                 where score > 90
                 select new { Last = student.LastName, score };

// ❌ 避免：使用 join 來存取集合
```

### 6. 隱含型別 (var) 使用規範

```csharp
// ✅ 型別明確時使用 var
var message = "This is clearly a string.";
var customer = new Customer();
var result = await GetCustomerAsync(id);

// ✅ for 迴圈使用 var
for (var i = 0; i < count; i++) { }

// ❌ 避免：型別不明確時使用 var
var count = GetCount(); // 回傳型別未知
int count = GetCount(); // 明確

// ✅ foreach 使用明確型別（除非是匿名型別）
foreach (Customer customer in customers) { }

// ✅ LINQ 查詢結果使用 var（通常是匿名型別）
var results = from c in customers
              select new { c.Name, c.City };
```

### 7. Delegates 與 Lambda

```csharp
// ✅ 使用 Func<> 和 Action<> 而非自訂委派
Action<string> log = message => _logger.LogInformation(message);
Func<int, int, int> add = (x, y) => x + y;

// ✅ Lambda 用於簡短事件處理
button.Click += (s, e) => MessageBox.Show("Clicked!");

// ❌ 避免：定義新委派型別（除非必要）
public delegate void CustomDelegate(string message);
```

### 8. 運算子最佳實踐

```csharp
// ✅ 使用 && 和 || （短路求值）
if ((divisor != 0) && (dividend / divisor) is var result)
{
    Console.WriteLine($"Result: {result}");
}

// ❌ 避免：& 和 |（會評估兩邊）
if ((divisor != 0) & (dividend / divisor > 0)) { } // 危險

// ✅ new 運算子簡潔形式
var customer = new Customer();
Customer customer = new(); // 型別已知時

// ✅ 使用物件初始設定式
var order = new Order
{
    Id = 123,
    Amount = 100.50m,
    Status = OrderStatus.Pending
};
```

### 9. 版面配置與格式

```csharp
// ✅ Allman 大括號風格（各自獨立一行）
public void ProcessOrder()
{
    if (IsValid())
    {
        Save();
    }
}

// ✅ 每行一個陳述式
int x = 1;
int y = 2;

// ✅ 運算式使用括號清楚分隔
if ((startX > endX) && (startX > previousX))
{
    // Take action
}

// ✅ 方法之間至少一個空白行
public void Method1()
{
    // Implementation
}

public void Method2()
{
    // Implementation
}
```

### 10. using 指令位置

```csharp
// ✅ 推薦：using 放在 namespace 外部（明確完整名稱）
using Microsoft.EntityFrameworkCore;
using BookKeeper.Api.Database;

namespace BookKeeper.Api.Features.Payments;

public class PaymentService { }

// ❌ 避免：using 在 namespace 內部（相對名稱，可能衝突）
namespace BookKeeper.Api.Features.Payments
{
    using Microsoft.EntityFrameworkCore;
}
```

## Vertical Slice Architecture 實踐（Milan Jovanovic）

### Feature 組織結構
```
Features/
├── Payments/
│   ├── CreatePayment/
│   │   ├── CreatePaymentCommand.cs       # Command 定義
│   │   ├── CreatePaymentValidator.cs     # FluentValidation
│   │   ├── CreatePaymentHandler.cs       # MediatR Handler
│   │   └── CreatePaymentEndpoint.cs      # Minimal API Endpoint
│   ├── GetPayment/
│   │   ├── GetPaymentQuery.cs
│   │   ├── GetPaymentHandler.cs
│   │   └── GetPaymentEndpoint.cs
│   └── Shared/
│       └── PaymentErrors.cs              # Shared errors for feature
```

### CQRS Command/Query Pattern
```csharp
// ✅ Command（修改狀態）
public record CreatePaymentCommand(
    string CustomerId,
    decimal Amount,
    string Description) : IRequest<Result<PaymentResponse>>;

// ✅ Query（讀取資料）
public record GetPaymentQuery(string PaymentId) : IRequest<Result<PaymentResponse>>;

// ✅ Handler
public class CreatePaymentHandler(
    ApplicationDbContext context,
    ILogger<CreatePaymentHandler> logger)
    : IRequestHandler<CreatePaymentCommand, Result<PaymentResponse>>
{
    public async Task<Result<PaymentResponse>> Handle(
        CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        // Business logic
        var payment = Payment.Create(command.CustomerId, command.Amount);
        
        context.Payments.Add(payment);
        await context.SaveChangesAsync(cancellationToken);
        
        return Result<PaymentResponse>.Success(new PaymentResponse(payment.Id));
    }
}
```

### Minimal API Endpoints
```csharp
// ✅ IEndpoint 模式
public class CreatePaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments", async (
            CreatePaymentRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new CreatePaymentCommand(
                request.CustomerId,
                request.Amount,
                request.Description);
            
            var result = await sender.Send(command, ct);
            
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("CreatePayment")
        .WithTags("Payments")
        .Produces<PaymentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
```

## 效能優化技巧（Milan Jovanovic）

### 1. 避免 N+1 查詢
```csharp
// ❌ N+1 問題
var orders = await context.Orders.ToListAsync();
foreach (var order in orders)
{
    var customer = await context.Customers.FindAsync(order.CustomerId); // N 次查詢
}

// ✅ 使用 Include
var orders = await context.Orders
    .Include(o => o.Customer)
    .ToListAsync();

// ✅ 使用 Select 投影（更高效）
var orderDtos = await context.Orders
    .Select(o => new OrderDto
    {
        OrderId = o.Id,
        CustomerName = o.Customer.Name
    })
    .ToListAsync();
```

### 2. AsNoTracking 用於唯讀查詢
```csharp
// ✅ 唯讀查詢使用 AsNoTracking
var customers = await context.Customers
    .AsNoTracking()
    .Where(c => c.City == "Seattle")
    .ToListAsync();
```

### 3. 分頁查詢
```csharp
// ✅ 使用 Skip/Take 分頁
public async Task<PagedResult<Customer>> GetCustomersAsync(int page, int pageSize)
{
    var query = context.Customers.AsNoTracking();
    
    var total = await query.CountAsync();
    
    var items = await query
        .OrderBy(c => c.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PagedResult<Customer>(items, total, page, pageSize);
}
```

### 4. 平行化查詢（謹慎使用）
```csharp
// ✅ 使用 Task.WhenAll 並行執行獨立查詢
var customerTask = context.Customers.FindAsync(customerId);
var ordersTask = context.Orders.Where(o => o.CustomerId == customerId).ToListAsync();
var paymentsTask = context.Payments.Where(p => p.CustomerId == customerId).ToListAsync();

await Task.WhenAll(customerTask, ordersTask, paymentsTask);

var customer = await customerTask;
var orders = await ordersTask;
var payments = await paymentsTask;

// ⚠️ 警告：DbContext 不是執行緒安全，需使用 IDbContextFactory
```

## 註解與文檔

```csharp
// ✅ 單行註解簡短說明
// Calculate the total with tax

// ✅ XML 註解用於公開 API
/// <summary>
/// Processes a payment transaction.
/// </summary>
/// <param name="request">The payment request details.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>A result containing the payment response or error.</returns>
public async Task<Result<PaymentResponse>> ProcessPaymentAsync(
    PaymentRequest request,
    CancellationToken cancellationToken)
{
    // Implementation
}

// ❌ 避免：多行註解 /* */（除非必要）
// ❌ 避免：過時的註解
// ❌ 避免：顯而易見的註解
// i++; // Increment i by 1
```

## 安全性最佳實踐

```csharp
// ✅ 使用參數化查詢（EF Core 自動處理）
var customer = await context.Customers
    .FirstOrDefaultAsync(c => c.Email == email);

// ❌ 避免：字串串連 SQL（SQL Injection 風險）
var sql = $"SELECT * FROM Customers WHERE Email = '{email}'"; // 危險

// ✅ 驗證輸入
if (string.IsNullOrWhiteSpace(request.Email))
    return Result<Customer>.Failure(Errors.Customer.EmailRequired);

// ✅ 使用 FluentValidation
public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .LessThanOrEqualTo(10000);
    }
}
```

## 檢查清單 / 輸出要求

撰寫或審查 .NET C# 代碼時，確認以下項目：

### 命名規範
- [ ] 類別/介面/方法/屬性使用 **PascalCase**
- [ ] 私有欄位使用 **_camelCase**（底線開頭）
- [ ] 參數/區域變數使用 **camelCase**
- [ ] 介面以 **`I`** 開頭（例如 `IPaymentService`）
- [ ] 常數使用 **UPPERCASE_SNAKE_CASE**

### 現代 C# 特性
- [ ] 不會被繼承的類別使用 **sealed**（Service/Handler/Repository）
- [ ] 使用 **Primary Constructors**（C# 12+）
- [ ] 使用 **required** 屬性而非建構函式
- [ ] 使用 **Collection Expressions** `[]`
- [ ] 使用 **File-Scoped Namespaces**
- [ ] 使用 **Raw String Literals** 處理多行字串
- [ ] `using` 指令放在 namespace 外部

### LINQ 與查詢
- [ ] 查詢變數名稱有意義（例如 `seattleCustomers`）
- [ ] `where` 子句優先（縮減資料集）
- [ ] 避免 N+1 查詢問題（使用 `Include` 或 `Select`）
- [ ] 唯讀查詢使用 `AsNoTracking()`
- [ ] 分頁查詢使用 `Skip()`/`Take()`

### 錯誤處理
- [ ] 業務邏輯錯誤使用 **Result Pattern**（不用例外）
- [ ] 僅捕捉可處理的特定例外
- [ ] 使用 `using` 簡化資源釋放
- [ ] 重新拋出例外使用 `throw;`（保留堆疊追蹤）

### 效能
- [ ] 迴圈中字串串連使用 `StringBuilder`
- [ ] 避免不必要的 `ToList()` 呼叫
- [ ] 使用 `async/await` 處理 I/O 操作
- [ ] 考慮使用 `IAsyncEnumerable<T>` 串流大資料集

### 程式碼品質
- [ ] 每行一個陳述式/宣告
- [ ] 使用 Allman 大括號風格
- [ ] 方法之間至少一個空白行
- [ ] 複雜邏輯使用括號清楚分隔
- [ ] XML 註解記錄公開 API

### Vertical Slice Architecture（若適用）
- [ ] Feature 按垂直切片組織
- [ ] Command/Query 分離清楚
- [ ] 使用 MediatR 處理請求
- [ ] Minimal API Endpoints 遵循 `IEndpoint` 模式
- [ ] Validator 使用 FluentValidation

## 範例提示

以下是可以直接使用的自然語言提示：

### 代碼生成
- 「請使用 Vertical Slice Architecture 為 Payments 功能建立一個 CreatePayment Command，包含 Validator、Handler 和 Endpoint」
- 「幫我建立一個 GetCustomerById Query，使用 Result Pattern 和 AsNoTracking」
- 「為 Order Entity 建立一個 Rich Domain Model，使用私有建構函式和靜態工廠方法」

### 代碼審查
- 「請審查這個 PaymentService 是否符合 .NET 命名規範和最佳實踐」
- 「檢查這個 LINQ 查詢是否有 N+1 問題，並提供優化建議」
- 「檢查哪些類別應該加上 sealed 關鍵字」

### 重構
- 「請將這個類別重構為使用 Primary Constructor、sealed 和 required 屬性」
- 「將這個例外處理改為 Result Pattern」
- 「優化這個 LINQ 查詢以避免 N+1 問題」
- 「為所有不需要繼承的類別加上 sealed 關鍵字rn」
- 「優化這個 LINQ 查詢以避免 N+1 問題」

## 相關資源

- [Microsoft C# Coding Conventions](https://learn.microsoft.com/zh-tw/dotnet/csharp/fundamentals/coding-style/coding-conventions) - 官方編碼規範
- [Milan Jovanovic Blog](https://www.milanjovanovic.tech/blog) - 現代 .NET 架構與模式
- [examples/vertical-slice-example.cs](examples/vertical-slice-example.cs) - VSA 完整範例
- [examples/result-pattern-example.cs](examples/result-pattern-example.cs) - Result Pattern 實作
- [references/naming-conventions.md](references/naming-conventions.md) - 命名規範速查表
- [references/modern-csharp-features.md](references/modern-csharp-features.md) - C# 12+ 新特性指南

相關 Skills：
- [skill-builder](../skill-builder/SKILL.md) - Agent Skill 建立指南
- [test-driven-development](../test-driven-development/SKILL.md) - TDD 開發流程（若存在）

## 版本更新

- **v1.0** (2026-01-29): 初始版本，整合 Microsoft 官方規範與 Milan Jovanovic 最佳實踐
