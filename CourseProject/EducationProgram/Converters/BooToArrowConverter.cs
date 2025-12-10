using System.Globalization;
using System.Windows.Data;

namespace EducationProgram.Converters
{
    public class BoolToArrowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? "↑" : "↓"; // ↑ = по возрастанию, ↓ = по убыванию
            return "↕"; // null = без сортировки
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
