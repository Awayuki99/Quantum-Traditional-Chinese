Imports System.Globalization
Imports System.Threading

Friend Module TaiwanText
    Private ReadOnly Strings As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
        {"Test/Save/Update", "測試／儲存／更新"},
        {"Test the qBittorrent settings above, if it connects the settings will be saved, this will also force a port update", "測試上方的 qBittorrent 連線設定；連線成功後會儲存設定，並立即更新通訊埠。"},
        {"Double click to see the unparsed log entry", "按兩下可查看原始記錄內容。"},
        {"Date/Time", "日期／時間"},
        {"Port Number", "通訊埠"},
        {"Valid", "有效"},
        {"Invalid", "無效"},
        {"Launch Quantum at System Startup", "登入 Windows 時啟動 Quantum"},
        {"Enable/Disable running Quantum when Windows starts", "啟用或停用登入 Windows 時自動啟動 Quantum。"},
        {"Locate ProtonVPN log files automaticlly", "自動尋找 Proton VPN 記錄檔"},
        {"Select automatic or manual log file selection", "選擇自動尋找或手動指定記錄檔。"},
        {"qBittorrent Host Address (e.g. http://127.0.0.1:8080)", "qBittorrent 網頁介面位址，例如 http://127.0.0.1:9090。"},
        {"Host (e.g. http://127.0.0.1:8080)", "位址（例如 http://127.0.0.1:9090）"},
        {"qBittorrent Username", "qBittorrent 使用者名稱"},
        {"qBittorrent Password", "qBittorrent 密碼"},
        {"Username", "使用者名稱"},
        {"Password", "密碼"},
        {"Settings", "設定"},
        {"Quantum Configuration", "Quantum 設定"},
        {"qBittorrent WebUI Configuration", "qBittorrent 網頁介面設定"},
        {"Action", "操作"},
        {"Logs", "記錄"},
        {"ProtonVPN Log File Output (Parsed)", "Proton VPN 記錄（解析結果）"},
        {"ProtonVPN Log Files Output (Parsed)", "Proton VPN 記錄（解析結果）"},
        {"ProtonVPN Log Files Output (Parsed) ", "Proton VPN 記錄（解析結果） "},
        {"Quantum Event Log", "Quantum 事件記錄"},
        {"Double click to see the event", "按兩下可查看事件內容。"},
        {"Event", "事件"},
        {"About", "關於"},
        {"This app is not affiliated in any way with Proton AG (https://proton.me/), use at your own risk.", "本程式與 Proton AG（https://proton.me/）無關。此為非官方繁體中文版本，請自行評估使用。"},
        {"By UHAX", "原作者：UHAX"},
        {"Show/Hide", "顯示／隱藏"},
        {"Show", "顯示"},
        {"Hide", "隱藏"},
        {"Update Port Now", "立即更新通訊埠"},
        {"Exit", "結束"},
        {"qBittorrent error: ", "qBittorrent 連線或操作失敗："},
        {"Connected to qBittorrent, settings saved!", "已連線至 qBittorrent，並儲存設定。"},
        {"Updated qBittorrent port to ", "已將 qBittorrent 通訊埠更新為 "},
        {"Reading log file...", "正在讀取記錄檔…"},
        {"Connected to qBittorrent, no changes to port detected", "已連線至 qBittorrent，通訊埠未變更。"},
        {"No valid port information found in logs", "記錄檔中找不到有效的通訊埠資訊。"},
        {"Cannot locate ProtonVPN log files", "找不到 Proton VPN 記錄檔。"},
        {"Disclaimer!", "使用說明"},
        {"Quantum Started!", "Quantum 已啟動。"},
        {"ProtonVPN Log Files Found!", "已找到 Proton VPN 記錄檔"},
        {"ProtonVPN Log Files Found!   (...)", "已找到 Proton VPN 記錄檔（選擇…）"},
        {"Enabling this setting is recommended", "啟用後會自動尋找記錄檔；若無法找到，請手動指定。"},
        {"Unable to locate ProtonVPN Logs File", "找不到 Proton VPN 記錄檔"},
        {"Unable to locate ProtonVPN Log Files   (...)", "找不到 Proton VPN 記錄檔（選擇…）"},
        {"Do you want to select a diffrent ProtonVPN log file location? Here is an example location of where you can find it: ", "要重新選擇 Proton VPN 記錄檔嗎？以下是記錄檔路徑範例："},
        {"Quantum is unable to locate the ProtonVPN log file, you must select a log file in the next screen, here is an example location of where you can find it: ", "Quantum 找不到 Proton VPN 記錄檔。請在接下來的視窗選擇記錄檔，路徑範例如下："},
        {"Confirmation", "確認"},
        {"Select the ProtonVPN log file", "選擇 Proton VPN 記錄檔"},
        {"Quantum cannot run properly without a valid log file!!!", "請指定有效的記錄檔，Quantum 才能正常運作。"},
        {"Warning", "提醒"},
        {"Error, check logs for more information - ", "發生錯誤，請查看記錄以了解詳情 — "},
        {"ProtonVPN Log File Entry", "Proton VPN 原始記錄"},
        {"Quantum Log Event - ", "Quantum 事件記錄 — "},
        {"Are you sure you want to disable automatic log file location?", "確定要停用自動尋找記錄檔嗎？"},
        {"Are you sure you want to delete the startup entry?", "確定要取消登入 Windows 時自動啟動嗎？"},
        {"Test/Save Settings", "測試並儲存設定"},
        {"Unexpected error: ", "發生錯誤："},
        {"Traditional Chinese suffix", "（繁體中文）"},
        {"Log file filter", "Proton VPN 記錄檔（*.txt）|*.txt|所有檔案（*.*）|*.*"}
    }

    Public Function [Get](key As String) As String
        Return Strings(key)
    End Function

    Public Sub ApplyCulture()
        Dim culture = CultureInfo.GetCultureInfo("zh-TW")
        Thread.CurrentThread.CurrentCulture = culture
        Thread.CurrentThread.CurrentUICulture = culture
        CultureInfo.DefaultThreadCurrentCulture = culture
        CultureInfo.DefaultThreadCurrentUICulture = culture
    End Sub
End Module
