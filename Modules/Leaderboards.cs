using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeedrunComSharp;

namespace TwitchBotConsole
{
    class Leaderboards
    {
        IrcClient irc;
        ReadMessage msg;

        public void recieveData(IrcClient _irc, ReadMessage _msg)
        {
            irc = _irc;
            msg = _msg;
        }

        public void getLeaderboard()
        {
            if (irc.moderators.Contains(msg.user))
            {
                var srlClient = new SpeedrunComClient();
                int indexGameStart = msg.message.IndexOf(' ');
                int indexGameCathegoryStart = msg.message.IndexOf(':');

                try
                {
                    string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                    if (indexGameStart > 0 && indexGameCathegoryStart > 0)
                    {
                        string[] additionalhelper = helper[1].Split(new char[] { ':' }, 2);
                        var game = srlClient.Games.SearchGame(name: additionalhelper[0]);

                        var _category = game.Categories.First(category => category.Name.ToLower() == additionalhelper[1].ToLower());

                        //Finding the World Record of the category
                        var worldRecord = _category.WorldRecord;

                        irc.sendChatMessage("World record for " + game + " (" + _category + ") is " + worldRecord.Times.Primary + " by " + worldRecord.Player.Name + ". http://www.speedrun.com/" + helper[1]);
                    }
                    else
                    {
                        var game = srlClient.Games.SearchGame(name: helper[1]);

                        var _category = game.Categories[0];

                        //Finding the World Record of the category
                        var worldRecord = _category.WorldRecord;

						irc.sendChatMessage("World record for " + game + " (" + _category + ") is " + worldRecord.Times.Primary + " by " + worldRecord.Player.Name + ". http://www.speedrun.com/" + helper[1].Replace(" ", "_"));
                    }
                }
                catch(Exception ex)
                {
                    irc.sendChatMessage("Nothing found. Maybe you'd like to visit http://speedrun.com and look for it yourself FrankerZ");
                }

            }
            else
            {
                irc.sendChatMessage("You don't have permissions to perform this command");
            }
        }
    }
}
