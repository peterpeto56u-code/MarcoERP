using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MarcoERP.Application.Interfaces;
using MarcoERP.WpfUI.Views.Sales;
using MarcoERP.WpfUI.Views.Shell;

namespace MarcoERP.WpfUI.Services
{
    public sealed class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICurrentUserService _currentUserService;

        public WindowService(IServiceProvider serviceProvider, ICurrentUserService currentUserService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public void OpenPosWindow()
        {
            var scope = _serviceProvider.CreateScope();
            var window = scope.ServiceProvider.GetRequiredService<PosWindow>();
            window.Closed += (_, _) => scope.Dispose();
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.Show();
        }

        public void ShowMainWindow()
        {
            var scope = _serviceProvider.CreateScope();
            var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Closed += (_, _) => scope.Dispose();
            System.Windows.Application.Current.MainWindow = mainWindow;
            mainWindow.Show();

            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window is LoginWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        public void LogoutToLogin()
        {
            _currentUserService.ClearUser();
            var scope = _serviceProvider.CreateScope();
            var loginWindow = scope.ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Closed += (_, _) => scope.Dispose();
            var oldMainWindow = System.Windows.Application.Current.MainWindow;
            System.Windows.Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
            oldMainWindow?.Close();
        }
    }
}
