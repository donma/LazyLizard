using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace LazyLizard.LogicHandler
{
    public class FetchRegistToken
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skypeToken"></param>
        /// <param name="logPath"></param>
        /// <returns>
        /// Item1:registrationToken
        /// Item2:endpointId
        /// </returns>
        public static Tuple<string, string> GetRegistToken(string skypeToken, string logPath)
        {
            //Get Regist Token
            string requestUriString = "https://client-s.gateway.messenger.live.com/v1/users/ME/endpoints";

            HttpWebRequest httpWebRequest9 = WebRequest.Create(requestUriString) as HttpWebRequest;
            httpWebRequest9.Method = "POST";
            httpWebRequest9.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:35.0) Gecko/20100101 Firefox/35.0";
            httpWebRequest9.Accept = "*/*";
            httpWebRequest9.Headers.Add("Accept-Language", "zh-tw,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            httpWebRequest9.Headers.Add("Accept-Encoding", "gzip, deflate");
            httpWebRequest9.Headers.Add("ClientInfo", "os=Windows; osVer=8.1; proc=Win32; lcid=en-us; deviceType=1; country=n/a; clientName=skype.com; clientVer=908/1.9.0.232//skype.com");
            httpWebRequest9.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            httpWebRequest9.Headers.Add("Pragma", "no-cache");
            httpWebRequest9.Headers.Add("Expires", "0");
            httpWebRequest9.Headers.Add("BehaviorOverride", "redirectAs404");
            httpWebRequest9.Headers.Add("Authentication", "skypetoken=" + skypeToken);
            httpWebRequest9.ContentType = "application/json; charset=UTF-8";
            httpWebRequest9.Referer = "https://web.skype.com/zh-Hant/";
            httpWebRequest9.Headers.Add("Origin", "https://web.skype.com");
            httpWebRequest9.KeepAlive = false;
            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object)new Dictionary<string, object>()));
            using (Stream requestStream = httpWebRequest9.GetRequestStream())
                requestStream.Write(bytes, 0, bytes.Length);

            var registTokenSource = "";
            using (WebResponse response = httpWebRequest9.GetResponse())
            {
                File.WriteAllText(logPath + "STEP6", response.Headers["Set-RegistrationToken"]);
                registTokenSource = response.Headers["Set-RegistrationToken"].ToString();

                var registrationToken = registTokenSource.Split(';')[0].Replace("registrationToken=", "");
                var endpointId = registTokenSource.Split(';')[2].Replace("endpointId=", "");

                return new Tuple<string, string>(registrationToken, endpointId);
            }






        }
    }
}
