using System;
using System.Globalization;
using System.Windows.Data;

namespace VotingPC
{
    public class AddValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double number = double.Parse(value.ToString());
            double addNumber = double.Parse(parameter.ToString());
            return number + addNumber;
        }
        public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
        {
            // Don't use this. You are warned.
            return (double)value - (double)param;
        }
    }
}
