using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotConsole
{
    class Ask
    {
        Dictionary<string,Tuple<DateTime,bool>> _ask = new Dictionary<string,Tuple<DateTime,bool>>();

        string[] AnswersTime = {
            "Never!",
            "Soon... FrankerZ",
            "At nein o\'clock FrankerZ"
        };

        string[] AnswersGeneric = {
            "Yes",
            "No",
            "Maybe" };

        string[] AnswersItem = {
            "Wurst",
            "BrustWurst",
            "Wall",
            "Chair",
            "Toilet Paper",
            "Potato",
            "A chair"
        };

        string[] AnswersPerson = {
            "Your mom FrankerZ",
            "Your dad FrankerZ"
        };
        string[] AnswersPlace = {
            "Here!",
            "There!",
            "At your home",
            "In your bed SoonerLater",
            "At ESA! ESA HYPE!!! PogChamp"
        };


        Random rnd = new Random(DateTime.UtcNow.Millisecond);

        public void answerAsk(IrcClient irc, ReadMessage msg)
        {
            int id;
            string[] helper = msg.message.Split(new char[] { ' ' }, 2);
            DateTime lastSendMsg = DateTime.UtcNow;
            bool _notified = false;
            double timedifference;

            if(_ask.ContainsKey(msg.user))
            {
                var temp = _ask[msg.user];
                lastSendMsg = temp.Item1;
                _notified = temp.Item2;
            }
            else
            {
                lastSendMsg = DateTime.MinValue;
                _notified = false;
                _ask.Add(msg.user, new Tuple<DateTime,bool>(lastSendMsg, _notified));         
            }

            if(!irc.safeAskMode)
            {
                timedifference = (DateTime.UtcNow - lastSendMsg).TotalSeconds;

                if (timedifference < irc.AskDelay)
                {
                    if (!_notified)
                    {
                        var temp = _ask[msg.user];
                        temp = new Tuple<DateTime, bool>(temp.Item1, true);
                        irc.sendChatMessage(msg.user + ": You have to wait " + (Math.Round((lastSendMsg - DateTime.UtcNow).TotalSeconds + irc.AskDelay, 2)).ToString() + " second(s).");
                        _ask[msg.user] = temp;
                    }
                }
                else
                {
                    if (helper[1].StartsWith("When", StringComparison.InvariantCultureIgnoreCase))
                    {
                        id = rnd.Next(0, AnswersTime.Length);
                        irc.sendChatMessage(msg.user + ": " + AnswersTime[id]);
                    }
                    else if (helper[1].StartsWith("What", StringComparison.InvariantCultureIgnoreCase))
                    {
                        id = rnd.Next(0, AnswersItem.Length);
                        irc.sendChatMessage(msg.user + ": " + AnswersItem[id]);
                    }
                    else if (helper[1].StartsWith("Who", StringComparison.InvariantCultureIgnoreCase))
                    {
                        id = rnd.Next(0, AnswersPerson.Length);
                        irc.sendChatMessage(msg.user + ": " + AnswersPerson[id]);
                    }
                    else if (helper[1].StartsWith("Where", StringComparison.InvariantCultureIgnoreCase))
                    {
                        id = rnd.Next(0, AnswersPlace.Length);
                        irc.sendChatMessage(msg.user + ": " + AnswersPlace[id]);
                    }
                    else
                    {
                        id = rnd.Next(0, AnswersGeneric.Length);
                        irc.sendChatMessage(msg.user + ": " + AnswersGeneric[id]);
                    }

                    _ask[msg.user] = new Tuple<DateTime, bool>(DateTime.UtcNow, false);
                }
            }
            else
            {
                if(irc.moderators.Contains(msg.user))
                {
                    timedifference = (DateTime.UtcNow - lastSendMsg).TotalSeconds;

                    if (timedifference < irc.AskDelay)
                    {
                        if (!_notified)
                        {
                            var temp = _ask[msg.user];
                            temp = new Tuple<DateTime, bool>(temp.Item1, true);
                            irc.sendChatMessage(msg.user + ": You have to wait " + (Math.Round((lastSendMsg - DateTime.UtcNow).TotalSeconds + irc.AskDelay, 2)).ToString() + " second(s).");
                            _ask[msg.user] = temp;
                        }
                    }
                    else
                    {
                        if (helper[1].StartsWith("When", StringComparison.InvariantCultureIgnoreCase))
                        {
                            id = rnd.Next(0, AnswersTime.Length);
                            irc.sendChatMessage(msg.user + ": " + AnswersTime[id]);
                        }
                        else if (helper[1].StartsWith("What", StringComparison.InvariantCultureIgnoreCase) || helper[1].StartsWith("Wat", StringComparison.InvariantCultureIgnoreCase))
                        {
                            id = rnd.Next(0, AnswersItem.Length);
                            irc.sendChatMessage(msg.user + ": " + AnswersItem[id]);
                        }
                        else if (helper[1].StartsWith("Who", StringComparison.InvariantCultureIgnoreCase))
                        {
                            id = rnd.Next(0, AnswersPerson.Length);
                            irc.sendChatMessage(msg.user + ": " + AnswersPerson[id]);
                        }
                        else if (helper[1].StartsWith("Where", StringComparison.InvariantCultureIgnoreCase))
                        {
                            id = rnd.Next(0, AnswersPlace.Length);
                            irc.sendChatMessage(msg.user + ": " + AnswersPlace[id]);
                        }
                        else
                        {
                            id = rnd.Next(0, AnswersGeneric.Length);
                            irc.sendChatMessage(msg.user + ": " + AnswersGeneric[id]);
                        }

                        _ask[msg.user] = new Tuple<DateTime, bool>(DateTime.UtcNow, false);
                    }
                }
            }
        }
    }
}
