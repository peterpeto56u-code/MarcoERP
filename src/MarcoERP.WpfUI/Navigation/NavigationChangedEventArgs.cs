using System;
using System.Windows.Controls;

namespace MarcoERP.WpfUI.Navigation
{
    public sealed class NavigationChangedEventArgs : EventArgs
    {
        public NavigationChangedEventArgs(string key, string title, UserControl view, object parameter)
        {
            Key = key;
            Title = title;
            View = view;
            Parameter = parameter;
        }

        public string Key { get; }

        public string Title { get; }

        public UserControl View { get; }

        public object Parameter { get; }
    }
}
