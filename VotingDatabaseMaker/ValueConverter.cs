using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VotingDatabaseMaker;

public class MinusValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double number = double.Parse(value.ToString());
        double minusNumber= double.Parse(parameter.ToString());
        return number - minusNumber;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Don't use this. You are warned.
        return (double)value + (double)parameter;
    }
}
public class HexToBrushValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value as string is "") { return Brushes.White; }
        return new BrushConverter().ConvertFromString("#" + value);
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        SolidColorBrush brush = (SolidColorBrush)value;
        // Don't use this. You are warned.
        return brush.Color.ToString();
    }
}