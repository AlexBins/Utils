namespace Hosting.Wpf;

using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;

/// <summary>
/// Interaction logic for PageHost.xaml
/// </summary>
internal partial class PageHost
{
    public PageHost(IEnumerable<Page> pages)
    {
        InitializeComponent();

        foreach (Page page in pages)
        {
            var tab = new TabItem
            {
                Content = new Frame
                {
                    Content = page
                }
            };
            tab.SetBinding(HeaderedContentControl.HeaderProperty, new Binding(Page.TitleProperty.Name)
            {
                Source = page
            });
            Tabs.Items.Add(tab);
        }
    }
}
