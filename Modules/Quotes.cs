using System;
using System.Collections.Generic;
using System.IO;

namespace TwitchBotConsole
{
    class Quotes
    {
        static string quotesfile = "quotes.txt";
        Random rnd = new Random();

        public List<string> quotelist = new List<string>();
        
        public void loadQuotesFromFile()
        {
            quotelist.Clear();
            if (!File.Exists(@quotesfile))
            {
                File.Create(@quotesfile);
            }
            StreamReader SR = new StreamReader(@quotesfile);
            string line = "";

            while ((line = SR.ReadLine()) != null)
            {
                if (line != "")
                    quotelist.Add(line);
            }
            SR.Close();
        }

        public void addQuote(oldIRCClient _irc, ReadMessage MSG)
        {
            if(_irc.moderators.Contains(MSG.user))
            {
                int personNameStart = MSG.message.IndexOf(' ');
                int quoteStart = MSG.message.IndexOf(':');
                if (personNameStart > 0 && quoteStart > 0)
                {
                    string[] helper = MSG.message.Split(new char[] { ' ' }, 2);
                    quotelist.Add(helper[1]);
                    saveQuotes();
                    _irc.sendChatMessage(quotelist[quotelist.Count - 1]);
                }
                else
                {
                    _irc.sendChatMessage("Invalid syntax");
                }
            }
            else
            {
                _irc.sendChatMessage("You don't have permissions to perform this command");
            }
        }

        public void getQuote(oldIRCClient _irc, string msg)
        {
            string[] helper = msg.Split(new char[]{' '}, 2);
            if(helper.Length >1)
            {
                int id = ResolveFilter(helper[1]);
                if (id == -1)
                {
                    _irc.sendChatMessage("No quote found");
                }
                else
                {
                    _irc.sendChatMessage(id.ToString() + ". " + quotelist[id]);
                }
            }
            else
            {
                if (quotelist.Count == 0)
                {
                    _irc.sendChatMessage("There are no quotes!");
                }
                else
                {
                    int ID = rnd.Next(0, quotelist.Count);
                    string[] quote = quotelist[ID].Split(new char[] { ':' }, 2);
                    _irc.sendChatMessage("\"" + quote[1] + "\" by " + quote[0]);
                }
            }
        }

        public void getQuotebyID(oldIRCClient _irc, ReadMessage msg)
        {
            int id = GetIDFromString(msg.message);

            if(id == -1)
            {
                _irc.sendChatMessage("Failed to convert ID. Syntax incorrect?");
            }
            else if(id>quotelist.Count-1)
            {
                _irc.sendChatMessage("Wrong ID (outside of a list)!");
            }
            else
            {
                string[] quote = quotelist[id].Split(new char[] { ':' }, 2);
                _irc.sendChatMessage("\"" + quote[1] + "\" by " + quote[0]);
            }
        }

        int ResolveFilter(string msg)
        {
            int id = 0;
            string[] helper = msg.Split(new char[] { ' ' }, 2);
            for(;id<quotelist.Count;id++)
            {
                if (quotelist[id].StartsWith(msg, StringComparison.InvariantCultureIgnoreCase))
                {
                    return id;
                }
            }
            return -1;
        }

        public void removeQuote(oldIRCClient irc, ReadMessage msg)
        {
            if(irc.moderators.Contains(msg.user))
            {
                int id = GetIDFromString(msg.message);
                if (id == -1)
                {
                    irc.sendChatMessage("Wrong syntax");
                }
                else if (id > quotelist.Count - 1)
                {
                    irc.sendChatMessage("Wrong id");
                }
                else
                {
                    irc.sendChatMessage("Removed (" + id.ToString() + "): " + quotelist[id]);
                    quotelist.RemoveAt(id);
                }
            }
            else
            {
                irc.sendChatMessage("You don't have permissions to perform this command");
            }
        }

        int GetIDFromString(string msg)
        {
            int id = 0;
            string[] helper = msg.Split(new char[] { ' ' }, 2);
            if (int.TryParse(helper[1], out id))
            {
                return id;
            }
            else
            {
                return -1;
            }
        }

        public void getNumberOfQuotes(oldIRCClient irc)
        {
            irc.sendChatMessage("Number of elements: " + quotelist.Count.ToString());
        }

        public void sortQuotes()
        {
            quotelist.Sort();
        }

        public void saveQuotes()
        {
            File.WriteAllLines(@quotesfile, quotelist);
        }
    }
}
