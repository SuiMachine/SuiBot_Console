using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;

namespace TwitchBotConsole
{
    public static class Updater
    {
        public static void Check(Version CurrentVersion)
        {
            try
            {
                string uriDirectoryForFiles = "";
                string infoUri = "update.xml";
                List<string> listOfFiles = new List<string>();


                /*HttpWebRequest request = (HttpWebRequest)WebRequest.Create(infoUri);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Close();*/

                XmlDocument doc = new XmlDocument();
                doc.Load(infoUri);
                
                foreach (XmlNode update in doc.SelectNodes("updates/update"))
                {
                    Version CheckedVersion = Version.Parse(update.SelectSingleNode("version").InnerText);
                    if(CurrentVersion < CheckedVersion)
                    {
                        foreach (XmlNode file in update.SelectNodes("file"))
                            if (!listOfFiles.Contains(file.InnerText)) listOfFiles.Add(file.InnerText);
                    }
                }

                foreach(string element in listOfFiles)
                {
                    Console.WriteLine("File" + element);
                }
            }
            catch
            {
                Console.WriteLine("Error finding an update.");
            }
        }
    }
}
