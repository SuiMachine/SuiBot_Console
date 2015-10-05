using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Timers;

namespace TwitchBotConsole
{
    class Program
    {
        private static IrcClient irc;
        private static Quotes _quotes;
        private static Ask _ask;
        private static Blacklist _blacklist;
        private static Coins _coins;
        private static Slots _slots;
        private static Bet _bet;
        private static CustomCvars _customCvars;
        private static Votes _votes;
        private static Intervals _intervals;
        private static Json_status _jsonStatus;
        private static System.Timers.Timer _statusCheckTimer;
        private static Leaderboards _leaderboards;
        private static System.Timers.Timer _timer;

        private static ReadMessage FormattedMessage;//for access by check

        private static bool cvarflag;

        private static void initBot()
        {
            irc.joinRoom(irc._config.channel);
            irc.sendIrcRawMessage("CAP REQ :twitch.tv/membership");

            _ask = new Ask();
            _quotes = new Quotes();
            _blacklist = new Blacklist(irc);
            _coins = new Coins();
            _slots = new Slots(_coins);
            _bet = new Bet(_coins);
            _customCvars = new CustomCvars();
            _votes = new Votes();
            _intervals = new Intervals();
            _jsonStatus = new Json_status();
            _statusCheckTimer = new System.Timers.Timer();
            _timer = new System.Timers.Timer();
            _leaderboards = new Leaderboards();

            _quotes.loadQuotesFromFile();
            _jsonStatus.SendChannel(irc._config.channel);
            _statusCheckTimer.Interval = 5 * 60 * 1000; // 5 minutes
            _statusCheckTimer.Start();
            _statusCheckTimer.Elapsed += new ElapsedEventHandler(_jsonStatus.TimerTick);
            _timer.Interval = 60 * 1000;
            _leaderboards.SendJsonPointer(_jsonStatus);
        }

        private static bool check(string toc)
        {
            if (FormattedMessage.message.StartsWith(toc, StringComparison.InvariantCultureIgnoreCase))
            {
                cvarflag = false;
                return true;
            }
            return false;
        }

        private static bool runBot()
        {
            string rawMessage = irc.readRawMessage();
            Trace.WriteLine(rawMessage);
            FormattedMessage = irc.readMessage(rawMessage);
            Console.WriteLine(FormattedMessage.user + ": " + FormattedMessage.message);

            #region PingResponse_DealingWithOPstatus
            if (rawMessage.StartsWith("PING"))
            {
                irc.sendIrcRawMessage("PONG tmi.twitch.tv\r\n");
                irc.sendIrcRawMessage("PONG tmi.twitch.tv\r\n");
                Console.WriteLine("Recieved PING, responded PONG.");
                return true;
            }
            else if (rawMessage.StartsWith((":jtv MODE #") + irc._config.channel))
            {
                string[] helper = rawMessage.Split(' ');
                if (helper[3] == "+o")                                     //Gets OP status
                {
                    if (!irc.moderators.Contains(helper[4]))                //To avoid duplicates
                        irc.moderators.Add(helper[4]);
                }
                else if (helper[3] == "-o")                                //Looses OP status
                {
                    if (!irc.supermod.Contains(helper[4]))                  //Ignore if the user is supermod
                        irc.moderators.Remove(helper[4]);
                }
                Console.WriteLine(rawMessage);
                return true;
            }
            else if(rawMessage.EndsWith(" JOIN #" + irc._config.channel))   //User Joined
            {
                int endOfName = rawMessage.IndexOf('!');
                string userName = rawMessage.Substring(1, endOfName - 1);
                Console.WriteLine("USER JOINED: " + userName);
                return true;
            }
            else if(rawMessage.EndsWith(" PART #" +irc._config.channel))    //User Left
            {
                int endOfName = rawMessage.IndexOf('!');
                string userName = rawMessage.Substring(1, endOfName - 1);
                Console.WriteLine("USER PART: " + userName);
                return true;
            }
            #endregion
            else
            {
                if (irc.filteringEnabled && !(irc.moderators.Contains(FormattedMessage.user) || irc.trustedUsers.Contains(FormattedMessage.user)) && _blacklist.checkForSpam(FormattedMessage))
                {
                    irc.purgeMessage(FormattedMessage.user);
                    if (irc.filteringRespond) irc.sendChatMessage("Probably spam FrankerZ");
                    return true;
                }
                //the goal here is going to be trying to group things
                if (!FormattedMessage.message.StartsWith("!") || irc.ignorelist.Contains(FormattedMessage.user))
                {
                    //literally nothing else happens in your code if this is false
                    return true;
                }
                cvarflag = true;//check if _ANYTHING_ matched.
                if (irc.vocalMode)
                {
                    if (check("!help")) irc.sendChatMessage("The list of commands is available at https://github.com/SuiMachine/SuiBot_Console/wiki/List-of-all-commands");
                    if (check("!commands")) irc.sendChatMessage("The list of commands is available at https://github.com/SuiMachine/SuiBot_Console/wiki/List-of-all-commands");
                    if (check("!ask ")) _ask.answerAsk(irc, FormattedMessage);
                    if (check("!addCvar ")) _customCvars.addCustomCvar(irc, FormattedMessage);
                    if (check("!removeCvar ")) _customCvars.removeCustomCvar(irc, FormattedMessage);
                    if (check("!customCvars")) _customCvars.showCustomCvars(irc, FormattedMessage);
                }
                if (irc.quoteEnabled)
                {
                    if (check("!addquote ")) _quotes.addQuote(irc, FormattedMessage);
                    if (check("!quoteID ")) _quotes.getQuotebyID(irc, FormattedMessage);
                    if (check("!quote")) _quotes.getQuote(irc, FormattedMessage.message);
                    if (check("!removeQuote")) _quotes.removeQuote(irc, FormattedMessage);
                    if (check("!getNumberOfQuotes")) _quotes.getNumberOfQuotes(irc);
                }
                if (irc.intervalMessagesEnabled)
                {
                    if (check("!intervalMessageAdd")) _intervals.AddIntervalMessage(irc, FormattedMessage);
                    if (check("!intervalMessageShow")) _intervals.ShowIntervalMessageID(irc, FormattedMessage);
                    if (check("!intervalMessageRemove")) _intervals.RemoveIntervalMessage(irc, FormattedMessage);
                }
                if (irc.slotsEnable)
                {
                    if (check("!slots ")) _slots.PlaySlots(irc, FormattedMessage);
                    if (check("!coins")) _coins.DisplayCoins(irc, FormattedMessage);
                    if (check("!addCoins ")) _coins.AddCoins(irc, FormattedMessage);
                }
                if (irc.voteEnabled)
                {
                    if (check("!callVote ")) _votes.callVote(irc, FormattedMessage);
                    if (check("!voteOptions ")) _votes.setOptions(irc, FormattedMessage);
                    if (check("!voteOpen")) _votes.voteOpen(irc, FormattedMessage);
                    if (check("!voteClose")) _votes.voteClose(irc, FormattedMessage);
                    if (check("!voteDisplay")) _votes.displayVote(irc, FormattedMessage);
                    if (check("!voteResults")) _votes.displayResults(irc, FormattedMessage);
                    if (check("!vote ")) _votes.Vote(irc, FormattedMessage);
                }
                if (irc.deathCounterEnabled)
                {
                    if (check("!deathCounter")) irc.DeathCounterDisplay(FormattedMessage);
                    if (check("!deathAdd")) irc.DeathCounterAdd(FormattedMessage);
                    if (check("!deathRemove")) irc.DeathCounterRemove(FormattedMessage);
                }
                if (irc.filteringEnabled)
                {
                    if (check("!filterAdd ")) _blacklist.AddFilter(FormattedMessage);
                    if (check("!filterRemove ")) _blacklist.RemoveFilter(FormattedMessage);
                }
                if (irc.leaderBoardEnabled)
                {
                    if (check("!forceSpeedrunPage")) _jsonStatus.forcedGameFunction(irc, FormattedMessage);
                    if (check("!speedrunName ")) irc.updateSpeedrunName(FormattedMessage);

                    if (check("!pb"))
                    {
                        Thread lbThread = new Thread(new ThreadStart(_leaderboards.getPB));
                        _leaderboards.recieveData(irc, FormattedMessage);
                        lbThread.Start();
                    }

                    if (check("!wr"))
                    {
                        Thread lbThread = new Thread(new ThreadStart(_leaderboards.getLeaderboard));
                        _leaderboards.recieveData(irc, FormattedMessage);
                        lbThread.Start();
                    }
                }
                //ones we do regardless
                if (check("!ignoreAdd ")) irc.ignoreListAdd(FormattedMessage);
                if (check("!ignoreRemove ")) irc.ignoreListRemove(FormattedMessage);
                if (check("!trustedAdd ")) irc.trustedUserAdd(FormattedMessage);
                if (check("!permit ")) irc.trustedUserAdd(FormattedMessage);
                if (check("!trustedRemove ")) irc.trustedUsersRemove(FormattedMessage);

                //mod only!
                if (irc.moderators.Contains(FormattedMessage.user))
                {
                    if (check("!updateJsonInfo")) _jsonStatus.requestUpdate(irc);
                }
                //supermod only!
                if (irc.supermod.Contains(FormattedMessage.user))
                {
                    if (check("!killBot"))
                    {
                        irc.sendChatMessage("Goodbye! BibleThump");

                        if (irc.filteringEnabled)
                            _blacklist.saveUserInfo();

                        return false;
                    }
                }

                if (cvarflag)
                {
                    _customCvars.cvarPerform(irc, FormattedMessage);
                }

                return true;
            }
        }

        static void Main(string[] args)
        {
            irc = new IrcClient();
            if (!irc.configFileExisted)
            {
                Console.WriteLine("No config file found. An example config file was created");
                System.Environment.Exit(1);
            }

            initBot();

            if (irc.intervalMessagesEnabled)
            {
                _intervals.sendIrc(irc, _jsonStatus);
                _timer.Start();
                _timer.Elapsed += new System.Timers.ElapsedEventHandler(_intervals.timerSender);
            }

            while (runBot()) ;
        }
    }
}
