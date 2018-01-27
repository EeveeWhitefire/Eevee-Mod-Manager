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
using System.Net.Http;
using System.Net.Http.Headers;

using EeveexModManager.Classes;
using EeveexModManager.Classes.JsonClasses;

using Newtonsoft.Json;
using System.IO;

namespace EeveexModManager.Windows
{
    /// <summary>
    /// Interaction logic for AccountLoginWindow.xaml
    /// </summary>
    public partial class AccountLoginWindow : Window
    {
        Action<string, Json_AccountInfo> WhenLogsIn;

        public AccountLoginWindow(  Action<string, Json_AccountInfo> actionIn)
        {
            InitializeComponent();

            WhenLogsIn = actionIn;
        }

        private void TryLogin(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text == string.Empty)
            {
                MessageBox.Show("Username cannot be empty!");
            }
            else if (PasswordTextBox.Password == string.Empty)
            {
                MessageBox.Show("Password cannot be empty!");
            }
            else
            {
                TryToLogIn(UsernameTextBox.Text, PasswordTextBox.Password, UpdateModManager);
            }
        }

        void UpdateModManager(string CookieSid, Json_AccountInfo u)
        {
            WhenLogsIn(CookieSid, u);
            Close();
        }

        public static void TryToLogIn(string username, string password, Action<string, Json_AccountInfo> WhenLoggedIn)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string API_LoginSource = $@"http://nmm.nexusmods.com/Sessions/?Login&username={username}&password={password}";

                httpClient.BaseAddress = new Uri("http://localhost:9000/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Nexus Client v0.63.14");

                string CookieSid = ((httpClient.GetAsync(API_LoginSource).GetAwaiter().GetResult()).Content.ReadAsStringAsync())
                    .GetAwaiter().GetResult();

                if (CookieSid != null)
                {
                    CookieSid = CookieSid.Replace("\"", string.Empty);

                    Json_AccountInfo accInfo = new Json_AccountInfo()
                    {
                        Username = username,
                        Password = password
                    };

                    string raw = JsonConvert.SerializeObject(accInfo);
                    var d = Environment.UserName;

                    File.WriteAllText("UserCredentials", Cryptographer.Encrypt(raw));

                    WhenLoggedIn(CookieSid, accInfo);
                }
                else
                {
                    MessageBox.Show("Error : Wrong username or password. Please try again!");
                }

            }
        }

        private void ShowPasswordCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if(ShowPassword.IsChecked.Value)
            {
                VisiblePasswordTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                VisiblePasswordTextBox.Visibility = Visibility.Hidden;
            }
        }

        private void VisiblePasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordTextBox.Password = VisiblePasswordTextBox.Text;
        }

        private void PasswordBox_TextChanged(object sender, RoutedEventArgs e)
        {
            VisiblePasswordTextBox.Text = PasswordTextBox.Password;
        }
    }
}
