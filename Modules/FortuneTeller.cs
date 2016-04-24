using System;
using System.Collections.Generic;

namespace TwitchBotConsole
{
    class FortuneTeller
    {
        //This is not a copy of Ask module.....
        //not at all
        Dictionary<string,Tuple<DateTime,bool>> _fortune = new Dictionary<string,Tuple<DateTime,bool>>();

        //...not at all....
        string[] Fortunes = {
            "You going to get a visitor at your door next week - DO NOT OPEN DOOR! It Jehovah Witness. They so annoying! DansGame",
            "Next time you get on plane - CHANGE SEAT TO EXIT ROW! This make sure you not sit next to big fatass. DansGame",
            "You going to go to fancy restaurant. You going to order snails. DO NOT EAT THEM! That disgusting! Snail very dirty! WutFace",
            "Here your lacky numbers - WRITE DOWN, I NOT REPEAT! Here go - 11, 17, 25, 93, 11, and uh... 62. FrankerZ",
            "You see orange cat on Tuesday - WHOA, THAT BAD! CALL DOCTOR! SoonerLater",
            "That guy you work with... yeah, he take all credit for your idea. DansGame",
			"Error 404 - Fortune not found!",
			"It is better to have loved and lost, then to have loved and gotten syphilis.",
			"Sorry, you no win this time, try again.",
			"You should not scratch yourself there",
			"Man who stand on toilet, high on pot.",
			"Man who buy drowned cat pay for wet pussy.",
			"You never going to score.",
			"Man who fart in church sit in own pew.",
			"Courtesy is contagious. So is Gonorrhea. FrankerZ",
			"Having sex is like playing bridge, If you don't have a good partner, you better have a good hand. SoonerLater",
			"Laugh and the world laughs with you. Cry and the world laughs at you.",
			"No one ever died from a broken heart. But a heart sliced from their chest while they looked on screaming? Not gonna lie. That's killed a couple people. FUNgineer",
			"You don't need a parachute to skydive. You need a parachute to skydive twice.",
			"That's what Ki said. FUNgineer",
			"You are not illiterate. 4Head",
			"You're never too old to learn something stupid.",
			"Lucky numers: 23, 34, 42, 69, 666.",
			"Time is an illusion. Lunchtime doubly so.",
			"Cardboard belt is a waist of paper.",
			"All Men Eat, but Fu Man Chu.",
			"The difference between an oral thermometer and a rectal thermometer is all a matter of taste.",
			"Light travels faster than sound. That is why some people look brilliant until you hear them speak."
        };


        Random rnd = new Random();

        public void FortuneTelling(IrcClient irc, ReadMessage msg)
        {
            DateTime lastSendMsg = DateTime.UtcNow;
            bool _notified = false;
            double timedifference;

            if(_fortune.ContainsKey(msg.user))
            {
                var temp = _fortune[msg.user];
                lastSendMsg = temp.Item1;
                _notified = temp.Item2;
            }
            else
            {
                lastSendMsg = DateTime.MinValue;
                _notified = false;
                _fortune.Add(msg.user, new Tuple<DateTime,bool>(lastSendMsg, _notified));         
            }

            if(irc.dynamicDelayCheck())
            {
                if (!irc.safeAskMode)
                {
                    timedifference = (DateTime.UtcNow - lastSendMsg).TotalSeconds;

                    if (timedifference < irc.GamesDelay)
                    {
                        if (!_notified)
                        {
                            var temp = _fortune[msg.user];
                            temp = new Tuple<DateTime, bool>(temp.Item1, true);
                            irc.sendChatMessage(msg.user + ": You have to wait " + (Math.Round((lastSendMsg - DateTime.UtcNow).TotalSeconds + irc.GamesDelay, 2)).ToString() + " second(s).");
                            _fortune[msg.user] = temp;
                        }
                    }
                    else
                    {
                        responsedToQuestion(irc, msg.user);
                    }
                }
                else
                {
                    if (irc.moderators.Contains(msg.user))
                    {
                        timedifference = (DateTime.UtcNow - lastSendMsg).TotalSeconds;

                        if (timedifference < irc.GamesDelay)
                        {
                            if (!_notified)
                            {
                                var temp = _fortune[msg.user];
                                temp = new Tuple<DateTime, bool>(temp.Item1, true);
                                irc.sendChatMessage(msg.user + ": You have to wait " + (Math.Round((lastSendMsg - DateTime.UtcNow).TotalSeconds + irc.GamesDelay, 2)).ToString() + " second(s).");
                                _fortune[msg.user] = temp;
                            }
                        }
                        else
                        {
                            responsedToQuestion(irc, msg.user);
                        }
                    }
                }
            }
        }

        private void responsedToQuestion(IrcClient irc, string user)
        {
            int id;
            id = rnd.Next(0, Fortunes.Length);
            irc.sendChatMessage(user + ": " + Fortunes[id]);

            _fortune[user] = new Tuple<DateTime, bool>(DateTime.UtcNow, false);
        }

        //OK, I might have copy pasted it
    }
}
