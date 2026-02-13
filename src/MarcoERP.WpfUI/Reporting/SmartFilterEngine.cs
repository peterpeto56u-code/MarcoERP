using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MarcoERP.Application.Reporting.Models;

namespace MarcoERP.WpfUI.Reporting
{
    /// <summary>
    /// Manages filter definitions, active filter values, and filter presets for a report.
    /// Works alongside server-side filtering — this engine tracks state only;
    /// actual filtering is done by <see cref="IPagedReportQuery{T}"/>.
    /// </summary>
    public sealed class SmartFilterEngine
    {
        private readonly List<FilterDefinition> _definitions = new();
        private readonly Dictionary<string, ActiveFilter> _activeFilters = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<FilterPreset> _presets = new();

        /// <summary>All available filter definitions for the current report.</summary>
        public IReadOnlyList<FilterDefinition> Definitions => _definitions;

        /// <summary>Currently applied filters.</summary>
        public IReadOnlyList<ActiveFilter> ActiveFilters
            => _activeFilters.Values.ToList();

        /// <summary>Number of active filters.</summary>
        public int ActiveFilterCount => _activeFilters.Count;

        /// <summary>Saved presets.</summary>
        public IReadOnlyList<FilterPreset> Presets => _presets;

        /// <summary>Raised when any filter value changes.</summary>
        public event EventHandler FiltersChanged;

        /// <summary>
        /// Initializes the filter engine with definitions from <see cref="ReportDefinition"/>.
        /// Applies default values.
        /// </summary>
        public void Initialize(ReportDefinition definition)
        {
            _definitions.Clear();
            _activeFilters.Clear();
            _presets.Clear();

            if (definition == null) return;

            _definitions.AddRange(definition.Filters);
            _presets.AddRange(definition.Presets);

            // Apply default values
            foreach (var filter in _definitions.Where(f => f.DefaultValue != null))
            {
                _activeFilters[filter.Key] = new ActiveFilter(filter.Key, filter.DefaultValue);
            }
        }

        /// <summary>
        /// Sets a filter value. Pass null value to clear the filter.
        /// </summary>
        public void SetFilter(string key, object value, object valueTo = null)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (value == null)
            {
                _activeFilters.Remove(key);
            }
            else
            {
                _activeFilters[key] = new ActiveFilter(key, value, valueTo);
            }

            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Gets the current value for a specific filter.</summary>
        public object GetFilterValue(string key)
            => _activeFilters.TryGetValue(key, out var f) ? f.Value : null;

        /// <summary>Gets the current range-end value for a specific range filter.</summary>
        public object GetFilterValueTo(string key)
            => _activeFilters.TryGetValue(key, out var f) ? f.ValueTo : null;

        /// <summary>Checks if a specific filter is currently active.</summary>
        public bool IsFilterActive(string key)
            => _activeFilters.ContainsKey(key);

        /// <summary>Clears all active filters and resets to defaults.</summary>
        public void ClearAll()
        {
            _activeFilters.Clear();

            foreach (var filter in _definitions.Where(f => f.DefaultValue != null))
            {
                _activeFilters[filter.Key] = new ActiveFilter(filter.Key, filter.DefaultValue);
            }

            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Applies a saved preset, replacing all active filters.
        /// </summary>
        public void ApplyPreset(FilterPreset preset)
        {
            if (preset == null) return;

            _activeFilters.Clear();
            foreach (var filter in preset.Filters)
            {
                _activeFilters[filter.Key] = filter;
            }

            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Saves current filters as a named preset.
        /// </summary>
        public FilterPreset SaveAsPreset(string name)
        {
            var preset = new FilterPreset
            {
                Name = name,
                Filters = _activeFilters.Values.Select(f =>
                    new ActiveFilter(f.Key, f.Value, f.ValueTo)).ToList()
            };

            // Replace if same name exists
            _presets.RemoveAll(p => p.Name == name);
            _presets.Add(preset);

            return preset;
        }

        /// <summary>
        /// Returns visible filter definitions based on current complexity mode.
        /// </summary>
        public IReadOnlyList<FilterDefinition> GetVisibleFilters(ReportComplexityMode mode)
            => _definitions.Where(f => f.MinComplexity <= mode).ToList();
    }
}
