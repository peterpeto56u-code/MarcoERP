using System.Windows;

namespace MarcoERP.WpfUI.Views.Common
{
    public partial class ChangePasswordDialog : Window
    {
        public ChangePasswordDialog()
        {
            InitializeComponent();
        }

        public string NewPassword { get; private set; }
        public string ConfirmNewPassword { get; private set; }

        private void OnChangeClick(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            var newPwd = NewPasswordBox.Password;
            var confirmPwd = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(newPwd))
            {
                ShowError("كلمة المرور الجديدة مطلوبة.");
                return;
            }

            if (newPwd.Length < 8)
            {
                ShowError("كلمة المرور يجب أن تكون 8 أحرف على الأقل.");
                return;
            }

            if (newPwd != confirmPwd)
            {
                ShowError("كلمة المرور الجديدة وتأكيدها غير متطابقتين.");
                return;
            }

            NewPassword = newPwd;
            ConfirmNewPassword = confirmPwd;
            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
