using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace LazyLizard.LogicHandler
{
    public class SecondLogin
    {



        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlPost"></param>
        /// <param name="MSPRequ"></param>
        /// <param name="MSPOK"></param>
        /// <param name="cookieCollection"></param>
        /// <param name="PPFT"></param>
        /// <param name="account"></param>
        /// <param name="pass"></param>
        /// <param name="logPath"></param>
        /// <returns>
        /// Item1: Token
        /// Item2: pprid
        /// Item3: NAP
        /// Item4: ANON
        /// Item5: next url
        /// </returns>
        public static Tuple<string, string, string, string, string, string> GetUrlPostAndCookies(string urlPost, string MSPRequ, string MSPOK, Dictionary<string, string> cookieCollection, string PPFT, string account, string pass, string logPath,string timestamp)
        {


            // url = "https://login.live.com/ppsecure/post.srf";
            var httpWebRequest2 = WebRequest.Create(urlPost) as HttpWebRequest;


            var cookieContainer2 = new CookieContainer();
            cookieContainer2.Add(new System.Net.Cookie("MSPRequ", MSPRequ, "/", "login.live.com"));
            cookieContainer2.Add(new System.Net.Cookie("MSPOK", MSPOK, "/", "login.live.com"));
            cookieContainer2.Add(new System.Net.Cookie("CkTst", "G" + timestamp, "/", "login.live.com"));


            //模擬之前連線的所有 COOKIE
            foreach (var tmpC in cookieCollection)
            {
                cookieContainer2.Add(new System.Net.Cookie(tmpC.Key, tmpC.Value, "/", "login.live.com"));

            }

            httpWebRequest2.CookieContainer = cookieContainer2;


            httpWebRequest2.Method = "POST";
            httpWebRequest2.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest2.Referer = "https://web.skype.com/zh-Hant/";
            httpWebRequest2.Headers.Add("Origin", "https://web.skype.com");
            httpWebRequest2.KeepAlive = false;
            NameValueCollection postParams2 = System.Web.HttpUtility.ParseQueryString(string.Empty);
            postParams2.Add("login", account);
            postParams2.Add("passwd", pass);
            postParams2.Add("PPFT", PPFT);
            postParams2.Add("i13", "0");
            postParams2.Add("loginfmt", "moveOffScreen, value: unsafe_displayName");
            postParams2.Add("type", "11");
            postParams2.Add("LoginOptions", "3");

            byte[] byteArray2 = Encoding.UTF8.GetBytes(postParams2.ToString());
            using (Stream reqStream = httpWebRequest2.GetRequestStream())
            {
                reqStream.Write(byteArray2, 0, byteArray2.Length);
            }
            string responseStr2 = "";

            using (WebResponse response = httpWebRequest2.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr2 = sr.ReadToEnd();

                    File.WriteAllText(logPath + "STEP3", responseStr2);

                    var Token = LogicHandler.Utility.GetHTMLHiddenValueByName(responseStr2, "t");
                    var pprid = LogicHandler.Utility.GetHTMLHiddenValueByName(responseStr2, "pprid");
                    var NAP = LogicHandler.Utility.GetHTMLHiddenValueByName(responseStr2, "NAP");
                    var ANON = LogicHandler.Utility.GetHTMLHiddenValueByName(responseStr2, "ANON");
                    var url2 = LogicHandler.Utility.GetFormActionValueByName(responseStr2, "fmHF");
                    var errMSG = "";

                    if (String.IsNullOrEmpty(Token) &&
                        String.IsNullOrEmpty(pprid) &&
                        String.IsNullOrEmpty(NAP) &&
                        String.IsNullOrEmpty(ANON) &&
                        String.IsNullOrEmpty(url2)) {

                        if (responseStr2.Contains("https://account.microsoft.com/security")) {
                            errMSG = "請關閉二次驗證再試一次"; 
                        }

                        if (responseStr2.Contains("Your account or password is incorrect"))
                        {
                            errMSG = "帳號或密碼錯誤";
                        }
                    }

                    return new Tuple<string, string, string, string, string, string>(Token, pprid, NAP, ANON, url2, errMSG);


                }
            }



        }


    }
}
