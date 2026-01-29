// Vertical Slice Architecture 完整範例
// BookKeeper.Api/Features/Payments/CreatePayment/

namespace BookKeeper.Api.Features.Payments.CreatePayment;

// ==================== Command ====================
/// <summary>
/// Command to create a new payment transaction.
/// </summary>
public record CreatePaymentCommand(
    string CustomerId,
    decimal Amount,
    string Currency,
    string? Description) : IRequest<Result<PaymentResponse>>;

// ==================== Contracts ====================
public record CreatePaymentRequest
{
    public required string CustomerId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public string? Description { get; init; }
}

public record PaymentResponse(
    string PaymentId,
    string Status,
    DateTime CreatedAt);

// ==================== Validator ====================
file class CreatePaymentValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required")
            .MaximumLength(50);
        
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("Amount cannot exceed 1,000,000");
        
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a 3-letter code (e.g., USD, EUR)");
        
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);
    }
}

// ==================== Handler ====================
public sealed class CreatePaymentHandler(
    ApplicationDbContext context,
    ILogger<CreatePaymentHandler> logger,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreatePaymentCommand, Result<PaymentResponse>>
{
    public async Task<Result<PaymentResponse>> Handle(
        CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing payment creation for customer {CustomerId}, amount {Amount} {Currency}",
            command.CustomerId,
            command.Amount,
            command.Currency);
        
        // 驗證
        var validator = new CreatePaymentValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            
            logger.LogWarning(
                "Validation failed for payment creation: {Errors}",
                string.Join(", ", errors));
            
            return Result<PaymentResponse>.Failure(
                new Error("Payment.ValidationFailed", string.Join("; ", errors)));
        }
        
        // 檢查客戶是否存在
        var customerExists = await context.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Id == command.CustomerId, cancellationToken);
        
        if (!customerExists)
        {
            logger.LogWarning("Customer {CustomerId} not found", command.CustomerId);
            return Result<PaymentResponse>.Failure(
                PaymentErrors.CustomerNotFound(command.CustomerId));
        }
        
        // 建立 Payment Entity（Rich Domain Model）
        var payment = Payment.Create(
            command.CustomerId,
            command.Amount,
            command.Currency,
            command.Description,
            dateTimeProvider.UtcNow);
        
        // 儲存
        context.Payments.Add(payment);
        await context.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation(
            "Payment {PaymentId} created successfully for customer {CustomerId}",
            payment.Id,
            command.CustomerId);
        
        // 回傳成功結果
        var response = new PaymentResponse(
            payment.Id,
            payment.Status.ToString(),
            payment.CreatedAt);
        
        return Result<PaymentResponse>.Success(response);
    }
}

// ==================== Endpoint ====================
public sealed class CreatePaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments", HandleAsync)
            .WithName("CreatePayment")
            .WithTags(Tags.Payments)
            .WithDescription("Create a new payment transaction")
            .Produces<PaymentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }
    
    private static async Task<IResult> HandleAsync(
        CreatePaymentRequest request,
        ISender sender,
        ILogger<CreatePaymentEndpoint> logger,
        CancellationToken ct)
    {
        logger.LogDebug("Received create payment request for customer {CustomerId}", request.CustomerId);
        
        var command = new CreatePaymentCommand(
            request.CustomerId,
            request.Amount,
            request.Currency,
            request.Description);
        
        var result = await sender.Send(command, ct);
        
        if (result.IsSuccess)
        {
            logger.LogInformation("Payment created successfully: {PaymentId}", result.Value!.PaymentId);
            return Results.Ok(result.Value);
        }
        
        logger.LogWarning("Payment creation failed: {Error}", result.Error);
        
        // 根據錯誤型別回傳適當的 HTTP 狀態碼
        return result.Error!.Code switch
        {
            "Payment.CustomerNotFound" => Results.NotFound(result.Error),
            "Payment.ValidationFailed" => Results.BadRequest(result.Error),
            _ => Results.Problem(
                title: "Payment Creation Failed",
                detail: result.Error.Message,
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}

// ==================== Domain Entity (Rich Domain Model) ====================
// BookKeeper.Api/Entities/Payment.cs
public class Payment
{
    // 私有建構函式：強制使用工廠方法
    private Payment() { }
    
    public string Id { get; private set; } = null!;
    public string CustomerId { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string? Description { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    
    // 靜態工廠方法：建立新 Payment
    public static Payment Create(
        string customerId,
        decimal amount,
        string currency,
        string? description,
        DateTime utcNow)
    {
        return new Payment
        {
            Id = Ulid.NewUlid().ToString(),
            CustomerId = customerId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Description = description,
            Status = PaymentStatus.Pending,
            CreatedAt = utcNow
        };
    }
    
    // 業務方法：處理支付
    public Result Process(DateTime utcNow)
    {
        if (Status != PaymentStatus.Pending)
        {
            return Result.Failure(
                new Error("Payment.InvalidStatus", "Payment is not in pending status"));
        }
        
        Status = PaymentStatus.Processed;
        ProcessedAt = utcNow;
        
        return Result.Success();
    }
    
    // 業務方法：取消支付
    public Result Cancel()
    {
        if (Status == PaymentStatus.Processed)
        {
            return Result.Failure(
                new Error("Payment.AlreadyProcessed", "Cannot cancel processed payment"));
        }
        
        Status = PaymentStatus.Cancelled;
        
        return Result.Success();
    }
}

public enum PaymentStatus
{
    Pending,
    Processed,
    Cancelled,
    Failed
}

// ==================== Error Definitions ====================
// BookKeeper.Api/Features/Payments/Shared/PaymentErrors.cs
file static class PaymentErrors
{
    public static Error CustomerNotFound(string customerId) => new(
        "Payment.CustomerNotFound",
        $"Customer with ID '{customerId}' was not found");
    
    public static Error InvalidAmount => new(
        "Payment.InvalidAmount",
        "Payment amount must be greater than 0");
    
    public static Error InvalidCurrency => new(
        "Payment.InvalidCurrency",
        "Invalid currency code");
    
    public static Error PaymentNotFound(string paymentId) => new(
        "Payment.NotFound",
        $"Payment with ID '{paymentId}' was not found");
}

// ==================== EF Core Configuration ====================
// BookKeeper.Api/Database/Configurations/PaymentConfiguration.cs
file class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments", Schemas.Application);
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .HasMaxLength(26)
            .IsRequired();
        
        builder.Property(p => p.CustomerId)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .IsRequired();
        
        builder.Property(p => p.Description)
            .HasMaxLength(500);
        
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(p => p.CreatedAt)
            .IsRequired();
        
        builder.HasIndex(p => p.CustomerId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);
    }
}

// ==================== 使用範例 ====================
/*
 * POST /api/payments
 * Authorization: Bearer {token}
 * Content-Type: application/json
 * 
 * {
 *   "customerId": "C123",
 *   "amount": 99.99,
 *   "currency": "USD",
 *   "description": "Product purchase"
 * }
 * 
 * Response 200 OK:
 * {
 *   "paymentId": "01J9KT5X...",
 *   "status": "Pending",
 *   "createdAt": "2026-01-29T10:30:00Z"
 * }
 */
