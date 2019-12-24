using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace LazyLizard.LogicHandler
{
    public class RedirectSkype2
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="Token"></param>
        /// <param name="logPath"></param>
        /// <returns>
        /// Item1:skypeid
        /// Item2:signinname
        /// Item3:expires_in
        /// Item4:skypetoken
        /// </returns>
        public static Tuple<string, string, string, string, Dictionary<string, string>> GetRedirectFormUrlGetToken(string url, string Token, string logPath)
        {
            HttpWebRequest httpWebRequest5 = WebRequest.Create(url) as HttpWebRequest;
            httpWebRequest5.Method = "POST";
            httpWebRequest5.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest5.Referer = "https://web.skype.com/zh-Hant/";
            httpWebRequest5.Headers.Add("Origin", "https://web.skype.com");
            httpWebRequest5.KeepAlive = false;
            NameValueCollection postParams5 = System.Web.HttpUtility.ParseQueryString(string.Empty);

            postParams5.Add("t", Token);

            byte[] byteArray5 = Encoding.UTF8.GetBytes(postParams5.ToString());
            using (Stream reqStream = httpWebRequest5.GetRequestStream())
            {
                reqStream.Write(byteArray5, 0, byteArray5.Length);
            }
            string responseStr5 = "";


            var tmpCook5 = new Dictionary<string, string>();
            using (WebResponse response = httpWebRequest5.GetResponse())
            {
                var cookieCollection = LogicHandler.Utility.ExtractCookiesFromResponse(response);
                WebHeaderCollection headers = response.Headers;

                foreach (var coo in cookieCollection)
                {
                    tmpCook5.Add(coo.Key, coo.Value);
                }

                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr5 = sr.ReadToEnd();
                    File.WriteAllText(logPath + "STEP5", responseStr5);

                    var skypeid = LogicHandler.Utility.GetHTMLHiddenValueByName(responseStr5, "skypeid");
                    var signinname = LogicHandler.Utility.GetHTMLHiddenValueByName(responseStr5, "signinname"); ;
                    var expires_in = LogicHandler.Utility.GetHTMLHiddenValueByName(responseStr5, "expires_in");
                    var skypetoken = LogicHandler.Utility.GetHTMLHiddenValueByName(responseStr5, "skypetoken");

                    return new Tuple<string, string, string, string, Dictionary<string, string>>(skypeid, signinname, expires_in, skypetoken, tmpCook5);

                }//end using  
            }


            //Request Final


        }
    }
}
