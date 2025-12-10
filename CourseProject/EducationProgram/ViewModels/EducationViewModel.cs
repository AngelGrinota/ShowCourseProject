using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducationProgram.Models;
using EducationProgram.Services;
using EducationProgram.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace EducationProgram.ViewModels
{
    public partial class EducationViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private readonly DataAccessService _dataAccessService;
        private readonly UserService _userService;

        [ObservableProperty]
        private User? currentUser;
        [ObservableProperty] private bool isStudent;

        [ObservableProperty]
        private Theme? selectedTheme;

        [ObservableProperty]
        private Section? currentSection;

        [ObservableProperty]
        private ObservableCollection<Paragraph> currentParagraphs = new();

        [ObservableProperty]
        private Test? currentTest;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsInTestMode))]
        private bool isTestMode;

        [ObservableProperty]
        private bool isPreviewTestMode;

        [ObservableProperty]
        private int currentQuestionIndex = 0;

        [ObservableProperty]
        private bool isSidebarVisible = true;

        [ObservableProperty]
        private double sidebarWidth = 370;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPdfSelected))]
        private Paragraph? selectedParagraph;

        public ObservableCollection<Section> Sections { get; set; } = new();

        [ObservableProperty]
        private int remainingAttempts = 0;

        [ObservableProperty]
        private int maxAttempts;

        [ObservableProperty]
        private PdfViewerViewModel pdfViewerViewModel;

        public bool IsPdfSelected => SelectedParagraph != null &&
                                     SelectedParagraph.Path.Contains(".pdf", StringComparison.OrdinalIgnoreCase);

        private Dictionary<int, int?> _selectedAnswers = new();

        public int? CurrentSelectedAnswerId
        {
            get => CurrentQuestion != null && _selectedAnswers.ContainsKey(CurrentQuestion.QuestionId)
                ? _selectedAnswers[CurrentQuestion.QuestionId]
                : null;
        }

        public Question? CurrentQuestion => CurrentTest?.Questions.ElementAtOrDefault(CurrentQuestionIndex);

        public bool CanGoPrevious => CurrentQuestionIndex > 0;
        public bool CanGoNext => CurrentTest != null && CurrentQuestionIndex < (CurrentTest.Questions?.Count ?? 0) - 1;

        public bool IsInTestMode => IsTestMode;

        public EducationViewModel(NavigationService navigationService, DataAccessService dataAccessService, UserService userService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dataAccessService = dataAccessService ?? throw new ArgumentNullException(nameof(dataAccessService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

            CurrentUser = _userService.CurrentUser;
            UpdateRoleProperties();

            _ = LoadSectionsAsync();

            WeakEventManager<UserService, System.ComponentModel.PropertyChangedEventArgs>
             .AddHandler(_userService, "PropertyChanged", OnUserServicePropertyChanged);
        }

        private async void OnUserServicePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserService.CurrentUser))
            {
                CurrentUser = _userService.CurrentUser;

                UpdateRoleProperties();

                await LoadSectionsAsync();
            }
        }

        private void UpdateRoleProperties()
        {
            IsStudent = _userService.IsStudent;
        }

        // --- ObservableProperty Partial Methods ---

        partial void OnCurrentQuestionIndexChanged(int oldValue, int newValue)
        {
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CurrentSelectedAnswerId));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));

            PreviousQuestionCommand.NotifyCanExecuteChanged();
            NextQuestionCommand.NotifyCanExecuteChanged();
        }
        partial void OnCurrentTestChanged(Test? oldValue, Test? newValue)
        {
            CurrentQuestionIndex = 0;
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
        }


        [RelayCommand]
        private async Task LoadSectionsAsync()
        {
            try
            {
                var sectionsData = await _dataAccessService.GetSections();
                Sections.Clear();
                foreach (var section in sectionsData)
                    Sections.Add(section);

                CurrentSection = Sections.FirstOrDefault();
                SelectedTheme = CurrentSection?.Themes.FirstOrDefault();

                if (SelectedTheme != null)
                {
                    CurrentParagraphs = new ObservableCollection<Paragraph>(SelectedTheme.Paragraphs);
                    var firstParagraph = CurrentParagraphs.FirstOrDefault();
                    if (firstParagraph != null)
                        SelectParagraph(firstParagraph);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading sections: {ex.Message}");
            }
        }


        [RelayCommand]
        private void SelectTheme(Theme? theme)
        {
            if (theme == null) return;

            SelectedTheme = theme;
            CurrentParagraphs = new ObservableCollection<Paragraph>(theme.Paragraphs ?? new List<Paragraph>());

            var firstParagraph = CurrentParagraphs.FirstOrDefault();
            if (firstParagraph != null)
                SelectParagraph(firstParagraph);

            ResetTestState();
        }

        [RelayCommand]
        private void SelectParagraph(Paragraph? paragraph)
        {
            if (paragraph == null) return;

            ResetPdf();

            UpdateSectionAndThemeByParagraph(paragraph);

            SelectedParagraph = paragraph;

            if (IsPdfSelected)
                LoadPdf(paragraph.Path);
        }

        private void ResetPdf()
        {
            IsTestMode = false;
            IsPreviewTestMode = false;
            SidebarWidth = 370;
        }

        private void UpdateSectionAndThemeByParagraph(Paragraph p)
        {
            var theme = Sections.SelectMany(s => s.Themes)
                                .FirstOrDefault(t => t.Paragraphs.Any(x => x.ParagraphId == p.ParagraphId));

            if (theme == null) return;

            SelectedTheme = theme;
            CurrentSection = Sections.First(s => s.Themes.Contains(theme));
        }

        private void LoadPdf(string path)
        {
            PdfViewerViewModel ??= new PdfViewerViewModel();
            PdfViewerViewModel.LoadPdf(path);
        }

        [RelayCommand]
        private void SelectAnswer(int answerId)
        {
            if (CurrentQuestion == null) return;

            // Записываем ответ в словарь
            if (_selectedAnswers.ContainsKey(CurrentQuestion.QuestionId))
            {
                _selectedAnswers[CurrentQuestion.QuestionId] = answerId;
            }
            else
            {
                _selectedAnswers.Add(CurrentQuestion.QuestionId, answerId);
            }

            // Уведомляем интерфейс, чтобы RadioButton переключился
            OnPropertyChanged(nameof(CurrentSelectedAnswerId));
        }

        private void ResetTestState()
        {
            IsTestMode = false;
            IsPreviewTestMode = false;
            SidebarWidth = 370;
            CurrentTest = null;
        }

        // --- Test Navigation ---

        [RelayCommand(CanExecute = nameof(CanGoPrevious))]
        private void PreviousQuestion() => CurrentQuestionIndex--;

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        private void NextQuestion()
        {
            CurrentQuestionIndex++;
        }

        [RelayCommand]
        private async Task StartTest(Test? test)
        {
            if (test == null) return;

            if (CurrentUser == null)
            {
                ShowInformationWindow("Требуется авторизация для прохождения теста!");
                return;
            }

            MaxAttempts = test.MaxAttempts;

            int attemptsUsed = await _dataAccessService.GetUserAttemptsCount(CurrentUser.UserId, test.TestId);
            RemainingAttempts = MaxAttempts - attemptsUsed;

            if (RemainingAttempts <= 0)
            {
                ShowInformationWindow("Вы исчерпали все попытки для этого теста!");
                return;
            }

            var questions = await _dataAccessService.GetFullQuestionsForTest(test.TestId);
            test.Questions = questions;

            CurrentTest = test;

            _selectedAnswers.Clear();

            foreach (var question in CurrentTest.Questions)
            {
                _selectedAnswers.Add(question.QuestionId, null);
            }

            OnPropertyChanged(nameof(CurrentSelectedAnswerId));

            CurrentQuestionIndex = 0;
            IsPreviewTestMode = true;
            IsTestMode = false;
        }

        [RelayCommand]
        private void BeginTest()
        {
            if (CurrentTest == null) return;

            IsTestMode = true;
            IsPreviewTestMode = false;
            SidebarWidth = 0;

            CurrentQuestionIndex = 0;
        }

        [RelayCommand]
        private async Task FinishTest()
        {
            if (CurrentTest == null || IsPreviewTestMode)
            {
                ResetTestState();
                return;
            }

            if (CurrentUser == null)
            {
                ResetTestState();
                return;
            }

            int score = 0;
            int totalPoints = 0;
            int correctQuestionsCount = 0;
            int totalQuestions = CurrentTest.Questions.Count;

            foreach (var question in CurrentTest.Questions)
            {
                totalPoints += question.Points;
                int? selectedAnswerId = _selectedAnswers.GetValueOrDefault(question.QuestionId);

                var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);

                if (selectedAnswerId.HasValue && correctAnswer != null && selectedAnswerId.Value == correctAnswer.AnswerId)
                {
                    score += question.Points;
                    correctQuestionsCount++;
                }
            }

            int nextAttemptNumber = await _dataAccessService.GetNextAttemptNumber(CurrentUser.UserId, CurrentTest.TestId);

            var testResult = new TestResult
            {
                UserId = CurrentUser.UserId,
                TestId = CurrentTest.TestId,
                AttemptNumber = nextAttemptNumber,
                Score = score,
                PassedAt = DateTime.Now
            };

            await _dataAccessService.SaveTestResult(testResult);

            var resultWindow = new InformationWindow
            {
                Owner = Application.Current.MainWindow
            };

            resultWindow.SetTestResult(score, totalPoints, correctQuestionsCount, totalQuestions);

            resultWindow.ShowDialog();

            ResetTestState();
            CurrentQuestionIndex = 0; CurrentQuestionIndex = 0;
        }

        // --- Section Navigation ---

        private void NavigateSection(int direction)
        {
            if (Sections.Count == 0 || CurrentSection == null) return;

            int currentIndex = Sections.IndexOf(CurrentSection);
            int nextIndex = currentIndex + direction;

            if (nextIndex < 0 || nextIndex >= Sections.Count) return;

            CurrentSection = Sections[nextIndex];
            SelectedTheme = CurrentSection.Themes.FirstOrDefault();

            if (SelectedTheme != null)
            {
                SelectTheme(SelectedTheme);
                SelectedParagraph = SelectedTheme.Paragraphs.FirstOrDefault();
            }
        }

        [RelayCommand]
        private void ToggleSidebar()
        {
            if (IsTestMode || IsPreviewTestMode) return;
            IsSidebarVisible = !IsSidebarVisible;
            SidebarWidth = IsSidebarVisible ? 370 : 0;
        }
        [RelayCommand] private void NextSection() => NavigateSection(1);
        [RelayCommand] private void PrevSection() => NavigateSection(-1);

        // --- User Actions ---

        [RelayCommand]
        private void Logout()
        {
            if (IsTestMode) return;

            if (CurrentUser != null)
            {
                var confirmationWindow = new LogoutConfirmationWindow();
                if (confirmationWindow.ShowDialog() == true)
                    _userService.Logout();
            }
            else
            {
                _navigationService.Navigate("Authorization");
            }
        }

        [RelayCommand]
        private void NavigateToProfile()
        {
            if (CurrentUser == null)
            {
                ShowInformationWindow("Вы не авторизованы!");
                return;
            }

            _navigationService.Navigate("UserProfile");
        }

        [RelayCommand] private void NavigateToAuthorization() => _navigationService.Navigate("Authorization");

        private void ShowInformationWindow(string message)
        {
            var info = new InformationWindow
            {
                Message = message,
                Owner = Application.Current.MainWindow
            };
            info.ShowDialog();
        }
    }
}
