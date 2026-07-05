using NetDoctor.App.Forms;
using NetDoctor.App.Licensing;
using NetDoctor.App.Localization;
using NetDoctor.App.Services;
using System.Windows.Forms;

namespace NetDoctor.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var settings = SettingsService.LoadAndApply();
        ThemeManager.Apply(AppConfig.Theme);

        if (!settings.FirstRunCompleted)
        {
            using var wizard = new FirstRunWizard(settings);
            if (wizard.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            settings = wizard.Settings;
            SettingsService.Save(settings);
            AppConfig.Apply(settings.Language, settings.Region, settings.Theme, settings.ReducedMotion);
            ThemeManager.Apply(AppConfig.Theme);
        }

        var licenseManager = new LicenseManager();
        var check = licenseManager.CheckStored();

        if (!check.IsValid)
        {
            if (check.Status != LicenseStatus.Missing)
            {
                MessageBox.Show(
                    ActivationForm.MessageFor(check.Status),
                    L.AppTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            using var activation = new ActivationForm(licenseManager);
            if (activation.ShowDialog() != DialogResult.OK)
            {
                return;
            }
        }

        Application.Run(new MainForm(settings));
    }
}
