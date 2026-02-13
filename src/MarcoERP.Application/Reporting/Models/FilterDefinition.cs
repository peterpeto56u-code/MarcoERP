using System;
using System.Collections.Generic;

namespace MarcoERP.Application.Reporting.Models
{
    /// <summary>
    /// Describes a single user-configurable filter for a report.
    /// </summary>
    public sealed class FilterDefinition
    {
        /// <summary>Unique key for this filter (e.g. "dateFrom", "customerId").</summary>
        public string Key { get; set; }

        /// <summary>Arabic display label.</summary>
        public string Label { get; set; }

        /// <summary>The type of filter control to render.</summary>
        public FilterType FilterType { get; set; }

        /// <summary>Property path on the row DTO this filter applies to.</summary>
        public string BindingPath { get; set; }

        /// <summary>For DropDown/MultiSelect filters: the list of available options.</summary>
        public List<FilterOption> Options { get; set; } = new();

        /// <summary>Default value (if any).</summary>
        public object DefaultValue { get; set; }

        /// <summary>Minimum complexity mode required to show this filter.</summary>
        public ReportComplexityMode MinComplexity { get; set; } = ReportComplexityMode.Simple;

        /// <summary>Whether this filter is required.</summary>
        public bool IsRequired { get; set; }

        /// <summary>Placeholder text for text/search filters.</summary>
        public string Placeholder { get; set; }
    }

    public enum FilterType
    {
        /// <summary>Free-text search.</summary>
        Text,

        /// <summary>Date picker.</summary>
        Date,

        /// <summary>Date range (From-To).</summary>
        DateRange,

        /// <summary>Single-select dropdown.</summary>
        DropDown,

        /// <summary>Multi-select dropdown.</summary>
        MultiSelect,

        /// <summary>Numeric range (Min-Max).</summary>
        NumericRange,

        /// <summary>Boolean toggle.</summary>
        Toggle,

        /// <summary>Entity lookup (with search-as-you-type).</summary>
        EntityLookup
    }

    /// <summary>
    /// A single option in a DropDown/MultiSelect filter.
    /// </summary>
    public sealed class FilterOption
    {
        public object Value { get; set; }
        public string DisplayText { get; set; }

        public FilterOption() { }
        public FilterOption(object value, string displayText)
        {
            Value = value;
            DisplayText = displayText;
        }
    }

    /// <summary>
    /// Represents an active filter value applied by the user.
    /// </summary>
    public sealed class ActiveFilter
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public object ValueTo { get; set; } // For range filters

        public ActiveFilter() { }
        public ActiveFilter(string key, object value, object valueTo = null)
        {
            Key = key;
            Value = value;
            ValueTo = valueTo;
        }
    }

    /// <summary>
    /// A saved filter preset that users can name and reuse.
    /// </summary>
    public sealed class FilterPreset
    {
        public string Name { get; set; }
        public List<ActiveFilter> Filters { get; set; } = new();
        public bool IsDefault { get; set; }
    }
}
