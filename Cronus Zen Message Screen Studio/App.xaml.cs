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
                Version ver = Assembly.GetAssembly(typeof(App)).GetName().Version;
                return ver.Major + "." + ver.Minor;
            }
        }
    }
}
