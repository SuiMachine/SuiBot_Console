using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TwitchBotConsole
{
	class SRL
	{
		Uri baseApiURL = new Uri("http://api.speedrunslive.com:81/");

		public SRL()
		{
		}

		public void getRaces(oldIRCClient IrcInst)
		{
			string response = "";
			try
			{
				if (JsonGrabber.GrabJson(GetUri("races"), out response))
				{
					string[] twitches = getEntrantsTwitches(response, IrcInst._config.channel);
					if(twitches != null)
					{
						IrcInst.sendChatMessage("http://kadgar.net/live/" + string.Join("/", twitches));
					}
					else
					{
						IrcInst.sendChatMessage("Nothing found");
					}
				}
			}
			catch
			{
				IrcInst.sendChatMessage("Some kind of error. Go, poke Sui to fix that");
			}
		}

		private string[] getEntrantsTwitches(string jsonTxt, string channel)
		{
			channel = channel.ToLower();
			var races = JObject.Parse(jsonTxt)["races"];
			foreach (var race in races)
			{
				int status = race["state"].ToObject<int>();
				var entrants = race["entrants"];
				if (status == 1 || status == 2 || status == 3 || status == 4)
				{
					foreach (var entrant in entrants)
					{
						var twitch = entrant.First["twitch"].Value<string>();
						if (twitch.ToLower() == channel)
						{
							List<string> twitches = new List<string>();
							foreach(var twitchEntrant in entrants)
							{
								if(twitchEntrant.First["twitch"].Value<string>() != "")
								{
									twitches.Add(twitchEntrant.First["twitch"].Value<string>());
								}
							}
							return twitches.ToArray();
						}
					}

				}
			}
			return null;
		}

		private Uri GetUri(string op)
		{
			return new Uri(baseApiURL, op);
		}
	}
}
