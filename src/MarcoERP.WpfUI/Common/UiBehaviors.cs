using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace MarcoERP.WpfUI.Common
{
    public static class LoadedCommandBehavior
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(LoadedCommandBehavior),
            new PropertyMetadata(null, OnCommandChanged));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
            "CommandParameter",
            typeof(object),
            typeof(LoadedCommandBehavior),
            new PropertyMetadata(null));

        public static void SetCommand(DependencyObject element, ICommand value) => element.SetValue(CommandProperty, value);
        public static ICommand GetCommand(DependencyObject element) => (ICommand)element.GetValue(CommandProperty);

        public static void SetCommandParameter(DependencyObject element, object value) => element.SetValue(CommandParameterProperty, value);
        public static object GetCommandParameter(DependencyObject element) => element.GetValue(CommandParameterProperty);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.Loaded -= OnLoaded;
                if (e.NewValue is ICommand)
                    element.Loaded += OnLoaded;
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var command = GetCommand(element);
                var parameter = GetCommandParameter(element);
                if (command != null && command.CanExecute(parameter))
                    command.Execute(parameter);
            }
        }
    }

    public static class EnterKeyFocusBehavior
    {
        public static readonly DependencyProperty TargetElementProperty = DependencyProperty.RegisterAttached(
            "TargetElement",
            typeof(IInputElement),
            typeof(EnterKeyFocusBehavior),
            new PropertyMetadata(null, OnTargetChanged));

        public static void SetTargetElement(DependencyObject element, IInputElement value) => element.SetValue(TargetElementProperty, value);
        public static IInputElement GetTargetElement(DependencyObject element) => (IInputElement)element.GetValue(TargetElementProperty);

        private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.PreviewKeyDown -= OnPreviewKeyDown;
                if (e.NewValue is IInputElement)
                    element.PreviewKeyDown += OnPreviewKeyDown;
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (sender is DependencyObject element)
            {
                var target = GetTargetElement(element);
                if (target != null)
                    Keyboard.Focus(target);
            }
        }
    }

    public static class TextBoxEnterCommandBehavior
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(TextBoxEnterCommandBehavior),
            new PropertyMetadata(null, OnCommandChanged));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
            "CommandParameter",
            typeof(object),
            typeof(TextBoxEnterCommandBehavior),
            new PropertyMetadata(null));

        public static void SetCommand(DependencyObject element, ICommand value) => element.SetValue(CommandProperty, value);
        public static ICommand GetCommand(DependencyObject element) => (ICommand)element.GetValue(CommandProperty);

        public static void SetCommandParameter(DependencyObject element, object value) => element.SetValue(CommandParameterProperty, value);
        public static object GetCommandParameter(DependencyObject element) => element.GetValue(CommandParameterProperty);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.PreviewKeyDown -= OnPreviewKeyDown;
                if (e.NewValue is ICommand)
                    element.PreviewKeyDown += OnPreviewKeyDown;
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (sender is not TextBox textBox)
                return;

            var command = GetCommand(textBox);
            if (command == null)
                return;

            var parameter = GetCommandParameter(textBox) ?? textBox.Text;
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
                e.Handled = true;
            }
        }
    }

    public static class WindowDragBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(WindowDragBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                if (e.NewValue is bool enabled && enabled)
                    element.MouseLeftButtonDown += OnMouseLeftButtonDown;
            }
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DependencyObject element && e.ButtonState == MouseButtonState.Pressed)
            {
                var window = Window.GetWindow(element);
                window?.DragMove();
            }
        }
    }

    public static class CloseWindowBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(CloseWindowBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ButtonBase button)
            {
                button.Click -= OnClick;
                if (e.NewValue is bool enabled && enabled)
                    button.Click += OnClick;
            }
        }

        private static void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is DependencyObject element)
            {
                var window = Window.GetWindow(element);
                window?.Close();
            }
        }
    }

    public static class SelectAllOnFocusBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(SelectAllOnFocusBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox)
                return;

            textBox.GotKeyboardFocus -= OnGotKeyboardFocus;
            textBox.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;

            if (e.NewValue is bool enabled && enabled)
            {
                textBox.GotKeyboardFocus += OnGotKeyboardFocus;
                textBox.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            }
        }

        private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
                textBox.SelectAll();
        }

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            if (!textBox.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                textBox.Focus();
            }
        }
    }

    public static class DataGridSmartEntryBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(DataGridSmartEntryBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        public static readonly DependencyProperty AddLineCommandProperty = DependencyProperty.RegisterAttached(
            "AddLineCommand",
            typeof(ICommand),
            typeof(DataGridSmartEntryBehavior),
            new PropertyMetadata(null));

        public static void SetAddLineCommand(DependencyObject element, ICommand value) => element.SetValue(AddLineCommandProperty, value);
        public static ICommand GetAddLineCommand(DependencyObject element) => (ICommand)element.GetValue(AddLineCommandProperty);

        public static readonly DependencyProperty RemoveLineCommandProperty = DependencyProperty.RegisterAttached(
            "RemoveLineCommand",
            typeof(ICommand),
            typeof(DataGridSmartEntryBehavior),
            new PropertyMetadata(null));

        public static void SetRemoveLineCommand(DependencyObject element, ICommand value) => element.SetValue(RemoveLineCommandProperty, value);
        public static ICommand GetRemoveLineCommand(DependencyObject element) => (ICommand)element.GetValue(RemoveLineCommandProperty);

        public static readonly DependencyProperty CancelEditCommandProperty = DependencyProperty.RegisterAttached(
            "CancelEditCommand",
            typeof(ICommand),
            typeof(DataGridSmartEntryBehavior),
            new PropertyMetadata(null));

        public static void SetCancelEditCommand(DependencyObject element, ICommand value) => element.SetValue(CancelEditCommandProperty, value);
        public static ICommand GetCancelEditCommand(DependencyObject element) => (ICommand)element.GetValue(CancelEditCommandProperty);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid)
                return;

            grid.PreviewKeyDown -= OnPreviewKeyDown;
            if (e.NewValue is bool enabled && enabled)
                grid.PreviewKeyDown += OnPreviewKeyDown;
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not DataGrid grid)
                return;

            if (!GetIsEnabled(grid))
                return;

            if (HandleShortcutKeys(grid, e))
                return;

            if (e.Key == Key.Enter)
                HandleEnterKey(grid, e);
        }

        private static bool HandleShortcutKeys(DataGrid grid, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                grid.BeginEdit();
                e.Handled = true;
                return true;
            }

            if (e.Key == Key.Escape)
            {
                var cancel = GetCancelEditCommand(grid);
                if (cancel != null && cancel.CanExecute(null))
                    cancel.Execute(null);
                e.Handled = true;
                return true;
            }

            if (e.Key == Key.Delete)
            {
                // Don't remove row if user is editing a cell (e.g., deleting text)
                if (e.OriginalSource is System.Windows.Controls.TextBox)
                    return false;

                var cmd = GetRemoveLineCommand(grid);
                var parameter = grid.SelectedItem;
                if (cmd != null && parameter != null && cmd.CanExecute(parameter))
                    cmd.Execute(parameter);
                e.Handled = true;
                return true;
            }

            return false;
        }

        private static void HandleEnterKey(DataGrid grid, KeyEventArgs e)
        {
            e.Handled = true;

            if (grid.CurrentCell.Column == null)
                return;

            var editableColumns = GetEditableColumns(grid);
            if (editableColumns.Count == 0)
                return;

            grid.CommitEdit(DataGridEditingUnit.Cell, true);
            grid.CommitEdit(DataGridEditingUnit.Row, true);

            var currentRowIndex = grid.Items.IndexOf(grid.CurrentItem);
            var currentEditableIndex = GetCurrentEditableColumnIndex(grid, editableColumns);
            var nextColumnIndex = currentEditableIndex + 1;

            if (nextColumnIndex < editableColumns.Count)
            {
                MoveToCell(grid, currentRowIndex, editableColumns[nextColumnIndex]);
                return;
            }

            HandleEnterAtEndOfRow(grid, currentRowIndex, editableColumns[0]);
        }

        private static void HandleEnterAtEndOfRow(DataGrid grid, int currentRowIndex, DataGridColumn firstEditableColumn)
        {
            var isLastRow = currentRowIndex >= grid.Items.Count - 1;
            if (isLastRow)
            {
                var add = GetAddLineCommand(grid);
                if (add != null && add.CanExecute(null))
                    add.Execute(null);

                grid.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    var newRowIndex = grid.Items.Count - 1;
                    MoveToCell(grid, newRowIndex, firstEditableColumn);
                }));
                return;
            }

            MoveToCell(grid, currentRowIndex + 1, firstEditableColumn);
        }

        private static List<DataGridColumn> GetEditableColumns(DataGrid grid)
        {
            return grid.Columns
                .Where(c => c.Visibility == Visibility.Visible && !c.IsReadOnly)
                .OrderBy(c => c.DisplayIndex)
                .ToList();
        }

        private static int GetCurrentEditableColumnIndex(DataGrid grid, List<DataGridColumn> editableColumns)
        {
            var currentColumn = grid.CurrentCell.Column;
            var idx = editableColumns.IndexOf(currentColumn);
            return idx < 0 ? 0 : idx;
        }

        private static void MoveToCell(DataGrid grid, int rowIndex, DataGridColumn column)
        {
            if (rowIndex < 0 || rowIndex >= grid.Items.Count)
                return;

            var item = grid.Items[rowIndex];
            grid.SelectedItem = item;
            grid.CurrentCell = new DataGridCellInfo(item, column);
            grid.ScrollIntoView(item, column);
            grid.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                grid.BeginEdit();
            }));
        }
    }
}
