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

namespace Taipei.Views
{
    /// <summary>
    /// Interaction logic for AboutTab.xaml
    /// </summary>
    public partial class AboutTab : UserControl
    {
        public ImageSource TaipeiImage { get; set; }

        public AboutTab()
        {
            InitializeComponent();

            TaipeiImage = new BitmapImage(new Uri("https://assets.vogue.com/photos/59b3295ff83970107aed8f14/1:1/w_2400,h_2400,c_limit/00-promo-image-taipei.jpg"));

            DataContext = this;
        }
    }
}
