using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace TwitchBotConsole
{
    class CustomCvars
    {
        public Dictionary<string, string> cvarslist = new Dictionary<string, string>();                         //Super mess in a memory! Shouldn't matter with like 100 cmds anyway.
        public Dictionary<string, string> restrictedCvars = new Dictionary<string, string>();
        static string cvarsFile = "cvars.txt";

        public CustomCvars()
        {
            loadCvarsFromFiles();
        }

        public void cvarPerform(oldIRCClient irc, ReadMessage msg)
        {
            if(msg.message.Length>2)
            {
                string helper = msg.message.Substring(1, msg.message.Length - 1).ToLower();

                if (cvarslist.ContainsKey(helper))
                {
                    string message;
                    cvarslist.TryGetValue(helper, out message);
                    irc.sendChatMessage(message);
                }
                else if (irc.moderators.Contains(msg.user) && restrictedCvars.ContainsKey(helper))
                {
                    string message;
                    restrictedCvars.TryGetValue(helper, out message);
                    irc.sendChatMessage(message);
                }
            }
        }

        public void addCustomCvar(oldIRCClient irc, ReadMessage msg)
        {
            if(irc.moderators.Contains(msg.user))
            {
                string[] start = msg.message.Split(new char[] { ' ' }, 2);
                int indexOfColon = start[1].IndexOf(':');
                int indexOfComa = start[1].IndexOf(',');

                if(indexOfColon> 0)
                {
                    if(indexOfComa > 0 && indexOfComa < indexOfColon)
                    {
                        int numberOfCvars = 1;                                                          //The beginning of it, is making sure program gets the part before the first Colon.
                        for (int i = 0; i < indexOfColon; i++)                                          //Example: You have bitch,tits,hits:This,message
                        {                                                                               //So here, we make sure we get:
                            if(start[1].ElementAt(i) == ',')                                            //helper[0] = bitch
                            {                                                                           //helper[1] = tits
                                numberOfCvars++;                                                        //helper[2] = hits:This,message
                            }
                        }
                        string[] helper = start[1].Split(new char[]{','}, numberOfCvars);
                        string[] lastPart = helper[helper.Length - 1].Split(new char[]{':'}, 2);        //Split out "hits:This,message" to:
                        for (int i = 0; i < helper.Length - 1; i++)                                     //lastpart[0] = hits
                        {                                                                               //lastpart[1] = This,message
                            cvarslist.Add(helper[i].ToLower(), lastPart[1]);                            //And finally add everything to Dictionary.
                        }
                        cvarslist.Add(lastPart[0].ToLower(), lastPart[1]);

                        irc.sendChatMessage("Custom cvar added!");
                    }
                    else
                    {
                        string[] lastPart = start[1].Split(new char[] { ':' }, 2);
                        cvarslist.Add(lastPart[0].ToLower(), lastPart[1]);

                        irc.sendChatMessage("Custom cvar added!");
                    }
                }
                else
                    irc.sendChatMessage("Failed to add custom cvar. Invalid syntax?");
                saveCvarsToFile();
            }
        }

        public void showCustomCvars(oldIRCClient irc, ReadMessage msg)
        {            
             irc.sendChatMessage("Commands are: " + string.Join(", ", cvarslist.Keys.ToArray()));
        }

        public void removeCustomCvar(oldIRCClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                string[] start = msg.message.Split(new char[] { ' ' }, 2);
                if (start[1].IndexOf(':') > 0)
                {
                    string[] helper = start[1].Split(new char[] { ':' }, 2);
                    if (helper[1].ToLower() == "ALL".ToLower())
                    {
                        string message;
                        string commandsRemoved = "";
                        cvarslist.TryGetValue(helper[0], out message);
                        foreach (var item in cvarslist.Where(kvp => kvp.Value == message).ToList())
                        {
                            commandsRemoved = commandsRemoved + item.Key + ", ";
                            cvarslist.Remove(item.Key);
                        }

                        irc.sendChatMessage("Custom cvars removed: " + commandsRemoved);
                    }
                    else if (helper[1].ToLower() == "SINGLE".ToLower())
                    {
                        cvarslist.Remove(helper[1]);
                        irc.sendChatMessage("Custom cvar removed!");
                    }
                    else
                        irc.sendChatMessage("Wrong syntax!");
                }
                else
                {
                    cvarslist.Remove(start[1]);

                    irc.sendChatMessage("Custom cvar removed!");
                }
            }
            saveCvarsToFile();
        }

        #region SaveLoadFunctions
        private void loadCvarsFromFiles()
        {
			if(File.Exists(@cvarsFile))
			{
				StreamReader SR = new StreamReader(@cvarsFile);
				string line = "";
				while((line = SR.ReadLine()) != null)
				{
					if (line.StartsWith("[["))
					{
						int starts = line.IndexOf("[[") + 2;
						int ends = line.IndexOf("]]");
						string cutLine = line.Substring(starts, ends - starts);
						if(cutLine.IndexOf(":") > 0)
						{
							string[] helper = cutLine.Split(new char[] { ':' }, 2);
							cvarslist.Add(helper[0], helper[1]);
						}
						else
							Console.WriteLine("Failed to read custom cvar from a file, skipping");
					}
					else if (line.StartsWith("[<"))
					{
						int starts = line.IndexOf("[<") + 2;
						int ends = line.IndexOf(">]");
						string cutLine = line.Substring(starts, ends - starts);
						if (cutLine.IndexOf(":") > 0)
						{
							string[] helper = cutLine.Split(new char[] { ':' }, 2);
							restrictedCvars.Add(helper[0], helper[1]);
						}
						else
							Console.WriteLine("Failed to read custom cvar from a file, skipping");
					}
				}
                SR.Close();
                SR.Dispose();
			}
        }

        private void saveCvarsToFile()
        {
            string[] outputCvars = cvarslist.Select(x => "[[" + x.Key + ":" + x.Value + "]]").ToArray();
            string[] outputRestricted = restrictedCvars.Select(x => "[<" + x.Key + ":" + x.Value + ">]").ToArray();
            outputRestricted.CopyTo(outputCvars, outputCvars.Length);
            File.WriteAllLines(cvarsFile, outputCvars);
        }
        #endregion
    }
}
