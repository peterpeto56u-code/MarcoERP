using System.Windows.Input;
using MarcoERP.Application.Interfaces.Reports;
using MarcoERP.WpfUI.Navigation;

namespace MarcoERP.WpfUI.ViewModels.Reports
{
    /// <summary>
    /// ViewModel for the Report Hub — the landing page that lets users
    /// choose which report to view. Invokes NavigateToReport callback.
    /// </summary>
    public sealed class ReportHubViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly INavigationService _navigationService;
        private readonly IViewRegistry _viewRegistry;

        public ReportHubViewModel(IReportService reportService, INavigationService navigationService, IViewRegistry viewRegistry)
        {
            _reportService = reportService;
            _navigationService = navigationService;
            _viewRegistry = viewRegistry;

            OpenReportCommand = new RelayCommand(param =>
            {
                if (param is string reportKey)
                {
                    if (!_viewRegistry.TryGet(reportKey, out _))
                    {
                        ErrorMessage = "التقرير غير مسجل في النظام.";
                        return;
                    }
                    _navigationService.NavigateTo(reportKey);
                }
            });
        }

        public IReportService ReportService => _reportService;

        public ICommand OpenReportCommand { get; }
    }
}
