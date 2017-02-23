using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cleverbot
{
    class CleverbotSui
    {
        private string API_key { get; set; }
        private string conversationID { get; set; }
        private int random;

        public CleverbotSui(string API_key)
        {
            random = new Random().Next();
            conversationID = "";
            this.API_key = API_key;
        }

        public string getResponse(string message)
        {
            try
            {
                //http://www.cleverbot.com/getreply?key=YOURAPIKEY&input=Hello&cs=76nxdxIJ02AAA&callback=ProcessReply
                HttpWebRequest wRequest = (HttpWebRequest)HttpWebRequest.Create("http://www.cleverbot.com/getreply?key=" + API_key + "&wrapper=\"SuiBot\"" + "&input=" + message);
                wRequest.ContentType = "application/json";
                wRequest.Method = "GET";

                HttpWebResponse wResponse = (HttpWebResponse)wRequest.GetResponse();
                StreamReader reader = new StreamReader(wResponse.GetResponseStream());
                string res = reader.ReadToEnd();
                reader.Close();

                if (res != String.Empty)
                {
                    Dictionary<string, string> temp = JsonHelpers.jsonToDictionary(res);
                    if (temp["conversation_id"] != String.Empty)
                    {
                        return temp["clever_output"];
                    }
                }
                return "Error";
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
            }
            //

            return "";
        }
    }
}
