using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EducationProgram.Converters
{
    public class ZebraStripeConverter : IValueConverter
    {
        private static readonly Brush EvenBrush = Brushes.White;
        private static readonly Brush OddBrush = new SolidColorBrush(Color.FromRgb(245, 247, 255));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return index % 2 == 0 ? EvenBrush : OddBrush;
            }
            return EvenBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
