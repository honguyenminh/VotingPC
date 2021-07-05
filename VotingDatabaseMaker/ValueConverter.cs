using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace VotingDatabaseMaker
{
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
}
