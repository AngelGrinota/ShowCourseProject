using System.Globalization;
using System.Windows.Data;

namespace EducationProgram.Converters
{
    public class IntToSortArrowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int state)
            {
                if (state == 1) // Возрастание
                    return "↑";
                if (state == 2) // Убывание
                    return "↓";
            }
            // 0 или любой другой (Сброс)
            return "↕";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}