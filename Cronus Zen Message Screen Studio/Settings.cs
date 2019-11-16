using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace CronusZenMessageScreenStudio
{
    internal class Settings
    {
        #region Settings

        public int PenThickness
        {
            get
            {
                if (_penThickness <= 1)
                {
                    return 1;
                }

                if (_penThickness >= 62)
                {
                    return 63;
                }

                if ((_penThickness - 1) % 2 > 0)
                {
                    return _penThickness + 1;
                }
                return _penThickness;
            }
            set => _penThickness = value;
        }

        public bool HighlightColumnAndRow { get; set; }
        public bool HighlightFullColumnAndRow { get; set; }

        #endregion

        public Settings()
        {
            //TODO: Set defaults
        }

        internal static Settings CurrentSettings
        {
            get
            {
                lock (Locker)
                {
                    if (_currentSettings != null)
                    {
                        return _currentSettings;
                    }

                    LoadSettings();
                    return _currentSettings;
                }
            }
        }

        internal static void LoadSettings(bool setDefaults = false)
        {
            _currentSettings = new Settings();
            if (setDefaults)
            {
                return;
            }

            try
            {
                if (!System.IO.File.Exists(SettingsPath))
                {
                    return;
                }

                XElement xml = XElement.Parse(System.IO.File.ReadAllText(SettingsPath));
                foreach (XElement element in xml.Elements())
                {
                    ParseSetting(element);
                }
            }
            catch
            {
                // Ignore any exceptions here... perhaps notify user??
            }
        }

        internal static void SaveSettings()
        {
            XElement xml = new XElement("settings");
            foreach (PropertyInfo property in typeof(Settings).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty))
            {
                xml.Add(new XElement(property.Name, property.GetValue(_currentSettings).ToString()));
            }

            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            File.WriteAllText(SettingsPath, xml.ToString());
        }

        #region Handling

        private static readonly string SettingsPath;
        private static readonly Dictionary<Type, Func<string, object>> SettingParsers;

        static Settings()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            SettingsPath = Path.Combine(appData, "Cronus Zen Message Screen Studio", "Config.xml");
            SettingParsers = new Dictionary<Type, Func<string, object>>
                             {
                                 {typeof(bool), BoolParser },
                                 {typeof(double), DoubleParser },
                                 {typeof(int), IntegerParser },
                                 {typeof(string), StringParser }
                             };
        }

        private static readonly object Locker = new object();
        private static Settings _currentSettings;
        private int _penThickness;

        private static void ParseSetting(XElement xml)
        {
            PropertyInfo property = typeof(Settings).GetProperty(xml.Name.ToString(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
            if (property != null)
            {
                if (SettingParsers.Keys.Contains(property.PropertyType))
                {
                    object value = SettingParsers[property.PropertyType](xml.Value);
                    property.SetValue(_currentSettings, value);
                }
            }
        }
        private static object BoolParser(string arg)
        {
            return string.Equals(arg, "true", StringComparison.InvariantCultureIgnoreCase);
        }
        private static object DoubleParser(string arg)
        {
            double.TryParse(arg, NumberStyles.Number, null, out double toReturn);
            return toReturn;
        }
        private static object IntegerParser(string arg)
        {
            int.TryParse(arg, NumberStyles.Integer, null, out int toReturn);
            return toReturn;
        }
        private static object StringParser(string arg) => arg;

        #endregion
    }
}
