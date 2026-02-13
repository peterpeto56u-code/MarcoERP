using System;
using System.Threading.Tasks;
using System.Windows;
using MarcoERP.Domain.Exceptions;

namespace MarcoERP.WpfUI.Common
{
    /// <summary>
    /// مساعد معالجة تعارضات التزامن — Concurrency conflict handling helper for ViewModels.
    /// </summary>
    public static class ConcurrencyHelper
    {
        /// <summary>
        /// Executes an action and handles concurrency conflicts with a user-friendly dialog.
        /// </summary>
        /// <param name="action">The async action to execute.</param>
        /// <param name="refreshAction">Optional action to refresh data after conflict.</param>
        /// <returns>True if the action succeeded, false if a conflict occurred.</returns>
        public static async Task<bool> ExecuteWithConcurrencyHandling(
            Func<Task> action,
            Func<Task> refreshAction = null)
        {
            try
            {
                await action();
                return true;
            }
            catch (ConcurrencyConflictException ex)
            {
                await ShowConflictAndRefreshAsync(ex, refreshAction);
                return false;
            }
        }

        /// <summary>
        /// Shows a concurrency conflict dialog and optionally refreshes data.
        /// Use this from existing catch blocks in ViewModels.
        /// </summary>
        /// <param name="ex">The concurrency conflict exception.</param>
        /// <param name="refreshAction">Optional action to refresh data after conflict.</param>
        public static async Task ShowConflictAndRefreshAsync(
            ConcurrencyConflictException ex,
            Func<Task> refreshAction = null)
        {
            var result = MessageBox.Show(
                $"تم تعديل هذا السجل ({ex.EntityName}) بواسطة مستخدم آخر.\n\n" +
                "هل تريد تحديث البيانات وإعادة المحاولة؟",
                "تعارض في التزامن",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.Yes,
                MessageBoxOptions.RtlReading);

            if (result == MessageBoxResult.Yes && refreshAction != null)
            {
                await refreshAction();
            }
        }
    }
}
