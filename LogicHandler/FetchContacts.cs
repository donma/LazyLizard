using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace LazyLizard.LogicHandler
{
    public class FetchContacts
    {
        public static ContactInfo GetContactInfo(string skypeId,string skypeToken,string logPath) {

            //Contact list
            string requestUriStringUL = "https://contacts.skype.com/contacts/v2/users/" + skypeId + "/contacts";

            //string requestUriStringUL = "https://client-s.gateway.messenger.live.com/v1/users/ME/conversations/" + skypeId+ "/messages";

            //string requestUriStringUL = "https://contacts.skype.com/contacts/v2/users/" + skypeId + "/groups";

            //string requestUriStringUL = "https://contacts.skype.com/contacts/v2/users/" + "self";

            HttpWebRequest httpWebRequest10 = WebRequest.Create(requestUriStringUL) as HttpWebRequest;
            httpWebRequest10.Method = "GET";
            httpWebRequest10.Headers.Add("X-Skypetoken", skypeToken);
            httpWebRequest10.Headers.Add("Authentication", "skypetoken=" + skypeToken);
            httpWebRequest10.ContentType = "application/json; charset=UTF-8";
            httpWebRequest10.Headers.Add("Origin", "https://web.skype.com");
            httpWebRequest10.KeepAlive = false;

            var friends = new ContactInfo();

            using (WebResponse response = httpWebRequest10.GetResponse())
            {
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string end = streamReader.ReadToEnd();
                streamReader.Close();
                File.WriteAllText(logPath + "STEP7", end);
               
              return  JsonConvert.DeserializeObject<ContactInfo>(end);
            }

        }
    }
}
