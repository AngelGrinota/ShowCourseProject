using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace EducationProgram.Helpers
{
    public partial class PagingHelper<T> : ObservableObject
    {
        public const int PageSize = 10;

        [ObservableProperty] private int currentPage = 1;
        [ObservableProperty] private int totalPages = 1;
        [ObservableProperty] private ObservableCollection<T> items = new();
        [ObservableProperty] private bool canGoPrev;
        [ObservableProperty] private bool canGoNext;

        public void Update(IList<T> source)
        {
            if (source == null || source.Count == 0)
            {
                TotalPages = 1;
                CurrentPage = 1;
                Items = new ObservableCollection<T>();
            }
            else
            {
                TotalPages = (int)Math.Ceiling(source.Count / (double)PageSize);
                CurrentPage = Math.Clamp(CurrentPage, 1, TotalPages);

                Items = new ObservableCollection<T>(
                    source
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                );
            }

            CanGoPrev = CurrentPage > 1;
            CanGoNext = CurrentPage < TotalPages;
        }

        public void Next(IList<T> source)
        {
            if (CanGoNext)
            {
                CurrentPage++;
                Update(source);
            }
        }

        public void Prev(IList<T> source)
        {
            if (CanGoPrev)
            {
                CurrentPage--;
                Update(source);
            }
        }
    }
}
