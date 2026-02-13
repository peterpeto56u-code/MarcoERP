using MarcoERP.Application.Reporting.Interfaces;
using MarcoERP.Application.Reporting.Models;
using MarcoERP.WpfUI.Navigation;

namespace MarcoERP.WpfUI.Reporting
{
    /// <summary>
    /// Executes drill-down navigation from a report row to the source document.
    /// Uses the existing Chrome-tab navigation system.
    /// </summary>
    public sealed class DrillDownEngine
    {
        private readonly IDrillDownResolver _resolver;
        private readonly INavigationService _navigationService;

        public DrillDownEngine(IDrillDownResolver resolver, INavigationService navigationService)
        {
            _resolver = resolver;
            _navigationService = navigationService;
        }

        /// <summary>
        /// Navigates to the source document for the given report row.
        /// Opens in a new tab via the existing NavigationService.
        /// </summary>
        /// <returns>True if navigation was performed; false if drill-down is not supported.</returns>
        public bool Navigate(ReportRowBase row)
        {
            if (row == null) return false;

            // Use explicit NavigationTarget if set
            if (!string.IsNullOrEmpty(row.NavigationTarget))
            {
                _navigationService.NavigateTo(row.NavigationTarget, row.SourceId);
                return true;
            }

            // Resolve from SourceType
            if (row.SourceType.HasValue && row.SourceId.HasValue)
            {
                var viewKey = _resolver.ResolveNavigationKey(row.SourceType.Value);
                if (viewKey != null)
                {
                    _navigationService.NavigateTo(viewKey, row.SourceId.Value);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the Arabic action label for the drill-down button tooltip.
        /// </summary>
        public string GetActionLabel(ReportRowBase row)
        {
            if (row?.SourceType == null) return null;
            return _resolver.GetActionLabel(row.SourceType.Value);
        }
    }
}
