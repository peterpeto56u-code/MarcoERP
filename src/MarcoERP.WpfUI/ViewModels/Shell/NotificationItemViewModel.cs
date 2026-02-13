using MaterialDesignThemes.Wpf;

namespace MarcoERP.WpfUI.ViewModels.Shell
{
    public sealed class NotificationItemViewModel : BaseViewModel
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Timestamp { get; set; }
        public PackIconKind Icon { get; set; }
    }
}
