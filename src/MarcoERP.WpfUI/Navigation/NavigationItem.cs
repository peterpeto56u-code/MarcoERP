using System.Windows.Input;
using System.Windows.Media;
using MarcoERP.WpfUI.ViewModels;
using MaterialDesignThemes.Wpf;

namespace MarcoERP.WpfUI.Navigation
{
    public sealed class NavigationItem : BaseViewModel
    {
        public NavigationItem(NavigationItemType itemType, string title = null)
        {
            ItemType = itemType;
            Title = title;
        }

        public NavigationItemType ItemType { get; }

        public string Title { get; }

        public PackIconKind IconKind { get; init; }

        public Brush IconBrush { get; init; }

        public string ViewKey { get; init; }

        public string PermissionKey { get; init; }

        public ICommand Command { get; set; }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }
    }
}
