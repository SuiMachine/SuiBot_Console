using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;

namespace TwitchBotConsole
{
    public static class Updater
    {
        public static Version Check()
        {
            try
            {
                string name = "SuiBot";
                string infoUri = "https://raw.githubusercontent.com/SuiMachine/SuiBot_Console/Testing/Update/update.xml";
                string url;
                string fileName;
                Version version;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(infoUri);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Close();

                XmlDocument doc = new XmlDocument();
                doc.Load(infoUri);
                Console.WriteLine("Testing:" + doc.DocumentElement.InnerText);

                return new Version("0.0.0.0");

                /*
                if (node == null)
                {
                    Console.WriteLine("Error finding an update - no XML node found.");
                    return new Version("0.0.0.0");
                }
                else
                {
                    version = Version.Parse(node["version"].InnerText);
                    url = node["urlLocation"].InnerText;
                    fileName = node["filename"].InnerText;

                    return version;
                }*/
            }
            catch
            {
                Console.WriteLine("Error finding an update.");
                return new Version("0.0.0.0");
            }
        }
    }
}
