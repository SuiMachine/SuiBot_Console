using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Timers;

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
        public bool checkForUpdates = true;
        static string configfile = "config.cfg";
        static string ignoredfile = "ignored_users.txt";
        static string trustedfile = "trusted_users.txt";
        static string deathSave = "deaths.txt";
        public string SpeedrunName = "";
        public config _config;
        public bool configFileExisted = false;
        public double GamesDelay = 30.0d;
        public uint SlotsInitialCoins = 100;
        public bool quoteEnabled = true;
        public bool safeAskMode = true;
        public bool filteringEnabled = true;
        public bool filteringHarsh = true;
        public bool filteringRespond = false;
        public bool slotsEnable = false;
        public bool intervalMessagesEnabled = true;
        public bool deathCounterEnabled = false;
        public uint deaths = 0;
        public uint delayBetweenAddedDeaths = 10;
        public bool leaderBoardEnabled = false;
        public bool vocalMode = true;
        public bool voteEnabled = true;
        public bool breakPyramids = true;
        public bool ConnectedStatus = true;
        public bool viewerPBActive = true;
        public bool adjustGamesDelayBasedOnChatActivity = true;
        public bool disableFunctionsWithHighlyActiveChat = false;

        public int amountOfCharactersLastMinute = 0;
        public int prevamountOfCharactersLastMinute = 0;

        public List<string> supermod = new List<string>();
        public List<string> moderators = new List<string>();
        public List<string> ignorelist = new List<string>();
        public List<string> trustedUsers = new List<string>();

        private string userName;
        private string channel;
        ReadMessage FormattedMessage;

        private Socket clientSocket;
        private NetworkStream networkStream;
        private StreamReader inputStream;
        private StreamWriter outputStream;
        DateTime LastSend;
        DateTime DeathLastAdded;

        #region Constructor
        public IrcClient()
        {
            if (!File.Exists(@configfile))
            {
                LoadExampleConfig();
            }
            else
            {
                configFileExisted = true;
                loadConfig();
                loadDeaths();

                if(loading_status)
                {
                    this.userName = _config.username;

                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    clientSocket.Connect(_config.server, _config.port);

                    networkStream = new NetworkStream(clientSocket);
                    inputStream = new StreamReader(networkStream);
                    outputStream = new StreamWriter(networkStream);


                    outputStream.WriteLine("PASS " + _config.password);
                    outputStream.WriteLine("NICK " + userName);
                    outputStream.WriteLine("USER " + userName + " 8 * :" + userName);
                    outputStream.Flush();

                    System.Timers.Timer checkConnection = new System.Timers.Timer();
                    checkConnection.Enabled = true;
                    checkConnection.Interval = 60*1000;
                    checkConnection.Elapsed += CheckConnection_Elapsed;
                    checkConnection.Start();
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

        private void CheckConnection_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool result = SocketConnected(clientSocket);
            if (result)
            {
                Console.WriteLine("RUNNING! Amount of characters last minute: " + amountOfCharactersLastMinute.ToString());
                prevamountOfCharactersLastMinute = amountOfCharactersLastMinute;
                amountOfCharactersLastMinute = 0;
            }
            else
            {
                Console.WriteLine("CONNECTION LOST!");
            }

        }

        private bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            Console.WriteLine("Part 1: " + part1);
            bool part2 = (s.Available == 0);
            Console.WriteLine("Part 2: " + part2);

            return true;
        }
        #endregion

        #region BasicFunctions
        public void joinRoom(string channel)
        {
            this.channel = channel;
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
            if (SpeedrunName == String.Empty)
                SpeedrunName = channel;
        }

        public void sendIrcRawMessage(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();
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
            amountOfCharactersLastMinute += FormattedMessage.message.Length;
            return FormattedMessage;
        }

        public string readRawMessage()
        {
            string message = inputStream.ReadLine();
            return message;
        }

        public void sendChatMessage(string message)
        {
            double timeSpan = (DateTime.UtcNow - LastSend).Seconds;
            int timeSpanInt = Convert.ToInt32(timeSpan * 1000);
            if (timeSpanInt < 2000)
            {
                Trace.WriteLine("Sleeping for" + (2000 - timeSpanInt).ToString());
                Thread.Sleep(2000 - timeSpanInt);
            }
            LastSend = DateTime.UtcNow;
            amountOfCharactersLastMinute += message.Length;
            sendIrcRawMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public void sendChatMessage_NoDelays(string message)
        {
            amountOfCharactersLastMinute += message.Length;
            sendIrcRawMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public void purgeMessage(string user)
        {
            sendIrcRawMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :.timeout " + user + " 1");
            System.Threading.Thread.Sleep(100);
            sendIrcRawMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :.timeout " + user + " 1");
            System.Threading.Thread.Sleep(100);
            sendIrcRawMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :.timeout " + user + " 1");
            LastSend = DateTime.UtcNow;
            Console.WriteLine("Purging: " + user);
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

        public void saveTrustedList()
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

        internal void version()
        {
            sendChatMessage("Bot's version is: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
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
            string output = "Server:" + _config.server + "\nPort:" + _config.port.ToString() + "\nUsername:" + _config.username + "\nPassword:" + _config.password + "\nChannel:" + _config.channel + "\nAutoUpdates:" + checkForUpdates.ToString() + "\n\n";
            for (int i = 0; i < supermod.Count; i++)
            {
                output = output + "\nSuperMod:" + supermod[i];
            }
            output += "\n\nPhraseFiltering:" + filteringEnabled.ToString();
            output += "\nQuotesEnabled:" + quoteEnabled.ToString();
            output += "\nSafeAskMode:" + safeAskMode.ToString();
            output += "\nSlotsEnabled:" + slotsEnable.ToString();
            output += "\nIntervalMessagesEnabled:" + intervalMessagesEnabled.ToString();

            File.WriteAllText(@configfile, output);
        }

        public void SaveConfig()
        {
            string output = "Server:" + _config.server + "\nPort:" + _config.port.ToString() + "\nUsername:" + _config.username + "\nPassword:" + _config.password + "\nChannel:" + _config.channel + "\nAutoUpdates:" + checkForUpdates.ToString() +"\nSpeedrunName:" + SpeedrunName +"\n";
            for (int i = 0; i < supermod.Count; i++)
            {
                output = output + "\nSuperMod:" + supermod[i];
            }
            output = output + "\n\nAdjustGamesDelayBasedOnChatActivity:" + adjustGamesDelayBasedOnChatActivity.ToString()+
                "\nDisableFunctionsWithHighlyActiveChat:" + disableFunctionsWithHighlyActiveChat.ToString()
                + "\n\nVocalMode:" + vocalMode.ToString()
                + "\nPhraseFiltering:" + filteringEnabled.ToString()
                + "\nFilteringHarsh:" + filteringHarsh.ToString()
                + "\nFilteringResponse:" + filteringRespond.ToString()
                + "\nQuotesEnabled:" + quoteEnabled.ToString()
                + "\nSafeAskMode:" + safeAskMode.ToString()
                + "\nGamesDelay:" + GamesDelay.ToString()
                + "\nSlotsEnabled:" + slotsEnable.ToString()
                + "\nIntervalMessagesEnabled:" + intervalMessagesEnabled.ToString()
                + "\nDeathCounterEnabled:" + deathCounterEnabled.ToString()
                + "\nDeathCounterSafetyDelay:" + delayBetweenAddedDeaths.ToString()
                + "\nViewerPBEnabled:" + viewerPBActive.ToString()
                + "\nLeaderboardEnabled:" + leaderBoardEnabled.ToString()
                + "\nVotesEnabled:" + voteEnabled.ToString();

            File.WriteAllText(@configfile, output);
        }

        internal void DeathCounterAdd(ReadMessage formattedMessage)
        {
            if(moderators.Contains(formattedMessage.user) || trustedUsers.Contains(formattedMessage.user))
            {
                if((DateTime.UtcNow-DeathLastAdded).TotalSeconds > delayBetweenAddedDeaths)
                {
                    if (formattedMessage.message.StartsWith("!deathAdd "))
                    {
                        string[] helper = formattedMessage.message.Split(new char[] { ' ' }, 2);
                        uint temp;
                        if(uint.TryParse(helper[1], out temp))
                        {
                            deaths = deaths + temp;
                            sendChatMessage("Added " + temp.ToString() + " deaths. Current number of deaths: " + deaths.ToString());
                            DeathLastAdded = DateTime.UtcNow;
                            saveDeaths();
                        }
                        else
                            sendChatMessage(FormattedMessage.user + ": Invalid syntax?");
                    }
                    else
                    {
                        deaths++;
                        sendChatMessage("Added 1 death. Current number of deaths: " + deaths.ToString());
                        DeathLastAdded = DateTime.UtcNow;
                        saveDeaths();
                    }
                }
                else
                {
                    sendChatMessage("Ignored adding a death (for safety).");
                }
            }
        }

        internal void updateSpeedrunName(ReadMessage msg)
        {
            string[] helper = msg.message.Split(new char[] { ' ' }, 2);
            if (helper[1] != "")
            {
                SpeedrunName = helper[1];
                sendChatMessage("Speedrun.com name for a streamer set to: " + helper[1]);
                SaveConfig();
            }
        }

        internal void DeathCounterDisplay(ReadMessage formattedMessage)
        {
            sendChatMessage("Current number of deaths: " + deaths.ToString());
        }

        internal void DeathCounterRemove(ReadMessage formattedMessage)
        {
            if (moderators.Contains(formattedMessage.user) || trustedUsers.Contains(formattedMessage.user))
            {
                if (formattedMessage.message.StartsWith("!deathRemove "))
                {
                    string[] helper = formattedMessage.message.Split(new char[] { ' ' }, 2);
                    uint temp;
                    if (uint.TryParse(helper[1], out temp))
                    {
                        if (deaths < temp)
                        {
                            sendChatMessage("Value is higher than the current number of deaths!");
                        }
                        else
                        {
                            deaths = deaths - temp;
                            sendChatMessage("Removed " + temp.ToString() + " deaths. Current number of deaths: " + deaths.ToString());
                            saveDeaths();
                        }
                    }
                    else
                        sendChatMessage(FormattedMessage.user + ": Invalid syntax?");
                }
                else
                {
                    deaths--;
                    sendChatMessage("Removed 1 death. Current number of deaths: " + deaths.ToString());
                    saveDeaths();
                }
            }
        }

        public void loadConfig()
        {
            bool LoadedProperly = true;
            StreamReader SR = new StreamReader(@configfile);
            string line = "";
            while ((line = SR.ReadLine()) != null)
            {
                bool tempBool;
                int tempInt;
                double tempDouble;
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
                else if (line.StartsWith("SpeedrunName:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    if (helper[1] != "")
                    {
                        SpeedrunName = helper[1];
                    }
                    else
                    {
                        SpeedrunName = "";
                    }
                }
                else if (line.StartsWith("SuperMod:"))
                {
                    string[] helper = line.Split(new char[] { ':' }, 2);
                    if (helper[1] != "")
                    {
                        supermod.Add(helper[1].ToLower());
                        moderators.Add(helper[1].ToLower());
                    }
                    else
                    {
                        Console.WriteLine("SuperMod string was empty");
                    }
                }   //Because this part was getting stupidly long, I wrote a functions to parse these and made it all fit on one screen
                else if (_configParseBool(line, "AutoUpdates:", true, out tempBool)) checkForUpdates = tempBool;
                else if (_configParseBool(line, "VocalMode:", false, out tempBool)) vocalMode = tempBool;
                else if (_configParseBool(line, "PhraseFiltering:", false, out tempBool)) filteringEnabled = tempBool;
                else if (_configParseBool(line, "FilteringHarsh:", false, out tempBool)) filteringHarsh = tempBool;
                else if (_configParseBool(line, "FilteringResponse:", false, out tempBool)) filteringRespond = tempBool;
                else if (_configParseBool(line, "QuotesEnabled:", false, out tempBool)) quoteEnabled = tempBool;
                else if (_configParseBool(line, "SafeAskMode:", true, out tempBool)) safeAskMode = tempBool;
                else if (_configParseBool(line, "SlotsEnabled:", false, out tempBool)) slotsEnable = tempBool;
                else if (_configParseBool(line, "IntervalMessagesEnabled:", false, out tempBool)) intervalMessagesEnabled = tempBool;
                else if (_configParseBool(line, "DeathCounterEnabled:", false, out tempBool)) deathCounterEnabled = tempBool;
                else if (_configParseBool(line, "ViewerPBEnabled:", false, out tempBool)) viewerPBActive = tempBool;
                else if (_configParseBool(line, "LeaderboardEnabled:", false, out tempBool)) leaderBoardEnabled = tempBool;
                else if (_configParseBool(line, "VotesEnabled:", false, out tempBool)) voteEnabled = tempBool;
                else if (_configParseBool(line, "AdjustGamesDelayBasedOnChatActivity:", true, out tempBool)) adjustGamesDelayBasedOnChatActivity = tempBool;
                else if (_configParseBool(line, "DisableFunctionsWithHighlyActiveChat:", false, out tempBool)) disableFunctionsWithHighlyActiveChat = tempBool;
                else if (_configParseDouble(line, "GamesDelay:", 30d, out tempDouble)) GamesDelay = tempDouble;
                else if (_configParseInt(line, "DeathCounterSafetyDelay:", 10, out tempInt)) delayBetweenAddedDeaths = (uint)tempInt;
            }
            SR.Close();
            SR.Dispose();

            loading_status = LoadedProperly;
        }

        private void loadDeaths()
        {
            if(File.Exists(@deathSave))
            {
                StreamReader SR = new StreamReader(@deathSave);
                string line;
                while ((line = SR.ReadLine()) != null)
                {
                    if(line.StartsWith("Deaths:"))
                    {
                        uint temp;
                        string[] helper = line.Split(new char[] { ':' }, 2);
                        if(uint.TryParse(helper[1], out temp))
                        {
                            deaths = temp;
                        }
                    }
                }
                SR.Close();
                SR.Dispose();
            }
        }

        private void saveDeaths()
        {
            string output = "Deaths:" + deaths.ToString();
            File.WriteAllText(@deathSave, output);
        }

        public bool dynamicDelayCheck()
        {
            int data;
            if (prevamountOfCharactersLastMinute > amountOfCharactersLastMinute)
                data = prevamountOfCharactersLastMinute;
            else
                data = amountOfCharactersLastMinute;

            if (disableFunctionsWithHighlyActiveChat && data > 475)
            {
                return false;
            }
            else if(data >200)
            {
                int datatemp = data - 200;
                GamesDelay = 0.003 * data * data + 30;
                return true;
            }
            else
            {
                GamesDelay = 30.0d;
                return true;
            }
        }

        #region customParseFunctions
        private bool _configParseBool(string readLine, string lookingFor, bool defaultValue, out bool value)
        {
            if (readLine.StartsWith(lookingFor, StringComparison.InvariantCultureIgnoreCase))
            {
                string[] helper = readLine.Split(new char[] { ':' }, 2);
                if (bool.TryParse(helper[1], out value))
                {
                    return true;
                }
                else
                {
                    value = defaultValue;
                    return true;
                }
            }
            else
            {
                value = true;
                return false;
            }
        }

        private bool _configParseInt(string readLine, string lookingFor, int defaultValue, out int value)
        {
            if (readLine.StartsWith(lookingFor, StringComparison.InvariantCultureIgnoreCase))
            {
                string[] helper = readLine.Split(new char[] { ':' }, 2);
                if (int.TryParse(helper[1], out value))
                {
                    return true;
                }
                else
                {
                    value = defaultValue;
                    return true;
                }
            }
            else
            {
                value = 0;
                return false;
            }
        }

        private bool _configParseDouble(string readLine, string lookingFor, double defaultValue, out double value)
        {
            if (readLine.StartsWith(lookingFor, StringComparison.InvariantCultureIgnoreCase))
            {
                string[] helper = readLine.Split(new char[] { ':' }, 2);
                if (double.TryParse(helper[1], out value))
                {
                    return true;
                }
                else
                {
                    value = defaultValue;
                    return true;
                }
            }
            else
            {
                value = 0;
                return false;
            }
        }
        #endregion
    }
}
