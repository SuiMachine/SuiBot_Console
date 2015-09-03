using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeedrunComSharp;
using System.Diagnostics;

namespace TwitchBotConsole
{
    class Leaderboards
    {
        IrcClient irc;
        ReadMessage msg;
        Json_status json;

        public void recieveData(IrcClient _irc, ReadMessage _msg)
        {
            irc = _irc;
            msg = _msg;
        }

        public void getLeaderboard()
        {
            if (irc.moderators.Contains(msg.user) || irc.trustedUsers.Contains(msg.user))
            {
                var srlClient = new SpeedrunComClient();
                int indexGameStart = msg.message.IndexOf(' ');
                int indexGameCathegoryStart = msg.message.IndexOf(':');

                try
                {
                    if(indexGameStart > 0)
                    {
                        string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                        if (indexGameCathegoryStart > 0 && msg.message.ElementAt(indexGameCathegoryStart+1) != ' ')
                        {
                            string[] additionalhelper = helper[1].Split(new char[] { ':' }, 2);
                            var game = srlClient.Games.SearchGame(name: additionalhelper[0]);
                            if (additionalhelper[1].ToLower().StartsWith("cat"))
                            {
                                string output = "Categories are:";
                                int id = 1;
                                foreach (var element in game.Categories)
                                {
                                    output = output + " [" + id.ToString() + "]" + element.ToString();
                                    id++;
                                }
                                irc.sendChatMessage(output);
                            }
                            else
                            {
                                int id = 0;
                                if (int.TryParse(additionalhelper[1], out id))
                                {
                                    id--;
                                    if (game.Categories.Count > id)
                                    {
                                        var _category = game.Categories[id];

                                        if (_category.WorldRecord != null)
                                        {
                                            //Finding the World Record of the category
                                            var worldRecord = _category.WorldRecord;

                                            if(worldRecord.Players.Count>1)
                                            {
                                                string players = "" + worldRecord.Players[0].User;
                                                int i = 1;
                                                for (; i<worldRecord.Players.Count-1; i++)
                                                {
                                                    players = players + ", " + worldRecord.Players[i].User;
                                                }
                                                players = players + " and " + worldRecord.Players[i].User;
                                                irc.sendChatMessage("World record for " + game + " (" + _category + ") is " + worldRecord.Times.Primary + " by " + players + ". " + _category.WebLink.AbsoluteUri);
                                            }
                                            else
                                                irc.sendChatMessage("World record for " + game + " (" + _category + ") is " + worldRecord.Times.Primary + " by " + worldRecord.Player.User + ". " + _category.WebLink.AbsoluteUri);
                                        }
                                        else
                                            irc.sendChatMessage("Currently there is no world record for this category. " + _category.WebLink.AbsoluteUri);
                                    }
                                    else
                                        irc.sendChatMessage("Wrong category ID!");
                                }
                                else
                                    irc.sendChatMessage("Failed to parse category ID.");

                            }
                        }
                        else
                        {
                            var game = srlClient.Games.SearchGame(name: helper[1]);

                            var _category = game.Categories[0];

                            if (_category.WorldRecord != null)
                            {
                                //Finding the World Record of the category
                                var worldRecord = _category.WorldRecord;

                                            if(worldRecord.Players.Count>1)
                                            {
                                                string players = "" + worldRecord.Players[0].User;
                                                int i = 1;
                                                for (; i<worldRecord.Players.Count-1; i++)
                                                {
                                                    players = players + ", " + worldRecord.Players[i].User;
                                                }
                                                players = players + " and " + worldRecord.Players[i].User;
                                                irc.sendChatMessage("World record for " + game + " (" + _category + ") is " + worldRecord.Times.Primary + " by " + players + ". " + _category.WebLink.AbsoluteUri);
                                            }
                                            else
                                                irc.sendChatMessage("World record for " + game + " (" + _category + ") is " + worldRecord.Times.Primary + " by " + worldRecord.Player.User + ". " + _category.WebLink.AbsoluteUri);                            }
                            else
                                irc.sendChatMessage("Currently there is no world record for this category. " + _category.WebLink.AbsoluteUri);
                        }
                    }
                    else
                    {
                        if(json.game != String.Empty)
                        {
                            var game = srlClient.Games.SearchGame(name: json.game);

                            var _category = game.Categories[0];

                            if (_category.WorldRecord != null)
                            {
                                //Finding the World Record of the category
                                var worldRecord = _category.WorldRecord;

                                if (worldRecord.Players.Count > 1)
                                {
                                    string players = "" + worldRecord.Players[0].User;
                                    int i = 1;
                                    for (; i < worldRecord.Players.Count - 1; i++)
                                    {
                                        players = players + ", " + worldRecord.Players[i].User;
                                    }
                                    players = players + " and " + worldRecord.Players[i].User;
                                    irc.sendChatMessage("World record for " + game + " (" + _category + ") is " + worldRecord.Times.Primary + " by " + players + ". " + _category.WebLink.AbsoluteUri);
                                }
                                else
                                    irc.sendChatMessage("World record for " + game + " (" + _category + ") is " + worldRecord.Times.Primary + " by " + worldRecord.Player.User + ". " + _category.WebLink.AbsoluteUri);
                            }
                            else
                                irc.sendChatMessage("Currently there is no world record for this category. " + _category.WebLink.AbsoluteUri);
                        }
                    }
                }
                catch(Exception ex)
                {
                    irc.sendChatMessage("Nothing found. Maybe you'd like to visit http://speedrun.com and look for it yourself FrankerZ");
                    Debug.WriteLine(ex.ToString());
                }

            }
            else
            {
                irc.sendChatMessage("You don't have permissions to perform this command");
            }
        }

        internal void SendJsonPointer(Json_status _jsonStatus)
        {
            json = _jsonStatus;
        }
    }
}
