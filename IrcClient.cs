using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace TwitchBotConsole
{
    struct config
    {
        public string server;
        public int port;
        public string username;
        public string password;
        public string channel;
    }

    struct ReadMessage
    {
        public string user;
        public string message;
    }

    class IrcClient
    {
        bool loading_status = true;
        static string configfile = "config.cfg";
        static string ignoredfile = "ignored_users.txt";
        static string trustedfile = "trusted_users.txt";
        public config _config;
        public bool configFileExisted = false;
        public double AskDelay = 30.0d;
        public double SlotsDelay = 30.0d;
        public uint SlotsInitialCoins = 100;
        public bool quoteEnabled = true;
        public bool safeAskMode = true;
        public bool filteringEnabled = true;
        public bool slotsEnable = true;
        public bool intervalMessagesEnabled = true;

        public List<string> supermod = new List<string>();
        public List<string> moderators = new List<string>();
        public List<string> ignorelist = new List<string>();
        public List<string> trustedUsers = new List<string>();

        private string userName;
        private string channel;
        ReadMessage FormattedMessage;

        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;
        DateTime LastSend;

        #region Constructor
        public IrcClient()
        {
            if(!File.Exists(@configfile))
            {
                LoadExampleConfig();
            }
            else
            {
                configFileExisted = true;
                loadConfig();

                if(loading_status)
                {
                    this.userName = _config.username;

                    tcpClient = new TcpClient(_config.server, _config.port);
                    inputStream = new StreamReader(tcpClient.GetStream());
                    outputStream = new StreamWriter(tcpClient.GetStream());

                    outputStream.WriteLine("PASS " + _config.password);
                    outputStream.WriteLine("NICK " + userName);
                    outputStream.WriteLine("USER " + userName + " 8 * :" + userName);
                    outputStream.Flush();
                }
                else
                {
                    configFileExisted = false;
                    LoadExampleConfig();
                }
            }

            loadIgnoredList();
            loadTrustedList();
            
        }
        #endregion

        #region BasicFunctions
        public void joinRoom(string channel)
        {
            this.channel = channel;
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
        }

        public void sendIrcRawMessage(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();
        }

        public void sendChatMessage(string message)
        {
            double timeSpan = (DateTime.UtcNow - LastSend).Seconds;
            int timeSpanInt = Convert.ToInt32(timeSpan*1000);
            if (timeSpanInt < 2000)
            {
                Trace.WriteLine("Sleeping for" + (2000 - timeSpanInt).ToString());
                Thread.Sleep(2000 - timeSpanInt);
            }
            LastSend = DateTime.UtcNow;

            sendIrcRawMessage(":" + userName + "!" + userName + "@" +userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public void sendChatMessage_NoDelays(string message)
        {
            sendIrcRawMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public ReadMessage readMessage(string input_message)
        {
            FormattedMessage.user = "";
            FormattedMessage.message = "";
            string message = input_message.Remove(0, 1);
            int nicknameStarts = message.IndexOf('!', 1);
            int nicknameEnd = message.IndexOf('@', 1);
            int startsAt = message.IndexOf(':', 1) +1;
            if(nicknameStarts > 0 && nicknameEnd > 0)
            {
                FormattedMessage.user = message.Substring(nicknameStarts+1, nicknameEnd-1 - nicknameStarts).Trim();
            }
            if (startsAt> 0)
            {
                FormattedMessage.message = message.Substring(startsAt);
            }
            return FormattedMessage;
        }

        public string readRawMessage()
        {
            string message = inputStream.ReadLine();
            return message;
        }

        public void temp(string user)
        {
            sendIrcRawMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :.mod " + user);
        }

        public void purgeMessage(string user)
        {
            sendIrcRawMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :.timeout " +user + " 1");
        }

        public void timeOutMessage(string user, int time)
        {
            sendIrcRawMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :.timeout " + user + " " + time.ToString());
        }
        public void banMessage(string user)
        {
            sendIrcRawMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :.ban " + user);
        }
        #endregion

        #region trustedUsers
        public void trustedUserAdd(ReadMessage msg)
        {
            if (moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                if (!moderators.Contains(helper[1].ToLower()))
                {
                    if (!trustedUsers.Contains(helper[1].ToLower()))
                    {
                        trustedUsers.Add(helper[1].ToLower());
                        saveTrustedList();
                        sendChatMessage("Added " + helper[1] + " to trusted list.");
                    }
                    else
                    {
                        sendChatMessage(helper[1] + " is already on trusted list.");
                    }
                }
            }
        }

        public void trustedUsersRemove(ReadMessage msg)
        {
            if (moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);

                if (trustedUsers.Contains(helper[1].ToLower()))
                {
                    trustedUsers.Remove(helper[1].ToLower());
                    saveTrustedList();
                    sendChatMessage("Removed " + helper[1] + " from trusted list.");
                }
                else
                {
                    sendChatMessage(helper[1] + " is not present on trusted list.");
                }
            }
        }

        private void loadTrustedList()
        {
            trustedUsers.Clear();
            if (File.Exists(@trustedfile))
            {
                StreamReader SR = new StreamReader(@trustedfile);
                string line = "";

                while ((line = SR.ReadLine()) != null)
                {
                    if (line != "")
                        trustedUsers.Add(line.ToLower());
                }
                SR.Close();
                SR.Dispose();
            } 
        }

        private void saveTrustedList()
        {
            File.WriteAllLines(trustedfile, trustedUsers);
        }
        #endregion

        #region IgnoredList
        public void ignoreListAdd(ReadMessage msg)
        {
            if(moderators.Contains(msg.user))
            {                                
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                if (!moderators.Contains(helper[1].ToLower()))
                {
                    if (!ignorelist.Contains(helper[1].ToLower()))
                    {
                        ignorelist.Add(helper[1].ToLower());
                        saveIgnoredList();
                        sendChatMessage("Added " + helper[1] + " to ignored list.");
                    }
                    else
                    {
                        sendChatMessage(helper[1] + " is already on ignored list.");
                    }
                }
                else
                    sendChatMessage("Moderators can't be added to ignored list!");

            }
        }

        public void ignoreListRemove(ReadMessage msg)
        {
            if (moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);

                    if (ignorelist.Contains(helper[1].ToLower()))
                    {
                        ignorelist.Remove(helper[1].ToLower());
                        saveIgnoredList();
                        sendChatMessage("Removed " + helper[1] + " from ignored list.");
                    }
                    else
                    {
                        sendChatMessage(helper[1] + " is not present on ignored list.");
                    }
            }
        }

        public void loadIgnoredList()
        {
            ignorelist.Clear();
            if(File.Exists(@ignoredfile))
            {
                StreamReader SR = new StreamReader(@ignoredfile);
                string line = "";

                while ((line = SR.ReadLine()) != null)
                {
                    if (line != "")
                        ignorelist.Add(line.ToLower());
                }
                SR.Close();
                SR.Dispose();
            } 
        }

        public void saveIgnoredList()
        {
            File.WriteAllLines(@ignoredfile, ignorelist);
        }
        #endregion

        public void LoadExampleConfig()
        {
            _config.server = "irc.twitch.tv";
            _config.port = 6667;
            _config.username = "Your Username";
            _config.password = "Auth Password";
            _config.channel = "Your channel";
            string output = "Server:" + _config.server + "\nPort:" + _config.port.ToString() + "\nUsername:" + _config.username + "\nPassword:" + _config.password + "\nChannel:" + _config.channel +"\n\n";
            for (int i = 0; i < supermod.Count; i++)
            {
                output = output + "\nSuperMod:" + supermod[i];
            }
            output = output + "\n\nPhraseFiltering:" + filteringEnabled.ToString();
            output = output + "\nQuotesEnabled:" + quoteEnabled.ToString();
            output = output + "\nSafeAskMode:" + safeAskMode.ToString();
            output = output + "\nSlotsEnabled:" + slotsEnable.ToString();
            output = output + "\nIntervalMessagesEnabled:" + intervalMessagesEnabled.ToString();

            File.WriteAllText(@configfile, output);
        }

        public void SaveConfig()
        {
            string output = "Server:" + _config.server + "\nPort:" + _config.port.ToString() + "\nUsername:" + _config.username + "\nPassword:" + _config.password + "\nChannel:" + _config.channel + "\n\n";
            for (int i = 0; i < supermod.Count; i++)
            {
                output = output + "\nSuperMod:" + supermod[i];
            }
            output = output + "\n\nPhraseFiltering:" + filteringEnabled.ToString();
            output = output + "\nQuotesEnabled:" + quoteEnabled.ToString();
            output = output + "\nSafeAskMode:" + safeAskMode.ToString();
            output = output + "\nSlotsEnabled:" + slotsEnable.ToString();
            output = output + "\nIntervalMessagesEnabled:" + intervalMessagesEnabled.ToString();

            File.WriteAllText(@configfile, output);
        }

        public void loadConfig()
        {
            bool LoadedProperly = true;
            StreamReader SR = new StreamReader(@configfile);
            string line = "";
            while ((line = SR.ReadLine()) != null)
            {
                if (line.StartsWith("Server:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    if (helper[1] == "")
                        LoadedProperly = false;
                    else
                        _config.server = helper[1];
                }
                else if (line.StartsWith("Port:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    int port = 0;
                    if (int.TryParse(helper[1], out port))
                    {
                        if (port > 0)
                            _config.port = port;
                        else
                            LoadedProperly = false;
                    }
                    else
                        LoadedProperly = false;
                }
                else if (line.StartsWith("Username:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    if (helper[1] == "" || helper[1].ToLower() == "your username")
                    {
                        LoadedProperly = false;
                    }
                    else
                        _config.username = helper[1].ToLower();
                }
                else if (line.StartsWith("Password:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    if (helper[1] == "" || helper[1] == "Auth Password")
                    {
                        LoadedProperly = false;
                    }
                    else
                        _config.password = helper[1];

                }
                else if (line.StartsWith("Channel:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    if (helper[1] == "" || helper[1].ToLower() == "your channel")
                    {
                        LoadedProperly = false;
                    }
                    else
                        _config.channel = helper[1].ToLower();
                }
                else if(line.StartsWith("SuperMod:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    if (helper[1] != "")
                    {
                        if (!supermod.Contains(helper[1]))
                            supermod.Add(helper[1].ToLower());
                        if (!moderators.Contains(helper[1]))
                            moderators.Add(helper[1].ToLower());
                    }
                }
                else if (line.StartsWith("PhraseFiltering:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    if (helper[1] != "")
                    {
                        bool loadedBool;
                        if(bool.TryParse(helper[1], out loadedBool))
                        {
                            filteringEnabled = loadedBool;
                        }
                        else
                        {
                            filteringEnabled = false;
                        }
                    }
                }
                else if (line.StartsWith("QuotesEnabled:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    if (helper[1] != "")
                    {
                        bool loadedBool;
                        if (bool.TryParse(helper[1], out loadedBool))
                        {
                            quoteEnabled = loadedBool;
                        }
                        else
                        {
                            quoteEnabled = false;
                        }
                    }
                }
                else if (line.StartsWith("SafeAskMode:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    if (helper[1] != "")
                    {
                        bool loadedBool;
                        if (bool.TryParse(helper[1], out loadedBool))
                        {
                            safeAskMode = loadedBool;
                            
                        }
                        else
                        {
                            safeAskMode = true;
                        }
                    }
                }
                else if (line.StartsWith("SlotsEnabled:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    if (helper[1] != "")
                    {
                        bool loadedBool;
                        if (bool.TryParse(helper[1], out loadedBool))
                        {
                            slotsEnable = loadedBool;
                        }
                        else
                        {
                            slotsEnable = false;
                        }
                    }
                }
                else if (line.StartsWith("IntervalMessagesEnabled:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    if (helper[1] != "")
                    {
                        bool loadedBool;
                        if (bool.TryParse(helper[1], out loadedBool))
                        {
                            intervalMessagesEnabled = loadedBool;

                        }
                        else
                        {
                            intervalMessagesEnabled = false;
                        }
                    }
                }
            }
            Trace.WriteLine("Filtering: " + filteringEnabled.ToString());
            Trace.WriteLine("Safe ask mode: " + safeAskMode.ToString());
            Trace.WriteLine("Quotes: " + quoteEnabled.ToString());
            Trace.WriteLine("Slots: " + slotsEnable.ToString());
            Trace.WriteLine("Interval messages: " + intervalMessagesEnabled.ToString());
            SR.Close();
            SR.Dispose();

            loading_status = LoadedProperly;
        }
    }
}
