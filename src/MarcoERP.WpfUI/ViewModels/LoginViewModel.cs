using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MarcoERP.Application.DTOs.Security;
using MarcoERP.Application.Interfaces.Security;
using MarcoERP.Application.Interfaces;
using MarcoERP.WpfUI.Services;
using MarcoERP.WpfUI.Views.Common;

namespace MarcoERP.WpfUI.ViewModels
{
    public sealed class LoginViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWindowService _windowService;

        public LoginViewModel(
            IAuthenticationService authenticationService,
            ICurrentUserService currentUserService,
            IWindowService windowService)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));

            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            CloseCommand = new RelayCommand(CloseApp);

            LoadSavedCredentials();
        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                    RelayCommand.RaiseCanExecuteChanged();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                    RelayCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        private bool _rememberMe;
        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand CloseCommand { get; }

        private bool CanLogin()
        {
            return !IsBusy
                && !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Password);
        }

        private async Task LoginAsync()
        {
            IsBusy = true;
            ClearError();

            try
            {
                var result = await _authenticationService.LoginAsync(new LoginDto
                {
                    Username = Username?.Trim(),
                    Password = Password
                });

                if (!result.IsSuccess)
                {
                    ErrorMessage = result.ErrorMessage ?? "بيانات الدخول غير صحيحة.";
                    return;
                }

                var loginResult = result.Data;
                _currentUserService.SetUser(
                    loginResult.UserId,
                    loginResult.Username,
                    loginResult.FullNameAr,
                    loginResult.RoleId,
                    loginResult.RoleNameAr,
                    loginResult.Permissions);

                if (loginResult.MustChangePassword)
                {
                    var changeResult = PromptForPasswordChange(loginResult.UserId);
                    if (!changeResult)
                    {
                        ErrorMessage = "يجب تغيير كلمة المرور للمتابعة.";
                        _currentUserService.ClearUser();
                        return;
                    }
                }

                // حفظ أو مسح بيانات "تذكرني"
                if (RememberMe)
                    CredentialStore.Save(Username?.Trim(), Password);
                else
                    CredentialStore.Clear();

                _windowService.ShowMainWindow();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"حدث خطأ غير متوقع: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
            }
            finally
            {
                IsBusy = false;
                RelayCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>تحميل بيانات الدخول المحفوظة من "تذكرني".</summary>
        private void LoadSavedCredentials()
        {
            var saved = CredentialStore.Load();
            if (saved != null)
            {
                Username = saved.Username;
                Password = saved.Password;
                RememberMe = true;
            }
        }

        private static void CloseApp()
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>يعرض نافذة تغيير كلمة المرور ويستدعي الخدمة. يعيد true عند النجاح.</summary>
        private bool PromptForPasswordChange(int userId)
        {
            const int maxAttempts = 3;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var dialog = new ChangePasswordDialog();
                var result = dialog.ShowDialog();

                if (result != true)
                    return false; // User cancelled

                var dto = new ChangePasswordDto
                {
                    CurrentPassword = Password,
                    NewPassword = dialog.NewPassword,
                    ConfirmNewPassword = dialog.ConfirmNewPassword
                };

                var changeTask = _authenticationService.ChangePasswordAsync(userId, dto);
                changeTask.Wait();
                var serviceResult = changeTask.Result;

                if (serviceResult.IsSuccess)
                {
                    MessageBox.Show("تم تغيير كلمة المرور بنجاح.", "نجاح",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }

                var remaining = maxAttempts - attempt - 1;
                var msg = serviceResult.ErrorMessage ?? "فشل تغيير كلمة المرور.";
                if (remaining > 0)
                    msg += $"\nمحاولات متبقية: {remaining}";

                MessageBox.Show(msg, "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return false;
        }
    }
}
