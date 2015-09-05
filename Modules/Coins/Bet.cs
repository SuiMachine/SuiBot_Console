using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotConsole
{
    class Bet
    {
        Coins coins;
        Dictionary<string, Tuple<uint, DateTime>> userCoins;

        private bool betRunning = false;

        public Bet(Coins _coins)
        {
            coins = _coins;
            userCoins = coins.userCoins;
        }

        public void callBet(IrcClient irc, ReadMessage msg)
        {
            if(irc.moderators.Contains(msg.user))
            {
                if (!betRunning)
                {

                }
                else
                    irc.sendChatMessage("A bet is already running. Close the bet first!");
            }
        }
        //stub
    }
}
