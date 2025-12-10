using System.Windows;

namespace EducationProgram.Windows
{
    /// <summary>
    /// Логика взаимодействия для DeleteConfirmationWindow.xaml
    /// </summary>
    public partial class DeleteConfirmationWindow : Window
    {
        public bool IsConfirmed { get; private set; } = false;

        public DeleteConfirmationWindow(string question, string description = "")
        {
            InitializeComponent();
            QuestionTextBlock.Text = question;
            DescriptionTextBlock.Text = description;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }
    }
}
