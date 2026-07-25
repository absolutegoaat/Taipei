using MahApps.Metro.Controls;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using Taipei.Models;
using Taipei.Models.Log;
using Taipei.Windows.Views;

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
            LoginConfig? config = Utils.Utils.InitConfig();
            GetInfo(id, config, DetailsTabControl);
        }

        private static async void GetInfo(int id, LoginConfig? config, DetailsTab detailsTab)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("taipei-auth", config?.Token);
                HttpResponseMessage res = await client.GetAsync($"{config?.RootUrl}/taipei/api/log/{id}");
                if (res.IsSuccessStatusCode)
                {
                    string body = await res.Content.ReadAsStringAsync();
                    var log = JsonConvert.DeserializeObject<CompleteLog>(body);
                    if (log != null)
                    {
                        detailsTab.DataContext = log;
                    }
                }
                else
                {
                    detailsTab.DataContext = null;
                    MessageBox.Show("Unable to get data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
