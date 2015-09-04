﻿using System;
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

        Dictionary<string, Tuple<uint, DateTime>> userCoins = new Dictionary<string, Tuple<uint, DateTime>>();

        public string[] emotes = { "FrankerZ", "OpieOP", "ResidentSleeper", "BibleThump" };

        public Slots()
        {
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

            if (timedifference < irc.SlotsDelay)
            {
                irc.sendChatMessage(msg.user + ": You have to wait " + (Math.Round((values.Item2 - DateTime.UtcNow).TotalSeconds + irc.SlotsDelay, 2)).ToString() + " second(s).");
            }
            else
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                uint coinsBet = 0;
                if (uint.TryParse(helper[1], out coinsBet))
                {
                    if (coinsBet > 0)
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
                                Tuple<uint, DateTime> newValues = new Tuple<uint, DateTime>(values.Item1+coinsBet*100, DateTime.Now);
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
                    else
                    {
                        irc.sendChatMessage(msg.user + ": The coins value has to be greater than 0!");
                    }
                }
            }
        }

        public void DisplayCoins(IrcClient irc, ReadMessage msg)
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

        public void AddCoins(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 3);
                uint coinsVal;
                Tuple<uint, DateTime> values;

                if (uint.TryParse(helper[2], out coinsVal))
                {
                    if(userCoins.ContainsKey(helper[1].ToLower()))
                    {
                        values = userCoins[helper[1].ToLower()];
                        Tuple<uint, DateTime> newValues = new Tuple<uint, DateTime>(values.Item1+coinsVal, values.Item2);
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

        public void SaveSlots()
        {

        }
    }
}
