using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MarcoERP.WpfUI.ViewModels;

namespace MarcoERP.WpfUI.Views.Shell
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<LoginViewModel>();
        }
    }
}
