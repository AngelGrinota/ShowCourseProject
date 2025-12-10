using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducationProgram.Helpers;
using EducationProgram.Models;
using EducationProgram.Services;
using EducationProgram.Windows;
using System.Collections.ObjectModel;
using System.Windows;

namespace EducationProgram.ViewModels
{
    public partial class UserProfileViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private readonly UserService _userService;
        private readonly DataAccessService _dataAccessService;

        [ObservableProperty] private User? currentUser;
        [ObservableProperty] private string selectedSection;
        [ObservableProperty] private bool isAdmin;
        [ObservableProperty] private bool isTeacher;
        [ObservableProperty] private bool isStudent;

        // --- Пагинаторы ---
        public PagingHelper<TestResultDisplay> TestPaging { get; } = new();
        public PagingHelper<Leaderboard> LeaderboardPaging { get; } = new();
        public PagingHelper<TestResultDisplay> StudentPaging { get; } = new();
        public PagingHelper<User> UsersPaging { get; } = new();
        public PagingHelper<Test> TestsPaging { get; } = new();
        public PagingHelper<Paragraph> ParagraphsPaging { get; } = new();

        // --- Полные списки данных ---
        private List<TestResultDisplay> _allTestResults = new();
        private List<Leaderboard> _allLeaderboard = new();
        private List<TestResultDisplay> _allStudentResults = new();
        private List<User> _allUsers = new();
        private List<Test> _allTests = new();
        private List<Paragraph> _allParagraphs = new();
        private List<Theme> _allThemes = new();
        private List<User> _filteredUsers = new();

        [ObservableProperty] private ObservableCollection<Test> testsDisplayList = new();
        [ObservableProperty] private ObservableCollection<Paragraph> paragraphsDisplayList = new();

        // Свойства для раздела "Материалы"
        [ObservableProperty] private string selectedMaterialType = "Тесты";
        [ObservableProperty] private bool isShowingMaterialList = true;
        [ObservableProperty] private bool isShowingTestCreation = false;

        // Свойства для создания теста
        [ObservableProperty] private ObservableCollection<Section> importSections = new();
        [ObservableProperty] private Section? selectedImportSection;
        [ObservableProperty] private ObservableCollection<Theme> importThemes = new();
        [ObservableProperty] private Theme? selectedImportTheme;
        [ObservableProperty] private string newTestName = string.Empty;
        [ObservableProperty] private int newTestMaxAttempts = 3;

        private List<TestResultDisplay> _filteredResultsForExport = new();

        // --- Фильтры ---
        
        [ObservableProperty] private ObservableCollection<UserGroup> filterGroups = new();
        [ObservableProperty] private ObservableCollection<Test> filterTests = new();
        [ObservableProperty] private ObservableCollection<int> filterGrades = new() { 1, 2, 3, 4, 5 };

        [ObservableProperty] private UserGroup? selectedGroupFilter;
        [ObservableProperty] private Test? selectedTestFilter;
        [ObservableProperty] private int? selectedGradeFilter;
        [ObservableProperty] private string searchFio = string.Empty;
        [ObservableProperty] private string searchUserFio = string.Empty;

        [ObservableProperty] private int dateSortState = 0; // 0: Сброс, 1: Возрастание, 2: Убывание
        [ObservableProperty] private bool isFioAscending = true;

        // NEW PROPERTIES FOR NAVIGATION
        [ObservableProperty] private UserManagmentViewModel? userEditVm;
        [ObservableProperty] private bool isEditingUser = false;

        [ObservableProperty] private string errorMessage = string.Empty;
        [ObservableProperty] private Visibility errorVisibility = Visibility.Collapsed;

        public UserProfileViewModel(NavigationService navigationService, UserService userService, DataAccessService dataAccessService)
        {
            _navigationService = navigationService;
            _userService = userService;
            _dataAccessService = dataAccessService;

            CurrentUser = _userService.CurrentUser;
            UpdateRoleProperties();

            _userService.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(UserService.CurrentUser))
                {
                    CurrentUser = _userService.CurrentUser;
                    UpdateRoleProperties();
                    await LoadDataAsync();
                }
            };

            _ = LoadDataAsync();
        }
        private void UpdateRoleProperties()
        {
            IsAdmin = _userService.IsAdmin;
            IsTeacher = _userService.IsTeacher;
            IsStudent = _userService.IsStudent;

            if (IsStudent) SelectedSection = "Пройденные тесты";
            else if (IsTeacher) SelectedSection = "Успеваемость";
            else if (IsAdmin) SelectedSection = "Пользователи";
        }
        private async Task LoadDataAsync()
        {
            if (IsStudent) await LoadTestResultsAsync();
            else if (IsTeacher) await LoadStudentPerformanceAsync();
            else if (IsAdmin) await LoadAdminDataAsync();
        }
        private async Task LoadAdminDataAsync()
        {
            await LoadUsersManagementAsync();
            await LoadMaterialsManagementAsync();

            // Загрузка для формы создания теста (раз в жизни)
            var sections = await _dataAccessService.GetSections();
            ImportSections = new ObservableCollection<Section>(sections);
        }
        private async Task LoadTestResultsAsync()
        {
            if (CurrentUser == null) return;
            try
            {
                _allTestResults = await _dataAccessService.GetTestResultDisplaysForUser(CurrentUser.UserId);
                TestPaging.Update(_allTestResults);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке результатов тестов: {ex.Message}"); //
            }
        }
        private async Task LoadLeaderboardAsync()
        {
            try
            {
                _allLeaderboard = await _dataAccessService.GetLeaderboard();
                LeaderboardPaging.Update(_allLeaderboard);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке лидерборда: {ex.Message}"); //
            }
        }
        private async Task LoadStudentPerformanceAsync()
        {
            try
            {
                var groups = await _dataAccessService.GetGroups();
                FilterGroups = new ObservableCollection<UserGroup>(groups);

                var tests = await _dataAccessService.GetTest();
                FilterTests = new ObservableCollection<Test>(tests);

                _allStudentResults = await _dataAccessService.GetAllTestResultDisplaysAsync();
                ApplyStudentFilters();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки успеваемости: {ex.Message}"); //
            }
        }

        private async Task LoadUsersManagementAsync() // Обновлено
        {
            try
            {
                var sections = await _dataAccessService.GetSections();
                ImportSections = new ObservableCollection<Section>(sections);

                _allThemes = await _dataAccessService.GetThemes();
                _allUsers = await _userService.GetAllUsersAsync();
                ApplyUserFilters();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        // --- Загрузка данных для вкладки Материалы ---
        private async Task LoadMaterialsManagementAsync()
        {
            try
            {
                _allTests = await _dataAccessService.GetTest();
                TestsPaging.Update(_allTests);

                _allParagraphs = await _dataAccessService.GetParagraphs();
                ParagraphsPaging.Update(_allParagraphs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки материалов: {ex.Message}");
            }
        }

        // --- Фильтрация и сортировка студентов и пользователей ---
        partial void OnSelectedGroupFilterChanged(UserGroup? value) => ApplyStudentFilters();
        partial void OnSelectedTestFilterChanged(Test? value) => ApplyStudentFilters();
        partial void OnSelectedGradeFilterChanged(int? value) => ApplyStudentFilters();
        partial void OnSearchFioChanged(string value) => ApplyStudentFilters();
        partial void OnSearchUserFioChanged(string value) => ApplyUserFilters();
        partial void OnSelectedImportSectionChanged(Section? value)
        {
            SelectedImportTheme = null;
            ImportThemes.Clear();

            if (value != null)
            {
                var filteredThemes = _allThemes.Where(t => t.SectionId == value.SectionId).ToList();

                foreach (var theme in filteredThemes)
                {
                    ImportThemes.Add(theme);
                }
            }
        }
        private void ApplyUserFilters()
        {
            IEnumerable<User> query = _allUsers
                .Where(u => u.Role != null && u.Role.RoleName != "Администратор");

            if (!string.IsNullOrWhiteSpace(SearchUserFio))
            {
                query = query.Where(u =>
                    MatchFio(
                        GetFullFio(u.Surname, u.Name, u.Patronymic),
                        SearchUserFio
                    ));
            }

            // Сортировка по ФИО
            query = OrderByFio(query, u => u.Surname, u => u.Name, u => u.Patronymic, true);

            _filteredUsers = query.ToList();
            UsersPaging.Update(_filteredUsers);
        }
        private void ApplyStudentFilters()
        {
            IEnumerable<TestResultDisplay> filtered = _allStudentResults;

            // --- Фильтрация по ФИО ---
            if (!string.IsNullOrWhiteSpace(SearchFio))
            {
                filtered = filtered.Where(r =>
                    MatchFio(
                        GetFullFio(r.Surname, r.Name, r.Patronymic),
                        SearchFio
                    ));
            }

            // --- Фильтрация по группе ---
            if (SelectedGroupFilter != null)
                filtered = filtered.Where(r => r.UserGroup == SelectedGroupFilter.GroupName);

            // --- Фильтрация по тесту ---
            if (SelectedTestFilter != null)
                filtered = filtered.Where(r => r.TestId == SelectedTestFilter.TestId);

            // --- Фильтрация по оценке ---
            if (SelectedGradeFilter.HasValue && SelectedGradeFilter > 0)
                filtered = filtered.Where(r => r.Grade == SelectedGradeFilter.Value);

            // --- Сортировка ---
            filtered = ApplyStudentSorting(filtered);

            _filteredResultsForExport = filtered.ToList();
            StudentPaging.Update(_filteredResultsForExport);
        }
        private IOrderedEnumerable<TestResultDisplay> ApplyStudentSorting(IEnumerable<TestResultDisplay> query)
        {
            // Сортировка по дате
            if (DateSortState != 0)
            {
                var ordered = DateSortState == 1
                    ? query.OrderBy(x => x.PassedAt)
                    : query.OrderByDescending(x => x.PassedAt);

                // вторичная сортировка ФИО — всегда ВОЗРАСТАНИЕ
                return ordered
                    .ThenBy(x => x.Surname)
                    .ThenBy(x => x.Name)
                    .ThenBy(x => x.Patronymic);
            }

            // Сортировка по ФИО
            return IsFioAscending
                ? query.OrderBy(x => x.Surname)
                       .ThenBy(x => x.Name)
                       .ThenBy(x => x.Patronymic)
                : query.OrderByDescending(x => x.Surname)
                       .ThenByDescending(x => x.Name)
                       .ThenByDescending(x => x.Patronymic);
        }

        private static string GetFullFio(string surname, string name, string? patronymic)
        {
            return $"{surname} {name} {patronymic ?? ""}".Trim();
        }

        private static bool MatchFio(string fio, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return true;

            var terms = search
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.ToLowerInvariant());

            var fioLower = fio.ToLowerInvariant();

            return terms.All(t => fioLower.Contains(t));
        }

        private static IOrderedEnumerable<T> OrderByFio<T>(
            IEnumerable<T> items,
            Func<T, string> surname,
            Func<T, string> name,
            Func<T, string?> patronymic,
            bool ascending)
        {
            return ascending
                ? items.OrderBy(surname).ThenBy(name).ThenBy(patronymic)
                : items.OrderByDescending(surname).ThenByDescending(name).ThenByDescending(patronymic);
        }

        // --- Команды для пагинации ---
        [RelayCommand] private void NextPage() => ChangePage(true);
        [RelayCommand] private void PrevPage() => ChangePage(false);

        private void ChangePage(bool next)
        {
            switch (SelectedSection)
            {
                case "Пройденные тесты":
                    if (next) TestPaging.Next(_allTestResults);
                    else TestPaging.Prev(_allTestResults);
                    break;
                case "Топ участников":
                    if (next) LeaderboardPaging.Next(_allLeaderboard);
                    else LeaderboardPaging.Prev(_allLeaderboard);
                    break;
                case "Успеваемость":
                    if (next) StudentPaging.Next(_allStudentResults);
                    else StudentPaging.Prev(_allStudentResults);
                    break;
                case "Пользователи":
                    if (next) UsersPaging.Next(_filteredUsers);
                    else UsersPaging.Prev(_filteredUsers);
                    break;

                case "Материалы":
                    if (SelectedMaterialType == "Тесты")
                    {
                        if (next) TestsPaging.Next(_allTests);
                        else TestsPaging.Prev(_allTests);
                    }
                    else if (SelectedMaterialType == "Параграфы")
                    {
                        if (next) ParagraphsPaging.Next(_allParagraphs);
                        else ParagraphsPaging.Prev(_allParagraphs);
                    }
                    break;
            }
        }

        // --- Команды сортировки ---
        [RelayCommand]
        private void ToggleDateSort()
        {
            DateSortState = DateSortState switch { 0 => 2, 2 => 1, _ => 0 };
            ApplyStudentFilters();
        }

        [RelayCommand]
        private void ToggleFioSort()
        {
            IsFioAscending = !IsFioAscending;
            DateSortState = 0;
            ApplyStudentFilters();
        }

        [RelayCommand]
        private void ResetFilters()
        {
            SelectedGroupFilter = null;
            SelectedTestFilter = null;
            SelectedGradeFilter = null;
            SearchFio = string.Empty;
        }

        //

        [RelayCommand]
        /// <summary>
        /// Команда экспорта результатов тестирования в Excel-файл.
        /// Вызывает универсальный сервис экспорта и обрабатывает возможные ошибки.
        /// </summary>
        private void ExportResults()
        {
            try
            {
                // Передаём отфильтрованную коллекцию результатов и имя создаваемого файла
                ExcelService.ExportToExcel(_filteredResultsForExport, "Результаты_студентов.xlsx");
            }
            catch (System.IO.IOException ioEx)
            {
                // Обработка распространённой ошибки: файл занят или открыт другой программой
                if (ioEx.Message.Contains("занят другим процессом") ||
                    ioEx.HResult == -2147024864 || ioEx.HResult == 32)
                {
                    ShowInformationWindow("Ошибка экспорта: Файл открыт в другом приложении. Пожалуйста, закройте его и повторите попытку.");
                }
                else
                {
                    // Любые другие ошибки ввода–вывода
                    ShowInformationWindow($"Ошибка ввода-вывода при экспорте: {ioEx.Message}");
                }
                return;
            }
            catch (Exception ex)
            {
                // Общий обработчик на случай всех непредвиденных исключений
                ShowInformationWindow($"Общая ошибка экспорта: {ex.Message}");
                return;
            }
        }


        [RelayCommand]
        /// <summary>
        /// Импортирует тестовые вопросы из Excel-файла и создаёт новый тест.
        /// Выполняет предварительную валидацию данных, проверяет корректность импортированных строк
        /// и сохраняет тест вместе с вопросами в базу данных.
        /// </summary>
        private async Task ImportTest()
        {
            // Проверяем корректность введённых пользователем параметров
            if (!ValidateImportInput())
                return;
            try
            {
                // Импортируем вопросы из Excel-файла в виде списка DTO-объектов
                var importedQuestions = ExcelService.ImportFromExcel<TestImportDto>();
                // Проверка: если файл пустой или содержит неверные заголовки — прекращаем импорт
                if (importedQuestions.Count == 0)
                {
                    ShowError("Файл пустой или не содержит допустимых данных!");
                    return;
                }
                // Проверяем, есть ли строки с некорректными или неполными данными
                var invalidRows = importedQuestions
                    .Where(q => string.IsNullOrWhiteSpace(q.QuestionText) ||
                                string.IsNullOrWhiteSpace(q.AnswerText) ||
                                q.Points <= 0)
                    .ToList();
                // Если найдены ошибочные вопросы — информируем пользователя и прерываем импорт
                if (invalidRows.Any())
                {
                    ShowError("Файл содержит пустые или некорректные строки. Проверьте данные.");
                    return;
                }
                // Создание нового теста и сохранение его вопросов в базе данных
                await _dataAccessService.CreateTestWithQuestionsAsync(
                    NewTestName,
                    SelectedImportTheme.ThemeId,
                    NewTestMaxAttempts,
                    importedQuestions
                );
                // Обновляем список материалов после успешного импорта
                await LoadMaterialsManagementAsync();
                // Сбрасываем состояние формы создания/импорта теста
                CancelTestCreation();
                // Уведомляем пользователя об успешном завершении операции
                ShowInformationWindow($"Тест '{NewTestName}' успешно импортирован и создан.");
            }
            catch (Exception ex)
            {
                // Обработка любых непредвиденных ошибок на этапе импорта или сохранения
                ShowError($"Ошибка при импорте: {ex.Message}");
            }
        }

        private bool ValidateImportInput()
        {
            ErrorVisibility = Visibility.Collapsed;

            if (SelectedImportSection == null)
            {
                ShowError("Выберите раздел!");
                return false;
            }

            if (SelectedImportTheme == null)
            {
                ShowError("Выберите тему!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewTestName))
            {
                ShowError("Введите название теста!");
                return false;
            }

            if (_allTests.Any(t => t.TestName.Equals(NewTestName, StringComparison.OrdinalIgnoreCase)))
            {
                ShowError("Тест с таким названием уже существует!");
                return false;
            }

            if (NewTestMaxAttempts < 1)
            {
                ShowError("Количество попыток должно быть больше 0!");
                return false;
            }

            return true;
        }

        // --- Навигация ---
        [RelayCommand] private void NavigateBack() => _navigationService.Navigate("Education");

        [RelayCommand]
        private async Task SelectProfileSection(string sectionName)
        {
            SelectedSection = sectionName;
            switch (sectionName)
            {
                case "Пройденные тесты": await LoadTestResultsAsync(); break;
                case "Топ участников": await LoadLeaderboardAsync(); break;
                case "Успеваемость": await LoadStudentPerformanceAsync(); break;
                case "Пользователи": await LoadUsersManagementAsync(); break;
                case "Материалы": await LoadMaterialsManagementAsync(); break;
            }
        }

        [RelayCommand]
        private async Task CreateTeacher()
        {
            var newUser = new User();
            await OpenManagmentView(newUser, isNew: true, isTeacherMode: true, onSaved: (user) =>
            {
                _allUsers.Add(user);
                ApplyUserFilters();
            });
        }

        [RelayCommand]
        private async Task EditUser(User? user)
        {
            if (user == null) return;

            await OpenManagmentView(user, isNew: false, isTeacherMode: false, onSaved: (user) =>
            {
                ApplyUserFilters();
            });
        }

        [RelayCommand]
        private async Task DeleteUser(User? user)
        {
            if (user == null) return;

            var window = new DeleteConfirmationWindow(
                $"Вы действительно хотите удалить пользователя {user.Login}?",
                "Удаление пользователя необратимо."
            );

            window.ShowDialog();

            if (window.IsConfirmed)
            {
                bool success = await _userService.DeleteUserAsync(user.UserId);
                if (success)
                {
                    var userToRemove = _allUsers.FirstOrDefault(u => u.UserId == user.UserId);
                    if (userToRemove != null)
                        _allUsers.Remove(userToRemove);

                    ApplyUserFilters();
                    ShowInformationWindow("Пользователь успешно удален.");
                }
                else
                {
                    ShowInformationWindow("Ошибка при удалении пользователя.");
                }
            }
        }

        // Тесты и параграфы

        [RelayCommand]
        private void OpenTestCreationView()
        {
            IsShowingMaterialList = false;
            IsShowingTestCreation = true;

            SelectedImportSection = null;
            SelectedImportTheme = null;
            NewTestName = string.Empty;
            NewTestMaxAttempts = 3;

            ErrorMessage = string.Empty;
            ErrorVisibility = Visibility.Collapsed;
        }

        [RelayCommand] // NEW: Закрывает форму создания теста
        private void CancelTestCreation()
        {
            IsShowingMaterialList = true;
            IsShowingTestCreation = false;
        }

        [RelayCommand]
        private async Task DeleteTest(Test testToDelete)
        {
            if (testToDelete == null) return;

            var window = new DeleteConfirmationWindow(
                $"Вы уверены, что хотите безвозвратно удалить тест '{testToDelete.TestName}'?",
                "Удаление теста нельзя отменить."
            );

            window.ShowDialog();

            if (window.IsConfirmed)
            {
                bool isSuccess = await _dataAccessService.DeleteTestAsync(testToDelete.TestId);
                if (isSuccess)
                {
                    _allTests.Remove(testToDelete);
                    TestsPaging.Update(_allTests);
                    ShowInformationWindow($"Тест '{testToDelete.TestName}' успешно удален.");
                }
                else
                {
                    ShowInformationWindow($"Ошибка при удалении теста '{testToDelete.TestName}'.");
                }
            }
        }

        //
        private async Task OpenManagmentView(User user, bool isNew, bool isTeacherMode, Action<User> onSaved)
        {
            var roles = await _dataAccessService.GetRoles();
            var groups = await _dataAccessService.GetGroups();

            var filtredRoles = roles.Where(r => r.RoleName != "Администратор").ToList();

            var vm = new UserManagmentViewModel(user, filtredRoles, groups, isNew, _userService, isTeacherMode);

            vm.CloseRequest += async result =>
            {
                if (result == true)
                {
                    await LoadUsersManagementAsync();
                }

                _navigationService.Navigate("UserProfile");
            };

            _navigationService.NavigateToInstance(vm);
        }

        private void ShowInformationWindow(string message)
        {
            var info = new InformationWindow
            {
                Message = message,
                Owner = Application.Current.MainWindow
            };
            info.ShowDialog();
        }
        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = Visibility.Visible;
        }
    }
}
