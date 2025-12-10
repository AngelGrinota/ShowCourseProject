using System.Windows;

namespace EducationProgram.Windows
{
    /// <summary>
    /// Логика взаимодействия для LogoutConfirmationWindow.xaml
    /// </summary>
    public partial class LogoutConfirmationWindow : Window
    {
        public LogoutConfirmationWindow()
        {
            InitializeComponent();
        }
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
