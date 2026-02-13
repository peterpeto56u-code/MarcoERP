using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Exceptions;

namespace MarcoERP.WpfUI.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels implementing INotifyPropertyChanged.
    /// Provides SetProperty helper for clean property change notification.
    /// Includes DbGuard semaphore to prevent concurrent DbContext access.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Serializes all async DB operations within a single ViewModel scope,
        /// preventing concurrent access to the same scoped DbContext instance.
        /// </summary>
        protected readonly SemaphoreSlim DbGuard = new(1, 1);

        /// <summary>
        /// Enqueues an async action to run serially against the DB guard.
        /// Safe to call fire-and-forget from property setters.
        /// </summary>
        protected void EnqueueDbWork(Func<Task> work)
        {
            _ = Task.Run(async () =>
            {
                await DbGuard.WaitAsync().ConfigureAwait(false);
                try
                {
                    await work().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[EnqueueDbWork] Error in {GetType().Name}: {ex.Message}");
                }
                finally
                {
                    DbGuard.Release();
                }
            });
        }

        /// <summary>Raises PropertyChanged for the given property name.</summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets a property value and raises PropertyChanged if the value changed.
        /// Returns true if the value was changed.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private bool _isBusy;
        /// <summary>Indicates whether the ViewModel is performing an async operation.</summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _statusMessage;
        /// <summary>Status message shown in the UI.</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private string _errorMessage;
        /// <summary>Error message shown in the UI.</summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasError));
            }
        }

        /// <summary>True if there is an error message.</summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>Clears the error message.</summary>
        protected void ClearError() => ErrorMessage = null;

        /// <summary>
        /// Converts a raw exception into a user-friendly Arabic error message.
        /// Never exposes stack traces or raw technical details to the user.
        /// </summary>
        protected static string FriendlyErrorMessage(string operationName, Exception ex)
        {
            if (ex is ConcurrencyConflictException)
                return "حدث تعارض في البيانات. يرجى إعادة تحميل السجل والمحاولة مجدداً.";

            if (ex is InvalidOperationException ioe && !string.IsNullOrEmpty(ioe.Message))
                return $"خطأ في {operationName}: {ioe.Message}";

            if (ex?.InnerException != null)
            {
                var inner = ex.InnerException.Message;
                if (inner.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
                    inner.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
                    return $"خطأ في {operationName}: يوجد سجل مكرر. تأكد من عدم تكرار البيانات.";

                if (inner.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
                    inner.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
                    return $"خطأ في {operationName}: لا يمكن الحذف لوجود سجلات مرتبطة.";
            }

            return $"حدث خطأ أثناء {operationName}. يرجى المحاولة مرة أخرى أو التواصل مع الدعم الفني.";
        }

        // ── Dirty State Tracking ──

        private bool _isDirty;
        /// <summary>True if the form has unsaved changes.</summary>
        public bool IsDirty
        {
            get => _isDirty;
            protected set => SetProperty(ref _isDirty, value);
        }

        /// <summary>Marks the ViewModel as having unsaved changes.</summary>
        protected void MarkDirty() => IsDirty = true;

        /// <summary>Resets the dirty flag (call after save or load).</summary>
        protected void ResetDirtyTracking() => IsDirty = false;

        // ── IDisposable ──

        private bool _disposed;

        /// <summary>Disposes the DbGuard semaphore. Override in derived classes for additional cleanup.</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DbGuard.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
