using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TwitchBotConsole
{
    class Slots
    {
        Random rnd = new Random(DateTime.UtcNow.Millisecond);
        Coins coins;
        Dictionary<string, Tuple<uint, DateTime>> userCoins;

        public string[] emotes = { "FrankerZ", "OpieOP", "ResidentSleeper", "BibleThump" };

        public Slots(Coins _coins)
        {
            coins = _coins;
            userCoins = coins.userCoins;
        }

        public void PlaySlots(IrcClient irc, ReadMessage msg)
        {
            Tuple<uint, DateTime> values;

            double timedifference;

            if (userCoins.ContainsKey(msg.user))
            {
                values = userCoins[msg.user];
            }
            else
            {
                values = new Tuple<uint, DateTime>(irc.SlotsInitialCoins, DateTime.MinValue);
                userCoins.Add(msg.user, values);
            }

            timedifference = (DateTime.UtcNow - values.Item2).TotalSeconds;

            if (timedifference < irc.GamesDelay)
            {
                irc.sendChatMessage(msg.user + ": You have to wait " + (Math.Round((values.Item2 - DateTime.UtcNow).TotalSeconds + irc.GamesDelay, 2)).ToString() + " second(s).");
            }
            else
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                uint coinsBet = 0;
                if (uint.TryParse(helper[1], out coinsBet))
                {
                    if (coinsBet > values.Item1)
                    {
                        irc.sendChatMessage(msg.user + ": You don't have that many coins!");
                    }
                    else
                    {
                        int[] results = new int[3];
                        results[0] = rnd.Next(0, emotes.Length);
                        results[1] = rnd.Next(0, emotes.Length);
                        results[2] = rnd.Next(0, emotes.Length);
                        if (results[0] == results[1] && results[0] == results[2])
                        {
                            irc.sendChatMessage(msg.user + ": " + emotes[results[0]] + " , " + emotes[results[1]] + " , " + emotes[results[2]] + " - Congratulations, you win " + (coinsBet * 100).ToString() + " coin(s)!");
                            Tuple<uint, DateTime> newValues = new Tuple<uint, DateTime>(values.Item1 + coinsBet * 100, DateTime.Now);
                            userCoins[msg.user] = newValues;
                        }
                        else
                        {
                            irc.sendChatMessage(msg.user + ": " + emotes[results[0]] + " , " + emotes[results[1]] + " , " + emotes[results[2]] + " - you loose, " + coinsBet.ToString() + " coin(s)!");
                            Tuple<uint, DateTime> newValues = new Tuple<uint, DateTime>(values.Item1 - coinsBet, DateTime.Now);
                            userCoins[msg.user] = newValues;
                        }
                    }
                }
            }
        }
    }
}
