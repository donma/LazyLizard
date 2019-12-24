using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LazyLizard.LogicHandler
{
    public static class Utility
    {

        public static string GetFromHtmlSource_urlPost(string htmlSource)
        {
            Match match = Regex.Match(htmlSource, "urlPost:'(.*?)',");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return "";
        }

        public static string GetHTMLHiddenValueByName(string htmlSource, string name)
        {
            Match match = Regex.Match(htmlSource, "<input[^>]*name=\"" + name + "\"[^>]*value=\"([^\"]*)\"");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return "";
        }

        public static string GetFormActionValueById(string htmlSource, string id)
        {
            Match match = Regex.Match(htmlSource, "<form[^>]*id=\"" + id + "\"[^>]*action=\"([^\"]*)\"");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return "";
        }

        public static string GetFormActionValueByName(string htmlSource, string name)
        {
            Match match = Regex.Match(htmlSource, "<form[^>]*name=\"" + name + "\"[^>]*action=\"([^\"]*)\"");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return "";
        }


        public static IDictionary<string, string> ExtractCookiesFromResponse(WebResponse httpResponseSource)
        {
            IDictionary<string, string> result = new Dictionary<string, string>();
            SetCookieHeaderValue.ParseList(httpResponseSource.Headers.GetValues("Set-Cookie")).ToList().ForEach(cookie =>
            {
                result.Add(cookie.Name.ToString(), cookie.Value.ToString());
            });
            return result;
        }


        /// <summary>
        /// 取得該網址的 資料，並且指定一些 cookie 取出他的資料
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieNames">用 , 分開你要抓的 cookie name</param>
        /// <param name="dictionaryCookie">回傳 有抓到的資料</param>
        /// <returns>該網頁的文字資料</returns>
        public static string GetUrlContentAndFetchCookieValue(string url, string[] cookieNames, ref Dictionary<string, string> dictionaryCookie)
        {

            HttpWebRequest httpWebRequest = WebRequest.Create("https://login.skype.com/login") as HttpWebRequest;
            httpWebRequest.Method = "GET";
            using (WebResponse response = httpWebRequest.GetResponse())
            {
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string contentStringData = streamReader.ReadToEnd();
                streamReader.Close();
                var cookies = ExtractCookiesFromResponse(response);

                WebHeaderCollection headers = response.Headers;
                if (cookieNames != null)
                {
                    foreach (var dc in cookieNames)
                    {
                        if (cookies.ContainsKey(dc))
                        {
                            dictionaryCookie.Add(dc, cookies[dc]);
                        }
                    }
                }
                return contentStringData;
            }
        }
    }
}
