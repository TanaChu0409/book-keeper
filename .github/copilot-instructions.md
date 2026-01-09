# iPASS TWQR - AI Copilot Instructions

> **版本**: v1.4.1 | **最後更新**: 2026-01-08 | **維護者**: AI Infrastructure Team | **狀態**: ✅ VS Code 環境已優化

---

## 🚀 快速開始 (GitHub Copilot 工作流)

### 使用方式
本專案使用 **GitHub Copilot (Claude Sonnet 4.5)** 配合 **Subagent 系統**。遵循以下步驟進行開發任務：

#### **第 1 步：規劃任務**
直接向 GitHub Copilot 描述需求，Copilot 會自動判斷是否需要啟動 Subagent：

```
請幫我規劃以下功能：
- 為 ClientApp API 新增批量用戶導入功能
- 重構 OffLineService 以支持非同步操作
- 優化 FISC 支付流程的加密邏輯
```

#### **第 2 步：Copilot 自動分析**
GitHub Copilot 會：
- ✅ 自動讀取 `my-ai-swarm/project-memory.md` 理解約束
- ✅ 使用 `semantic_search` 分析現有代碼結構
- ✅ 判斷任務複雜度，決定是否啟動 Subagent
- ✅ 產出詳細的實裝計畫或直接實作

#### **第 3 步：執行實作**
GitHub Copilot 會：
- ✅ 依照計畫逐步實現功能
- ✅ 自動更新 `project-memory.md` 的決策日誌
- ✅ 使用現有的 Subagent（Developer / Plan / Beast Mode）處理複雜任務
- ✅ 回報完整的實裝結果

---

## 🤖 GitHub Copilot 工作流程與 Subagent 系統

### 工作流程架構
GitHub Copilot 會根據任務複雜度**自動決定**工作流程：

| 任務類型 | 處理方式 | Copilot 行為 |
|---------|---------|-------------|
| **簡單修改/Bug 修復** | 直接處理 | Copilot 直接實作，自動更新決策日誌 |
| **中等複雜功能** | 規劃 + 實作 | Copilot 先規劃，取得確認後實作 |
| **高複雜度任務** | 啟動 Subagent | 啟動 `Developer` 或 `Plan` Subagent 自主處理 |
| **多步驟研究** | 啟動 Subagent | 啟動 `Plan` Subagent 進行深度分析 |
| **決策記錄** | 自動維護 | Copilot 自動更新 `project-memory.md` |

### 可用的 Subagent

本專案配置了以下 Subagent，Copilot 會在需要時自動調用：

#### 內建 Subagent（VS Code 原生支持）
| Subagent | 用途 | 何時啟動 |
|----------|------|----------|
| **Developer** | 完整實作能力，嚴格執行規劃 | 複雜的多文件修改、需要自主決策的實作 |
| **Plan** | 研究與多步驟規劃 | 需要深度研究、不確定的技術方案 |
| **Beast Mode** | 強化版 Copilot | 用戶明確要求或極複雜任務 |

#### 延伸角色 Agent（`.github/agents/` 中的配置）
| Agent | 角色 | 用途 | 調用時機 |
|------|------|------|---------|
| **Architect** | 架構師 | 設計與規劃、分析代碼結構、提出實裝計畫 | 新功能、重大重構前 |
| **Impact Validator** | 風險評估師 | 評估跨系統影響、識別風險、擬定應對策略 | 高複雜度/高風險變更 |
| **Developer** | 開發工程師 | 嚴格執行規劃、實裝代碼、本地測試 | Architect 計畫批准後 |
| **QA Reviewer** | 品質審查員 | 代碼品質檢查、設計合規驗證、測試覆蓋審查 | Developer 實裝完成後 |
| **Memory Manager** | 記憶管理員 | 決策日誌記錄、版本維護、跨檔案同期 | QA 批准完成後 |

**使用方式**：
- **自動調用**（推薦）：Copilot 根據工作流程自動判斷並調用適當的 Agent
- **手動調用**（高級）：在對話中明確指定 Agent 名稱，例如 "@Architect" 或 "啟動 Impact Validator"

#### 工作流程示例

**簡單 Bug 修復**（流程 C）：
```
User: 支付功能返回 500 錯誤
Copilot: 自動分析 → Developer 直接修復 → QA 快速審查 → Memory 記錄
```

**新功能開發**（流程 A）：
```
User: 新增批量用戶導入功能
Copilot: 自動分析 → Architect 設計 → Impact Validator 評估風險 
  → Developer 實裝 → QA 審查 → Memory 記錄決策
```

### 工作流程文檔

| 文檔 | 用途 |
|------|------|
| [WORKFLOW_ROUTES.md](../my-ai-swarm/procedures/WORKFLOW_ROUTES.md) | 流程判斷樹、角色職責、Handoff 檢查點 |
| [WORKFLOW_CHECKLIST.md](../my-ai-swarm/procedures/WORKFLOW_CHECKLIST.md) | 各角色的前置檢查清單、驗證標準 |
| [WORKFLOW_HANDOFF.md](../my-ai-swarm/procedures/WORKFLOW_HANDOFF.md) | Handoff 交接驗證程序、異常處理 |
| [REQUIREMENT_TEMPLATE.md](../my-ai-swarm/procedures/REQUIREMENT_TEMPLATE.md) | 用戶需求提交模板、自動流程判斷 |

### 快速啟動

提交需求時，使用 [REQUIREMENT_TEMPLATE.md](../my-ai-swarm/procedures/REQUIREMENT_TEMPLATE.md)，系統會**自動判斷流程（A/B/C）**並路由給正確的角色。

---

## 🔄 自動工作流程 (Automatic Workflows)

### 規格書 PDF 自動轉換流程

**目的**：每次執行 AI 前，自動檢查規格書資料夾中的 PDF 文件，將新文件轉換為 Markdown 並存入記憶區。

**執行步驟**：

```
前置檢查：
0. 檢查 c:\IPASS\iPASS_TWQR\規格書\ 資料夾是否存在：
  ├─ 若不存在 → 詢問使用者是否建立；
  ├─ 若使用者同意 → 建立資料夾後繼續；
  ├─ 若使用者拒絕 → 記錄「跳過規格書掃描」狀態，後續執行均跳過此流程，直到使用者重新要求。
1. 驗證 pymupdf4llm 是否已安裝（位置：Python packages）
   ├─ 若未安裝 → 執行 pip install pymupdf4llm
   └─ 若已安裝 → 繼續

PDF 掃描與轉換：
2. 檢查 c:\IPASS\iPASS_TWQR\規格書\ 資料夾所有 *.pdf 文件
3. 比對 my-ai-swarm/specifications/ 已有的 .md 文件
4. 識別新文件（未轉換過的 PDF）

轉換與分析：
5. 執行 pymupdf4llm.to_markdown() 轉換新 PDF → .md
   ├─ 轉換結果存放至 my-ai-swarm/specifications/
   └─ 命名規則：{原PDF名稱}.md（保持中文原名）

6. 根據內容類型產出分析文檔：
   ├─ 若為規格書 → SPECIFICATION_ANALYSIS_REPORT.md（更新）
   ├─ 若為資料庫架構 → DATABASE_SCHEMA_ANALYSIS.md（更新）
   ├─ 若為交易類型 → TWQRREQUEST_WIKI_ANALYSIS.md（更新）
   └─ 若為新類型 → 建立新分析文檔

決策記錄：
7. 更新 project-memory.md：
   ├─ 新增決策日誌行：| YYYY-MM-DD | #[ID] | 新規格書轉換 {PDF名稱} | 自動工作流程 |
   └─ 同步更新版本號 v[current+0.0.1]

檔案整理與歸檔：
8. 將 規格書/ 內所有 .md 檔（若有）搬移到 my-ai-swarm/specifications/，確保 規格書/ 內僅保留原始 .pdf 檔
9. 若轉換產生圖片資產，請以 {PDF名稱} 建立目錄搬移至 my-ai-swarm/specifications/assets/{PDF名稱}/

清理臨時檔案：
10. 刪除轉換過程中的臨時檔案（若有）
```

**啟動時機**：
- ✅ 每次 Copilot 執行前（自動）
- ✅ 用戶明確要求「檢查新規格」時
- ✅ 發現新 PDF 文件時
- ⚠️ 若先前使用者明確拒絕建立/掃描規格書資料夾，則跳過此流程，直到使用者重新要求。

**相關檔案**：
- 源資料夾：`規格書/`（僅保留原始 PDF，不保留 .md）
- 轉換結果：`my-ai-swarm/specifications/`（Markdown 歸檔與分析資產）
- 分析報告：`my-ai-swarm/specifications/SPECIFICATION_ANALYSIS_REPORT.md` 等
- 決策記錄：`my-ai-swarm/project-memory.md` （決策日誌表）

**異常處理**：
| 問題 | 解決方案 |
|------|--------|
| pymupdf4llm 安裝失敗 | 顯示錯誤信息，手動執行 pip install 或通知用戶 |
| PDF 轉換失敗 | 記錄失敗文件名，提示人工檢查；不中斷流程 |
| 分析文檔更新衝突 | 附加新內容至文檔末尾，記錄更新時戳 |

---

### 專案理解與記憶維護流程

**目的**：確保每次 AI 開始處理專案前，已有有效的專案理解與記憶；每次變更後，同步更新記憶與 README。

**前置檢查**：
0. 檢查 `my-ai-swarm/` 資料夾是否存在：
  ├─ 若不存在 → 自動建立 `my-ai-swarm/` 與子資料夾 `specifications/`、`decisions/`、`procedures/`（若適用），並初始化必要的筆記檔（如空的 project-memory.md）。
  ├─ 若使用者明確拒絕建立 → 記錄「跳過 my-ai-swarm 初始化」狀態，後續執行均跳過此流程，直到使用者重新要求。
1. 檢查 `my-ai-swarm/` 是否已有專案理解/研究文件（例如 project-memory.md 內的專案概覽、近期分析報告）。
2. 若不存在或明顯過時 → 先對專案執行語意搜尋/掃描主要目錄，建立「專案理解重點筆記」，再開始任何開發/分析。

**執行步驟**：
1. 若需建立理解筆記：
  - 掃描主要程式碼目錄（Controllers/Service/Model/DAL/Utility）。
  - 在 `my-ai-swarm/`（建議 `my-ai-swarm/project-memory.md` 或同層筆記）新增/更新專案概覽與關鍵模組摘要。
2. 變更完成後：
  - 更新 `my-ai-swarm/project-memory.md` 的決策/理解筆記，紀錄影響範圍與重要設計要點。
  - 檢查 [README.md](../README.md) 是否需同步更新（新增/移除/調整端點、流程、設定等）。如需更新，先修 README 再結束任務。

**角色責任**：
- Architect：確保開工前有完整專案理解筆記；發現缺失時先補筆記再開發。
- Memory Manager：在每次變更後同步更新 `project-memory.md`（或相關筆記）並觸發 README 是否需更新的檢查。
- Developer：若偵測到 `my-ai-swarm/` 被使用者要求跳過初始化，須於開始實作前詢問是否恢復；若已存在則直接依筆記工作。

**啟動時機**：
- 每次 Copilot 開始處理專案時（自動檢查）。
- 每次完成代碼/文檔變更後（自動要求更新記憶與 README 評估）。
- ⚠️ 若使用者曾明確拒絕初始化 `my-ai-swarm/`，則跳過此初始化，直到使用者重新要求。


## 📚 核心文檔速查

| 文檔 | 用途 | 說明 |
|---|---|---|
| **此檔案** | Copilot 通用指南 | GitHub Copilot 的主要配置文件 |
| **project-memory.md** | 長期決策與架構約束 | 記錄所有重要決策與架構規則 |
| **WORKFLOW_ROUTES.md** | 工作流程指引 | 任務分類與處理流程參考 |
| **WORKFLOW_CHECKLIST.md** | 檢查清單 | 代碼審查與品質標準 |
| **REQUIREMENT_TEMPLATE.md** | 需求模板 | 提交需求時的標準格式 |

---

## 📦 可攜導入指引（將本資料夾帶到其他專案）

若要將本 AI 代理配置移植到其他專案：
- 複製 `.github/` 目錄（含 copilot-instructions.md 與 agents/）到目標專案根目錄。
- 首次執行時：
  - 系統會自動檢查並建立 `my-ai-swarm/` 及子資料夾（specifications/、decisions/、procedures/）與 `project-memory.md`。
  - 若目標專案有規格書資料夾（預設 `規格書/`），會自動詢問是否啟用 PDF 轉換並建立資料夾；拒絕則記錄跳過狀態。
- 如需關閉自動規格書掃描或 my-ai-swarm 初始化，可在提示時選擇拒絕，系統會記錄跳過狀態，直到再次要求。
- 建議同步目標專案的 README：按本檔「專案理解與記憶維護流程」在完成變更後更新。

---

## ⚡ 常見任務快速指令

### 新增 API 端點
```
請幫我新增以下功能到 ClientApp API:
- 端點: POST /api/ClientApp/BatchImport
- 功能: 批量導入用戶資料
- 模型: 參考 ClientAppService.cs 的模式
```

### 擴展既有服務
```
請在 PaymentService 中新增以下方法:
- 名稱: ProcessBulkPayment
- 參數: List<PaymentRequest>
- 回傳: BulkPaymentResponse
```

### 資料庫操作
```
請在 TWQR.DAL 中新增以下資料存取邏輯:
- 方法: GetMerchantTransactionStats
- 資料表: special_store, LifePaymentRecord
- 索引優化: 考慮 merchant_id + date_range
```

---

## ✅ 技術參考

### Architecture Overview

**iPASS TWQR** is a .NET 8 enterprise QR payment system using **layered architecture**:

- **IPASS.TWQR**: Main Web API (ASP.NET Core) with Controllers
- **IPASS.TWQR.DAL**: Data Access Layer with Entity Framework Core
  - `DataController` classes (suffix `DC`) handle database operations via stored procedures and direct queries
  - `IPassPayContext`: EF Core DbContext auto-generated from SQL Server via EF Power Tools
- **IPASS.TWQR.Model**: Domain models, request/response DTOs, enums, constants
  - Organized by feature: `Client/`, `FISC/`, `Hivex/`, `Payment/`, `Voucher/`, `OffLine/`, `TaiwanTaxi/`, `CTBCGateway/`
  - `BaseRequest`/`BaseResponse`: Standard API contracts with `sessionToken` and `rtnCode`/`rtnMsg`
- **IPASS.TWQR.Service**: Business logic services
  - Interfaces in `Service/Interface/`; implementations as `*Service` classes
  - DI auto-discovery via `ServiceConfig.cs` convention: `I{ClassName}` interfaces paired with concrete implementations
- **IPASS.Library** & **IPASS.Utility**: Shared libraries across projects

## Dependency Injection & Service Registration

DI is configured in [SystemComponentExtensions.cs](SystemComponentExtensions.cs) and [ServiceConfig.cs](App_Start/ServiceConfig.cs):

```csharp
// DAL services: Classes ending in "DC" in IPASS.TWQR.DAL.DataController namespace
// Auto-discovered and registered as scoped services

// Domain services: Classes ending in "Service" in IPASS.TWQR.Service namespace  
// Auto-discovered and registered as scoped services

// Keyed services: Merchant check & voucher services use AddKeyedScoped
services.AddKeyedScoped<IMerchantCheckSubService, InboundFiscService>("Inbound");
```

When adding new services:
1. Follow naming convention: `IMyService` interface + `MyService` implementation
2. Place in appropriate namespace: `IPASS.TWQR.Service.*`
3. No manual registration needed—auto-discovery handles it
4. Use constructor injection with required parameters

## Request/Response Patterns

All API endpoints follow this pattern:

1. **Request**: Inherit from `BaseRequest` (includes `sessionToken`)
2. **Response**: Inherit from `BaseResponse` (includes `rtnCode`, `rtnMsg`)
3. **JSON serialization**: Use `System.Text.Json` with custom `CustomDateTimeConverter` for `"yyyy-MM-dd HH:mm:ss"` format
4. **Logging**: Call `bc.SaveLog()` in services; `bc.Insert()` / `bc.Update()` log API transactions automatically via [BaseController](Controllers/BaseController.cs)

Example from [ClientAppService.cs](Service/ClientAppService.cs):
- Request/response inherit from `BaseRequest`/`BaseResponse`
- Services receive `BaseController bc` parameter for logging and transaction tracking
- `ReqId` (Guid) tracks request lifecycle across system

## Database & Configuration

- **Connection strings**: Read from registry (Windows) or fallback to [appsettings.json](appsettings.json)
  - `IPASS_DBConnection`: Main database
  - Registry path: `SOFTWARE\iPASS\iPASS_TWQR` (HKLM)
- **EF Core**: Uses SQL Server with Power Tools auto-generation
  - DbContext: [IPassPayContext.cs](../IPASS.TWQR.DAL/IPassPayContext.cs)
  - Stored procedures accessed via context methods in `IPassPayContextProcedures.cs`
- **Configuration**: Load via [BaseSettings.cs](BaseSettings.cs) and [DataBaseConfig.cs](App_Start/DataBaseConfig.cs)
- **Environment-specific settings**: appsettings.{Environment}.json files (Development, Staging, RC, Production, DR)

## Key Integrations & External Services

### FISC (Financial Information Service Center)
- Handler: [FISCService.cs](Service/FISCService.cs), [FiscPurchaseController.cs](Controllers/FiscPurchaseController.cs)
- Abstract base: [FiscFoundationBase.cs](Service/Abstractions/FiscFoundationBase.cs)
- Encryption/Decryption: AES encryption + HMAC signature via [IFISCApiService](Service/Interface/IFISCApiService.cs)
- Request format: Inherits from FISC specs; response wrapped in `FiscDataReturn` with signed/encrypted body

### Hivex (Cross-Border Payment)
- Handler: [HivexService.cs](Service/HivexService.cs), [HivexApiService.cs](Service/HivexApiService.cs)
- HMAC-based request signing
- Response: `HivexResponseBase<T>` with HTTP status code + headers + data

### Offline Transactions
- Handler: [OffLineService.cs](Service/OffLineService.cs), [OffLineBase.cs](Service/OffLine/OffLineBase.cs)
- Supports Payment, Capture, Void, Refund operations
- Data stored in `fault_payment_record` table for error tracking

### Taiwan Taxi Integration
- Handler: [TaiwanTaxiService.cs](Service/TaiwanTaxiService.cs)
- Uses company ID, API key, and signature-based authentication

### Voucher/Coupons
- Handler: [VoucherService.cs](Service/VoucherService.cs)
- FISC-integrated via [VoucherFiscService.cs](Service/Vouchers/VoucherFiscService.cs)

---

## 🎯 Agent Swarm 最佳實踐

### Architect 的檢查清單
在提交計畫前，確保：
- [ ] 讀取並遵守 `project-memory.md` 的所有約束
- [ ] 驗證命名約定是否符合規則（Service/DC/Controller 末尾）
- [ ] 確認所有相依性都已列出
- [ ] 提供明確的步驟序列（讓 Developer 可盲目執行）
- [ ] 包括驗證策略（單元測試/集成測試）
- [ ] 若任一需求/假設/影響無法確認，彙整「澄清問題」清單並向需求方詢問；取得回覆或明確同意的假設後再提交計畫/交接

### Developer 的執行清單
在開始實作前：
- [ ] 確認收到的計畫是否清晰可行
- [ ] 檢查現有代碼是否有類似實現可參考
- [ ] 逐步完成，每個概念區塊提交一次
- [ ] 更新 `project-memory.md` 的決策日誌
- [ ] 提供完整的實裝成果報告

### 記憶與決策管理
每次重大決策後：
1. 記錄到 `project-memory.md` 的決策日誌
2. 格式: `| YYYY-MM-DD | #ID | 決策內容 | 理由 |`
3. 範例: `| 2025-12-29 | #003 | 新增 AsyncBatchImport 方法 | 改善大量導入效能 |`

---

## 📊 命名約定速查表

| 元件 | 命名模式 | 位置 | 示例 |
|---|---|---|---|
| **Service 介面** | `I{FeatureName}Service` | `Service/Interface/` | `IPaymentService` |
| **Service 實現** | `{FeatureName}Service` | `Service/` | `PaymentService` |
| **DAL 控制器** | `{FeatureName}DC` | `DAL/DataController/` | `PaymentDC` |
| **Web 控制器** | `{FeatureName}Controller` | `Controllers/` | `PaymentController` |
| **請求模型** | `{Action}Request` | `Model/{Feature}/Request/` | `PaymentRequest` |
| **響應模型** | `{Action}Response` | `Model/{Feature}/Response/` | `PaymentResponse` |
| **錯誤碼** | `{Feature}_{ErrorType}` | `Model/Domain/` | `Payment_InvalidAmount` |

---

## 🔧 常見修改步驟

### 添加新 API 端點的完整流程

**1. 模型層** (`IPASS.TWQR.Model/{Feature}/`)
```csharp
// Request/Response 模型，繼承基底類別
public class BatchImportRequest : BaseRequest
{
    public List<ImportItem> Items { get; set; }
}

public class BatchImportResponse : BaseResponse
{
    public int SuccessCount { get; set; }
    public List<ErrorDetail> Errors { get; set; }
}
```

**2. 服務層** (`IPASS.TWQR.Service/`)
```csharp
// 介面 (Service/Interface/IClientAppService.cs)
public interface IClientAppService
{
    BatchImportResponse BatchImport(BatchImportRequest req, BaseController bc);
}

// 實現 (Service/ClientAppService.cs)
public class ClientAppService : IClientAppService
{
    public BatchImportResponse BatchImport(BatchImportRequest req, BaseController bc)
    {
        bc.SaveLog(IpassLogLevel.Info, "BatchImport", "Processing batch import");
        // 業務邏輯...
        return response;
    }
}
```

**3. 控制器層** (`IPASS.TWQR.Controllers/`)
```csharp
[ApiController]
[Route("api/ClientApp")]
public class ClientAppController(IClientAppService service, ICommonDC commonDC)
    : BaseController(commonDC)
{
    [HttpPost("BatchImport")]
    public IActionResult BatchImport([FromBody] JsonElement request)
    {
        var response = service.BatchImport(request, this);
        return Ok(response);
    }
}
```

**4. DI 自動註冊**
- ✅ 無需修改 `ServiceConfig.cs`
- ✅ 遵循命名末尾規則自動探索
- ✅ `IClientAppService` + `ClientAppService` 自動配對

---

## ⚠️ 禁止操作與陷阱

| ❌ 禁止 | ✅ 正確做法 |
|---|---|
| 手動修改 `IPassPayContext.cs` | 用 EF Power Tools 重新生成 |
| 在 `ServiceConfig.cs` 手動註冊 | 遵循命名規則，讓自動探索處理 |
| 使用 `Newtonsoft.Json` | 統一使用 `System.Text.Json` |
| 硬編碼連線字串 | 從 Registry 或 appsettings.json 讀取 |
| 忽略 `ReqId` 追蹤 | 在所有 Service 調用中傳遞 `BaseController bc` |
| 直接拋擲例外 | 使用 `iPASSException(code, message)` |

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

- 本服務為 ASP.NET Core Web API，使用 Kestrel；Swagger 僅在 Development/Staging 啟用。
- 連線字串優先從 Windows Registry 讀取，失敗時回退 appsettings.json。
- 關鍵環境變數：`ASPNETCORE_ENVIRONMENT`（預設 Development）。
- 最小步驟：

```bash
dotnet restore
dotnet build --configuration Release
dotnet run --project IPASS.TWQR
```

- 啟動後 Swagger（僅 Dev/Staging）：https://localhost:{port}/swagger
- 必備前置：
  - SQL Server 可用且具備 iPASS 資料庫（連線字串見 IPASS.TWQR/appsettings.Development.json）。
  - 若在非 Windows 或 Production/RC 執行，建議於 appsettings.{Environment}.json 提供有效連線字串。

---

## 🧪 Testing

- 測試框架：MSTest（專案：IPASS.TWQR.Tests）。
- 基本指令：

```bash
dotnet test
```

- 篩選測試：

```bash
dotnet test --filter TestCategory=Unit
```

- 產出覆蓋率（coverlet.collector）：

```bash
dotnet test --collect:"XPlat Code Coverage"
```

- 注意：部分服務倚賴資料庫或 Redis，單元測試請以介面 Mock 方式注入；整合測試前請先備妥測試環境設定。

---

## 🛠️ Tool Compatibility

- Cursor 規則已統一工具清單：`search`, `read`, `edit`, `agent`。
- 已移除過時名稱（如 runSubagent、read_file）。
- 參考：.cursor/rules/dotnet-architect.prompt.md、.cursor/rules/dotnet-developer.prompt.md。
- AI 代理協作：先由 Architect 規劃與審核，再交由 Developer 依步驟實作；重大決策記錄於 my-ai-swarm/project-memory.md。

---

## 🆕 New Services（近期新增）

- PaymentQRGeneratorService（Service/Generators）：產生 TWQR 規格 QR Code 與 BarCode（含 OTP）。
- IPaymentQRGeneratorService（Service/Interface）：產碼用介面與結果模型。
- PaymentQRCodeService（Service/）：封裝取得付款 QRCode 流程，含 Redis 寫入 TTL 管控與數位券處理。
- IPaymentQRCodeService（Service/Interface）：對應服務介面。
- VoucherFiscService / IVoucherFiscService（Service/Vouchers）：數位券 MerchantCheck 流程（查重→圈存），並以 Keyed Scoped 參與合成流程。
- DI：命名以 Service 結尾自動註冊；VoucherFiscService 另以 KeyedScoped 註冊。
- 相依性：資料庫（EF Core/SQL Server）；PaymentQRCodeService 透過 IRedisService 操作 Redis。

## 🆕 近期決策摘要

### 決策 #039：OffLineService 文件化完成（2026-01-06）
離線卡片交易編排服務，1434 行代碼，7 個公開 API，支援 Payment/Capture/Void/Refund 操作。詳見 [decisions/architecture/offline-service-analysis.md](../../my-ai-swarm/decisions/architecture/offline-service-analysis.md)。

### 決策 #040：HivexService 文件化 + 5 層拆分計畫確立（2026-01-06）
跨境支付異步編排服務，3336 行代碼，16 個公開方法，Redis+DB 雙層狀態機。提出 5 子服務拆分計畫（預估 3-4 週工期）。詳見 [decisions/architecture/hivex-service-split-plan.md](../../my-ai-swarm/decisions/architecture/hivex-service-split-plan.md)。

## 📖 相關連結

- [.NET 8 文檔](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/)
- [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [NLog 日誌框架](https://nlog-project.org/)

---

**最後更新**: 2026-01-08 by @memory-manager