using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace LazyLizard.LogicHandler
{
    public class SendMessage
    {
        private static int Count { get; set; }


        public static void UploadFileToObject(string skypeToken, string imageId, string imageLocalFilePath, string logPath)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Host", "api.asm.skype.com");
                client.Headers.Add("Authorization", "skype_token " + skypeToken);
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
                client.Headers.Add("Content-Type", "application");

                var fileInfo = new FileInfo(imageLocalFilePath);

                var res = client.UploadData("https://api.asm.skype.com/v1/objects/" + imageId + "/content/imgpsh", "PUT", File.ReadAllBytes(imageLocalFilePath));


            }
        }
        public static string CreateObject(string skypeToken, string skypeId, string logPath)
        {

            HttpWebRequest httpWebRequestw1 = WebRequest.Create("https://api.asm.skype.com/v1/objects") as HttpWebRequest;
            httpWebRequestw1.Method = "POST";
            httpWebRequestw1.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:35.0) Gecko/20100101 Firefox/35.0";
            httpWebRequestw1.Headers.Add("Accept-Language", "zh-tw,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            httpWebRequestw1.Headers.Add("Accept-Encoding", "gzip, deflate");
            httpWebRequestw1.Referer = ("https://web.skype.com/");
            httpWebRequestw1.Headers.Add("Authorization", "skype_token " + skypeToken);
            httpWebRequestw1.Headers.Add("Origin", "https://web.skype.com");
            httpWebRequestw1.Headers.Add("X-Client-Version", "1418/8.55.0.123//");
            httpWebRequestw1.KeepAlive = true;
            httpWebRequestw1.ContentType = "application/json; charset=UTF-8";
            var str = "{\"permissions\":{\"" + "8:" + skypeId + "\":[\"read\"]},\"type\":\"pish/image\",\"filename\":\"ddd.jpg\"}";

            byte[] bytesw1 = Encoding.UTF8.GetBytes(str);
            using (Stream requestStream = httpWebRequestw1.GetRequestStream())
            {
                requestStream.Write(bytesw1, 0, bytesw1.Length);
            }

            var imageInfo = new ResponseImageId();

            using (WebResponse response = httpWebRequestw1.GetResponse())
            {
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string end = streamReader.ReadToEnd();
                streamReader.Close();
                Count += 1;
                File.WriteAllText(logPath + "CREATEIMAGE_OBJECT_" + (Count), end);
                imageInfo = JsonConvert.DeserializeObject<ResponseImageId>(end);
                return imageInfo.id;
            }

        }


        public static void SendIImage(string registrationToken, string objectId, string userId, string filename, string logPath)
        {

            var src5 = "<URIObject type=\"Picture.1\" uri=\"https://api.asm.skype.com/v1/objects/" + objectId + "\"" +
              " url_thumbnail=\"https://api.asm.skype.com/v1/objects/" + objectId + "/views/imgt1\"><Title /><Description />" +
              "<meta type=\"photo\" originalName=\"ddd.jpg\"/>" +
              "<OriginalName v=\"ddd.jpg\"/>" +
              "</URIObject>";


            //POST https://client-s.gateway.messenger.live.com/v1/users/ME/conversations/(string: id)/messages
            HttpWebRequest httpWebRequest = WebRequest.Create("https://client-s.gateway.messenger.live.com/v1/users/ME/conversations/" + userId + "/messages") as HttpWebRequest;
            httpWebRequest.Method = "POST";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:35.0) Gecko/20100101 Firefox/35.0";
            httpWebRequest.Accept = "application/json, text/javascript";
            httpWebRequest.Headers.Add("Accept-Language", "zh-tw,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
            httpWebRequest.Headers.Add("ClientInfo", "os=Windows; osVer=8.1; proc=Win32; lcid=en-us; deviceType=1; country=n/a; clientName=skype.com; clientVer=908/1.9.0.232//skype.com");
            httpWebRequest.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            httpWebRequest.Headers.Add("Pragma", "no-cache");
            httpWebRequest.Headers.Add("Expires", "0");
            httpWebRequest.Headers.Add("BehaviorOverride", "redirectAs404");
            httpWebRequest.Headers.Add("RegistrationToken", "registrationToken=" + registrationToken);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Referer = "https://web.skype.com/zh-Hant/";
            httpWebRequest.Headers.Add("Origin", "https://web.skype.com");
            httpWebRequest.KeepAlive = true;
            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object)new Dictionary<string, object>()
      {
        {
          "content",
           (object)src5
        },
       {
          "messagetype",
          (object) "RichText/UriObject"
        },
        {
          "contenttype",
          (object)src5
        },
        {
          "clientmessageid",
          (object) DateTime.Now.Ticks
        }
      }));
            using (Stream requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (httpWebRequest.GetResponse())
            {

            }
        }

        public static void SendText(string registrationToken, string message, string id)
        {


            //POST https://client-s.gateway.messenger.live.com/v1/users/ME/conversations/(string: id)/messages
            HttpWebRequest httpWebRequest = WebRequest.Create("https://client-s.gateway.messenger.live.com/v1/users/ME/conversations/" + id + "/messages") as HttpWebRequest;
            httpWebRequest.Method = "POST";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:35.0) Gecko/20100101 Firefox/35.0";
            httpWebRequest.Accept = "application/json, text/javascript";
            httpWebRequest.Headers.Add("Accept-Language", "zh-tw,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
            httpWebRequest.Headers.Add("ClientInfo", "os=Windows; osVer=8.1; proc=Win32; lcid=en-us; deviceType=1; country=n/a; clientName=skype.com; clientVer=908/1.9.0.232//skype.com");
            httpWebRequest.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            httpWebRequest.Headers.Add("Pragma", "no-cache");
            httpWebRequest.Headers.Add("Expires", "0");
            httpWebRequest.Headers.Add("BehaviorOverride", "redirectAs404");
            httpWebRequest.Headers.Add("RegistrationToken", "registrationToken=" + registrationToken);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Referer = "https://web.skype.com/zh-Hant/";
            httpWebRequest.Headers.Add("Origin", "https://web.skype.com");
            httpWebRequest.KeepAlive = true;
            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object)new Dictionary<string, object>()
      {
        {
          "content",
           (object)message
        },
        {
          "messagetype",
          (object) "Text"
        },
        {
          "contenttype",
          (object)"text"
        },
        {
          "clientmessageid",
          (object) DateTime.Now.Ticks
        }
      }));
            using (Stream requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (httpWebRequest.GetResponse())
            {

            }
        }
    }
}
