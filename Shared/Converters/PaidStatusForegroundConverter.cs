using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace GestionCommerciale.Shared.Converters;

/// <summary>Paid → green, unpaid → orange (warning).</summary>
public sealed class PaidStatusForegroundConverter : IValueConverter
{
    public static readonly PaidStatusForegroundConverter Instance = new();

    private static readonly IBrush Paid = new SolidColorBrush(Color.Parse("#16A34A"));
    private static readonly IBrush Unpaid = new SolidColorBrush(Color.Parse("#D97706"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool paid)
            return null;
        return paid ? Paid : Unpaid;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
