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
        int voteType = 0;
        List<string> usersVoted = new List<string>();
        List<string> options = new List<string>();
        List<int> votes = new List<int>();
        bool voteActive = false;

        public void callVote(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                if (!voteActive)
                {
                    voteObjective = helper[1];
                    irc.sendChatMessage("Vote object set to: '" + voteObjective + "'. Vote type set to default.");
                    usersVoted.Clear();
                    options.Clear();
                    votes.Clear();
                    voteType = 0;
                }
                else
                {
                    irc.sendChatMessage("A vote is currently active. Please close the vote, first.");
                }
            }
        }

        public void setType(IrcClient irc, ReadMessage msg)
        {
            //stub
        }

        public void setOptions(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                options.Clear();
                votes.Clear();
                string[] starter = msg.message.Split(new char[] { ' ' }, 2);
                var count = starter[1].Count(x => x == ':');
                if (count > 0)
                {
                    string[] voteOptions = starter[1].Split(':');
                    for (int i = 0; i < voteOptions.Length; i++)
                    {
                        options.Add(voteOptions[i]);
                        votes.Add(0);
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
                if (!usersVoted.Contains(msg.user))
                {
                    int value;
                    string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                    if (int.TryParse(helper[1], out value))
                    {
                        value--;
                        if (value >= 0 && value < options.Count)
                        {
                            usersVoted.Add(msg.user);
                            votes[value] = votes[value] + 1;
                            Console.WriteLine("User " + msg.user + "voted for " + value + 1.ToString());
                        }
                    }
                }
                else
                    irc.sendChatMessage(msg.user + ": You have already voted!");
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
                    for (int i = 0; i < options.Count; i++)
                    {
                        irc.sendChatMessage_NoDelays((i + 1).ToString() + ". " + options[i]);
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
            if (irc.moderators.Contains(msg.user))
            {
                if (voteActive)
                {
                    irc.sendChatMessage_NoDelays(voteObjective);
                    for (int i = 0; i < options.Count; i++)
                    {
                        irc.sendChatMessage_NoDelays((i + 1).ToString() + ". " + options[i]);
                    }
                }
            }
        }

        public void displayResults(IrcClient irc, ReadMessage msg)
        {
            if (irc.moderators.Contains(msg.user))
            {
                if (voteActive)
                {
                    irc.sendChatMessage("A vote is currently active. Close the vote, first!");
                }
                else
                {
                    if (voteObjective != String.Empty && votes.Count > 1)
                    {
                        int sum = 0;
                        irc.sendChatMessage_NoDelays("Results for: '" + voteObjective + "' are:");
                        for (int i = 0; i < votes.Count; i++)                ///Get a sum
                        {
                            sum = sum + votes[i];
                        }
                        int[] resultsNum = new int[3] { -1, -1, -1 };
                        string[] resultOption = new string[3];

                        resultsNum[0] = -1;
                        resultOption[0] = "";

                        for (int i = 0; i < votes.Count; i++)
                        {
                            if (resultsNum[0] < votes[i])
                            {
                                resultsNum[2] = resultsNum[1];
                                resultOption[2] = resultOption[1];
                                resultsNum[1] = resultsNum[0];
                                resultOption[1] = resultOption[0];
                                resultsNum[0] = votes[i];
                                resultOption[0] = options[i];
                            }
                            else if (resultsNum[1] < votes[i])
                            {
                                resultsNum[2] = resultsNum[1];
                                resultOption[2] = resultOption[1];
                                resultsNum[1] = votes[i];
                                resultOption[1] = resultOption[i];
                            }
                            else if (resultsNum[2] < votes[i])
                            {
                                resultsNum[2] = votes[i];
                                resultOption[2] = resultOption[i];
                            }
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            if (resultsNum[i] > 0)
                            {
                                double prec = Math.Round(((resultsNum[i] * 1.0 / sum) * 100), 2);
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
