using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace TwitchBotConsole
{
    public static class Updater
    {
        //I HAVE NO IDEA HOW THIS WORKS BUT SOMEHOW I WROTE IT, PLEASE SEND HELP!
        public static bool CheckAndDownload(Version CurrentVersion, out string updaterPath)
        {
            try
            {
                string uriDirectoryForFiles = "https://raw.githubusercontent.com/SuiMachine/SuiBot_Console/Testing/UpdateDirectory/";
                string infoUri = "https://raw.githubusercontent.com/SuiMachine/SuiBot_Console/Testing/UpdateDirectory/update.xml";
                List<string> listOfFiles = new List<string>();

                if(CheckIfXMLExists(infoUri))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(infoUri);

                    foreach (XmlNode update in doc.SelectNodes("updates/update"))
                    {
                        //creates a list of files that needs to be updated
                        Version CheckedVersion = Version.Parse(update.SelectSingleNode("version").InnerText);
                        if (CurrentVersion < CheckedVersion)
                        {
                            foreach (XmlNode file in update.SelectNodes("file"))
                                if (!listOfFiles.Contains(file.InnerText)) listOfFiles.Add(file.InnerText);
                        }
                        else
                        {
                            Console.WriteLine("No update necessery.");
                            updaterPath = "";
                            return false;
                        }
                    }

                    string[,] locations;
                    bool result = downloadFiles(listOfFiles, uriDirectoryForFiles, out locations);

                    if(result)
                    {
                        Console.WriteLine("SUCCESS: All files successfully downloaded!");
                        updaterPath = CreateCMDUpdater(locations);
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Failed to download all the files!");
                        updaterPath = "";
                        return false;
                    }

                    return true;
                }
                else
                {
                    Console.WriteLine("ERROR: Failed to receive XML file from a server. This may be because of server being down.");
                    updaterPath = "";
                    return false;
                }
            }
            catch
            {
                Console.WriteLine("ERROR: Exception in Update module.");
                updaterPath = "";
                return false;
            }
        }

        private static string CreateCMDUpdater(string[,] locations)
        {
            //This part makes a CMD file that will be run once the bot's process is killed, replacing all of the files with files downloaded to temp folder
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Debug.WriteLine("CURRENT PATH: " + currentPath);
            string tempLocation = Path.GetTempPath();
            Debug.WriteLine("TEMP LOCATION: " + tempLocation);
            string fileCopying = "";
            for (int i = 0; i < locations.GetLength(0); i++)
            {
                fileCopying += "Del " + currentPath + "\\" + locations[i, 0] + "\n" +
                    "Choice /C Y /N /D Y /T 1\n" +
                    "ECHO " + locations[i, 1] + " -> " + currentPath + "\\"+ locations[i, 0] + "\n" +
                    "Move /Y " + locations[i, 1] + " " + currentPath + "\\" + locations[i, 0] + "\n";
            }

            string output = "@ECHO OFF\n" +
                "ECHO !!SuiBot Updater!!\n" +
                "Choice /C Y /N /D Y /T 4\n" +
                fileCopying +
                "Choice /C Y /N /D Y /T 1\n" +
                "Start \"\" /D " + currentPath + " " + "TwitchBotConsole.exe";
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "SuiBotUpdater.cmd"), output);

            string pathToCMDFile = Path.Combine(Path.GetTempPath(), "SuiBotUpdater.cmd");
            return pathToCMDFile;
        }

        private static bool downloadFiles(List<string> listOfFiles, string sourceLocation, out string[,] tempLocations)
        {
            WebClient wbClient = new WebClient();
            wbClient.DownloadFileCompleted += WbClient_DownloadFileCompleted;

            int numberOfFiles = listOfFiles.Count;
            tempLocations = new string[numberOfFiles,2];
            for(int i=0; i<numberOfFiles; i++)
            {
                tempLocations[i, 0] = listOfFiles[i];
                tempLocations[i, 1] = Path.GetTempFileName();
                Console.WriteLine("Update: " + tempLocations[i, 0] + " -> " + tempLocations[i, 1]);
            }

            for(int i=0; i<numberOfFiles; i++)
            {
                Uri tempUri;
                Uri.TryCreate(sourceLocation + tempLocations[i, 0], UriKind.Absolute, out tempUri);

                try { wbClient.DownloadFile(tempUri, tempLocations[i, 1]); }
                catch { return false; }
            }

            return true;
        }

        private static void WbClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if(e.Error!=null)
            {
                Trace.WriteLine("Error downloading file!");
            }
            else
            {
                Trace.WriteLine("Downloading file completed.");
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
