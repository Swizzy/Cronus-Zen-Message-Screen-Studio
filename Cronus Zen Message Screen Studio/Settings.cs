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

        public int PenWidth
        {
            get
            {
                if (_penWidth <= 1)
                {
                    return 1;
                }

                if (_penWidth >= 128)
                {
                    return 128;
                }
                return _penWidth;
            }
            set => _penWidth = value;
        }

        public int PenHeight
        {
            get
            {
                if (_penHeight <= 1)
                {
                    return 1;
                }

                if (_penHeight >= 64)
                {
                    return 64;
                }
                return _penHeight;
            }
            set => _penHeight = value;
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

        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }

        public double PreviewWidth { get; set; }
        public double PreviewHeight { get; set; }
        public double DevicePreviewWidth { get; set; }
        public double DevicePreviewHeight { get; set; }
        public PenShapes PenShape { get; set; }

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
                                 {typeof(string), StringParser },
                                 {typeof(PenShapes), PenShapesParser }
                             };
        }

        private static readonly object Locker = new object();
        private static Settings _currentSettings;
        private int _penWidth;
        private int _penHeight;

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

        private static object PenShapesParser(string arg)
        {
            Enum.TryParse(arg, true, out PenShapes toReturn);
            return toReturn;
        }
        private static object StringParser(string arg) => arg;

        #endregion

        internal enum PenShapes
        {
            Square,
            Ellipse,
            Cross,
            TriangleUp,
            TriangleDown,
            TriangleLeft,
            TriangleRight,
            Diamond,
            LineLTR,
            LineRTL

        }

        public static List<SelectionData<PenShapes>> MakePenShapeSelectionList()
        {
            return new List<SelectionData<PenShapes>>
            {
                new SelectionData<PenShapes>("Square", PenShapes.Square),
                new SelectionData<PenShapes>("Ellipse (Circle)", PenShapes.Ellipse),
                new SelectionData<PenShapes>("Cross (+)", PenShapes.Cross),
                new SelectionData<PenShapes>("Triangle Up (^)", PenShapes.TriangleUp),
                new SelectionData<PenShapes>("Triangle Down (v)", PenShapes.TriangleDown),
                new SelectionData<PenShapes>("Triangle Left (<)", PenShapes.TriangleLeft),
                new SelectionData<PenShapes>("Triangle Right (>)", PenShapes.TriangleRight),
                new SelectionData<PenShapes>("Diamond (♦)", PenShapes.Diamond),
                new SelectionData<PenShapes>("Diagonal line (\\)", PenShapes.LineLTR),
                //new SelectionData<PenShapes>("Diagonal line (/)", PenShapes.LineRTL)
            };
        }
    }
}
