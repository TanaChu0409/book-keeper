# 現代 C# 特性指南 (C# 8 - C# 12+)

## C# 12 特性 (.NET 8+)

### 1. Primary Constructors

```csharp
// ✅ 類別使用 Primary Constructor
public class PaymentService(
    IPaymentRepository repository,
    ILogger<PaymentService> logger)
{
    // 參數自動成為可用的欄位
    public async Task ProcessAsync(Payment payment)
    {
        logger.LogInformation("Processing {Id}", payment.Id);
        await repository.SaveAsync(payment);
    }
}

// ✅ 結構使用 Primary Constructor
public struct Point(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;
}

// ✅ 記錄使用 Primary Constructor（參數為屬性）
public record Customer(string FirstName, string LastName, string Email);

// 相當於：
public record Customer
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    
    public Customer(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }
}
```

### 2. Collection Expressions

```csharp
// ✅ 陣列初始化
int[] numbers = [1, 2, 3, 4, 5];
string[] names = ["Alice", "Bob", "Charlie"];

// ✅ List 初始化
List<int> list = [1, 2, 3, 4, 5];

// ✅ Spread Operator (..)
int[] first = [1, 2, 3];
int[] second = [4, 5, 6];
int[] combined = [..first, 0, ..second]; // [1, 2, 3, 0, 4, 5, 6]

// ✅ 條件元素
bool includeZero = true;
int[] numbers = [1, 2, ..( includeZero ? [0] : [] ), 3, 4];

// ✅ 適用於任何集合型別
IEnumerable<string> query = ["a", "b", "c"];
HashSet<int> set = [1, 2, 3, 4];
```

### 3. Raw String Literals

```csharp
// ✅ 多行字串（至少 3 個引號）
var json = """
    {
        "name": "John",
        "age": 30,
        "city": "New York"
    }
    """;

// ✅ SQL 查詢
var sql = """
    SELECT c.Name, c.Email, o.Total
    FROM Customers c
    INNER JOIN Orders o ON c.Id = o.CustomerId
    WHERE o.Status = 'Pending'
    ORDER BY o.CreatedDate DESC
    """;

// ✅ 包含引號和特殊字元（無需跳脫）
var path = """C:\Users\John\Documents\file.txt""";
var message = """He said "Hello!" and left.""";

// ✅ 插值 Raw String
var name = "John";
var age = 30;
var json = $$"""
    {
        "name": "{{name}}",
        "age": {{age}}
    }
    """;
```

### 4. Alias Any Type

```csharp
// ✅ 為複雜型別建立別名
using Point = (int X, int Y);
using CustomerMap = Dictionary<string, Customer>;
using PaymentResult = Result<Payment, Error>;

// 使用
Point point = (10, 20);
CustomerMap customers = new();
PaymentResult result = ProcessPayment(request);
```

## C# 11 特性 (.NET 7)

### 1. Required Members

```csharp
// ✅ 使用 required 強制初始化
public class CreateOrderRequest
{
    public required string CustomerId { get; init; }
    public required decimal Amount { get; init; }
    public string? Description { get; init; }  // 可選
}

// 使用時必須提供 required 屬性
var request = new CreateOrderRequest
{
    CustomerId = "C123",
    Amount = 100.50m
    // Description 可省略
};

// ❌ 編譯錯誤：未提供 CustomerId
var invalid = new CreateOrderRequest
{
    Amount = 100.50m
};
```

### 2. File-Scoped Types

```csharp
// ✅ File-scoped type：僅在當前檔案可見
file class InternalHelper
{
    public static void DoSomething() { }
}

// 其他檔案無法存取 InternalHelper
```

### 3. Generic Attributes

```csharp
// ✅ 泛型屬性
public class TypedAttribute<T> : Attribute
{
    public T Value { get; }
    public TypedAttribute(T value) => Value = value;
}

// 使用
[Typed<int>(42)]
public class MyClass { }
```

## C# 10 特性 (.NET 6)

### 1. File-Scoped Namespace

```csharp
// ✅ 推薦：節省一層縮排
namespace BookKeeper.Api.Features.Payments;

public class PaymentService
{
    public void Process() { }
}

// ❌ 舊風格
namespace BookKeeper.Api.Features.Payments
{
    public class PaymentService
    {
        public void Process() { }
    }
}
```

### 2. Global Using Directives

```csharp
// GlobalUsings.cs
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Microsoft.EntityFrameworkCore;

// 其他檔案無需重複這些 using
```

### 3. Constant Interpolated Strings

```csharp
// ✅ 常數字串可使用插值
const string SchemaName = "dbo";
const string TableName = "Customers";
const string FullName = $"{SchemaName}.{TableName}"; // "dbo.Customers"
```

## C# 9 特性 (.NET 5)

### 1. Record Types

```csharp
// ✅ 不可變記錄
public record Customer(string FirstName, string LastName, string Email);

// ✅ with 表達式（非破壞性變更）
var customer1 = new Customer("John", "Doe", "john@example.com");
var customer2 = customer1 with { Email = "john.doe@example.com" };

// ✅ 記錄可繼承
public record Employee(string FirstName, string LastName, string Email, string Department)
    : Customer(FirstName, LastName, Email);
```

### 2. Init-Only Properties

```csharp
// ✅ init 存取子：僅在初始化時可設定
public class Customer
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
}

var customer = new Customer
{
    FirstName = "John",
    LastName = "Doe"
};

// ❌ 編譯錯誤：無法在初始化後修改
customer.FirstName = "Jane";
```

### 3. Top-Level Statements

```csharp
// ✅ Program.cs 無需 Main 方法
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
```

## C# 8 特性 (.NET Core 3.1+)

### 1. Nullable Reference Types

```csharp
#nullable enable

// ✅ 明確標註可空與不可空
public class Customer
{
    public string Name { get; set; }        // 不可空
    public string? Email { get; set; }      // 可空
}

// ✅ Null-forgiving operator (!)
public void Process(Customer? customer)
{
    if (customer is not null)
    {
        Console.WriteLine(customer.Name);  // 安全
    }
    
    // 或使用 ! 告知編譯器確定不為 null
    Console.WriteLine(customer!.Name);
}
```

### 2. Pattern Matching Enhancements

```csharp
// ✅ Property patterns
public decimal CalculateDiscount(Customer customer) => customer switch
{
    { IsVip: true, TotalSpent: > 10000 } => 0.2m,
    { IsVip: true } => 0.1m,
    { TotalSpent: > 5000 } => 0.05m,
    _ => 0m
};

// ✅ Tuple patterns
public string GetQuadrant(int x, int y) => (x, y) switch
{
    (> 0, > 0) => "Quadrant 1",
    (< 0, > 0) => "Quadrant 2",
    (< 0, < 0) => "Quadrant 3",
    (> 0, < 0) => "Quadrant 4",
    _ => "Origin"
};
```

### 3. Using Declarations

```csharp
// ✅ 簡化的 using（無大括號）
public async Task ProcessFileAsync(string path)
{
    using var reader = new StreamReader(path);
    var content = await reader.ReadToEndAsync();
    // reader 會在方法結束時自動 Dispose
}

// ❌ 舊風格
public async Task ProcessFileAsync(string path)
{
    using (var reader = new StreamReader(path))
    {
        var content = await reader.ReadToEndAsync();
    }
}
```

## 最佳實踐組合範例

### 完整 Feature 範例（結合所有現代特性）

```csharp
// CreatePaymentCommand.cs
namespace BookKeeper.Api.Features.Payments.CreatePayment;

public record CreatePaymentCommand(
    string CustomerId,
    decimal Amount,
    string Description) : IRequest<Result<PaymentResponse>>;

// CreatePaymentValidator.cs
file class CreatePaymentValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

// CreatePaymentHandler.cs
public class CreatePaymentHandler(
    ApplicationDbContext context,
    ILogger<CreatePaymentHandler> logger)
    : IRequestHandler<CreatePaymentCommand, Result<PaymentResponse>>
{
    public async Task<Result<PaymentResponse>> Handle(
        CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        // 驗證
        var validator = new CreatePaymentValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            return Result<PaymentResponse>.Failure(new Error("Validation.Failed", errors));
        }
        
        // 業務邏輯
        var payment = Payment.Create(
            command.CustomerId,
            command.Amount,
            command.Description);
        
        context.Payments.Add(payment);
        await context.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Payment {PaymentId} created", payment.Id);
        
        return Result<PaymentResponse>.Success(new PaymentResponse(payment.Id));
    }
}

// CreatePaymentEndpoint.cs
public class CreatePaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments", HandleAsync)
            .WithName("CreatePayment")
            .WithTags("Payments")
            .Produces<PaymentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
    
    private static async Task<IResult> HandleAsync(
        CreatePaymentRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CreatePaymentCommand(
            request.CustomerId,
            request.Amount,
            request.Description);
        
        var result = await sender.Send(command, ct);
        
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }
}
```

## 遷移建議

### 從舊代碼升級到現代 C#

1. **啟用 Nullable Reference Types**
   ```xml
   <PropertyGroup>
       <Nullable>enable</Nullable>
   </PropertyGroup>
   ```

2. **轉換為 File-Scoped Namespace**
   - 移除命名空間大括號
   - 減少一層縮排

3. **使用 Primary Constructors**
   - 移除私有欄位宣告
   - 簡化建構函式

4. **採用 Collection Expressions**
   - 將 `new[]` 改為 `[]`
   - 使用 spread operator `..`

5. **Raw String Literals**
   - 多行字串使用 `"""`
   - SQL/JSON 字串使用 raw literals

## 檢查清單

升級代碼時檢查：

- [ ] 使用 File-Scoped Namespaces
- [ ] 使用 Primary Constructors（C# 12）
- [ ] 使用 Collection Expressions `[]`（C# 12）
- [ ] 多行字串使用 Raw String Literals
- [ ] 啟用 Nullable Reference Types
- [ ] 不可變資料使用 Record Types
- [ ] Using declarations 不使用大括號
- [ ] Pattern matching 取代 if-else
- [ ] Required properties 取代建構函式參數
- [ ] Global usings 統一管理常用命名空間
