using System.Windows;

namespace EducationProgram.Windows
{
    public partial class InformationWindow : Window
    {
        // Свойства для привязки в XAML
        public string Message { get; set; }
        public int Score { get; set; }
        public int TotalPoints { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }

        // Свойства видимости (для переключения интерфейса)
        public Visibility MessageVisibility { get; set; } = Visibility.Visible;
        public Visibility ResultVisibility { get; set; } = Visibility.Collapsed;

        public InformationWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        // Метод для настройки окна в режим показа результатов теста
        public void SetTestResult(int score, int totalPoints, int correctAnswers, int totalQuestions)
        {
            Score = score;
            TotalPoints = totalPoints;
            CorrectAnswers = correctAnswers;
            TotalQuestions = totalQuestions;

            // Скрываем обычное сообщение, показываем блок результатов
            MessageVisibility = Visibility.Collapsed;
            ResultVisibility = Visibility.Visible;

            // Обновляем привязки, так как свойства изменились (простой способ без INotifyPropertyChanged для окна)
            InitializeComponent();
            DataContext = this;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
