using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace TwitchBotConsole
{
    class Json_status
    {
        public bool isOnline = true;
        public string game = "";
        string sUrl = "";

        public void SendChannel(string channel)
        {
            sUrl = "https://api.twitch.tv/kraken/streams/" + channel;
        }

        public void getStatus()
        {
            HttpWebRequest wRequest = (HttpWebRequest)HttpWebRequest.Create(sUrl);
            wRequest.ContentType = "application/json";
            wRequest.Accept = "application/vnd.twitchtv.v3+json";
            wRequest.Method = "GET";

            dynamic wResponse = wRequest.GetResponse().GetResponseStream();
            StreamReader reader = new StreamReader(wResponse);
            dynamic res = reader.ReadToEnd();
            reader.Close();
            wResponse.Close();

            if(res.Contains("display_name"))
            {
                isOnline = true;
                string temp = Convert.ToString(res);
                int indexStart = temp.IndexOf("game");
                if(indexStart>0)
                {
                    indexStart = indexStart + 7;
                    int indexEnd = temp.IndexOf(",", indexStart)-1;
                    game = temp.Substring(indexStart, indexEnd-indexStart);
                    Console.WriteLine("Stream is online, game: " + game);
                }
                else
                {
                    Console.WriteLine("Checked stream status. Is online.");
                }
            }
            else
            {
                isOnline = false;
                Console.WriteLine("Checked stream status. Is offline.");
            }
        }

        internal void TimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            getStatus();
        }

        internal void requestUpdate(IrcClient irc)
        {
            getStatus();
            if(game!=string.Empty)
            {
                irc.sendChatMessage("New isOnline status is - " + isOnline.ToString() + " and the game is: " + game);
            }
            else
            {
                irc.sendChatMessage("New isOnline status is - " + isOnline.ToString());
            }
        }
    }
}
