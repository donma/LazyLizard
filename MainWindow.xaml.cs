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


        Tuple<string, string, string, string, Dictionary<string, string>> step5Result = null;
        Tuple<string, string, string> step6Result = null;

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

            MessageBox.Show("版本為 0.4.200922 .", "版本資訊", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void WriteSourceLog(string content, string path, string name)
        {

            new Thread(() =>
            {
                File.WriteAllText(path + name, content);
            }).Start();

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


                    //foreach (var id in allSend)
                    //{
                    Parallel.ForEach(allSend, new ParallelOptions { MaxDegreeOfParallelism = 2 }, id =>
                     {
                         try
                         {

                             LogicHandler.SendMessage.SendText(step6Result.Item1, message, id.Split('|')[0],Server2);

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


                foreach (var id in allSend)
                {

                    var objectId = LogicHandler.SendMessage.CreateObject(step5Result.Item4, step5Result.Item1, logPath);

                    LogicHandler.SendMessage.UploadFileToObject(step5Result.Item4, objectId, imagePath, logPath, byteSource);

                    try
                    {
                        LogicHandler.SendMessage.SendIImage(step6Result.Item1, objectId, id.Split('|')[0], fileInfo.Name, message, Server2);

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


            //Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            //        {
            //            //  AddLog("STEP9 寄送圖片成功  !!");
            //            AddLog("STEP9 寄送 " + userCount + " 筆");
            //        }));

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

        private void btnLoadContact_Click(object sender, RoutedEventArgs e)
        {
            btnLoadContact.IsEnabled = false;
            var PPFT = "";
            var MSPRequ = "";
            var MSPOK = "";

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


            #region INIT
            //Init Log Environment.

            if (!string.IsNullOrEmpty(SelfTrasationId) && !string.IsNullOrEmpty(txtLog.Text))
            {
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);
            }

            SelfTrasationId = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 10);
            System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "log");
            System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId);
            var logPath = AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + System.IO.Path.DirectorySeparatorChar;
            txtLog.Text = "";

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                AddLog("TrasationId is " + SelfTrasationId);
            }));

            #endregion


            #region STEP1
            //STEP1 - Get PPFT,MSPRequ,MSPOK
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                AddLog("STEP1- 開始獲取 PPFT , MSPRequ , MSPOK");
            }));


            var timstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "";
            var cookies = new Dictionary<string, string>();

            try
            {
                var tmpStr = LogicHandler.Utility.GetUrlContentAndFetchCookieValue("https://login.skype.com/login/oauth/microsoft?" +
                    "client_id=572381&" +
                    "partner=999&" +
                    "redirect_uri=https://web.skype.com/Auth/PostHandler&state=8a515ab4-8a78-41f6-a093-6fce99afb76d", "MSPRequ,MSPOK".Split(','),
                    ref cookies);


                WriteSourceLog(tmpStr, logPath, "STEP1");
                PPFT = LogicHandler.Utility.GetHTMLHiddenValueByName(tmpStr, "PPFT");
                MSPRequ = cookies["MSPRequ"];
                MSPOK = cookies["MSPOK"];

                new Thread(() =>
                {
                    AddLog("MSPRequ:" + MSPRequ);
                    AddLog("PPFT:" + PPFT);
                    AddLog("MSPOK:" + MSPOK);
                }).Start();


            }
            catch (Exception ex)
            {
                new Thread(() =>
                {
                    txtLog.Dispatcher.Invoke((Action)(() =>
                    {
                        txtLog.Text = DateTime.Now.ToString("HH:mm:ss.ff") + ": " + "STEP1 出錯了:" + ex.Message + "," + ex.StackTrace + "\r\n\r\n" + txtLog.Text;
                    }));
                }).Start();

                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);


                MessageBox.Show("STEP1 發生錯誤，進行中止");
                return;
            }

            #endregion


            #region STEP2
            //STEP2 - 1st. Login To Live 

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                AddLog("STEP2- 開始執行都入 MS Live , 第一道穿越..");
            }));

            Tuple<string, Dictionary<string, string>> step2Result = null;

            try
            {

                step2Result = LogicHandler.FirstLogin.GetUrlPostAndCookies(txtAccount.Text, txtPass.Password, MSPRequ, MSPOK, PPFT, logPath, timstamp);


                if (!string.IsNullOrEmpty(step2Result.Item1))
                {

                    WriteSourceLog(step2Result.Item1, logPath, "STEP2");
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        AddLog("urlPost:" + step2Result.Item1);
                        AddLog("STEP2 第一道穿越成功了 ，得到 STEP3 進入門口 !!");
                    }));

                }
                else
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        AddLog("STEP2 第一道穿越失敗，沒有拿到 urlPost");
                    }));

                    MessageBox.Show("STEP2 發生錯誤，進行中止:沒有拿到 urlPost");
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);
                    return;

                }

            }
            catch (Exception ex)
            {
                new Thread(() =>
                {
                    AddLog("STEP2 出錯了:" + ex.Message + "," + ex.StackTrace);
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);
                }).Start();

                MessageBox.Show("STEP2 發生錯誤，進行中止");
                return;
            }


            #endregion


            #region STEP3
            Tuple<string, string, string, string, string, string> step3Result = null;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                AddLog("STEP3 開始執行入 MS Live , 第二道穿越..");
            }));

            try
            {
                step3Result = LogicHandler.SecondLogin.GetUrlPostAndCookies(step2Result.Item1, MSPRequ, MSPOK, step2Result.Item2, PPFT, txtAccount.Text, txtPass.Password, logPath, timstamp);

                if (!string.IsNullOrEmpty(step3Result.Item5))
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        AddLog("Next Path :" + step2Result.Item1);
                        AddLog("STEP3 第二道穿越成功了  !!");
                    }));
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        AddLog("STEP3 穿越失敗，沒有拿到 參數; 原因:"+ step3Result.Item6);
                    }));

                    MessageBox.Show("STEP3 發生錯誤，進行中止; "+ step3Result.Item6);
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);
                    return;

                }

            }
            catch (Exception ex)
            {
                new Thread(() =>
                {
                    AddLog("STEP3 出錯了:" + ex.Message + "," + ex.StackTrace);
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);
                }).Start();

                MessageBox.Show("STEP3 發生錯誤，進行中止");
                return;
            }

            #endregion


            #region STEP4

            Tuple<string, Dictionary<string, string>> step4Result = null;

            new Thread(() =>
            {
                AddLog("STEP4- 跳轉Skype進入登入流程..");
            }).Start();

            try
            {
                step4Result = LogicHandler.RedirectSkype1.GetRedirectFormUrl(step3Result.Item5, step3Result.Item1, step3Result.Item2, step3Result.Item3, step3Result.Item4, logPath);

                if (!string.IsNullOrEmpty(step4Result.Item1))
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        AddLog("STEP4 Success =>Next Path :" + step4Result.Item1);
                        AddLog("STEP4 跳轉Skype進入登入流程成功  !!");
                    }));

                }
                else
                {

                    AddLog("STEP4 跳轉失敗");
                    MessageBox.Show("STEP4 發生錯誤，進行中止:跳轉失敗");
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);
                    return;

                }

            }
            catch (Exception ex)
            {
                new Thread(() =>
                {
                    AddLog("STEP4 出錯了:" + ex.Message + "," + ex.StackTrace);
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);
                }).Start();

                MessageBox.Show("STEP4 發生錯誤，進行中止");
                return;
            }



            #endregion


            #region STEP5

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                AddLog("STEP5- 開始取的 skypeId and token..");
            }));


            try
            {
                step5Result = LogicHandler.RedirectSkype2.GetRedirectFormUrlGetToken(step4Result.Item1, step3Result.Item1, logPath);

                if (!string.IsNullOrEmpty(step4Result.Item1))
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        AddLog("STEP5 Success =>skypeid :" + step5Result.Item1 + "," + "skype_token:" + step5Result.Item4);
                        AddLog("STEP5 取得skypetoken 成功  !!");
                    }));

                }
                else
                {

                    AddLog("STEP5 取得skypetoken 失敗");
                    MessageBox.Show("STEP5 發生錯誤，進行中止:跳轉失敗");
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);
                    return;

                }

            }
            catch (Exception ex)
            {
                new Thread(() =>
                {
                    AddLog("STEP5 出錯了:" + ex.Message + "," + ex.StackTrace);
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);
                }).Start();

                MessageBox.Show("STEP5 發生錯誤，進行中止");
                return;
            }



            #endregion


            #region STEP6

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                AddLog("STEP6- 開始取的 registrationToken..");
            }));

            try
            {
                step6Result = LogicHandler.FetchRegistToken.GetRegistToken(step5Result.Item4, logPath);
                Server2 = step6Result.Item3;
                //if (step6Result.Item1 == "" && step6Result.Item2 != "")
                //{
                //    var c = step6Result.Item2;
                //    Server2 = c.Replace("endpoints", "");
                //    step6Result = LogicHandler.FetchRegistToken.GetRegistToken(step5Result.Item4, logPath, c);
                //}

                if (!string.IsNullOrEmpty(step6Result.Item1))
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        AddLog("STEP6 Success =>registrationToken :" + step6Result.Item1 + "," + "endpointId:" + step6Result.Item2);
                        AddLog("STEP6 registrationToken 成功  !!");
                    }));

                }
                else
                {

                    AddLog("STEP6 取得 registrationToken 失敗");
                    MessageBox.Show("STEP6 發生錯誤，進行中止");
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);
                    return;

                }

            }
            catch (Exception ex)
            {
                new Thread(() =>
                {
                    AddLog("STEP6 出錯了:" + ex.Message + "," + ex.StackTrace);
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);
                }).Start();

                MessageBox.Show("STEP6 發生錯誤，進行中止");
                return;
            }


            #endregion


            #region STEP7
            ContactInfo step7Result = null;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                AddLog("STEP7- 開始取得 朋友清單..");
            }));
            try
            {
                step7Result = LogicHandler.FetchContacts.GetContactInfo(step5Result.Item1, step5Result.Item4, logPath);

                if (step7Result != null)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        AddLog("STEP7 Success =>" + step7Result.contacts.Count() + " 筆，朋友資料");
                        AddLog("STEP7 取得 朋友清單 成功  !!");
                    }));

                    var i = 0;
                    step7Result.contacts = step7Result.contacts.OrderBy(x => x.display_name).ToList();
                    foreach (var f in step7Result.contacts)
                    {
                        if (!f.blocked)
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                            {
                                //    AddLog(f.display_name + "," + f.blocked);
                                //for (var i = 1; i <= 10; i++)
                                //{
                                
                                var c = new CheckBox();
                                c.Content = f.display_name + " | " + f.person_id;
                                c.Tag = f.person_id;

                                listContact.Items.Add(c);
                                //   }
                                i++;
                            }));
                        }
                    }
                    MessageBox.Show("載入  " + i + " 筆聯絡人，不包含封鎖資料 !");

                }
                else
                {

                    AddLog("STEP7 取得 朋友清單 失敗");
                    MessageBox.Show("STEP7 發生錯誤，進行中止");
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);
                    return;

                }

            }
            catch (Exception ex)
            {
                new Thread(() =>
                {
                    AddLog("STEP7 出錯了:" + ex.Message + "," + ex.StackTrace);
                }).Start();

                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);


                MessageBox.Show("STEP7 發生錯誤，進行中止");
                return;
            }

            this.btnSent.IsEnabled = true;
            btnLoadContact.IsEnabled = false;
            #endregion
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
