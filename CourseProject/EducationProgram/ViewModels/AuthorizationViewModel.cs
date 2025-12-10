using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducationProgram.Services;
using System.Windows;

namespace EducationProgram.ViewModels
{
    public partial class AuthorizationViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private readonly UserService _userService; 

        [ObservableProperty]
        private string login = string.Empty;

        [ObservableProperty]
        private string password; 

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private Visibility errorVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private bool isPasswordVisible = false; 

        public string PasswordVisibilityIcon => IsPasswordVisible
            ? "pack://application:,,,/Resources/Images/eye_open.png"
            : "pack://application:,,,/Resources/Images/eye_close.png"; 

        public AuthorizationViewModel(NavigationService navigationService, UserService userService) 
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
            OnPropertyChanged(nameof(PasswordVisibilityIcon));
        }

        [RelayCommand]
        private async Task LoginUser()
        {
            ErrorVisibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Введите логин и пароль.");
                return;
            }

            try
            {
                bool isAuthenticated = await _userService.Login(Login, Password);

                if (isAuthenticated)
                {
                    _navigationService.Navigate("Education");
                }
                else
                {
                    ShowError("Неверный логин или пароль.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при попытке входа: {ex.Message}");
            }
        }

        [RelayCommand]
        private void GoToRegistration()
        {
            _navigationService.Navigate("Registration");
        }

        [RelayCommand]
        private void Back()
        {
            _navigationService.Navigate("Education");
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = Visibility.Visible;
        }
    }
}