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
        public static Tuple<string, string, string> GetRegistToken(string skypeToken, string logPath, string targetUri= "https://client-s.gateway.messenger.live.com/v1/users/ME/endpoints")
        {
            //Get Regist Token
            //string requestUriString = "https://bn2-s.gateway.messenger.live.com/v1/users/ME/endpoints";
            //string requestUriString = "https://client-s.gateway.messenger.live.com/v1/users/ME/endpoints";
            //string requestUriString = "https://db5-client-s.gateway.messenger.live.com/v1/users/ME/endpoints";
            // string requestUriString = "https://eusbn1-client-s.gateway.messenger.live.com/v1/users/ME/endpoints";
            //

            //string requestUriString = targetUri;
            //HttpWebRequest httpWebRequest9 = WebRequest.Create(requestUriString) as HttpWebRequest;
            //httpWebRequest9.Method = "POST";
            //httpWebRequest9.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:35.0) Gecko/20100101 Firefox/35.0";
            //httpWebRequest9.Accept = "*/*";
            //httpWebRequest9.Headers.Add("Accept-Language", "zh-tw,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            //httpWebRequest9.Headers.Add("Accept-Encoding", "gzip, deflate");
            //httpWebRequest9.Headers.Add("ClientInfo", "os=Windows; osVer=8.1; proc=Win32; lcid=en-us; deviceType=1; country=n/a; clientName=skype.com; clientVer=908/1.9.0.232//skype.com");
            //httpWebRequest9.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            //httpWebRequest9.Headers.Add("Pragma", "no-cache");
            //httpWebRequest9.Headers.Add("Expires", "0");
            //httpWebRequest9.Headers.Add("BehaviorOverride", "redirectAs404");
            //httpWebRequest9.Headers.Add("Authentication", "skypetoken=" + skypeToken);
            //httpWebRequest9.ContentType = "application/json; charset=UTF-8";
            //httpWebRequest9.Referer = "https://web.skype.com/zh-Hant/";
            //httpWebRequest9.Headers.Add("Origin", "https://web.skype.com");
            //httpWebRequest9.KeepAlive = false;
            //byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object)new Dictionary<string, object>()));
            //using (Stream requestStream = httpWebRequest9.GetRequestStream())
            //{
            //    requestStream.Write(bytes, 0, bytes.Length);
            //}
            //var registTokenSource = "";

            //try
            //{
            //    var response = httpWebRequest9.GetResponse();
            //    File.WriteAllText(logPath + "STEP6", response.Headers["Set-RegistrationToken"]);
            //    registTokenSource = response.Headers["Set-RegistrationToken"].ToString();

            //    var registrationToken = registTokenSource.Split(';')[0].Replace("registrationToken=", "");
            //    var endpointId = registTokenSource.Split(';')[2].Replace("endpointId=", "");

            //    return new Tuple<string, string>(registrationToken, endpointId);
            //}
            //catch (WebException webExcp)
            //{
            //    return new Tuple<string, string>("", webExcp.Response.Headers["Location"]);
            //}

            //return new Tuple<string, string>("", "");


            //using (WebResponse response = httpWebRequest9.GetResponse())
            //{
            //    File.WriteAllText(logPath + "STEP6", response.Headers["Set-RegistrationToken"]);
            //    registTokenSource = response.Headers["Set-RegistrationToken"].ToString();

            //    var registrationToken = registTokenSource.Split(';')[0].Replace("registrationToken=", "");
            //    var endpointId = registTokenSource.Split(';')[2].Replace("endpointId=", "");

            //    return new Tuple<string, string>(registrationToken, endpointId);
            //}




            //Update:

            string requestUriString = targetUri;
            var serverUri = requestUriString.Replace("endpoints", "");
            var httpWebRequest9 = CreateHttpWebRequest9(requestUriString, skypeToken);
            var registTokenSource = "";

            WebResponse response = null;

            try
            {
                response = httpWebRequest9.GetResponse();
                registTokenSource = response.Headers["Set-RegistrationToken"].ToString();
            }
            catch (WebException webExcp)
            {
                requestUriString = webExcp.Response.Headers["Location"];
                serverUri = requestUriString.Replace("endpoints", "");
                var httpWebRequest9_1 = CreateHttpWebRequest9(requestUriString, skypeToken);
                response = httpWebRequest9_1.GetResponse();
                registTokenSource = response.Headers["Set-RegistrationToken"].ToString();
            }

            File.WriteAllText(logPath + "STEP6", response.Headers["Set-RegistrationToken"]);
            var registrationToken = registTokenSource.Split(';')[0].Replace("registrationToken=", "");
            var endpointId = registTokenSource.Split(';')[2].Replace("endpointId=", "");

            return new Tuple<string, string, string>(registrationToken, endpointId, serverUri);


            HttpWebRequest CreateHttpWebRequest9(string url, string skypeToken)
            {
                HttpWebRequest httpWebRequest9 = WebRequest.Create(url) as HttpWebRequest;
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

                return httpWebRequest9;
            }
        }
    }
}
