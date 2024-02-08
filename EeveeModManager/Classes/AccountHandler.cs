using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;

using Newtonsoft.Json;

using System.Windows;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using EeveexModManager.Classes.NexusClasses.API;

namespace EeveexModManager.Classes
{
    public class AccountHandler
    {
        private Action<string> Callback = null;
        public string Token { get; protected set; }
        public string Username { get; protected set; }
        public bool IsLoggedIn { get; protected set; }

        public AccountHandler(Action<string> callback)
        {
            Callback = callback;
        }
        public void Init()
        {
            if (!File.Exists(Defined.Settings.ApplicationDataPath + "\\token"))
            {
                LogIn("Would you like to log in to Nexus?");
            }
            else
            {
                using (StreamReader r = new StreamReader(Defined.Settings.ApplicationDataPath + "\\token"))
                {
                    string key = r.ReadToEndAsync().GetAwaiter().GetResult();

                    if (ValidateKey(key))
                    {
                        Token = key;
                        Callback(Username);
                    }
                    else
                    {
                        LogIn("Error! The Token saved is invalid, would you like to log in to Nexus?");
                    }
                }
            }
        }
        
        private void LogIn(string msg)
        {
            if (MessageBox.Show(msg, "Nexus Login",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.No)
            {
                TryLogin().GetAwaiter().GetResult();
            }
        }

        //for testing purpose only, accept any dodgy certificate... 
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private bool ValidateKey(string key)
        {
            int count = 0;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Defined.NEXUSAPI_BASE + "/users/validate");
            request.Headers.Add("APIKEY", key);
            request.Credentials = CredentialCache.DefaultCredentials;

            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateServerCertificate);
            tryagain:
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string output = reader.ReadToEndAsync().GetAwaiter().GetResult();
                try
                {
                    var user_info = JsonConvert.DeserializeObject<Api_UserInfo>(output);
                    Username = user_info.name;
                    Token = user_info.key;
                    IsLoggedIn = true;
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                count++;
                if(count < 5)
                    goto tryagain;
                else
                {
                    LogIn("Error! It seems that the Nexus servers return 503 [Unavailable], would you like to log in again to Nexus?");
                    return false;
                }
            }
        }

        public async Task TryLogin()
        {
            string randomId = new Random().Next(0, 80000000).ToString();

            string json = JsonConvert.SerializeObject(new Api_Authorization { appid = "Vortex", id = randomId });

            using (ClientWebSocket ws = new ClientWebSocket())
            {
                Uri serverUri = new Uri("wss://sso.nexusmods.com:8443/");
                await ws.ConnectAsync(serverUri, CancellationToken.None);
                if(ws.State == WebSocketState.Open)
                {
                    var encoded = Encoding.UTF8.GetBytes(json);
                    var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
                    await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    try
                    {
                        System.Diagnostics.Process.Start($"www.nexusmods.com/sso?id={randomId}");
                        buffer = new ArraySegment<byte>(new byte[1024]);
                        WebSocketReceiveResult result = (await ws.ReceiveAsync(buffer, CancellationToken.None));
                        string key = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "nuthin", CancellationToken.None);

                        if (key != null)
                        {
                            key = key.Replace("\"", string.Empty);
                            if (ValidateKey(key))
                            {
                                using (StreamWriter w = new StreamWriter(Defined.Settings.ApplicationDataPath + "\\token"))
                                {
                                    await w.WriteAsync(key);
                                }
                                Callback(Username);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error : No internet connection!");
                    }
                }
            }
        }
        public void TryLogout()
        {
            IsLoggedIn = false;
        }
    }
}
