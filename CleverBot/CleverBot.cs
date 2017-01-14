using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Diagnostics;
using System.Net.Http;

namespace CleverBotNS
{
    class CleverBot
    {

        //This class is loosly based on CleverBot.Net
        //Normally I'd just use the entire thing, but apparently async connections don't work very well with Mono
        public bool isConnected { get; set; }
        private string ApiUser { get; set; }
        private string ApiKey { get; set; }
        public string BotNick { get; private set; }

        public CleverBot(string ApiUser, string ApiKey, string BotNick = "")
        {
            this.ApiUser = ApiUser;
            this.ApiKey = ApiKey;
            this.BotNick = BotNick;
            isConnected = connect();
            if (isConnected)
            {
                Console.WriteLine("Connected: " + isConnected.ToString());
                Console.WriteLine("BotNick: " + this.BotNick);
            }
        }

        public string askAndGetResponse(string text)
        {
            if (!isConnected)
                connect();

            try
            {
                HttpWebRequest wRequest = (HttpWebRequest)HttpWebRequest.Create("https://cleverbot.io/1.0/ask");
                wRequest.ContentType = "application/json";
                wRequest.Method = "POST";
                StreamWriter sw = new StreamWriter(wRequest.GetRequestStream());

                {
                    string tempJson = JsonHelpers.Property("user", ApiUser)
                        + JsonHelpers.Property("key", ApiKey)
                        + JsonHelpers.Property("nick", BotNick)
                        + JsonHelpers.Property("text", text);
                    sw.Write(JsonHelpers.Segment(tempJson));
                }

                sw.Flush();
                sw.Close();

                HttpWebResponse wResponse = (HttpWebResponse)wRequest.GetResponse();
                StreamReader reader = new StreamReader(wResponse.GetResponseStream());
                string res = reader.ReadToEnd();
                reader.Close();

                if (res != String.Empty)
                {
                    Dictionary<string, string> temp = JsonHelpers.jsonToDictionary(res);
                    if (temp["status"] == "success")
                    {
                        return temp["response"];
                    }
                }
                return "Error";
            }
            catch (Exception ex)
            {
                isConnected = false;
                Console.WriteLine(ex);
                return "Error";
            }
        }


        private bool connect()
        {
            Debug.WriteLine("Connecting to CleverBot.io");
            HttpWebRequest wRequest = (HttpWebRequest)HttpWebRequest.Create("https://cleverbot.io/1.0/create");
            wRequest.ContentType = "application/json";
            wRequest.Method = "POST";
            StreamWriter sw = new StreamWriter(wRequest.GetRequestStream());


            {
                string tempJson = JsonHelpers.Property("user", ApiUser)
                    + JsonHelpers.Property("key", ApiKey);
                if (BotNick != "")
                    tempJson += JsonHelpers.Property("nick", BotNick);
                sw.Write(JsonHelpers.Segment(tempJson));
            }


            sw.Flush();
            sw.Close();
            HttpWebResponse wResponse = (HttpWebResponse)wRequest.GetResponse();
            StreamReader reader = new StreamReader(wResponse.GetResponseStream());
            string res = reader.ReadToEnd();
            reader.Close();
            if (res!=String.Empty)
            {
                try
                {
                    Dictionary<string, string> temp = JsonHelpers.jsonToDictionary(res);
                    if (temp["status"] == "success")
                    {
                        Debug.WriteLine("Connected to CleverBot.io");
                        BotNick = temp["nick"];
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("Failed to connect to cleverbot io: " + res);
                        return false;
                    }

                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error Exception" + ex);
                    return false;
                }
            }

            return false;
        }
    }
}
