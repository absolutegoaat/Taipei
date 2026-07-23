using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Taipei.Utils;

namespace Taipei.Views
{
    public partial class DataTab : UserControl, INotifyPropertyChanged
    {
        private string? _totalRequests;
        public string? TotalRequests
        {
            get => _totalRequests;
            set { _totalRequests = value; OnPropertyChanged(); }
        }

        private string? _totalCookies;
        public string? TotalCookies
        {
            get => _totalCookies;
            set { _totalCookies = value; OnPropertyChanged(); }
        }

        private string? _total2xx;
        public string? Total2xx
        {
            get => _total2xx;
            set { _total2xx = value; OnPropertyChanged(); }
        }

        private string? _total4xx;
        public string? Total4xx
        {
            get => _total4xx;
            set { _total4xx = value; OnPropertyChanged(); }
        }

        private string? _total5xx;
        public string? Total5xx
        {
            get => _total5xx;
            set { _total5xx = value; OnPropertyChanged(); }
        }

        private class TotalData
        {
            public string? TotalRequests { get; set; }
            public string? TotalCookies { get; set; }
            public string? Total2xx { get; set; }
            public string? Total4xx { get; set; }
            public string? Total5xx { get; set; }
        }

        public DataTab()
        {
            InitializeComponent();
            DataContext = this;
            _ = GetData();
        }

        public async Task GetData()
        {
            try
            {
                LoginConfig? config = Taipei.Utils.Utils.InitConfig();
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("taipei-auth", config?.Token);
                HttpResponseMessage res = await client.GetAsync($"{config?.RootUrl}/taipei/api/totals");

                if (res.IsSuccessStatusCode)
                {
                    string body = await res.Content.ReadAsStringAsync();
                    var totals = JsonConvert.DeserializeObject<TotalData>(body);

                    if (totals != null)
                    {
                        TotalRequests = totals.TotalRequests;
                        TotalCookies = totals.TotalCookies;
                        Total2xx = totals.Total2xx;
                        Total4xx = totals.Total4xx;
                        Total5xx = totals.Total5xx;
                    }
                }
                else
                {
                    MessageBox.Show("Failed to get data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}