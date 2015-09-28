using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotConsole
{
    class Blacklist
    {
        string filterFile = "filter.txt";

        IrcClient irc;
        List<string> blacklist_fullphrase = new List<string>();
        List<string> blacklist_startswith = new List<string>();
        List<string> blacklist_endswith = new List<string>();
        List<string> blacklist_words = new List<string>();

        public Blacklist(IrcClient _irc)
        {
            irc = _irc;

            if (File.Exists(filterFile))
            {
                loadBlacklist();
            }
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

        enum addingToEnum : byte
        {
            phrase,
            startswith,
            endswith,
            word
        }

        private void loadBlacklist()
        {
            StreamReader SR = new StreamReader(filterFile);
            string line;

            byte addingToVal = 0;

            while((line = SR.ReadLine()) != null)
            {
                if(line == "")
                {
                    Debug.WriteLine("Skipping");
                    continue;
                }
                else if(line == "####PHRASE########")
                {
                    addingToVal = (byte)addingToEnum.phrase;
                    Debug.WriteLine("Set to: PHRASE");
                }
                else if(line == "####STARTSWITH####")
                {
                    addingToVal = (byte)addingToEnum.startswith;
                    Debug.WriteLine("Set to: STARTWITH");
                }
                else if (line == "####ENDSWITH######")
                {
                    addingToVal = (byte)addingToEnum.endswith;
                    Debug.WriteLine("Set to: ENDSWITH");
                }
                else if(line == "####WORDS#########")
                {
                    addingToVal = (byte)addingToEnum.word;
                    Debug.WriteLine("Set to: WORDS");
                }
                else if(line.StartsWith(":"))
                {
                    string filter = line.Substring(1, line.Length - 1);

                    if(addingToVal == (byte)addingToEnum.phrase)
                    {
                        if(!blacklist_fullphrase.Contains(filter.ToLower()))
                        {
                            blacklist_fullphrase.Add(filter);
                            Debug.WriteLine("Adding: " + filter + " " + addingToVal.ToString());
                        }
                    }
                    else if(addingToVal == (byte)addingToEnum.startswith)
                    {
                        if (!blacklist_startswith.Contains(filter.ToLower()))
                        {
                            blacklist_startswith.Add(filter);
                            Debug.WriteLine("Adding: " + filter + " " + addingToVal.ToString());
                        }
                    }
                    else if(addingToVal == (byte)addingToEnum.endswith)
                    {
                        if (!blacklist_endswith.Contains(filter.ToLower()))
                        {
                            blacklist_endswith.Add(filter);
                            Debug.WriteLine("Adding: " + filter + " " + addingToVal.ToString());
                        }
                    }
                    else if(addingToVal == (byte)addingToEnum.word)
                    {
                        if (!blacklist_words.Contains(filter.ToLower()))
                        {
                            blacklist_words.Add(filter);
                            Debug.WriteLine("Adding: " + filter + " " +addingToVal.ToString());
                        }
                    }
                    line = "";
                }
            }
            SR.Close();
            SR.Dispose();
        }

        private void saveBlacklist()
        {
            string output =
                "####PHRASE########" + "\n" +
                getStringFromList(blacklist_fullphrase) +
                "\n" + "####STARTSWITH####" + "\n" +
                getStringFromList(blacklist_startswith) +
                "\n" + "####ENDSWITH######" + "\n" +
                getStringFromList(blacklist_endswith) +
                "\n" + "####WORDS#########" + "\n" +
                getStringFromList(blacklist_words);

            File.WriteAllText(filterFile, output);
        }

        private string getStringFromList(List<string> ListWithElements)
        {
            StringBuilder text = new StringBuilder();
            text.Clear();
            foreach(string element in ListWithElements)
            {
                text.Append(":").Append(element).Append("\n");
            }

            return text.ToString();
        }

        internal void AddFilter(ReadMessage msg)
        {
            if(irc.moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                if(helper[1].Length > 3)
                {
                    string tempFilter = helper[1].Substring(1, helper[1].Length - 2).ToLower();
                    if (helper[1].StartsWith("[") && helper[1].EndsWith("]"))
                    {
                        if (!blacklist_fullphrase.Contains(tempFilter))
                        {
                            blacklist_fullphrase.Add(tempFilter);
                            irc.sendChatMessage("Added new full phrase filter: " + tempFilter);
                            saveBlacklist();
                        }
                        else
                        {
                            irc.sendChatMessage("Such full phrase filter already exists!");
                        }
                    }
                    else if (helper[1].StartsWith("*") && helper[1].EndsWith("]"))
                    {
                        if (!blacklist_startswith.Contains(tempFilter))
                        {
                            blacklist_startswith.Add(tempFilter);
                            irc.sendChatMessage("Added new 'starts with' filter: " + tempFilter);
                            saveBlacklist();
                        }
                        else
                        {
                            irc.sendChatMessage("Such 'starts with' filter already exists!");
                        }
                    }
                    else if (helper[1].StartsWith("[") && helper[1].EndsWith("*"))
                    {
                        if (!blacklist_endswith.Contains(tempFilter))
                        {
                            blacklist_endswith.Add(tempFilter);
                            irc.sendChatMessage("Added new 'ends with' filter: " + tempFilter);
                            saveBlacklist();
                        }
                        else
                        {
                            irc.sendChatMessage("Such 'ends with' filter already exists!");
                        }
                    }
                    else if (helper[1].StartsWith("*") && helper[1].EndsWith("*"))
                    {
                        string filter = helper[1].Substring(1, helper[1].Length - 2).ToLower();
                        if (!blacklist_words.Contains(filter))
                        {
                            blacklist_words.Add(filter);
                            irc.sendChatMessage("Added new word filter: " + filter);
                            saveBlacklist();
                        }
                        else
                        {
                            irc.sendChatMessage("Such word filter already exists!");
                        }
                    }
                    else
                        irc.sendChatMessage("Failed to add a new filter. Wrong syntax?");
                }
            }
        }

        internal void RemoveFilter(ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);

                if (helper[1].StartsWith("[") && helper[1].EndsWith("]"))
                {
                    string filter = helper[1].Substring(1, helper[1].Length - 2).ToLower();
                    if (blacklist_fullphrase.Contains(filter))
                    {
                        blacklist_fullphrase.Remove(filter);
                        irc.sendChatMessage("Removed full phrase filter: " + filter);
                        saveBlacklist();
                    }
                    else
                    {
                        irc.sendChatMessage("No filter found");
                    }
                }
                else if (helper[1].StartsWith("*") && helper[1].EndsWith("]"))
                {
                    string filter = helper[1].Substring(1, helper[1].Length - 2).ToLower();
                    if (blacklist_startswith.Contains(filter))
                    {
                        blacklist_startswith.Remove(filter);
                        irc.sendChatMessage("Removed 'starts with' filter: " + filter);
                        saveBlacklist();
                    }
                    else
                    {
                        irc.sendChatMessage("No filter found");
                    }
                }
                else if (helper[1].StartsWith("[") && helper[1].EndsWith("*"))
                {
                    string filter = helper[1].Substring(1, helper[1].Length - 2).ToLower();
                    if (blacklist_endswith.Contains(filter))
                    {
                        blacklist_endswith.Remove(filter);
                        irc.sendChatMessage("Removed 'ends with' filter: " + filter);
                        saveBlacklist();
                    }
                    else
                    {
                        irc.sendChatMessage("No filter found");
                    }
                }
                else if (helper[1].StartsWith("*") && helper[1].EndsWith("*"))
                {
                    string filter = helper[1].Substring(1, helper[1].Length - 2).ToLower();
                    if (blacklist_words.Contains(filter))
                    {
                        blacklist_words.Remove(filter);
                        irc.sendChatMessage("Removed word filter: " + filter);
                        saveBlacklist();
                    }
                    else
                    {
                        irc.sendChatMessage("No filter found");
                    }
                }
                else
                    irc.sendChatMessage("Wrong syntax?");
            }
        }
    }
}
