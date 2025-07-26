using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CronusZenMessageScreenStudio;

public class FontFamilyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is System.Drawing.FontFamily drawingFontFamily)
        {
            return new FontFamily(drawingFontFamily.Name);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}