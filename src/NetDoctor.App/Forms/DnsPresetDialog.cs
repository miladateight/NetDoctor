using NetDoctor.App.Localization;
using NetDoctor.App.Services;

namespace NetDoctor.App.Forms;

/// <summary>Lets the user pick which DNS preset Fix Safely should apply.</summary>
internal sealed class DnsPresetDialog : Form
{
    public DnsPreset? SelectedPreset { get; private set; }

    public DnsPresetDialog()
    {
        Text = L.DnsChooserTitle;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = AppConfig.IsRtl ? new Size(680, 580) : new Size(640, 540);
        BackColor = Palette.AppBackground;
        Font = UiFonts.Create(10F);
        RightToLeft = AppConfig.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        RightToLeftLayout = AppConfig.IsRtl;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(18)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, AppConfig.IsRtl ? 96 : 72));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        Controls.Add(root);

        var prompt = new Label
        {
            Dock = DockStyle.Fill,
            Text = L.DnsChooserPrompt,
            ForeColor = Palette.Text,
            Font = UiFonts.Create(10.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            AutoEllipsis = true
        };
        root.Controls.Add(prompt, 0, 0);

        var list = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };
        root.Controls.Add(list, 0, 1);

        var recommended = DnsPresets.Recommended;
        foreach (var preset in DnsPresets.All)
        {
            var isRecommended = preset.Id == recommended.Id;
            var label = isRecommended
                ? $"{preset.DisplayName} {L.DnsChooserRecommended} - {preset.DisplayNote}"
                : $"{preset.DisplayName} - {preset.DisplayNote}";

            var radio = new RadioButton
            {
                Text = label,
                Tag = preset,
                AutoSize = false,
                Width = Math.Max(560, list.ClientSize.Width - 28),
                Height = AppConfig.IsRtl ? 84 : 68,
                Checked = isRecommended,
                ForeColor = Palette.Text,
                Font = UiFonts.Create(10F),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(6, 0, 6, 0)
            };
            radio.CheckedChanged += (_, _) =>
            {
                if (radio.Checked)
                {
                    SelectedPreset = preset;
                }
            };
            list.Controls.Add(radio);
        }

        SelectedPreset = recommended;

        var buttons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(buttons, 0, 2);

        var applyButton = new Button
        {
            Text = L.Apply,
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 8, 4, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Palette.Green,
            ForeColor = Color.White,
            Font = UiFonts.Create(10F, FontStyle.Bold),
            DialogResult = DialogResult.OK
        };
        applyButton.FlatAppearance.BorderSize = 0;
        applyButton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(Palette.Green, 0.18f);
        applyButton.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(Palette.Green, 0.06f);

        var cancelButton = new Button
        {
            Text = L.Cancel,
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 8, 4, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Palette.LightButton,
            ForeColor = Palette.Text,
            Font = UiFonts.Create(10F, FontStyle.Bold),
            DialogResult = DialogResult.Cancel
        };
        cancelButton.FlatAppearance.BorderColor = Palette.Border;
        cancelButton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(Palette.LightButton, 0.4f);

        buttons.Controls.Add(cancelButton, 0, 0);
        buttons.Controls.Add(applyButton, 1, 0);

        AcceptButton = applyButton;
        CancelButton = cancelButton;
    }
}
