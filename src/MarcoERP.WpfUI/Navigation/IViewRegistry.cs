using System;
using System.Windows.Controls;

namespace MarcoERP.WpfUI.Navigation
{
    public interface IViewRegistry
    {
        void Register<TView, TViewModel>(string key, string title)
            where TView : UserControl
            where TViewModel : class;

        bool TryGet(string key, out NavigationRoute route);

        UserControl CreateView(string key, IServiceProvider serviceProvider);
    }
}
