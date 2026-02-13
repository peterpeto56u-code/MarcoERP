using System;
using System.Windows.Controls;

namespace MarcoERP.WpfUI.Navigation
{
    public interface INavigationService
    {
        UserControl CurrentView { get; }

        event EventHandler<NavigationChangedEventArgs> NavigationChanged;

        void NavigateTo(string key);

        /// <summary>
        /// Navigates to a view by key, passing an optional parameter (e.g. entity ID).
        /// The target ViewModel should implement <see cref="INavigationAware"/> to receive it.
        /// </summary>
        void NavigateTo(string key, object parameter);

        /// <summary>
        /// Removes a cached view instance for the given key and parameter.
        /// </summary>
        void CloseView(string key, object parameter);
    }
}
