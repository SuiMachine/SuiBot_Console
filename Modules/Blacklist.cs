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

            //Start with
            blacklist_startswith.Add("░");

            //Ends with
            blacklist_endswith.Add("░");


            //Words (mostly URL shorteners)
            blacklist_words.Add("apo.af/".ToLower());
            blacklist_words.Add("bit.ly/".ToLower());
			blacklist_words.Add("goo.gl/".ToLower());
            blacklist_words.Add("ow.ly/".ToLower());
            blacklist_words.Add("tinyurl.com/".ToLower());
            blacklist_words.Add("t.co/".ToLower());
            blacklist_words.Add("shortr.org/".ToLower());
			blacklist_words.Add("sh.st/".ToLower());
            blacklist_words.Add("sh.st/".ToLower());
            blacklist_words.Add("bit.do/".ToLower());
            blacklist_words.Add("lnkd.in/".ToLower());
            blacklist_words.Add("db.tt/".ToLower());
            blacklist_words.Add("qr.ae/".ToLower());
            blacklist_words.Add("adf.ly/".ToLower());
            blacklist_words.Add("adf.ly/".ToLower());
            blacklist_words.Add("cur.lv/".ToLower());
            blacklist_words.Add("bitly.com/".ToLower());
            blacklist_words.Add("adcrun.ch/".ToLower());
            blacklist_words.Add("ity.im/".ToLower());
            blacklist_words.Add("q.gs/".ToLower());
            blacklist_words.Add("viralurl.com/".ToLower());
            blacklist_words.Add("is.gd/".ToLower());
            blacklist_words.Add("vur.me/".ToLower());
            blacklist_words.Add("bc.vc/".ToLower());
            blacklist_words.Add("twitthis.com/".ToLower());
            blacklist_words.Add("u.to/".ToLower());
            blacklist_words.Add("j.mp/".ToLower());
            blacklist_words.Add("buzurl.com/".ToLower());
            blacklist_words.Add("cutt.us/".ToLower());
            blacklist_words.Add("u.bb/".ToLower());
            blacklist_words.Add("yourls.org/".ToLower());
            blacklist_words.Add("x.co/".ToLower());
            blacklist_words.Add("adcraft.com/".ToLower());
            blacklist_words.Add("virl.ws/".ToLower());
            blacklist_words.Add("scrnch.me/".ToLower());
            blacklist_words.Add("1url.com/".ToLower());
            blacklist_words.Add("7vd.cn/".ToLower());
            blacklist_words.Add("dft.ba/".ToLower());
            blacklist_words.Add("aka.gr/".ToLower());
            blacklist_words.Add("tr.im/".ToLower());
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
