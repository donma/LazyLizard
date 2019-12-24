using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace LazyLizard.LogicHandler
{
    public class FirstLogin
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="pass"></param>
        /// <param name="MSPRequ"></param>
        /// <param name="MSPOK"></param>
        /// <param name="PPFT"></param>
        /// <returns>Item1 : for urlPost , Item2 : for cookie copy</returns>
        public static Tuple<string, Dictionary<string, string>> GetUrlPostAndCookies(string account, string pass, string MSPRequ, string MSPOK, string PPFT,string logPath,string timestamp)
        {
            var httpWebRequestFirstLogin = WebRequest.Create("https://login.live.com/ppsecure/post.srf?wa=wsignin1.0&" +
              "wp=MBI_SSL&" +
              "wreply=https://lw.skype.com/login/oauth/proxy?client_id=578134&site_name=lw.skype.com&redirect_uri=https%3A%2F%2Fweb.skype.com%2F") as HttpWebRequest;

            httpWebRequestFirstLogin.Method = "POST";
            httpWebRequestFirstLogin.KeepAlive = false;

            //simulate cookie
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new System.Net.Cookie("MSPRequ", MSPRequ, "/", "login.live.com"));
            cookieContainer.Add(new System.Net.Cookie("MSPOK", MSPOK, "/", "login.live.com"));
            cookieContainer.Add(new System.Net.Cookie("CkTst", timestamp, "/", "login.live.com"));
            httpWebRequestFirstLogin.CookieContainer = cookieContainer;
            httpWebRequestFirstLogin.ContentType = "application/x-www-form-urlencoded";

            //Simulate post data
            NameValueCollection postParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
            postParams.Add("loginfmt", account);
            postParams.Add("passwd", pass);
            postParams.Add("PPFT", PPFT);

            var responseFirstloginbytes = Encoding.UTF8.GetBytes(postParams.ToString());
            using (Stream reqStream = httpWebRequestFirstLogin.GetRequestStream())
            {
                reqStream.Write(responseFirstloginbytes, 0, responseFirstloginbytes.Length);
            }

            //API回傳的字串
            string responseFirstloginStr = "";

            //對回應回來的 cookie 進行抄錄
            var tmpFirstLoginCookieResponse = new Dictionary<string, string>();

            //發出Request
            using (WebResponse response = httpWebRequestFirstLogin.GetResponse())
            {

                var cookieCollection = LogicHandler.Utility.ExtractCookiesFromResponse(response);
                WebHeaderCollection headers = response.Headers;
                foreach (var coo in cookieCollection)
                {
                    tmpFirstLoginCookieResponse.Add(coo.Key, coo.Value);
                }
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseFirstloginStr = sr.ReadToEnd();

                    File.WriteAllText(logPath + "STEP2", responseFirstloginStr);

                    var urlPost = LogicHandler.Utility.GetFromHtmlSource_urlPost(responseFirstloginStr);
                    return new Tuple<string, Dictionary<string, string>>(urlPost, tmpFirstLoginCookieResponse);
                }
            };
          

        }
    }
}