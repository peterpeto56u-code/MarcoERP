using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MarcoERP.WpfUI.Views.Common
{
    /// <summary>
    /// Reusable search lookup window. Provides real-time substring filtering
    /// across multiple fields and a professional DataGrid for entity selection.
    /// </summary>
    public partial class SearchLookupWindow : Window
    {
        private readonly ICollectionView _view;
        private readonly string _displayPath;
        private readonly string _valuePath;
        private readonly PropertyInfo[] _searchableProps;
        private string _searchText = "";

        /// <summary>The value (Id) of the selected item, or null if cancelled.</summary>
        public object SelectedValue { get; private set; }

        /// <summary>
        /// Creates a new search lookup window.
        /// </summary>
        /// <param name="itemsSource">The collection to search in.</param>
        /// <param name="displayPath">Property name for the main display column.</param>
        /// <param name="valuePath">Property name for the value (Id).</param>
        /// <param name="title">Window title.</param>
        public SearchLookupWindow(IEnumerable itemsSource, string displayPath, string valuePath, string title)
        {
            InitializeComponent();

            Title = title;
            _displayPath = displayPath;
            _valuePath = valuePath;

            // Materialise the collection for independent filtering
            var list = itemsSource.Cast<object>().ToList();
            _view = CollectionViewSource.GetDefaultView(list);
            _view.Filter = FilterPredicate;

            // Detect searchable string properties
            if (list.Count > 0)
            {
                var type = list[0].GetType();
                _searchableProps = GetSearchableProperties(type, displayPath);
                BuildColumns(type, displayPath, valuePath);
            }
            else
            {
                _searchableProps = Array.Empty<PropertyInfo>();
                // Build columns from the generic type even when list is empty
                var enumerableType = itemsSource.GetType();
                var itemType = enumerableType.GetGenericArguments().FirstOrDefault();
                if (itemType != null)
                    BuildColumns(itemType, displayPath, valuePath);
            }

            dgResults.ItemsSource = _view;
            txtResultCount.Text = $"({list.Count})";
        }

        // ── Column builder ───────────────────────────────────────

        private void BuildColumns(Type type, string displayPath, string valuePath)
        {
            dgResults.Columns.Clear();

            // Always show the ID column (narrow)
            var idProp = type.GetProperty(valuePath);
            if (idProp != null)
            {
                dgResults.Columns.Add(new DataGridTextColumn
                {
                    Header = "#",
                    Binding = new Binding(valuePath),
                    Width = new DataGridLength(50),
                    IsReadOnly = true,
                    ElementStyle = CreateCellStyle(HorizontalAlignment.Center)
                });
            }

            // Main display column (wide, star) — RTL right-aligned
            dgResults.Columns.Add(new DataGridTextColumn
            {
                Header = "الاسم",
                Binding = new Binding(displayPath),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                IsReadOnly = true,
                ElementStyle = CreateCellStyle(HorizontalAlignment.Right)
            });

            // Optional auxiliary columns
            TryAddColumn(type, "Code", "الكود", 80);
            TryAddColumn(type, "Barcode", "الباركود", 100);
            TryAddColumn(type, "Phone", "الهاتف", 100);
            TryAddColumn(type, "InvoiceNumber", "رقم الفاتورة", 100);
            TryAddColumn(type, "AccountNumber", "رقم الحساب", 90);
        }

        private void TryAddColumn(Type type, string propName, string header, double width)
        {
            if (propName == _displayPath || propName == _valuePath) return;
            var prop = type.GetProperty(propName);
            if (prop == null || !IsDisplayableType(prop.PropertyType)) return;

            dgResults.Columns.Add(new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(propName),
                Width = new DataGridLength(width),
                IsReadOnly = true,
                ElementStyle = CreateCellStyle(HorizontalAlignment.Center)
            });
        }

        private static bool IsDisplayableType(Type t)
        {
            var underlying = Nullable.GetUnderlyingType(t) ?? t;
            return underlying == typeof(string) || underlying == typeof(int) || underlying == typeof(long)
                || underlying == typeof(decimal) || underlying == typeof(double);
        }

        private static Style CreateCellStyle(HorizontalAlignment alignment)
        {
            var style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, alignment));
            return style;
        }

        // ── Searchable properties ────────────────────────────────

        private static PropertyInfo[] GetSearchableProperties(Type type, string displayPath)
        {
            var candidates = new[] { displayPath, "NameAr", "NameEn", "Code", "Barcode",
                "Phone", "AccountNameAr", "InvoiceNumber", "AccountNumber" };

            return candidates
                .Distinct()
                .Select(n => type.GetProperty(n))
                .Where(p => p != null && (p.PropertyType == typeof(string)
                    || p.PropertyType == typeof(int) || p.PropertyType == typeof(int?)
                    || p.PropertyType == typeof(long) || p.PropertyType == typeof(long?)))
                .ToArray()!;
        }

        // ── Filtering ────────────────────────────────────────────

        private bool FilterPredicate(object item)
        {
            if (string.IsNullOrWhiteSpace(_searchText)) return true;

            var needle = _searchText.Trim().ToLowerInvariant();

            foreach (var prop in _searchableProps)
            {
                var val = prop.GetValue(item)?.ToString();
                if (val != null && val.ToLowerInvariant().Contains(needle))
                    return true;
            }

            return false;
        }

        // ── Event handlers ───────────────────────────────────────

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = txtSearch.Text;
            _view?.Refresh();
            var count = _view?.Cast<object>().Count() ?? 0;
            txtResultCount.Text = $"({count})";
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e) => SelectCurrentItem();

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => SelectCurrentItem();

        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectCurrentItem();
                e.Handled = true;
            }
        }

        private void SelectCurrentItem()
        {
            if (dgResults.SelectedItem == null) return;

            var type = dgResults.SelectedItem.GetType();
            var prop = type.GetProperty(_valuePath);
            if (prop != null)
            {
                SelectedValue = prop.GetValue(dgResults.SelectedItem);
                DialogResult = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
            }
        }
    }
}
