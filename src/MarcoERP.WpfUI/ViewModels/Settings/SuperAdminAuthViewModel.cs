using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Security;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Security;
using MarcoERP.Application.Interfaces.Settings;

namespace MarcoERP.WpfUI.ViewModels.Settings
{
    /// <summary>
    /// Phase 7C: Super Admin Authentication Dialog ViewModel.
    /// Uses IAuthenticationService — no backdoor, no bypass.
    /// Phase 7E: Session Isolation — does NOT change ICurrentUserService.
    /// Phase 7F: Security Hardening — rate limiting, generic errors, no raw passwords.
    /// </summary>
    public sealed class SuperAdminAuthViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authService;
        private readonly IGovernanceAuditService _auditService;
        private readonly IDateTimeProvider _dateTimeProvider;

        // ── 7F: Rate limiting ───────────────────────────────────────
        private static int _failedAttempts;
        private static DateTime? _lockoutUntil;
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

        public SuperAdminAuthViewModel(
            IAuthenticationService authService,
            IGovernanceAuditService auditService,
            IDateTimeProvider dateTimeProvider)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));

            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
        }

        // ── Properties ───────────────────────────────────────────

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                    RelayCommand.RaiseCanExecuteChanged();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                    RelayCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Authenticated username after successful login.
        /// Used by the caller to know who accessed governance (7E: session isolation).
        /// </summary>
        public string AuthenticatedUsername { get; private set; }

        // ── Commands ─────────────────────────────────────────────

        public ICommand LoginCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>Callback to close the dialog. True = authenticated, False = cancelled.</summary>
        public Action<bool> RequestClose { get; set; }

        // ── Methods ──────────────────────────────────────────────

        private bool CanLogin()
        {
            return !IsBusy
                && !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Password);
        }

        private async Task LoginAsync()
        {
            IsBusy = true;
            ClearError();

            try
            {
                // ── 7F: Check lockout ───────────────────────────
                if (_lockoutUntil.HasValue && _dateTimeProvider.UtcNow < _lockoutUntil.Value)
                {
                    var remaining = (_lockoutUntil.Value - _dateTimeProvider.UtcNow).TotalMinutes;
                    ErrorMessage = $"تم تعليق الوصول مؤقتاً. حاول بعد {Math.Ceiling(remaining)} دقيقة.";
                    return;
                }

                // ── 7C: Authenticate through IAuthenticationService ──
                var result = await _authService.LoginAsync(new LoginDto
                {
                    Username = Username?.Trim(),
                    Password = Password
                });

                // 7F: Clear password from memory immediately
                Password = string.Empty;

                if (!result.IsSuccess)
                {
                    _failedAttempts++;

                    // Log failed attempt (7D: Audit)
                    await _auditService.LogAsync(
                        "GovernanceAccessDenied",
                        Username?.Trim() ?? "Unknown",
                        $"فشل محاولة دخول وحدة التحكم (المحاولة {_failedAttempts})");

                    if (_failedAttempts >= MaxFailedAttempts)
                    {
                        _lockoutUntil = _dateTimeProvider.UtcNow.Add(LockoutDuration);
                        _failedAttempts = 0;
                        ErrorMessage = "تم تعليق الوصول مؤقتاً بسبب محاولات فاشلة متكررة.";
                    }
                    else
                    {
                        // 7F: Generic error — hide exact reason
                        ErrorMessage = "بيانات الدخول غير صحيحة أو لا تملك الصلاحية المطلوبة.";
                    }
                    return;
                }

                // ── 7C: Check Governance.Access permission ──────
                var loginData = result.Data;
                if (!loginData.Permissions.Contains(PermissionKeys.GovernanceAccess))
                {
                    _failedAttempts++;

                    // Log unauthorized attempt (7D: Audit)
                    await _auditService.LogAsync(
                        "GovernanceAccessDenied",
                        loginData.Username,
                        $"المستخدم ليس لديه صلاحية governance.access (المحاولة {_failedAttempts})");

                    if (_failedAttempts >= MaxFailedAttempts)
                    {
                        _lockoutUntil = _dateTimeProvider.UtcNow.Add(LockoutDuration);
                        _failedAttempts = 0;
                        ErrorMessage = "تم تعليق الوصول مؤقتاً بسبب محاولات فاشلة متكررة.";
                    }
                    else
                    {
                        // 7F: Same generic error — don't reveal that auth succeeded but permission missing
                        ErrorMessage = "بيانات الدخول غير صحيحة أو لا تملك الصلاحية المطلوبة.";
                    }
                    return;
                }

                // ── Success: reset counters ─────────────────────
                _failedAttempts = 0;
                _lockoutUntil = null;

                // 7E: Store authenticated username for governance context only — NOT in ICurrentUserService
                AuthenticatedUsername = loginData.Username;

                // 7D: Log successful access
                await _auditService.LogAsync(
                    "GovernanceAccessGranted",
                    loginData.Username,
                    "Governance Console Access Granted");

                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = "حدث خطأ غير متوقع.";
                System.Diagnostics.Debug.WriteLine($"[GovernanceAuth] Error: {ex}");
            }
            finally
            {
                IsBusy = false;
                RelayCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
