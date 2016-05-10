using System;
using System.Collections.Generic;

namespace TwitchBotConsole
{
    class Coins
    {
        public Dictionary<string, Tuple<uint, DateTime>> userCoins = new Dictionary<string, Tuple<uint, DateTime>>();


        public void DisplayCoins(oldIRCClient irc, ReadMessage msg)
        {
            if(irc.dynamicDelayCheck())
            {
                Tuple<uint, DateTime> values;

                if (userCoins.ContainsKey(msg.user))
                {
                    values = userCoins[msg.user];
                }
                else
                {
                    values = new Tuple<uint, DateTime>(irc.SlotsInitialCoins, DateTime.MinValue);
                    userCoins[msg.user] = values;
                }
                irc.sendChatMessage(msg.user + ": You have " + values.Item1.ToString() + " coin(s).");
            }
        }

        public void AddCoins(oldIRCClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 3);
                uint coinsVal;
                Tuple<uint, DateTime> values;

                if (uint.TryParse(helper[2], out coinsVal))
                {
                    if (userCoins.ContainsKey(helper[1].ToLower()))
                    {
                        values = userCoins[helper[1].ToLower()];
                        Tuple<uint, DateTime> newValues = new Tuple<uint, DateTime>(values.Item1 + coinsVal, values.Item2);
                        userCoins[helper[1].ToLower()] = newValues;
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
    }
}
