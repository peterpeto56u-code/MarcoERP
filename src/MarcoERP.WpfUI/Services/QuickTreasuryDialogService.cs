using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MarcoERP.WpfUI.Views.Common;

namespace MarcoERP.WpfUI.Services
{
    public sealed class QuickTreasuryDialogService : IQuickTreasuryDialogService
    {
        private readonly IServiceProvider _serviceProvider;

        public QuickTreasuryDialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Task<QuickTreasuryDialogResult> ShowAsync(QuickTreasuryDialogRequest request, CancellationToken ct = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var dialog = scope.ServiceProvider.GetRequiredService<QuickTreasuryDialog>();
            if (System.Windows.Application.Current?.MainWindow != null)
                dialog.Owner = System.Windows.Application.Current.MainWindow;

            dialog.ViewModel.Initialize(request);

            var ok = dialog.ShowDialog();
            if (ok == true)
                return Task.FromResult(dialog.ViewModel.Result);

            return Task.FromResult<QuickTreasuryDialogResult>(null);
        }
    }
}
