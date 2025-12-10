using EducationProgram.DataBaseContext;
using EducationProgram.Models;
using Microsoft.EntityFrameworkCore;

namespace EducationProgram.Services
{
    public class DataAccessService
    {
        private readonly EducationDbContext _context;

        public DataAccessService(EducationDbContext context)
        {
            _context = context;
        }

        //GET

        public async Task<List<Theme>> GetThemes() => await _context.Themes.AsNoTracking().ToListAsync();

        /// <summary>
        /// Возвращает полный список разделов (Sections) с включёнными связанными сущностями.
        /// </summary>
        /// <remarks>
        /// Метод загружает связанные данные:
        /// - Темы (Themes)
        /// - Параграфы (Paragraphs)
        /// - Тесты (Tests)
        /// Используется AsNoTracking, так как данные не редактируются.
        /// </remarks>
        /// <returns>
        /// Список разделов с вложенными темами, параграфами и тестами.
        /// </returns>
        public async Task<List<Section>> GetSections() => await _context.Sections
            .AsNoTracking()
            .Include(s => s.Themes)
                .ThenInclude(t => t.Paragraphs)
            .Include(s => s.Themes)
                .ThenInclude(t => t.Tests)
            .ToListAsync();


        /// <summary>
        /// Возвращает список параграфов с информацией о связанных темах и разделах.
        /// </summary>
        /// <remarks>
        /// Дозагружается:
        /// - Тема (Theme)
        /// - Раздел (Section) через Theme
        /// </remarks>
        /// <returns>Список параграфов с навигационными свойствами.</returns>
        public async Task<List<Paragraph>> GetParagraphs() => await _context.Paragraphs
            .AsNoTracking()
            .Include(p => p.Theme)
                .ThenInclude(t => t.Section)
            .ToListAsync();


        /// <summary>
        /// Возвращает список всех пользователей с включёнными ролями и учебными группами.
        /// </summary>
        /// <returns>
        /// Коллекция пользователей, содержащая связанные сущности Role и Group.
        /// </returns>
        public async Task<List<User>> GetUsers() => await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Group)
            .ToListAsync();


        /// <summary>
        /// Возвращает список тестов с информацией о темах и разделах.
        /// </summary>
        /// <returns>
        /// Коллекция тестов со связанными сущностями Theme и Section.
        /// </returns>
        public async Task<List<Test>> GetTest() => await _context.Tests
            .AsNoTracking()
            .Include(t => t.Theme)
                .ThenInclude(t => t.Section)
            .ToListAsync();


        /// <summary>
        /// Возвращает список результатов тестирования для конкретного пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>
        /// Отсортированный по дате список отображаемых результатов тестов (<see cref="TestResultDisplay"/>).
        /// </returns>
        public async Task<List<TestResultDisplay>> GetTestResultDisplaysForUser(int userId) => await _context.TestResultDisplays
            .AsNoTracking()
            .Where(trd => trd.UserId == userId)
            .OrderByDescending(trd => trd.PassedAt)
            .ToListAsync();


        /// <summary>
        /// Возвращает таблицу рейтинга пользователей, отсортированную по общему количеству баллов.
        /// </summary>
        /// <returns>
        /// Коллекция элементов лидерборда, упорядоченная по убыванию TotalScore.
        /// </returns>
        public async Task<List<Leaderboard>> GetLeaderboard() => await _context.Leaderboards
            .AsNoTracking()
            .OrderByDescending(l => l.TotalScore)
            .ToListAsync();

        public async Task<List<Role>> GetRoles() => await _context.Roles.AsNoTracking().ToListAsync();

        public async Task<List<UserGroup>> GetGroups() => await _context.UserGroups.AsNoTracking().OrderBy(g => g.GroupName).ToListAsync();

        public async Task<List<Question>> GetFullQuestionsForTest(int testId) => await _context.Questions
            .Where(q => q.TestId == testId)
            .Include(q => q.Answers)
            .AsNoTracking()
            .ToListAsync();

        //DELETE

        /// <summary>
        /// Удаляет пользователя по его идентификатору.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя, которого требуется удалить.</param>
        /// <returns>
        /// true, если пользователь был найден и успешно удалён; 
        /// false, если пользователь с указанным ID отсутствует в базе данных.
        /// </returns>
        public async Task<bool> DeleteUserAsync(int userId)
        {
            // Ищем пользователя по первичному ключуы
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false; // Пользователь не найден
            // Удаляем пользователя из контекста
            _context.Users.Remove(user);
            // Фиксируем изменения в базе данных
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Удаляет тест по его идентификатору.
        /// </summary>
        /// <param name="testId">Идентификатор теста, который требуется удалить.</param>
        /// <returns>
        /// true, если тест найден и успешно удалён;
        /// false, если тест отсутствует в базе данных.
        /// </returns>
        public async Task<bool> DeleteTestAsync(int testId)
        {
            // Пытаемся найти тест по его ID
            var testToDelete = await _context.Tests.FindAsync(testId);
            if (testToDelete == null) return false; // Тест не найден
            // Удаляем тест
            _context.Tests.Remove(testToDelete);
            // Сохраняем изменения
            await _context.SaveChangesAsync();
            return true;
        }


        //OTHER

        // CREATE

        /// <summary>
        /// Добавляет нового пользователя в базу данных.
        /// </summary>
        /// <param name="newUser">
        /// Экземпляр пользователя, содержащий данные для сохранения. 
        /// Навигационные свойства могут быть не заполнены, достаточно указать идентификаторы роли и группы.
        /// </param>
        /// <returns>
        /// Возвращает созданного пользователя с загруженными навигационными свойствами 
        /// (ролью и группой). 
        /// </returns>
        public async Task<User?> AddUserAsync(User newUser)
        {
            newUser.Role = null;
            newUser.Group = null;

            _context.Users.Add(newUser);

            await _context.SaveChangesAsync();

            await _context.Entry(newUser).Reference(u => u.Role).LoadAsync();

            if (newUser.GroupId.HasValue)
            {
                await _context.Entry(newUser).Reference(u => u.Group).LoadAsync();
            }

            return newUser;
        }


        // UPDATE

        /// <summary>
        /// Обновляет данные существующего пользователя в базе данных.
        /// </summary>
        /// <param name="updatedUser">
        /// Объект пользователя с обновлёнными данными. 
        /// Должен содержать корректный идентификатор UserId.
        /// </param>
        /// <returns>
        /// Возвращает обновлённого пользователя с загруженными навигационными свойствами
        /// (ролью и группой), либо null, если пользователь с указанным ID не найден.
        /// </returns>
        public async Task<User?> UpdateUserAsync(User updatedUser)
        {
            // Ищем пользователя в базе данных по его ID
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == updatedUser.UserId);
            if (existingUser == null) return null; // Если не найден — возвращаем null
            // Обновляем основные поля пользователя
            existingUser.Login = updatedUser.Login;
            existingUser.Password = updatedUser.Password;
            existingUser.Name = updatedUser.Name;
            existingUser.Surname = updatedUser.Surname;
            existingUser.Patronymic = updatedUser.Patronymic;
            // Обновляем связи: роль и учебная группа
            existingUser.RoleId = updatedUser.RoleId;
            existingUser.GroupId = updatedUser.GroupId;
            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();
            // Загружаем навигационные свойства, чтобы вернуть полностью заполненный объект
            await _context.Entry(existingUser).Reference(u => u.Role).LoadAsync();
            // Группа загружается только если у пользователя она вообще есть
            if (existingUser.GroupId.HasValue)
            {
                await _context.Entry(existingUser).Reference(u => u.Group).LoadAsync();
            }
            // Возвращаем актуальные данные пользователя
            return existingUser;
        }

        public async Task<int> GetUserAttemptsCount(int userId, int testId)
        {
            return await _context.TestResults
                .Where(tr => tr.UserId == userId && tr.TestId == testId)
                .CountAsync();
        }

        public async Task<List<TestResultDisplay>> GetAllTestResultDisplaysAsync()
        {
            return await _context.TestResultDisplays
                .AsNoTracking()
                .OrderByDescending(trd => trd.PassedAt)
                .ToListAsync();
        }

        public async Task<int> GetNextAttemptNumber(int userId, int testId)
        {
            var lastAttempt = await _context.TestResults
                .Where(r => r.UserId == userId && r.TestId == testId)
                .OrderByDescending(r => r.AttemptNumber)
                .FirstOrDefaultAsync();

            return lastAttempt?.AttemptNumber + 1 ?? 1;
        }
        public async Task SaveTestResult(TestResult result)
        {
            _context.TestResults.Add(result);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsLoginTakenAsync(string login, int? excludeUserId = null)
        {
            if (excludeUserId.HasValue)
            {
                return await _context.Users
                    .AnyAsync(u => u.Login == login && u.UserId != excludeUserId.Value);
            }

            return await _context.Users.AnyAsync(u => u.Login == login);
        }

        // Метод для создания теста и наполнения его вопросами из импорта
        public async Task CreateTestWithQuestionsAsync(string testName, int themeId, int maxAttempts, List<TestImportDto> importData)
        {
            if (string.IsNullOrWhiteSpace(testName)) throw new Exception("Не указано название теста.");
            if (importData == null || !importData.Any()) throw new Exception("Файл пуст или имеет неверный формат.");

            // 1. Создаем сам тест (Метаданные)
            var newTest = new Test
            {
                TestName = testName, 
                ThemeId = themeId,   
                MaxAttempts = maxAttempts 
            };

            _context.Tests.Add(newTest);
            await _context.SaveChangesAsync(); // Сохраняем, чтобы БД присвоила TestId

            // 2. Группируем строки Excel по тексту вопроса
            var groupedQuestions = importData.GroupBy(x => x.QuestionText);

            foreach (var group in groupedQuestions)
            {
                var firstItem = group.First();

                // Создаем вопрос
                var question = new Question
                {
                    TestId = newTest.TestId,
                    QuestionText = group.Key, // 
                    Points = firstItem.Points // 
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync(); // Сохраняем, чтобы получить QuestionId

                // Добавляем варианты ответов к этому вопросу
                foreach (var item in group)
                {
                    var answer = new Answer
                    {
                        QuestionId = question.QuestionId, 
                        AnswerText = item.AnswerText,     
                        IsCorrect = item.IsCorrect         
                    };
                    _context.Answers.Add(answer);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

