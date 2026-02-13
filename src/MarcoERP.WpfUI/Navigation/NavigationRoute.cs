using System;
using System.Windows.Controls;

namespace MarcoERP.WpfUI.Navigation
{
    public sealed class NavigationRoute
    {
        public NavigationRoute(string title, Func<IServiceProvider, UserControl> factory)
        {
            Title = title;
            Factory = factory;
        }

        public string Title { get; }

        public Func<IServiceProvider, UserControl> Factory { get; }
    }
}
