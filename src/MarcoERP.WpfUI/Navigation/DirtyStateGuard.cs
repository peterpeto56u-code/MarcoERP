using System;
using System.Threading.Tasks;
using System.Windows;
using MarcoERP.WpfUI.Resources;

namespace MarcoERP.WpfUI.Navigation
{
    /// <summary>
    /// Centralized helper for unsaved-changes prompts and save/continue decisions.
    /// </summary>
    public static class DirtyStateGuard
    {
        /// <summary>
        /// Prompts the user to save, discard, or cancel when unsaved changes exist.
        /// Returns true when navigation can proceed.
        /// </summary>
        public static async Task<bool> ConfirmContinueAsync(IDirtyStateAware dirtyState)
        {
            if (dirtyState == null || !dirtyState.IsDirty)
                return true;

            var result = MessageBox.Show(
                UiStrings.UnsavedChangesPrompt,
                UiStrings.UnsavedChangesTitle,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel,
                MessageBoxOptions.RtlReading);

            if (result == MessageBoxResult.Yes)
            {
                var saved = await dirtyState.SaveChangesAsync();
                if (!saved)
                {
                    MessageBox.Show(
                        UiStrings.UnsavedChangesSaveFailed,
                        UiStrings.UnsavedChangesTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK,
                        MessageBoxOptions.RtlReading);
                    return false;
                }

                return true;
            }

            return result == MessageBoxResult.No;
        }
    }
}
