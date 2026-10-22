using System;
using System.Globalization;
using System.Windows.Data;

namespace GusakovUP.Services
{
    public class PriorityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int priorityId)
            {
                switch (priorityId)
                {
                    case 1: return "Низкий";
                    case 3: return "Высокий";
                    default: return "Средний";
                }
            }
            return "Средний";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
