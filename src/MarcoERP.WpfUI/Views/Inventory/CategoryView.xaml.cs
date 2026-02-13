using System.Windows.Controls;
using System.Windows.Input;
using MarcoERP.WpfUI.ViewModels.Inventory;

namespace MarcoERP.WpfUI.Views.Inventory
{
    public partial class CategoryView : UserControl
    {
        private CategoryViewModel ViewModel => DataContext as CategoryViewModel;

        public CategoryView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (ViewModel != null) await ViewModel.LoadCategoriesAsync();
            };
        }

    }
}
