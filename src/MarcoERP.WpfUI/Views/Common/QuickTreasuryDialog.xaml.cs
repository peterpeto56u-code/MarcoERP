using System.Windows;
using MarcoERP.WpfUI.ViewModels.Common;

namespace MarcoERP.WpfUI.Views.Common
{
    public partial class QuickTreasuryDialog : Window
    {
        private readonly QuickTreasuryDialogViewModel _viewModel;

        public QuickTreasuryDialog(QuickTreasuryDialogViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.RequestClose += OnRequestClose;
        }

        public QuickTreasuryDialogViewModel ViewModel => _viewModel;

        private void OnRequestClose(bool confirmed)
        {
            DialogResult = confirmed;
            Close();
        }
    }
}
