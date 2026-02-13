using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarcoERP.WpfUI.Views.Common;

namespace MarcoERP.WpfUI.Common
{
    /// <summary>
    /// Attached behavior that opens a search lookup window when F1 is pressed on a ComboBox.
    /// Usage: &lt;ComboBox common:F1SearchBehavior.IsEnabled="True" common:F1SearchBehavior.Title="بحث العملاء" /&gt;
    /// </summary>
    public static class F1SearchBehavior
    {
        // ── IsEnabled ────────────────────────────────────────────
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(F1SearchBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        // ── Title ────────────────────────────────────────────────
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(F1SearchBehavior),
                new PropertyMetadata("بحث"));

        public static string GetTitle(DependencyObject obj) => (string)obj.GetValue(TitleProperty);
        public static void SetTitle(DependencyObject obj, string value) => obj.SetValue(TitleProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox combo)
            {
                combo.PreviewKeyDown -= OnComboPreviewKeyDown;
                if ((bool)e.NewValue)
                    combo.PreviewKeyDown += OnComboPreviewKeyDown;
            }
        }

        private static void OnComboPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F1) return;
            if (sender is not ComboBox combo) return;
            if (combo.ItemsSource == null) return;

            var title = GetTitle(combo);
            var displayPath = combo.DisplayMemberPath ?? "NameAr";
            var valuePath = combo.SelectedValuePath ?? "Id";

            var window = new SearchLookupWindow(combo.ItemsSource, displayPath, valuePath, title)
            {
                Owner = Window.GetWindow(combo)
            };

            if (window.ShowDialog() == true && window.SelectedValue != null)
            {
                combo.SelectedValue = window.SelectedValue;
            }

            e.Handled = true;
        }
    }
}
