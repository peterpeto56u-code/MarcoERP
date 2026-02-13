using System.Windows;
using System.Windows.Controls;

namespace MarcoERP.WpfUI.Reporting.Controls
{
    /// <summary>
    /// Reusable report chrome: header, filter bar, KPI cards, pagination, loading overlay.
    /// Concrete report views provide <see cref="FilterTemplate"/> and <see cref="GridTemplate"/>
    /// to inject their specific filter controls and DataGrid column definitions.
    /// </summary>
    public partial class ReportViewBase : UserControl
    {
        public ReportViewBase()
        {
            InitializeComponent();
        }

        /// <summary>
        /// DataTemplate for the filter area (DatePickers, ComboBoxes, etc.).
        /// Bound to the report ViewModel.
        /// </summary>
        public static readonly DependencyProperty FilterTemplateProperty =
            DependencyProperty.Register(nameof(FilterTemplate), typeof(DataTemplate), typeof(ReportViewBase));

        public DataTemplate FilterTemplate
        {
            get => (DataTemplate)GetValue(FilterTemplateProperty);
            set => SetValue(FilterTemplateProperty, value);
        }

        /// <summary>
        /// DataTemplate for the main DataGrid area.
        /// Bound to the report ViewModel.
        /// </summary>
        public static readonly DependencyProperty GridTemplateProperty =
            DependencyProperty.Register(nameof(GridTemplate), typeof(DataTemplate), typeof(ReportViewBase));

        public DataTemplate GridTemplate
        {
            get => (DataTemplate)GetValue(GridTemplateProperty);
            set => SetValue(GridTemplateProperty, value);
        }
    }
}
