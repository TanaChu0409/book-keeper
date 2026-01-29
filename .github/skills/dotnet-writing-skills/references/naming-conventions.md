# .NET Naming Conventions 速查表

## 快速參考

| 項目 | 規則 | 範例 |
|------|------|------|
| **Namespace** | PascalCase | `BookKeeper.Api.Features` |
| **類別/記錄** | PascalCase | `PaymentService`, `OrderRequest` |
| **介面** | PascalCase + `I` 前綴 | `IPaymentService`, `IRepository<T>` |
| **方法** | PascalCase | `GetCustomerById`, `ProcessPayment` |
| **屬性** | PascalCase | `FirstName`, `TotalAmount` |
| **公開欄位** | PascalCase | `MaxRetryCount`（極少使用） |
| **私有欄位** | _camelCase | `_customerRepository`, `_logger` |
| **參數** | camelCase | `customerId`, `paymentAmount` |
| **區域變數** | camelCase | `totalCount`, `userName` |
| **常數** | UPPERCASE_SNAKE_CASE | `MAX_RETRY_COUNT`, `DEFAULT_TIMEOUT` |
| **枚舉** | PascalCase | `OrderStatus`, `PaymentType` |
| **枚舉值** | PascalCase | `Pending`, `Completed` |

## 特殊規則

### Primary Constructors (C# 12+)

```csharp
// 記錄：參數使用 PascalCase
public record Person(string FirstName, string LastName, int Age);

// 類別/結構：參數使用 camelCase
public class Container(string label, int capacity)
{
    public string Label { get; } = label;
    public int Capacity { get; } = capacity;
}
```

### 私有欄位命名

```csharp
// ✅ 推薦：底線 + camelCase
private readonly ILogger<PaymentService> _logger;
private readonly ApplicationDbContext _context;

// ❌ 避免：無底線
private readonly ILogger logger; // 不推薦

// ❌ 避免：m_ 前綴（舊 C++ 風格）
private readonly ILogger m_logger; // 不推薦
```

### 非同步方法命名

```csharp
// ✅ 異步方法以 Async 結尾
public async Task<Customer> GetCustomerAsync(int id);
public async Task ProcessPaymentAsync(Payment payment);

// ❌ 同步方法不應有 Async
public Customer GetCustomer(int id); // 同步
```

### 泛型型別參數

```csharp
// ✅ 單一型別參數使用 T
public class Repository<T> where T : class { }

// ✅ 多型別參數使用描述性名稱，T 開頭
public interface IMapper<TSource, TDestination> { }
public class Dictionary<TKey, TValue> { }

// ✅ 限制型別參數也可使用描述性名稱
public class Handler<TRequest, TResponse>
    where TRequest : IRequest<TResponse> { }
```

## Boolean 屬性命名

```csharp
// ✅ 使用 Is/Has/Can 等前綴
public bool IsActive { get; set; }
public bool HasChildren { get; set; }
public bool CanEdit { get; set; }
public bool WasProcessed { get; set; }

// ❌ 避免否定式命名
public bool IsNotActive { get; set; } // 不推薦
public bool IsDisabled { get; set; }  // 改用 IsEnabled
```

## 集合命名

```csharp
// ✅ 使用複數形式
public List<Customer> Customers { get; set; }
public IEnumerable<Order> Orders { get; set; }

// ✅ Dictionary 使用 "by" 關鍵字
public Dictionary<int, Customer> CustomersById { get; set; }

// ❌ 避免 List/Array 後綴
public List<Customer> CustomerList { get; set; } // 不推薦
```

## 事件命名

```csharp
// ✅ 事件使用動詞或動詞片語
public event EventHandler OrderCreated;
public event EventHandler PaymentProcessing;
public event EventHandler<OrderEventArgs> OrderStatusChanged;

// ✅ 事件處理方法使用 "On" 前綴
protected virtual void OnOrderCreated(OrderEventArgs e) { }
```

## 縮寫與首字母縮寫

```csharp
// ✅ 2 個字母：全大寫
public class IOHelper { }
public string ID { get; set; }

// ✅ 3+ 個字母：僅首字母大寫
public class HtmlParser { }
public class XmlDocument { }

// 範例
public string CustomerId { get; set; }  // ✅
public string CustomerID { get; set; }  // ❌ (除非 ID 獨立)
```

## 單位與度量

```csharp
// ✅ 在名稱中包含單位
public int TimeoutInSeconds { get; set; }
public decimal PriceInUsd { get; set; }
public long SizeInBytes { get; set; }

// ✅ 或使用型別安全的值物件
public TimeSpan Timeout { get; set; }
public Money Price { get; set; }
```

## 常見錯誤

### ❌ 匈牙利命名法
```csharp
// ❌ 不使用型別前綴
string strName;
int iCount;
bool bIsValid;

// ✅ 應該改為
string name;
int count;
bool isValid;
```

### ❌ 不一致的大小寫
```csharp
// ❌ 混合風格
public class paymentService { }  // 應為 PaymentService
public void GetCustomer_ById() { }  // 應為 GetCustomerById
```

### ❌ 縮寫過度
```csharp
// ❌ 難以理解
public void PrcOrd() { }  // ProcessOrder
public class CustSvc { }  // CustomerService

// ✅ 使用完整名稱
public void ProcessOrder() { }
public class CustomerService { }
```

## Minimal API 端點命名

```csharp
// ✅ 使用 HTTP 動詞 + 資源名稱
app.MapGet("/api/customers", GetCustomers)
   .WithName("GetCustomers");  // 端點名稱使用 PascalCase

app.MapPost("/api/payments", CreatePayment)
   .WithName("CreatePayment");

// ✅ Tag 使用複數 PascalCase
.WithTags("Customers")
.WithTags("Payments")
```

## 測試方法命名

```csharp
// ✅ 模式：MethodName_Scenario_ExpectedBehavior
[Fact]
public void ProcessPayment_WithInvalidAmount_ReturnsError() { }

[Fact]
public void GetCustomer_WhenCustomerExists_ReturnsCustomer() { }

[Fact]
public void CreateOrder_WithValidData_CreatesOrderSuccessfully() { }
```

## 專案與命名空間

```csharp
// ✅ 專案命名：CompanyName.ProductName.Component
BookKeeper.Api
BookKeeper.Domain
BookKeeper.Infrastructure

// ✅ 命名空間遵循資料夾結構
BookKeeper.Api.Features.Payments
BookKeeper.Api.Features.Customers
```

## 快速檢查清單

代碼審查時檢查以下項目：

- [ ] 所有公開型別/成員使用 PascalCase
- [ ] 私有欄位使用 _camelCase（底線開頭）
- [ ] 介面以 `I` 開頭
- [ ] 異步方法以 `Async` 結尾
- [ ] Boolean 屬性使用 Is/Has/Can 前綴
- [ ] 集合使用複數名稱
- [ ] 沒有匈牙利命名法或型別前綴
- [ ] 縮寫規則正確（2 字母全大寫，3+ 字母僅首字母大寫）
- [ ] 命名具描述性，避免過度縮寫
- [ ] 常數使用 UPPERCASE_SNAKE_CASE
