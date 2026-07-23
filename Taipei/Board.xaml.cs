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
using System.Net.Http;

namespace Taipei
{
    /// <summary>
    /// Interaction logic for Board.xaml
    /// </summary>
    
    public partial class Board : MetroWindow
    {
        public string LoggedIn { get; set; } = $"Hello, {Environment.UserName}";

        public Board()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
