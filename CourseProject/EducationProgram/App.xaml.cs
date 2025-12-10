using EducationProgram.Services;
using EducationProgram.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace EducationProgram
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            services.AddDbContext<DataBaseContext.EducationDbContext>(options =>
            {
                var connectionString = "server=mysql.softsols.ru;port=63307;user id=admin;password=pOM8K8HA99685dXI;database=mydb";

                options.UseMySql(connectionString, ServerVersion.Parse("8.0.44-mysql"));
            }, ServiceLifetime.Transient);

            services.AddSingleton<NavigationService>();           
            services.AddSingleton<UserService>();
            services.AddSingleton<MainViewModel>();

            services.AddTransient<DataAccessService>();
            services.AddTransient<EducationViewModel>();
            services.AddTransient<AuthorizationViewModel>();
            services.AddTransient<RegistrationViewModel>();
            services.AddTransient<UserProfileViewModel>();         

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
            };

            mainWindow.Show();
        }
    }

}
