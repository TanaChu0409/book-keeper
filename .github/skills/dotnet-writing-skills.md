# .NET 程式碼撰寫技巧與最佳實踐 (dotnet-writing-skills)

> **版本**: v1.0.0 | **建立日期**: 2026-01-28 | **來源**: Microsoft Official + Milan Jovanović

---

## 🎯 核心設計原則（Milan Jovanović 風格）

### 1. **Result Pattern Over Exceptions**
```csharp
// ✅ 優先：錯誤作為資料
public Result<Payment> CreatePayment(decimal amount)
{
    if (amount <= 0)
        return Result<Payment>.Failure(new Error("Invalid amount"));
    
    return Result<Payment>.Success(new Payment(amount));
}

// ❌ 避免：錯誤作為例外
// public Payment CreatePayment(decimal amount)
// {
//     if (amount <= 0)
//         throw new InvalidOperationException();
// }
```

### 2. **Vertical Slice Architecture**
- 按功能垂直切分，而非水平分層
- 每個 Feature 包含完整的 Command/Query → Validator → Handler → Endpoint

### 3. **Rich Domain Models**
- 業務邏輯封裝在 Domain 實體內
- 私有建構函式 + 靜態工廠方法
- 所有方法返回 Result

### 4. **CQRS with MediatR**
- Command：改變狀態
- Query：讀取資料
- 使用 MediatR 解耦

---

## 📖 目錄

1. [命名約定與慣例](#命名約定與慣例)
2. [現代 C# 語言特性](#現代-c-語言特性)
3. [程式碼結構與組織](#程式碼結構與組織)
4. [錯誤處理與例外](#錯誤處理與例外)
5. [效能與最佳化](#效能與最佳化)
6. [測試與可維護性](#測試與可維護性)
7. [非同步程式設計](#非同步程式設計)
8. [架構模式](#架構模式)
9. [資料庫與 EF Core](#資料庫與-ef-core)
10. [快速檢查清單](#快速檢查清單)

---

## 命名約定與慣例

### 基本原則

**Pascal Case (帕斯卡命名法)**
```csharp
// ✅ 使用於：類別、方法、屬性、事件、命名空間
public class CustomerService { }
public void ProcessPayment() { }
public string FirstName { get; set; }
public event EventHandler OrderCompleted;
namespace BookKeeper.Api.Features { }
```

**Camel Case (駝峰命名法)**
```csharp
// ✅ 使用於：私有欄位、區域變數、參數
private readonly ILogger _logger;
public void Calculate(int itemCount, decimal unitPrice)
{
    var totalAmount = itemCount * unitPrice;
}
```

**特殊規則**
```csharp
// ✅ Record 主要建構函式參數：Pascal Case
public record Person(string FirstName, string LastName);

// ✅ Class/Struct 主要建構函式參數：Camel Case
public class LabelledContainer<T>(string label)
{
    public string Label { get; } = label;
    public required T Contents { get; init; }
}

// ✅ 介面：I 前綴 + Pascal Case
public interface IPaymentService { }

// ❌ 避免：使用 Hungarian notation
// 錯誤: string strName;
// 正確: string name;
```

### 有意義的命名

```csharp
// ✅ 好的命名：清晰、具描述性
public async Task<Result<Payment>> ProcessPaymentAsync(
    PaymentRequest request, 
    CancellationToken cancellationToken)
{
    var validationResult = await ValidatePaymentAsync(request);
    if (validationResult.IsFailure)
    {
        return Result<Payment>.Failure(validationResult.Error);
    }
    
    var payment = Payment.Create(request.Amount, request.Currency);
    await _repository.AddAsync(payment, cancellationToken);
    
    return Result<Payment>.Success(payment);
}

// ❌ 避免：縮寫、模糊命名
// 錯誤:
public async Task<Result<Payment>> ProcPmt(PaymentReq req, CancellationToken ct)
{
    var vr = await ValPmt(req);
    // ...
}
```

### 布林命名

```csharp
// ✅ 使用 Is/Has/Can/Should 前綴
public bool IsActive { get; set; }
public bool HasPermission { get; set; }
public bool CanExecute { get; set; }
public bool ShouldValidate { get; set; }

// ✅ 查詢方法
public bool IsEligibleForDiscount(Customer customer)
{
    return customer.TotalPurchases > 1000m && customer.IsActive;
}
```

---

## 現代 C# 語言特性

### 使用 File-Scoped Namespace

```csharp
// ✅ 現代寫法：減少縮排
namespace BookKeeper.Api.Features.Payments;

public class PaymentService
{
    // ...
}

// ❌ 舊寫法：多餘的縮排
// namespace BookKeeper.Api.Features.Payments
// {
//     public class PaymentService { }
// }
```

### 使用 Target-Typed New

```csharp
// ✅ 當類型明確時，省略類型
ExampleClass instance = new();
List<string> names = new();
Dictionary<string, int> scores = new();

// ✅ 使用 var 時，需明確類型
var instance2 = new ExampleClass();
```

### 使用 Collection Expressions

```csharp
// ✅ .NET 8+ 集合表達式
string[] vowels = ["a", "e", "i", "o", "u"];
List<int> numbers = [1, 2, 3, 4, 5];
int[] combined = [..firstArray, ..secondArray];

// ❌ 舊寫法
// string[] vowels = new[] { "a", "e", "i", "o", "u" };
```

### 使用 Primary Constructors

```csharp
// ✅ 主要建構函式：減少樣板代碼
public sealed class PaymentService(
    IPaymentRepository repository,
    ILogger<PaymentService> logger,
    IDateTimeProvider dateTimeProvider)
{
    public async Task<Result> ProcessAsync(Payment payment)
    {
        logger.LogInformation("Processing payment {PaymentId}", payment.Id);
        await repository.AddAsync(payment);
        return Result.Success();
    }
}

// ❌ 舊寫法：手動欄位與建構函式
// public class PaymentService
// {
//     private readonly IPaymentRepository _repository;
//     private readonly ILogger<PaymentService> _logger;
//     
//     public PaymentService(IPaymentRepository repository, ILogger<PaymentService> logger)
//     {
//         _repository = repository;
//         _logger = logger;
//     }
// }
```

### 使用 Required Properties

```csharp
// ✅ required 屬性：強制初始化
public sealed class CreatePaymentRequest
{
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string CustomerId { get; init; }
}

// 使用時必須初始化
var request = new CreatePaymentRequest
{
    Amount = 100m,
    Currency = "USD",
    CustomerId = "cust_123"
};
```

### 使用 Raw String Literals

```csharp
// ✅ 原始字串：避免逸出字元
var query = """
    SELECT p.id, p.amount, p.currency
    FROM payments p
    WHERE p.status = 'Completed'
      AND p.created_at > @startDate
    """;

var json = """
    {
        "name": "John Doe",
        "path": "C:\Users\John\Documents"
    }
    """;

// ❌ 舊寫法：需要逸出
// var path = "C:\\Users\\John\\Documents";
```

### 使用 String Interpolation

```csharp
// ✅ 字串插補（短字串）
string displayName = $"{user.LastName}, {user.FirstName}";
logger.LogInformation($"Processing order {order.Id} for customer {customer.Name}");

// ✅ StringBuilder（迴圈/大量文字）
var builder = new StringBuilder();
for (int i = 0; i < 10000; i++)
{
    builder.Append($"Item {i}\n");
}

// ✅ 表達式式插補
foreach (var student in students)
{
    Console.WriteLine($"{student.Last} Score: {student.Score}");
}
```

### 使用 Pattern Matching

```csharp
// ✅ Switch Expression
public decimal CalculateDiscount(Customer customer) => customer.Type switch
{
    CustomerType.Premium => customer.TotalPurchases * 0.15m,
    CustomerType.Regular => customer.TotalPurchases * 0.05m,
    CustomerType.New => 0m,
    _ => throw new ArgumentException($"Unknown customer type: {customer.Type}")
};

// ✅ Property Pattern
public bool IsEligibleForRefund(Order order) => order switch
{
    { Status: OrderStatus.Completed, DaysSinceOrder: <= 30 } => true,
    { Status: OrderStatus.Cancelled } => false,
    _ => false
};

// ✅ Null 檢查
if (order is not null && order.IsValid)
{
    await ProcessOrderAsync(order);
}
```

---

## 程式碼結構與組織

### Using 指令位置

```csharp
// ✅ 放在 namespace 外部：使用完整限定名稱
using Microsoft.EntityFrameworkCore;
using BookKeeper.Api.Database;

namespace BookKeeper.Api.Features.Payments;

public class PaymentService { }

// ❌ 避免：放在 namespace 內部（可能造成命名衝突）
// namespace BookKeeper.Api.Features.Payments
// {
//     using Microsoft.EntityFrameworkCore;
// }
```

### 使用 Sealed 關鍵字

```csharp
// ✅ 不會被繼承的類別：使用 sealed
public sealed class PaymentService(
    IPaymentRepository repository,
    ILogger<PaymentService> logger)
{
    public async Task<Result<Payment>> CreateAsync(CreatePaymentRequest request)
    {
        // 實作邏輯
    }
}

// ✅ Record 類型預設為 sealed（除非使用 abstract）
public sealed record CreatePaymentCommand(decimal Amount, string Currency);
public sealed record PaymentResponse(string Id, decimal Amount);

// ✅ 好處：
// 1. 效能優化：編譯器可以進行虛擬方法去虛擬化（devirtualization）
// 2. 明確設計意圖：表明此類不應被繼承
// 3. 防止誤用：避免不當的繼承破壞封裝性

// ❌ 何時不使用 sealed：
// - 明確設計為可擴展的基底類別
// - 使用 Moq 等需要動態代理的測試框架（考慮改用介面）
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }
}
```

### 版面配置慣例

```csharp
// ✅ 良好的格式
public sealed class PaymentService(
    IPaymentRepository repository,
    ILogger<PaymentService> logger)
{
    // 每個方法間至少一個空行
    public async Task<Result<Payment>> CreateAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        // 使用空行分隔邏輯區塊
        var validation = ValidateRequest(request);
        if (validation.IsFailure)
        {
            return Result<Payment>.Failure(validation.Error);
        }
        
        var payment = Payment.Create(
            request.Amount, 
            request.Currency);
        
        await repository.AddAsync(payment, cancellationToken);
        
        return Result<Payment>.Success(payment);
    }
    
    // 私有方法在公開方法之後
    private Result ValidateRequest(CreatePaymentRequest request)
    {
        if (request.Amount <= 0)
        {
            return Result.Failure(
                new Error("Payment.InvalidAmount", "Amount must be positive"));
        }
        
        return Result.Success();
    }
}

// ✅ 縮排規則
// - 使用 4 個空格（不用 Tab）
// - 每行一個陳述式
// - 每行一個宣告
// - 連續行若不自動縮排，則手動縮排一個 Tab
```

### 使用 Allman 大括號風格

```csharp
// ✅ Allman 風格：左右大括號各自一行
public class PaymentService
{
    public async Task<Result> ProcessAsync(Payment payment)
    {
        if (payment.IsValid)
        {
            await SaveAsync(payment);
            return Result.Success();
        }
        
        return Result.Failure(new Error("Invalid payment"));
    }
}
```

---

## 錯誤處理與例外

### 核心原則：優先使用 Result Pattern

**Milan Jovanović 的錯誤處理哲學**：

```csharp
// ✅ 優先：Result Pattern（函數式錯誤處理）
// - 將錯誤視為預期的業務結果
// - 錯誤是資料，不是例外
// - 編譯時期檢查，強制處理錯誤

public async Task<Result<Payment>> CreatePaymentAsync(CreatePaymentRequest request)
{
    // 驗證失敗 → 返回 Result.Failure
    if (request.Amount <= 0)
        return Result<Payment>.Failure(new Error("Payment.InvalidAmount", "Amount must be positive"));
    
    // 業務規則違反 → 返回 Result.Failure
    if (!IsValidCurrency(request.Currency))
        return Result<Payment>.Failure(new Error("Payment.InvalidCurrency", "Unsupported currency"));
    
    var payment = Payment.Create(request.Amount, request.Currency);
    await _repository.AddAsync(payment);
    
    return Result<Payment>.Success(payment);
}

// ❌ 避免：使用 Exception 作為流程控制
// public Payment CreatePayment(CreatePaymentRequest request)
// {
//     if (request.Amount <= 0)
//         throw new InvalidOperationException("Amount must be positive");
//     
//     // Exception 的問題：
//     // 1. 效能開銷大（堆疊追蹤）
//     // 2. 中斷控制流程
//     // 3. 調用方可能忘記處理
//     // 4. 難以測試所有錯誤路徑
// }

// ⚠️ 何時使用 Exception：
// - 真正的異常情況（記憶體不足、網路中斷）
// - 不可預期的錯誤（程式設計錯誤、系統故障）
// - 基礎設施失敗（資料庫連線失敗）
```

### 使用 Result Pattern

```csharp
// ✅ Result Pattern：避免異常作為流程控制
public sealed record Error(string Code, string Message);

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    
    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result Success() => new(true, default!);
    public static Result Failure(Error error) => new(false, error);
}

public class Result<T> : Result
{
    public T Value { get; }
    
    private Result(T value, bool isSuccess, Error error) 
        : base(isSuccess, error)
    {
        Value = value;
    }
    
    public static Result<T> Success(T value) => 
        new(value, true, default!);
    
    public static new Result<T> Failure(Error error) => 
        new(default!, false, error);
}

// ✅ 使用 Result
public async Task<Result<Payment>> GetByIdAsync(
    string id, 
    CancellationToken cancellationToken)
{
    var payment = await _context.Payments
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    
    if (payment is null)
    {
        return Result<Payment>.Failure(
            new Error("Payment.NotFound", $"Payment {id} not found"));
    }
    
    return Result<Payment>.Success(payment);
}
```

### Result Pattern 進階用法

```csharp
// ✅ Result Extensions：鏈式操作
public static class ResultExtensions
{
    // Map：轉換成功值
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        return result.IsSuccess
            ? Result<TOut>.Success(mapper(result.Value))
            : Result<TOut>.Failure(result.Error);
    }
    
    // Bind：鏈接多個 Result 操作
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> func)
    {
        return result.IsSuccess
            ? await func(result.Value)
            : Result<TOut>.Failure(result.Error);
    }
    
    // Match：Pattern Matching
    public static T Match<TValue, T>(
        this Result<TValue> result,
        Func<TValue, T> onSuccess,
        Func<Error, T> onFailure)
    {
        return result.IsSuccess 
            ? onSuccess(result.Value) 
            : onFailure(result.Error);
    }
}

// ✅ 使用範例：優雅的錯誤處理
public async Task<Result<PaymentDto>> ProcessPaymentAsync(string paymentId)
{
    return await GetPaymentAsync(paymentId)
        .BindAsync(payment => ValidatePaymentAsync(payment))
        .BindAsync(payment => ChargePaymentAsync(payment))
        .Map(payment => payment.ToDto());
}

// ✅ Controller 中使用 Match
[HttpPost("{id}/process")]
public async Task<IActionResult> ProcessPayment(string id)
{
    var result = await _service.ProcessPaymentAsync(id);
    
    return result.Match(
        onSuccess: payment => Ok(payment),
        onFailure: error => error.Code switch
        {
            "Payment.NotFound" => NotFound(error),
            "Payment.InvalidStatus" => BadRequest(error),
            _ => StatusCode(500, error)
        }
    );
}
```

### 異常處理：僅用於真正的異常

```csharp
// ✅ Try-Catch：僅捕捉基礎設施層的不可預期錯誤
public async Task<Result<Payment>> SavePaymentAsync(
    Payment payment,
    CancellationToken cancellationToken)
{
    try
    {
        await _context.Payments.AddAsync(payment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Payment>.Success(payment);
    }
    catch (DbUpdateException ex)
    {
        // 基礎設施失敗 → 記錄並返回 Result.Failure
        _logger.LogError(ex, "Database error saving payment {PaymentId}", payment.Id);
        return Result<Payment>.Failure(
            new Error("Payment.DatabaseError", "Failed to save payment"));
    }
}

// ✅ 網路呼叫異常處理
public async Task<Result<PaymentResponse>> CallExternalApiAsync(
    PaymentRequest request)
{
    try
    {
        var response = await _httpClient.PostAsJsonAsync("/api/payments", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        return Result<PaymentResponse>.Success(result!);
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "HTTP request failed");
        return Result<PaymentResponse>.Failure(
            new Error("Payment.ExternalApiError", "External API unavailable"));
    }
    catch (TaskCanceledException ex)
    {
        _logger.LogWarning(ex, "Request timeout");
        return Result<PaymentResponse>.Failure(
            new Error("Payment.Timeout", "Request timed out"));
    }
}

// ❌ 避免：將業務邏輯錯誤作為異常
// if (amount <= 0)
//     throw new InvalidOperationException("Invalid amount");
```

### 使用 Using Statement

```csharp
// ✅ 現代 using：不需大括號
public async Task<string> ReadFileAsync(string path)
{
    using var stream = File.OpenRead(path);
    using var reader = new StreamReader(stream);
    return await reader.ReadToEndAsync();
}

// ✅ 傳統 using（明確範圍）
public async Task ProcessFileAsync(string path)
{
    using (var connection = new SqlConnection(connectionString))
    {
        await connection.OpenAsync();
        // 處理邏輯
    } // connection.Dispose() 在此自動呼叫
}
```

---

## 效能與最佳化

### 使用 Span<T> 與 Memory<T>

```csharp
// ✅ Span<T>：零複製切片
public static int CountVowels(ReadOnlySpan<char> text)
{
    int count = 0;
    foreach (char c in text)
    {
        if ("aeiouAEIOU".Contains(c))
        {
            count++;
        }
    }
    return count;
}

// 使用
string sentence = "Hello World";
int vowelCount = CountVowels(sentence.AsSpan(0, 5)); // "Hello"
```

### 避免不必要的配置

```csharp
// ✅ 使用 ValueTask<T>（非同步方法可能同步完成）
public ValueTask<Customer?> GetCachedCustomerAsync(string id)
{
    if (_cache.TryGetValue(id, out var customer))
    {
        return new ValueTask<Customer?>(customer); // 同步返回，無配置
    }
    
    return new ValueTask<Customer?>(GetFromDatabaseAsync(id));
}

// ✅ 使用 ArrayPool<T>（大型陣列）
public void ProcessLargeData()
{
    var buffer = ArrayPool<byte>.Shared.Rent(1024);
    try
    {
        // 處理邏輯
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
```

### 快取策略

```csharp
// ✅ 使用 HybridCache (.NET 9+)
public sealed class ProductService(HybridCache cache)
{
    public async Task<Product?> GetProductAsync(
        string id, 
        CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync(
            $"product:{id}",
            async ct => await _repository.GetByIdAsync(id, ct),
            cancellationToken: cancellationToken);
    }
}

// ✅ 快取失效（Pub/Sub）
public async Task InvalidateProductCacheAsync(string productId)
{
    await _cache.RemoveAsync($"product:{productId}");
    await _messageBus.PublishAsync(new ProductCacheInvalidated(productId));
}
```

---

## 測試與可維護性

### 單元測試最佳實踐

```csharp
// ✅ Arrange-Act-Assert 模式
[Fact]
public async Task CreatePayment_WithValidData_ShouldSucceed()
{
    // Arrange
    var request = new CreatePaymentRequest
    {
        Amount = 100m,
        Currency = "USD",
        CustomerId = "cust_123"
    };
    
    var repository = Substitute.For<IPaymentRepository>();
    var service = new PaymentService(repository);
    
    // Act
    var result = await service.CreateAsync(request, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Amount.Should().Be(100m);
    await repository.Received(1).AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
}

// ✅ 測試命名：MethodName_Scenario_ExpectedBehavior
[Fact]
public void CalculateDiscount_WhenCustomerIsPremium_ShouldApply15PercentDiscount()
{
    // ...
}
```

### 可測試設計

```csharp
// ✅ 依賴注入：使用介面
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

// 測試時可輕鬆 Mock
public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new(2026, 1, 1);
}
```

---

## 非同步程式設計

### 基本原則

```csharp
// ✅ 非同步方法命名：Async 後綴
public async Task<Result<Payment>> ProcessPaymentAsync(
    PaymentRequest request,
    CancellationToken cancellationToken)
{
    var payment = await _repository.GetByIdAsync(request.Id, cancellationToken);
    await payment.ProcessAsync(cancellationToken);
    await _repository.UpdateAsync(payment, cancellationToken);
    
    return Result<Payment>.Success(payment);
}

// ✅ 使用 CancellationToken
public async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken)
{
    return await _context.Orders
        .Where(o => o.IsActive)
        .ToListAsync(cancellationToken);
}
```

### ConfigureAwait 指引

```csharp
// ✅ Library code：使用 ConfigureAwait(false)
public async Task<string> GetDataAsync()
{
    var data = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
    return ProcessData(data);
}

// ✅ ASP.NET Core：不需要 ConfigureAwait（預設無 SynchronizationContext）
public async Task<IActionResult> GetPayment(string id)
{
    var payment = await _service.GetByIdAsync(id); // 不需 ConfigureAwait
    return Ok(payment);
}
```

### 避免 Async Void

```csharp
// ❌ 避免：async void（無法捕捉異常）
// public async void ProcessPaymentAsync() { }

// ✅ 使用：async Task
public async Task ProcessPaymentAsync()
{
    await _service.ProcessAsync();
}

// ✅ 事件處理器例外：async void
public async void OnButtonClick(object sender, EventArgs e)
{
    try
    {
        await ProcessAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing");
    }
}
```

---

## 架構模式

### Vertical Slice Architecture

```csharp
// ✅ 按功能垂直切分
Features/
├── Payments/
│   ├── CreatePayment/
│   │   ├── CreatePaymentCommand.cs
│   │   ├── CreatePaymentValidator.cs
│   │   ├── CreatePaymentHandler.cs
│   │   └── CreatePaymentEndpoint.cs
│   ├── GetPayment/
│   │   ├── GetPaymentQuery.cs
│   │   ├── GetPaymentHandler.cs
│   │   └── GetPaymentEndpoint.cs
│   └── PaymentErrors.cs

// ✅ Command/Query
public sealed record CreatePaymentCommand(
    decimal Amount,
    string Currency,
    string CustomerId) : IRequest<Result<PaymentResponse>>;

public sealed class CreatePaymentHandler(
    ApplicationDbContext context,
    IValidator<CreatePaymentCommand> validator)
    : IRequestHandler<CreatePaymentCommand, Result<PaymentResponse>>
{
    public async Task<Result<PaymentResponse>> Handle(
        CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        // ✅ FluentValidation 驗證失敗 → Result.Failure
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<PaymentResponse>.Failure(
                new Error("Validation.Failed", validationResult.ToString()));
        }
        
        // ✅ Domain 驗證失敗 → Result.Failure
        var paymentResult = Payment.Create(command.Amount, command.Currency);
        if (paymentResult.IsFailure)
        {
            return Result<PaymentResponse>.Failure(paymentResult.Error);
        }
        
        context.Payments.Add(paymentResult.Value);
        await context.SaveChangesAsync(cancellationToken);
        
        return Result<PaymentResponse>.Success(
            new PaymentResponse(paymentResult.Value.Id));
    }
}
```

### CQRS 模式

```csharp
// ✅ Command：改變狀態
public sealed record CreatePaymentCommand(decimal Amount) : IRequest<Result<Guid>>;

// ✅ Query：讀取資料
public sealed record GetPaymentQuery(Guid Id) : IRequest<Result<PaymentDto>>;

// ✅ 使用 MediatR
public sealed class PaymentsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePaymentCommand(request.Amount);
        var result = await sender.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Ok(result.Value) 
            : BadRequest(result.Error);
    }
}
```

### Rich Domain Model

```csharp
// ✅ 封裝業務邏輯的實體（使用 Result Pattern）
public sealed class Payment
{
    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    
    // 私有建構函式：防止無效狀態
    private Payment() { }
    
    // ✅ 靜態工廠方法：返回 Result 而非拋出異常
    public static Result<Payment> Create(decimal amount, string currency)
    {
        // 驗證邏輯：使用 Result Pattern
        if (amount <= 0)
        {
            return Result<Payment>.Failure(
                new Error("Payment.InvalidAmount", "Amount must be positive"));
        }
        
        if (string.IsNullOrWhiteSpace(currency))
        {
            return Result<Payment>.Failure(
                new Error("Payment.InvalidCurrency", "Currency is required"));
        }
        
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            Currency = currency,
            Status = PaymentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
        
        return Result<Payment>.Success(payment);
    }
    
    // ✅ 業務邏輯方法：返回 Result
    public Result Complete()
    {
        if (Status != PaymentStatus.Pending)
        {
            return Result.Failure(
                new Error("Payment.InvalidStatus", "Payment must be pending"));
        }
        
        Status = PaymentStatus.Completed;
        return Result.Success();
    }
    
    // ✅ 鏈式操作
    public Result Refund(decimal amount)
    {
        if (Status != PaymentStatus.Completed)
        {
            return Result.Failure(
                new Error("Payment.CannotRefund", "Only completed payments can be refunded"));
        }
        
        if (amount > Amount)
        {
            return Result.Failure(
                new Error("Payment.RefundExceedsAmount", "Refund amount exceeds payment amount"));
        }
        
        Status = PaymentStatus.Refunded;
        return Result.Success();
    }
}
```

---

## 資料庫與 EF Core

### DbContext 最佳實踐

```csharp
// ✅ 使用 IDbContextFactory（平行查詢）
public sealed class OrderService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<List<Order>> GetOrdersInParallelAsync()
    {
        var task1 = GetCompletedOrdersAsync();
        var task2 = GetPendingOrdersAsync();
        
        await Task.WhenAll(task1, task2);
        
        return task1.Result.Concat(task2.Result).ToList();
    }
    
    private async Task<List<Order>> GetCompletedOrdersAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Orders
            .Where(o => o.Status == OrderStatus.Completed)
            .ToListAsync();
    }
    
    private async Task<List<Order>> GetPendingOrdersAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Orders
            .Where(o => o.Status == OrderStatus.Pending)
            .ToListAsync();
    }
}

// ❌ 避免：共用 DbContext（非執行緒安全）
```

### Entity Configuration

```csharp
// ✅ 使用 IEntityTypeConfiguration
public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => new PaymentId(value));
        
        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .IsRequired();
        
        builder.HasIndex(p => new { p.CustomerId, p.Status });
    }
}
```

### 查詢最佳化

```csharp
// ✅ 使用 AsNoTracking（唯讀查詢）
public async Task<List<PaymentDto>> GetPaymentsAsync(CancellationToken cancellationToken)
{
    return await _context.Payments
        .AsNoTracking()
        .Where(p => p.Status == PaymentStatus.Completed)
        .Select(p => new PaymentDto
        {
            Id = p.Id,
            Amount = p.Amount,
            Currency = p.Currency
        })
        .ToListAsync(cancellationToken);
}

// ✅ 避免 N+1 查詢
public async Task<List<Order>> GetOrdersWithItemsAsync(CancellationToken cancellationToken)
{
    return await _context.Orders
        .Include(o => o.Items)
        .Where(o => o.IsActive)
        .ToListAsync(cancellationToken);
}

// ✅ 分頁查詢
public async Task<PagedResult<Payment>> GetPagedPaymentsAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken)
{
    var totalCount = await _context.Payments.CountAsync(cancellationToken);
    
    var items = await _context.Payments
        .OrderByDescending(p => p.CreatedAtUtc)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
    
    return new PagedResult<Payment>(items, totalCount, page, pageSize);
}
```

---

## 快速檢查清單

### 代碼審查檢查清單

**命名**
- [ ] 使用 Pascal Case（類別、方法、屬性）
- [ ] 使用 Camel Case（參數、區域變數）
- [ ] 介面使用 `I` 前綴
- [ ] 非同步方法使用 `Async` 後綴
- [ ] 布林使用 `Is/Has/Can/Should` 前綴

**現代 C# 特性**
- [ ] 使用 file-scoped namespace
- [ ] 使用 primary constructors（適當時）
- [ ] 使用 required properties
- [ ] 使用 collection expressions
- [ ] 使用 raw string literals（多行字串）
- [ ] 使用 pattern matching

**架構**
- [ ] 遵循 Vertical Slice Architecture
- [ ] Command/Query 分離（CQRS）
- [ ] **優先使用 Result Pattern（避免 throw exception）**
- [ ] Domain 方法返回 Result 而非拋出異常
- [ ] Exception 僅用於基礎設施失敗或不可預期錯誤
- [ ] Rich Domain Model（業務邏輯在實體內）

**效能**
- [ ] 使用 `AsNoTracking()`（唯讀查詢）
- [ ] 避免 N+1 查詢
- [ ] 使用 `CancellationToken`
- [ ] 考慮使用 `ValueTask<T>`（高頻呼叫）
- [ ] 使用 `Span<T>`（性能關鍵路徑）

**測試**
- [ ] 單元測試覆蓋率 > 70%
- [ ] 使用 Arrange-Act-Assert
- [ ] 測試命名清晰
- [ ] Mock 外部依賴

**一般**
- [ ] 不會被繼承的類別使用 `sealed`
- [ ] 使用 4 空格縮排
- [ ] 使用 Allman 大括號風格
- [ ] 每行限制 120 字元
- [ ] 方法間至少一個空行
- [ ] Using 指令在 namespace 外部
- [ ] 使用有意義的註解（避免過度註解）

---

## 🔗 參考資源

### 官方文檔
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Entity Framework Core Best Practices](https://learn.microsoft.com/en-us/ef/core/)

### Milan Jovanović 推薦閱讀
- [Vertical Slice Architecture](https://www.milanjovanovic.tech/blog/vertical-slice-architecture)
- [CQRS Pattern](https://www.milanjovanovic.tech/blog/cqrs-pattern-with-mediatr)
- [EF Core Performance](https://www.milanjovanovic.tech/blog/ef-core-performance-tips)
- [Result Pattern](https://www.milanjovanovic.tech/blog/functional-error-handling-in-dotnet-with-the-result-pattern)

### 工具
- **EditorConfig**: 自動格式化配置
- **StyleCop Analyzers**: 程式碼風格檢查
- **SonarAnalyzer**: 程式碼品質分析
- **Roslynator**: Roslyn 分析器與重構

---

## 📝 版本歷史

| 版本 | 日期 | 變更內容 |
|------|------|---------|
| v1.0.0 | 2026-01-28 | 初始版本：整合 Microsoft 官方約定與 Milan Jovanović 最佳實踐 |

---

**最後更新**: 2026-01-28  
**維護者**: BookKeeper AI Swarm  
**狀態**: ✅ Active
