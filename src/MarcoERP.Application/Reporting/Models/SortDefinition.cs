using System.ComponentModel;

namespace MarcoERP.Application.Reporting.Models
{
    /// <summary>
    /// Defines the current sort column and direction for a report.
    /// </summary>
    public sealed class SortDefinition
    {
        /// <summary>Property name to sort by.</summary>
        public string PropertyName { get; set; }

        /// <summary>Sort direction.</summary>
        public ListSortDirection Direction { get; set; } = ListSortDirection.Ascending;

        public SortDefinition() { }
        public SortDefinition(string propertyName, ListSortDirection direction = ListSortDirection.Ascending)
        {
            PropertyName = propertyName;
            Direction = direction;
        }
    }
}
