using MarcoERP.Application.Interfaces;

namespace MarcoERP.Application.Common
{
    /// <summary>
    /// Centralized authorization guard methods.
    /// AUTHZ-01: All authorization checks happen at the Application layer.
    /// Admin (RoleId == 1) bypasses all permission checks via ICurrentUserService.HasPermission.
    /// </summary>
    public static class AuthorizationGuard
    {
        private const string NotAuthenticatedMsg = "يجب تسجيل الدخول أولاً.";
        private const string NotAuthorizedMsg = "لا تملك الصلاحية لتنفيذ هذه العملية.";

        /// <summary>
        /// Ensures the user is authenticated and has the required permission.
        /// Returns a failed ServiceResult if unauthorized; null if authorized.
        /// </summary>
        public static ServiceResult<T> Check<T>(ICurrentUserService currentUser, string permissionKey)
        {
            if (!currentUser.IsAuthenticated)
                return ServiceResult<T>.Failure(NotAuthenticatedMsg);

            if (!currentUser.HasPermission(permissionKey))
                return ServiceResult<T>.Failure(NotAuthorizedMsg);

            return null; // authorized — caller proceeds
        }

        /// <summary>
        /// Ensures the user is authenticated and has the required permission.
        /// Returns a failed ServiceResult if unauthorized; null if authorized.
        /// </summary>
        public static ServiceResult Check(ICurrentUserService currentUser, string permissionKey)
        {
            if (!currentUser.IsAuthenticated)
                return ServiceResult.Failure(NotAuthenticatedMsg);

            if (!currentUser.HasPermission(permissionKey))
                return ServiceResult.Failure(NotAuthorizedMsg);

            return null; // authorized — caller proceeds
        }
    }
}
