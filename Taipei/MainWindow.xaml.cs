using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Net.Http;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

#pragma warning disable CS1998
namespace Taipei
{
    public class LoginConfig
    {
        public string? Token { get; set; }
        public string? RootUrl { get; set; }
    }

    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Login_ClickAsync(object sender, RoutedEventArgs e)
        {
            /*
#if DEBUG
            this.Close();
            open like the dash
#endif
            */

            string exeDir = AppContext.BaseDirectory;
            string configPath = Path.Combine(exeDir, "config.json");

            if (File.Exists(configPath))
            {
                string json = File.ReadAllText("config.json");
                try
                {
                    LoginConfig? config = JsonConvert.DeserializeObject<LoginConfig>(json);

                    try
                    {
                        using var client = new HttpClient();
                        client.DefaultRequestHeaders.Add("taipei-auth", config?.Token);
                        HttpResponseMessage res = await client.GetAsync($"{config?.RootUrl}/validate");

                        if (res.IsSuccessStatusCode)
                        {
                            var dash = new Board();

                            this.Close();
                            dash.Show();
                        } 
                        else
                        {
                            string body = await res.Content.ReadAsStringAsync();

                            var dialog = new TaskDialog
                            {
                                WindowTitle = "Error",
                                MainInstruction = "Failed to connect to root url. Please go to config.json and make sure that is the correct url and correct token.",
                                Content = $"{body}",
                                MainIcon = TaskDialogIcon.Error,
                                ButtonStyle = TaskDialogButtonStyle.Standard
                            };

                            var okButton = new TaskDialogButton(ButtonType.Ok);
                            dialog.Buttons.Add(okButton);

                            TaskDialogButton result = dialog.ShowDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        var dialog = new TaskDialog
                        {
                            WindowTitle = "Error",
                            MainInstruction = "Error has occurred.",
                            Content = $"{ex.Message}",
                            MainIcon = TaskDialogIcon.Error,
                            ButtonStyle = TaskDialogButtonStyle.Standard
                        };

                        var okButton = new TaskDialogButton(ButtonType.Ok);
                        dialog.Buttons.Add(okButton);

                        TaskDialogButton di = dialog.ShowDialog();
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show($"Error Occurred while looking at config file:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                var loginSettings = new LoginDialogSettings
                {
                    InitialUsername = "",
                    AffirmativeButtonText = "Connect",
                    PasswordWatermark = "Token",
                    UsernameWatermark = "Root URL"
                };

                LoginDialogData result = await this.ShowLoginAsync(
                    "Login required",
                    "Enter token and root url.",
                    loginSettings
                );

                if (result == null) return;

                string token = result.Password;
                string rootUrl = result.Username;

                var controller = await this.ShowProgressAsync("Please wait", "Working on it...");
                controller.SetIndeterminate();

                try
                {
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("taipei-auth", token);

                    HttpResponseMessage res = await client.GetAsync(rootUrl + "/validate");
                    res.EnsureSuccessStatusCode();

                    string body = await res.Content.ReadAsStringAsync();

                    var config = new LoginConfig { Token = token, RootUrl = rootUrl };
                    string folder = AppContext.BaseDirectory;

                    Directory.CreateDirectory(folder);
                    string filePath = Path.Combine(folder, "config.json");
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(config, Formatting.Indented));

                    this.Close();
                    var dash = new Board();

                    dash.Show();
                }
                catch (Exception ex)
                {
                    await controller.CloseAsync();
                    await this.ShowMessageAsync("Login failed", $"Could not validate credentials: {ex.Message}");
                    return;
                }

                await controller.CloseAsync();
            }
        }
    }
}