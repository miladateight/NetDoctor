using NetDoctor.App.Core;
using NetDoctor.App.Forms;
using NetDoctor.App.Licensing;
using NetDoctor.App.Localization;
using NetDoctor.App.Models;
using NetDoctor.App.Services;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace NetDoctor.App;

internal sealed partial class MainForm : Form
{
    private readonly DiagnosisEngine diagnosisEngine = new();
    private readonly SafeFixService safeFixService = new();
    private readonly HistorySessionService historyService = new();
    private readonly ReportService reportService = new();
    private readonly SpeedTestService speedTestService = new();
    private readonly List<ProblemDefinition> problems = ProblemDefinition.Defaults();
    private readonly Dictionary<string, PillButton> navButtons = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, StatusCard> statusCards = new(StringComparer.OrdinalIgnoreCase);

    private readonly Panel topBar = new();
    private readonly Panel sidebar = new();
    private readonly Panel viewHost = new();
    private readonly Label statusBar = new();
    private readonly Toast toast = new();
    private readonly NotifyIcon trayIcon = new();
    private readonly Label licensePill = new();
    private readonly Button themeButton = new();

    private AppSettings settings;
    private DiagnosticReport? lastReport;
    private CancellationTokenSource? diagnosisCancellation;
    private CancellationTokenSource? speedCancellation;
    private TextBox? detailsBox;
    private KpiCard? kpiLocal;
    private KpiCard? kpiInternational;
    private KpiCard? kpiDns;
    private KpiCard? kpiQuality;
    private Button? fixButton;
    private Button? undoButton;
    private ListBox? historyList;
    private bool allowExit;

    public MainForm(AppSettings appSettings)
    {
        settings = appSettings;
        ThemeManager.Apply(AppConfig.Theme);
        Text = L.AppTitle;
        MinimumSize = new Size(1060, 720);
        Size = new Size(1260, 820);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Palette.AppBackground;
        Font = UiFonts.Create(10F);
        RightToLeft = AppConfig.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        RightToLeftLayout = AppConfig.IsRtl;
        Icon = TryLoadIcon();

        BuildShell();
        BuildTray();
        NetworkChange.NetworkAddressChanged += OnNetworkChanged;
        Navigate("dashboard");
        UpdateLicensePill();
    }

    public MainForm() : this(SettingsService.LoadAndApply())
    {
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!allowExit && e.CloseReason == CloseReason.UserClosing && settings.MinimizeToTrayOnClose)
        {
            e.Cancel = true;
            Hide();
            trayIcon.Visible = true;
            trayIcon.ShowBalloonTip(1800, L.AppTitle, "Net Doctor is still running in the tray.", ToolTipIcon.Info);
            return;
        }

        trayIcon.Visible = false;
        trayIcon.Dispose();
        NetworkChange.NetworkAddressChanged -= OnNetworkChanged;
        base.OnFormClosing(e);
    }

    private void BuildShell()
    {
        Controls.Clear();
        topBar.Dock = DockStyle.Top;
        topBar.Height = 72;
        topBar.BackColor = Palette.Shell;
        topBar.Padding = P(24, 12, 24, 10);

        sidebar.Dock = AppConfig.IsRtl ? DockStyle.Right : DockStyle.Left;
        sidebar.Width = 238;
        sidebar.BackColor = Palette.Shell;
        sidebar.Padding = new Padding(18, 18, 18, 18);

        viewHost.Dock = DockStyle.Fill;
        viewHost.BackColor = Palette.AppBackground;
        viewHost.Padding = new Padding(22);

        statusBar.Dock = DockStyle.Bottom;
        statusBar.Height = 32;
        statusBar.BackColor = Palette.Shell;
        statusBar.ForeColor = Palette.MutedText;
        statusBar.TextAlign = ContentAlignment.MiddleLeft;
        statusBar.Padding = P(22, 0, 22, 0);
        statusBar.Text = L.Ready;

        BuildTopBar();
        BuildSidebar();
        Controls.Add(viewHost);
        Controls.Add(sidebar);
        Controls.Add(statusBar);
        Controls.Add(topBar);
    }

    private void BuildTopBar()
    {
        topBar.Controls.Clear();
        var title = new Label
        {
            Dock = DockStyle.Left,
            Width = 430,
            Text = L.T("App.Title") + Environment.NewLine + L.T("App.Subtitle"),
            ForeColor = Palette.Text,
            Font = UiFonts.Create(10F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 2, 0, 0)
        };

        var quick = CreatePill($"{Glyphs.Play} {L.T("TopBar.QuickCheck")}", true);
        quick.Width = 142;
        quick.Click += async (_, _) => await RunDiagnosisAsync(easyMode: true);

        themeButton.FlatStyle = FlatStyle.Flat;
        themeButton.FlatAppearance.BorderSize = 0;
        themeButton.Width = 44;
        themeButton.Height = 42;
        themeButton.Font = new Font("Segoe MDL2 Assets", 12F);
        themeButton.Text = ThemeManager.Current == ThemeMode.Dark ? Glyphs.Sun : Glyphs.Moon;
        themeButton.BackColor = Palette.LightButton;
        themeButton.ForeColor = Palette.Text;
        themeButton.Cursor = Cursors.Hand;
        themeButton.Click += (_, _) => ToggleTheme();

        licensePill.AutoSize = false;
        licensePill.Width = 132;
        licensePill.Height = 42;
        licensePill.TextAlign = ContentAlignment.MiddleCenter;
        licensePill.Font = UiFonts.Create(8.7F, FontStyle.Bold);
        licensePill.Margin = new Padding(8, 0, 0, 0);

        actions.Controls.Add(quick);
        actions.Controls.Add(themeButton);
        actions.Controls.Add(licensePill);
        topBar.Controls.Add(actions);
        topBar.Controls.Add(title);
    }

    private void BuildSidebar()
    {
        sidebar.Controls.Clear();
        navButtons.Clear();
        var logo = new Label
        {
            Dock = DockStyle.Top,
            Height = 66,
            Text = "Net Doctor\nCommand Center",
            ForeColor = Palette.Text,
            Font = UiFonts.Create(14F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        sidebar.Controls.Add(logo);

        var items = new (string Id, string Glyph, string Key)[]
        {
            ("dashboard", Glyphs.Dashboard, "Nav.Dashboard"),
            ("diagnose", Glyphs.Diagnose, "Nav.Diagnose"),
            ("reset", Glyphs.Reset, "Nav.Reset"),
            ("speed", Glyphs.Speed, "Nav.SpeedTest"),
            ("monitor", Glyphs.Monitor, "Nav.Monitor"),
            ("history", Glyphs.History, "Nav.History"),
            ("logs", Glyphs.Logs, "Nav.Logs"),
            ("settings", Glyphs.Settings, "Nav.Settings"),
            ("about", Glyphs.About, "Nav.About")
        };

        for (var index = items.Length - 1; index >= 0; index--)
        {
            var item = items[index];
            sidebar.Controls.Add(CreateNavButton(item.Id, $"{item.Glyph}  {L.T(item.Key)}"));
        }
    }

    private PillButton CreateNavButton(string id, string text)
    {
        var button = CreatePill(text);
        button.Dock = DockStyle.Top;
        button.Height = 44;
        button.Margin = new Padding(0, 0, 0, 8);
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.Click += (_, _) => Navigate(id);
        navButtons[id] = button;
        return button;
    }

    private PillButton CreatePill(string text, bool accent = false)
    {
        return new PillButton
        {
            Text = text,
            Accent = accent,
            Width = 128,
            Height = 42,
            Margin = new Padding(8, 0, 0, 0)
        };
    }
    private void Navigate(string id)
    {
        foreach (var item in navButtons)
        {
            item.Value.Accent = item.Key == id;
            item.Value.Invalidate();
        }

        Control view = id switch
        {
            "diagnose" => BuildDiagnoseView(),
            "reset" => BuildResetView(),
            "speed" => BuildSpeedTestView(),
            "monitor" => BuildMonitorView(),
            "history" => BuildHistoryView(),
            "logs" => BuildLogsView(),
            "settings" => BuildSettingsView(),
            "about" => BuildAboutView(),
            _ => BuildDashboardView()
        };

        viewHost.Controls.Clear();
        view.Dock = DockStyle.Fill;
        viewHost.Controls.Add(view);
        Animator.Run(220, value => viewHost.Padding = new Padding(22, 22 + (int)((1 - value) * 12), 22, 22));
    }

    private Control BuildHeader(string title, string subtitle)
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = Color.Transparent };
        var titleLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 36,
            Text = title,
            ForeColor = Palette.Text,
            Font = UiFonts.Create(21F, FontStyle.Bold),
            TextAlign = ContentAlignment.BottomLeft
        };
        var sub = new Label
        {
            Dock = DockStyle.Fill,
            Text = subtitle,
            ForeColor = Palette.MutedText,
            Font = UiFonts.Create(10F),
            TextAlign = ContentAlignment.TopLeft
        };
        panel.Controls.Add(sub);
        panel.Controls.Add(titleLabel);
        return panel;
    }

    private Control BuildDiagnoseView()
    {
        var root = CreateViewRoot();
        root.Controls.Add(BuildHeader(L.T("Diagnose.Title"), L.HeroSubtitle));
        var card = new RoundedPanel { Dock = DockStyle.Top, Height = 320, Radius = 8, Shadow = true, FillColor = Palette.Surface };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5, BackColor = Color.Transparent };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        for (var i = 0; i < 5; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 4 ? 72 : 52));

        var problemBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiFonts.Create(10F) };
        problemBox.Items.AddRange(problems.Select(problem => problem.Title).Cast<object>().ToArray());
        problemBox.SelectedIndex = 0;
        var hostBox = new TextBox { Dock = DockStyle.Fill, Text = problems[0].DefaultHost, Font = UiFonts.Create(10F), BorderStyle = BorderStyle.FixedSingle };
        var portBox = new NumericUpDown { Dock = DockStyle.Left, Width = 120, Minimum = 1, Maximum = 65535, Value = problems[0].DefaultPort, Font = UiFonts.Create(10F) };
        problemBox.SelectedIndexChanged += (_, _) =>
        {
            var selected = problems[Math.Max(0, problemBox.SelectedIndex)];
            hostBox.Text = selected.DefaultHost;
            portBox.Value = selected.DefaultPort;
        };

        AddFormRow(layout, 0, L.T("Diagnose.Problem"), problemBox);
        AddFormRow(layout, 1, L.T("Diagnose.Host"), hostBox);
        AddFormRow(layout, 2, L.T("Diagnose.Port"), portBox);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, FlowDirection = FlowDirection.LeftToRight };
        var manual = CreatePill($"{Glyphs.Play} {L.T("Diagnose.Run")}", true);
        manual.Width = 180;
        manual.Click += async (_, _) => await RunDiagnosisAsync(false, problems[Math.Max(0, problemBox.SelectedIndex)], hostBox.Text, (int)portBox.Value);
        var easy = CreatePill($"{Glyphs.Shield} {L.T("Diagnose.EasyRun")}");
        easy.Width = 250;
        easy.Click += async (_, _) => await RunDiagnosisAsync(true, problems[Math.Max(0, problemBox.SelectedIndex)], hostBox.Text, (int)portBox.Value);
        actions.Controls.Add(manual);
        actions.Controls.Add(easy);
        layout.Controls.Add(actions, 1, 4);
        card.Controls.Add(layout);
        root.Controls.Add(card);
        return root;
    }

    private Control BuildDashboardView()
    {
        var root = CreateViewRoot();
        root.Controls.Add(BuildHeader(L.T("Dashboard.Title"), L.T("Dashboard.Subtitle")));
        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.Transparent };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 38));

        var kpis = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, BackColor = Color.Transparent, Padding = new Padding(0, 0, 0, 12) };
        for (var i = 0; i < 4; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        kpiLocal = new KpiCard(L.T("Dashboard.KpiLocal"));
        kpiInternational = new KpiCard(L.T("Dashboard.KpiInternational"));
        kpiDns = new KpiCard(L.T("Dashboard.KpiDns"));
        kpiQuality = new KpiCard(L.T("Dashboard.KpiQuality"));
        kpis.Controls.Add(kpiLocal, 0, 0);
        kpis.Controls.Add(kpiInternational, 1, 0);
        kpis.Controls.Add(kpiDns, 2, 0);
        kpis.Controls.Add(kpiQuality, 3, 0);

        var actionBar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, FlowDirection = FlowDirection.LeftToRight };
        var back = CreatePill(L.T("Common.Back"));
        back.Click += (_, _) => Navigate("diagnose");
        var runAgain = CreatePill($"{Glyphs.Play} {L.T("Dashboard.RunAgain")}", true);
        runAgain.Width = 146;
        runAgain.Click += async (_, _) => await RunDiagnosisAsync(false);
        undoButton = CreatePill($"{Glyphs.Undo} {L.T("Dashboard.Undo")}");
        undoButton.Width = 120;
        undoButton.Click += async (_, _) => await UndoAsync();
        fixButton = CreatePill($"{Glyphs.Shield} {L.T("Dashboard.FixSafely")}", true);
        fixButton.Width = 158;
        fixButton.Click += async (_, _) => await ApplySafeFixAsync();
        var reset = CreatePill($"{Glyphs.Reset} {L.T("Dashboard.NetworkReset")}");
        reset.Width = 160;
        reset.Click += (_, _) => Navigate("reset");
        actionBar.Controls.Add(back);
        actionBar.Controls.Add(runAgain);
        actionBar.Controls.Add(undoButton);
        actionBar.Controls.Add(fixButton);
        actionBar.Controls.Add(reset);

        var cardsGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3, BackColor = Color.Transparent };
        for (var i = 0; i < 3; i++)
        {
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333f));
        }
        statusCards.Clear();
        foreach (var info in new[]
        {
            ("adapter", L.T("Check.Adapter.Title")), ("gateway", L.T("Check.Gateway.Title")), ("internet", L.T("Check.Internet.Title")),
            ("dns", L.T("Check.Dns.Title")), ("packet-loss", L.T("Check.PacketLoss.Title")), ("port", L.T("Check.Port.Title")),
            ("vpn", L.T("Check.Vpn.Title")), ("proxy", L.T("Check.Proxy.Title")), ("hosts", L.T("Check.HostsFile.Title"))
        })
        {
            var card = new StatusCard(info.Item1, info.Item2) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 12) };
            card.SetPending();
            statusCards[info.Item1] = card;
            cardsGrid.Controls.Add(card);
        }

        var details = new CollapsiblePanel(L.T("Dashboard.Details")) { Dock = DockStyle.Fill, Height = 220 };
        detailsBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.None,
            BackColor = Palette.Surface,
            ForeColor = Palette.Text,
            Font = new Font(FontFamily.GenericMonospace, 9F)
        };
        details.Body.Controls.Add(detailsBox);
        main.Controls.Add(kpis, 0, 0);
        main.Controls.Add(actionBar, 0, 1);
        main.Controls.Add(cardsGrid, 0, 2);
        main.Controls.Add(details, 0, 3);
        root.Controls.Add(main);
        RefreshDashboard(lastReport);
        return root;
    }
    private Control BuildResetView()
    {
        var root = CreateViewRoot();
        root.Controls.Add(BuildHeader(L.T("Reset.Title"), L.T("Undo.NotAvailable.Body")));
        var grid = new TableLayoutPanel { Dock = DockStyle.Top, Height = 270, ColumnCount = 2, BackColor = Color.Transparent };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.Controls.Add(BuildResetCard(L.T("Reset.Safe"), L.T("Reset.SafeBody"), "quick-refresh"), 0, 0);
        grid.Controls.Add(BuildResetCard(L.T("Reset.Deep"), L.T("Reset.DeepBody"), "deep-repair"), 1, 0);
        root.Controls.Add(grid);
        return root;
    }

    private Control BuildResetCard(string title, string body, string fixId)
    {
        var panel = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 14, 0), FillColor = Palette.Surface, Shadow = true };
        var titleLabel = new Label { Dock = DockStyle.Top, Height = 34, Text = title, ForeColor = Palette.Text, Font = UiFonts.Create(16F, FontStyle.Bold) };
        var bodyLabel = new Label { Dock = DockStyle.Top, Height = 100, Text = body, ForeColor = Palette.MutedText, Font = UiFonts.Create(10F) };
        var action = CreatePill((FixRegistry.RequiresAdmin(fixId) ? $"{Glyphs.Shield} " : string.Empty) + L.T("Common.Start"), fixId != "deep-repair");
        action.Width = 170;
        action.Click += async (_, _) => await ApplyFixByIdAsync(fixId, strongWarning: fixId == "deep-repair");
        panel.Controls.Add(action);
        panel.Controls.Add(bodyLabel);
        panel.Controls.Add(titleLabel);
        return panel;
    }

    private Control BuildSpeedTestView()
    {
        var root = CreateViewRoot();
        root.Controls.Add(BuildHeader(L.T("Speed.Title"), "Latency, jitter and download with cancel/fallback support."));
        var card = new RoundedPanel { Dock = DockStyle.Top, Height = 260, FillColor = Palette.Surface, Shadow = true };
        var spark = new Sparkline { Dock = DockStyle.Bottom, Height = 88 };
        var result = new Label { Dock = DockStyle.Top, Height = 80, Text = "--", ForeColor = Palette.Text, Font = UiFonts.Create(18F, FontStyle.Bold) };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, BackColor = Color.Transparent };
        var start = CreatePill(L.T("Common.Start"), true);
        var cancel = CreatePill(L.T("Common.Cancel"));
        cancel.Enabled = false;
        start.Click += async (_, _) =>
        {
            speedCancellation = new CancellationTokenSource();
            start.Enabled = false;
            cancel.Enabled = true;
            try
            {
                var speed = await speedTestService.RunAsync(settings, new Progress<string>(_ => statusBar.Text = L.T("Speed.Fallback")), speedCancellation.Token);
                result.Text = $"{L.T("Speed.Latency")}: {speed.LatencyMs:n0} ms    {L.T("Speed.Jitter")}: {speed.JitterMs:n0} ms    {L.T("Speed.Download")}: {speed.DownloadMbps:n1} Mbps";
                spark.Push((float)speed.LatencyMs);
            }
            catch (OperationCanceledException)
            {
                result.Text = L.DiagnosisCancelled;
            }
            catch (Exception ex)
            {
                result.Text = ex.Message;
            }
            finally
            {
                start.Enabled = true;
                cancel.Enabled = false;
            }
        };
        cancel.Click += (_, _) => speedCancellation?.Cancel();
        actions.Controls.Add(start);
        actions.Controls.Add(cancel);
        card.Controls.Add(spark);
        card.Controls.Add(actions);
        card.Controls.Add(result);
        root.Controls.Add(card);
        return root;
    }

    private Control BuildMonitorView()
    {
        var root = CreateViewRoot();
        root.Controls.Add(BuildHeader(L.T("Monitor.Title"), "Adapter refreshes on Windows network change events."));
        var card = new RoundedPanel { Dock = DockStyle.Top, Height = 290, FillColor = Palette.Surface, Shadow = true };
        var spark = new Sparkline { Dock = DockStyle.Bottom, Height = 100 };
        var info = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Palette.Surface, ForeColor = Palette.Text, Font = new Font(FontFamily.GenericMonospace, 9F) };
        var refresh = CreatePill(L.T("Common.Refresh"), true);
        refresh.Dock = DockStyle.Top;
        refresh.Click += async (_, _) => await RefreshMonitorAsync(info, spark);
        card.Controls.Add(info);
        card.Controls.Add(spark);
        card.Controls.Add(refresh);
        root.Controls.Add(card);
        _ = RefreshMonitorAsync(info, spark);
        return root;
    }

    private Control BuildHistoryView()
    {
        var root = CreateViewRoot();
        root.Controls.Add(BuildHeader(L.T("History.Title"), "Saved diagnosis sessions can be reopened, exported or deleted."));
        historyList = new ListBox { Dock = DockStyle.Fill, BackColor = Palette.Surface, ForeColor = Palette.Text, BorderStyle = BorderStyle.None, Font = UiFonts.Create(10F) };
        RefreshHistoryList();
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 58, BackColor = Color.Transparent };
        foreach (var format in new[] { "txt", "json", "html" })
        {
            var export = CreatePill($"{Glyphs.Export} {format.ToUpperInvariant()}");
            export.Width = 108;
            export.Click += async (_, _) => await ExportSelectedHistoryAsync(format);
            actions.Controls.Add(export);
        }
        var delete = CreatePill(L.T("Common.Delete"));
        delete.Danger = true;
        delete.Click += (_, _) => DeleteSelectedHistory();
        actions.Controls.Add(delete);
        root.Controls.Add(historyList);
        root.Controls.Add(actions);
        return root;
    }

    private Control BuildLogsView()
    {
        var root = CreateViewRoot();
        root.Controls.Add(BuildHeader(L.T("Logs.Title"), L.T("Logs.Today")));
        var box = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BorderStyle = BorderStyle.None, BackColor = Palette.Surface, ForeColor = Palette.Text, Font = new Font(FontFamily.GenericMonospace, 9F) };
        box.Text = File.Exists(PathService.TodayLogFile) ? File.ReadAllText(PathService.TodayLogFile) : string.Empty;
        var open = CreatePill(L.T("Logs.OpenFolder"), true);
        open.Dock = DockStyle.Bottom;
        open.Click += (_, _) => Process.Start(new ProcessStartInfo(PathService.LogsDirectory) { UseShellExecute = true });
        root.Controls.Add(box);
        root.Controls.Add(open);
        return root;
    }
    private Control BuildSettingsView()
    {
        var root = CreateViewRoot();
        root.Controls.Add(BuildHeader(L.T("Settings.Title"), "Language, region, theme, tray and auto-start."));
        var card = new RoundedPanel { Dock = DockStyle.Top, Height = 360, FillColor = Palette.Surface, Shadow = true };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, BackColor = Color.Transparent };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        for (var i = 0; i < 6; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        var language = Combo(["en", "de", "fa", "ar"], settings.Language);
        var region = Combo(["World", "Iran"], settings.Region);
        var theme = Combo(["System", "Light", "Dark"], settings.Theme);
        var reduced = new CheckBox { Dock = DockStyle.Fill, Text = L.T("Settings.ReducedMotion"), Checked = settings.ReducedMotion, ForeColor = Palette.Text, Font = UiFonts.Create(10F) };
        var tray = new CheckBox { Dock = DockStyle.Fill, Text = L.T("Settings.MinimizeToTray"), Checked = settings.MinimizeToTrayOnClose, ForeColor = Palette.Text, Font = UiFonts.Create(10F) };
        var auto = new CheckBox { Dock = DockStyle.Fill, Text = L.T("Settings.AutoStart"), Checked = AutoStartService.IsEnabled(), ForeColor = Palette.Text, Font = UiFonts.Create(10F) };
        AddFormRow(layout, 0, L.T("Settings.Language"), language);
        AddFormRow(layout, 1, L.T("Settings.Region"), region);
        AddFormRow(layout, 2, L.T("Settings.Theme"), theme);
        layout.Controls.Add(reduced, 1, 3);
        layout.Controls.Add(tray, 1, 4);
        layout.Controls.Add(auto, 1, 5);
        var save = CreatePill(L.T("Common.Save"), true);
        save.Dock = DockStyle.Bottom;
        save.Click += (_, _) =>
        {
            settings = settings with
            {
                Language = language.Text,
                Region = region.Text,
                Theme = theme.Text,
                ReducedMotion = reduced.Checked,
                MinimizeToTrayOnClose = tray.Checked,
                AutoStartEnabled = auto.Checked,
                FirstRunCompleted = true
            };
            SettingsService.Save(settings);
            AutoStartService.SetEnabled(auto.Checked);
            AppConfig.Apply(settings.Language, settings.Region, settings.Theme, settings.ReducedMotion);
            ThemeManager.Apply(AppConfig.Theme);
            toast.ShowMessage(this, "Settings saved. Restart the app to fully reload localized legacy strings.");
            BuildShell();
            Navigate("settings");
        };
        card.Controls.Add(layout);
        root.Controls.Add(save);
        root.Controls.Add(card);
        return root;
    }

    private Control BuildAboutView()
    {
        var root = CreateViewRoot();
        root.Controls.Add(BuildHeader(L.T("About.Title"), string.Format(L.T("About.Version"), AppConfig.Version)));
        var card = new RoundedPanel { Dock = DockStyle.Top, Height = 220, FillColor = Palette.Surface, Shadow = true };
        var text = new Label
        {
            Dock = DockStyle.Fill,
            Text = $"Net Doctor v0.5.0\n\n{L.T("About.Deferred")}\n\nLicense path: {PathService.LicenseFile}\nReports: {PathService.ReportsDirectory}",
            ForeColor = Palette.Text,
            Font = UiFonts.Create(10.5F),
            TextAlign = ContentAlignment.MiddleLeft
        };
        card.Controls.Add(text);
        root.Controls.Add(card);
        return root;
    }

    private Panel CreateViewRoot()
    {
        return new Panel { Dock = DockStyle.Fill, BackColor = Palette.AppBackground, Padding = new Padding(0) };
    }

    private async Task RunDiagnosisAsync(bool easyMode, ProblemDefinition? problem = null, string? host = null, int? port = null)
    {
        problem ??= problems[0];
        host ??= problem.DefaultHost;
        port ??= problem.DefaultPort;
        Navigate("dashboard");
        statusBar.Text = L.RunningDiagnosis;
        diagnosisCancellation?.Cancel();
        diagnosisCancellation = new CancellationTokenSource();
        foreach (var card in statusCards.Values)
        {
            card.SetRunning(L.RunningDiagnosis);
        }

        try
        {
            var progress = new Progress<DiagnosticProgress>(item =>
            {
                var id = item.Check.Id == "quality" ? "packet-loss" : item.Check.Id;
                if (statusCards.TryGetValue(id, out var card))
                {
                    card.SetCheck(item.Check);
                }
            });
            var legacy = await new NetworkDiagnosticService().RunAsync(problem.Title, new PortProbeRequest(host, port.Value), progress, diagnosisCancellation.Token);
            lastReport = legacy;
            await diagnosisEngine.RunAsync(problem.Title, new PortProbeRequest(host, port.Value), null, diagnosisCancellation.Token);
            await historyService.SaveAsync(legacy, CancellationToken.None);
            LogService.Info($"Diagnosis finished: {legacy.PlainLanguageSummary}");
            RefreshDashboard(legacy);
            statusBar.Text = legacy.HasSafeFix ? L.DiagnosisCompleteFixAvailable : L.DiagnosisComplete;
            if (easyMode && legacy.HasSafeFix)
            {
                await ApplySafeFixAsync();
            }
        }
        catch (OperationCanceledException)
        {
            statusBar.Text = L.DiagnosisCancelled;
        }
        catch (Exception ex)
        {
            statusBar.Text = L.DiagnosisFailed;
            LogService.Error("Diagnosis failed", ex);
            toast.ShowMessage(this, ex.Message);
        }
    }

    private void RefreshDashboard(DiagnosticReport? report)
    {
        if (kpiLocal is null) return;
        if (report is null)
        {
            kpiLocal.SetValue("--", L.WaitingToRun, Palette.MutedText);
            kpiInternational!.SetValue("--", L.WaitingToRun, Palette.MutedText);
            kpiDns!.SetValue("--", L.WaitingToRun, Palette.MutedText);
            kpiQuality!.SetValue("--", L.WaitingToRun, Palette.MutedText);
            fixButton!.Enabled = false;
            undoButton!.Enabled = safeFixService.HasUndoSnapshot;
            return;
        }

        foreach (var check in report.Checks)
        {
            var id = check.Id == "quality" ? "packet-loss" : check.Id;
            if (statusCards.TryGetValue(id, out var card)) card.SetCheck(check);
        }

        SetKpi(kpiLocal, report.Checks.FirstOrDefault(c => c.Id == "local"));
        SetKpi(kpiInternational!, report.Checks.FirstOrDefault(c => c.Id == "international"));
        SetKpi(kpiDns!, report.Checks.FirstOrDefault(c => c.Id == "dns"));
        SetKpi(kpiQuality!, report.Checks.FirstOrDefault(c => c.Id == "quality"));
        detailsBox!.Text = BuildDetails(report);
        fixButton!.Enabled = report.HasSafeFix;
        fixButton.Text = FixRegistry.RequiresAdmin(report.SafeFixKey) ? $"{Glyphs.Shield} {L.T("Dashboard.FixSafely")}" : L.T("Dashboard.FixSafely");
        undoButton!.Enabled = safeFixService.HasUndoSnapshot;
    }

    private static void SetKpi(KpiCard card, DiagnosticCheck? check)
    {
        if (check is null)
        {
            card.SetValue("--", L.WaitingToRun, Palette.MutedText);
            return;
        }
        var value = check.Status switch
        {
            CheckStatus.Healthy => "OK",
            CheckStatus.Warning => "WARN",
            CheckStatus.Failed => "FAIL",
            _ => "..."
        };
        var color = check.Status switch
        {
            CheckStatus.Healthy => Palette.Green,
            CheckStatus.Warning => Palette.Amber,
            CheckStatus.Failed => Palette.Red,
            _ => Palette.Accent
        };
        card.SetValue(value, check.Summary, color);
    }

    private static string BuildDetails(DiagnosticReport report)
    {
        var lines = new List<string> { report.PlainLanguageSummary, string.Empty };
        foreach (var check in report.Checks)
        {
            lines.Add($"[{check.Status}] {check.Title}: {check.Summary}");
            lines.AddRange(check.Details.Select(detail => $"  - {detail}"));
            lines.Add(string.Empty);
        }
        return string.Join(Environment.NewLine, lines);
    }
    private async Task ApplySafeFixAsync()
    {
        if (lastReport?.SafeFixKey is null)
        {
            toast.ShowMessage(this, L.AllHealthyNoFix);
            return;
        }
        await ApplyFixByIdAsync(lastReport.SafeFixKey, strongWarning: FixRegistry.Find(lastReport.SafeFixKey)?.RiskLevel == RiskLevel.High);
    }

    private async Task ApplyFixByIdAsync(string fixId, bool strongWarning)
    {
        var metadata = FixRegistry.Find(fixId);
        var message = metadata is null ? SafeFixService.Describe(fixId) : $"{SafeFixService.Describe(fixId)}\n\nRisk: {metadata.RiskLevel}\nUndo: {(metadata.IsUndoable ? "available" : "not available")}";
        if (strongWarning)
        {
            message += $"\n\n{L.T("Undo.NotAvailable.Body")}";
        }
        if (MessageBox.Show(message, L.ConfirmContinue, MessageBoxButtons.OKCancel, strongWarning ? MessageBoxIcon.Warning : MessageBoxIcon.Information) != DialogResult.OK)
        {
            return;
        }

        try
        {
            statusBar.Text = L.RunningFix;
            string result;
            if (fixId == "dns-safe-public")
            {
                using var dialog = new DnsPresetDialog();
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedPreset is null)
                {
                    return;
                }
                await new NetworkSnapshotService().CaptureAsync(fixId, CancellationToken.None);
                result = await safeFixService.ApplyDnsPresetAsync(dialog.SelectedPreset, CancellationToken.None);
            }
            else
            {
                result = await safeFixService.ApplyAsync(fixId, CancellationToken.None);
            }
            LogService.Info($"Fix {fixId}: {result}");
            toast.ShowMessage(this, result);
        }
        catch (Exception ex)
        {
            LogService.Error($"Fix failed: {fixId}", ex);
            toast.ShowMessage(this, ex.Message);
        }
        finally
        {
            statusBar.Text = L.Ready;
            RefreshDashboard(lastReport);
        }
    }

    private async Task UndoAsync()
    {
        if (!safeFixService.HasUndoSnapshot)
        {
            MessageBox.Show(L.T("Undo.NotAvailable.Body"), L.T("Undo.NotAvailable.Title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (MessageBox.Show(L.UndoConfirm, L.ConfirmContinue, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
        {
            return;
        }
        var result = await safeFixService.UndoAsync(CancellationToken.None);
        LogService.Info($"Undo: {result}");
        toast.ShowMessage(this, result);
        RefreshDashboard(lastReport);
    }

    private void RefreshHistoryList()
    {
        if (historyList is null) return;
        historyList.Items.Clear();
        foreach (var file in historyService.ListSessionFiles())
        {
            historyList.Items.Add(file);
        }
        if (historyList.Items.Count == 0) historyList.Items.Add(L.T("History.Empty"));
    }

    private async Task ExportSelectedHistoryAsync(string format)
    {
        if (historyList?.SelectedItem is not string path || !File.Exists(path)) return;
        var report = historyService.Open(path);
        if (report is null) return;
        var exported = await reportService.ExportAsync(report, format, CancellationToken.None);
        toast.ShowMessage(this, exported);
    }

    private void DeleteSelectedHistory()
    {
        if (historyList?.SelectedItem is not string path || !File.Exists(path)) return;
        historyService.Delete(path);
        RefreshHistoryList();
    }

    private async Task RefreshMonitorAsync(TextBox info, Sparkline spark)
    {
        var adapter = new NetworkDiagnosticService().FindPrimaryAdapter();
        info.Text = adapter is null
            ? "No primary adapter found."
            : $"{adapter.Name}\r\n{adapter.Description}\r\n{adapter.OperationalStatus}\r\nSpeed: {adapter.Speed / 1_000_000:n0} Mbps";
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("1.1.1.1", 2000);
            if (reply.Status == IPStatus.Success) spark.Push(reply.RoundtripTime);
        }
        catch
        {
            spark.Push(0);
        }
    }

    private void OnNetworkChanged(object? sender, EventArgs e)
    {
        if (!IsHandleCreated) return;
        BeginInvoke(new Action(() => statusBar.Text = L.T("Monitor.NetworkChanged")));
    }

    private void ToggleTheme()
    {
        ThemeManager.Toggle();
        settings = settings with { Theme = ThemeManager.Current == ThemeMode.Dark ? "Dark" : "Light" };
        SettingsService.Save(settings);
        AppConfig.Apply(settings.Language, settings.Region, settings.Theme, settings.ReducedMotion);
        BuildShell();
        Navigate("dashboard");
    }

    private void UpdateLicensePill()
    {
        var check = new LicenseManager().CheckStored();
        licensePill.Text = check.Status switch
        {
            LicenseStatus.Valid when check.Info?.DaysRemaining <= 5 => L.T("TopBar.LicenseExpiring"),
            LicenseStatus.Valid => L.T("TopBar.LicenseActive"),
            LicenseStatus.Expired => L.T("TopBar.LicenseExpired"),
            _ => L.T("TopBar.LicenseMissing")
        };
        licensePill.BackColor = check.Status switch
        {
            LicenseStatus.Valid when check.Info?.DaysRemaining <= 5 => Palette.Amber,
            LicenseStatus.Valid => Palette.Green,
            LicenseStatus.Expired => Palette.Red,
            _ => Palette.LightButton
        };
        licensePill.ForeColor = check.Status == LicenseStatus.Missing ? Palette.Text : Color.White;
    }

    private void BuildTray()
    {
        trayIcon.Icon = Icon ?? SystemIcons.Application;
        trayIcon.Text = L.AppTitle;
        trayIcon.Visible = true;
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => { Show(); WindowState = FormWindowState.Normal; Activate(); });
        menu.Items.Add("Quick Check", null, async (_, _) => await RunDiagnosisAsync(true));
        menu.Items.Add("Settings", null, (_, _) => { Show(); Navigate("settings"); });
        menu.Items.Add("Exit", null, (_, _) => { allowExit = true; Close(); });
        trayIcon.ContextMenuStrip = menu;
        trayIcon.DoubleClick += (_, _) => { Show(); WindowState = FormWindowState.Normal; Activate(); };
    }

    private static ComboBox Combo(string[] values, string selected)
    {
        var combo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiFonts.Create(10F) };
        combo.Items.AddRange(values.Cast<object>().ToArray());
        combo.SelectedItem = values.FirstOrDefault(v => v.Equals(selected, StringComparison.OrdinalIgnoreCase)) ?? values[0];
        return combo;
    }

    private static void AddFormRow(TableLayoutPanel layout, int row, string label, Control control)
    {
        var lbl = new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Palette.MutedText, Font = UiFonts.Create(9.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        layout.Controls.Add(lbl, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private static Padding P(int left, int top, int right, int bottom)
    {
        return AppConfig.IsRtl ? new Padding(right, top, left, bottom) : new Padding(left, top, right, bottom);
    }

    private static Icon? TryLoadIcon()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "assets", "netdoctor-icon.ico");
        return File.Exists(path) ? new Icon(path) : null;
    }
}

internal sealed record ProblemDefinition(string Title, string Description, string DefaultHost, int DefaultPort)
{
    public static List<ProblemDefinition> Defaults() =>
    [
        new("Websites do not open", "General browsing and DNS reachability", "example.com", 443),
        new("DNS problem", "Name resolution is slow or broken", "cloudflare.com", 443),
        new("VPN or proxy issue", "VPN/proxy may be changing routes", "one.one.one.one", 443),
        new("A specific port is blocked", "Advanced TCP port probe", "example.com", 443),
        new("Internet is unstable", "Latency and packet loss investigation", "1.1.1.1", 443)
    ];
}
