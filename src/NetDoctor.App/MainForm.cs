using NetDoctor.App.Models;
using NetDoctor.App.Services;
using System.Drawing.Drawing2D;

namespace NetDoctor.App;

internal sealed class MainForm : Form
{
    private readonly NetworkDiagnosticService diagnosticService = new();
    private readonly SafeFixService safeFixService = new();
    private readonly Dictionary<string, StatusCard> cards = new();
    private readonly List<ProblemDefinition> problems = ProblemDefinition.Defaults();

    private readonly Panel startPanel = new();
    private readonly Panel dashboardPanel = new();
    private readonly Label stateLabel = new();
    private readonly Label summaryLabel = new();
    private readonly Label selectedProblemLabel = new();
    private readonly Label fixHintLabel = new();
    private readonly TextBox detailsBox = new();
    private readonly TextBox hostBox = new();
    private readonly NumericUpDown portBox = new();
    private readonly ProgressBar progressBar = new();
    private readonly Button startButton = new();
    private readonly Button runAgainButton = new();
    private readonly Button backButton = new();
    private readonly Button fixButton = new();
    private readonly Button undoButton = new();

    private DiagnosticReport? lastReport;
    private ProblemDefinition selectedProblem;
    private CancellationTokenSource? runCancellation;

    public MainForm()
    {
        selectedProblem = problems[0];
        Text = "Net Doctor";
        MinimumSize = new Size(1080, 720);
        Size = new Size(1240, 820);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Palette.AppBackground;
        Font = new Font("Segoe UI", 10F);
        RightToLeft = RightToLeft.No;
        RightToLeftLayout = false;
        Icon = TryLoadIcon();

        BuildStartPanel();
        BuildDashboardPanel();
        Controls.Add(dashboardPanel);
        Controls.Add(startPanel);
        dashboardPanel.Visible = false;
        RefreshProblemFields();
        RefreshUndoButton();
    }

    private void BuildStartPanel()
    {
        startPanel.Dock = DockStyle.Fill;
        startPanel.BackColor = Palette.AppBackground;
        startPanel.Padding = new Padding(28);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        startPanel.Controls.Add(layout);

        var hero = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            BackColor = Palette.Navy,
            Padding = new Padding(28)
        };
        layout.Controls.Add(hero, 0, 0);

        var heroLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        heroLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
        heroLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        heroLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        heroLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
        hero.Controls.Add(heroLayout);

        var logoBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = TryLoadImage("netdoctor-logo.png")
        };
        heroLayout.Controls.Add(logoBox, 0, 0);

        var title = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Net Doctor",
            Font = new Font("Segoe UI Semibold", 30F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.BottomLeft
        };
        heroLayout.Controls.Add(title, 0, 1);

        var subtitle = new Label
        {
            Dock = DockStyle.Fill,
            Text = "A friendly network diagnosis tool that explains what is broken and offers reversible repairs.",
            Font = new Font("Segoe UI", 12F),
            ForeColor = Color.FromArgb(210, 232, 242),
            TextAlign = ContentAlignment.TopLeft
        };
        heroLayout.Controls.Add(subtitle, 0, 2);

        var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(22, 0, 0, 0), BackColor = Palette.AppBackground };
        layout.Controls.Add(content, 1, 0);

        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        content.Controls.Add(right);

        var heading = new Label
        {
            Dock = DockStyle.Fill,
            Text = "What problem are you having?",
            Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold),
            ForeColor = Palette.Text,
            TextAlign = ContentAlignment.MiddleLeft
        };
        right.Controls.Add(heading, 0, 0);

        var problemGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4
        };
        problemGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        problemGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (var i = 0; i < 4; i++)
        {
            problemGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        }
        right.Controls.Add(problemGrid, 0, 1);

        foreach (var problem in problems)
        {
            var tile = new ProblemTile(problem)
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 12, 12)
            };
            tile.Click += (_, _) => SelectProblem(problem, problemGrid);
            foreach (Control child in tile.Controls)
            {
                child.Click += (_, _) => SelectProblem(problem, problemGrid);
            }
            problemGrid.Controls.Add(tile);
        }

        var advanced = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            BackColor = Color.White,
            Padding = new Padding(16)
        };
        right.Controls.Add(advanced, 0, 2);

        var advancedGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2
        };
        advancedGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        advancedGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        advancedGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        advancedGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        advancedGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        advancedGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        advanced.Controls.Add(advancedGrid);

        AddSmallLabel(advancedGrid, "Host or website", 0);
        AddSmallLabel(advancedGrid, "Port", 1);
        AddSmallLabel(advancedGrid, "Selected issue", 2);
        hostBox.Dock = DockStyle.Fill;
        portBox.Dock = DockStyle.Fill;
        portBox.Minimum = 1;
        portBox.Maximum = 65535;
        selectedProblemLabel.Dock = DockStyle.Fill;
        selectedProblemLabel.TextAlign = ContentAlignment.MiddleLeft;
        selectedProblemLabel.ForeColor = Palette.MutedText;

        startButton.Text = "Start diagnosis";
        startButton.Dock = DockStyle.Fill;
        startButton.FlatStyle = FlatStyle.Flat;
        startButton.FlatAppearance.BorderSize = 0;
        startButton.BackColor = Palette.Blue;
        startButton.ForeColor = Color.White;
        startButton.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        startButton.Click += async (_, _) => await StartDiagnosticsAsync();

        advancedGrid.Controls.Add(hostBox, 0, 1);
        advancedGrid.Controls.Add(portBox, 1, 1);
        advancedGrid.Controls.Add(selectedProblemLabel, 2, 1);
        advancedGrid.Controls.Add(startButton, 3, 1);

        var note = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Net Doctor will compare local-country access with international access, then check DNS, quality, ports, VPN and proxy.",
            ForeColor = Palette.MutedText,
            TextAlign = ContentAlignment.MiddleLeft
        };
        right.Controls.Add(note, 0, 3);

        SelectProblem(selectedProblem, problemGrid);
    }

    private void BuildDashboardPanel()
    {
        dashboardPanel.Dock = DockStyle.Fill;
        dashboardPanel.BackColor = Palette.AppBackground;
        dashboardPanel.Padding = new Padding(24);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 56));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 44));
        dashboardPanel.Controls.Add(root);

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 430));
        root.Controls.Add(header, 0, 0);

        header.Controls.Add(new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = TryLoadImage("netdoctor-icon-512.png")
        }, 0, 0);

        var headerText = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        headerText.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
        headerText.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
        header.Controls.Add(headerText, 1, 0);
        headerText.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "Net Doctor",
            Font = new Font("Segoe UI Semibold", 25F, FontStyle.Bold),
            ForeColor = Palette.Text,
            TextAlign = ContentAlignment.BottomLeft
        }, 0, 0);
        stateLabel.Dock = DockStyle.Fill;
        stateLabel.Text = "Ready";
        stateLabel.ForeColor = Palette.MutedText;
        stateLabel.TextAlign = ContentAlignment.TopLeft;
        headerText.Controls.Add(stateLabel, 0, 1);

        var actions = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Padding = new Padding(0, 28, 0, 0)
        };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27));
        header.Controls.Add(actions, 2, 0);
        ConfigureActionButton(backButton, "Back", Palette.LightButton, Palette.Text);
        ConfigureActionButton(runAgainButton, "Run again", Palette.LightButton, Palette.Text);
        ConfigureActionButton(fixButton, "Fix Safely", Palette.Green, Color.White);
        ConfigureActionButton(undoButton, "Undo", Palette.LightButton, Palette.Text);
        backButton.Click += (_, _) => ShowStart();
        runAgainButton.Click += async (_, _) => await StartDiagnosticsAsync();
        fixButton.Click += async (_, _) => await ApplySafeFixAsync();
        undoButton.Click += async (_, _) => await UndoAsync();
        actions.Controls.Add(backButton, 0, 0);
        actions.Controls.Add(undoButton, 1, 0);
        actions.Controls.Add(runAgainButton, 2, 0);
        actions.Controls.Add(fixButton, 3, 0);

        var summaryPanel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            BackColor = Color.White,
            Padding = new Padding(18)
        };
        root.Controls.Add(summaryPanel, 0, 1);
        var summaryLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
        summaryPanel.Controls.Add(summaryLayout);
        summaryLabel.Dock = DockStyle.Fill;
        summaryLabel.Text = "Choose a problem and start diagnosis.";
        summaryLabel.Font = new Font("Segoe UI Semibold", 12.5F, FontStyle.Bold);
        summaryLabel.ForeColor = Palette.Text;
        summaryLabel.TextAlign = ContentAlignment.MiddleLeft;
        summaryLayout.Controls.Add(summaryLabel, 0, 0);
        progressBar.Dock = DockStyle.Fill;
        progressBar.Maximum = 8;
        progressBar.Style = ProgressBarStyle.Continuous;
        summaryLayout.Controls.Add(progressBar, 0, 1);

        var cardsGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            Padding = new Padding(0, 18, 0, 10)
        };
        for (var i = 0; i < 4; i++)
        {
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }
        cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        root.Controls.Add(cardsGrid, 0, 2);

        foreach (var (id, title) in new[]
        {
            ("adapter", "Network adapter"),
            ("local", "Local internet"),
            ("international", "International internet"),
            ("dns", "DNS"),
            ("quality", "Connection quality"),
            ("port", "Port access"),
            ("vpn", "VPN"),
            ("proxy", "Proxy")
        })
        {
            var card = new StatusCard { Dock = DockStyle.Fill, Margin = new Padding(7), Title = title };
            cards[id] = card;
            cardsGrid.Controls.Add(card);
        }

        var detailPanel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            BackColor = Color.White,
            Padding = new Padding(18)
        };
        root.Controls.Add(detailPanel, 0, 3);
        var detailLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        detailPanel.Controls.Add(detailLayout);
        detailLayout.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "Technical details",
            Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
            ForeColor = Palette.Text,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        fixHintLabel.Dock = DockStyle.Fill;
        fixHintLabel.Text = "Fix Safely runs a reversible repair when one is available, or a harmless network refresh for uncertain results.";
        fixHintLabel.ForeColor = Palette.MutedText;
        fixHintLabel.TextAlign = ContentAlignment.MiddleLeft;
        detailLayout.Controls.Add(fixHintLabel, 0, 1);
        detailsBox.Dock = DockStyle.Fill;
        detailsBox.Multiline = true;
        detailsBox.ScrollBars = ScrollBars.Vertical;
        detailsBox.ReadOnly = true;
        detailsBox.BorderStyle = BorderStyle.None;
        detailsBox.BackColor = Color.White;
        detailsBox.ForeColor = Color.FromArgb(38, 50, 65);
        detailsBox.Font = new Font("Consolas", 10F);
        detailLayout.Controls.Add(detailsBox, 0, 2);
    }

    private void SelectProblem(ProblemDefinition problem, Control container)
    {
        selectedProblem = problem;
        foreach (Control child in container.Controls)
        {
            if (child is ProblemTile tile)
            {
                tile.Selected = tile.Problem == problem;
            }
        }
        RefreshProblemFields();
    }

    private void RefreshProblemFields()
    {
        selectedProblemLabel.Text = selectedProblem.Title;
        hostBox.Text = selectedProblem.DefaultHost;
        portBox.Value = selectedProblem.DefaultPort;
    }

    private async Task StartDiagnosticsAsync()
    {
        ShowDashboard();
        runCancellation?.Cancel();
        runCancellation = new CancellationTokenSource();

        SetBusy(true);
        ResetCards();
        progressBar.Value = 0;
        detailsBox.Text = string.Empty;
        summaryLabel.Text = "Running diagnosis...";
        fixHintLabel.Text = "Testing local-country access, international access, DNS, quality, port, VPN and proxy.";
        fixButton.Enabled = false;

        var progress = new Progress<DiagnosticProgress>(UpdateCheck);

        try
        {
            lastReport = await diagnosticService.RunAsync(
                selectedProblem.Title,
                new PortProbeRequest(hostBox.Text.Trim(), (int)portBox.Value),
                progress,
                runCancellation.Token);

            summaryLabel.Text = lastReport.PlainLanguageSummary;
            detailsBox.Text = BuildDetailsText(lastReport);
            fixHintLabel.Text = lastReport.HasSafeFix
                ? SafeFixService.Describe(lastReport.SafeFixKey)
                : "Everything looks healthy, so no repair action is recommended.";
            stateLabel.Text = lastReport.HasSafeFix ? "Diagnosis complete. A safe repair is available." : "Diagnosis complete.";
        }
        catch (OperationCanceledException)
        {
            stateLabel.Text = "Diagnosis cancelled.";
        }
        catch (Exception ex)
        {
            summaryLabel.Text = "Diagnosis could not be completed.";
            detailsBox.Text = ex.ToString();
            stateLabel.Text = "Error";
        }
        finally
        {
            SetBusy(false);
            RefreshUndoButton();
        }
    }

    private void UpdateCheck(DiagnosticProgress progress)
    {
        if (cards.TryGetValue(progress.CheckId, out var card))
        {
            card.SetCheck(progress.Check);
        }

        var completed = cards.Values.Count(card => card.Status is CheckStatus.Healthy or CheckStatus.Warning or CheckStatus.Failed);
        progressBar.Value = Math.Min(progressBar.Maximum, completed);
    }

    private async Task ApplySafeFixAsync()
    {
        if (lastReport?.SafeFixKey is null)
        {
            return;
        }

        var approved = MessageBox.Show(
            $"{SafeFixService.Describe(lastReport.SafeFixKey)}\n\nNet Doctor saves the current setting first so Undo can restore it. Continue?",
            "Fix Safely",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (approved != DialogResult.Yes)
        {
            return;
        }

        SetBusy(true);
        stateLabel.Text = "Running Fix Safely...";
        var message = await safeFixService.ApplyAsync(lastReport.SafeFixKey, CancellationToken.None);
        MessageBox.Show(message, "Net Doctor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        SetBusy(false);
        RefreshUndoButton();
        await StartDiagnosticsAsync();
    }

    private async Task UndoAsync()
    {
        var approved = MessageBox.Show(
            "Restore the last saved setting?",
            "Undo",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (approved != DialogResult.Yes)
        {
            return;
        }

        SetBusy(true);
        stateLabel.Text = "Restoring the previous setting...";
        var message = await safeFixService.UndoAsync(CancellationToken.None);
        MessageBox.Show(message, "Net Doctor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        SetBusy(false);
        RefreshUndoButton();
        await StartDiagnosticsAsync();
    }

    private void ShowDashboard()
    {
        startPanel.Visible = false;
        dashboardPanel.Visible = true;
        dashboardPanel.BringToFront();
    }

    private void ShowStart()
    {
        runCancellation?.Cancel();
        dashboardPanel.Visible = false;
        startPanel.Visible = true;
        startPanel.BringToFront();
    }

    private void SetBusy(bool busy)
    {
        startButton.Enabled = !busy;
        runAgainButton.Enabled = !busy;
        backButton.Enabled = !busy;
        hostBox.Enabled = !busy;
        portBox.Enabled = !busy;
        fixButton.Enabled = !busy && lastReport?.HasSafeFix == true;
        undoButton.Enabled = !busy && safeFixService.HasUndoSnapshot;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private void RefreshUndoButton()
    {
        undoButton.Enabled = safeFixService.HasUndoSnapshot;
    }

    private void ResetCards()
    {
        foreach (var card in cards.Values)
        {
            card.SetCheck(new DiagnosticCheck("pending", card.Title, CheckStatus.Unknown, "Waiting to run...", []));
        }
    }

    private static void AddSmallLabel(TableLayoutPanel grid, string text, int column)
    {
        grid.Controls.Add(new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            ForeColor = Palette.MutedText,
            Font = new Font("Segoe UI", 9F),
            TextAlign = ContentAlignment.BottomLeft
        }, column, 0);
    }

    private static void ConfigureActionButton(Button button, string text, Color backColor, Color foreColor)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(4, 0, 4, 0);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = backColor == Palette.LightButton ? 1 : 0;
        button.FlatAppearance.BorderColor = Palette.Border;
        button.BackColor = backColor;
        button.ForeColor = foreColor;
        button.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
    }

    private static string BuildDetailsText(DiagnosticReport report)
    {
        var lines = new List<string>
        {
            $"Problem: {report.Scenario}",
            $"Started: {report.StartedAt:yyyy-MM-dd HH:mm:ss}",
            $"Finished: {report.FinishedAt:yyyy-MM-dd HH:mm:ss}",
            string.Empty
        };

        foreach (var check in report.Checks)
        {
            lines.Add($"[{StatusToText(check.Status)}] {check.Title}");
            lines.Add(check.Summary);
            foreach (var detail in check.Details)
            {
                lines.Add($"  - {detail.ReplaceLineEndings(" ")}");
            }
            lines.Add(string.Empty);
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string StatusToText(CheckStatus status)
    {
        return status switch
        {
            CheckStatus.Healthy => "OK",
            CheckStatus.Warning => "Warning",
            CheckStatus.Failed => "Problem",
            CheckStatus.Running => "Running",
            _ => "Pending"
        };
    }

    private static Image? TryLoadImage(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "assets", fileName);
        if (!File.Exists(path))
        {
            path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "assets", fileName);
        }

        return File.Exists(path) ? Image.FromFile(path) : null;
    }

    private static Icon? TryLoadIcon()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "assets", "netdoctor-icon.ico");
        if (!File.Exists(path))
        {
            path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "assets", "netdoctor-icon.ico");
        }

        return File.Exists(path) ? new Icon(path) : null;
    }

    internal sealed record ProblemDefinition(string Title, string Description, string DefaultHost, int DefaultPort)
    {
        public static List<ProblemDefinition> Defaults()
        {
            return
            [
                new("Websites do not open", "Internet seems connected, but pages fail or spin forever.", "www.google.com", 443),
                new("International sites fail", "Local sites may work, but global services do not.", "www.cloudflare.com", 443),
                new("Only one website fails", "Check DNS and TCP access for one specific site.", "www.google.com", 443),
                new("VPN connected, no internet", "Find routing, DNS or adapter issues caused by VPN.", "www.cloudflare.com", 443),
                new("Email stuck in Outbox", "Check SMTP reachability for mail sending.", "smtp.gmail.com", 587),
                new("Network file does not open", "Check SMB/file sharing port access.", "fileserver.local", 445),
                new("App is slow online", "Measure latency, packet loss and proxy state.", "www.microsoft.com", 443),
                new("After an update, internet broke", "Look for DNS, proxy, VPN and adapter changes.", "www.microsoft.com", 443)
            ];
        }
    }
}

internal sealed class ProblemTile : RoundedPanel
{
    private readonly Label titleLabel = new();
    private readonly Label descriptionLabel = new();
    private bool selected;

    public ProblemTile(MainForm.ProblemDefinition problem)
    {
        Problem = problem;
        Radius = 8;
        BackColor = Color.White;
        Padding = new Padding(16);
        Cursor = Cursors.Hand;

        titleLabel.Dock = DockStyle.Top;
        titleLabel.Height = 30;
        titleLabel.Text = problem.Title;
        titleLabel.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        titleLabel.ForeColor = Palette.Text;
        titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        titleLabel.Cursor = Cursors.Hand;

        descriptionLabel.Dock = DockStyle.Fill;
        descriptionLabel.Text = problem.Description;
        descriptionLabel.Font = new Font("Segoe UI", 9.2F);
        descriptionLabel.ForeColor = Palette.MutedText;
        descriptionLabel.TextAlign = ContentAlignment.TopLeft;
        descriptionLabel.Cursor = Cursors.Hand;

        Controls.Add(descriptionLabel);
        Controls.Add(titleLabel);
    }

    public MainForm.ProblemDefinition Problem { get; }

    public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            BackColor = selected ? Color.FromArgb(229, 247, 249) : Color.White;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Selected)
        {
            using var pen = new Pen(Palette.Teal, 3);
            e.Graphics.DrawLine(pen, 8, Height - 4, Width - 8, Height - 4);
        }
    }
}

internal sealed class StatusCard : RoundedPanel
{
    private readonly Label titleLabel = new();
    private readonly Label statusLabel = new();
    private readonly Label summaryLabel = new();
    private CheckStatus status = CheckStatus.Unknown;

    public StatusCard()
    {
        Radius = 8;
        BackColor = Color.White;
        Padding = new Padding(16);

        titleLabel.Dock = DockStyle.Top;
        titleLabel.Height = 28;
        titleLabel.Font = new Font("Segoe UI Semibold", 10.4F, FontStyle.Bold);
        titleLabel.ForeColor = Palette.Text;
        titleLabel.TextAlign = ContentAlignment.MiddleLeft;

        statusLabel.Dock = DockStyle.Top;
        statusLabel.Height = 34;
        statusLabel.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;

        summaryLabel.Dock = DockStyle.Fill;
        summaryLabel.Font = new Font("Segoe UI", 9.2F);
        summaryLabel.ForeColor = Palette.MutedText;
        summaryLabel.TextAlign = ContentAlignment.TopLeft;

        Controls.Add(summaryLabel);
        Controls.Add(statusLabel);
        Controls.Add(titleLabel);
        SetCheck(new DiagnosticCheck("unknown", Title, CheckStatus.Unknown, "Waiting to run...", []));
    }

    public string Title
    {
        get => titleLabel.Text;
        set => titleLabel.Text = value;
    }

    public CheckStatus Status => status;

    public void SetCheck(DiagnosticCheck check)
    {
        status = check.Status;
        statusLabel.Text = check.Status switch
        {
            CheckStatus.Healthy => "Healthy",
            CheckStatus.Warning => "Needs attention",
            CheckStatus.Failed => "Problem found",
            CheckStatus.Running => "Checking...",
            _ => "Pending"
        };
        statusLabel.ForeColor = check.Status switch
        {
            CheckStatus.Healthy => Palette.Green,
            CheckStatus.Warning => Palette.Amber,
            CheckStatus.Failed => Palette.Red,
            CheckStatus.Running => Palette.Blue,
            _ => Palette.MutedText
        };
        summaryLabel.Text = check.Summary;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(statusLabel.ForeColor, 4);
        e.Graphics.DrawLine(pen, 4, 18, 4, Height - 18);
    }
}

internal class RoundedPanel : Panel
{
    public int Radius { get; set; } = 8;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = CreateRoundRect(ClientRectangle, Radius);
        using var brush = new SolidBrush(BackColor);
        e.Graphics.FillPath(brush, path);
        using var pen = new Pen(Palette.Border);
        e.Graphics.DrawPath(pen, path);
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        using var path = CreateRoundRect(ClientRectangle, Radius);
        Region = new Region(path);
    }

    private static GraphicsPath CreateRoundRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        if (bounds.Width <= 1 || bounds.Height <= 1)
        {
            path.AddRectangle(Rectangle.Empty);
            return path;
        }

        var rect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
        radius = Math.Max(1, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
        var diameter = radius * 2;
        path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}

internal static class Palette
{
    public static readonly Color AppBackground = Color.FromArgb(244, 248, 250);
    public static readonly Color Navy = Color.FromArgb(10, 22, 34);
    public static readonly Color Text = Color.FromArgb(21, 32, 43);
    public static readonly Color MutedText = Color.FromArgb(92, 107, 123);
    public static readonly Color Border = Color.FromArgb(216, 225, 232);
    public static readonly Color Blue = Color.FromArgb(31, 111, 214);
    public static readonly Color Teal = Color.FromArgb(22, 164, 154);
    public static readonly Color Green = Color.FromArgb(28, 142, 93);
    public static readonly Color Amber = Color.FromArgb(181, 112, 25);
    public static readonly Color Red = Color.FromArgb(200, 54, 69);
    public static readonly Color LightButton = Color.FromArgb(238, 243, 247);
}
