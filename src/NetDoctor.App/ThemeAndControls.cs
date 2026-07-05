using System.ComponentModel;
using System.Drawing.Drawing2D;
using NetDoctor.App.Localization;
using NetDoctor.App.Models;

namespace NetDoctor.App;

internal enum ThemeMode
{
    Light,
    Dark
}

internal static class ThemeManager
{
    public static ThemeMode Current { get; private set; } = ThemeMode.Light;

    public static void Apply(ThemePreference preference)
    {
        Current = preference switch
        {
            ThemePreference.Dark => ThemeMode.Dark,
            ThemePreference.Light => ThemeMode.Light,
            _ => IsSystemDark() ? ThemeMode.Dark : ThemeMode.Light
        };
    }

    public static void Toggle()
    {
        Current = Current == ThemeMode.Dark ? ThemeMode.Light : ThemeMode.Dark;
    }

    public static void ApplyTo(Control root)
    {
        root.BackColor = Palette.AppBackground;
        root.ForeColor = Palette.Text;
        foreach (Control child in root.Controls)
        {
            ApplyTo(child);
        }
    }

    private static bool IsSystemDark()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return Convert.ToInt32(key?.GetValue("AppsUseLightTheme") ?? 1) == 0;
        }
        catch
        {
            return false;
        }
    }
}

internal static class Palette
{
    public static Color AppBackground => ThemeManager.Current == ThemeMode.Dark ? Color.FromArgb(18, 20, 31) : Color.FromArgb(245, 247, 252);
    public static Color Shell => ThemeManager.Current == ThemeMode.Dark ? Color.FromArgb(24, 27, 43) : Color.White;
    public static Color Surface => ThemeManager.Current == ThemeMode.Dark ? Color.FromArgb(31, 35, 54) : Color.FromArgb(255, 255, 255);
    public static Color SurfaceSoft => ThemeManager.Current == ThemeMode.Dark ? Color.FromArgb(38, 43, 65) : Color.FromArgb(239, 242, 250);
    public static Color Border => ThemeManager.Current == ThemeMode.Dark ? Color.FromArgb(58, 64, 92) : Color.FromArgb(220, 225, 236);
    public static Color Text => ThemeManager.Current == ThemeMode.Dark ? Color.FromArgb(245, 247, 255) : Color.FromArgb(29, 34, 49);
    public static Color MutedText => ThemeManager.Current == ThemeMode.Dark ? Color.FromArgb(166, 174, 198) : Color.FromArgb(102, 112, 133);
    public static Color Accent => Color.FromArgb(120, 85, 255);
    public static Color Accent2 => Color.FromArgb(194, 47, 177);
    public static Color AccentSoft => ThemeManager.Current == ThemeMode.Dark ? Color.FromArgb(54, 45, 95) : Color.FromArgb(239, 234, 255);
    public static Color Green => Color.FromArgb(28, 172, 119);
    public static Color Amber => Color.FromArgb(233, 157, 37);
    public static Color Red => Color.FromArgb(222, 70, 83);
    public static Color Blue => Color.FromArgb(55, 116, 255);
    public static Color LightButton => ThemeManager.Current == ThemeMode.Dark ? Color.FromArgb(42, 47, 70) : Color.FromArgb(244, 246, 251);
    public static Color Glass => ThemeManager.Current == ThemeMode.Dark ? Color.FromArgb(38, 44, 70) : Color.FromArgb(250, 251, 255);
}

internal static class Glyphs
{
    public const string Menu = "\uE700";
    public const string Dashboard = "\uF246";
    public const string Diagnose = "\uE9D9";
    public const string Reset = "\uE777";
    public const string Speed = "\uEADA";
    public const string Monitor = "\uE9D2";
    public const string History = "\uE81C";
    public const string Logs = "\uE8A5";
    public const string Settings = "\uE713";
    public const string About = "\uE946";
    public const string Shield = "\uE83D";
    public const string Moon = "\uE708";
    public const string Sun = "\uE706";
    public const string Play = "\uE768";
    public const string Undo = "\uE7A7";
    public const string Export = "\uEDE1";
}

internal class RoundedPanel : Panel
{
    public int Radius { get; set; } = 8;
    public Color BorderColor { get; set; } = Palette.Border;
    public Color FillColor { get; set; } = Palette.Surface;
    public bool Shadow { get; set; }

    public RoundedPanel()
    {
        DoubleBuffered = true;
        Padding = new Padding(16);
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = ClientRectangle;
        rect.Width -= 1;
        rect.Height -= 1;
        if (Shadow)
        {
            using var shadowBrush = new SolidBrush(Color.FromArgb(ThemeManager.Current == ThemeMode.Dark ? 70 : 24, Color.Black));
            using var shadowPath = RoundedRect(new Rectangle(rect.X + 2, rect.Y + 4, rect.Width - 2, rect.Height - 3), Radius + 2);
            e.Graphics.FillPath(shadowBrush, shadowPath);
        }

        using var fill = new SolidBrush(FillColor);
        using var path = RoundedRect(rect, Radius);
        e.Graphics.FillPath(fill, path);
        using var pen = new Pen(BorderColor);
        e.Graphics.DrawPath(pen, path);
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        Invalidate();
    }

    internal static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}

internal sealed class PillButton : Button
{
    private bool hovering;

    [DefaultValue(false)]
    public bool Accent { get; set; }

    [DefaultValue(false)]
    public bool Danger { get; set; }

    public PillButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Height = 42;
        Cursor = Cursors.Hand;
        Font = UiFonts.Create(9.5F, FontStyle.Bold);
        UseVisualStyleBackColor = false;
        Padding = new Padding(14, 0, 14, 0);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        hovering = true;
        base.OnMouseEnter(e);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        hovering = false;
        base.OnMouseLeave(e);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var fill = Danger ? Palette.Red : Accent ? Palette.Accent : Palette.LightButton;
        if (hovering)
        {
            fill = Accent || Danger ? ControlPaint.Light(fill, 0.08f) : Palette.AccentSoft;
        }

        using var brush = new SolidBrush(fill);
        using var path = RoundedPanel.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), Height / 2);
        pevent.Graphics.FillPath(brush, path);
        TextRenderer.DrawText(
            pevent.Graphics,
            Text,
            Font,
            ClientRectangle,
            Accent || Danger ? Color.White : Palette.Text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | (AppConfig.IsRtl ? TextFormatFlags.RightToLeft : 0));
    }
}

internal sealed class KpiCard : RoundedPanel
{
    private readonly Label titleLabel = new();
    private readonly Label valueLabel = new();
    private readonly Label captionLabel = new();

    public KpiCard(string title)
    {
        Radius = 8;
        FillColor = Palette.Surface;
        BorderColor = Palette.Border;
        Shadow = true;
        Height = 118;
        titleLabel.Text = title;
        titleLabel.Dock = DockStyle.Top;
        titleLabel.Height = 24;
        titleLabel.ForeColor = Palette.MutedText;
        titleLabel.Font = UiFonts.Create(9F, FontStyle.Bold);
        valueLabel.Dock = DockStyle.Top;
        valueLabel.Height = 44;
        valueLabel.ForeColor = Palette.Text;
        valueLabel.Font = UiFonts.Create(22F, FontStyle.Bold);
        captionLabel.Dock = DockStyle.Fill;
        captionLabel.ForeColor = Palette.MutedText;
        captionLabel.Font = UiFonts.Create(8.5F);
        captionLabel.AutoEllipsis = true;
        Controls.Add(captionLabel);
        Controls.Add(valueLabel);
        Controls.Add(titleLabel);
    }

    public void SetValue(string value, string caption, Color color)
    {
        FillColor = Palette.Surface;
        titleLabel.ForeColor = Palette.MutedText;
        valueLabel.ForeColor = color;
        valueLabel.Text = value;
        captionLabel.Text = caption;
        Invalidate();
    }
}

internal sealed class StatusCard : RoundedPanel
{
    private readonly Label titleLabel = new();
    private readonly Label summaryLabel = new();
    private readonly Label statusLabel = new();
    private readonly ProgressRing ring = new();

    public string CheckId { get; }

    public StatusCard(string checkId, string title)
    {
        CheckId = checkId;
        Radius = 8;
        Shadow = true;
        Height = 116;
        FillColor = Palette.Surface;
        BorderColor = Palette.Border;

        ring.Dock = DockStyle.Left;
        ring.Width = 54;
        ring.Value = 0;
        ring.Color = Palette.Accent;

        var textPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(8, 2, 0, 0) };
        titleLabel.Text = title;
        titleLabel.Dock = DockStyle.Top;
        titleLabel.Height = 24;
        titleLabel.ForeColor = Palette.Text;
        titleLabel.Font = UiFonts.Create(10F, FontStyle.Bold);
        statusLabel.Dock = DockStyle.Top;
        statusLabel.Height = 24;
        statusLabel.ForeColor = Palette.MutedText;
        statusLabel.Font = UiFonts.Create(8.5F, FontStyle.Bold);
        summaryLabel.Dock = DockStyle.Fill;
        summaryLabel.ForeColor = Palette.MutedText;
        summaryLabel.Font = UiFonts.Create(8.8F);
        summaryLabel.AutoEllipsis = true;
        textPanel.Controls.Add(summaryLabel);
        textPanel.Controls.Add(statusLabel);
        textPanel.Controls.Add(titleLabel);
        Controls.Add(textPanel);
        Controls.Add(ring);
    }

    public void SetPending()
    {
        statusLabel.Text = L.T("Severity.Info");
        summaryLabel.Text = L.WaitingToRun;
        ring.Value = 8;
        ring.Color = Palette.MutedText;
        Invalidate();
    }

    public void SetRunning(string summary)
    {
        statusLabel.Text = L.RunningDiagnosis;
        summaryLabel.Text = summary;
        ring.Value = 45;
        ring.Color = Palette.Accent;
        Invalidate();
    }

    public void SetCheck(DiagnosticCheck check)
    {
        titleLabel.Text = check.Title;
        summaryLabel.Text = check.Summary;
        statusLabel.Text = check.Status.ToString();
        ring.Value = check.Status switch
        {
            CheckStatus.Healthy => 100,
            CheckStatus.Warning => 68,
            CheckStatus.Failed => 100,
            CheckStatus.Running => 42,
            _ => 12
        };
        ring.Color = check.Status switch
        {
            CheckStatus.Healthy => Palette.Green,
            CheckStatus.Warning => Palette.Amber,
            CheckStatus.Failed => Palette.Red,
            _ => Palette.Accent
        };
        BorderColor = Color.FromArgb(120, ring.Color);
        Invalidate();
    }
}

internal sealed class ProgressRing : Control
{
    public int Value { get; set; }
    public Color Color { get; set; } = Palette.Accent;

    public ProgressRing()
    {
        DoubleBuffered = true;
        Size = new Size(48, 48);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(8, 8, Width - 16, Height - 16);
        using var bg = new Pen(Palette.Border, 6) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var fg = new Pen(Color, 6) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        e.Graphics.DrawArc(bg, rect, -90, 360);
        e.Graphics.DrawArc(fg, rect, -90, Math.Max(0, Math.Min(100, Value)) * 3.6f);
    }
}

internal sealed class Sparkline : Control
{
    private readonly List<float> values = [];

    public void Push(float value)
    {
        values.Add(value);
        while (values.Count > 40)
        {
            values.RemoveAt(0);
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var bg = new SolidBrush(Palette.SurfaceSoft);
        using var path = RoundedPanel.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 8);
        e.Graphics.FillPath(bg, path);
        if (values.Count < 2)
        {
            return;
        }

        var max = Math.Max(1, values.Max());
        var points = values.Select((value, index) => new PointF(
            index * (Width - 18f) / Math.Max(1, values.Count - 1) + 9,
            Height - 10 - value / max * (Height - 20))).ToArray();
        using var pen = new Pen(Palette.Accent, 3) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        e.Graphics.DrawLines(pen, points);
    }
}

internal sealed class CollapsiblePanel : RoundedPanel
{
    private readonly Label header = new();
    private readonly Panel body = new();
    private bool expanded = true;

    public Panel Body => body;

    public CollapsiblePanel(string title)
    {
        Radius = 8;
        FillColor = Palette.Surface;
        BorderColor = Palette.Border;
        header.Text = title;
        header.Dock = DockStyle.Top;
        header.Height = 40;
        header.ForeColor = Palette.Text;
        header.Font = UiFonts.Create(10F, FontStyle.Bold);
        header.Cursor = Cursors.Hand;
        header.Click += (_, _) => Toggle();
        body.Dock = DockStyle.Fill;
        body.BackColor = Color.Transparent;
        Controls.Add(body);
        Controls.Add(header);
    }

    private void Toggle()
    {
        expanded = !expanded;
        body.Visible = expanded;
        Height = expanded ? 220 : 58;
    }
}

internal sealed class Toast : RoundedPanel
{
    private readonly Label label = new();

    public Toast()
    {
        Radius = 8;
        FillColor = Palette.Accent;
        BorderColor = Palette.Accent;
        Height = 48;
        Width = 360;
        Visible = false;
        label.Dock = DockStyle.Fill;
        label.ForeColor = Color.White;
        label.TextAlign = ContentAlignment.MiddleCenter;
        label.Font = UiFonts.Create(9F, FontStyle.Bold);
        Controls.Add(label);
    }

    public void ShowMessage(Control parent, string text)
    {
        label.Text = text;
        if (Parent is null)
        {
            parent.Controls.Add(this);
            BringToFront();
        }
        Location = new Point(parent.ClientSize.Width - Width - 24, 76);
        Visible = true;
        BringToFront();
        var timer = new System.Windows.Forms.Timer { Interval = 2600 };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            timer.Dispose();
            Visible = false;
        };
        timer.Start();
    }
}

internal static class Animator
{
    public static void Run(int durationMs, Action<double> onTick, Action? onComplete = null)
    {
        if (AppConfig.ReducedMotion || durationMs <= 0)
        {
            onTick(1);
            onComplete?.Invoke();
            return;
        }

        var start = DateTime.UtcNow;
        var timer = new System.Windows.Forms.Timer { Interval = 16 };
        timer.Tick += (_, _) =>
        {
            var progress = Math.Min(1, (DateTime.UtcNow - start).TotalMilliseconds / durationMs);
            var eased = 1 - Math.Pow(1 - progress, 3);
            onTick(eased);
            if (progress >= 1)
            {
                timer.Stop();
                timer.Dispose();
                onComplete?.Invoke();
            }
        };
        timer.Start();
    }
}
