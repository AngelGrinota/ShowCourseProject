using ClosedXML.Excel;
using EducationProgram.Models; // Для доступа к TestResultDisplay
using EducationProgram.Windows;
using Microsoft.Win32;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;

namespace EducationProgram.Services
{
    public static class ExcelService
    {
        private static readonly Dictionary<string, string> TestResultDisplayHeaders = new()
        {
            {"UserId", "ID пользователя"},
            {"UserLogin", "Логин"},
            {"Surname", "Фамилия"},
            {"Name", "Имя"},
            {"Patronymic", "Отчество"},
            {"UserGroup", "Группа"},
            {"TestId", "ID теста"},
            {"TestName", "Название теста"},
            {"AttemptNumber", "Попытка"},
            {"Score", "Набрано баллов"},
            {"MaxScore", "Макс. баллы"},
            {"Percent", "Процент (%)"},
            {"Grade", "Оценка"},
            {"PassedAt", "Дата сдачи"}
        };

        /// <summary>
        /// Экспортирует любую коллекцию объектов в Excel.
        /// </summary>
        public static void ExportToExcel<T>(IEnumerable<T> data, string defaultFileName = "data.xlsx")
        {
            if (!CheckDataExists(data)) return;

            var fileName = ShowSaveFileDialog(defaultFileName);
            if (fileName == null) return;

            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Лист1");

            FillHeaders<T>(ws);
            FillData(ws, data);

            ws.Columns().AdjustToContents();
            wb.SaveAs(fileName);

            ShowInformationWindow("Данные успешно экспортированы.");
        }

        /// <summary>
        /// Импорт данных из Excel в список объектов типа T.
        /// </summary>
        public static List<T> ImportFromExcel<T>() where T : new()
        {
            var fileName = ShowOpenFileDialog();
            if (fileName == null) return new List<T>();

            var wb = new XLWorkbook(fileName);
            var ws = wb.Worksheet(1);

            if (ws.RowsUsed().Count() < 2)
                throw new Exception("Файл Excel пустой!");

            var headers = GetHeaders(ws);
            var result = new List<T>();
            int rowNumber = 2;

            while (!ws.Cell(rowNumber, 1).IsEmpty())
            {
                var row = ws.Row(rowNumber);
                var item = ParseRow<T>(row, headers);
                result.Add(item);
                rowNumber++;
            }

            return result;
        }

        #region Вспомогательные методы для Export

        private static bool CheckDataExists<T>(IEnumerable<T> data)
        {
            if (data == null || !data.Any())
            {
                ShowInformationWindow("Нет данных для экспорта!");
                return false;
            }
            return true;
        }

        private static string? ShowSaveFileDialog(string defaultFileName)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = defaultFileName
            };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        private static void FillHeaders<T>(IXLWorksheet ws)
        {
            var props = typeof(T).GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                string header = props[i].Name;
                if (typeof(T) == typeof(TestResultDisplay) && TestResultDisplayHeaders.ContainsKey(header))
                {
                    header = TestResultDisplayHeaders[header];
                }
                ws.Cell(1, i + 1).Value = header;
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }
        }

        private static void FillData<T>(IXLWorksheet ws, IEnumerable<T> data)
        {
            var props = typeof(T).GetProperties();
            int row = 2;

            foreach (var item in data)
            {
                for (int col = 0; col < props.Length; col++)
                {
                    var value = props[col].GetValue(item);
                    if (value == null)
                    {
                        ws.Cell(row, col + 1).Value = Blank.Value;
                    }
                    else
                    {
                        ws.Cell(row, col + 1).Value = XLCellValue.FromObject(value);
                    }
                }
                row++;
            }
        }

        #endregion

        #region Вспомогательные методы для Import

        private static string? ShowOpenFileDialog()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx"
            };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        private static List<string> GetHeaders(IXLWorksheet ws)
        {
            return ws.Row(1).CellsUsed().Select(c => c.GetString()).ToList();
        }

        private static T ParseRow<T>(IXLRow row, List<string> headers) where T : new()
        {
            var item = new T();
            bool rowIsValid = true;
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                int colIndex = headers.FindIndex(h => string.Equals(h, prop.Name, StringComparison.OrdinalIgnoreCase));
                object? value = null;

                if (colIndex >= 0)
                {
                    var cell = row.Cell(colIndex + 1);
                    if (!cell.IsEmpty())
                    {
                        try
                        {
                            value = ConvertValue(cell.GetString(), prop.PropertyType);
                            prop.SetValue(item, value);
                        }
                        catch
                        {
                            rowIsValid = false;
                        }
                    }

                    if (prop.GetCustomAttribute<RequiredAttribute>() != null &&
                        (value == null || (value is string s && string.IsNullOrWhiteSpace(s))))
                    {
                        rowIsValid = false;
                    }
                }
                else if (prop.GetCustomAttribute<RequiredAttribute>() != null)
                {
                    rowIsValid = false;
                }
            }

            if (!rowIsValid)
                throw new Exception($"Ошибка в строке {row.RowNumber()}: пропущены обязательные данные или некорректный формат.");

            return item;
        }

        #endregion

        #region Общие вспомогательные методы

        private static object? ConvertValue(string value, Type targetType)
        {
            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(int) && int.TryParse(value, out int i))
                return i;

            if (targetType == typeof(decimal) && decimal.TryParse(value, out decimal d))
                return d;

            if (targetType == typeof(double) && double.TryParse(value, out double dbl))
                return dbl;

            if (targetType == typeof(bool) && bool.TryParse(value, out bool b))
                return b;

            if (targetType == typeof(DateTime) && DateTime.TryParse(value, out DateTime dt))
                return dt;

            if (targetType.IsEnum && Enum.TryParse(targetType, value, out var enumValue))
                return enumValue;

            return null;
        }

        private static void ShowInformationWindow(string message)
        {
            var info = new InformationWindow
            {
                Message = message,
                Owner = Application.Current.MainWindow
            };
            info.ShowDialog();
        }

        #endregion
    }
}
