using CommunityToolkit.Mvvm.ComponentModel;
using EducationProgram.Services;

namespace EducationProgram.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;

        public ObservableObject CurrentViewModel => _navigationService.CurrentViewModel;

        [ObservableProperty]
        private string title;
        public MainViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            _navigationService.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(NavigationService.CurrentViewModel))
                {
                    OnPropertyChanged(nameof(CurrentViewModel));
                    UpdateTitle();
                }
            };

            _navigationService.Navigate("Education");
            UpdateTitle();
        }

        private void UpdateTitle()
        {

            if (_navigationService.CurrentViewModel is UserManagmentViewModel userEditVM)
            {
                Title = userEditVM.WindowTitle;
                return; 
            }

            switch (_navigationService.CurrentViewModel)
            {
                case AuthorizationViewModel:
                    Title = "Авторизация";
                    break;
                case RegistrationViewModel:
                    Title = "Регистрация";
                    break;
                case UserProfileViewModel:
                    Title = "Профиль пользователя";
                    break;
                default:
                    Title = "Синтаксис и пунктуация";
                    break;
            }
        }
    }
}
