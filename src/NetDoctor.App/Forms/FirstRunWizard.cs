using NetDoctor.App.Localization;
using NetDoctor.App.Services;

namespace NetDoctor.App.Forms;

internal sealed class FirstRunWizard : Form
{
    private readonly ComboBox languageBox;
    private readonly ComboBox regionBox;
    private readonly ComboBox themeBox;

    public AppSettings Settings { get; private set; }

    public FirstRunWizard(AppSettings initial)
    {
        Settings = initial;
        Text = L.T("FirstRun.Title");
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(560, 430);
        MinimumSize = new Size(520, 400);
        BackColor = Palette.AppBackground;
        Font = UiFonts.Create(10F);
        RightToLeft = AppConfig.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        RightToLeftLayout = AppConfig.IsRtl;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28),
            ColumnCount = 2,
            RowCount = 6,
            BackColor = Palette.AppBackground
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        var title = new Label
        {
            Dock = DockStyle.Fill,
            Text = L.T("FirstRun.Title") + Environment.NewLine + L.T("FirstRun.Body"),
            ForeColor = Palette.Text,
            Font = UiFonts.Create(14F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        root.Controls.Add(title, 0, 0);
        root.SetColumnSpan(title, 2);

        languageBox = Combo(["en", "de", "fa", "ar"], initial.Language);
        regionBox = Combo(["World", "Iran"], initial.Region);
        themeBox = Combo(["System", "Light", "Dark"], initial.Theme);
        AddRow(root, 1, L.T("FirstRun.Language"), languageBox);
        AddRow(root, 2, L.T("FirstRun.Region"), regionBox);
        AddRow(root, 3, L.T("FirstRun.Theme"), themeBox);

        var finish = new PillButton { Dock = DockStyle.Right, Width = 190, Text = L.T("FirstRun.Finish"), Accent = true };
        finish.Click += (_, _) =>
        {
            Settings = initial with
            {
                Language = languageBox.Text,
                Region = regionBox.Text,
                Theme = themeBox.Text,
                FirstRunCompleted = true
            };
            DialogResult = DialogResult.OK;
            Close();
        };
        root.Controls.Add(finish, 1, 5);
        Controls.Add(root);
    }

    private static ComboBox Combo(string[] values, string selected)
    {
        var combo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = UiFonts.Create(10F) };
        combo.Items.AddRange(values.Cast<object>().ToArray());
        combo.SelectedItem = values.FirstOrDefault(v => v.Equals(selected, StringComparison.OrdinalIgnoreCase)) ?? values[0];
        return combo;
    }

    private static void AddRow(TableLayoutPanel root, int row, string label, Control control)
    {
        root.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = label,
            ForeColor = Palette.MutedText,
            Font = UiFonts.Create(9.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, row);
        root.Controls.Add(control, 1, row);
    }
}
