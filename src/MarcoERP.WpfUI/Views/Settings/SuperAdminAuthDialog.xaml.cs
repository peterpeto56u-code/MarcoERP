using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarcoERP.WpfUI.ViewModels.Settings;

namespace MarcoERP.WpfUI.Views.Settings
{
    /// <summary>
    /// Phase 7C: Super Admin Authentication Dialog.
    /// 7F: Disables clipboard copy/cut/paste on the password field.
    /// </summary>
    public partial class SuperAdminAuthDialog : Window
    {
        public SuperAdminAuthDialog()
        {
            InitializeComponent();
        }

        /// <summary>Sync PasswordBox → ViewModel (PasswordBox doesn't support binding).</summary>
        private void PasswordField_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SuperAdminAuthViewModel vm)
                vm.Password = PasswordField.Password;
        }

        /// <summary>7F: Block clipboard operations (Copy, Cut, Paste) on the password field.</summary>
        private void PasswordField_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
