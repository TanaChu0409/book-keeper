# BookKeeper - AI Copilot Instructions

> **版本**: v1.0.0 | **最後更新**: 2026-01-06 | **維護者**: AI Infrastructure Team | **專案**: BookKeeper Personal Finance API

---

## 🚀 快速開始 (AI Agent Swarm 工作流)

### 使用方式
本專案已配置 **AI Agent Swarm** 系統。遵循以下步驟進行開發任務：

#### **第 1 步：請求規劃**
```
@dotnet-architect

描述您的需求，例如：
- "為 Expenditure API 新增按日期範圍查詢功能"
- "新增 Budget 實體與 CRUD 操作"
- "優化 GetExpenditure查詢效能"
```

#### **第 2 步：審閱計畫**
Architect 代理會：
- ✅ 讀取 `my-ai-swarm/project-memory.md` 理解約束
- ✅ 分析現有代碼結構與相依性
- ✅ 產出詳細的實裝計畫
- ✅ 提供驗證策略

#### **第 3 步：批准並實裝**
選擇 **"批准計畫並開始實作 (Approve & Implement)"** 按鈕，Developer 代理會：
- ✅ 嚴格遵循批准的計畫
- ✅ 逐步實現每個功能區塊
- ✅ 自動更新 `project-memory.md` 的決策日誌
- ✅ 回報完整的實裝結果

---

## 🏗️ AI Swarm 5 角色工作流程（v2.0）

### 工作流程架構
本系統採用**方案 B**（5 角色 + 3 流程路由）：

| 角色 | 責任 | 觸發條件 |
|------|------|--------|
| **Architect** | 規劃、設計決策 | 所有需求（流程 A/B） |
| **Impact Validator** | 跨系統風險評估 | 高複雜度決策（流程 A 只有） |
| **Developer** | 代碼實裝 | 所有需求（流程 A/B/C） |
| **QA Reviewer** | 代碼品質把關 | 所有實裝（流程 A/B/C） |
| **Memory Manager** | 決策日誌維護 | 所有完成（流程 A/B/C） |

### 三種標準流程

**流程 A（複雜新功能 / 重大重構）**
```
需求 → Architect (設計) → Impact (評估) → Developer (實裝) 
→ QA (審查) → Memory (記錄) → 完成
工時：1-2 週
```

**流程 B（中等優化 / 重構）**
```
需求 → Architect (設計) → Developer (實裝) 
→ QA (審查) → Memory (記錄) → 完成
工時：3-5 天
```

**流程 C（Bug 修復 / 小型優化）**
```
需求 → Developer (實裝) → QA (審查) → Memory (記錄) → 完成
工時：1-2 天
```

### 工作流程文檔

| 文檔 | 用途 |
|------|------|
| [WORKFLOW_ROUTES.md](../my-ai-swarm/procedures/WORKFLOW_ROUTES.md) | 流程判斷樹、角色職責、Handoff 檢查點 |
| [WORKFLOW_CHECKLIST.md](../my-ai-swarm/procedures/WORKFLOW_CHECKLIST.md) | 各角色的前置檢查清單、驗證標準 |
| [WORKFLOW_HANDOFF.md](../my-ai-swarm/procedures/WORKFLOW_HANDOFF.md) | Handoff 交接驗證程序、異常處理 |
| [REQUIREMENT_TEMPLATE.md](../my-ai-swarm/procedures/REQUIREMENT_TEMPLATE.md) | 用戶需求提交模板、自動流程判斷 |
| [FEATURE_TEMPLATE.md](../my-ai-swarm/procedures/FEATURE_TEMPLATE.md) | Vertical Slice 功能模板 |

### 快速啟動

提交需求時，使用 [REQUIREMENT_TEMPLATE.md](../my-ai-swarm/procedures/REQUIREMENT_TEMPLATE.md)，系統會**自動判斷流程（A/B/C）**並路由給正確的角色。

---

## 📚 核心文檔速查

| 文檔 | 用途 | 讀者 |
|---|---|---|
| **此檔案** | Copilot 通用指南 | 所有 AI 代理 |
| **project-memory.md** | 架構決策與程式碼模式 | Architect/Developer |
| **QUICK-REFERENCE.md** | 快速指令與決策樹 | Developer |
| **FEATURE_TEMPLATE.md** | Vertical Slice 功能模板 | Developer |

---

## ⚡ 常見任務快速指令

### 新增查詢功能
```
@dotnet-architect

新增以下查詢功能到 Expenditure:
- 方法名: GetExpendituresByDateRange
- 參數: DateOnly startDate, DateOnly endDate
- 回傳: List<ExpenditureResponse>
```

### 新增完整 CRUD
```
@dotnet-architect

新增完整的 Budget 實體與 CRUD 操作:
- Entity: Budget (Id, Name, Amount, Period, CategoryId)
- 操作: Create, Get, GetAll, Update, Delete
- 驗證: Name 必填、Amount > 0、Period 有效
```

### 新增複雜查詢
```
@dotnet-architect

新增月度支出報表查詢:
- 端點: GET /api/expenditures/monthly-report
- 參數: year, month
- 回傳: 按 Label 分組的支出總額
```

---

## ✅ 技術參考

### Architecture Overview

**BookKeeper** is a .NET 8 Minimal API using **Vertical Slice Architecture**:

- **BookKeeper.Api**: Main API project with all features
  - **Features/**: Vertical slices (one feature = one file with Command/Query/Handler/Validator/Endpoint)
  - **Entities/**: Domain models with Factory Pattern
  - **Contracts/**: Request/Response DTOs organized by feature
  - **Database/**: ApplicationDbContext + EF configurations
  - **Endpoints/**: IEndpoint interface for auto-discovery
  - **Shared/**: Result pattern, errors, common utilities
  - **Extensions/**: Extension methods
  - **Middleware/**: Global exception handler

### Key Design Patterns

**Vertical Slice Architecture**:
- Each feature is self-contained in one file
- Nested classes: Command/Query, Validator, Handler, Endpoint
- No traditional layers (Controller/Service/Repository)

**CQRS with MediatR**:
- Commands for write operations (`IRequest<Result<T>>`)
- Queries for read operations
- Handlers implement `IRequestHandler<TRequest, TResponse>`

**Result Pattern**:
- No exceptions for business logic failures
- Return `Result<T>` from all handlers
- Use `.Match()` in endpoints to handle success/failure

**Entity Factory Pattern**:
- Private constructors for entities
- Static `Create()` methods for instantiation
- `Update()` methods for modifications
- ULID with prefix for IDs (`e_`, `i_`, `l_`)

**Auto-Endpoint Discovery**:
- Implement `IEndpoint` interface
- Automatic registration via reflection
- No manual endpoint mapping in Program.cs

### Dependency Injection

DI is configured in [DependencyInjection.cs](DependencyInjection.cs):

```csharp
// MediatR - auto-discovers all handlers in assembly
services.AddMediatR(Assembly.GetExecutingAssembly());

// FluentValidation - auto-discovers all validators
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Endpoints - auto-discovers all IEndpoint implementations
services.AddEndpoints(Assembly.GetExecutingAssembly());
```

When adding new features:
1. Follow naming convention and nested class structure
2. No manual registration needed—auto-discovery handles it
3. Use constructor injection with required parameters

### Request/Response Patterns

All API endpoints follow this pattern:

1. **Request**: Defined in `Contracts/{Feature}/` with `init` properties
2. **Response**: Record types with `required` properties
3. **Command/Query**: Implements `IRequest<Result<T>>`
4. **Validator**: Inherits `AbstractValidator<TCommand>`
5. **Handler**: Implements `IRequestHandler<TCommand, Result<T>>`
6. **Endpoint**: Implements `IEndpoint`, uses `.Match()` for result handling

Example from [CreateExpenditure.cs](BookKeeper/BookKeeper/BookKeeper.Api/Features/Expenditures/CreateExpenditure.cs):
- All classes nested in one static class
- Handler returns `Result<string>` (entity Id)
- Endpoint maps result to appropriate HTTP response

### Database & Configuration

- **Database**: PostgreSQL 17.2
- **ORM**: Entity Framework Core 8.0.21
- **Connection strings**: Read from [appsettings.json](BookKeeper/BookKeeper/BookKeeper.Api/appsettings.json) or environment variables
  - `ConnectionStrings:Database`: Main database connection
- **Naming Convention**: Snake_case (via `EFCore.NamingConventions`)
- **Migrations**: Automatic application in Development environment
- **DbContext**: [ApplicationDbContext.cs](BookKeeper/BookKeeper/BookKeeper.Api/Database/ApplicationDbContext.cs)
  - DbSets: Expenditures, Incomes, Labels
- **Configurations**: Explicit configuration in `Database/Configurations/`

### Key Features

**Labels (Categories)**:
- 7 operations: Create, Get, GetAll, Update, Delete, GetExpenditureLabels, GetIncomeLabels
- Soft delete support (`IsDeleted` flag)
- Linked to Expenditures and Incomes

**Expenditures (Expenses)**:
- 5 operations: Create, Get (by Id), GetAll (paginated), Update, Delete
- Entity Id format: `e_{ULID}`
- Linked to Labels (required)

**Incomes**:
- 5 operations: Create, Get (by Id), GetAll (paginated), Update, Delete
- Entity Id format: `i_{ULID}`
- Linked to Labels (required)

---

## 🎯 Agent Swarm 最佳實踐

### Architect 的檢查清單
在提交計畫前，確保：
- [ ] 讀取並遵守 `project-memory.md` 的所有約束
- [ ] 驗證命名約定是否符合規則（Vertical Slice 結構）
- [ ] 確認所有相依性都已列出
- [ ] 提供明確的步驟序列（讓 Developer 可執行）
- [ ] 包括驗證策略（本地測試/Swagger 驗證）
- [ ] 若任一需求/假設無法確認，彙整「澄清問題」清單並向需求方詢問

### Developer 的執行清單
在開始實作前：
- [ ] 確認收到的計畫是否清晰可行
- [ ] 檢查現有代碼是否有類似實現可參考
- [ ] 使用 [FEATURE_TEMPLATE.md](../my-ai-swarm/procedures/FEATURE_TEMPLATE.md) 作為骨架
- [ ] 逐步完成，每個 Phase 驗證一次
- [ ] 更新 `project-memory.md` 的決策日誌（若有新決策）
- [ ] 提供完整的實裝成果報告

### 記憶與決策管理
每次重大決策後：
1. 記錄到 `project-memory.md` 的決策日誌
2. 格式: `| YYYY-MM-DD | #ID | 決策內容 | 理由 | 影響範圍 |`
3. 範例: `| 2026-01-06 | #009 | 新增 Budget 實體 | 支援預算管理功能 | 新增 Entity + 5 端點 |`

---

## 📊 命名約定速查表

| 元件 | 命名模式 | 位置 | 示例 |
|---|---|---|---|
| **Feature 檔案** | `{Action}{Domain}.cs` | `Features/{Domain}/` | `CreateExpenditure.cs` |
| **Command/Query** | `Command` / `Query` | Feature 檔案內（巢狀類別） | `CreateExpenditure.Command` |
| **Handler** | `Handler` | Feature 檔案內（巢狀 sealed class） | `CreateExpenditure.Handler` |
| **Validator** | `Validator` | Feature 檔案內（巢狀類別） | `CreateExpenditure.Validator` |
| **Endpoint** | `{Action}{Domain}Endpoint` | Feature 檔案內 | `CreateExpenditureEndpoint` |
| **Entity** | `{Domain}` | `Entities/` | `Expenditure`, `Income`, `Label` |
| **Entity ID** | `{prefix}_{ULID}` | Entity 內生成 | `e_01J9KT...`, `i_01J9KT...`, `l_01J9KT...` |
| **Request Contract** | `{Action}{Domain}Request` | `Contracts/{Domain}/` | `CreateExpenditureRequest` |
| **Response Contract** | `{Domain}Response` | `Contracts/{Domain}/` | `ExpenditureResponse` |
| **EF Configuration** | `{Entity}Configuration` | `Database/Configurations/` | `ExpenditureConfiguration` |
| **Error Class** | `{Domain}Errors` | `Shared/Errors/` | `LabelErrors`, `ExpenditureErrors` |

---

## 🔧 常見修改步驟

### 添加新 Feature 的完整流程

**參照**: [project-memory.md § 新增 Feature 完整檢查清單](../my-ai-swarm/project-memory.md#新增-feature-完整檢查清單)

**Phase 1: 規劃**
- 確認需求類型（CRUD / 複雜查詢 / 業務邏輯）
- 決定是否需要新 Entity
- 設計 API 路徑與 HTTP 方法

**Phase 2: Entity 與資料庫**（若需新 Entity）
```bash
# 1. 建立 Entity
# File: Entities/{Domain}.cs
# - 私有建構函式
# - 靜態 Create() 方法
# - ULID Id 生成

# 2. 建立 EF Configuration
# File: Database/Configurations/{Domain}Configuration.cs

# 3. 更新 DbContext
# Edit: ApplicationDbContext.cs - 新增 DbSet

# 4. 建立 Migration
dotnet ef migrations add Add{Domain} -p BookKeeper/BookKeeper/BookKeeper.Api

# 5. 檢查並套用 Migration
dotnet ef database update -p BookKeeper/BookKeeper/BookKeeper.Api
```

**Phase 3: Contracts**
```csharp
// File: Contracts/{Domain}/{Action}{Domain}Request.cs
public sealed record CreateExpenditureRequest
{
    public string PaymentName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    // ...
}

// File: Contracts/{Domain}/{Domain}Response.cs
public sealed record ExpenditureResponse
{
    public required string Id { get; init; }
    public required string PaymentName { get; init; }
    // ...
}
```

**Phase 4: Feature 實現**
```csharp
// File: Features/{Domain}/{Action}{Domain}.cs
// 使用 FEATURE_TEMPLATE.md 作為骨架
// 包含：Command/Query, Validator, Handler, Endpoint
```

**Phase 5: 測試**
```bash
# 啟動應用程式
dotnet run --project BookKeeper/BookKeeper/BookKeeper.Api

# 或使用 Docker
cd BookKeeper/BookKeeper
docker-compose up --build

# 訪問 Swagger
# http://localhost:9000/swagger
```

---

## ⚠️ 禁止操作與陷阱

| ❌ 禁止 | ✅ 正確做法 |
|---|---|
| 在 Handler 拋出 Exception | 回傳 `Result.Failure<T>(error)` |
| 公開 Entity 建構函式 | 私有建構函式 + `Create()` |
| 手動註冊端點 | 實現 `IEndpoint` 自動掃描 |
| 跳過 FluentValidation | 每個 Command 有 `Validator` |
| 使用 GUID 作為 Id | 使用 `Ulid` + 前綴 |
| 分散 Feature 類別 | 所有類別巢狀在一個檔案 |
| 使用 Layered Architecture | 使用 Vertical Slice |
| DateTime.Now | `DateTime.UtcNow` |
| 硬編碼連線字串 | 使用 `appsettings.json` |

---

## 📞 緊急協助指令

如果遇到問題，使用以下命令啟動專家協助：

```
@dotnet-architect

[診斷請求]
錯誤類型: [Build Error / Logic Bug / Performance Issue]
錯誤訊息: [詳細錯誤]
相關檔案: [受影響的檔案]
上下文: [發生的情況]
```

Architect 會進行根本原因分析並提出解決方案。

---

## 🚦 Build / Run

- 本服務為 ASP.NET Core Minimal API，使用 Kestrel。
- Swagger 在所有環境啟用（開發階段）。
- 連線字串從 appsettings.json 或環境變數讀取。
- 關鍵環境變數：`ASPNETCORE_ENVIRONMENT`（預設 Development）。
- 最小步驟：

```bash
# 本地啟動
cd BookKeeper/BookKeeper
dotnet restore
dotnet build
dotnet run --project BookKeeper.Api

# Docker 啟動（推薦）
docker-compose up --build
```

- 啟動後 Swagger：http://localhost:9000/swagger
- Aspire Dashboard：http://localhost:18888
- 必備前置：
  - PostgreSQL 可用（Docker Compose 自動啟動）
  - 連線字串正確（見 appsettings.Development.json）

---

## 🧪 Testing

- 目前無測試專案（待建立）
- 建議測試策略：
  - Unit Tests: Handler 業務邏輯
  - Integration Tests: 端點完整流程
  - Database Tests: EF Configuration 驗證

---

## 🛠️ Tool Compatibility

- Cursor 規則已統一工具清單：`search`, `read`, `edit`, `agent`。
- AI 代理協作：先由 Architect 規劃與審核，再交由 Developer 依步驟實作。
- 重大決策記錄於 my-ai-swarm/project-memory.md。

---

## 📖 相關連結

- [.NET 8 文檔](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [MediatR](https://github.com/jbogard/MediatR)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [ULID Specification](https://github.com/ulid/spec)
- [Vertical Slice Architecture](https://jimmybogard.com/vertical-slice-architecture/)

---

**最後更新**: 2026-01-06 by @dotnet-architect
