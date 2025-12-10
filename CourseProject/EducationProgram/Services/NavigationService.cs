using CommunityToolkit.Mvvm.ComponentModel;
using EducationProgram.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace EducationProgram.Services
{
    public partial class NavigationService : ObservableObject
    {
        private readonly IServiceProvider _provider;

        [ObservableProperty]
        private ObservableObject currentViewModel;

        private readonly Dictionary<string, Func<ObservableObject>> _viewModelFactories = new();

        public NavigationService(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));

            _viewModelFactories["Education"] = () => _provider.GetRequiredService<EducationViewModel>();
            _viewModelFactories["Authorization"] = () => _provider.GetRequiredService<AuthorizationViewModel>();
            _viewModelFactories["Registration"] = () => _provider.GetRequiredService<RegistrationViewModel>();
            _viewModelFactories["UserProfile"] = () => _provider.GetRequiredService<UserProfileViewModel>();
        }

        public void NavigateToInstance(ObservableObject viewModel)
        {
            CurrentViewModel = viewModel;
        }

        public void Navigate(string key)
        {
            if (_viewModelFactories.TryGetValue(key, out var factory))
            {
                CurrentViewModel = factory();
            }
            else
            {
                throw new ArgumentException($"ViewModel с ключом '{key}' не найден.");
            }
        }
    }
}
