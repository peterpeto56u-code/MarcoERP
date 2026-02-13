using System.Threading.Tasks;

namespace MarcoERP.WpfUI.Navigation
{
    /// <summary>
    /// Implemented by ViewModels that track unsaved form changes.
    /// NavigationService checks this before navigating away.
    /// </summary>
    public interface IDirtyStateAware
    {
        /// <summary>Whether the form has unsaved modifications.</summary>
        bool IsDirty { get; }

        /// <summary>Resets the dirty flag (e.g. after save).</summary>
        void ResetDirtyState();

        /// <summary>
        /// Attempts to save pending changes.
        /// Returns true if changes were saved successfully.
        /// </summary>
        Task<bool> SaveChangesAsync();
    }
}
