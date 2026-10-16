# 繁體中文介面隔離驗證

在 Windows 互動式桌面與 .NET 8 SDK 環境執行。測試會短暫顯示測試視窗，使用私有本機假 qBittorrent HTTP API 及記憶體設定供應器，不使用實際的 VPN、qBittorrent 或自動啟動設定。

```powershell
dotnet run --project .\tests\Quantum.LocalizationChecks\Quantum.LocalizationChecks.csproj -c Release -- .\test-results
```

驗證不同埠更新、相同埠不重寫、按鈕操作不依賴顯示文字、測試並儲存設定、中文成功確認及連線失敗說明。失敗時回傳非零結束碼；成功結果與畫面存於指定資料夾。

三個分頁以控制項尺寸與字型模擬 100%、150%、200% 縮放。這不是實際切換螢幕 DPI；請另外人工檢查輸出的 PNG。較長的記錄欄位使用水平捲動。

僅產生指定比例畫面：

```powershell
$env:QUANTUM_RENDER_ONLY = '1'
$env:QUANTUM_QA_SCALE = '150'
dotnet run --project .\tests\Quantum.LocalizationChecks\Quantum.LocalizationChecks.csproj -c Release -- .\test-results
Remove-Item Env:\QUANTUM_RENDER_ONLY, Env:\QUANTUM_QA_SCALE
```
