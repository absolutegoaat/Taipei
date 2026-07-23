using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Taipei.Utils;
using Taipei.Models;
using Taipei.Windows;

namespace Taipei.Views
{
    public partial class DataTable : UserControl
    {
        public List<LogEntry> Logs { get; set; } = new();

        private int _offset = 0;
        private const int _limit = 50;
        private int _total = 0;
        private LoginConfig? _config;

        public DataTable()
        {
            InitializeComponent();
            DataContext = this;
            _config = Utils.Utils.InitConfig();
            _ = GetLogs(LogsDataGrid, _config, _offset);
        }

        public async Task GetLogs(DataGrid grid, LoginConfig? config, int offset)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("taipei-auth", config?.Token);

                HttpResponseMessage res = await client.GetAsync(
                    $"{config?.RootUrl}/taipei/get_logs?limit={_limit}&offset={offset}");

                if (res.IsSuccessStatusCode)
                {
                    string body = await res.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<PagedLogResponse>(body);

                    if (response != null)
                    {
                        Logs = response.Results;
                        grid.ItemsSource = Logs;
                        _offset = response.Offset;
                        _total = response.Total;
                        UpdateNavButtons();
                    }
                }
                else
                {
                    MessageBox.Show("Failed to fetch data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateNavButtons()
        {
            PrevPageButton.IsEnabled = _offset > 0;
            NextPageButton.IsEnabled = _offset + _limit < _total;

            int shownFrom = _total == 0 ? 0 : _offset + 1;
            int shownTo = Math.Min(_offset + _limit, _total);
            PageLabel.Text = $"Showing {shownFrom}–{shownTo} of {_total}";
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            _offset += _limit;
            await GetLogs(LogsDataGrid, _config, _offset);
        }

        private async void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            _offset = Math.Max(0, _offset - _limit);
            await GetLogs(LogsDataGrid, _config, _offset);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _config = Utils.Utils.InitConfig();
            _ = GetLogs(LogsDataGrid, _config, 0);
        }

        private void LogsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LogsDataGrid.SelectedItem != null)
            {
                var selectedData = LogsDataGrid.SelectedItem as LogEntry;

                if (selectedData != null)
                {
                    /*
                    MessageBox.Show($"Time: {selectedData.Timestamp}\n" +
                                    $"Level: {selectedData.Client_Ip}\n" +
                                    $"Message: {selectedData.Id}");
                    */
                }
            }
        }
    }
}