﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace TwitchBotConsole
{
    class Intervals
    {
        oldIRCClient irc;
        Json_status json;
        bool prevChannelOnline = false;
        string previousGame = "";
        List<int> time = new List<int>();
        List<int> srcTime = new List<int>();
        List<string> intervalMessage = new List<string>();
        static string intervalsFile = "interval_messages.txt";

        public Intervals()
        {
            if (File.Exists(@intervalsFile))
            {
                StreamReader SR = new StreamReader(@intervalsFile);
                string line = "";
                while((line = SR.ReadLine()) != null)
                {
                    if(line.Contains(':'))
                    {
                        string[] helper = line.Split(new char[] { ':' }, 2);
                        int interval;
                        if(int.TryParse(helper[0], out interval))
                        {
                            time.Add(interval);
                            srcTime.Add(interval);
                            intervalMessage.Add(helper[1]);
                            Console.WriteLine("Loaded interval message: \'" + helper[1] + "\' with a time of " + interval.ToString() + " minutes.\n");
                        }
                    }
                }
                SR.Close();
                SR.Dispose();
            }
        }

        internal void timerSender(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(json.isOnline)
            {
                for (int i = 0; i < time.Count; i++)
                {
                    time[i]--;
                    if (time[i] == 0)
                    {
                        irc.sendChatMessage(intervalMessage[i]);
                        time[i] = srcTime[i];
                    }
                }

                if(json.game != previousGame)
                {
                    //irc.sendChatMessage("The game is: " + json.game);
                    previousGame = json.game;
                }
            }
            else if(json.isOnline!= prevChannelOnline)
            {
                prevChannelOnline = json.isOnline;
            }
        }

        public void AddIntervalMessage(oldIRCClient _irc, ReadMessage msg)
        {
            if(_irc.moderators.Contains(msg.user))
            {
                string[] temp = msg.message.Split(new char[] { ' ' }, 2);
                string[] helper = temp[1].Split(new char[] { ':' }, 2);

                int intervalMin = -1;
                if(helper.Length == 2)
                {
                    if (int.TryParse(helper[0], out intervalMin))
                    {
                        time.Add(intervalMin);
                        srcTime.Add(intervalMin);
                        intervalMessage.Add(helper[1]);
                        _irc.sendChatMessage("\"" + helper[1] + "\" with interval of " + intervalMin.ToString() + " minute(s)");
                    }
                    else
                    {
                        _irc.sendChatMessage("Failed to convert the minutes. Wrong syntax?");
                    }
                }
                else
                {
                    _irc.sendChatMessage("Wrong syntax!! It's \"Number:Message\"");
                }
            }
            else
            {
                _irc.sendChatMessage(msg.user + ": Insufficient rights!");
            }
        }

        public void ShowIntervalMessageID(oldIRCClient _irc, ReadMessage msg)
        {
            string[] helper = msg.message.Split(new char[] { ' ' }, 2);
            int id = -1;
            if (helper.Length > 1 && int.TryParse(helper[1], out id))
            {
                if(id >= 0)
                {
                    if(id < intervalMessage.Count)
                    {
                        _irc.sendChatMessage("Message is:" +intervalMessage[id]);
                    }
                    else
                    {
                        _irc.sendChatMessage("Message doesn't exist.");
                    }
                }
            }
            else
            {
                _irc.sendChatMessage("Failed to convert ID");
            }
        }

        public void RemoveIntervalMessage(oldIRCClient _irc, ReadMessage msg)
        {
            if (_irc.moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);

                int id = -1;
                
                if (helper.Length > 1 && int.TryParse(helper[1], out id))
                {
                    if(id< time.Count && id< srcTime.Count &&id<intervalMessage.Count)
                    {
                        _irc.sendChatMessage("Interval message removed: " + intervalMessage[id]);
                        time.RemoveAt(id);
                        srcTime.RemoveAt(id);
                        intervalMessage.RemoveAt(id);
                    }
                    else
                    {
                        _irc.sendChatMessage("Wrong ID!");
                    }
                }
                else
                {
                    _irc.sendChatMessage("Failed to convert the minutes. Wrong syntax?");
                }
            }
            else
                _irc.sendChatMessage(msg.user + ": Insufficient rights!");
        }

        public void sendIrc(oldIRCClient _irc, Json_status _json)
        {
            irc = _irc;
            json = _json;
        }
    }
}
