using LazyLizard.LogicHandler;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
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
        public MainWindow()
        {
            InitializeComponent();

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

            var PPFT = "";
            var MSPRequ = "";
            var MSPOK = "";


            //Check Parameter First.
            if (string.IsNullOrEmpty(txtAccount.Text))
            {
                MessageBox.Show("Account is Empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }
            if (string.IsNullOrEmpty(txtPass.Password))
            {
                MessageBox.Show("Password is Empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtMessage.Text.Trim()) && string.IsNullOrEmpty(txtImageFilePath.Text.Trim()))
            {
                MessageBox.Show("You hava nothing to send.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            Tuple<string, string, string, string, string> step3Result = null;

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
                        AddLog("STEP3 穿越失敗，沒有拿到 參數");
                    }));

                    MessageBox.Show("STEP3 發生錯誤，進行中止:沒有拿到 t and nextpath");
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

            Tuple<string, string, string, string, Dictionary<string, string>> step5Result = null;
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
            Tuple<string, string> step6Result = null;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                AddLog("STEP6- 開始取的 registrationToken..");
            }));

            try
            {
                step6Result = LogicHandler.FetchRegistToken.GetRegistToken(step5Result.Item4, logPath);

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


            #endregion


            #region STEP8 

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                AddLog("STEP8- 廣播文字..");
            }));

            try
            {
                if (txtMessage.Text.Trim().Length > 0)
                {
                    var userCount = 0;
                    foreach (var c in step7Result.contacts)
                    {
                        try
                        {
                            if (!c.blocked)
                            {
                                LogicHandler.SendMessage.SendText(step6Result.Item1, txtMessage.Text, c.person_id);
                                Thread.Sleep(196);
                                userCount++;
                                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                                {
                                    AddLog("已寄送文字給" + c.display_name + " , 第" + userCount + "筆");
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                            {
                                AddLog("ERROR 已寄送文字給" + c.display_name + " , 失敗，請手動寄送" + ex.Message);

                            }));
                         
                            continue;
                        }
                    }


                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        AddLog("STEP8 寄送成功  !!");
                        AddLog("STEP8 寄送 " + userCount + " 筆");
                    }));

                }
                else
                {
                    AddLog("STEP8 無文字所以跳過不廣播了  !!");
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


            try
            {
                if (txtImageFilePath.Text.Trim().Length > 0)
                {
                   
                    var userCount = 0;
                    foreach (var c in step7Result.contacts)
                    {
                        if (!c.blocked)
                        {
                            try
                            {
                                var objectId = LogicHandler.SendMessage.CreateObject(step5Result.Item4, step5Result.Item1, logPath);
                                LogicHandler.SendMessage.UploadFileToObject(step5Result.Item4, objectId, txtImageFilePath.Text.Trim(), logPath);
                                var fileInfo = new FileInfo(txtImageFilePath.Text.Trim());
                                
                                LogicHandler.SendMessage.SendIImage(step6Result.Item1, objectId, c.person_id, fileInfo.Name, logPath);
                                
                                Thread.Sleep(196);
                                userCount++;
                                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                                {
                                    AddLog("已寄送圖片給" + c.display_name + " , 第" + userCount + "筆");

                                }));
                            }
                            catch (Exception ex)
                            {
                                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                                {
                                    AddLog("ERROR 已寄送圖片給" + c.display_name + " , 失敗，請手動寄送" + ex.Message);

                                }));
                                
                                continue;
                            }

                        }
                    }

                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        AddLog("STEP9 寄送圖片成功  !!");
                        AddLog("STEP9 寄送 " + userCount + " 筆");
                    }));


                }
                else
                {
                    AddLog("STEP9 無圖片所以跳過不廣播了  !!");
                }

            }
            catch (Exception ex)
            {
                new Thread(() =>
                {
                    AddLog("STEP9 出錯了:" + ex.Message + "," + ex.StackTrace);
                }).Start();

                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log" + System.IO.Path.DirectorySeparatorChar + SelfTrasationId + ".log", txtLog.Text);


                MessageBox.Show("STEP9 發生錯誤，進行中止");
                return;
            }


            #endregion
        }
    }
}
