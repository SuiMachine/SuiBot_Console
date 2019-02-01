using System;
using System.Collections.Generic;
using TwitchBotConsole; //and this way it's useless to make it in seperate namespace... oh well

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
			Uri url = new Uri("http://www.cleverbot.com/getreply?key=" + API_key + "&wrapper=\"SuiBot\"" + "&input=" + message);
			string res = "";
			if (JsonGrabber.GrabJson(url, null, "application/json", null, "GET", out res))
			{
				try
				{
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
				catch (Exception e)
				{
					Console.WriteLine("Exception: " + e.ToString());
				}
			}

            return "";
        }
    }
}
