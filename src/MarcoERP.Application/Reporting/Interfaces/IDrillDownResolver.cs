using MarcoERP.Domain.Enums;

namespace MarcoERP.Application.Reporting.Interfaces
{
    /// <summary>
    /// Resolves a <see cref="SourceType"/> to a navigation key
    /// that the UI's INavigationService can use to open the source document.
    /// </summary>
    public interface IDrillDownResolver
    {
        /// <summary>
        /// Gets the navigation view key for the given source type.
        /// Returns null if drill-down is not supported for this source type.
        /// </summary>
        string ResolveNavigationKey(SourceType sourceType);

        /// <summary>
        /// Gets the Arabic display label for the drill-down action.
        /// </summary>
        string GetActionLabel(SourceType sourceType);
    }
}
