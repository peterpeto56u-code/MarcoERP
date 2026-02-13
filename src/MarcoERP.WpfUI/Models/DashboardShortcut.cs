namespace MarcoERP.WpfUI.Models
{
    /// <summary>
    /// Persistence model for a single dashboard shortcut card.
    /// Serialized to/from dashboard_shortcuts.json.
    /// </summary>
    public sealed class DashboardShortcut
    {
        public string ViewKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        /// <summary>PackIconKind name stored as string for JSON serialization.</summary>
        public string IconKind { get; set; } = "OpenInNew";
    }
}
