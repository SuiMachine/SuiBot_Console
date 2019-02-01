namespace TwitchBotConsole
{
    class PyramidBreaker
    {
        oldIRCClient irc;
        ReadMessage[] msgStack;

        public PyramidBreaker(oldIRCClient irc)
        {
            this.irc = irc;
            this.msgStack = new ReadMessage[2] { new ReadMessage(), new ReadMessage() };
            this.msgStack[0].user = "";
            this.msgStack[1].user = "";
        }

        public void breakPyramid(ReadMessage newMessage)
        {
            if(newMessage.message != "")
            {
                if (msgStack[0].user == "")
                {
                    msgStack[0] = newMessage;
                }
                else
                {
                    if (msgStack[0].user != newMessage.user)
                    {
                        msgStack[0].user = "";
                        msgStack[1].user = "";
                    }
                    else if (msgStack[1].user == "")
                    {
                        msgStack[1] = newMessage;
                    }
                    else
                    {
                        string[] words = new string[3];
                        if (getPyramidFromString(msgStack[0].message, 1, out words[0]) &&
                            getPyramidFromString(msgStack[1].message, 2, out words[1]) &&
                            getPyramidFromString(newMessage.message, 3, out words[2]))
                        {
                            if (words[0] == words[1] && words[1] == words[2])
                            {
                                msgStack[0].user = "";
                                msgStack[1].user = "";
                                irc.sendChatMessage("No! MrDestructoid ");
                            }
                            else
                            {
                                msgStack[0] = msgStack[1];
                                msgStack[1] = newMessage;
                            }
                        }
                        else
                        {
                            msgStack[0] = msgStack[1];
                            msgStack[1] = newMessage;
                        }
                    }
                }
            }    
        }

        private bool getPyramidFromString(string message, int numberOfWords, out string output)
        {
            if(message.EndsWith(" "))
                message = message.TrimEnd(' ');
            if (message.StartsWith(" "))
                message = message.TrimStart(' ');

            try
            {
                while (message.Contains("  "))
                    message.Replace("  ", " ");

                if (message.Contains(" "))
                {
                    string[] temp = message.Split(' ');
                    if(temp.Length != numberOfWords)
                    {
                        output = "";
                        return false;
                    }
                    else
                    {
                        string keyWord = temp[0];
                        for(int i=1; i<temp.Length; i++)
                        {
                            if(temp[i] != keyWord)
                            {
                                output = "";
                                return false;
                            }
                        }

                        output = keyWord;
                        return true;
                    }
                }
                else
                {
                    if(numberOfWords == 1)
                    {
                        output = message;
                        return true;
                    }
                    else
                    {
                        output = "";
                        return false;
                    }

                }
            }
            catch
            {
                output = "";
                return false;
            }
        }
    }
}
