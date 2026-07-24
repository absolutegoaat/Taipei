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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Taipei.Models;

namespace Taipei.Windows
{
    /// <summary>
    /// Interaction logic for WindowViewLog.xaml
    /// </summary>
    public partial class WindowViewLog : MetroWindow
    {
        public WindowViewLog(int id)
        {
            InitializeComponent();

            GetInfo(id);
        }

        private static void GetInfo(int id)
        {

        }
    }
}
