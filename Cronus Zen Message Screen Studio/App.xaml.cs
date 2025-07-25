using System.Windows;

namespace CronusZenMessageScreenStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        public static string Version
        {
            get
            {
                if (string.IsNullOrWhiteSpace(GitVersionInformation.PreReleaseLabel) == false)
                {
                    return $"{GitVersionInformation.MajorMinorPatch} {GitVersionInformation.PreReleaseLabel} {GitVersionInformation.PreReleaseNumber}";
                }

                return GitVersionInformation.MajorMinorPatch;
            }
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Settings.SaveSettings();
        }
    }
}
