using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EducationProgram.Converters
{
    public class BoolToBackgroundConverter : IValueConverter
    {
        public Brush TrueBrush { get; set; } = new SolidColorBrush(Color.FromRgb(243, 229, 245)); // #F3E5F5 (светло-фиолетовый)
        public Brush FalseBrush { get; set; } = Brushes.Transparent;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
                return isSelected ? TrueBrush : FalseBrush;

            return FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Конвертация обратно не требуется
            throw new NotImplementedException();
        }
    }
}
