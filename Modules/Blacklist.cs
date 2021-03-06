﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace TwitchBotConsole
{
    class Blacklist
    {
        string filterFile = "filter.txt";
        string directory = "cache";
        string userInfoFile = "userinfo.txt";

        oldIRCClient irc;
        List<string> blacklist_fullphrase = new List<string>();
        List<string> blacklist_startswith = new List<string>();
        List<string> blacklist_endswith = new List<string>();
        List<string> blacklist_words = new List<string>();

        List<string> blacklist_ban_fullphrase = new List<string>();
        List<string> blacklist_ban_startswith = new List<string>();
        List<string> blacklist_ban_endswith = new List<string>();
        List<string> blacklist_ban_words = new List<string>();
        byte allowedToPostLinksRequirement = 3;

        string[] annoyingCharacters = new string[] { "ฏ๎", "Ỏ̷͖" };


        List<string> url_marks = new List<string>() { "http:", "www." , "https:", "ftp:", "ftps:", "sftp:", "steam:", "imap:", "file:"};
        Dictionary<string, byte> allowedToPostLinks = new Dictionary<string, byte>();

        public Blacklist(oldIRCClient _irc)
        {
            irc = _irc;
            if (File.Exists(filterFile))
            {
                loadBlacklist();
            }

            if (File.Exists(Path.Combine(directory, userInfoFile)))
            {
                loadUserInfo();
            }
        }

        public TimeOutReason checkForAnnoyingTrash(ReadMessage msg)
        {
            string message = msg.message;

            if (msg.user != String.Empty)
            {
                foreach(string element in annoyingCharacters)
                {
                    if (message.Contains(element))
                        return TimeOutReason.TrashCharacters;
                }

            }
            return TimeOutReason.NoPurge;
        }

        public TimeOutReason checkForSpam(ReadMessage msg)
        {
            string message = msg.message.ToLower();

            if(msg.user != String.Empty)
            {
                if (!allowedToPostLinks.ContainsKey(msg.user))
                {
                    allowedToPostLinks.Add(msg.user, 0);
                }

                if (allowedToPostLinks[msg.user] < allowedToPostLinksRequirement && isLink(message))
                {
                    return TimeOutReason.LinkBelowRequirement;
                }

                if (blacklist_fullphrase.Contains(message))
                    return TimeOutReason.PurgePhrase;
                else if (blacklist_startswith.Any(s => message.StartsWith(s)))
                    return TimeOutReason.PurgePhrase;
                else if (blacklist_endswith.Any(s => message.EndsWith(s)))
                    return TimeOutReason.PurgePhrase;
                else if (blacklist_words.Any(s => message.Contains(s)))
                    return TimeOutReason.PurgeWord;
                else
                {
                    if (allowedToPostLinks[msg.user] < allowedToPostLinksRequirement)
                        allowedToPostLinks[msg.user]++;
                    return TimeOutReason.NoPurge;
                }
            }
            return TimeOutReason.NoPurge;
        }

        public TimeOutReason checkForBanWorthyContent(ReadMessage msg)
        {
            string message = msg.message.ToLower();

            if (msg.user != String.Empty)
            {
                if (blacklist_ban_fullphrase.Contains(message))
                    return TimeOutReason.BannedPhrase;
                else if (blacklist_ban_startswith.Any(s => message.StartsWith(s)))
                    return TimeOutReason.BannedPhrase;
                else if (blacklist_ban_endswith.Any(s => message.EndsWith(s)))
                    return TimeOutReason.BannedPhrase;
                else if (blacklist_ban_words.Any(s => message.Contains(s)))
                    return TimeOutReason.BannedWord;
                else
                    return TimeOutReason.NoPurge;
            }
            return TimeOutReason.NoPurge;
        }

        private bool isLink(string message)
        {
            string[] helper = message.Replace("..", "+").Split(' ', '\"', '\\', '(', ')', '<', '>');   //probably need more
            foreach (string word in helper)
            {
                if (url_marks.Any(s => word.Contains(s)))
                {
                    return true;
                }
                else if (irc.filteringHarsh && word.Contains('.'))
                {
                    MatchCollection matches = Regex.Matches(word, @"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)", RegexOptions.IgnoreCase);  //this has been literally taken from the internet... because I'm dumb
                    if(matches.Count > 0)
                        return true;
                }

            }
            return false;
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
            bool isBanWorthy = false;

            while((line = SR.ReadLine()) != null)
            {
                if(line == "")
                {
                    Debug.WriteLine("Skipping");
                    continue;
                }
                else if (line == "#####PURGE#######")
                {
                    isBanWorthy = false;
                    Debug.WriteLine("Set to: Purge list");
                }
                else if (line == "#######BAN#######")
                {
                    isBanWorthy = true;
                    Debug.WriteLine("Set to: Ban list");
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
                            if (!isBanWorthy)
                                blacklist_fullphrase.Add(filter);
                            else
                                blacklist_fullphrase.Add(filter);
                            Debug.WriteLine("Adding: " + filter + " " + addingToVal.ToString());
                        }
                    }
                    else if(addingToVal == (byte)addingToEnum.startswith)
                    {
                        if (!blacklist_startswith.Contains(filter.ToLower()))
                        {
                            if (!isBanWorthy)
                                blacklist_startswith.Add(filter);
                            else
                                blacklist_ban_startswith.Add(filter);
                            Debug.WriteLine("Adding: " + filter + " " + addingToVal.ToString());
                        }
                    }
                    else if(addingToVal == (byte)addingToEnum.endswith)
                    {
                        if (!blacklist_endswith.Contains(filter.ToLower()))
                        {
                            if (!isBanWorthy)
                                blacklist_endswith.Add(filter);
                            else
                                blacklist_ban_endswith.Add(filter);
                            Debug.WriteLine("Adding: " + filter + " " + addingToVal.ToString());
                        }
                    }
                    else if(addingToVal == (byte)addingToEnum.word)
                    {
                        if (!blacklist_words.Contains(filter.ToLower()))
                        {
                            if (!isBanWorthy)
                                blacklist_words.Add(filter);
                            else
                                blacklist_ban_words.Add(filter);
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
                "#####PURGE#######" + "\n" +
                "####PHRASE########" + "\n" +
                getStringFromList(blacklist_fullphrase) +
                "\n" + "####STARTSWITH####" + "\n" +
                getStringFromList(blacklist_startswith) +
                "\n" + "####ENDSWITH######" + "\n" +
                getStringFromList(blacklist_endswith) +
                "\n" + "####WORDS#########" + "\n" +
                getStringFromList(blacklist_words) +
                "\n" + "#######BAN#######" + "\n" +
                "\n" + "####PHRASE########" + "\n" +
                getStringFromList(blacklist_ban_fullphrase) +
                "\n" + "####STARTSWITH####" + "\n" +
                getStringFromList(blacklist_ban_startswith) +
                "\n" + "####ENDSWITH######" + "\n" +
                getStringFromList(blacklist_ban_endswith) +
                "\n" + "####WORDS#########" + "\n" +
                getStringFromList(blacklist_ban_words); 


            File.WriteAllText(filterFile, output);
        }

        private void loadUserInfo()
        {
            StreamReader SR = new StreamReader(Path.Combine(directory, userInfoFile));
            string line = "";

            while ((line = SR.ReadLine()) != null)
            {
                if (line.Contains(':'))
                {
                    string[] helper = line.Split(':');
                    byte value = 0;
                    if (byte.TryParse(helper[1], out value))
                    {
                        allowedToPostLinks.Add(helper[0], value);
                    }
                }
            }
            SR.Close();
            SR.Dispose();
        }

        public void saveUserInfo()
        {
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllLines(Path.Combine(directory, userInfoFile), allowedToPostLinks.Select(x=> x.Key + ":" + x.Value.ToString()).ToArray());
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

        internal void addToAllowedToPostLinks(ReadMessage msg)
        {
            if(irc.moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(' ');
                if (helper.Length > 1)
                {
                    for (int i = 1; i < helper.Length; i++)
                    {
                        if(!helper[i].Any(char.IsPunctuation))
                        {
                            string name = helper[i].ToLower();
                            if(allowedToPostLinks.ContainsKey(name))                            
                                allowedToPostLinks[name] = allowedToPostLinksRequirement;       //if dictionary contains the user
                            else
                                allowedToPostLinks.Add(name, allowedToPostLinksRequirement);    //if it doesn't contain the user

                            irc.sendChatMessage("Set allowedToPostLinks for " + name + " to " + allowedToPostLinksRequirement);
                        }
                    }
                }
                else
                    irc.sendChatMessage("Invalid syntax!");
            }
        }

        internal void resetFromAllowedToPostLinks(ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(' ');
                if (helper.Length > 1)
                {
                    for (int i = 1; i < helper.Length; i++)
                    {
                        if (!helper[i].Any(char.IsPunctuation))
                        {
                            string name = helper[i].ToLower();
                            if (allowedToPostLinks.ContainsKey(name))
                            {
                                allowedToPostLinks[name] = 0;       //if dictionary contains the user
                                irc.sendChatMessage("Reseted requirement for " + name + " to 0");
                            }
                            else
                            {
                                irc.sendChatMessage(name + " not found in database.");
                            }
                        }
                    }
                }
                else
                    irc.sendChatMessage("Invalid syntax!");
            }
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
