# BookKeeper - Service Memory (服務清單)

> **版本**: v1.3.0 | **最後更新**: 2026-01-28 | **用途**: 記錄所有 Features、Endpoints、Handlers 映射關係

---

## 📋 概述

此文檔記錄 BookKeeper 專案中所有的 **Features**（功能切片）、**API Endpoints**（端點）、**MediatR Handlers**（處理器）與 **Entities**（實體）的映射關係，方便快速查找與維護。

---

## 🏗️ 實體 (Entities) 清單

| 實體 | ID 前綴 | 檔案位置 | Schema | 用途 |
|------|---------|---------|--------|------|
| **Label** | `l_` | [Entities/Label.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Entities/Label.cs) | `application` | 收入/支出分類標籤 |
| **Income** | `i_` | [Entities/Income.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Entities/Income.cs) | `application` | 收入記錄 |
| **Expenditure** | `e_` | [Entities/Expenditure.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Entities/Expenditure.cs) | `application` | 支出記錄 |
| **StatisticOfDate** | `sod_` | [Entities/StatisticOfDate.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Entities/StatisticOfDate.cs) | `application` | 每日統計（每用戶每日的總收支） |
| **StatisticOfMonth** | `som_` | [Entities/StatisticOfMonth.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Entities/StatisticOfMonth.cs) | `application` | 每月統計（每用戶每月的總收支） |
| **User** | - | [Entities/User.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Entities/User.cs) | `identity` | ASP.NET Identity 用戶 |
| **RefreshToken** | - | [Entities/RefreshToken.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Entities/RefreshToken.cs) | `identity` | JWT Refresh Token |

### 實體關聯圖

```
┌──────────────┐          ┌──────────────┐
│    Label     │◄─────────│   Income     │
│  (分類標籤)   │  1    *   │   (收入)     │
└──────────────┘          └──────────────┘
       ▲
       │ 1
       │
       │ *
┌──────────────┐
│ Expenditure  │
│   (支出)     │
└──────────────┘
```

---

## 📂 Auth (認證)

### Feature 清單

| Feature | HTTP | 端點 | Handler | 用途 |
|---------|------|------|---------|------|
| **RegisterUser** | POST | `/api/auth/register` | `RegisterUser.Handler` | 建立 Identity + Domain 用戶、指派 Member 角色、產生 access/refresh token |
| **LoginUser** | POST | `/api/auth/login` | `LoginUser.Handler` | 驗證帳密、產生新的 access/refresh token（單一 active refresh） |
| **RefreshAccessToken** | POST | `/api/auth/refresh` | `RefreshAccessToken.Handler` | 驗證 refresh token、過期即刪除、嚴格輪替僅保留一組刷新憑證 |

**備註**:
- Refresh Token 儲存於 `identity` schema 的 `RefreshTokens`，同一用戶只保留一筆有效記錄（新簽發會清除舊 token）。
- 預設角色指派為 Member；密碼策略沿用 Identity 預設設定。

---

## 📂 Users (用戶管理)

### Feature 清單

| Feature | HTTP | 端點 | Handler | 用途 |
|---------|------|------|---------|------|
| **GetCurrentUser** | GET | `/api/users/me` | `GetCurrentUser.Handler` | 取得當前登入用戶基本資料 |
| **GetUserById** | GET | `/api/users/{id}` | `GetUserById.Handler` | Admin 查詢指定用戶資料 |

**備註**:
- `GetUserById` 需 Admin 角色授權；`GetCurrentUser` 需登入。
- 回傳欄位: id、email、name、createdAtUtc、updatedAtUtc。

---

## 📂 Labels (標籤管理) - 完整 CRUD

### Feature 清單

| Feature | HTTP | 端點 | Handler | 用途 |
|---------|------|------|---------|------|
| **GetLabels** | GET | `/api/labels` | `GetLabels.Handler` | 分頁查詢所有標籤 |
| **GetLabel** | GET | `/api/labels/{id}` | `GetLabel.Handler` | 查詢單個標籤 |
| **GetIncomeLabels** | GET | `/api/labels/incomes` | `GetIncomeLabels.Handler` | 查詢收入類標籤 |
| **GetExpenditureLabels** | GET | `/api/labels/expenditures` | `GetExpenditureLabels.Handler` | 查詢支出類標籤 |
| **CreateLabel** | POST | `/api/labels` | `CreateLabel.Handler` | 建立新標籤 |
| **UpdateLabel** | PUT | `/api/labels/{id}` | `UpdateLabel.Handler` | 更新標籤 |
| **DeleteLabel** | DELETE | `/api/labels/{id}` | `DeleteLabel.Handler` | 軟刪除標籤 |

### 檔案結構

```
Features/Labels/
├── CreateLabel.cs          # POST 建立標籤
├── DeleteLabel.cs          # DELETE 軟刪除
├── GetLabel.cs             # GET 單個標籤
├── GetLabels.cs            # GET 分頁列表
├── GetIncomeLabels.cs      # GET 收入標籤
├── GetExpenditureLabels.cs # GET 支出標籤
└── UpdateLabel.cs          # PUT 更新標籤
```

### API 詳細規格

#### 1. **CreateLabel** - 建立標籤

**端點**: `POST /api/labels`  
**Request Body**:
```json
{
  "name": "薪資",
  "isIncome": true
}
```

**Response** (201 Created):
```json
"l_01J9KT123456789ABCDEF"  // Label ID
```

**錯誤**:
- `Label.AlreadyExists` (400): 標籤名稱重複

**檔案**: [Features/Labels/CreateLabel.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Labels/CreateLabel.cs)

---

#### 2. **GetLabels** - 查詢所有標籤（分頁）

**端點**: `GET /api/labels?page=1&pageSize=10`  
**Query Parameters**:
- `page` (int, 預設 1)
- `pageSize` (int, 預設 10)

**Response** (200 OK):
```json
{
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "items": [
    {
      "id": "l_01J9KT...",
      "name": "薪資",
      "isIncome": true,
      "isDeleted": false
    }
  ]
}
```

**檔案**: [Features/Labels/GetLabels.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Labels/GetLabels.cs)

---

#### 3. **GetLabel** - 查詢單個標籤

**端點**: `GET /api/labels/{id}`  
**Response** (200 OK):
```json
{
  "id": "l_01J9KT...",
  "name": "薪資",
  "isIncome": true,
  "isDeleted": false
}
```

**錯誤**:
- `Label.NotFound` (404): 標籤不存在

**檔案**: [Features/Labels/GetLabel.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Labels/GetLabel.cs)

---

#### 4. **GetIncomeLabels** - 查詢收入標籤

**端點**: `GET /api/labels/incomes?page=1&pageSize=10`  
**Response**: 同 `GetLabels`，但僅回傳 `isIncome = true` 的標籤

**檔案**: [Features/Labels/GetIncomeLabels.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Labels/GetIncomeLabels.cs)

---

#### 5. **GetExpenditureLabels** - 查詢支出標籤

**端點**: `GET /api/labels/expenditures?page=1&pageSize=10`  
**Response**: 同 `GetLabels`，但僅回傳 `isIncome = false` 的標籤

**檔案**: [Features/Labels/GetExpenditureLabels.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Labels/GetExpenditureLabels.cs)

---

#### 6. **UpdateLabel** - 更新標籤

**端點**: `PUT /api/labels/{id}`  
**Request Body**:
```json
{
  "name": "食物",
  "isIncome": false
}
```

**Response** (204 No Content)

**錯誤**:
- `Label.NotFound` (404): 標籤不存在

**檔案**: [Features/Labels/UpdateLabel.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Labels/UpdateLabel.cs)

---

#### 7. **DeleteLabel** - 軟刪除標籤

**端點**: `DELETE /api/labels/{id}`  
**Response** (204 No Content)

**錯誤**:
- `Label.NotFound` (404): 標籤不存在

**檔案**: [Features/Labels/DeleteLabel.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Labels/DeleteLabel.cs)

---

## 📂 Incomes (收入管理) - 完整 CRUD

### Feature 清單

| Feature | HTTP | 端點 | Handler | 用途 |
|---------|------|------|---------|------|
| **GetIncomes** | GET | `/api/incomes` | `GetIncomes.Handler` | 分頁查詢收入記錄 |
| **GetIncome** | GET | `/api/incomes/{id}` | `GetIncome.Handler` | 查詢單筆收入 |
| **CreateIncome** | POST | `/api/incomes` | `CreateIncome.Handler` | 建立收入記錄 |
| **UpdateIncome** | PUT | `/api/incomes/{id}` | `UpdateIncome.Handler` | 更新收入記錄 |
| **DeleteIncome** | DELETE | `/api/incomes/{id}` | `DeleteIncome.Handler` | 刪除收入記錄 |

### 檔案結構

```
Features/Incomes/
├── CreateIncome.cs      # POST 建立收入
├── DeleteIncome.cs      # DELETE 刪除收入
├── GetIncome.cs         # GET 單筆收入
├── GetIncomes.cs        # GET 分頁列表
└── UpdateIncome.cs      # PUT 更新收入
```

### API 詳細規格

#### 1. **CreateIncome** - 建立收入

**端點**: `POST /api/incomes`  
**Request Body**:
```json
{
  "incomeName": "兼職收入",
  "amount": 1000.00,
  "incomeDateOnUtc": "2025-12-01",
  "labelId": "l_01J9KT..."
}
```

**Response** (201 Created):
```json
"i_01J9KT123456789ABCDEF"  // Income ID
```

**錯誤**:
- `Label.NotFound` (404): 標籤不存在
- `Label.MustBeIncome` (400): 標籤必須為收入類別

**檔案**: [Features/Incomes/CreateIncome.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Incomes/CreateIncome.cs)

---

#### 2. **GetIncomes** - 查詢收入列表（分頁）

**端點**: `GET /api/incomes?page=1&pageSize=10`  
**Response** (200 OK):
```json
{
  "page": 1,
  "pageSize": 10,
  "totalCount": 50,
  "items": [
    {
      "id": "i_01J9KT...",
      "incomeName": "兼職收入",
      "amount": 1000.00,
      "incomeDateOnUtc": "2025-12-01",
      "labelId": "l_01J9KT...",
      "labelName": "薪資"
    }
  ]
}
```

**檔案**: [Features/Incomes/GetIncomes.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Incomes/GetIncomes.cs)

---

#### 3. **GetIncome** - 查詢單筆收入

**端點**: `GET /api/incomes/{id}`  
**Response** (200 OK): 同 `GetIncomes` 的 item 結構

**錯誤**:
- `Income.NotFound` (404): 收入記錄不存在

**檔案**: [Features/Incomes/GetIncome.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Incomes/GetIncome.cs)

---

#### 4. **UpdateIncome** - 更新收入

**端點**: `PUT /api/incomes/{id}`  
**Request Body**: 同 `CreateIncome`

**Response** (204 No Content)

**錯誤**:
- `Income.NotFound` (404)
- `Label.NotFound` (404)
- `Label.MustBeIncome` (400)

**檔案**: [Features/Incomes/UpdateIncome.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Incomes/UpdateIncome.cs)

---

#### 5. **DeleteIncome** - 刪除收入

**端點**: `DELETE /api/incomes/{id}`  
**Response** (204 No Content)

**錯誤**:
- `Income.NotFound` (404)

**檔案**: [Features/Incomes/DeleteIncome.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Incomes/DeleteIncome.cs)

---

## 📂 Expenditures (支出管理) - 完整 CRUD

### Feature 清單

| Feature | HTTP | 端點 | Handler | 用途 |
|---------|------|------|---------|------|
| **GetExpenditures** | GET | `/api/expenditures` | `GetExpenditures.Handler` | 分頁查詢支出記錄 |
| **GetExpenditure** | GET | `/api/expenditures/{id}` | `GetExpenditure.Handler` | 查詢單筆支出 |
| **CreateExpenditure** | POST | `/api/expenditures` | `CreateExpenditure.Handler` | 建立支出記錄 |
| **UpdateExpenditure** | PUT | `/api/expenditures/{id}` | `UpdateExpenditure.Handler` | 更新支出記錄 |
| **DeleteExpenditure** | DELETE | `/api/expenditures/{id}` | `DeleteExpenditure.Handler` | 刪除支出記錄 |

### 檔案結構

```
Features/Expenditures/
├── CreateExpenditure.cs      # POST 建立支出
├── DeleteExpenditure.cs      # DELETE 刪除支出
├── GetExpenditure.cs         # GET 單筆支出
├── GetExpenditures.cs        # GET 分頁列表
└── UpdateExpenditure.cs      # PUT 更新支出
```

### API 詳細規格

#### 1. **CreateExpenditure** - 建立支出

**端點**: `POST /api/expenditures`  
**Request Body**:
```json
{
  "paymentName": "晚餐",
  "amount": 200.00,
  "paymentDateOnUtc": "2025-12-01",
  "labelId": "l_01J9KT..."
}
```

**Response** (201 Created):
```json
"e_01J9KT123456789ABCDEF"  // Expenditure ID
```

**錯誤**:
- `Label.NotFound` (404): 標籤不存在
- `Label.MustBeExpenditure` (400): 標籤必須為支出類別

**檔案**: [Features/Expenditures/CreateExpenditure.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Expenditures/CreateExpenditure.cs)

---

#### 2. **GetExpenditures** - 查詢支出列表（分頁）

**端點**: `GET /api/expenditures?page=1&pageSize=10`  
**Response** (200 OK):
```json
{
  "page": 1,
  "pageSize": 10,
  "totalCount": 100,
  "items": [
    {
      "id": "e_01J9KT...",
      "paymentName": "晚餐",
      "amount": 200.00,
      "paymentDateOnUtc": "2025-12-01",
      "labelId": "l_01J9KT...",
      "labelName": "食物"
    }
  ]
}
```

**檔案**: [Features/Expenditures/GetExpenditures.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Expenditures/GetExpenditures.cs)

---

#### 3. **GetExpenditure** - 查詢單筆支出

**端點**: `GET /api/expenditures/{id}`  
**Response** (200 OK): 同 `GetExpenditures` 的 item 結構

**錯誤**:
- `Expenditure.NotFound` (404): 支出記錄不存在

**檔案**: [Features/Expenditures/GetExpenditure.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Expenditures/GetExpenditure.cs)

---

#### 4. **UpdateExpenditure** - 更新支出

**端點**: `PUT /api/expenditures/{id}`  
**Request Body**: 同 `CreateExpenditure`

**Response** (204 No Content)

**錯誤**:
- `Expenditure.NotFound` (404)
- `Label.NotFound` (404)
- `Label.MustBeExpenditure` (400)

**檔案**: [Features/Expenditures/UpdateExpenditure.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Expenditures/UpdateExpenditure.cs)

---

#### 5. **DeleteExpenditure** - 刪除支出

**端點**: `DELETE /api/expenditures/{id}`  
**Response** (204 No Content)

**錯誤**:
- `Expenditure.NotFound` (404)

**檔案**: [Features/Expenditures/DeleteExpenditure.cs](../BookKeeper/BookKeeper/BookKeeper.Api/Features/Expenditures/DeleteExpenditure.cs)

---

## 🔍 Handler 依賴關係

### MediatR Handler 註冊

所有 Handler 透過 `DependencyInjection.AddMediaR()` 自動註冊為 **Transient** 生命週期。

### 常見依賴注入模式

```csharp
// ✅ 標準 Handler 依賴
internal sealed class Handler(
    ApplicationDbContext context,  // EF Core DbContext
    IDateTimeProvider dateTime     // 時間抽象
) : IRequestHandler<Command, Result<T>>
{
    // ...
}
```

### 依賴圖

```
Handler
  ↓ 依賴
ApplicationDbContext (Scoped)
  ↓ 依賴
PostgreSQL (Database Connection Pool)

Handler
  ↓ 依賴
IDateTimeProvider (Singleton)
  ↓ 實現
DateTimeProvider
```

---

## 📊 錯誤類別清單

| 錯誤類別 | 檔案位置 | 定義的錯誤 |
|---------|---------|----------|
| **LabelErrors** | - | `NotFound`, `AlreadyExists`, `MustBeIncome`, `MustBeExpenditure` |
| **IncomeErrors** | - | `NotFound` |
| **ExpenditureErrors** | - | `NotFound` |

> **注意**: 專案中錯誤定義尚未統一至 `Shared/Errors/` 資料夾，建議未來重構時統一管理。

---� Statistics (統計功能)

### Background Jobs 清單

| Job | 排程 | Handler | 用途 |
|-----|------|---------|------|
| **ProcessStatisticOfDate** | 每日 03:00 | `CreateStatisticOfDate.ProcessStatisticOfDate` | 統計每位用戶當日的總收入與總支出，寫入 `StatisticsOfDates` 表 |
| **ProcessStatisticOfMonth** | 每月 1 日 03:00 | `CreateStatisticOfMonth.ProcessStatisticOfMonth` | 統計每位用戶上個月的總收入與總支出，寫入 `StatisticsOfMonths` 表 |

### 檔案結構

```
Features/Statistics/
├── CreateStatisticOfDate.cs   # 每日統計 Job (每日 03:00 執行)
└── CreateStatisticOfMonth.cs  # 每月統計 Job (每月 1 日 03:00 執行)
```

### Job 詳細規格

#### 1. ProcessStatisticOfDate (每日統計)

**排程**: 每日凌晨 3:00 執行  
**排程方式**: `WithDailyTimeIntervalSchedule` + `OnEveryDay().StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(3, 0))`  
**統計範圍**: 當天 (`DateOnly.FromDateTime(DateTime.UtcNow)`)  
**資料來源**: `Incomes` + `Expenditures`  
**處理邏輯**:
- GroupBy `UserId` 聚合當天的 `Incomes` 與 `Expenditures`
- 遍歷所有 Users，計算每位用戶的 `TotalIncomeAmount` 與 `TotalExpendAmount`
- 支援 Upsert：若該用戶當天已有記錄則更新，否則建立新記錄
- 計算 `SumAmount = TotalIncomeAmount - TotalExpendAmount`

**輸出實體**: `StatisticOfDate` (ID 前綴 `sod_`)  
**唯一索引**: `(UserId, DateOnUtc)`

#### 2. ProcessStatisticOfMonth (每月統計)

**排程**: 每月 1 日凌晨 3:00 執行  
**排程方式**: `WithCronSchedule("0 0 3 1 * ?")`  
**統計範圍**: 上個月 (`DateTime.UtcNow.AddMonths(-1)`)  
**資料來源**: `Incomes` + `Expenditures`（直接查詢，不依賴 StatisticsOfDates）  
| v1.3.0 | 2026-01-28 | 新增 Statistics 區塊，記錄 ProcessStatisticOfDate 與 ProcessStatisticOfMonth 背景任務，新增實體 StatisticOfDate/StatisticOfMonth |

---

**最後更新**: 2026-01-28alIncome > 0 || totalExpend > 0`），無交易則不寫入
- 支援 Upsert：若該用戶當月已有記錄則更新，否則建立新記錄
- 計算 `SumAmount = TotalIncomeAmount - TotalExpendAmount`

**輸出實體**: `StatisticOfMonth` (ID 前綴 `som_`)  
**唯一索引**: `(UserId, Year, Month)`

### 設計決策

| 決策點 | StatisticOfDate | StatisticOfMonth |
|--------|-----------------|------------------|
| **統計維度** | 按日（DateOnly） | 按月（int Year + int Month） |
| **零值記錄** | 寫入所有用戶（含零值） | 僅寫入有交易的用戶 |
| **排程頻率** | 每日 03:00 | 每月 1 日 03:00 |
| **資料來源** | Incomes + Expenditures（原始資料） | Incomes + Expenditures（原始資料，不依賴 StatisticsOfDates） |
| **Upsert 支援** | ✅ 支援 | ✅ 支援 |

**備註**:
- 兩個 Job 皆使用 `[DisallowConcurrentExecution]` 避免並發執行
- 查詢使用 `GroupBy` + `Sum` 聚合，效能依賴於資料庫索引（`UserId`、日期欄位）
- 月度統計選擇從原始資料計算而非依賴每日統計，確保獨立性與容錯性

---

## �

## 🔧 Validators 清單

| Feature | Validator | 主要規則 |
|---------|----------|---------|
| **CreateLabel** | `CreateLabel.Validator` | Name 非空 |
| **UpdateLabel** | `UpdateLabel.Validator` | Name 非空 |
| **CreateIncome** | `CreateIncome.Validator` | IncomeName 非空、Amount > 0、LabelId 非空 |
| **UpdateIncome** | `UpdateIncome.Validator` | 同上 |
| **CreateExpenditure** | `CreateExpenditure.Validator` | PaymentName 非空、Amount > 0、LabelId 非空 |
| **UpdateExpenditure** | `UpdateExpenditure.Validator` | 同上 |

### 驗證規則範例

```csharp
public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.PaymentName)
            .NotEmpty().WithMessage("Payment name is required")
            .MaximumLength(100).WithMessage("Payment name must not exceed 100 characters");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");
        
        RuleFor(x => x.LabelId)
            .NotEmpty().WithMessage("Label ID is required");
    }
}
```

---

## 📝 版本歷史

| 版本 | 日期 | 變更摘要 |
|------|------|---------|
| v1.0.0 | 2026-01-06 | 初始版本，記錄 Labels/Incomes/Expenditures 服務 |
| v1.1.0 | 2026-01-08 | 完整重建，新增詳細 API 規格、Handler 依賴關係、錯誤類別清單 |
| v1.2.0 | 2026-01-12 | 新增 Users 區塊，列出 GetCurrentUser / GetUserById 端點 (Admin 限制) |

---

**最後更新**: 2026-01-12  
**維護者**: GitHub Copilot
