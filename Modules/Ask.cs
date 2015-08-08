using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotConsole
{
    class Ask
    {      
        string[] AnswersTime = { "Never!", "Soon... FrankerZ", "At nein o\'clock FrankerZ" };
        string[] AnswersGeneric = { "Yes", "No", "Maybe" };
        string[] AnswersItem = { "Wurst", "BrustWurst", "Wall", "Chair", "Toilet Paper", "Potato" };
        string[] AnswersPerson = { "Your mom FrankerZ", "Your dad FrankerZ" };
        string[] AnswersPlace = { "Here!", "There!", "At your home", "In your bed SoonerLater", "At ESA! ESA HYPE!!! PogChamp" };
        List<string> _AskUsernames = new List<string>();
        List<DateTime> _AskLastMassage = new List<DateTime>();
        List<bool> _AskNotified = new List<bool>();

        Random rnd = new Random(DateTime.UtcNow.Millisecond);

        public void answerAsk(IrcClient irc, ReadMessage msg)
        {
            int id;
            string[] helper = msg.message.Split(new char[] { ' ' }, 2);
            DateTime lastSendMsg = DateTime.UtcNow;
            int userID;
            bool _notified = false;
            double timedifference;

            if(_AskUsernames.Contains(msg.user))
            {
                userID = _AskUsernames.IndexOf(msg.user);
                lastSendMsg = _AskLastMassage[userID];
                _notified = _AskNotified[userID];            
            }
            else
            {
                lastSendMsg = DateTime.MinValue;
                _AskUsernames.Add(msg.user);
                _AskLastMassage.Add(lastSendMsg);
                _AskNotified.Add(false);
                userID = _AskUsernames.IndexOf(msg.user);            
            }

            if(!irc.safeAskMode)
            {

                timedifference = (DateTime.UtcNow - lastSendMsg).TotalSeconds;

                if (timedifference < irc.AskDelay)
                {
                    if (!_notified)
                    {
                        irc.sendChatMessage(msg.user + ": You have to wait " + (Math.Round((lastSendMsg - DateTime.UtcNow).TotalSeconds + irc.AskDelay, 2)).ToString() + " second(s).");
                        _AskNotified[userID] = true;
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
                    _AskLastMassage[userID] = DateTime.UtcNow;
                    _AskNotified[userID] = false;
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
                            irc.sendChatMessage(msg.user + ": You have to wait " + (Math.Round((lastSendMsg - DateTime.UtcNow).TotalSeconds + irc.AskDelay, 2)).ToString() + " second(s).");
                            _AskNotified[userID] = true;
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
                        _AskLastMassage[userID] = DateTime.UtcNow;
                        _AskNotified[userID] = false;
                    }
                }
            }
        }
    }
}
