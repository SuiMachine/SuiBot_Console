using System;
using System.Collections.Generic;

namespace TwitchBotConsole
{
    class Bet
    {
        Coins coins;
        List<string> results = new List<string>();
        Dictionary<string, Tuple<uint, DateTime>> userCoins;
        Dictionary<string, Tuple<uint,int>> betDataValue = new Dictionary<string, Tuple<uint,int>>();        //User, Amount of coins, number
        Dictionary<string, Tuple<uint, uint, byte, float>> betDataTime = new Dictionary<string, Tuple<uint, uint, byte, float>>();        //User, Amount of coins, hours, min, seconds

        enum eBetType
        {
            Value,
            Time
        }

        byte betType=(byte)eBetType.Value;

        bool betRunning = false;
        bool betEnded = false;
        bool resultsCalculated = false;
        string objective = "";

        public Bet(Coins _coins)
        {
            coins = _coins;
            userCoins = coins.userCoins;
        }

        #region ModeratorFunctions
        public void callBet(IrcClient irc, ReadMessage msg)
        {
            if(irc.moderators.Contains(msg.user))
            {
                if (!betRunning)
                {
                    string[] message = msg.message.Split(new char[] { ' ' }, 2);

                    betDataValue.Clear();
                    betDataTime.Clear();
                    betEnded = false;

                    if (message[1] != String.Empty)
                    {
                        objective = message[1];
                        irc.sendChatMessage("New bet (Int Value): " + objective);
                    }
                }
                else
                    irc.sendChatMessage("A bet is already running. Close the bet first!");
            }
        }

        public void setBetType(IrcClient irc, ReadMessage msg)
        {
            if(irc.moderators.Contains(msg.user))
            {
                if(!betRunning)
                {
                    string[] message = msg.message.Split(new char[] { ' ' }, 2);

                    if (message[1] != String.Empty)
                    {
                        if(message[1].StartsWith("Time", StringComparison.InvariantCultureIgnoreCase))
                        {
                            betType = (byte)eBetType.Time;
                            betEnded = false;
                            results.Clear();
                            betDataTime.Clear();
                            betDataValue.Clear();
                            irc.sendChatMessage("Betting type set to: Time");
                        }
                        else if(message[1].StartsWith("Number", StringComparison.InvariantCultureIgnoreCase) || message[1].StartsWith("Value", StringComparison.InvariantCultureIgnoreCase)|| message[1].StartsWith("default", StringComparison.InvariantCultureIgnoreCase))
                        {
                            betType = (byte)eBetType.Value;
                            betEnded = false;
                            results.Clear();
                            betDataTime.Clear();
                            betDataValue.Clear();
                            irc.sendChatMessage("Betting type set to: Number (Int)");
                        }
                        else
                        {
                            irc.sendChatMessage("No changes were made");
                        }

                    }
                    else
                        irc.sendChatMessage("Empty string? Should be either a 'number' or 'time'");

                }
                else
                    irc.sendChatMessage("A bet is already running. Close the bet first!");
            }
        }

        public void openBet(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                if (!betRunning)
                {
                    betRunning = true;
                    irc.sendChatMessage("Bets are now opened.");
                }
                else
                    irc.sendChatMessage("A bet is already running.");
            }
        }

        public void closeBet(IrcClient irc, ReadMessage msg)
        {
            if(irc.moderators.Contains(msg.user))
            {
                if (betRunning)
                {
                    betRunning = false;
                    irc.sendChatMessage("Bets are now closed.");
                }
                else
                    irc.sendChatMessage("A bet isn't currently running.");
            }
        }

        public void betAnswer(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                if(!betRunning)
                {
                    if(betType == (byte)eBetType.Value)
                    {
                        string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                        if (helper[1] != String.Empty)
                        {
                            int answer = 0;
                            if (int.TryParse(helper[1], out answer))
                            {
                                betEnded = true;
                                irc.sendChatMessage("Answer is: " + answer.ToString() + ". If this is correct, do \"!betWinners \"" );
                            }
                            else
                                irc.sendChatMessage("Failed to parse the answer. Has to be Int value");
                        }
                        else
                            irc.sendChatMessage("No answer provided!");

                    }
                    else if(betType == (byte)eBetType.Time)
                    {

                    }
                }
                else
                {
                    irc.sendChatMessage("A bet is currently running.");
                }
            }
        }

        public void getWinners(IrcClient irc, ReadMessage msg)
        {
            if(betEnded)
            {
                if(!resultsCalculated)
                {

                }
            }
            else
            {

            }
        }
        #endregion

        public void playerBets(IrcClient irc, ReadMessage msg)
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


            if(betType == (byte)eBetType.Value)
            {
                timedifference = (DateTime.UtcNow - values.Item2).TotalSeconds;

                if (timedifference < irc.GamesDelay)
                {
                    irc.sendChatMessage(msg.user + ": You have to wait " + (Math.Round((values.Item2 - DateTime.UtcNow).TotalSeconds + irc.GamesDelay, 2)).ToString() + " second(s).");
                }
                else
                {
                    if (msg.message.IndexOf(' ') > 0 && msg.message.IndexOf(':') > 0)
                    {
                        int betOn;
                        uint valueCoins;
                        string[] message = msg.message.Split(new char[] { ' ' }, 2);
                        string[] helper = message[1].Split(new char[] { ':' }, 2);
                        if (uint.TryParse(helper[0], out valueCoins) && int.TryParse(helper[1], out betOn))
                        {
                            if (values.Item1 > valueCoins)
                            {
                                Tuple<uint, int> userBet = new Tuple<uint, int>(valueCoins, betOn);
                                betDataValue.Add(msg.user, userBet);
                                Tuple<uint, DateTime> newValues = new Tuple<uint, DateTime>(values.Item1 - valueCoins, DateTime.Now);
                                userCoins[msg.user] = newValues;
                                irc.sendChatMessage(msg.user + ": You've bet " + valueCoins.ToString() + " coin(s) on " + betOn.ToString());
                            }
                            else
                                irc.sendChatMessage(msg.user + ": You don't have enough coins!");
                        }
                        else
                            irc.sendChatMessage(msg.user + ": Failed to convert values. The syntax is: !bet 'amount of coins':'value you bet on'");
                    }
                }
            }
        }
    }
}
