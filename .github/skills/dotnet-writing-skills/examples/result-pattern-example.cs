// Result Pattern 實作範例（來自 Milan Jovanovic）

namespace BookKeeper.Api.Shared;

// ==================== Result Pattern 核心 ====================

/// <summary>
/// 代表操作結果，包含成功值或錯誤。
/// </summary>
public record Result
{
    public bool IsSuccess { get; init; }
    public Error? Error { get; init; }
    
    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(Error error) => new() { IsSuccess = false, Error = error };
    
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// 泛型 Result，包含成功時的值。
/// </summary>
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public Error? Error { get; init; }
    
    public static Result<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };
    
    public static Result<T> Failure(Error error) => new()
    {
        IsSuccess = false,
        Error = error
    };
    
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}

/// <summary>
/// 錯誤資訊封裝。
/// </summary>
public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    
    public static Error NotFound(string entityName, string identifier) => new(
        $"{entityName}.NotFound",
        $"{entityName} with identifier '{identifier}' was not found");
    
    public static Error Validation(string propertyName, string message) => new(
        $"Validation.{propertyName}",
        message);
}

// ==================== 擴展方法 ====================

public static class ResultExtensions
{
    /// <summary>
    /// 將 Result 轉換為 IResult（Minimal API）。
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }
        
        return result.Error!.Code switch
        {
            var code when code.EndsWith(".NotFound") => Results.NotFound(result.Error),
            var code when code.StartsWith("Validation.") => Results.BadRequest(result.Error),
            _ => Results.Problem(
                title: "An error occurred",
                detail: result.Error.Message,
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
    
    /// <summary>
    /// 將 Result 轉換為 ActionResult（Controller）。
    /// </summary>
    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }
        
        return result.Error!.Code switch
        {
            var code when code.EndsWith(".NotFound") => new NotFoundObjectResult(result.Error),
            var code when code.StartsWith("Validation.") => new BadRequestObjectResult(result.Error),
            _ => new ObjectResult(result.Error)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            }
        };
    }
    
    /// <summary>
    /// Match pattern：根據成功/失敗執行不同動作。
    /// </summary>
    public static TResult Match<T, TResult>(
        this Result<T> result,
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value!)
            : onFailure(result.Error!);
    }
    
    /// <summary>
    /// Map：轉換成功值。
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        return result.IsSuccess
            ? Result<TOut>.Success(mapper(result.Value!))
            : Result<TOut>.Failure(result.Error!);
    }
    
    /// <summary>
    /// Bind：鏈接多個可能失敗的操作。
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        return result.IsSuccess
            ? await binder(result.Value!)
            : Result<TOut>.Failure(result.Error!);
    }
}

// ==================== 使用範例 1: Service 層 ====================

public sealed class PaymentService(
    IPaymentRepository repository,
    ICustomerRepository customerRepository,
    ILogger<PaymentService> logger)
{
    public async Task<Result<Payment>> CreatePaymentAsync(
        string customerId,
        decimal amount,
        CancellationToken ct = default)
    {
        // 驗證輸入
        if (amount <= 0)
        {
            return Error.Validation(nameof(amount), "Amount must be greater than 0");
        }
        
        // 檢查客戶是否存在
        var customerResult = await customerRepository.GetByIdAsync(customerId, ct);
        if (!customerResult.IsSuccess)
        {
            logger.LogWarning("Customer {CustomerId} not found", customerId);
            return Error.NotFound("Customer", customerId);
        }
        
        // 建立支付
        var payment = Payment.Create(
            customerId,
            amount,
            DateTime.UtcNow);
        
        // 儲存
        await repository.AddAsync(payment, ct);
        
        logger.LogInformation("Payment {PaymentId} created", payment.Id);
        
        return payment;  // 隱式轉換為 Result<Payment>
    }
    
    public async Task<Result> ProcessPaymentAsync(
        string paymentId,
        CancellationToken ct = default)
    {
        // 取得支付
        var paymentResult = await repository.GetByIdAsync(paymentId, ct);
        if (!paymentResult.IsSuccess)
        {
            return paymentResult.Error!;
        }
        
        var payment = paymentResult.Value!;
        
        // 處理支付（Domain 方法可能回傳 Result）
        var processResult = payment.Process(DateTime.UtcNow);
        if (!processResult.IsSuccess)
        {
            logger.LogWarning(
                "Failed to process payment {PaymentId}: {Error}",
                paymentId,
                processResult.Error!.Message);
            return processResult;
        }
        
        // 儲存變更
        await repository.UpdateAsync(payment, ct);
        
        logger.LogInformation("Payment {PaymentId} processed successfully", paymentId);
        
        return Result.Success();
    }
}

// ==================== 使用範例 2: Minimal API Endpoint ====================

public sealed class CreatePaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments", async (
            CreatePaymentRequest request,
            PaymentService service,
            CancellationToken ct) =>
        {
            var result = await service.CreatePaymentAsync(
                request.CustomerId,
                request.Amount,
                ct);
            
            return result.ToHttpResult();
        })
        .WithName("CreatePayment")
        .WithTags("Payments");
    }
}

// ==================== 使用範例 3: Controller ====================

[ApiController]
[Route("api/payments")]
public class PaymentsController(PaymentService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Payment>> CreatePayment(
        CreatePaymentRequest request,
        CancellationToken ct)
    {
        var result = await service.CreatePaymentAsync(
            request.CustomerId,
            request.Amount,
            ct);
        
        return result.ToActionResult();
    }
    
    [HttpPost("{id}/process")]
    public async Task<ActionResult> ProcessPayment(
        string id,
        CancellationToken ct)
    {
        var result = await service.ProcessPaymentAsync(id, ct);
        
        if (result.IsSuccess)
        {
            return Ok();
        }
        
        return result.Error!.Code switch
        {
            var code when code.EndsWith(".NotFound") => NotFound(result.Error),
            _ => BadRequest(result.Error)
        };
    }
}

// ==================== 使用範例 4: Chaining with Map/Bind ====================

public sealed class OrderService(
    IOrderRepository orderRepository,
    PaymentService paymentService,
    IEmailService emailService)
{
    public async Task<Result<OrderConfirmation>> CreateOrderWithPaymentAsync(
        CreateOrderRequest request,
        CancellationToken ct)
    {
        // 建立訂單
        var orderResult = await CreateOrderAsync(request, ct);
        
        // 使用 Bind 鏈接操作
        return await orderResult
            .BindAsync(order => paymentService.CreatePaymentAsync(
                order.CustomerId,
                order.Total,
                ct))
            .BindAsync(async payment =>
            {
                // 發送確認信
                await emailService.SendOrderConfirmationAsync(
                    orderResult.Value!.Id,
                    payment.Id,
                    ct);
                
                return Result<OrderConfirmation>.Success(
                    new OrderConfirmation(orderResult.Value.Id, payment.Id));
            });
        
        // 若任一步驟失敗，整個鏈會短路並回傳錯誤
    }
    
    private async Task<Result<Order>> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken ct)
    {
        var order = Order.Create(
            request.CustomerId,
            request.Items,
            DateTime.UtcNow);
        
        await orderRepository.AddAsync(order, ct);
        
        return order;
    }
}

// ==================== 使用範例 5: Match Pattern ====================

public sealed class PaymentReportService(PaymentService paymentService)
{
    public async Task<string> GeneratePaymentReportAsync(
        string paymentId,
        CancellationToken ct)
    {
        var result = await paymentService.GetPaymentAsync(paymentId, ct);
        
        // 使用 Match pattern 處理成功/失敗
        return result.Match(
            onSuccess: payment => 
                $"Payment {payment.Id}: {payment.Amount} {payment.Currency}",
            onFailure: error => 
                $"Error: {error.Message}");
    }
}

// ==================== Repository 範例（也使用 Result） ====================

public interface IPaymentRepository
{
    Task<Result<Payment>> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Result> AddAsync(Payment payment, CancellationToken ct = default);
    Task<Result> UpdateAsync(Payment payment, CancellationToken ct = default);
}

public sealed class PaymentRepository(ApplicationDbContext context) : IPaymentRepository
{
    public async Task<Result<Payment>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var payment = await context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        
        if (payment is null)
        {
            return Error.NotFound("Payment", id);
        }
        
        return payment;
    }
    
    public async Task<Result> AddAsync(Payment payment, CancellationToken ct = default)
    {
        try
        {
            context.Payments.Add(payment);
            await context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return new Error("Database.Error", ex.Message);
        }
    }
    
    public async Task<Result> UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        try
        {
            context.Payments.Update(payment);
            await context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return new Error("Database.ConcurrencyError", "The record was modified by another user");
        }
    }
}

// ==================== 常見錯誤定義 ====================

public static class CommonErrors
{
    // Payment Errors
    public static class Payment
    {
        public static Error NotFound(string id) => Error.NotFound("Payment", id);
        public static Error InvalidAmount => new("Payment.InvalidAmount", "Amount must be greater than 0");
        public static Error AlreadyProcessed => new("Payment.AlreadyProcessed", "Payment has already been processed");
    }
    
    // Customer Errors
    public static class Customer
    {
        public static Error NotFound(string id) => Error.NotFound("Customer", id);
        public static Error EmailRequired => Error.Validation("Email", "Email is required");
        public static Error InvalidEmail => Error.Validation("Email", "Invalid email format");
    }
    
    // Order Errors
    public static class Order
    {
        public static Error NotFound(string id) => Error.NotFound("Order", id);
        public static Error EmptyCart => new("Order.EmptyCart", "Cannot create order with empty cart");
    }
}

/*
 * ==================== 總結 ====================
 * 
 * Result Pattern 優點：
 * 1. 明確的錯誤處理：呼叫方必須處理錯誤
 * 2. 避免例外作為流程控制：例外僅用於真正的異常情況
 * 3. 可組合性：使用 Map/Bind 鏈接操作
 * 4. 型別安全：編譯時期檢查
 * 5. 可讀性：程式碼意圖清晰
 * 
 * 何時使用：
 * - 業務邏輯錯誤（驗證失敗、資料不存在）
 * - 可預期的失敗情況
 * - 需要回傳詳細錯誤資訊給 API 呼叫方
 * 
 * 何時不使用（改用例外）：
 * - 真正的異常情況（資料庫連線失敗、記憶體不足）
 * - 程式錯誤（ArgumentNullException）
 * - 基礎設施層級錯誤
 */
