using System.Windows;
using System.Windows.Controls;

namespace MarcoERP.WpfUI.Common
{
    public static class PasswordBoxAssistant
    {
        public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxAssistant),
            new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached(
            "IsUpdating",
            typeof(bool),
            typeof(PasswordBoxAssistant),
            new PropertyMetadata(false));

        public static void SetBoundPassword(DependencyObject element, string value) => element.SetValue(BoundPasswordProperty, value);
        public static string GetBoundPassword(DependencyObject element) => (string)element.GetValue(BoundPasswordProperty);

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox box)
            {
                box.PasswordChanged -= HandlePasswordChanged;
                if (!GetIsUpdating(box))
                    box.Password = e.NewValue?.ToString() ?? string.Empty;
                box.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox box)
            {
                SetIsUpdating(box, true);
                SetBoundPassword(box, box.Password);
                SetIsUpdating(box, false);
            }
        }

        private static void SetIsUpdating(DependencyObject element, bool value) => element.SetValue(IsUpdatingProperty, value);
        private static bool GetIsUpdating(DependencyObject element) => (bool)element.GetValue(IsUpdatingProperty);
    }
}
