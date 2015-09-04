using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotConsole
{
    class Bet
    {
        IrcClient irc;
        Slots slots;
        private bool betRunning = false;

        public Bet(IrcClient _irc,Slots _Slots)
        {
            irc = _irc;
            slots = _Slots;
        }

        public void callBet(ReadMessage msg)
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
