using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;
using System.IO;
using Meebey.SmartIrc4net;

namespace TwitchBotConsole
{
    class Program
    {
        private static oldIRCClient irc;
        private static Quotes _quotes;
        private static Ask _ask;
        private static Blacklist _blacklist;
        private static Coins _coins;
        private static Slots _slots;
        private static Bet _bet;
        private static CustomCvars _customCvars;
        private static Votes _votes;
        private static Intervals _intervals;
        private static ViewerPB _viewerPB;
        private static Json_status _jsonStatus;
		private static SRL _srl;
        private static System.Timers.Timer _statusCheckTimer;
        private static Leaderboards _leaderboards;
        private static System.Timers.Timer _timer;
        private static string cachedMessage;
        private static ReadMessage FormattedMessage;
        private static PyramidBreaker _pyramidBreaker;

        private static bool cvarflag;

        private static void initBot()
        {
            irc.meebyIrc.OnError += MeebyIrc_OnError;
            irc.meebyIrc.OnErrorMessage += MeebyIrc_OnErrorMessage;
            irc.meebyIrc.OnConnecting += MeebyIrc_OnConnecting;
            irc.meebyIrc.OnConnected += MeebyIrc_OnConnected;
            irc.meebyIrc.OnAutoConnectError += MeebyIrc_OnAutoConnectError;
            irc.meebyIrc.OnDisconnecting += MeebyIrc_OnDisconnecting;
            irc.meebyIrc.OnDisconnected += MeebyIrc_OnDisconnected;
            irc.meebyIrc.OnRegistered += MeebyIrc_OnRegistered;
            irc.meebyIrc.OnPart += MeebyIrc_OnPart;
            irc.meebyIrc.OnJoin += MeebyIrc_OnJoin;
            irc.meebyIrc.OnChannelAction += MeebyIrc_OnChannelAction;
            irc.meebyIrc.OnReadLine += MeebyIrc_OnReadLine;
            irc.meebyIrc.OnChannelMessage += MeebyIrc_OnChannelMessage;
            irc.meebyIrc.OnOp += MeebyIrc_OnOp;
            irc.meebyIrc.OnDeop += MeebyIrc_OnDeop;
            if (irc.checkForUpdates)
                CheckForUpdate();

            irc.meebyIrc.WriteLine("CAP REQ :twitch.tv/membership");

            _ask = new Ask();
            _quotes = new Quotes();
            _blacklist = new Blacklist(irc);
            _coins = new Coins();
            _slots = new Slots(_coins);
            _bet = new Bet(_coins);
            _customCvars = new CustomCvars();
            _votes = new Votes();
            _intervals = new Intervals();
            _viewerPB = new ViewerPB(irc);
            _jsonStatus = new Json_status(irc, _viewerPB);
            _statusCheckTimer = new System.Timers.Timer();
            _timer = new System.Timers.Timer();
            _leaderboards = new Leaderboards();
			_srl = new SRL();
            _pyramidBreaker = new PyramidBreaker(irc);
            


            _quotes.loadQuotesFromFile();
            _jsonStatus.SendChannel("cadarev");
            //_jsonStatus.SendChannel(irc._config.channel);
            _statusCheckTimer.Interval = 5 * 60 * 1000; // 5 minutes
            _statusCheckTimer.Start();
            _statusCheckTimer.Elapsed += new ElapsedEventHandler(_jsonStatus.TimerTick);
            _jsonStatus.TimerTick(null, null);
            _timer.Interval = 60 * 1000;
            _leaderboards.SendJsonPointer(_jsonStatus);
        }

        private static void CheckForUpdate()
        {
            try
            {
                Version currentVer = Assembly.GetExecutingAssembly().GetName().Version;
                string updaterPath;
                bool result = Updater.CheckAndDownload(currentVer, out updaterPath);
                if(result)
                {
                    ProcessStartInfo info = new ProcessStartInfo();
					string OS = Environment.OSVersion.ToString();
					if(OS.StartsWith("Microsoft"))     //Is Shit OS
					{
						info.FileName = updaterPath;
						Process.Start(info);
						System.Environment.Exit(0);
					}
					else
					{
						info.FileName = "bash";
						info.Arguments = updaterPath;
						Process.Start(info);
						System.Environment.Exit(0);
					}
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
        }

        private static void reloadBot()
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            ProcessStartInfo info = new ProcessStartInfo();
            string OS = Environment.OSVersion.ToString();
            if (OS.StartsWith("Microsoft"))     //Is Shit OS
            {
                info.FileName = Path.Combine(currentPath, "TwitchBotConsole.exe");
                info.Arguments = "+msg Reloaded FrankerZ";
                info.WorkingDirectory = currentPath;
                Process.Start(info);
                System.Environment.Exit(0);
            }
            else
            {
                info.FileName = "mono";
                info.WorkingDirectory = currentPath;
                info.Arguments = Path.Combine(currentPath, "TwitchBotConsole.exe +msg Reloaded FrankerZ");
                Process.Start(info);
                System.Environment.Exit(0);
            }
        }

        private static bool check_lazy(string toc)
        {
            if (FormattedMessage.message.StartsWith(toc, StringComparison.InvariantCultureIgnoreCase))
            {
                cvarflag = false;
                return true;
            }
            return false;
        }

        private static bool check_exact(string toc)
        {
            if (FormattedMessage.message.ToLower() == toc.ToLower())
            {
                cvarflag = false;
                return true;
            }
            return false;
        }

        private static bool runBot(ReadMessage formattedMessage)
        {
            FormattedMessage = formattedMessage;
            Console.WriteLine(formattedMessage.user + ": " + formattedMessage.message);


            if (cachedMessage != String.Empty)
            {
                irc.sendChatMessage(cachedMessage);
                cachedMessage = String.Empty;
            }
            TimeOutReason reason = TimeOutReason.NoPurge;

            if (irc.filteringEnabled && !(irc.moderators.Contains(formattedMessage.user) || irc.trustedUsers.Contains(formattedMessage.user)) && (reason = _blacklist.checkForAnnoyingTrash(formattedMessage)) != TimeOutReason.NoPurge)
            {
                irc.purgeMessage(formattedMessage.user, reason);
                if (irc.filteringRespond) irc.sendChatMessage("Don't do that again! DansGame");
                return true;
            }

            if (irc.filteringEnabled && !(irc.moderators.Contains(formattedMessage.user) || irc.trustedUsers.Contains(formattedMessage.user)) && (reason = _blacklist.checkForSpam(formattedMessage)) != TimeOutReason.NoPurge)
            {
                irc.purgeMessage(formattedMessage.user, reason);
                if (irc.filteringRespond) irc.sendChatMessage("Probably spam FrankerZ");
                return true;
            }

            if (irc.filteringEnabled && !(irc.moderators.Contains(formattedMessage.user) || irc.trustedUsers.Contains(formattedMessage.user)) && (reason = _blacklist.checkForBanWorthyContent(formattedMessage)) != TimeOutReason.NoPurge)
            {
                irc.banMessage(formattedMessage.user, reason);
                if (irc.filteringRespond) irc.sendChatMessage("And he/she is gone! FrankerZ");
                return true;
            }
            if (irc.breakPyramids) _pyramidBreaker.breakPyramid(formattedMessage);

            //the goal here is going to be trying to group things
            if (!formattedMessage.message.StartsWith("!") || irc.ignorelist.Contains(formattedMessage.user))
            {
                //literally nothing else happens in your code if this is false
                return true;
            }
            cvarflag = true;//check if _ANYTHING_ matched.
            if (irc.vocalMode)
            {
                if (irc.moderators.Contains(formattedMessage.user) && check_lazy("!help")) irc.sendChatMessage("The list of commands is available at https://github.com/SuiMachine/SuiBot_Console/wiki/List-of-all-commands");
                if (irc.moderators.Contains(formattedMessage.user) && check_lazy("!commands")) irc.sendChatMessage("The list of commands is available at https://github.com/SuiMachine/SuiBot_Console/wiki/List-of-all-commands");
                if (check_lazy("!ask ")) _ask.answerAsk(irc, formattedMessage);
                if (check_lazy("!addCvar ")) _customCvars.addCustomCvar(irc, formattedMessage);
                if (check_lazy("!removeCvar ")) _customCvars.removeCustomCvar(irc, formattedMessage);
                if (check_lazy("!customCvars")) _customCvars.showCustomCvars(irc, formattedMessage);
            }
            if (irc.quoteEnabled)
            {
                if (check_lazy("!addquote ")) _quotes.addQuote(irc, formattedMessage);
                if (check_lazy("!quoteID ")) _quotes.getQuotebyID(irc, formattedMessage);
                if (check_lazy("!quote")) _quotes.getQuote(irc, formattedMessage.message);
                if (check_lazy("!removeQuote")) _quotes.removeQuote(irc, formattedMessage);
                if (check_lazy("!getNumberOfQuotes")) _quotes.getNumberOfQuotes(irc);
            }
            if (irc.intervalMessagesEnabled)
            {
                if (check_lazy("!intervalMessageAdd")) _intervals.AddIntervalMessage(irc, formattedMessage);
                if (check_lazy("!intervalMessageShow ")) _intervals.ShowIntervalMessageID(irc, formattedMessage);
                if (check_lazy("!intervalMessageRemove")) _intervals.RemoveIntervalMessage(irc, formattedMessage);
            }
            if (irc.slotsEnable)
            {
                if (check_lazy("!slots ")) _slots.PlaySlots(irc, formattedMessage);
                if (check_lazy("!coins")) _coins.DisplayCoins(irc, formattedMessage);
                if (check_lazy("!addCoins ")) _coins.AddCoins(irc, formattedMessage);
            }
            if (irc.voteEnabled)
            {
                if (check_lazy("!callVote ")) _votes.callVote(irc, formattedMessage);
                if (check_lazy("!voteOptions ")) _votes.setOptions(irc, formattedMessage);
                if (check_lazy("!voteOpen")) _votes.voteOpen(irc, formattedMessage);
                if (check_lazy("!voteClose")) _votes.voteClose(irc, formattedMessage);
                if (check_lazy("!voteDisplay")) _votes.displayVote(irc, formattedMessage);
                if (check_lazy("!voteResults")) _votes.displayResults(irc, formattedMessage);
                if (check_lazy("!vote ")) _votes.Vote(irc, formattedMessage);
            }
            if (irc.deathCounterEnabled)
            {
                if (check_lazy("!deathCounter")) irc.DeathCounterDisplay(formattedMessage);
                if (check_lazy("!deathAdd")) irc.DeathCounterAdd(formattedMessage);
                if (check_lazy("!deathRemove")) irc.DeathCounterRemove(formattedMessage);
            }
            if (irc.viewerPBActive)
            {
                if (check_lazy("!viewerPB")) _viewerPB.displayViewerPB(formattedMessage);
            }
            if (irc.filteringEnabled)
            {
                if (check_lazy("!filterAdd ")) _blacklist.AddFilter(formattedMessage);
                if (check_lazy("!filterRemove ")) _blacklist.RemoveFilter(formattedMessage);
                if (check_lazy("!allowToPostLinks")) _blacklist.addToAllowedToPostLinks(formattedMessage);
                if (check_lazy("!resetAllowToPostLinks")) _blacklist.resetFromAllowedToPostLinks(formattedMessage);  //No idea why
            }
            if (irc.leaderBoardEnabled)
            {
                if (check_lazy("!forceSpeedrunPage")) _jsonStatus.forcedGameFunction(formattedMessage);
                if (check_lazy("!setCategory ") || check_lazy("!setCathegory ")) _jsonStatus.forceCategoryFunction(formattedMessage);
                if (check_lazy("!speedrunName ")) irc.updateSpeedrunName(formattedMessage);

                if (check_lazy("!pb"))
                {
                    _leaderboards.recieveData(irc, formattedMessage);
                    Task.Factory.StartNew(() =>
                    {
                        _leaderboards.getPB();
                    });
                }

                if (check_exact("!wr") || check_lazy("!wr "))
                {
                    _leaderboards.recieveData(irc, formattedMessage);
                    Task.Factory.StartNew(() =>
                    {
                        _leaderboards.getLeaderboard();
                    });
                }
            }

			if (true)
			{
				if (check_lazy("!srl"))
					_srl.getRaces(irc);
			}


			//ones we do regardless
			if (check_lazy("!ignoreAdd ")) irc.ignoreListAdd(formattedMessage);
            if (check_lazy("!ignoreRemove ")) irc.ignoreListRemove(formattedMessage);
            if (check_lazy("!trustedAdd ")) irc.trustedUserAdd(formattedMessage);
            if (check_lazy("!permit ")) irc.trustedUserAdd(formattedMessage);
            if (check_lazy("!trustedRemove ")) irc.trustedUsersRemove(formattedMessage);


            //mod only!
            if (irc.moderators.Contains(formattedMessage.user))
            {
                if (check_lazy("!updateJsonInfo")) _jsonStatus.requestUpdate();
                if (check_lazy("!version")) irc.version();
                if (check_lazy("!highlight")) irc.createHighlight(formattedMessage, _jsonStatus);
                if (check_lazy("!sellout") || check_lazy("!subscribe")) irc.postSubscribeMessage();
            }
            //supermod only!
            if (irc.supermod.Contains(formattedMessage.user))
            {
                if (check_lazy("!killBot"))
                {
                    irc.sendChatMessage("Goodbye! BibleThump");

                    if (irc.filteringEnabled)
                        _blacklist.saveUserInfo();
                    irc.meebyIrc.Disconnect();

                    return false;
                }
                if (check_lazy("!reloadBot")) reloadBot();
            }

            //Property
            if (check_lazy("!getProperty")) irc.getParameter(formattedMessage);
            if (check_lazy("!setProperty")) irc.setParameter(formattedMessage);

            if (cvarflag)
            {
                _customCvars.cvarPerform(irc, formattedMessage);
            }

            return true;

        }

        static void Main(string[] args)
        {
            string _cachedMessage = "";

            Console.CancelKeyPress += Console_CancelKeyPress;       //Some additional events
            irc = new oldIRCClient();
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
            for(int i=0; i<args.Length; i++)
            {
                if(args[i] == "+msg")
                {
                    i++;
                    while(i < args.Length && !args[i].StartsWith("+"))
                    {
                        _cachedMessage += args[i] + " ";
                        i++;
                    }
                    i--;
                }
            }
            cachedMessage = _cachedMessage;

			irc.meebyIrc.Listen();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            PerformShutdownTasks();
        }

        private static void PerformShutdownTasks()
        {
            if (irc != null)
            {
                if (_blacklist != null && irc.filteringEnabled)
                {
                    _blacklist.saveUserInfo();
                }
            }
        }

        #region EventHandlers
        private static void MeebyIrc_OnJoin(object sender, Meebey.SmartIrc4net.JoinEventArgs e)
        {
            Console.WriteLine("! JOINED: " + e.Data.Nick);
        }

        private static void MeebyIrc_OnRegistered(object sender, EventArgs e)
        {
            Console.WriteLine("! LOGIN VERIFIED");
        }

        private static void MeebyIrc_OnDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected.");
        }

        private static void MeebyIrc_OnDisconnecting(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnecting...");
        }

        private static void MeebyIrc_OnAutoConnectError(object sender, Meebey.SmartIrc4net.AutoConnectErrorEventArgs e)
        {
            Console.WriteLine("OnAutoConnectError Event: " + e.Exception);
        }

        private static void MeebyIrc_OnConnecting(object sender, EventArgs e)
        {
            Console.WriteLine("Connecting: " + e);
        }

        private static void MeebyIrc_OnConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected: (DEBUG)" + e);
            irc.verifyLogin();
            irc.meebyIrc.RfcJoin("#" + irc._config.channel);
        }

        private static void MeebyIrc_OnChannelAction(object sender, Meebey.SmartIrc4net.ActionEventArgs e)
        {
            Console.WriteLine("OnChannelAction Event: " + e.Data);
        }

        private static void MeebyIrc_OnErrorMessage(object sender, Meebey.SmartIrc4net.IrcEventArgs e)
        {
            Console.WriteLine("! " + e.Data.Message + " !");
        }

        private static void MeebyIrc_OnError(object sender, Meebey.SmartIrc4net.ErrorEventArgs e)
        {
            Console.WriteLine("OnError Event: " + e.Data.Message);
        }

        private static void MeebyIrc_OnReadLine(object sender, Meebey.SmartIrc4net.ReadLineEventArgs e)
        {
            //Console.WriteLine("onReadLine Event:" + e.Line);
        }

        private static void MeebyIrc_OnPart(object sender, PartEventArgs e)
        {
            Console.WriteLine("! PART: " + e.Data.Nick);
        }

        private static void MeebyIrc_OnChannelMessage(object sender, IrcEventArgs e)
        {
            ReadMessage msg;
            msg.user = e.Data.Nick;
            msg.message = e.Data.Message;
            runBot(msg);
        }

        private static void MeebyIrc_OnOp(object sender, OpEventArgs e)
        {
            if(!irc.moderators.Contains(e.Whom))
                irc.moderators.Add(e.Whom);
            Console.WriteLine("! +OP: " + e.Whom);
        }

        private static void MeebyIrc_OnDeop(object sender, DeopEventArgs e)
        {
            if (!irc.supermod.Contains(e.Whom))                  //Ignore if the user is supermod
                irc.moderators.Remove(e.Whom);
            Console.WriteLine("! -OP: " + e.Whom);
        }
        #endregion
    }
}
