using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GCAT.Converters;

// Turns a 0.0-1.0 ratio into a pixel height, given a max-height ConverterParameter.
// Used to size the bars in the "income vs expenses" chart.
public class RatioToHeightConverter : IValueConverter
{
    public static readonly RatioToHeightConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double ratio && parameter is string maxStr && double.TryParse(maxStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var max))
        {
            var height = ratio * max;
            return height < 4 ? 4 : height; // keep a sliver visible even for ~0 values
        }

        return 4.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
