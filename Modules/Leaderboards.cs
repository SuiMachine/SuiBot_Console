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

        Dictionary<string, string> proxyName = new Dictionary<string, string>();

        public Leaderboards()
        {
            loadProxyNames();
        }

        private void loadProxyNames()
        {
            proxyName.Add("Star Wars: Jedi Knight - Jedi Academy", "jka");
            proxyName.Add("Darksiders II", "Darksiders 2");
            proxyName.Add("GTA3", "gtaiii");
            proxyName.Add("GTA 3", "gtaiii");
            proxyName.Add("Zork", "Zork I: The Great Underground Empire");
            proxyName.Add("Zork I", "Zork I: The Great Underground Empire");
            proxyName.Add("Zork 2", "Zork II: The Wizard of Frobozz");
            proxyName.Add("Zork II", "Zork II: The Wizard of Frobozz");
            proxyName.Add("Zork 3", "Zork III: The Dungeon Master");
            proxyName.Add("Zork III", "Zork III: The Dungeon Master");
            proxyName.Add("Thief", "Thief: The Dark Project");
            proxyName.Add("Thief: Gold", "Thief Gold");
            proxyName.Add("Thief 2", "Thief ll: The Metal Age");
            proxyName.Add("Thief II", "Thief ll: The Metal Age");
        }

        public void recieveData(IrcClient _irc, ReadMessage _msg)
        {
            irc = _irc;
            msg = _msg;
        }

        #region WRs
        public void getLeaderboard()
        {
            if (irc.moderators.Contains(msg.user) || irc.trustedUsers.Contains(msg.user))
            {
                getAndReturnLeaderboard();
            }
            else
            {
                irc.sendChatMessage("You don't have permissions to perform this command");
            }
        }

        private void getAndReturnLeaderboard()
        {
            int indexGameStart = msg.message.IndexOf(' ');
            int indexGameCathegoryStart = msg.message.IndexOf(':');

            if (indexGameStart > 0)
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                if (indexGameCathegoryStart > 0 && msg.message.ElementAt(indexGameCathegoryStart + 1) != ' ')
                {
                    string[] additionalhelper = helper[1].Split(new char[] { ':' }, 2);
                    if (additionalhelper[1].ToLower().StartsWith("cat"))
                    {
                        displayCategories(additionalhelper[0]);
                    }
                    else
                    {
                        displayBestTimeFromGivenCategory(additionalhelper[0], additionalhelper[1]);
                    }
                }
                else
                {
                    displayBestTimeFromDefaultCategory(helper[1]);
                }
            }
            else
            {   //Find a best time from default cathegory, based on currently played game on Twitch
                if (json.isForcedPage)
                {
                    displayBestTimeFromDefaultCategory(json.forcedGame);
                }
                else if (json.game != String.Empty)
                {
                    displayBestTimeFromDefaultCategory(json.game);
                }
                else
                    irc.sendChatMessage("Currently there is no active game.");
            }
        }
        #endregion

        #region PBs
        public void getPB()
        {
            if (irc.moderators.Contains(msg.user) || irc.trustedUsers.Contains(msg.user))
            {
                if(irc.SpeedrunName!="")
                {
                    getAndReturnPB();
                }
                else
                {
                    irc.sendChatMessage("Speedrun.com name wasn't set. Can't find PB!");
                }
            }
            else
            {
                irc.sendChatMessage("You don't have permissions to perform this command");
            }
        }

        private void getAndReturnPB()
        {
            int indexGameStart = msg.message.IndexOf(' ');
            int indexGameCathegoryStart = msg.message.IndexOf(':');

            if (indexGameStart > 0)
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);

                if (indexGameCathegoryStart > 0 && msg.message.ElementAt(indexGameCathegoryStart + 1) != ' ')
                {
                    string[] additionalhelper = helper[1].Split(new char[] { ':' }, 2);
                    if (additionalhelper[1].ToLower().StartsWith("cat"))
                    {
                        displayCategories(additionalhelper[0]);
                    }
                    else
                    {
                        displayPBFromGivenCategory(additionalhelper[0], additionalhelper[1]);
                    }
                }
                else
                {
                    //Find a PB from a user provided game
                    displayPBFromAGame(helper[1]);
                }
            }
            else
            {   //Find a PB in currently streamed game
                if (json.isForcedPage)
                {
                    displayPBFromAGame(json.forcedGame);
                }
                else if (json.game != String.Empty)
                {
                    displayPBFromAGame(json.game);
                }
                else
                    irc.sendChatMessage("Currently there is no active game.");
            }
        }
        #endregion

        #region Functions
        private void displayCategories(string gameName)
        {
            try
            {
                var srlClient = new SpeedrunComClient();
                var game = srlClient.Games.SearchGame(name: getProxyName(gameName));
                if (game != null)        //If game was found -> Build a string and display all categories
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
                    irc.sendChatMessage("No game was found");
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine("EXECPTION ERROR: " + ex.ToString());
                irc.sendChatMessage("Exception error!");
            }

        }

        private void displayBestTimeFromGivenCategory(string gameName, string categoryIndex)
        {
            try
            {
                var srlClient = new SpeedrunComClient();
                var game = srlClient.Games.SearchGame(name: getProxyName(gameName));

                if (game != null)
                {
                    int id = 0;
                    if (int.TryParse(categoryIndex, out id))
                    {
                        id--;
                        if (game.Categories.Count > id)
                        {
                            var _category = game.Categories[id];

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
                        else
                            irc.sendChatMessage("Wrong category ID!");
                    }
                    else
                        irc.sendChatMessage("Failed to parse category ID.");
                }
                else
                {
                    irc.sendChatMessage("Game was not found!");
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine("EXCEPTION ERROR: " + ex.ToString());
                irc.sendChatMessage("Exception error!");
            }
        }

        private void displayBestTimeFromDefaultCategory(string gameName)
        {
            try
            {
                var srlClient = new SpeedrunComClient();
                var game = srlClient.Games.SearchGame(name: getProxyName(gameName));
                if(game != null)
                {
                    var _category = game.Categories[0];

                    if (_category.WorldRecord != null)
                    {
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
                else
                {
                    irc.sendChatMessage("No game was found!");
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine("EXCEPTIO ERROR: " + ex.ToString());
                irc.sendChatMessage("Exception error");
            }
        }

        private void displayPBFromAGame(string gameName)
        {
            try
            {
                var srlClient = new SpeedrunComClient();
                var game = srlClient.Games.SearchGame(name: getProxyName(gameName));
                if (game != null)
                {
                    var gameID = game.ID;
                    var playersPB = srlClient.Users.GetPersonalBests(irc.SpeedrunName, null, null, gameID);
                    Record tehUrn;
                    if((tehUrn = playersPB.First(run => run.Category.ID == game.Categories[0].ID)) != null)
                    {
                        irc.sendChatMessage("Strimmer PB in " + tehUrn.Game.Name + " (" + tehUrn.Category.Name + ") is: " + tehUrn.Times.Primary + ". " + tehUrn.WebLink);
                    }
                    else
                    {
                        irc.sendChatMessage("Doesn't seem like a streamer ran the main cathegory for this game. FrankerZ");
                    }
                }
                else
                {
                    irc.sendChatMessage("No game was found!");
                }
            }
            catch (Exception ex)
            {
                irc.sendChatMessage("Exception error");
                Trace.WriteLine("EXCEPTION ERROR: " + ex.ToString());
            }
        }

        private void displayPBFromGivenCategory(string gameName, string categoryIndex)
        {
            try
            {
                var srlClient = new SpeedrunComClient();
                var game = srlClient.Games.SearchGame(name: getProxyName(gameName));

                if(game!= null)
                {
                    var gameID = game.ID;
                    //Display PB from a category
                    int id = 0;
                    if (int.TryParse(categoryIndex, out id))
                    {
                        id--;
                        if (game.Categories.Count > id && id >= 0)
                        {
                            var _category = game.Categories[id];
                            string categoryName = _category.Name;
                            var playersPB = srlClient.Users.GetPersonalBests(irc.SpeedrunName, null, null, gameID);
                            int numberOfPBs = playersPB.Count;
                            int i = 0;
                            for (i = 0; i < numberOfPBs; i++)
                            {
                                if (playersPB[i].Category.Name == categoryName)
                                    break;
                            }

                            if (i < numberOfPBs)
                            {
                                irc.sendChatMessage("Strimmer PB for " + playersPB[i].Game.Name + " (" + playersPB[i].Category.Name + ") is: " + playersPB[i].Times.Primary);
                            }
                        }
                        else
                            irc.sendChatMessage("Wrong category ID!");
                    }
                    else
                        irc.sendChatMessage("Failed to parse category ID.");
                }
                else
                {
                    irc.sendChatMessage("No game found!");
                }

            }
            catch(Exception ex)
            {
                irc.sendChatMessage("Exception error!");
                Trace.WriteLine("EXCEPTION ERROR: " + ex.ToString());
            }
        }

        #endregion


        internal void SendJsonPointer(Json_status _jsonStatus)
        {
            json = _jsonStatus;
        }

        private string getProxyName(string name)
        {
            string proxy;
            if (proxyName.ContainsKey(name))
            {
                proxy = proxyName[name];
            }
            else
                proxy = name;
            return proxy;
        }
    }
}
