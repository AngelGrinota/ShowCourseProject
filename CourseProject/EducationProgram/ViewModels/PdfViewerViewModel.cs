using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducationProgram.Helpers;
using PdfiumViewer;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EducationProgram.ViewModels
{
    public partial class PdfViewerViewModel : ObservableObject, IDisposable
    {
        private PdfDocument? _pdfDocument;
        private readonly Dictionary<string, Bitmap> _pageCache = new();
        private readonly DispatcherTimer _zoomRenderTimer;
        private readonly object _cacheLock = new();

        [ObservableProperty] private BitmapImage? currentPageImage;
        [ObservableProperty] private int currentPage;
        [ObservableProperty] private int totalPages;

        [ObservableProperty] private bool canGoToPreviousPage;
        [ObservableProperty] private bool canGoToNextPage;

        [ObservableProperty] private double currentZoom = 1.0;
        [ObservableProperty] private string zoomStatusText = "100%";
        [ObservableProperty] private string pageInfoText = "0 / 0";

        // DPI для PDF и масштаб экрана
        private const double PdfDpi = 300.0;
        private const double ScreenDpi = 96.0;
        private double DpiScale => PdfDpi / ScreenDpi; // ≈3.125

        public double TotalZoom => CurrentZoom * DpiScale;

        public PdfViewerViewModel()
        {
            _zoomRenderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
            _zoomRenderTimer.Tick += (s, e) =>
            {
                _zoomRenderTimer.Stop();
                RenderPageAsync(CurrentPage);
            };
        }

        public void LoadPdf(string relativePath)
        {
            Dispose();

            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            if (!File.Exists(fullPath))
            {
                PageInfoText = $"Файл не найден: {fullPath}";
                return;
            }

            try
            {
                _pdfDocument = PdfDocument.Load(fullPath);
                TotalPages = _pdfDocument.PageCount;

                CurrentPage = 0;
                CurrentZoom = 1.0;
                ZoomStatusText = "100%";

                OnPropertyChanged(nameof(TotalZoom));

                RenderPageAsync(CurrentPage);
            }
            catch (Exception ex)
            {
                Dispose();
                PageInfoText = $"Ошибка PDF: {ex.Message}";
            }
        }

        [RelayCommand] private void NextPage() => NavigateTo(CurrentPage + 1);
        [RelayCommand] private void PreviousPage() => NavigateTo(CurrentPage - 1);

        private void NavigateTo(int page)
        {
            if (_pdfDocument == null || page < 0 || page >= TotalPages) return;

            _zoomRenderTimer.Stop();
            CurrentPage = page;

            RenderPageAsync(page);
        }

        private void UpdateNavigation()
        {
            CanGoToPreviousPage = CurrentPage > 0;
            CanGoToNextPage = CurrentPage < TotalPages - 1;
            PageInfoText = $"{CurrentPage + 1} / {TotalPages}";
        }

        [RelayCommand] private void ZoomIn() => ChangeZoom(0.1);
        [RelayCommand] private void ZoomOut() => ChangeZoom(-0.1);

        private void ChangeZoom(double delta)
        {
            double newZoom = Math.Clamp(CurrentZoom + delta, 1.0, 2.2);

            if (Math.Abs(newZoom - CurrentZoom) < 0.001) return;

            CurrentZoom = newZoom;
            ZoomStatusText = $"{Math.Round(CurrentZoom * 100)}%";

            OnPropertyChanged(nameof(TotalZoom));

            // Debounce для рендера
            _zoomRenderTimer.Stop();
            _zoomRenderTimer.Start();
        }

        private double GetBaseZoom(double zoom) => Math.Round(zoom * 2) / 2;

        private async void RenderPageAsync(int page)
        {
            if (_pdfDocument == null) return;

            double baseZoom = GetBaseZoom(CurrentZoom);
            string cacheKey = $"{page}-{baseZoom:F1}";

            Bitmap? bmpCached = null;
            lock (_cacheLock)
            {
                _pageCache.TryGetValue(cacheKey, out bmpCached);
            }

            if (bmpCached != null)
            {
                CurrentPageImage = ImageHelper.Convert(bmpCached);
                UpdateNavigation();
                return;
            }

            Bitmap? bmp = null;

            await Task.Run(() =>
            {
                try
                {
                    var size = _pdfDocument.PageSizes[page];

                    // Высокое качество: динамический DPI
                    float dpi = (float)(150 * baseZoom);
                    if (dpi < 60) dpi = 60;

                    int width = (int)(size.Width * dpi / 72.0);
                    int height = (int)(size.Height * dpi / 72.0);

                    width = Math.Max(width, 80);
                    height = Math.Max(height, 80);

                    width = Math.Min(width, 5000);
                    height = Math.Min(height, 5000);

                    bmp = (Bitmap)_pdfDocument.Render(
                        page,
                        width,
                        height,
                        dpi,
                        dpi,
                        PdfRenderFlags.LcdText |
                        PdfRenderFlags.ForPrinting |
                        PdfRenderFlags.Annotations
                    );
                }
                catch { }
            });

            if (bmp != null)
            {
                lock (_cacheLock)
                {
                    if (!_pageCache.ContainsKey(cacheKey))
                        _pageCache[cacheKey] = (Bitmap)bmp.Clone();
                    else
                        bmp.Dispose();
                }

                CurrentPageImage = ImageHelper.Convert(bmp);
                bmp.Dispose();
            }

            UpdateNavigation();
        }

        public void Dispose()
        {
            _pdfDocument?.Dispose();
            _pdfDocument = null;

            lock (_cacheLock)
            {
                foreach (var img in _pageCache.Values)
                    img.Dispose();

                _pageCache.Clear();
            }

            CurrentPageImage = null;
        }

    }
}
