# Quantum 1.5.0 非官方繁體中文版

原作者 UHAX。基於官方 v1.5.0 原始碼，繁體中文介面使用台灣用語。
Quantum 原始碼依 GPL-3.0 授權，完整條款見 LICENSE；qBittorrent Client 依賴保留其原始授權。

## 原始碼來源

- Quantum：https://github.com/UHAXM1/Quantum/tree/v1.5.0
- qBittorrent Client 子模組固定提交：`68397e48b0be1b763e9afafb203a5817961940ac`，由 `libs/qbittorrent-net-client` 子模組提供；請使用遞迴複製或先執行 `git submodule update --init --recursive`。

## 修改

- 新增 `scr/Quantum/TaiwanText.vb` 集中管理 65 組介面與狀態字串，預設 zh-TW。
- 翻譯分頁、按鈕、提示、系統匣選單、確認視窗與程式事件。
- 以 ConnectionAction 明確狀態決定測試／儲存或更新通訊埠，不依賴顯示文字。
- 使用 Microsoft JhengHei UI 9pt 與 PerMonitorV2；修正欄位高度、記錄欄寬與關於頁連結範圍。
- 檔案選擇器支援文字記錄檔，包含目前 Proton 的 client-logs.txt。
- 保留原始通訊埠解析、每分鐘檢查與 qBittorrent API 操作方式。
- 保留組件名稱、公司 UHAX 與版本 1.5.0.0，便於沿用現有設定。

## 建置

Windows 上使用 .NET 8 SDK。此次使用 8.0.426，SDK 解壓至工作資料夾，沒有安裝系統 SDK。

```powershell
dotnet build .\scr\Quantum\Quantum.vbproj -c Release -p:GeneratePackageOnBuild=false
```

產物位於 `scr/Quantum/bin/Release/net8.0-windows7.0`。
執行需要 .NET 8 Windows Desktop Runtime。
部署時請沿用目前的 `Quantum.dll.config`；原始碼的預設設定不包含個人連線設定。

## 驗證

隔離測試使用私有本機假 qBittorrent API 與記憶體設定供應器，不存取使用者的 qBittorrent 或變更自動啟動。
涵蓋不同埠更新、相同埠不重寫、按鈕不依賴文字、測試並儲存、成功確認及連線失敗中文說明。
三個分頁以 100%、150%、200% 模擬縮放產生畫面並檢查；未變更使用者的 Windows 顯示比例。

