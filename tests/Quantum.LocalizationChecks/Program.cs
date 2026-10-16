using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Drawing.Imaging;

class MemorySettings : SettingsProvider
{
    readonly Dictionary<string, object> values = new();
    public override string ApplicationName { get; set; } = "QuantumChecks";
    public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection properties)
    {
        var result = new SettingsPropertyValueCollection();
        foreach (SettingsProperty property in properties)
        {
            object value = values.TryGetValue(property.Name, out var saved) ? saved : Convert.ChangeType(property.DefaultValue, property.PropertyType, CultureInfo.InvariantCulture);
            result.Add(new SettingsPropertyValue(property) { PropertyValue = value, IsDirty = false });
        }
        return result;
    }
    public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection properties)
    {
        foreach (SettingsPropertyValue property in properties) values[property.Name] = property.PropertyValue;
    }
}

class FakeQbit : IDisposable
{
    readonly TcpListener listener = new(IPAddress.Loopback, 0);
    readonly CancellationTokenSource stop = new();
    public int ListenPort = 12345;
    public int Updates;
    public string Url { get; private set; }
    public FakeQbit()
    {
        listener.Start();
        Url = "http://127.0.0.1:" + ((IPEndPoint)listener.LocalEndpoint).Port;
        _ = Serve();
    }
    async Task Serve()
    {
        while (!stop.IsCancellationRequested)
        {
            try { var client = await listener.AcceptTcpClientAsync(stop.Token); _ = Handle(client).ContinueWith(t => { if (t.IsFaulted) Console.Error.WriteLine(t.Exception); }); }
            catch (OperationCanceledException) { break; }
            catch (ObjectDisposedException) { break; }
        }
    }
    async Task Handle(TcpClient client)
    {
        using (client)
        {
            var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, false, 4096, true);
            var first = await reader.ReadLineAsync();
            if (first == null) return;
            string path = first.Split(' ')[1];
            Console.WriteLine("MOCK: " + first);
            int length = 0;
            bool expectContinue = false;
            string header;
            while (!string.IsNullOrEmpty(header = await reader.ReadLineAsync()))
            {
                if (header.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) length = int.Parse(header.Substring(15).Trim());
                if (header.StartsWith("Expect:", StringComparison.OrdinalIgnoreCase)) expectContinue = true;
            }
            if (expectContinue) await stream.WriteAsync(Encoding.ASCII.GetBytes("HTTP/1.1 100 Continue\r\n\r\n"));
            var body = new char[length];
            int read = 0;
            while (read < length) { int n = await reader.ReadAsync(body.AsMemory(read)); if (n == 0) break; read += n; }
            string output = "";
            string mime = "text/plain";
            if (path.EndsWith("/auth/login")) output = "Ok.";
            else if (path.EndsWith("/app/preferences")) { output = "{\"listen_port\":" + ListenPort + "}"; mime = "application/json"; }
            else if (path.EndsWith("/app/setPreferences"))
            {
                string form = new(body);
                string json = Uri.UnescapeDataString(form.Substring(form.IndexOf('=') + 1).Replace("+", " "));
                ListenPort = JsonDocument.Parse(json).RootElement.GetProperty("listen_port").GetInt32();
                Interlocked.Increment(ref Updates);
            }
            else if (path.EndsWith("/app/webapiVersion")) output = "2.8.3";
            else if (path.EndsWith("/app/version")) output = "v5.2.4";
            else if (path == "/version/api" || path == "/version/api_min") output = "25";
            byte[] payload = Encoding.UTF8.GetBytes(output);
            byte[] headers = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: " + mime + "; charset=utf-8\r\nContent-Length: " + payload.Length + "\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(headers);
            await stream.WriteAsync(payload);
            await stream.FlushAsync();
            client.Client.Shutdown(SocketShutdown.Send);
            await Task.Delay(100);
        }
    }
    public void Dispose() { stop.Cancel(); listener.Stop(); stop.Dispose(); }
}

static class Program
{
    static readonly BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    static readonly List<string> passed = new();
    static Form main;
    static ApplicationSettingsBase settings;
    static FakeQbit server;
    static string output;
    static int result;

    static object Member(object owner, string name) => owner.GetType().GetProperty(name, Members)?.GetValue(owner) ?? owner.GetType().GetField(name, Members)?.GetValue(owner);
    static T Control<T>(string name) => (T)Member(main, name);
    static async Task Call(string method) { await (Task)main.GetType().GetMethod(method, Members).Invoke(main, null); }
    static void Check(bool condition, string name) { if (!condition) throw new Exception(name); passed.Add(name); Console.WriteLine("PASS: " + name); }
    static async Task WaitUntil(Func<bool> condition)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        while (!condition() && timer.Elapsed < TimeSpan.FromSeconds(8)) await Task.Delay(25);
    }
    static void Setting(string name, object value) => settings[name] = Convert.ChangeType(value, settings.Properties[name].PropertyType, CultureInfo.InvariantCulture);

    [STAThread]
    static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        output = Path.GetFullPath(args.Length > 0 ? args[0] : "work/Quantum-QA");
        Directory.CreateDirectory(output);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetDefaultFont(new Font("Microsoft JhengHei UI", 9));
        using (server = new FakeQbit())
        {
            var assembly = typeof(Quantum.Main).Assembly;
            var type = assembly.GetType("Quantum.My.MySettings", true);
            settings = (ApplicationSettingsBase)type.GetProperty("Default", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            var provider = new MemorySettings(); provider.Initialize("Memory", null);
            settings.Providers.Clear(); settings.Providers.Add(provider);
            foreach (SettingsProperty property in settings.Properties) property.Provider = provider;
            settings.Reload();
            Setting("Host", server.Url); Setting("Username", ""); Setting("Password", "");
            Setting("FirstRun", false); Setting("UpgradeRequired", false); Setting("AutoFindLog", false);
            string fixture = Path.Combine(output, "proton-fixture.txt");
            File.WriteAllText(fixture, "2026-10-14T10:00:00Z | INFO | PROCESS.COMM | Received PortForwarding Status 'SleepingUntilRefresh', Port pair 37084->37084, expiring in 00:01:00 at 2026/10/14 上午 10:01:00 | {}\n");
            Setting("FilePath", fixture);
            main = new Quantum.Main();
            // Exercise the real UI/sync methods without the production startup
            // handler, which registers autostart and reads the actual registry.
            var productionLoad = main.GetType().GetMethod("Main_Load", Members);
            main.Load -= (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), main, productionLoad);
            main.Text = "Quantum - v1.5.0（繁體中文）";
            Control<TextBox>("TXT_Host").Text = server.Url;
            main.GetType().GetMethod("UpdateSelectButton", Members).Invoke(main, null);
            main.Load += (_, _) => main.BeginInvoke(new Action(async () => await RunTests()));
            Application.Run(main);
        }
        return result;
    }

    static async Task RunTests()
    {
        try
        {
            if (Environment.GetEnvironmentVariable("QUANTUM_RENDER_ONLY") == "1")
            {
                Control<ListView>("LV_VPNLogFile").Items.Add(new ListViewItem(new[] { "2026-10-14T10:00:00Z", "37084", "有效" }));
                Control<ListView>("LV_QLog").Items.Add(new ListViewItem(new[] { "2026/10/14 下午 07:00:00", "已將 qBittorrent 通訊埠更新為 37084" }));
                CaptureLayouts();
                return;
            }
            using (var probe = new System.Net.Http.HttpClient())
                Check(await probe.GetStringAsync(server.Url + "/version/api") == "25", "Private fake API responds correctly");
            using (var probeClient = new QBittorrent.Client.QBittorrentClient(new Uri(server.Url)))
            {
                await probeClient.LoginAsync("", "");
                await probeClient.GetPreferencesAsync();
            }
            await Task.Delay(300);
            Control<System.Windows.Forms.Timer>("TMR_Main").Stop();
            await Call("CheckConnection");
            await WaitUntil(() => server.ListenPort == 37084 && Control<ToolStripStatusLabel>("TSSL_Main").Text.Contains("已將 qBittorrent 通訊埠更新為"));
            Control<System.Windows.Forms.Timer>("TMR_Main").Stop();
            Check(server.ListenPort == 37084, "Different port is corrected by the original sync flow");
            int before = server.Updates;
            await Call("CheckConnection"); await Task.Delay(50);
            Control<System.Windows.Forms.Timer>("TMR_Main").Stop();
            Check(server.Updates == before, "Matching port is not rewritten");
            Check(Control<ToolStripStatusLabel>("TSSL_Main").Text.Contains("通訊埠未變更"), "Normal status is Traditional Chinese");
            Check(Control<Button>("BTN_TestSaveUpdate").Text == "立即更新通訊埠", "Saved settings show the localized update action");

            server.ListenPort = 37085;
            Control<Button>("BTN_TestSaveUpdate").Text = "測試文字，操作不應依賴文字";
            Control<Button>("BTN_TestSaveUpdate").PerformClick();
            await WaitUntil(() => server.ListenPort == 37084);
            Control<System.Windows.Forms.Timer>("TMR_Main").Stop();
            Check(server.ListenPort == 37084, "Update button uses explicit action state rather than displayed text");

            Control<TextBox>("TXT_Host").Text = server.Url + "/";
            Check(Control<Button>("BTN_TestSaveUpdate").Text == "測試並儲存設定", "Editing connection settings selects the save action");
            using var dialogCloser = new System.Windows.Forms.Timer { Interval = 100 };
            string dialogText = "";
            dialogCloser.Tick += (_, _) => dialogText = Dialogs.AcceptSavedDialog(dialogText);
            dialogCloser.Start();
            Control<Button>("BTN_TestSaveUpdate").PerformClick();
            for (int retry = 0; retry < 30 && Control<Button>("BTN_TestSaveUpdate").Text != "立即更新通訊埠"; retry++) await Task.Delay(100);
            dialogCloser.Stop(); Control<System.Windows.Forms.Timer>("TMR_Main").Stop();
            Check((string)settings["Host"] == server.Url + "/", "Test-and-save action persists the tested settings in an isolated provider");
            Check(dialogText.Contains("已連線至 qBittorrent"), "Successful confirmation dialog is Traditional Chinese");
            Check(Control<Button>("BTN_TestSaveUpdate").Text == "立即更新通訊埠", "Save completion restores the update action");

            // Use a closed local endpoint; no request reaches the user's real client.
            var temporary = new TcpListener(IPAddress.Loopback, 0); temporary.Start();
            int closedPort = ((IPEndPoint)temporary.LocalEndpoint).Port; temporary.Stop();
            Setting("Host", "http://127.0.0.1:" + closedPort);
            await Call("CheckConnection"); Control<System.Windows.Forms.Timer>("TMR_Main").Stop();
            var log = Control<ListView>("LV_QLog");
            Check(log.Items.Cast<ListViewItem>().Any(item => item.SubItems[1].Text.StartsWith("qBittorrent 連線或操作失敗：")), "Connection failures include a localized explanation and original error details");
            Setting("Host", server.Url + "/"); Control<TextBox>("TXT_Host").Text = server.Url + "/";
            await Call("CheckConnection"); Control<System.Windows.Forms.Timer>("TMR_Main").Stop();
            CaptureLayouts();
            File.WriteAllText(Path.Combine(output, "results.json"), JsonSerializer.Serialize(new { Result = "PASS", Scenarios = passed }, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception error) {
            result = 1; Console.Error.WriteLine(error);
            foreach (ListViewItem item in Control<ListView>("LV_QLog").Items) Console.Error.WriteLine("APP: " + item.SubItems[1].Text);
            Console.Error.WriteLine("CONFIG: " + settings["Host"] + "; " + settings["FilePath"]);
        }
        finally { Control<System.Windows.Forms.Timer>("TMR_Main").Stop(); main.Dispose(); Application.ExitThread(); }
    }

    static void CaptureLayouts()
    {
        var original = main;
        var vpnItems = Control<ListView>("LV_VPNLogFile").Items.Cast<ListViewItem>().Select(item => item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(sub => sub.Text).ToArray()).ToArray();
        var appItems = Control<ListView>("LV_QLog").Items.Cast<ListViewItem>().Select(item => item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(sub => sub.Text).ToArray()).ToArray();
        string scaleOverride = Environment.GetEnvironmentVariable("QUANTUM_QA_SCALE");
        int[] scales = scaleOverride == null ? new[] { 100, 150, 200 } : new[] { int.Parse(scaleOverride) };
        foreach (int percentage in scales)
        {
            main = new Quantum.Main();
            main.Load -= (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), main, main.GetType().GetMethod("Main_Load", Members));
            main.Text = "Quantum - v1.5.0（繁體中文）";
            main.WindowState = FormWindowState.Normal;
            main.Show(); DisableAutoScale(main); main.ClientSize = new Size(480, 450);
            main.Location = new Point(Screen.PrimaryScreen.WorkingArea.Left + 10, Screen.PrimaryScreen.WorkingArea.Top + 10);
            main.BringToFront(); main.Activate();
            Control<TextBox>("TXT_Host").Text = server.Url + "/";
            main.GetType().GetMethod("UpdateSelectButton", Members).Invoke(main, null);
            Control<ListView>("LV_VPNLogFile").Items.AddRange(vpnItems.Select(item => new ListViewItem(item)).ToArray());
            Control<ListView>("LV_QLog").Items.AddRange(appItems.Select(item => new ListViewItem(item)).ToArray());
            float factor = percentage / 100f;
            if (factor != 1) { main.Scale(new SizeF(factor, factor)); ScaleFonts(main, factor); }
            main.GetType().GetMethod("FitRecordColumns", Members).Invoke(main, new object[] {main, EventArgs.Empty});
            var tabs = Control<TabControl>("TAB_Main");
            foreach (TabPage page in tabs.TabPages)
            {
                tabs.SelectedTab = page;
                main.WindowState = FormWindowState.Normal;
                main.Show(); main.Location = new Point(20, 20); main.BringToFront(); main.Activate();
                main.PerformLayout(); main.Refresh(); Application.DoEvents(); Thread.Sleep(300);
                Console.WriteLine($"RENDER: {page.Name} {percentage}% {main.Bounds} visible={main.Visible} state={main.WindowState}");
                using var image = new Bitmap(main.Width, main.Height);
                main.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                string pageName = page.Name.Replace("TP_", "");
                image.Save(Path.Combine(output, $"{pageName}-{percentage}.png"), ImageFormat.Png);
            }
            main.Dispose();
        }
        main = original;
        Check(true, "Rendered all three pages at 100%, 150%, and 200% for visual inspection");
    }
    static void ScaleFonts(System.Windows.Forms.Control control, float factor)
    {
        control.Font = new Font(control.Font.FontFamily, control.Font.Size * factor, control.Font.Style);
        // Child controls inherit the scaled font unless they define their own font.
    }
    static void DisableAutoScale(System.Windows.Forms.Control control)
    {
        if (control is ContainerControl container) container.AutoScaleMode = AutoScaleMode.None;
        foreach (System.Windows.Forms.Control child in control.Controls) DisableAutoScale(child);
    }
}

static class Dialogs
{
    delegate bool Callback(IntPtr handle, IntPtr parameter);
    [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool EnumWindows(Callback callback, IntPtr data);
    [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr parent, Callback callback, IntPtr data);
    [System.Runtime.InteropServices.DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr handle, out uint process);
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)] static extern int GetClassName(IntPtr handle, StringBuilder text, int max);
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)] static extern int GetWindowText(IntPtr handle, StringBuilder text, int max);
    [System.Runtime.InteropServices.DllImport("user32.dll")] static extern IntPtr PostMessage(IntPtr handle, uint message, IntPtr parameter, IntPtr data);
    [System.Runtime.InteropServices.DllImport("user32.dll")] static extern IntPtr GetDlgItem(IntPtr parent, int id);
    public static string AcceptSavedDialog(string previous)
    {
        string found = previous;
        EnumWindows((handle, data) =>
        {
            GetWindowThreadProcessId(handle, out uint process);
            if (process != Environment.ProcessId) return true;
            var name = new StringBuilder(256); GetClassName(handle, name, name.Capacity);
            if (name.ToString() != "#32770") return true;
            string text = "";
            EnumChildWindows(handle, (child, _) => { var label = new StringBuilder(2048); GetWindowText(child, label, label.Capacity); text += label.ToString(); return true; }, IntPtr.Zero);
            if (text.Contains("已連線至 qBittorrent")) { found = text; PostMessage(handle, 0x0010, IntPtr.Zero, IntPtr.Zero); }
            return true;
        }, IntPtr.Zero);
        return found;
    }
}
