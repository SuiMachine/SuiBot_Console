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

        List<string> User = new List<string>();
        List<int> Coins = new List<int>();
        List<DateTime> LastPlayed = new List<DateTime>();
        public string[] emotes = { "FrankerZ", "OpieOP", "ResidentSleeper", "BibleThump" };

        public Slots()
        {
        }

        public void PlaySlots(IrcClient irc, ReadMessage msg)
        {
            DateTime lastPlayedCurrent = DateTime.UtcNow;
            int userID;
            double timedifference;

            if (User.Contains(msg.user))
            {
                userID = User.IndexOf(msg.user);
                lastPlayedCurrent = LastPlayed[userID];
            }
            else
            {
                lastPlayedCurrent = DateTime.MinValue;
                User.Add(msg.user);
                LastPlayed.Add(lastPlayedCurrent);
                Coins.Add(irc.SlotsInitialCoins);
                userID = User.IndexOf(msg.user);
            }

            timedifference = (DateTime.UtcNow - lastPlayedCurrent).TotalSeconds;

            if (timedifference < irc.SlotsDelay)
            {
                irc.sendChatMessage(msg.user + ": You have to wait " + (Math.Round((lastPlayedCurrent - DateTime.UtcNow).TotalSeconds + irc.SlotsDelay, 2)).ToString() + " second(s).");
            }
            else
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                int coinsBet = 0;
                if (int.TryParse(helper[1], out coinsBet))
                {
                    if (coinsBet > 0)
                    {
                        if (coinsBet > Coins[userID])
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
                                Coins[userID] = Coins[userID] + coinsBet * 100;
                            }
                            else
                            {
                                irc.sendChatMessage(msg.user + ": " + emotes[results[0]] + " , " + emotes[results[1]] + " , " + emotes[results[2]] + " - you loose, " + coinsBet.ToString() + " coin(s)!");
                                Coins[userID] = Coins[userID] - coinsBet;
                            }
                            LastPlayed[userID] = DateTime.UtcNow;
                            Trace.WriteLine("Setting new lastplayed at: " + LastPlayed[userID]);
                        }
                    }
                    else
                    {
                        irc.sendChatMessage(msg.user + ": The coins value has to be greater than 0!");
                    }
                }
            }
        }

        public void DisplayCoins(IrcClient irc, ReadMessage msg)
        {
            int userID;
            int coinsVal;
            if (User.Contains(msg.user))
            {
                userID = User.IndexOf(msg.user);
            }
            else
            {
                User.Add(msg.user);
                LastPlayed.Add(DateTime.MinValue);
                Coins.Add(irc.SlotsInitialCoins);
                userID = User.IndexOf(msg.user);
            }
            coinsVal = Coins[userID];
            irc.sendChatMessage(msg.user + ": You have " + coinsVal.ToString() + " coin(s).");
        }

        public void AddCoins(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 3);

                int userID;
                int coinsVal;
                if (int.TryParse(helper[2], out coinsVal))
                {
                    if (User.Contains(helper[1]))
                    {
                        userID = User.IndexOf(helper[1]);
                        Coins[userID] = Coins[userID] + coinsVal;
                        irc.sendChatMessage(msg.user + ": Added " + coinsVal.ToString() + " coin(s) to a user " + helper[1]);
                    }
                    else
                    {
                        irc.sendChatMessage("No user under this name found");
                    }
                }
                else
                {
                    irc.sendChatMessage("Failed to convert the coins value. Wrong syntax?");
                }
            }
            else
            {
                irc.sendChatMessage("You don't have permissions to perform this command");
            }

        }

        public void SaveSlots()
        {

        }
    }
}
