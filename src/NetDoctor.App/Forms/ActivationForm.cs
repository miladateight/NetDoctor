using NetDoctor.App.Licensing;
using NetDoctor.App.Localization;
using System.Diagnostics;

namespace NetDoctor.App.Forms;

/// <summary>Shown at startup when there is no valid license. Closes with OK once activated.</summary>
internal sealed class ActivationForm : Form
{
    private readonly LicenseManager licenseManager;
    private readonly TextBox machineBox = new();
    private readonly TextBox keyBox = new();
    private readonly Label statusLabel = new();

    public LicenseInfo? ActivatedLicense { get; private set; }

    public ActivationForm(LicenseManager licenseManager)
    {
        this.licenseManager = licenseManager;

        Text = $"{L.LicenseTitle} - {L.AppTitle}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = AppConfig.IsRtl ? new Size(680, 660) : new Size(640, 600);
        BackColor = Palette.AppBackground;
        Font = UiFonts.Create(10F);
        RightToLeft = AppConfig.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        RightToLeftLayout = AppConfig.IsRtl;
        Icon = TryLoadIcon();

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 10,
            Padding = new Padding(22)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));  // title
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, AppConfig.IsRtl ? 96 : 70));  // intro
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));  // machine label
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // machine row
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, AppConfig.IsRtl ? 64 : 48));  // machine hint
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));  // key label
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));  // key box
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // status
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));  // buy link
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));  // buttons
        Controls.Add(root);

        root.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = L.LicenseTitle,
            Font = UiFonts.Create(16F, FontStyle.Bold),
            ForeColor = Palette.Text,
            TextAlign = ContentAlignment.MiddleCenter
        }, 0, 0);

        root.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = L.LicenseIntro,
            ForeColor = Palette.MutedText,
            TextAlign = ContentAlignment.TopCenter,
            AutoEllipsis = true
        }, 0, 1);

        root.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = L.LicenseMachineLabel,
            ForeColor = Palette.Text,
            Font = UiFonts.Create(9.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.BottomCenter
        }, 0, 2);

        root.Controls.Add(BuildMachineRow(), 0, 3);

        root.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = L.LicenseMachineHint,
            ForeColor = Palette.MutedText,
            TextAlign = ContentAlignment.TopCenter,
            AutoEllipsis = true
        }, 0, 4);

        root.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = L.LicenseKeyLabel,
            ForeColor = Palette.Text,
            Font = UiFonts.Create(9.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.BottomCenter
        }, 0, 5);

        keyBox.Dock = DockStyle.Fill;
        keyBox.Multiline = true;
        keyBox.ScrollBars = ScrollBars.Vertical;
        keyBox.Font = new Font("Consolas", 9.5F);
        keyBox.RightToLeft = RightToLeft.No; // license keys are ASCII; keep them left-to-right
        root.Controls.Add(keyBox, 0, 6);

        statusLabel.Dock = DockStyle.Fill;
        statusLabel.TextAlign = ContentAlignment.TopCenter;
        statusLabel.AutoEllipsis = true;
        statusLabel.Text = L.LicenseBuyHint;
        statusLabel.ForeColor = Palette.MutedText;
        root.Controls.Add(statusLabel, 0, 7);

        var buyLink = new LinkLabel
        {
            Dock = DockStyle.Fill,
            Text = L.LicenseBuyLinkText,
            LinkColor = Palette.Blue,
            Font = UiFonts.Create(9.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        buyLink.LinkClicked += (_, _) => OpenPurchaseLink();
        root.Controls.Add(buyLink, 0, 8);

        root.Controls.Add(BuildButtons(), 0, 9);
    }

    private static void OpenPurchaseLink()
    {
        try
        {
            Process.Start(new ProcessStartInfo(L.LicensePurchaseUrl) { UseShellExecute = true });
        }
        catch (Exception)
        {
            // If no browser is registered, the user can still read/copy the URL from the label text.
        }
    }

    private Control BuildMachineRow()
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
        row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        machineBox.Dock = DockStyle.Fill;
        machineBox.ReadOnly = true;
        machineBox.Font = new Font("Consolas", 11F, FontStyle.Bold);
        machineBox.RightToLeft = RightToLeft.No; // Machine ID is ASCII; always left-to-right
        machineBox.TextAlign = HorizontalAlignment.Left;
        machineBox.BackColor = Color.White;

        var available = MachineFingerprint.IsAvailable && !string.IsNullOrWhiteSpace(MachineFingerprint.Current);
        machineBox.Text = available ? MachineFingerprint.Current : L.LicenseErrMachineUnavailable;

        var copyButton = new Button
        {
            Text = L.LicenseCopyButton,
            Dock = DockStyle.Fill,
            Margin = new Padding(6, 0, 0, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Palette.LightButton,
            ForeColor = Palette.Text,
            Font = UiFonts.Create(9.5F, FontStyle.Bold),
            Enabled = available
        };
        copyButton.FlatAppearance.BorderColor = Palette.Border;
        copyButton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(Palette.LightButton, 0.4f);
        copyButton.Click += (_, _) => CopyMachineId();

        row.Controls.Add(machineBox, 0, 0);
        row.Controls.Add(copyButton, 1, 0);
        return row;
    }

    private Control BuildButtons()
    {
        var buttons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var exitButton = new Button
        {
            Text = L.LicenseExitButton,
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 8, 4, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Palette.LightButton,
            ForeColor = Palette.Text,
            Font = UiFonts.Create(10F, FontStyle.Bold),
            DialogResult = DialogResult.Cancel
        };
        exitButton.FlatAppearance.BorderColor = Palette.Border;
        exitButton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(Palette.LightButton, 0.4f);

        var activateButton = new Button
        {
            Text = L.LicenseActivateButton,
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 8, 4, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Palette.Green,
            ForeColor = Color.White,
            Font = UiFonts.Create(10F, FontStyle.Bold)
        };
        activateButton.FlatAppearance.BorderSize = 0;
        activateButton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(Palette.Green, 0.18f);
        activateButton.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(Palette.Green, 0.06f);
        activateButton.Click += (_, _) => TryActivate();

        buttons.Controls.Add(exitButton, 0, 0);
        buttons.Controls.Add(activateButton, 1, 0);

        AcceptButton = activateButton;
        CancelButton = exitButton;
        return buttons;
    }

    private void CopyMachineId()
    {
        try
        {
            Clipboard.SetText(MachineFingerprint.Current);
            statusLabel.ForeColor = Palette.Green;
            statusLabel.Text = L.LicenseCopied;
        }
        catch (Exception)
        {
            // Clipboard can briefly be unavailable; ignore and leave the ID visible to copy manually.
        }
    }

    private void TryActivate()
    {
        var result = licenseManager.Activate(keyBox.Text);
        if (result.IsValid && result.Info is not null)
        {
            ActivatedLicense = result.Info;
            MessageBox.Show(
                L.LicenseActivated(result.Info.Name, result.Info.DaysRemaining),
                L.AppTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        statusLabel.ForeColor = Palette.Red;
        statusLabel.Text = MessageFor(result.Status);
    }

    internal static string MessageFor(LicenseStatus status) => status switch
    {
        LicenseStatus.Missing => L.LicenseErrMissing,
        LicenseStatus.Malformed => L.LicenseErrMalformed,
        LicenseStatus.BadSignature => L.LicenseErrBadSignature,
        LicenseStatus.Expired => L.LicenseErrExpired,
        LicenseStatus.WrongEdition => L.LicenseErrWrongEdition,
        LicenseStatus.WrongMachine => L.LicenseErrWrongMachine,
        LicenseStatus.ClockTampered => L.LicenseErrClock,
        _ => L.LicenseErrBadSignature
    };

    private static Icon? TryLoadIcon()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "assets", "netdoctor-icon.ico");
        if (!File.Exists(path))
        {
            path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "assets", "netdoctor-icon.ico");
        }

        return File.Exists(path) ? new Icon(path) : null;
    }
}
