using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace MauiApp1.Converters
{
    public class PriorityToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int priority)
            {
                return priority switch
                {
                    1 => "🟢",
                    2 => "🟡",
                    3 => "🔴",
                    _ => "⚪"
                };
            }
            return "⚪";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
