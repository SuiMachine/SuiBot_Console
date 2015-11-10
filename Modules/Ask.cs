using System;
using System.Collections.Generic;

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
                    responsedToQuestion(irc, msg.user, helper[1]);
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
                        responsedToQuestion(irc, msg.user, helper[1]);
                    }
                }
            }
        }

        private void responsedToQuestion(IrcClient irc, string user, string question)
        {
            int id;
            string response;
            if ((response = uniqueQuestion(user, question)) != String.Empty)
            {
                irc.sendChatMessage(user + ": " + response);
            }
            else if (question.StartsWith("When", StringComparison.InvariantCultureIgnoreCase))
            {
                id = rnd.Next(0, AnswersTime.Length);
                irc.sendChatMessage(user + ": " + AnswersTime[id]);
            }
            else if (question.StartsWith("What", StringComparison.InvariantCultureIgnoreCase) || question.StartsWith("Wat", StringComparison.InvariantCultureIgnoreCase))
            {
                id = rnd.Next(0, AnswersItem.Length);
                irc.sendChatMessage(user + ": " + AnswersItem[id]);
            }
            else if (question.StartsWith("Who", StringComparison.InvariantCultureIgnoreCase))
            {
                id = rnd.Next(0, AnswersPerson.Length);
                irc.sendChatMessage(user + ": " + AnswersPerson[id]);
            }
            else if (question.StartsWith("Where", StringComparison.InvariantCultureIgnoreCase))
            {
                id = rnd.Next(0, AnswersPlace.Length);
                irc.sendChatMessage(user + ": " + AnswersPlace[id]);
            }
            else
            {
                id = rnd.Next(0, AnswersGeneric.Length);
                irc.sendChatMessage(user + ": " + AnswersGeneric[id]);
            }

            _ask[user] = new Tuple<DateTime, bool>(DateTime.UtcNow, false);
        }

        private string uniqueQuestion(string user, string question)
        {
            if (question.StartsWith("Who are you", StringComparison.InvariantCultureIgnoreCase) || question.StartsWith("Are you ", StringComparison.InvariantCultureIgnoreCase))
            {
                return "I'm a bot MrDestructoid";
            }
            else if(question.StartsWith("R ", StringComparison.InvariantCultureIgnoreCase) || question.StartsWith("UR ", StringComparison.InvariantCultureIgnoreCase) || question.StartsWith("U ", StringComparison.InvariantCultureIgnoreCase))
            {
                return "DansGame";
            }
            else if (question.StartsWith("Who", StringComparison.InvariantCultureIgnoreCase))     //Here are unique people (who)
            {
                string[] helper = question.ToLower().Split(new char[] { ' ', '?' });                //Split the words and check if one of them fits

                foreach (string word in helper)
                {                                                           //Remember words are lowercase only!
                    if(word == user)
                    {
                        return "It's you! OpieOP";
                    }
                    else if (word == "cosmo")
                    {
                        return "I really have no idea BibleThump";
                    }
                    else if (word == "sui" || word == "suicidemachine")
                    {
                        return "My father <3";
                    }
                    else if (word == "bunny" || word == "randompinkbunny")
                    {
                        return "Bunny is love.... so much love <3";
                    }
                    else if (word == "soul" || word == "soulgamingdude")
                    {
                        switch(rnd.Next(2))
                        {
                            case 0: return "а судьи кто? Kappa";
                            case 1: return "Some shitter Kappa";
                        }
                    }
                    else if (word == "heinki")
                    {
                        return "";
                    }
                    else if (word == "thekotti" || word == "kotti")
                    {
                        return "";
                    }
                    else if(word == "spat" || word == "seductivespatula")
                    {
                        return "";
                    }
                    else if(word == "epicdudeguy")
                    {
                        return "Can't you read? Epic... dude.... guy. Epic^^ FrankerZ";
                    }
                    else if (word == "chops" || word == "drtchops")
                    {
                        return "He is PJSalt ! Nothing but PJSalt !";
                    }
                    else if (word == "snowy" || word == "snowysnowwolf")
                    {
                        return "I don't know who Slowy is Kappa";
                    }
                }
                return String.Empty;
            }
			else if (question.StartsWith("What time is it", StringComparison.InvariantCultureIgnoreCase) || question.StartsWith("What's the time", StringComparison.InvariantCultureIgnoreCase))
            {
                DateTime utcTime = DateTime.UtcNow;

                return "It's: " + utcTime.AddHours(1).ToShortTimeString() + " (Central Europe), " + utcTime.AddHours(-5).ToShortTimeString() + "(Eastern Standard Time) or " + utcTime.AddHours(11).ToShortTimeString() + " (Aussy time) FrankerZ";
            }
            else if (question.StartsWith("What", StringComparison.InvariantCultureIgnoreCase) || question.StartsWith("Wat", StringComparison.InvariantCultureIgnoreCase))
            {
                string[] helper = question.ToLower().Split(new char[] { ' ', '?' });

                foreach (string word in helper)
                {
                    if(word== "address")
                    {
                        switch(rnd.Next(2))
                        {
                            case 0: return "It's 192.168.0." + rnd.Next(1, 256).ToString() + " Kappa";
                            case 1: return "What do you think I am? Skynet?! DansGame";
                        }
                    }
                    else if(word == "0451")
                    {
                        return "It is a reference to the book Fahrenheit 451. Its author - Ray Bradbury, asserted that \"book-paper\" will auto-ignite at the temperature higher than 451 degrees of fahrenheit* FrankerZ";
                    }
                    else if(word == "love")
                    {
                        switch(rnd.Next(5))
                        {
                            case 0: return "'Love' is making a shot to the knees of a target 120 kilometers away using an Aratech sniper rifle with a tri - light scope...Love is knowing your target, putting them in your targeting reticule, and together, achieving a singular purpose against statistically long odds. 4Head";
                            case 1: return "The triumph of imagination over intelligence.";
                            case 2: return "The irresistible desire to be irresistibly desired.";
                            case 3: return "\"Love is like an hourglass, with the heart filling up as the brain empties.\" - Jules Renard";
                            case 4: return "\"Love is an exploding cigar we willingly smoke.\" -  Lynda Barry";
                        }
                    }
                    else if (word == "cosmo")
                    {
                        return "I really have no idea BibleThump";
                    }
                }
                return String.Empty;
            }
            else
                return String.Empty;
        }
    }
}
