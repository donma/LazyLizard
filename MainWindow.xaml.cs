using LazyLizard.LogicHandler;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Xml.Linq;
using static LazyLizard.Model.SkypeModel;

namespace LazyLizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region DotSkype

        const string LiveHost = "https://login.live.com/";
        const string SkypeHost = "https://edge.skype.com/";
        const string ApiUser = "https://api.skype.com/";
        const string ContactHost = "https://contacts.skype.com/";
        const string MsgHost = "https://client-s.gateway.messenger.live.com/v1/";

        private string _skypeToken = "";
        private string _endPoint = "";

        public string BuildSopa(string account, string password)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<Envelope xmlns='http://schemas.xmlsoap.org/soap/envelope/'");
            stringBuilder.AppendLine("xmlns:wsse='http://schemas.xmlsoap.org/ws/2003/06/secext'");
            stringBuilder.AppendLine("xmlns:wsp='http://schemas.xmlsoap.org/ws/2002/12/policy'");
            stringBuilder.AppendLine("xmlns:wsa='http://schemas.xmlsoap.org/ws/2004/03/addressing'");
            stringBuilder.AppendLine("xmlns:wst='http://schemas.xmlsoap.org/ws/2004/04/trust'");
            stringBuilder.AppendLine("xmlns:ps='http://schemas.microsoft.com/Passport/SoapServices/PPCRL'>");
            stringBuilder.AppendLine("<Header>");
            stringBuilder.AppendLine("<wsse:Security>");
            stringBuilder.AppendLine("<wsse:UsernameToken Id='user'>");
            stringBuilder.AppendLine("<wsse:Username>" + account + "</wsse:Username>");
            stringBuilder.AppendLine("<wsse:Password>" + password + "</wsse:Password>");
            stringBuilder.AppendLine("</wsse:UsernameToken>");
            stringBuilder.AppendLine("</wsse:Security>");
            stringBuilder.AppendLine("</Header>");
            stringBuilder.AppendLine("<Body>");
            stringBuilder.AppendLine("<ps:RequestMultipleSecurityTokens Id='RSTS'>");
            stringBuilder.AppendLine("<wst:RequestSecurityToken Id='RST0'>");
            stringBuilder.AppendLine("<wst:RequestType>http://schemas.xmlsoap.org/ws/2004/04/security/trust/Issue</wst:RequestType>");
            stringBuilder.AppendLine("<wsp:AppliesTo>");
            stringBuilder.AppendLine("<wsa:EndpointReference>");
            stringBuilder.AppendLine("<wsa:Address>wl.skype.com</wsa:Address>");
            stringBuilder.AppendLine("</wsa:EndpointReference>");
            stringBuilder.AppendLine("</wsp:AppliesTo>");
            stringBuilder.AppendLine("<wsse:PolicyReference URI='MBI_SSL'></wsse:PolicyReference>");
            stringBuilder.AppendLine("</wst:RequestSecurityToken>");
            stringBuilder.AppendLine("</ps:RequestMultipleSecurityTokens>");
            stringBuilder.AppendLine("</Body>");
            stringBuilder.AppendLine("</Envelope>");

            return stringBuilder.ToString();
        }

        public string SendSoapLogin(string account, string password)
        {
            var request = WebRequest.Create(LiveHost + "RST.srf") as HttpWebRequest;
            byte[] bytes;
            bytes = System.Text.Encoding.UTF8.GetBytes(BuildSopa(account, password));
            request.ContentType = "application/xml;";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                //new XmlReader(response)
                var xml = XDocument.Load(response.GetResponseStream());
                XNamespace aw = "http://schemas.xmlsoap.org/ws/2003/06/secext";
                var columns = xml.Descendants(aw + "BinarySecurityToken");

                var token = columns.FirstOrDefault()?.Value;
                string responseStr = new StreamReader(responseStream).ReadToEnd();
                //token = responseStr.Replace("<wsse:BinarySecurityToken Id=\"Compact0\">", "|").Replace("</wsse:BinarySecurityToken>", "|").Split('|')[1];

                return token;
            }
            return null;
        }
        public string ExchangeSkypeToken(string token)
        {
            var request = WebRequest.Create(SkypeHost + "rps/v1/rps/skypetoken") as HttpWebRequest;



            NameValueCollection postParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
            postParams.Add("partner", "999");
            postParams.Add("access_token", token);
            postParams.Add("scopes", "client");


            byte[] bytes;
            bytes = Encoding.UTF8.GetBytes(postParams.ToString());
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();

                var Jdata = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(responseStr);
                _skypeToken = Jdata["skypetoken"].ToString();

                return _skypeToken;
            }
            return null;
        }

        public string GetRegisterToken()
        {
            var request = WebRequest.Create(MsgHost + "users/ME/endpoints") as HttpWebRequest;
            request.Method = "GET";
            request.Headers.Add("X-Skypetoken", _skypeToken);
            request.Headers.Add("Authentication", "skypetoken=" + _skypeToken);



            try
            {
                var response = request.GetResponse();

                if (!MsgHost.Contains(response.ResponseUri.Host))
                {
                    _endPoint = "https://" + response.ResponseUri.Host + "/v1/";
                }
                else
                {
                    _endPoint = MsgHost;
                }

                var registTokenSource = response.Headers["Set-RegistrationToken"].ToString();
                return registTokenSource;
            }
            catch (WebException webExcp)
            {
                return "";
            }
        }

        public string GetSkypeUserProfile()
        {
            HttpWebRequest httpWebRequest = WebRequest.Create(ApiUser + "users/self/profile") as HttpWebRequest;
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("X-Skypetoken", _skypeToken);

            using (WebResponse response = httpWebRequest.GetResponse())
            {
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string contentStringData = streamReader.ReadToEnd();

                var userProfile = JsonConvert.DeserializeObject<UserProfile>(contentStringData);
                streamReader.Close();


                if (userProfile.username.Contains("live:"))
                {
                    // userProfile.username = userProfile.username.Replace("live:","");
                }
                return userProfile.username;
            }
        }

        public LogicHandler.ContactInfo GetSkypeUserContactInfoList(string skypeId)
        {
            string requestUriStringUL = ContactHost + "contacts/v2/users/" + skypeId + "/contacts";
            HttpWebRequest request = WebRequest.Create(requestUriStringUL) as HttpWebRequest;
            request.Method = "Get";
            request.Headers.Add("X-Skypetoken", _skypeToken);
            request.ContentType = "application/json; charset=UTF-8";


            //byte[] bytes;
            //bytes = Encoding.UTF8.GetBytes("28:"+skypeId);
            //request.ContentLength = bytes.Length;
            //Stream requestStream = request.GetRequestStream();
            //requestStream.Write(bytes, 0, bytes.Length);
            //requestStream.Close();

            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string end = streamReader.ReadToEnd();
                streamReader.Close();
                return JsonConvert.DeserializeObject<LogicHandler.ContactInfo>(end);
            }
            return null;


            //var friends = new ContactInfo();

            //using (WebResponse response = request.GetResponse())
            //{
            //    StreamReader streamReader = new StreamReader(response.GetResponseStream());
            //    string end = streamReader.ReadToEnd();
            //    streamReader.Close();
            //    return JsonConvert.DeserializeObject<ContactInfo>(end);
            //}

        }

        public ConversactionThread GetSkypeConversactionList(string backLink = "")
        {
            try
            {
                var url = "";

                if (string.IsNullOrEmpty(backLink))
                {
                    url = MsgHost + "users/ME/conversations" + "?view=msnp24Equivalent&startTime=0&targetType=Thread";
                }
                else
                {
                    url = backLink;
                }

                HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
                httpWebRequest.Method = "GET";
                httpWebRequest.Headers.Add("X-Skypetoken", _skypeToken);
                httpWebRequest.Headers.Add("Authentication", "skypetoken=" + _skypeToken);

                using (WebResponse response = httpWebRequest.GetResponse())
                {
                    StreamReader streamReader = new StreamReader(response.GetResponseStream());
                    string contentStringData = streamReader.ReadToEnd();

                    var conversactionThread = JsonConvert.DeserializeObject<ConversactionThread>(contentStringData);
                    streamReader.Close();
                    return conversactionThread;
                }
            }
            catch (Exception ex)
            {

                return null;


            }

        }

        public List<ConversactionItem> conversactionItems = new List<ConversactionItem>();

        public void QueryThread(string backLink)
        {
            var threadList = GetSkypeConversactionList(backLink);
            if (threadList == null)
            {
                Thread.Sleep(1 * 60 * 1000);
                QueryThread(backLink);
            }
            else
            {
                if (threadList._metadata.backwardLink != "")
                {
                    conversactionItems.AddRange(threadList.conversations);
                    QueryThread(threadList._metadata.backwardLink);
                }
                else
                {
                    conversactionItems.AddRange(threadList.conversations);
                }
            }
        }

        public void UploadFileToObject(string imageId, string imageLocalFilePath, string logPath, byte[] imageSrcByte)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Host", "api.asm.skype.com");
                client.Headers.Add("Authorization", "skype_token " + _skypeToken);
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
                client.Headers.Add("Content-Type", "application");
                var res = client.UploadData("https://api.asm.skype.com/v1/objects/" + imageId + "/content/imgpsh", "PUT", imageSrcByte);

            }
        }

        public void SendMultipleText(string registrationToken, string message, List<string> ids, int reTryCount = 1)
        {
            int tryCount = 0;
            List<string> failIds = new List<string>();
            foreach (var item in ids)
            {
                try
                {
                    SendText(registrationToken, message, item);
                }
                catch (Exception)
                {
                    failIds.Add(item);
                }

            }

            if (failIds.Count > 0)
            {
                if (tryCount < reTryCount)
                {
                    SendMultipleText(registrationToken, message, failIds);
                }
                tryCount++;


            }

        }

        public void SendIImage(string registrationToken, string objectId, string userId, string imageName, string text)
        {

            var sourceTempalate = "<URIObject type=\"Picture.1\" uri=\"https://api.asm.skype.com/v1/objects/" + objectId + "\"" +
              " url_thumbnail=\"https://api.asm.skype.com/v1/objects/" + objectId + "/views/imgt1\"><Title /><Description />" +
              "<meta type=\"photo\" originalName=\"" + imageName + "\"/>" +
              "<OriginalName v=\"" + imageName + "\"/>" +
              "</URIObject>";


            //POST https://client-s.gateway.messenger.live.com/v1/users/ME/conversations/(string: id)/messages
            HttpWebRequest httpWebRequest = WebRequest.Create(MsgHost + "v1/users/ME/conversations/" + userId + "/messages") as HttpWebRequest;
            httpWebRequest.Method = "POST";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:35.0) Gecko/20100101 Firefox/35.0";
            httpWebRequest.Accept = "application/json, text/javascript";
            httpWebRequest.Headers.Add("Accept-Language", "zh-tw,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
            httpWebRequest.Headers.Add("ClientInfo", "os=Windows; osVer=8.1; proc=Win32; lcid=en-us; deviceType=1; country=n/a; clientName=skype.com; clientVer=908/1.9.0.232//skype.com");
            httpWebRequest.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            httpWebRequest.Headers.Add("Pragma", "no-cache");
            httpWebRequest.Headers.Add("Expires", "0");
            httpWebRequest.Headers.Add("BehaviorOverride", "redirectAs404");
            httpWebRequest.Headers.Add("RegistrationToken", "registrationToken=" + registrationToken);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Referer = "https://web.skype.com/zh-Hant/";
            httpWebRequest.Headers.Add("Origin", "https://web.skype.com");
            httpWebRequest.KeepAlive = true;
            var str = "";
            if (string.IsNullOrEmpty(text))
            {
                str = sourceTempalate;
            }
            else
            {
                str = sourceTempalate + "\r\n" + text;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object)new Dictionary<string, object>()
            {
            {
                "content",
                (object)str
            },
            {
                "messagetype",
                (object) "RichText/UriObject"
            },
            {
                "contenttype",
                (object)str
            },
            {
                "clientmessageid",
                (object) DateTime.Now.Ticks
            }
            }));
            using (Stream requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (httpWebRequest.GetResponse())
            {

            }
        }

        public void SendText(string registrationToken, string message, string id, bool tryAgain = false)
        {
            HttpWebResponse response = null;
            try
            {

                //
                //POST https://client-s.gateway.messenger.live.com/v1/users/ME/conversations/(string: id)/messages

                HttpWebRequest httpWebRequest = WebRequest.Create(_endPoint + "users/ME/conversations/" + id + "/messages") as HttpWebRequest;
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Expires", "0");
                httpWebRequest.Headers.Add("BehaviorOverride", "redirectAs404");
                httpWebRequest.Headers.Add("RegistrationToken", registrationToken);
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.KeepAlive = true;
                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object)new Dictionary<string, object>()
            {
            {
                "content",
                (object)message
            },
            {
                "messagetype",
                (object) "Text"
            },
            {
                "contenttype",
                (object)"text"
            },
            {
                "clientmessageid",
                (object) DateTime.Now.Ticks
            }
            }));

                using (Stream requestStream = httpWebRequest.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }


                response = (HttpWebResponse)httpWebRequest.GetResponse();

                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string contentStringData = streamReader.ReadToEnd();


            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    response = (HttpWebResponse)e.Response;
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        if (!tryAgain)
                        {
                            GetRegisterToken();
                            SendText(registrationToken, message, id, true);
                        }
                    }
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }


        #endregion


        public string SelfTrasationId { get; set; }
        public bool IsDebug { get; set; }

        public void CheckDebug()
        {
            if (IsDebug)
            {

                btnLoadContact.Visibility = Visibility.Visible;
            }
            else
            {

                btnLoadContact.Visibility = Visibility.Hidden;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            IsDebug = true;
            CheckDebug();

            MessageBox.Show("版本為 0.4.201006", "版本資訊", MessageBoxButton.OK, MessageBoxImage.Information);
            //Reference :
            //https://skpy.t.allofti.me/
            //http://wayneprogramcity.blogspot.com/2016/05/skype-apicskype.html
            //https://github.com/Mrmaxmeier/web_skype4py/blob/master/main.py/
            //https://github.com/msndevs/protocol-docs/wiki/Messaging
        }

        private void btnGetImagePath_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            if (!string.IsNullOrEmpty(openFileDialog.FileName))
            {
                imageUploadPreview.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                txtImageFilePath.Text = openFileDialog.FileName;
            }

        }

        private void AddLog(string message)
        {

            txtLog.Dispatcher.BeginInvoke(new Action(() =>
            {
                txtLog.Text = DateTime.Now.ToString("HH:mm:ss.ff") + ": " + message + "\r\n\r\n" + txtLog.Text;
            }));
        }

        private void btnSent_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtMessage.Text.Trim()) && string.IsNullOrEmpty(txtImageFilePath.Text.Trim()))
            {
                MessageBox.Show("You hava nothing to send.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var logPath = AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + System.IO.Path.DirectorySeparatorChar;


            #region STEP8 

            if (string.IsNullOrEmpty(txtImageFilePath.Text.Trim()))
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    AddLog("STEP8- 廣播文字..");
                }));
            }

            try
            {
                if (txtMessage.Text.Trim().Length > 0 && txtImageFilePath.Text.Trim().Length <= 0)
                {
                    var userCount = 0;
                    var message = txtMessage.Text;
                    List<string> allSend = new List<string>();
                    btnSent.IsEnabled = false;
                    MessageBox.Show("有點耐心等待，好了我會跟你說");

                    foreach (CheckBox c in listContact.Items)
                    {
                        if (c.IsChecked.Value)
                        {
                            allSend.Add(c.Tag.ToString() + "|" + c.Content.ToString());
                        }
                    }

                    var registerToken = GetRegisterToken();

                    //foreach (var id in allSend)
                    //{
                    Parallel.ForEach(allSend, new ParallelOptions { MaxDegreeOfParallelism = 2 }, id =>
                     {
                         try
                         {
                             SendText(registerToken, message, id.Split('|')[0]);
                             userCount++;
                             AddLog("已寄送文字給" + id.Split('|')[1] + " ,SUCCESS");
                         }
                         catch (Exception ex)
                         {
                             Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                             {
                                 AddLog("ERROR 已寄送文字給" + id.Split('|')[1] + " , 失敗，請手動寄送" + ex.Message);

                             }));
                             // continue;
                         }
                         // });

                         //}
                     });

                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        //     AddLog("STEP8 寄送成功  !!");
                        AddLog("STEP8 寄送 " + userCount + " 筆");
                    }));

                    MessageBox.Show("Success");
                    btnSent.IsEnabled = true;
                }
                else
                {
                    // AddLog("STEP8 無文字所以跳過不廣播了  !!");
                }
            }
            catch (Exception ex)
            {
                new Thread(() =>
                {
                    AddLog("STEP8 出錯了:" + ex.Message + "," + ex.StackTrace);
                }).Start();

                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);


                MessageBox.Show("STEP8 發生錯誤，進行中止");
                return;
            }


            #endregion


            #region STEP9


            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                AddLog("STEP9- 廣播圖片..");
            }));



            if (txtImageFilePath.Text.Trim().Length > 0)
            {

                var userCount = 0;
                var byteSource = File.ReadAllBytes(txtImageFilePath.Text.Trim());
                var fileInfo = new FileInfo(txtImageFilePath.Text.Trim());
                var message = txtMessage.Text.Trim();
                var imagePath = txtImageFilePath.Text.Trim();
                List<string> allSend = new List<string>();


                foreach (CheckBox c in listContact.Items)
                {
                    if (c.IsChecked.Value)
                    {
                        allSend.Add(c.Tag.ToString() + "|" + c.Content.ToString());
                        //MessageBox.Show(c.Tag.ToString());
                    }

                }

                this.btnSent.IsEnabled = false;

                MessageBox.Show("需要點時間，請耐心等待，好了我會跟你說 ");

                var registerToken = GetRegisterToken();

                foreach (var id in allSend)
                {
                    var objectId = LogicHandler.SendMessage.CreateObject(exchangeToken, id.Split('|')[0], logPath);

                    UploadFileToObject(objectId, imagePath, logPath, byteSource);

                    try
                    {
                        SendIImage(registerToken, objectId, id.Split('|')[0], fileInfo.Name, message);

                        userCount++;
                        //AddLog("已寄送圖片給" + id.Split('|')[1]);
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                        {
                            AddLog("已寄送圖片給" + id.Split('|')[1] + ", " + userCount + "/" + allSend.Count());

                        }));
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                        {
                            AddLog("ERROR 已寄送圖片給" + id.Split('|')[1] + " , 失敗，請手動寄送" + ex.Message);

                        }));
                    }
                }

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    AddLog("STEP9 發送完成  !!");
                }));

                this.btnSent.IsEnabled = true;
                MessageBox.Show("Success");
            }
            else
            {
                AddLog("STEP9 無圖片所以跳過不廣播了  !!");
            }

            #endregion
        }

        private void Window_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void txtAccount_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (txtAccount.Text.ToLower() == "imdm")
            {
                IsDebug = true;
                CheckDebug();
            }
        }


        string exchangeToken = "";

        private void btnLoadContact_Click(object sender, RoutedEventArgs e)
        {
            btnLoadContact.IsEnabled = false;

            listContact.Items.Clear();
            //Check Parameter First.
            if (string.IsNullOrEmpty(txtAccount.Text))
            {
                btnLoadContact.IsEnabled = true;
                MessageBox.Show("Account is Empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }
            if (string.IsNullOrEmpty(txtPass.Password))
            {
                btnLoadContact.IsEnabled = true;
                MessageBox.Show("Password is Empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var loginToken = SendSoapLogin(txtAccount.Text, txtPass.Password);
            if (loginToken == null) {
                btnLoadContact.IsEnabled = true;
                MessageBox.Show("登入失敗: 帳號密碼錯誤或未關閉2FA雙因素驗證;");
                return;
            }

            if (loginToken == "HttpStatusNotOK") {
                btnLoadContact.IsEnabled = true;
                MessageBox.Show("Skype伺服器發生異常: 請聯絡系統開發人員;");
                return;
            }

            exchangeToken = ExchangeSkypeToken(loginToken);
            var userName = GetSkypeUserProfile();

            var contact = GetSkypeUserContactInfoList(userName);

            foreach (var f in contact.contacts)
            {
                if (!f.blocked)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        var c = new CheckBox();
                        c.Content = f.display_name + " | " + f.person_id;
                        c.Tag = f.person_id;

                        listContact.Items.Add(c);
                    }));
                }
            }
            MessageBox.Show("載入  " + contact.contacts.Count() + " 筆聯絡人，不包含封鎖資料 !");

            this.btnSent.IsEnabled = true;
            btnLoadContact.IsEnabled = false;
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in listContact.Items)
            {

                var c = (item as CheckBox);
                c.IsChecked = true;

            }
        }

        private void btnUnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in listContact.Items)
            {

                var c = (item as CheckBox);
                c.IsChecked = false;

            }
        }

        private void btnSaveContact_Click(object sender, RoutedEventArgs e)
        {
            var res = new List<string>();
            foreach (CheckBox item in listContact.Items)
            {
                if (item.IsChecked.Value)
                {
                    res.Add(item.Tag.ToString());
                }

            }

            var str = JsonConvert.SerializeObject(res);
            str = EncryptUtil.EncAesString(str, "LazyLizard_LazyLizard_LazyLizard");

            SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "名單"; // Default file name
            dlg.DefaultExt = ".lazylizard"; // Default file extension
            dlg.Filter = "LazyLizard documents (.lazylizard)|*.lazylizard"; // Filter files by extension

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {

                string filename = dlg.FileName;

                File.WriteAllText(filename, str);

                MessageBox.Show("Save Success !!");
            }

        }

        private void btnReadContact_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".lazylizard"; // Default file extension
            dlg.Filter = "LazyLizard documents (.lazylizard)|*.lazylizard"; // Filter files by extension

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                try
                {
                    string filename = dlg.FileName;
                    var str = File.ReadAllText(filename);

                    var data = JsonConvert.DeserializeObject<List<string>>(EncryptUtil.DescAesString(str, "LazyLizard_LazyLizard_LazyLizard"));

                    foreach (CheckBox item in listContact.Items)
                    {
                        if (data.Any(x => x == item.Tag.ToString()))
                        {
                            item.IsChecked = true;
                        }
                        else
                        {
                            item.IsChecked = false;
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("發生錯誤:" + ex.Message);
                }
            }
        }

        private void btnClearImage_Click(object sender, RoutedEventArgs e)
        {
            imageUploadPreview.Source = null;
            txtImageFilePath.Text = "";
        }
    }
}
