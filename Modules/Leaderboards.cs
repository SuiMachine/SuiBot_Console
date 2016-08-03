using System;
using System.Collections.Generic;
using System.Linq;
using SpeedrunComSharp;
using System.Diagnostics;

namespace TwitchBotConsole
{
    class Leaderboards
    {
        //This class is a total mess
        //Enjoy!
        oldIRCClient irc;
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
            proxyName.Add("Star Wars: Jedi Knight II - Jedi Outcast", "jk2");
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
            proxyName.Add("F.E.A.R.: First Encounter Assault Recon", "F.E.A.R.");
            proxyName.Add("Judge Dredd: Dredd vs Death", "dreddgasm");
            proxyName.Add("Heroes of Might and Magic II: The Price of Loyalty", "Heroes of Might and Magic II");
            proxyName.Add("Heroes of Might and Magic III: The Restoration of Erathia", "Heroes of Might and Magic III");
            proxyName.Add("Heroes of Might and Magic III: Armageddon's Blade", "Heroes of Might and Magic III");
            proxyName.Add("Heroes of Might and Magic III: In the wake of gods", "Heroes of Might and Magic III");
            proxyName.Add("Heroes of Might and Magic III: The Shadow of Death", "Heroes of Might and Magic III");
            proxyName.Add("Heroes of Might and Magic IV", "Heroes of Might and Magic III");
            proxyName.Add("Heroes of Might and Magic 3", "Heroes of Might and Magic III");
			proxyName.Add("homm3", "Heroes of Might and Magic III");
            proxyName.Add("Trespasser: Jurassic Park", "Jurassic Park: Trespasser");
            proxyName.Add("Hitman (2016)", "just_hitman");
            proxyName.Add("Star Wars: Knights of the Old Republic II - The Sith Lords", "Star Wars: Knights of the Old Republic 2 - The Sith Lords");
        }

        public void recieveData(oldIRCClient _irc, ReadMessage _msg)
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
                //irc.sendChatMessage("You don't have permissions to perform this command");
            }
        }

        private void getAndReturnLeaderboard()
        {
            int indexGameStart = msg.message.IndexOf(' ');
            int indexGameCathegoryStart = findIndexOfACharacterStartingCategory(msg.message);

            if (indexGameStart > 0)
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);
                if (indexGameCathegoryStart > 3)
                {
                    indexGameCathegoryStart = indexGameCathegoryStart - 3;
                    string gameName = helper[1].Substring(0, indexGameCathegoryStart - 1);
                    string variable = helper[1].Substring(indexGameCathegoryStart, helper[1].Length - indexGameCathegoryStart);
                    if (variable.ToLower().StartsWith("cat"))
                    {
                        displayCategories(gameName);
                    }
                    else if (variable.ToLower().StartsWith("level"))
                    {
                        getBestLevelTimeForGivenLevel(gameName, variable);
                    }
                    else
                    {
                        displayBestTimeFromGivenCategory(gameName, variable);
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

        #region ILs
        private void displayBestLevelTimeForDefaultCategory(string gameName, string levelIndex)
        {
            try
            {
                var srlClient = new SpeedrunComClient();
                if (gameName == "this")
                {
                    if (json.isForcedPage)
                    {
                        gameName = json.forcedGame;
                    }
                    else
                    {
                        gameName = json.game;
                    }
                }
                var game = srlClient.Games.SearchGame(name: getProxyName(gameName));

                if (game != null)
                {
                    int id = 0;
                    if (int.TryParse(levelIndex, out id))
                    {
                        id--;
                        if (game.Levels[id] != null)
                        {
                            var leaderboard = srlClient.Leaderboards.GetLeaderboardForLevel(game.ID, game.Levels[id].ID, game.LevelCategories.First().ID, null, null);
                            if (leaderboard.Records.Count > 0)
                            {
                                var record = leaderboard.Records[0];

                                if (record.Players.Count > 1)       //if there is more than 1 player, build a string for them
                                {
                                    string players = "" + record.Players[0].User;
                                    int i = 1;
                                    for (; i < record.Players.Count - 1; i++)
                                    {
                                        players = players + ", " + record.Players[i].User;
                                    }
                                    players = players + " and " + record.Players[i].User;
                                    irc.sendChatMessage("World record for level '" + record.Level.Name + "' is " + record.Times.Primary.ToString() + " by " + players + " " + record.WebLink.AbsoluteUri);
                                }
                                else
                                    irc.sendChatMessage("World record for level '" + record.Level.Name + "' is " + record.Times.Primary.ToString() + " by " + record.Player.Name + " " + record.WebLink.AbsoluteUri);
                            }
                            else
                                irc.sendChatMessage("There doesn't seem to be a WR for '" + leaderboard.Level.Name + "' " + leaderboard.WebLink.AbsoluteUri);
                        }
                        else
                            irc.sendChatMessage("Wrong level ID");
                    }
                    else
                        irc.sendChatMessage("Failed to parse level ID.");
                }
                else
                {
                    irc.sendChatMessage("Game was not found!");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("EXCEPTION ERROR: " + ex.ToString());
                irc.sendChatMessage("Exception error!");
            }
        }

        private void getBestLevelTimeForGivenLevel(string gameName, string restOfMsg)
        {
            if (restOfMsg.ToLower() == "level" || restOfMsg.ToLower() == "levels")
            {
                displayLevelsFromAGame(gameName);
            }
            else
            {
                string levelId = restOfMsg.Substring(6, restOfMsg.Length - 6);
                displayBestLevelTimeForDefaultCategory(gameName, levelId);
            }
        }

        private void displayLevelsFromAGame(string gameName)
        {
            try
            {
                var srlClient = new SpeedrunComClient();
                if (gameName == "this")
                {
                    if(json.isForcedPage)
                    {
                        gameName = json.forcedGame;
                    }
                    else
                    {
                        gameName = json.game;
                    }
                }

                var game = srlClient.Games.SearchGame(name: getProxyName(gameName));
                if (game != null)        //If game was found -> Build a string and display all levels
                {
                    if (game.Levels.Count > 0)
                    {
                        string output = "Levels are:";
                        int id = 1;
                        foreach (var element in game.Levels)
                        {
                            output = output + " [" + id.ToString() + "]" + element.Name;
                            id++;
                        }
                        irc.sendChatMessage(output);
                    }
                    else
                        irc.sendChatMessage("The game doesn't seem to have specified levels");

                }
                else
                {
                    irc.sendChatMessage("No game was found");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("EXCEPTION ERROR: " + ex.ToString());
                irc.sendChatMessage("Exception error!");
            }
        }

        private void getPBTimeForGivenLevel(string gameName, string restOfMsg)
        {
            if (restOfMsg.ToLower() == "level" || restOfMsg.ToLower() == "levels")
            {
                displayLevelsFromAGame(gameName);
            }
            else
            {
                string levelId = restOfMsg.Substring(6, restOfMsg.Length - 6);
                displayPBTimeForGivenLevel(gameName, levelId);
            }
        }

        private void displayPBTimeForGivenLevel(string gameName, string levelIndex)
        {
            try
            {
                var srlClient = new SpeedrunComClient();
                if (gameName == "this")
                {
                    if (json.isForcedPage)
                    {
                        gameName = json.forcedGame;
                    }
                    else
                    {
                        gameName = json.game;
                    }
                }

                var game = srlClient.Games.SearchGame(name: getProxyName(gameName));

                if (game != null)
                {
                    var gameID = game.ID;
                    //Display PB from a category
                    int id = 0;
                    if (int.TryParse(levelIndex, out id))
                    {
                        id--;
                        if (game.Levels.Count > id && id >= 0)
                        {
                            var _levelID = game.Levels[id].ID;
                            var _levelCategoryID = game.LevelCategories.First().ID;
                            var playersPB = srlClient.Users.GetPersonalBests(irc.SpeedrunName, null, null, gameID);
                            if (playersPB != null)
                            {
                                int numberOfPBs = playersPB.Count;
                                int i = 0;
                                for (i = 0; i < numberOfPBs; i++)
                                {
                                    if (playersPB[i].CategoryID == _levelCategoryID && playersPB[i].LevelID == _levelID)
                                        break;
                                }

                                if (i < numberOfPBs)
                                {
                                    irc.sendChatMessage("Strimmer PB for the level " + playersPB[i].Level.Name + " is: " + playersPB[i].Times.Primary + ". " + playersPB[i].WebLink.AbsoluteUri);
                                }
                                else
                                    irc.sendChatMessage("Doesn't seem like a strimmer ran this level FrankerZ");
                            }
                            else
                                irc.sendChatMessage("Doesn't seem like a strimmer ran this game");

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
            catch (Exception ex)
            {
                irc.sendChatMessage("Exception error!");
                Trace.WriteLine("EXCEPTION ERROR: " + ex.ToString());
            }
        }

        private void displayPBTimeForDefaultCategory()
        {
            int indexGameStart = msg.message.IndexOf(' ');
            int indexGameCathegoryStart = findIndexOfACharacterStartingCategory(msg.message);

            if (indexGameStart > 0)
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);

                if (indexGameCathegoryStart > 0)
                {
                    indexGameCathegoryStart = indexGameCathegoryStart - 3;
                    string gameName = helper[1].Substring(0, indexGameCathegoryStart - 1);
                    string variable = helper[1].Substring(indexGameCathegoryStart, helper[1].Length - indexGameCathegoryStart);
                    if (variable.ToLower().StartsWith("cat"))
                    {
                        displayCategories(gameName);
                    }
                    else if (variable.ToLower().StartsWith("level"))
                    {
                        getPBTimeForGivenLevel(gameName, variable);
                    }
                    else
                    {
                        displayPBFromGivenCategory(gameName, variable);
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
                //irc.sendChatMessage("You don't have permissions to perform this command");
            }
        }

        private void getAndReturnPB()
        {
            int indexGameStart = msg.message.IndexOf(' ');
            int indexGameCathegoryStart = findIndexOfACharacterStartingCategory(msg.message);

            if (indexGameStart > 0)
            {
                string[] helper = msg.message.Split(new char[] { ' ' }, 2);

                if (indexGameCathegoryStart > 0)
                {
                    indexGameCathegoryStart = indexGameCathegoryStart - 3;
                    string gameName = helper[1].Substring(0, indexGameCathegoryStart - 1);
                    string variable = helper[1].Substring(indexGameCathegoryStart, helper[1].Length - indexGameCathegoryStart);
                    if (variable.ToLower().StartsWith("cat"))
                    {
                        displayCategories(gameName);
                    }
                    else if (variable.ToLower().StartsWith("level"))
                    {
                        getPBTimeForGivenLevel(gameName, variable);
                    }
                    else
                    {
                        displayPBFromGivenCategory(gameName, variable);
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
        private int findIndexOfACharacterStartingCategory(string message)
        {
            for (int i = 1; i < message.Length - 1; i++)
            {
                if (char.IsLetterOrDigit(message.ElementAt(i - 1)) && message.ElementAt(i) == ':' && char.IsLetterOrDigit(message.ElementAt(i + 1)))
                {
                    return i;
                }
            }
            return 0;
        }

        private void displayCategories(string gameName)
        {
            try
            {
                var srlClient = new SpeedrunComClient();
                if(gameName == "this")
                {
                    if(json.isForcedPage)
                    {
                        gameName = json.forcedGame;
                    }
                    else
                    {
                        gameName = json.game;
                    }
                }


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
                if (gameName == "this")
                {
                    if (json.isForcedPage)
                    {
                        gameName = json.forcedGame;
                    }
                    else
                    {
                        gameName = json.game;
                    }
                }

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
                if (gameName == "this")
                {
                    if (json.isForcedPage)
                    {
                        gameName = json.forcedGame;
                    }
                    else
                    {
                        gameName = json.game;
                    }
                }

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
                if (gameName == "this")
                {
                    if (json.isForcedPage)
                    {
                        gameName = json.forcedGame;
                    }
                    else
                    {
                        gameName = json.game;
                    }
                }

                var game = srlClient.Games.SearchGame(name: getProxyName(gameName));
                if (game != null)
                {
                    var gameID = game.ID;
                    var playersPB = srlClient.Users.GetPersonalBests(irc.SpeedrunName, null, null, gameID);
                    if (playersPB.Count > 0)
                    {
                        if (playersPB.Any(run => run.Category.ID == game.Categories[0].ID))
                        {
                            Record tehUrn = playersPB.First(run => run.Category.ID == game.Categories[0].ID);
                            irc.sendChatMessage("Strimmer PB in " + tehUrn.Game.Name + " (" + tehUrn.Category.Name + ") is: " + tehUrn.Times.Primary + ". " + tehUrn.WebLink);
                        }
                        else
                        {
                            irc.sendChatMessage("Doesn't seem like a streamer ran the main cathegory for this game. FrankerZ");
                        }
                    }
                    else
                        irc.sendChatMessage("Doesn't seem like a streamer ran this game FrankerZ");

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
                if (gameName == "this")
                {
                    if (json.isForcedPage)
                    {
                        gameName = json.forcedGame;
                    }
                    else
                    {
                        gameName = json.game;
                    }
                }

                var game = srlClient.Games.SearchGame(name: getProxyName(gameName));

                if(game!= null)
                {
                    var gameID = game.ID;
                    //Display PB from a category
                    int id = 0;
                    if (int.TryParse(categoryIndex, out id))
                    {
                        id--;
                        if (game.FullGameCategories.Count() > id && id >= 0)
                        {
                            var _category = game.FullGameCategories.ElementAt(id);
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
                            else
                                irc.sendChatMessage("Doesn't seem like a strimmer ran this category. FrankerZ");
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
