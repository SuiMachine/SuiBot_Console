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
            "At nein o\'clock FrankerZ",
            "Only on a monday FrankerZ"
        };

        string[] AnswersGeneric = {
            "Yes",
            "No",
            "Maybe",
            "Yes. Are you glad? FrankerZ",
            "I don't know, I'm a bot FrankerZ",
            "Who knows FrankerZ",
            "You really shouldn't ask such a stupid question Kappa"
        };

        string[] AnswersItem = {
            "Wurst",
            "BrustWurst",
            "Wall",
            "Chair",
            "Toilet Paper",
            "Potato",
            "A chair",
            "Nokia OSsloth",
            "A miserable little pile of secrets!"
        };

        string[] AnswersPerson = {
            "Your mom FrankerZ",
            "Your dad FrankerZ",
            "Hitler WutFace",
            "The cutest person in the world FrankerZ",
            "One sexy beast! ;)",
            "Just a \"nobody\"",
            "JOHN MADDEN SwiftRage",
			"JOHN CENA!!!!!! SwiftRage"
        };

        string[] AnswersPlace = {
            "Here!",
            "There!",
            "At your home",
            "In your bed SoonerLater",
            "On top of Eiffel tower OpieOP",
            "At ESA! ESA HYPE!!! PogChamp",
            "On Facebook OpieOP",
            "In the cage in your basement DOOMGuy"
        };


        Random rnd = new Random();

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
