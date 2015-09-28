using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace TwitchBotConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            bool botRun = true;
            ReadMessage FormattedMessage;
            IrcClient irc = new IrcClient();
            if(irc.configFileExisted)
            {
                irc.joinRoom(irc._config.channel);
                irc.sendIrcRawMessage("CAP REQ :twitch.tv/membership");         //Request for Twitch to send additional information (like mod statuses, chat users list etc.)
                Ask _ask = new Ask();
                Quotes _quotes = new Quotes();
                _quotes.loadQuotesFromFile();
                Blacklist _blacklist = new Blacklist(irc);
                Coins _coins = new Coins();
                Slots _slots = new Slots(_coins);
                Bet _bet = new Bet(_coins);
                CustomCvars _customCvars = new CustomCvars();
                Votes _votes = new Votes();

                Intervals _intervals = new Intervals();

                Json_status _jsonStatus = new Json_status();
                _jsonStatus.SendChannel(irc._config.channel);
                System.Timers.Timer _statusCheckTimer = new System.Timers.Timer();
                _statusCheckTimer.Interval = 5 * 60 * 1000;             //every 5 minutes
                _statusCheckTimer.Start();
                _statusCheckTimer.Elapsed += new System.Timers.ElapsedEventHandler(_jsonStatus.TimerTick);

                System.Timers.Timer _timer = new System.Timers.Timer();
                _timer.Interval = 60*1000;
                if (irc.intervalMessagesEnabled)
                {
                    _intervals.sendIrc(irc, _jsonStatus);
                    _timer.Start();
                    _timer.Elapsed += new System.Timers.ElapsedEventHandler(_intervals.timerSender);
                }

                Leaderboards _leaderboards = new Leaderboards();
                _leaderboards.SendJsonPointer(_jsonStatus);


                while (botRun)
                {
                    string rawMessage = irc.readRawMessage();
                    Trace.WriteLine(rawMessage);
                    FormattedMessage = irc.readMessage(rawMessage);
                    Console.WriteLine(FormattedMessage.user + ": " + FormattedMessage.message);
                    #region PingResponse_DealingWithOPstatus
                    if (rawMessage.StartsWith("PING"))
                    {
                        irc.sendIrcRawMessage("PONG tmi.twitch.tv\r\n");
                        Console.WriteLine("Recieved PING, responded PONG.");
                    }
                    else if (rawMessage.StartsWith((":jtv MODE #") + irc._config.channel))
                    {
                        string[] helper = rawMessage.Split(' ');
                        if (helper[3] == "+o")                                     //Gets OP status
                        {
                            if(!irc.moderators.Contains(helper[4]))                //To avoid duplicates
                                irc.moderators.Add(helper[4]);
                        }
                        else if (helper[3] == "-o")                                //Looses OP status
                        {
                            if (!irc.supermod.Contains(helper[4]))                  //Ignore if the user is supermod
                                irc.moderators.Remove(helper[4]);
                        }
                        Console.WriteLine(rawMessage);
                    }
                    #endregion
                    else
                    {
                        if (irc.filteringEnabled && !irc.moderators.Contains(FormattedMessage.user) && !irc.trustedUsers.Contains(FormattedMessage.user) && _blacklist.checkForSpam(FormattedMessage.message))
                        {
                            irc.purgeMessage(FormattedMessage.user);
                            if(irc.filteringRespond)
                                irc.sendChatMessage("Probably spam FrankerZ");
                        }
                        else if(FormattedMessage.message.StartsWith("!"))
                        {
                            if (!irc.ignorelist.Contains(FormattedMessage.user))
                            {
                                #region ModulesAndFunctions
                                if (irc.vocalMode && (FormattedMessage.message.StartsWith( "!commands", StringComparison.InvariantCultureIgnoreCase) || FormattedMessage.message.StartsWith("!help", StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    irc.sendChatMessage("The list of commands is available at https://github.com/SuiMachine/SuiBot_Console/wiki/List-of-all-commands");
                                }
                                else if (irc.quoteEnabled && FormattedMessage.message.StartsWith("!addquote ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _quotes.addQuote(irc, FormattedMessage);
                                }
                                else if (irc.quoteEnabled && FormattedMessage.message.StartsWith("!quoteID ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _quotes.getQuotebyID(irc, FormattedMessage);
                                }
                                else if (irc.quoteEnabled && FormattedMessage.message.StartsWith("!quote", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _quotes.getQuote(irc, FormattedMessage.message);
                                }
                                else if (irc.quoteEnabled && FormattedMessage.message.StartsWith("!removeQuote", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _quotes.removeQuote(irc, FormattedMessage);
                                }
                                else if (irc.quoteEnabled && FormattedMessage.message.StartsWith("!getNumberOfQuotes", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _quotes.getNumberOfQuotes(irc);
                                }
                                #region intervalMessages
                                else if (irc.intervalMessagesEnabled && FormattedMessage.message.StartsWith("!intervalMessageAdd", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _intervals.AddIntervalMessage(irc, FormattedMessage);
                                }
                                else if (irc.intervalMessagesEnabled && FormattedMessage.message.StartsWith("!intervalMessageShow", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _intervals.ShowIntervalMessageID(irc, FormattedMessage);
                                }
                                else if (irc.intervalMessagesEnabled && FormattedMessage.message.StartsWith("!intervalMessageRemove", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _intervals.RemoveIntervalMessage(irc, FormattedMessage);
                                }
                                #endregion
                                else if (irc.vocalMode && FormattedMessage.message.StartsWith("!ask ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _ask.answerAsk(irc, FormattedMessage);
                                }
                                #region Slots
                                else if (irc.slotsEnable && FormattedMessage.message.StartsWith("!slots ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _slots.PlaySlots(irc, FormattedMessage);
                                }
                                else if (irc.slotsEnable && FormattedMessage.message.StartsWith("!coins", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _coins.DisplayCoins(irc, FormattedMessage);
                                }
                                else if (irc.slotsEnable && FormattedMessage.message.StartsWith("!addCoins ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _coins.AddCoins(irc, FormattedMessage);
                                }
                                #endregion
                                else if (FormattedMessage.message.StartsWith("!ignoreAdd ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    irc.ignoreListAdd(FormattedMessage);
                                }
                                else if (FormattedMessage.message.StartsWith("!ignoreRemove ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    irc.ignoreListRemove(FormattedMessage);
                                }
                                else if (FormattedMessage.message.StartsWith("!trustedAdd ", StringComparison.InvariantCultureIgnoreCase) || FormattedMessage.message.StartsWith("!permit ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    irc.trustedUserAdd(FormattedMessage);
                                }
                                else if (FormattedMessage.message.StartsWith("!trustedRemove ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    irc.trustedUsersRemove(FormattedMessage);
                                }
                                #endregion
                                #region Votes
                                else if(irc.voteEnabled && FormattedMessage.message.StartsWith("!callVote ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _votes.callVote(irc, FormattedMessage);
                                }
                                else if (irc.voteEnabled && FormattedMessage.message.StartsWith("!voteOptions ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _votes.setOptions(irc, FormattedMessage);
                                }
                                else if (irc.voteEnabled && FormattedMessage.message.StartsWith("!voteOpen", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _votes.voteOpen(irc, FormattedMessage);
                                }
                                else if (irc.voteEnabled && FormattedMessage.message.StartsWith("!voteClose", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _votes.voteClose(irc, FormattedMessage);
                                }
                                else if (irc.voteEnabled && FormattedMessage.message.StartsWith("!voteDisplay", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _votes.displayVote(irc, FormattedMessage);
                                }
                                else if (irc.voteEnabled && FormattedMessage.message.StartsWith("!voteResults", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _votes.displayResults(irc, FormattedMessage);
                                }
                                else if (irc.voteEnabled && FormattedMessage.message.StartsWith("!vote ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _votes.Vote(irc, FormattedMessage);
                                }
                                #endregion
                                else if (irc.deathCounterEnabled && FormattedMessage.message.StartsWith("!deathCounter", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    irc.DeathCounterDisplay(FormattedMessage);
                                }
                                else if (irc.deathCounterEnabled && FormattedMessage.message.StartsWith("!deathAdd", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    irc.DeathCounterAdd(FormattedMessage);
                                }
                                else if (irc.deathCounterEnabled && FormattedMessage.message.StartsWith("!deathRemove", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    irc.DeathCounterRemove(FormattedMessage);
                                }
                                else if (FormattedMessage.message.StartsWith("!updateJsonInfo", StringComparison.InvariantCultureIgnoreCase) && irc.moderators.Contains(FormattedMessage.user))
                                {
                                    _jsonStatus.requestUpdate(irc);
                                }
                                #region LeaderboardsAndShortcuts
                                else if (irc.leaderBoardEnabled && (FormattedMessage.message.StartsWith("!leaderboard", StringComparison.InvariantCultureIgnoreCase) || FormattedMessage.message.StartsWith("!lb", StringComparison.InvariantCultureIgnoreCase)  || FormattedMessage.message.StartsWith("!wr", StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    Thread lbThread = new Thread(new ThreadStart(_leaderboards.getLeaderboard));
                                    _leaderboards.recieveData(irc, FormattedMessage);
                                    lbThread.Start();
                                }
                                else if (irc.leaderBoardEnabled && FormattedMessage.message.StartsWith("!pb", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    Thread lbThread = new Thread(new ThreadStart(_leaderboards.getPB));
                                    _leaderboards.recieveData(irc, FormattedMessage);
                                    lbThread.Start();
                                }
                                else if (irc.leaderBoardEnabled && FormattedMessage.message.StartsWith("!forceSpeedrunPage", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _jsonStatus.forcedGameFunction(irc, FormattedMessage);
                                }
                                else if (FormattedMessage.message.StartsWith("!speedrunName ", StringComparison.InvariantCultureIgnoreCase) && irc.supermod.Contains(FormattedMessage.user))
                                {
                                    irc.updateSpeedrunName(FormattedMessage);
                                }
                                #endregion
                                #region CustomCvars
                                else if (irc.vocalMode && FormattedMessage.message.StartsWith("!addCvar ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _customCvars.addCustomCvar(irc, FormattedMessage);
                                }
                                else if (irc.vocalMode && FormattedMessage.message.StartsWith("!removeCvar ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _customCvars.removeCustomCvar(irc, FormattedMessage);
                                }
                                else if (irc.vocalMode && FormattedMessage.message.StartsWith("!customCvars", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _customCvars.showCustomCvars(irc, FormattedMessage);
                                }
                                else if(irc.filteringEnabled && FormattedMessage.message.StartsWith("!filterAdd ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _blacklist.AddFilter(FormattedMessage);
                                }
                                else if(irc.filteringEnabled && FormattedMessage.message.StartsWith("!filterRemove ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _blacklist.RemoveFilter(FormattedMessage);
                                }
                                else if (FormattedMessage.message.StartsWith("!killBot", StringComparison.InvariantCultureIgnoreCase) && irc.supermod.Contains(FormattedMessage.user))
                                {
                                    irc.sendChatMessage("Goodbye! BibleThump");
                                    botRun = false;
                                }
                                else
                                    _customCvars.cvarPerform(irc, FormattedMessage);
                                #endregion
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No config file found. An example config file was created");
            }
        }
    }
}
