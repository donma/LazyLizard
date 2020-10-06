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

namespace LazyLizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SelfTrasationId { get; set; }

        public bool IsDebug { get; set; }

        public string Server2 = "https://client-s.gateway.messenger.live.com/v1/users/ME/";

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

            MessageBox.Show("版本為 0.4.201005 .", "版本資訊", MessageBoxButton.OK, MessageBoxImage.Information);
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

                    var dotSkype = new DotSkype.DotSkype();
                    var registerToken = dotSkype.GetRegisterToken(exchangeToken);

                    //foreach (var id in allSend)
                    //{
                    Parallel.ForEach(allSend, new ParallelOptions { MaxDegreeOfParallelism = 2 }, id =>
                     {
                         try
                         {
                             dotSkype.SendText(registerToken, message, id.Split('|')[0]);
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

                var dotSkype = new DotSkype.DotSkype();
                var registerToken = dotSkype.GetRegisterToken(exchangeToken);

                foreach (var id in allSend)
                {
                    var objectId = LogicHandler.SendMessage.CreateObject(exchangeToken, id.Split('|')[0], logPath);

                    dotSkype.UploadFileToObject(exchangeToken, objectId, imagePath, logPath, byteSource);

                    try
                    {
                        dotSkype.SendIImage(registerToken, objectId, id.Split('|')[0], fileInfo.Name, message);

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

            DotSkype.DotSkype dotSkype = new DotSkype.DotSkype();
            var loginToken = dotSkype.SendSoapLogin(txtAccount.Text, txtPass.Password);
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

            exchangeToken = dotSkype.ExchangeSkypeToken(loginToken);
            var userName = dotSkype.GetSkypeUserProfile(exchangeToken);

            var contact = dotSkype.GetSkypeUserContactInfoList(userName, exchangeToken);

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
