using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.ComponentModel;

namespace TwitchBotConsole
{
    public static class Updater
    {
        public static void Check(Version CurrentVersion)
        {
            try
            {
                string uriDirectoryForFiles = "https://raw.githubusercontent.com/SuiMachine/SuiBot_Console/Testing/UpdateDirectory/";
                string infoUri = "update.xml"; //https://raw.githubusercontent.com/SuiMachine/SuiBot_Console/Testing/UpdateDirectory/
                List<string> listOfFiles = new List<string>();

                if(true) //CheckIfXMLExists(infoUri)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(infoUri);

                    foreach (XmlNode update in doc.SelectNodes("updates/update"))
                    {
                        Version CheckedVersion = Version.Parse(update.SelectSingleNode("version").InnerText);
                        if (CurrentVersion < CheckedVersion)
                        {
                            foreach (XmlNode file in update.SelectNodes("file"))
                                if (!listOfFiles.Contains(file.InnerText)) listOfFiles.Add(file.InnerText);
                        }
                    }

                    foreach (string element in listOfFiles)
                    {
                        Console.WriteLine("File: " + element);
                    }

                    bool result = downloadFiles(listOfFiles, uriDirectoryForFiles);
                }
                else
                {
                    Console.WriteLine("ERROR: Couldn't recieve XML file.");
                }
            }
            catch
            {
                Console.WriteLine("Error finding an update.");
            }
        }

        private static bool downloadFiles(List<string> listOfFiles, string sourceLocation)
        {
            WebClient wbClient = new WebClient();
            wbClient.DownloadFileCompleted += WbClient_DownloadFileCompleted;

            int numberOfFiles = listOfFiles.Count;
            string[,] tempFiles = new string[numberOfFiles,2];
            for(int i=0; i<numberOfFiles; i++)
            {
                tempFiles[i, 0] = listOfFiles[i];
                tempFiles[i, 1] = Path.GetTempFileName();
                Console.WriteLine("Update: " + tempFiles[i, 0] + " -> " + tempFiles[i, 1]);
            }

            for(int i=0; i<numberOfFiles; i++)
            {
                Uri tempUri;
                Uri.TryCreate(sourceLocation + tempFiles[i, 0], UriKind.Absolute, out tempUri);

                try { wbClient.DownloadFileAsync(tempUri, tempFiles[i, 1]); }
                catch { return false; }
            }

            return false;
        }

        private static void WbClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if(e.Error!=null)
            {
                Console.WriteLine("Error downloading file!");
            }
            else
            {
                Console.WriteLine("Downloading file completed");

            }
        }

        private static bool CheckIfXMLExists(string uri)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                resp.Close();

                return resp.StatusCode == HttpStatusCode.OK;
            }
            catch { return false; }
        }
    }
}
