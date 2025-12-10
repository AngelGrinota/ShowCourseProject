using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducationProgram.Models;
using EducationProgram.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml.Linq;

namespace EducationProgram.ViewModels
{
    public partial class RegistrationViewModel : ObservableObject
    {
        private const int DefaultStudentRoleId = 1; 

        private readonly NavigationService _navigationService;
        private readonly UserService _userService;
        private readonly DataAccessService _dataAccessService;

        // --- Свойства для привязки ---
        [ObservableProperty]
        private string login = string.Empty;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string confirmPassword;

        [ObservableProperty]
        private string surname = string.Empty;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string patronymic = string.Empty;

        [ObservableProperty]
        private ObservableCollection<UserGroup> groups = new();

        [ObservableProperty]
        private UserGroup? selectedGroup;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private Visibility errorVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private bool isPasswordVisible = false;

        public string PasswordVisibilityIcon => IsPasswordVisible
            ? "pack://application:,,,/Resources/images/eye_open.png"
            : "pack://application:,,,/Resources/images/eye_close.png";

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
            OnPropertyChanged(nameof(PasswordVisibilityIcon));
        }

        [ObservableProperty]
        private bool isConfirmPasswordVisible = false;

        public string ConfirmPasswordVisibilityIcon => IsConfirmPasswordVisible
            ? "pack://application:,,,/Resources/images/eye_open.png"
            : "pack://application:,,,/Resources/images/eye_close.png";

        [RelayCommand]
        private void ToggleConfirmPasswordVisibility()
        {
            IsConfirmPasswordVisible = !IsConfirmPasswordVisible;
            OnPropertyChanged(nameof(ConfirmPasswordVisibilityIcon));
        }

        public RegistrationViewModel(NavigationService navigationService, UserService userService, DataAccessService dataAccessService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _dataAccessService = dataAccessService ?? throw new ArgumentNullException(nameof(dataAccessService));

            _ = LoadDataAsync();
        }

        // Загрузка данных
        private async Task LoadDataAsync()
        {
            try
            {
                var loadedGroups = await _dataAccessService.GetGroups();

                var sortedGroups = loadedGroups.OrderBy(g => g.GroupName).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Groups.Clear();
                    foreach (var group in sortedGroups)
                    {
                        Groups.Add(group);
                    }
                    SelectedGroup = Groups.FirstOrDefault();
                });
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Register()
        {
            ErrorVisibility = Visibility.Collapsed;

            string loginTrimmed = Login?.Trim() ?? "";
            string passwordTrimmed = Password?.Trim() ?? "";
            string confirmPasswordTrimmed = ConfirmPassword?.Trim() ?? "";
            string surnameTrimmed = Surname?.Trim() ?? "";
            string nameTrimmed = Name?.Trim() ?? "";
            string patronymicTrimmed = Patronymic?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(loginTrimmed) ||
                string.IsNullOrWhiteSpace(passwordTrimmed) ||
                string.IsNullOrWhiteSpace(confirmPasswordTrimmed) ||
                string.IsNullOrWhiteSpace(surnameTrimmed) ||
                string.IsNullOrWhiteSpace(nameTrimmed) ||
                string.IsNullOrWhiteSpace(patronymicTrimmed) ||
                SelectedGroup == null)
            {
                ShowError("Все поля обязательны для заполнения.");
                return;
            }

            if (loginTrimmed.Contains(' ') ||
                passwordTrimmed.Contains(' ') ||
                confirmPasswordTrimmed.Contains(' ') ||
                surnameTrimmed.Contains(' ') ||
                nameTrimmed.Contains(' ') ||
                patronymicTrimmed.Contains(' '))
            {
                ShowError("Пробелы не допускаются в любом поле.");
                return;
            }

            if (passwordTrimmed.Length < 8)
            {
                ShowError("Пароль должен быть не менее 8 символов.");
                return;
            }

            if (passwordTrimmed != confirmPasswordTrimmed)
            {
                ShowError("Пароли не совпадают.");
                return;
            }

            try
            {
                var registeredUser = await _userService.RegisterUser(
                    loginTrimmed,
                    passwordTrimmed,
                    surnameTrimmed,
                    nameTrimmed,
                    patronymicTrimmed,
                    DefaultStudentRoleId,
                    SelectedGroup.GroupId);

                if (registeredUser != null)
                {
                    _navigationService.Navigate("Education");
                }
                else
                {
                    ShowError("Пользователь с таким логином уже существует.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка регистрации: {ex.Message}");
            }
        }

        [RelayCommand]
        private void GoToAuthorization()
        {
            _navigationService.Navigate("Authorization");
        }

        [RelayCommand]
        private void Back()
        {
            _navigationService.Navigate("Authorization");
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = Visibility.Visible;
        }
    }
}
