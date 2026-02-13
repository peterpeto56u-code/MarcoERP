using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarcoERP.WpfUI.ViewModels;
using MarcoERP.WpfUI.ViewModels.Common;

namespace MarcoERP.WpfUI.Views.Common
{
    /// <summary>
    /// Shared modal popup for adding/editing an invoice line item.
    /// Used by Sales Invoice, Purchase Invoice, Sales Return, Purchase Return.
    /// Handles Enter-to-next-field navigation for fast data entry.
    /// </summary>
    public partial class InvoiceAddLineWindow : Window
    {
        /// <summary>True when the user confirmed adding a line (vs cancelling).</summary>
        public bool LineAdded { get; private set; }

        /// <summary>True when the user wants to add another line after this one.</summary>
        public bool AddAnother { get; private set; }

        public InvoiceAddLineWindow()
        {
            InitializeComponent();
        }

        private void Field_EnterToNext(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;

            if (sender is TextBox textBox)
            {
                var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                binding?.UpdateSource();
            }
            else if (sender is ComboBox comboBox)
            {
                var binding = comboBox.GetBindingExpression(ComboBox.SelectedValueProperty);
                binding?.UpdateSource();
            }

            (sender as UIElement)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void AddAndNext_Click(object sender, RoutedEventArgs e)
        {
            LineAdded = true;
            AddAnother = true;
            DialogResult = true;
        }

        private void AddAndClose_Click(object sender, RoutedEventArgs e)
        {
            LineAdded = true;
            AddAnother = false;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            LineAdded = false;
            DialogResult = false;
        }

        private async void QuickAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new QuickAddProductDialog { Owner = this };
            await dialog.InitializeAsync();

            if (dialog.ShowDialog() == true && dialog.CreatedProductId.HasValue)
            {
                // Refresh products list from host ViewModel
                if (DataContext is IInvoiceLineFormHost host)
                {
                    await host.RefreshProductsAsync();
                    // Select the newly created product in the ComboBox
                    cmbProduct.SelectedValue = dialog.CreatedProductId.Value;
                }
            }
        }
    }
}
