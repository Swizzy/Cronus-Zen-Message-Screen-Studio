using System;
using System.Reflection;

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
                Assembly assembly = typeof(App).Assembly;
                try
                {
                    Type infoType = assembly.GetType("GitVersionInformation");
                    var major = infoType.GetField("Major").GetValue(null);
                    var minor = infoType.GetField("Minor").GetValue(null);
                    var preReleaseLabel = infoType.GetField("PreReleaseLabel").GetValue(null)?.ToString();
                    if (preReleaseLabel != "")
                    {
                        preReleaseLabel += " " + infoType.GetField("PreReleaseNumber").GetValue(null);
                    }
                    return (major + "." + minor + " " + preReleaseLabel).Trim();
                }
                catch
                {
                    Version ver = assembly.GetName().Version;
                    return ver.Major + "." + ver.Minor;
                }
            }
        }
    }
}
