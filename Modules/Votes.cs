using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotConsole
{
    class Votes
    {
        string voteObjective = "";
        Dictionary<string, uint> userVoted = new Dictionary<string, uint>();
        List<string> voteOptions = new List<string>();
        bool voteActive = false;

        public void callVote(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                if (!voteActive)
                {
                    voteObjective = helper[1];
                    irc.sendChatMessage("Vote object set to: '" + voteObjective + "'.");
                    userVoted.Clear();
                    voteOptions.Clear();
                }
                else
                {
                    irc.sendChatMessage("A vote is currently active. Please close the vote, first.");
                }
            }
        }

        public void setOptions(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                voteOptions.Clear();
                string[] starter = msg.message.Split(new char[] { ' ' }, 2);
                var count = starter[1].Count(x => x == ':');
                if (count > 0)
                {
                    string[] voteOptions = starter[1].Split(':');
                    for (int i = 0; i < voteOptions.Length; i++)
                    {
                        this.voteOptions.Add(voteOptions[i]);
                    }
                }
                else
                {
                    irc.sendChatMessage("There have to be at least 2 options!");
                }
            }
        }

        public void Vote(IrcClient irc, ReadMessage msg)
        {
            if (voteActive)
            {
                if (!userVoted.ContainsKey(msg.user))
                {
                    uint value;
                    string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                    if (uint.TryParse(helper[1], out value))
                    {
                        value--;
                        if (value >= 0 && value < voteOptions.Count)
                        {
                            userVoted.Add(msg.user, value);
                            Console.WriteLine("User " + msg.user + " voted for " + (value + 1).ToString());
                        }
                    }
                }
                else
                {
                    uint value;
                    string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                    if (uint.TryParse(helper[1], out value))
                    {
                        value--;
                        if (value >= 0 && value < voteOptions.Count)
                        {
                            userVoted[msg.user] = value;
                            Console.WriteLine("User " + msg.user + " changed his vote to: " + (value + 1).ToString());
                        }
                    }
                }
            }
            else
                irc.sendChatMessage("There is no currently active vote!");
        }

        public void voteClose(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                if (voteActive)
                {
                    voteActive = false;
                    irc.sendChatMessage("'" + voteObjective + "' voting closed.");
                }
                else
                {
                    irc.sendChatMessage("No vote is currently active.");
                }
            }
        }



        public void voteOpen(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                if (!voteActive)
                {
                    voteActive = true;
                    irc.sendChatMessage_NoDelays("Vote opened: " + voteObjective);
                    for (int i = 0; i < voteOptions.Count; i++)
                    {
                        irc.sendChatMessage_NoDelays((i + 1).ToString() + ". " + voteOptions[i]);
                    }
                }
                else
                {
                    irc.sendChatMessage("No vote is currently active.");
                }
            }
        }

        public void displayVote(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user) || irc.trustedUsers.Contains(msg.user))
            {
                if (voteActive)
                {
                    irc.sendChatMessage_NoDelays(voteObjective);
                    for (int i = 0; i < voteOptions.Count; i++)
                    {
                        irc.sendChatMessage_NoDelays((i + 1).ToString() + ". " + voteOptions[i]);
                    }
                }
            }
        }

        public void displayResults(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                int numberOfVotes = userVoted.Count;
                if (voteActive)
                {
                    irc.sendChatMessage("A vote is currently active. Close the vote, first!");
                }
                else
                {
                    if (voteObjective != String.Empty && numberOfVotes > 0)
                    {
                        irc.sendChatMessage_NoDelays("Results for: '" + voteObjective + "' are:");

                        uint[] results = new uint[voteOptions.Count];
                        foreach(uint element in userVoted.Values)
                        {
                            results[element]++;
                        }
                        int[] resultsNum = new int[3] { -1, -1, -1 };
                        string[] resultOption = new string[3];

                        resultsNum[0] = -1;
                        resultOption[0] = "";

                        for (int i = 0; i < results.Length; i++)
                        {
                            if (resultsNum[0] < results[i])
                            {
                                resultsNum[2] = resultsNum[1];
                                resultOption[2] = resultOption[1];
                                resultsNum[1] = resultsNum[0];
                                resultOption[1] = resultOption[0];
                                resultsNum[0] =  (int)results[i];
                                resultOption[0] = voteOptions[i];
                            }
                            else if (resultsNum[1] < results[i])
                            {
                                resultsNum[2] = resultsNum[1];
                                resultOption[2] = resultOption[1];
                                resultsNum[1] = (int)results[i];
                                resultOption[1] = resultOption[i];
                            }
                            else if (resultsNum[2] < results[i])
                            {
                                resultsNum[2] = (int)results[i];
                                resultOption[2] = resultOption[i];
                            }
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            if (resultsNum[i] > 0)
                            {
                                double prec = Math.Round(((resultsNum[i] * 1.0 / numberOfVotes) * 100), 2);
                                irc.sendChatMessage_NoDelays((i+1).ToString() + ". " + resultOption[i] + " (" + prec.ToString() + "%).");
                            }
                        }
                    }
                    else
                    {
                        irc.sendChatMessage("Vote results are empty. There was no voting in this session?");
                    }
                }
            }
        }
    }
}
