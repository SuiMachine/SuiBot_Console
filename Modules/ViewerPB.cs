using System.IO;


namespace TwitchBotConsole
{
    class ViewerPB
    {
        string directory = "cache";
        string userInfoFile = "userpb.txt";
        public uint viewerPB = 0;
        IrcClient irc;

        public ViewerPB(IrcClient _irc)
        {
            irc = _irc;
            if(File.Exists(Path.Combine(directory, userInfoFile)))
            {
                loadData();
            }
        }

        public void CheckViewerPB(uint newData)
        {
            if(newData > viewerPB)
            {
                viewerPB = newData;
                savePB();
            }
        }

        private void savePB()
        {
            string output = "ViewerPB:" + viewerPB.ToString();
            File.WriteAllText(Path.Combine(directory, userInfoFile), output);
        }

        private void loadData()
        {
            StreamReader SR = new StreamReader(Path.Combine(directory, userInfoFile));

            string line = "";

            while ((line = SR.ReadLine()) != null)
            {
                if (line.StartsWith("ViewerPB:"))
                {
                    string[] helper = line.Split(':');
                    uint value = 0;
                    if (uint.TryParse(helper[1], out value))
                    {
                        viewerPB = value;
                    }
                }
            }
            SR.Close();
            SR.Dispose();
        }

        internal void displayViewerPB(ReadMessage msg)
        {
            if(irc.moderators.Contains(msg.user))
            {
                irc.sendChatMessage("Viewer PB for this channel is: " + viewerPB.ToString());
            }
        }
    }
}
