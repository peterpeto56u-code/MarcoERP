using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace MarcoERP.WpfUI.Navigation
{
    public sealed class NavigationService : INavigationService
    {
        private readonly IViewRegistry _viewRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, CachedView> _viewCache = new(StringComparer.OrdinalIgnoreCase);

        public NavigationService(IViewRegistry viewRegistry, IServiceProvider serviceProvider)
        {
            _viewRegistry = viewRegistry ?? throw new ArgumentNullException(nameof(viewRegistry));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public UserControl CurrentView { get; private set; }

        public event EventHandler<NavigationChangedEventArgs> NavigationChanged;

        public void NavigateTo(string key)
        {
            NavigateTo(key, null);
        }

        public async void NavigateTo(string key, object parameter)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            try
            {
                // ── Dirty-state guard ──
                if (CurrentView?.DataContext is IDirtyStateAware dirty)
                {
                    var canContinue = await DirtyStateGuard.ConfirmContinueAsync(dirty);
                    if (!canContinue)
                        return;
                }

                var cacheKey = BuildCacheKey(key, parameter);
                if (!_viewCache.TryGetValue(cacheKey, out var cached))
                {
                    // Create a new DI scope so each view gets its own DbContext lifetime
                    var scope = _serviceProvider.CreateScope();
                    var view = _viewRegistry.CreateView(key, scope.ServiceProvider);

                    // If the ViewModel implements INavigationAware, await initialization before showing
                    if (view.DataContext is INavigationAware navigationAware)
                    {
                        await navigationAware.OnNavigatedToAsync(parameter);
                    }

                    cached = new CachedView(view, scope);
                    _viewCache[cacheKey] = cached;
                }
                var title = _viewRegistry.TryGet(key, out var route) ? route.Title : key;

                CurrentView = cached.View;
                NavigationChanged?.Invoke(this, new NavigationChangedEventArgs(key, title, cached.View, parameter));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NavigationService] Navigation to '{key}' failed: {ex.Message}");

                var errorView = CreateNavigationErrorView(key, ex);
                var title = _viewRegistry.TryGet(key, out var route) ? route.Title : key;

                CurrentView = errorView;
                NavigationChanged?.Invoke(this, new NavigationChangedEventArgs(key, title, errorView, parameter));
            }
        }

        public void CloseView(string key, object parameter)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            var cacheKey = BuildCacheKey(key, parameter);
            if (_viewCache.Remove(cacheKey, out var cached))
            {
                cached.Scope.Dispose(); // Disposes the DI scope and its DbContext
            }
        }

        private static string BuildCacheKey(string key, object parameter)
        {
            if (parameter == null) return key;
            return key + ":" + parameter;
        }

        private static UserControl CreateNavigationErrorView(string key, Exception ex)
        {
            return new UserControl
            {
                Content = new Border
                {
                    Margin = new Thickness(16),
                    Padding = new Thickness(16),
                    BorderThickness = new Thickness(1),
                    BorderBrush = System.Windows.Media.Brushes.IndianRed,
                    Child = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "تعذر فتح الشاشة المطلوبة",
                                FontSize = 18,
                                FontWeight = FontWeights.SemiBold,
                                Margin = new Thickness(0, 0, 0, 8)
                            },
                            new TextBlock
                            {
                                Text = $"المفتاح: {key}",
                                FontSize = 13,
                                Margin = new Thickness(0, 0, 0, 6)
                            },
                            new TextBlock
                            {
                                Text = ex.Message,
                                FontSize = 12,
                                TextWrapping = TextWrapping.Wrap
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Holds a cached view along with its DI scope for proper lifetime management.
        /// </summary>
        private sealed class CachedView
        {
            public UserControl View { get; }
            public IServiceScope Scope { get; }

            public CachedView(UserControl view, IServiceScope scope)
            {
                View = view;
                Scope = scope;
            }
        }
    }
}
