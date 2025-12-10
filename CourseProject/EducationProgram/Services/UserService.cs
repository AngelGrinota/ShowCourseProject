using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Spreadsheet;
using EducationProgram.Models;

namespace EducationProgram.Services
{
    public partial class UserService : ObservableObject
    {
        private readonly DataAccessService _dataAccessService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAuthenticated), nameof(IsAdmin), nameof(IsTeacher), nameof(IsStudent))]
        private User? _currentUser;
        public bool IsAuthenticated => CurrentUser != null;
        public bool IsAdmin => CurrentUser?.Role?.RoleName == "Администратор";
        public bool IsTeacher => CurrentUser?.Role?.RoleName == "Преподаватель";
        public bool IsStudent => CurrentUser?.Role?.RoleName == "Студент";
        public UserService(DataAccessService dataAccessService)
        {
            _dataAccessService = dataAccessService;
        }

        public async Task<User?> RegisterUser(string login, string password, string surname, string name, string patronymic, int roleId, int groupId)
        {
            var newUser = new User
            {
                Login = login,
                Password = password,
                Surname = surname,
                Name = name,
                Patronymic = patronymic,
                RoleId = roleId,
                GroupId = groupId
            };

            var registeredUser = await _dataAccessService.AddUserAsync(newUser);
            if (registeredUser != null) CurrentUser = registeredUser;

            return registeredUser;
        }

        /// <summary>
        /// Аутентифицирует пользователя по логину и паролю.
        /// </summary>
        /// <param name="login">Логин пользователя.</param>
        /// <param name="password">Пароль пользователя.</param>
        /// <returns>
        /// <c>true</c>, если пользователь с указанными учетными данными найден; иначе <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Если аутентификация успешна, свойство <see cref="CurrentUser"/> устанавливается на найденного пользователя.
        /// Метод использует <see cref="DataAccessService"/> для получения списка пользователей.
        /// </remarks>
        public async Task<bool> Login(string login, string password)
        {
            // Получаем всех пользователей из базы
            var user = (await _dataAccessService.GetUsers())
                // Ищем пользователя с совпадающим логином и паролем
                .FirstOrDefault(u => u.Login == login && u.Password == password);
            // Сохраняем найденного пользователя как текущего
            CurrentUser = user;
            // Возвращаем true, если пользователь найден
            return user != null;
        }

        public void Logout() => CurrentUser = null;

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _dataAccessService.GetUsers();
        }

        public async Task<User?> AddUserAsync(User user)
        {
            return await _dataAccessService.AddUserAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            var updatedUser = await _dataAccessService.UpdateUserAsync(user);

            if (CurrentUser?.UserId == updatedUser?.UserId)
            {
                CurrentUser = updatedUser;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            return await _dataAccessService.DeleteUserAsync(userId);
        }

        public async Task<bool> IsLoginTakenAsync(string login, int? excludeUserId = null)
        {
            return await _dataAccessService.IsLoginTakenAsync(login, excludeUserId);
        }
    }
}
