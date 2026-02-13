using System.Threading.Tasks;

namespace MarcoERP.WpfUI.Navigation
{
    /// <summary>
    /// Implemented by ViewModels that need to receive navigation parameters
    /// (e.g. an entity ID when navigating from a list view to a detail view).
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>
        /// Called after the ViewModel is created and before the view is displayed.
        /// </summary>
        /// <param name="parameter">
        /// The parameter passed via <see cref="INavigationService.NavigateTo(string, object)"/>.
        /// Typically an entity ID (int) or null for new-record mode.
        /// </param>
        Task OnNavigatedToAsync(object parameter);
    }
}
