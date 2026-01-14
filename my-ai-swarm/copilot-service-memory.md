# BookKeeper - Service Memory (服務清單)

> **版本**: v1.2.0 | **最後更新**: 2026-01-12 | **用途**: 記錄所有 Features、Endpoints、Handlers 映射關係

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

---

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
