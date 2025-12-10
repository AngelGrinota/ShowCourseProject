using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EducationProgram.Converters
{
    public class GradeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int grade))
            {
                // По умолчанию, если оценка невалидна или отсутствует
                // Для фона: светло-серый; для текста: черный.
                return parameter?.ToString() == "Background"
                    ? new SolidColorBrush(Color.FromRgb(200, 200, 200))
                    : new SolidColorBrush(Colors.Black);
            }

            // 1. Цвета для фона (Background)
            if (parameter?.ToString() == "Background")
            {
                return grade switch
                {
                    // Красный: 1, 2 (Плохо)
                    <= 2 => new SolidColorBrush(Color.FromArgb(255, 220, 53, 69)),      // Danger/Dark Red
                    // Оранжевый/Желтый: 3 (Удовлетворительно)
                    3 => new SolidColorBrush(Color.FromArgb(255, 255, 193, 7)),         // Warning/Bright Yellow
                    // Зеленый: 4, 5 (Хорошо/Отлично)
                    >= 4 => new SolidColorBrush(Color.FromArgb(255, 40, 167, 69)),      // Success/Dark Green
                };
            }
            // 2. Цвета для текста (Foreground)
            else
            {
                return new SolidColorBrush(Colors.White);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}