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
                object major, minor;
                string preReleaseLabel = "";
                Assembly assembly = typeof(App).Assembly;
                try
                {
                    Type infoType = assembly.GetType("GitVersionInformation");
                    major = infoType.GetField("Major").GetValue(null);
                    minor = infoType.GetField("Minor").GetValue(null);
                    preReleaseLabel = infoType.GetField("PreReleaseLabel").GetValue(null)?.ToString();
                    if (preReleaseLabel != "")
                    {
                        preReleaseLabel += " " + infoType.GetField("PreReleaseNumber").GetValue(null);
                    }
                }
                catch
                {
                    Version ver = assembly.GetName().Version;
                    major = ver.Major;
                    minor = ver.Minor;
                }
                return (major + "." + minor + " " + preReleaseLabel).Trim();
            }
        }
    }
}
