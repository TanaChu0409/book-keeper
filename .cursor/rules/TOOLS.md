# Available Tools for Architect & Developer

本文件統一列舉所有可用工具及其使用情景。所有 Architect 和 Developer prompt 應遵循此規範。

---

## 工具清單

| 工具名稱 | 用途 | 何時使用 | 歸屬角色 |
|---------|------|---------|---------|
| **search** | 語義搜尋代碼庫 | 查找功能、模式、概念；瞭解現有架構；探索相似實現 | Architect / Developer |
| **read** | 讀取檔案內容 | 理解現有代碼結構；查看特定檔案的實現細節；檢查配置文件 | Architect / Developer |
| **edit** | 編輯檔案內容 | 實作代碼變更；修改配置；建立新檔案 | Developer |
| **agent** | 啟動 Subagent（Plan / Beast Mode 等） | 複雜多步任務；需要自主研究；需要並行處理多項工作 | Architect / Developer |

---

## 使用範例

### Architect 使用模式
```
1. 收到需求 → 先用 search/read 探索現有代碼
2. 設計方案 → 必須先讀 project-memory.md（read）
3. 規劃步驟 → 如需深度研究，啟動 agent（Plan）
4. 產出計畫 → 無需 edit，交由 Developer 執行
```

### Developer 使用模式
```
1. 收到批准計畫 → read 計畫內容確認清楚
2. 探索現有代碼 → search/read 瞭解上下文
3. 實作變更 → 使用 edit 逐步完成
4. 複雜問題 → 使用 agent 自動化研究與修複
```

---

## 工具組合最佳實踐

### 場景 A：新增 API 端點
```
1. search("ClientApp API 相關端點") → 找到現有模式
2. read("ClientAppService.cs") → 研究服務層實現
3. read("ClientAppController.cs") → 研究控制器層實現
4. 產出設計方案
5. Developer 使用 edit 建立新方法
```

### 場景 B：重構大型服務
```
1. agent(Plan) → 自動分析服務、生成重構計畫
2. Developer read 計畫
3. search("相關依賴") → 確認影響範圍
4. edit 逐步實現各個步驟
```

### 場景 C：除錯問題
```
1. search("錯誤信息或相關類別名") → 找到可能原因
2. read("問題檔案") → 查看代碼上下文
3. agent(Beast Mode) → 複雜問題自動診斷
4. Developer edit 修復
```

---

## ⚠️ 禁止操作

- ❌ Architect 直接使用 edit（應由 Developer 執行）
- ❌ 未先讀 project-memory.md 就開始規劃（避免重複決策）
- ❌ 過度使用 agent 而不先做基礎 search/read（浪費資源）
- ❌ 編輯檔案時無序進行（應遵循計畫的步驟順序）

---

## 版本

> **版本**：v1.0.0 | **更新日期**：2026-01-06 | **維護者**：@dotnet-architect
