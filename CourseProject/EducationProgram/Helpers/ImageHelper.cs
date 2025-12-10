using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace EducationProgram.Helpers
{
    public static class ImageHelper
    {
        public static BitmapImage Convert(System.Drawing.Image img)
        {
            using (var memory = new MemoryStream())
            {
                // Сохраняем картинку в поток памяти в формате PNG
                img.Save(memory, ImageFormat.Png); //
                memory.Position = 0;

                // Создаем WPF картинку из потока
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                // Замораживаем объект для использования в разных потоках UI (оптимизация)
                bitmapImage.Freeze(); //

                return bitmapImage;
            }
        }
    }
}
