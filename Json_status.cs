using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace TwitchBotConsole
{
    class Json_status
    {
        oldIRCClient irc;
        ViewerPB viewerPB;
        public bool isOnline = true;
        public bool isForcedPage = false;
        public string game = "";
        public string forcedGame = "";
		Uri sUrl = null;
        public uint category = 0;
		Dictionary<string, string> RequestHeaders;


		public Json_status(oldIRCClient _irc, ViewerPB _viewerPB)
        {
            irc = _irc;
            viewerPB = _viewerPB;
			RequestHeaders = new Dictionary<string, string>();
			RequestHeaders.Add("Client-ID", "m5fhbaoh4ca8bhl23u5escfn5583fq4");

		}

        public void SendChannel(string channel)
        {
            sUrl = new Uri("https://api.twitch.tv/kraken/streams/" + channel);
        }

        public void getStatus()
        {
			string res = "";
			if(JsonGrabber.GrabJson(sUrl, RequestHeaders, "application/json", "application/vnd.twitchtv.v3+json", "GET", out res))
			{
				if (res.Contains("display_name"))
				{
					isOnline = true;
					string temp = Convert.ToString(res);
					int indexStart = temp.IndexOf("stream_type");
					if (indexStart > 0)
					{
						indexStart += "stream_type".Length + 3;
						int indexEnd = temp.IndexOf(",", indexStart);
						string thatThing = temp.Substring(indexStart, indexEnd - indexStart - 1).ToLower();
						if (thatThing == "live")
							isOnline = true;
						else
							isOnline = false;
					}

					indexStart = temp.IndexOf("game");
					if (indexStart > 0)
					{
						indexStart = indexStart + 7;
						int indexEnd = temp.IndexOf(",", indexStart) - 1;
						game = temp.Substring(indexStart, indexEnd - indexStart);
						if (game == "ul")
						{
							game = String.Empty;
						}
						Console.WriteLine("Stream is online, game: " + game);
					}
					else
					{
						Console.WriteLine("Checked stream status. Is online.");
					}

					indexStart = temp.IndexOf("viewers");
					if (indexStart > 0)
					{
						indexStart = indexStart + 9;
						int indexEnd = temp.IndexOf(",", indexStart);
						uint Value;
						if (uint.TryParse(temp.Substring(indexStart, indexEnd - indexStart), out Value))
						{
							viewerPB.CheckViewerPB(Value);
						}
					}
				}
				else
				{
					isOnline = false;
					Console.WriteLine("Checked stream status. Is offline.");
				}
			}
			else
			{
				Console.WriteLine("Error checking Json");
			}


			
        }

        public string getStreamTime()
        {
			string res = "";
			if (JsonGrabber.GrabJson(sUrl, RequestHeaders, "application/json", "application/vnd.twitchtv.v3+json", "GET", out res))
			{
				if (res.Contains("display_name"))
				{
					isOnline = true;
					string temp = Convert.ToString(res);
					int indexStart = temp.IndexOf("created_at");

					if (indexStart > 0)
					{
						indexStart = indexStart + 13;
						int indexEnd = temp.IndexOf(",", indexStart) - 2;
						string output = temp.Substring(indexStart, indexEnd - indexStart);
						DateTime dt;
						if (DateTime.TryParse(output, out dt))
						{
							TimeSpan difference = DateTime.UtcNow - dt;
							return "on " + dt.Date.ToShortDateString() + " at " + dt.TimeOfDay.ToString() + " -- " + difference.Hours.ToString("00") + ":" + difference.Minutes.ToString("00") + ":" + difference.Seconds.ToString("00");
						}
						else
							return "";

					}
				}
				else
				{
					isOnline = false;
				}
				return "";
			}
			return "";


        }

        internal void TimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            getStatus();
        }

        internal void requestUpdate()
        {
            getStatus();
            if(game!=string.Empty)
            {
                irc.sendChatMessage("New isOnline status is - \'" + isOnline.ToString() + "\' and the game is: " + game);
            }
            else
            {
                irc.sendChatMessage("New isOnline status is - " + isOnline.ToString());
            }
        }

        internal void forcedGameFunction(ReadMessage msg)
        {
            if(irc.moderators.Contains(msg.user))
            {
                if(msg.message.Contains(' '))
                {
                    string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                    forcedGame = helper[1];
                    isForcedPage = true;
                    irc.sendChatMessage("Forcing a game set to: " + helper[1]);
                }
                else
                {
                    forcedGame = string.Empty;
                    isForcedPage = false;
                    irc.sendChatMessage("Disabled forcing speedrun.con game");
                }
            }
        }

        internal void forceCategoryFunction(ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                if (msg.message.Contains(' '))
                {
                    uint number = 0;
                    string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                    if(uint.TryParse(helper[1], out number))
                    {
                        category = number;
                        irc.sendChatMessage("The category has been set to: " + category.ToString());
                    }
                    else
                    {
                        irc.sendChatMessage("Invalid category ID!");
                    }


                }
            }
        }
    }
}
