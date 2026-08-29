using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GCAT.Converters;

// Loads an icon from Assets/Icons by name. If the file hasn't been added yet, it just
// returns null so the layout renders cleanly with an empty image slot instead of crashing.
public class PathToBitmapConverter : IValueConverter
{
    public static readonly PathToBitmapConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            var uri = path.StartsWith("avares://", StringComparison.OrdinalIgnoreCase)
                ? new Uri(path)
                : new Uri($"avares://GCAT/{path.TrimStart('/')}");

            if (AssetLoader.Exists(uri))
            {
                using var stream = AssetLoader.Open(uri);
                return new Bitmap(stream);
            }
        }
        catch
        {
            // Missing placeholder icon - ignore so the UI still renders.
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
