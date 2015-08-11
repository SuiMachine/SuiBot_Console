using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotConsole
{
    class Blacklist
    {
        List<string> blacklist_fullphrase = new List<string>();
        List<string> blacklist_startswith = new List<string>();
        List<string> blacklist_endswith = new List<string>();
        List<string> blacklist_words = new List<string>();

        public Blacklist()
        {
            //Full phrases
            blacklist_fullphrase.Add("Hey has anyone been reading the whole 'Asian Masculinity' thing on Reddit?".ToLower());
            blacklist_fullphrase.Add("For Asian guys, have you seen the Asian Gamer Chicks subreddit?".ToLower());

            //StartsWith
            blacklist_startswith.Add("why don't you put a banner for goldmine? (http://goo.gl/".ToLower());

            //Words
            blacklist_words.Add("http://apo.af/".ToLower());
            blacklist_words.Add("http://www.apo.af/".ToLower());
            blacklist_words.Add("http://bit.ly/".ToLower());
            blacklist_words.Add("http://www.bit.ly/".ToLower());
			blacklist_words.Add("http://goo.gl/".ToLower());
			blacklist_words.Add("http://www.goo.gl/".ToLower());
        }
        
        public bool checkForSpam(string recievedmessage)
        {
            string message = recievedmessage.ToLower();
            if (blacklist_fullphrase.Contains(message))
                return true;
            else if (blacklist_startswith.Any(s => message.StartsWith(s)))
                return true;
            else if (blacklist_endswith.Any(s => message.EndsWith(s)))
                return true;
            else if (blacklist_words.Any(s => message.Contains(s)))
                return true;
            else
            {
                return false;
            }
        }

        private void loadBlacklist()
        {

        }

        private void saveBlacklist()
        {
 
        }
    }
}
