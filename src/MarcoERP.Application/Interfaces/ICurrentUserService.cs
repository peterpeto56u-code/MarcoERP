using System.Collections.Generic;

namespace MarcoERP.Application.Interfaces
{
    /// <summary>
    /// Provides the identity of the currently authenticated user.
    /// Implementation: Infrastructure layer (reads from auth context).
    /// Extended in Phase 5C with UserId, RoleId, RoleName, Permissions, HasPermission.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>Username of the currently authenticated user.</summary>
        string Username { get; }

        /// <summary>True if a user is authenticated.</summary>
        bool IsAuthenticated { get; }

        /// <summary>Database ID of the currently authenticated user.</summary>
        int UserId { get; }

        /// <summary>Role ID of the currently authenticated user.</summary>
        int RoleId { get; }

        /// <summary>Arabic name of the user's role.</summary>
        string RoleNameAr { get; }

        /// <summary>Full name of the authenticated user in Arabic.</summary>
        string FullNameAr { get; }

        /// <summary>List of permission keys assigned to the user's role.</summary>
        IReadOnlyList<string> Permissions { get; }

        /// <summary>Checks if the current user has a specific permission.</summary>
        bool HasPermission(string permissionKey);

        /// <summary>Sets the current user after successful authentication (legacy — use SetUser overload).</summary>
        void SetUser(string username);

        /// <summary>Sets the current user with full identity after successful authentication.</summary>
        void SetUser(int userId, string username, string fullNameAr, int roleId, string roleNameAr, IReadOnlyList<string> permissions);

        /// <summary>Clears the current user on logout.</summary>
        void ClearUser();
    }
}
