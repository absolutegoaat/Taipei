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
using Taipei.Models.Log;

namespace Taipei.Windows.Views
{
    /// <summary>
    /// Interaction logic for HeadersTab.xaml
    /// </summary>
    public partial class HeadersTab : UserControl
    {
        public HeadersTab()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void CopyHeadVal_Click(object sender, RoutedEventArgs e)
        {
            if (HeadersGrid.SelectedItem is HeaderLog selectedRow)
            {
                Clipboard.SetText(selectedRow.HeaderValue ?? string.Empty);
            }
        }

        private void HeadersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CopyHeadVal.IsEnabled = HeadersGrid.SelectedItem != null;
        }
    }
}
