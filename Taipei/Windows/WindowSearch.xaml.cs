using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
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
using Taipei.Utils;
using Newtonsoft.Json;
using Taipei.Models;

namespace Taipei.Windows
{
    /// <summary>
    /// Interaction logic for WindowSearch.xaml
    /// </summary>
    public partial class WindowSearch : MetroWindow
    {
        private string _searchByField = "IP";

        public WindowSearch()
        {
            InitializeComponent();
        }

        private void SearchByOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string tag)
            {
                _searchByField = tag;
                SearchByButton.Content = $"Search By: {tag}"; 
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e) => RunSearch();

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) RunSearch();
        }

        private async void RunSearch()
        {
            LoginConfig? config = Utils.Utils.InitConfig();
            string query = SearchBox.Text;
            string filterField = _searchByField.ToLower();

            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Please enter a query", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //MessageBox.Show(query + "\n" + filterField);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("taipei-auth", config?.Token);
            HttpResponseMessage res = await client.GetAsync($"{config?.RootUrl}/taipei/api/search?query={Uri.EscapeDataString(query)}&filter={Uri.EscapeDataString(filterField)}");

            if (res.IsSuccessStatusCode)
            {
                string body = await res.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<List<SearchModel>>(body) ?? new List<SearchModel>();

                if (json.Count == 0)
                {
                    MessageBox.Show("Data returned nothing.");
                    return;
                }

                SearchedData.ItemsSource = json;
            }
            else
            {
                SearchedData.DataContext = null;
                MessageBox.Show("Failed to fetch search data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void SearchedData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SearchedData.SelectedItem != null)
            {
                var selectedItem = SearchedData.SelectedItem as SearchModel;

                if (selectedItem != null)
                {
                    WindowViewLog detailWindow = new WindowViewLog(selectedItem.Id);
                    detailWindow.ShowDialog();
                }
            }
        }
    }
}
