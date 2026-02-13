using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace MarcoERP.WpfUI.Navigation
{
    public sealed class ViewRegistry : IViewRegistry
    {
        private readonly Dictionary<string, NavigationRoute> _routes = new(StringComparer.OrdinalIgnoreCase);

        public void Register<TView, TViewModel>(string key, string title)
            where TView : UserControl
            where TViewModel : class
        {
            _routes[key] = new NavigationRoute(title, serviceProvider =>
            {
                var view = serviceProvider.GetRequiredService<TView>();
                var viewModel = serviceProvider.GetRequiredService<TViewModel>();
                view.DataContext = viewModel;
                return view;
            });
        }

        public bool TryGet(string key, out NavigationRoute route)
        {
            return _routes.TryGetValue(key, out route);
        }

        public UserControl CreateView(string key, IServiceProvider serviceProvider)
        {
            if (TryGet(key, out var route))
            {
                return route.Factory(serviceProvider);
            }

            return new UserControl
            {
                Content = new TextBlock
                {
                    Text = $"صفحة غير مسجلة: {key}",
                    FontSize = 18,
                    Margin = new Thickness(16),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }
    }
}
