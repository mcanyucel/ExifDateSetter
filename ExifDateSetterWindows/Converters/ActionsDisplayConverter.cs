using System.Globalization;
using System.Windows.Data;
using ExifDateSetterWindows.Model;

namespace ExifDateSetterWindows.Converters;

public class ActionsDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Actions action)
        {
            return action switch
            {
                Actions.ExifToFileDate => "Set File Date from Exif Date",
                Actions.FileDateToExif => "Set Exif Date from File Date",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
        throw new ArgumentException("Invalid value type", nameof(value));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("ConvertBack is not supported.");
    }
}