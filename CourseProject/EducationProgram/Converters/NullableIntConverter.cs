using System.Globalization;
using System.Windows.Data;

namespace EducationProgram.Converters
{
    public class NullableIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value?.ToString();

            if (string.IsNullOrWhiteSpace(str))
                return null; // ← ВОТ ТУТ МАГИЯ!

            if (int.TryParse(str, out int number))
                return number;

            return null;
        }
    }
}
