using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EducationProgram.Converters
{
    public class LeaderboardColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int rank = 0;

            if (value is int intValue)
                rank = intValue;
            else if (value is ulong ulongValue)
                rank = (int)ulongValue;
            else
                return new SolidColorBrush(Color.FromRgb(50, 50, 50)); // По умолчанию

            if (parameter?.ToString() == "Background")
            {
                return rank switch
                {
                    1 => new SolidColorBrush(Color.FromRgb(255, 223, 0)),   // Золотой
                    2 => new SolidColorBrush(Color.FromRgb(192, 192, 192)), // Серебряный
                    3 => new SolidColorBrush(Color.FromRgb(205, 127, 50)),  // Бронзовый
                    _ => new SolidColorBrush(Color.FromRgb(250, 250, 250))  // Остальные
                };
            }
            else
            {
                return rank switch
                {
                    1 => new SolidColorBrush(Color.FromRgb(153, 115, 0)),   // Темный золотой
                    2 => new SolidColorBrush(Color.FromRgb(128, 128, 128)), // Темный серый
                    3 => new SolidColorBrush(Color.FromRgb(136, 68, 8)),    // Темный бронзовый
                    _ => new SolidColorBrush(Color.FromRgb(50, 50, 50))     // Остальные
                };
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
