using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducationProgram.Models;
using EducationProgram.Services;
using EducationProgram.Windows;
using System.Collections.ObjectModel;
using System.Windows;

namespace EducationProgram.ViewModels
{
    public partial class UserManagmentViewModel : ObservableObject
    {
        private readonly UserService _userService;
        private readonly User _targetUser;
        private readonly bool _isNew;

        // --- Поля для TextBox ---
        [ObservableProperty] private string userLogin;
        [ObservableProperty] private string userPassword;
        [ObservableProperty] private string userSurname;
        [ObservableProperty] private string userName;
        [ObservableProperty] private string userPatronymic;

        // --- Коллекции для ComboBox ---
        [ObservableProperty] private ObservableCollection<Role> roles;
        [ObservableProperty] private ObservableCollection<UserGroup> groups;

        // --- Выбранные элементы ---
        [ObservableProperty] private Role? selectedRole;
        [ObservableProperty] private UserGroup? selectedGroup;

        // --- UI управление ---
        [ObservableProperty] private string windowTitle;
        [ObservableProperty] private bool isGroupVisible;
        [ObservableProperty] private bool isRoleEditable;

        // Событие для закрытия окна
        public Action<bool?>? CloseRequest;

        [ObservableProperty] private string errorMessage = string.Empty;
        [ObservableProperty] private Visibility errorVisibility = Visibility.Collapsed;

        public UserManagmentViewModel(User user, List<Role> allRoles, List<UserGroup> allGroups, bool isNew, UserService userService, bool isTeacherMode = false)
        {
            _targetUser = user;
            _isNew = isNew;
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

            Roles = new ObservableCollection<Role>(allRoles);
            Groups = new ObservableCollection<UserGroup>(allGroups);

            UserLogin = user.Login ?? string.Empty;
            UserPassword = user.Password ?? string.Empty;
            UserSurname = user.Surname ?? string.Empty;
            UserName = user.Name ?? string.Empty;
            UserPatronymic = user.Patronymic ?? string.Empty;

            WindowTitle = isNew ? "Создание преподавателя" : "Редактирование пользователя";

            if (isTeacherMode)
            {
                SelectedRole = Roles.FirstOrDefault(r => r.RoleName == "Преподаватель");
                IsRoleEditable = false;
                IsGroupVisible = false;
            }
            else
            {
                SelectedRole = Roles.FirstOrDefault(r => r.RoleId == user.RoleId);
                IsRoleEditable = true;
                CheckGroupVisibility();
            }

            if (user.GroupId != null && user.GroupId != 0)
            {
                SelectedGroup = Groups.FirstOrDefault(g => g.GroupId == user.GroupId);
            }
        }

        partial void OnSelectedRoleChanged(Role? value)
        {
            CheckGroupVisibility();
        }

        private void CheckGroupVisibility()
        {
            bool isStudent = SelectedRole?.RoleName == "Студент";
            IsGroupVisible = isStudent;

            if (!isStudent)
            {
                SelectedGroup = null;
            }
        }

        private bool Validate(out string errorMessage)
        {
            string loginTrimmed = UserLogin?.Trim() ?? string.Empty;
            string passwordTrimmed = UserPassword?.Trim() ?? string.Empty;
            string surnameTrimmed = UserSurname?.Trim() ?? string.Empty;
            string nameTrimmed = UserName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(loginTrimmed) ||
                string.IsNullOrWhiteSpace(passwordTrimmed) ||
                string.IsNullOrWhiteSpace(surnameTrimmed) ||
                string.IsNullOrWhiteSpace(nameTrimmed))
            {
                errorMessage = "Все поля должны быть заполнены.";
                return false;
            }    

            if (SelectedRole == null)
            {
                errorMessage = "Выберите роль пользователя.";
                return false;
            }

            if (passwordTrimmed.Length < 8)
            {
                errorMessage = "Пароль должен быть не менее 8 символов.";
                return false;
            }

            if (passwordTrimmed.Contains(" "))
            {
                errorMessage = "Пароль не должен содержать пробелов.";
                return false;
            }

            if (IsGroupVisible && SelectedGroup == null)
            {
                errorMessage = "Для студента необходимо выбрать группу.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
        private void UpdateTargetUser()
        {
            _targetUser.Login = UserLogin.Trim(); 
            _targetUser.Password = UserPassword.Trim(); 
            _targetUser.Surname = UserSurname.Trim(); 
            _targetUser.Name = UserName.Trim(); 
            _targetUser.Patronymic = UserPatronymic.Trim();
            _targetUser.Role = SelectedRole;
            _targetUser.RoleId = SelectedRole?.RoleId ?? 0;
            _targetUser.Group = IsGroupVisible ? SelectedGroup : null;
            _targetUser.GroupId = IsGroupVisible && SelectedGroup != null ? SelectedGroup.GroupId : null;
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = Visibility.Visible;
        }

        [RelayCommand]
        private async Task Save()
        {
            ErrorVisibility = Visibility.Collapsed;

            if (!Validate(out var error))
            {
                ShowError(error);
                return;
            }

            try
            {
                int? excludeId = _isNew ? null : _targetUser.UserId;

                bool isTaken = await _userService.IsLoginTakenAsync(UserLogin.Trim(), excludeId);

                if (isTaken)
                {
                    ShowError($"Логин '{UserLogin}' уже занят другим пользователем.");
                    return;
                }

                UpdateTargetUser();

                if (_isNew)
                {
                    await _userService.AddUserAsync(_targetUser);
                }
                else
                {
                    await _userService.UpdateUserAsync(_targetUser);
                }

                CloseRequest?.Invoke(true);
            }
            catch (Exception ex)
            {
                ShowError($"Критическая ошибка сохранения: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequest?.Invoke(false);
        }
    }
}
