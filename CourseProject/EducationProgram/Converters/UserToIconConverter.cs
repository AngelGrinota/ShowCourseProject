using EducationProgram.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace EducationProgram.Converters
{
    public class UserToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is User)
                return new BitmapImage(new Uri("pack://application:,,,/Resources/images/logout.png"));
            return new BitmapImage(new Uri("pack://application:,,,/Resources/images/login.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
