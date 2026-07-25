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
    public partial class CookiesTab : UserControl
    {
        public CookiesTab()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void CopyCookieVal_Click(object sender, RoutedEventArgs e)
        {
            if (CookiesDatagrid.SelectedItem is CookieLog selectedRow)
            {
                Clipboard.SetText(selectedRow.CookieValue ?? string.Empty);
                MessageBox.Show("Copied!", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CookiesDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CopyCookieVal.IsEnabled = CookiesDatagrid.SelectedItem != null;
        }
    }
}
