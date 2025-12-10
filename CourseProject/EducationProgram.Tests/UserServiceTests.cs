using EducationProgram.DataBaseContext;
using EducationProgram.Models;
using EducationProgram.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Xunit;

namespace EducationProgram.Tests
{
    public class UserServiceTests
    {
        private async Task<EducationDbContext> GetInMemoryDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<EducationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // уникальная база для каждого теста
                .Options;

            var context = new EducationDbContext(options);

            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            return context;
        }

        [Fact]
        public async Task DeleteUserAsync_UserExists_ReturnsTrue()
        {
            // Создаём in-memory базу данных
            var context = await GetInMemoryDbContextAsync();

            // Создаём роль
            var role = new Role { RoleId = 1, RoleName = "Student" };
            context.Roles.Add(role);

            // Создаём пользователя, который должен быть удалён
            var user = new User
            {
                UserId = 1,         
                RoleId = 1,         
                Login = "alice123",
                Name = "Alice",
                Surname = "Smith",
                Patronymic = "A.",
                Password = "password123"
            };

            // Добавляем пользователя в in-memory базу
            context.Users.Add(user);

            // Сохраняем изменения, имитируя работу реальной БД
            await context.SaveChangesAsync();

            // Создаём сервисы, как в реальном приложении
            var dataAccess = new DataAccessService(context);
            var service = new UserService(dataAccess);

            // Пытаемся удалить пользователя с ID = 1
            var result = await service.DeleteUserAsync(1);

            // Метод должен вернуть true — пользователь найден и удалён
            Assert.True(result);

            // В базе больше не должно быть пользователя с UserId = 1
            Assert.Null(await context.Users.FindAsync(1));
        }

        //[Fact]
        //public async Task DeleteUserAsync_UserDoesNotExist_ReturnsFalse()
        //{
        //    var context = await GetInMemoryDbContextAsync();
        //    var dataAccess = new DataAccessService(context);
        //    var service = new UserService(dataAccess);

        //    var result = await service.DeleteUserAsync(42); // Пользователя с ID 42 нет

        //    Assert.False(result);
        //}
    }
}
