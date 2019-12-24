using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace LazyLizard.LogicHandler
{
   public  class RedirectSkype1
    {

        public static Tuple<string,  Dictionary<string, string>> GetRedirectFormUrl(string url, string Token, string pprid, string NAP,string ANON,string   logPath)
        {

            var  httpWebRequest4 = WebRequest.Create(url) as HttpWebRequest;
            httpWebRequest4.Method = "POST";
            httpWebRequest4.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest4.Referer = "https://web.skype.com/zh-Hant/";
            httpWebRequest4.Headers.Add("Origin", "https://web.skype.com");
            httpWebRequest4.KeepAlive = false;
            NameValueCollection postParams4 = System.Web.HttpUtility.ParseQueryString(string.Empty);
            postParams4.Add("t", Token);
            postParams4.Add("pprid", pprid);
            postParams4.Add("NAP", NAP);
            postParams4.Add("ANON", ANON);

            byte[] byteArray4 = Encoding.UTF8.GetBytes(postParams4.ToString());
            using (Stream reqStream = httpWebRequest4.GetRequestStream())
            {
                reqStream.Write(byteArray4, 0, byteArray4.Length);
            }
            string responseStr4 = "";


            var tmpCook2 = new Dictionary<string, string>();
            using (WebResponse response = httpWebRequest4.GetResponse())
            {
                var cookieCollectionTmp = LogicHandler.Utility.ExtractCookiesFromResponse(response);
                WebHeaderCollection headers = response.Headers;

                foreach (var coo in cookieCollectionTmp)
                {
                    tmpCook2.Add(coo.Key, coo.Value);
                }


                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr4 = sr.ReadToEnd();
                    File.WriteAllText(logPath + "STEP4", responseStr4);
                    var url3 = LogicHandler.Utility.GetFormActionValueById(responseStr4, "redirectForm");
                    return new Tuple<string, Dictionary<string, string>>(url3, tmpCook2);
                }
            }


        }


    }
}
