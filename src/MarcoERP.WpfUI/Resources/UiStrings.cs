using System.Globalization;
using System.Resources;

namespace MarcoERP.WpfUI.Resources
{
    public static class UiStrings
    {
        private static readonly ResourceManager ResourceManager = new(
            "MarcoERP.WpfUI.Resources.UiStrings",
            typeof(UiStrings).Assembly);

        public static string UnsavedChangesPrompt =>
            ResourceManager.GetString(nameof(UnsavedChangesPrompt), CultureInfo.CurrentUICulture);

        public static string UnsavedChangesTitle =>
            ResourceManager.GetString(nameof(UnsavedChangesTitle), CultureInfo.CurrentUICulture);

        public static string UnsavedChangesSaveFailed =>
            ResourceManager.GetString(nameof(UnsavedChangesSaveFailed), CultureInfo.CurrentUICulture);
    }
}
