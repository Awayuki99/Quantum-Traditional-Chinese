# Quantum 繁體中文版（台灣用語）

自動將 Proton VPN 分配的轉送通訊埠同步至 qBittorrent。此 fork 基於 [UHAXM1/Quantum](https://github.com/UHAXM1/Quantum) v1.5.0，提供非官方繁體中文介面，使用台灣用語。

[下載繁體中文版](https://github.com/Awayuki99/Quantum-Traditional-Chinese/releases/latest) · [建置說明](BUILD-zh-TW.md) · [上游英文說明](README.en.md)

## 功能

- 每分鐘讀取 Proton VPN 記錄檔，通訊埠變更時自動更新 qBittorrent。
- 「設定」、「記錄」、「關於」、系統匣選單、提示及程式狀態訊息均使用繁體中文。
- 可登入 Windows 後自動啟動，關閉視窗時縮小至系統匣。
- 保留產品名稱、網址、路徑、Proton 原始記錄及外部錯誤詳情，方便查核。

Quantum 不會將 Proton VPN 的通訊埠固定。重新連線後，會在下一輪檢查同步新埠號，通常需等待約一分鐘。

## 執行需求

- Windows x64，以及 .NET 8 Windows Desktop Runtime。
- 已啟用通訊埠轉送並連上支援此功能伺服器的 Proton VPN。
- 已啟用網頁介面的 qBittorrent。

本版本曾在 Proton VPN 5.1.8 與 qBittorrent 5.2.4.10 上完成本機驗證。程式仍依賴 Proton 記錄格式，後續版本可能需要調整。

## 下載與設定

1. 在 [Releases](https://github.com/Awayuki99/Quantum-Traditional-Chinese/releases) 下載 `Quantum-1.5.0-zh-TW.1-win-x64.zip`，完整解壓縮後執行 `Quantum.exe`。
2. 在 qBittorrent「工具 → 選項 → 網頁介面」啟用網頁介面，確認位址、通訊埠與驗證方式。只在本機操作時，可使用 `127.0.0.1`。
3. 在 Quantum「設定」輸入網頁介面位址，例如 `http://127.0.0.1:8080`，以及設定的使用者名稱、密碼。若 qBittorrent 已允許本機略過驗證，這兩欄可留白。
4. 按「測試並儲存設定」。成功後，按鈕會改為「立即更新通訊埠」。
5. 確認 Proton VPN 記錄檔位置。若自動尋找失敗，關閉「自動尋找 Proton VPN 記錄檔」，手動選擇記錄檔。

新版 Proton VPN 的記錄檔通常位於：

```text
%LOCALAPPDATA%\Proton\Proton VPN\Logs\client-logs.txt
```

舊版可能使用安裝資料夾下的 `ServiceData\Logs\service-logs.txt`。自動尋找功能沿用上游做法；新版 Proton 建議確認實際路徑。

設定完成後，不需要按 Proton 通知的「複製」，也不需要每次手動按「立即更新通訊埠」。可在「記錄」查看偵測到的通訊埠及同步結果；較長事件可水平捲動，或按兩下查看完整內容。

若要取消登入自動啟動，關閉「登入 Windows 時啟動 Quantum」。若要停止程式，使用系統匣選單的「結束」。

## 從現有版本升級

先結束 Quantum 並備份原資料夾。替換程式檔時，保留原本的 `Quantum.dll.config` 與使用者設定；完成後確認網頁介面及記錄檔路徑。

Release 壓縮檔僅含程式、授權與通用預設值，不包含個人帳密、記錄檔或本機部署資料。

## 開發與測試

```powershell
git clone --recurse-submodules https://github.com/Awayuki99/Quantum-Traditional-Chinese.git
cd Quantum-Traditional-Chinese
dotnet build .\scr\Quantum\Quantum.vbproj -c Release -p:GeneratePackageOnBuild=false
```

完整建置與修改說明請見 [BUILD-zh-TW.md](BUILD-zh-TW.md)。隔離驗證程式位於 [tests/Quantum.LocalizationChecks](tests/Quantum.LocalizationChecks/README.md)，使用假 qBittorrent API 與記憶體設定供應器。

## 授權與致謝

原作者：UHAX。上游專案：[UHAXM1/Quantum](https://github.com/UHAXM1/Quantum)。

Quantum 依 [GPL-3.0](LICENSE) 授權；使用的 [qBittorrent-net-client](https://github.com/fedarovich/qbittorrent-net-client) 保留其原始授權。此 fork 與 Proton AG、qBittorrent 專案及原作者無官方關聯。
