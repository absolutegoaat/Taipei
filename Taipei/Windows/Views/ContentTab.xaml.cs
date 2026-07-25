using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Taipei.Windows.Views
{
    /// <summary>
    /// Interaction logic for ContentTab.xaml
    /// </summary>
    public partial class ContentTab : UserControl
    {
        private string? _pendingHtml;

        public ContentTab()
        {
            InitializeComponent();
            Loaded += ContentTab_Loaded;
        }

        private async void ContentTab_Loaded(object sender, RoutedEventArgs e)
        {
            await HtmlPreview.EnsureCoreWebView2Async(null);
            if (_pendingHtml != null)
            {
                HtmlPreview.CoreWebView2.NavigateToString(_pendingHtml);
            }
        }

        public async void ShowHtml(string html)
        {
            _pendingHtml = html;
            if (HtmlPreview.CoreWebView2 != null)
            {
                HtmlPreview.CoreWebView2.NavigateToString(html);
            }
        }
    }
}
