using MahApps.Metro.Controls;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
using Taipei.Models;
using Taipei.Models.Log;
using Taipei.Windows.Views;
using Microsoft.Web.WebView2.Core;

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
            GetCookies(id, config, CookiesTab);
            GetHeaders(id, config, HeadersTab);
            GetContent(id, config, ContentTab);
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

        private static async void GetCookies(int id, LoginConfig? config, CookiesTab cookietab)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("taipei-auth", config?.Token);
                HttpResponseMessage res = await client.GetAsync($"{config?.RootUrl}/taipei/api/log/{id}/cookies");
                if (res.IsSuccessStatusCode)
                {
                    string body = await res.Content.ReadAsStringAsync();
                    var cookies = JsonConvert.DeserializeObject<List<CookieLog>>(body); // if im returning items and not item do <list<class>>
                    if (cookies != null)
                    {
                        cookietab.CookiesDatagrid.ItemsSource = cookies;
                    }
                }
                else
                {
                    cookietab.DataContext = null;
                    MessageBox.Show("Unable to get cookies", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static async void GetHeaders(int id, LoginConfig? config, HeadersTab headertab)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("taipei-auth", config?.Token);
                HttpResponseMessage res = await client.GetAsync($"{config?.RootUrl}/taipei/api/log/{id}/headers");
                if (res.IsSuccessStatusCode)
                {
                    string body = await res.Content.ReadAsStringAsync();
                    var headers = JsonConvert.DeserializeObject<List<HeaderLog>>(body);
                    if (headers != null)
                    {
                        headertab.HeadersGrid.ItemsSource = headers;
                    }
                }
                else
                {
                    headertab.DataContext = null;
                    MessageBox.Show("Unable to get headers", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static async void GetContent(int id, LoginConfig? config, ContentTab content)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("taipei-auth", config?.Token);
                HttpResponseMessage res = await client.GetAsync($"{config?.RootUrl}/taipei/api/log/{id}/content");
                if (res.IsSuccessStatusCode)
                {
                    string body = await res.Content.ReadAsStringAsync();
                    var fullTexts = JsonConvert.DeserializeObject<List<ContentLog>>(body);
                    if (fullTexts != null)
                    {
                        var requestEntry = fullTexts.FirstOrDefault(f => f.ContentType == "request");
                        content.HTMLReqBox.Text = requestEntry?.TextContent ?? string.Empty;

                        var resEntry = fullTexts.FirstOrDefault(f => f.ContentType == "response");
                        content.HTMLResBox.Text = resEntry?.TextContent ?? string.Empty;

                        await content.HtmlPreview.EnsureCoreWebView2Async(null);

                        var html = resEntry?.TextContent ?? "<h1>No Content</h1>";
                        if (Encoding.UTF8.GetByteCount(html) > 1_900_000)
                        {
                            html = "<h1>Content too large to preview</h1>";
                        }
                        content.ShowHtml(html);
                    }
                }
                else
                {
                    content.DataContext = null;
                    MessageBox.Show("Unable to get headers", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
