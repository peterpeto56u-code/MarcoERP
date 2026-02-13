using System.Windows;
using System.Windows.Controls;

namespace MarcoERP.WpfUI.Navigation
{
    public sealed class NavigationItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HeaderTemplate { get; set; }

        public DataTemplate SeparatorTemplate { get; set; }

        public DataTemplate ItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is NavigationItem navItem)
            {
                return navItem.ItemType switch
                {
                    NavigationItemType.Header => HeaderTemplate,
                    NavigationItemType.Separator => SeparatorTemplate,
                    _ => ItemTemplate
                };
            }

            return base.SelectTemplate(item, container);
        }
    }
}
