using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MarcoERP.WpfUI.Views.Shell
{
    /// <summary>
    /// Dialog that lets the user pick up to 6 shortcut screens for the dashboard.
    /// </summary>
    public partial class ShortcutConfigDialog : Window
    {
        private const int MaxShortcuts = 6;

        public List<ScreenOption> ScreenOptions { get; }

        /// <summary>
        /// After dialog closes with OK, contains the selected ViewKeys in order.
        /// </summary>
        public List<string> SelectedKeys { get; private set; } = new();

        public ShortcutConfigDialog(
            IReadOnlyList<(string Key, string Title, string IconKind)> allScreens,
            IReadOnlyList<string> currentKeys)
        {
            InitializeComponent();

            var currentSet = new HashSet<string>(currentKeys ?? Array.Empty<string>(),
                                                  StringComparer.OrdinalIgnoreCase);

            ScreenOptions = allScreens
                .Select(s => new ScreenOption
                {
                    ViewKey = s.Key,
                    Title = s.Title,
                    IconKind = s.IconKind,
                    IsSelected = currentSet.Contains(s.Key)
                })
                .ToList();

            ScreensListBox.ItemsSource = ScreenOptions;
            UpdateSelectionCount();
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateSelectionCount();
        }

        private void UpdateSelectionCount()
        {
            int count = ScreenOptions.Count(o => o.IsSelected);
            SelectionCountText.Text = $"المحدد: {count} / {MaxShortcuts}";

            // Disable unchecked checkboxes if already at max
            if (count >= MaxShortcuts)
            {
                foreach (var opt in ScreenOptions.Where(o => !o.IsSelected))
                    opt.IsEnabled = false;
            }
            else
            {
                foreach (var opt in ScreenOptions)
                    opt.IsEnabled = true;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedKeys = ScreenOptions
                .Where(o => o.IsSelected)
                .Select(o => o.ViewKey)
                .ToList();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    /// <summary>Screen option displayed in the configuration dialog.</summary>
    public sealed class ScreenOption : INotifyPropertyChanged
    {
        public string ViewKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string IconKind { get; set; } = string.Empty;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
