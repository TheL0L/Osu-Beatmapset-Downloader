using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Collections.Specialized;

namespace OsuBeatmapsetDownloader
{
    /// <summary>
    /// CookieAwareWebClient allows us to login on osu.ppy.sh
    ///                      and download the protected files
    /// </summary>
    public class CookieAwareWebClient : WebClient
    {
        public CookieAwareWebClient()
        {
            CookieContainer = new CookieContainer();
        }
        public CookieContainer CookieContainer { get; private set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            request.CookieContainer = CookieContainer;
            return request;
        }
    }

    class Program
    {
        static int DL_Count, DL_Complete, DL_Failed; // counters for downloads_count, and downloads_completed
        static string username, password; // credentials for downloading beatmaps

        [STAThread]
        static void Main(string[] args)
        {
            // change the window title
            Console.Title = "Osu! Beatmap Downloader";
            {
                Console.WriteLine("Greetings! :3\n");
                Console.WriteLine("Use /help to list all available commands.\n");

                Console.WriteLine("Please note that using the download commands will require entering your username & password");
                Console.WriteLine("of your osu! account. It's used to download the beatmaps and nothing else!");
                Console.WriteLine("Please note that until you use /clear command, your credentials will stay on the console window!\n\n");
            }

            // read the user input
            string input = Console.ReadLine();
            string[] parts;

            while (true)
            {
                // if the input is not empty
                if (input != String.Empty)
                {
                    // turns the command to lowercase and splits it into
                    //      [/command] [arg1] [arg2] [...]
                    parts = input.ToLower().Split(' ');

                    // add a space line between the input and result
                    Console.WriteLine();

                    // checks what command the user gave
                    AnalyzeInput(parts);
                }

                // read the user input
                input = Console.ReadLine();
            }
        }

        static void AnalyzeInput(string[] str)
        {
            // for each variant of input do the 
            switch (str[0])
            {
                case "/help":
                    {
                        // check if any parameters were given
                        if (str.Length == 1) Help();
                        else Help(str[1]);

                        break;
                    }
                case "/exit":
                    {
                        // close the application
                        Environment.Exit(0);
                        break;
                    }
                case "/savelist":
                    {
                        // check if any parameters were given
                        if (str.Length == 1) SaveToList(String.Empty);
                        else SaveToList(str[1]);
                        break;
                    }
                case "/dllist":
                    {
                        // check if any parameters were given
                        if (str.Length == 1) DownloadFromList(String.Empty);
                        else DownloadFromList(str[1]);
                        break;
                    }
                case "/dl":
                    {
                        // check if any parameters were given
                        if (str.Length == 1) Download(String.Empty);
                        else Download(str[1]);
                        break;
                    }
                case "/clear":
                    {
                        // clear the application window
                        Console.Clear();
                        break;
                    }

                // the input wasn't recognized as a command
                default:
                    {
                        Console.WriteLine("Please enter a vaild command, use /help to see available commands.\n");
                        break;
                    }
            }
        }

        static void Help()
        {
            Console.WriteLine("/help - list all available commands.");
            Console.WriteLine("/help <command> - show help on a specific command.\n");

            Console.WriteLine("/clear - clear the application window from text.");
            Console.WriteLine("    Use if you want to clear the application window.\n");

            Console.WriteLine("/dl <beatmapsetID> - download a beatmap set via its unique ID.");
            Console.WriteLine("    <beatmapsetID> - the unique ID of the mapset, by default it will open the beatmap sets listing page.");
            Console.WriteLine("    The beatmap set file will be saved on the desktop inside a folder called <OSU_DOWNLOADER_BEATMAPS>.\n");

            Console.WriteLine("/savelist <path> - save a list of all beatmap listing URLs in your current osu!\\songs folder.");
            Console.WriteLine("    <path> - the path to save the list, default is  {0}\\beatmaps_list.txt",
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            Console.WriteLine("    Use to share your beatmaps collection.\n");

            Console.WriteLine("/dllist <path> - download beatmaps from a saved list.");
            Console.WriteLine("    <path> - the path of the saved list, default is  {0}",
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            Console.WriteLine("    The beatmap set files will be saved on the desktop inside a folder called <OSU_DOWNLOADER_BEATMAPS>.");
            Console.WriteLine("    Use to quickly download the beatmaps listed in the file.\n");

            Console.WriteLine("/exit - close the application, you always can use the TaskManager or Alt+F4 or the [X] instead... \\(^_^)/\n");
        }

        static void Help(string args)
        {
            // check what command the user requested help for
            switch (args)
            {
                case "help":
                case "/help":
                    {
                        Console.WriteLine("/help - list all available commands.");
                        Console.WriteLine("/help <command> - show help on a specific command.\n");
                        break;
                    }
                case "dl":
                case "/dl":
                    {
                        Console.WriteLine("/dl <beatmapsetID> - download a beatmap set via its unique ID.");
                        Console.WriteLine("    <beatmapsetID> - the unique ID of the mapset, by default it will open the beatmap sets listing page.");
                        Console.WriteLine("    The beatmap set file will be saved on the desktop inside a folder called <OSU_DOWNLOADER_BEATMAPS>.\n");
                        break;
                    }
                case "savelist":
                case "/savelist":
                    {
                        Console.WriteLine("/savelist <path> - save a list of all beatmap listing URLs in your current osu!\\songs folder.");
                        Console.WriteLine("    <path> - the path to save the list, default is  {0}\\beatmaps_list.txt",
                            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
                        Console.WriteLine("    The beatmap set files will be saved on the desktop inside a folder called <OSU_DOWNLOADER_BEATMAPS>.");
                        Console.WriteLine("    Use to share your beatmaps collection.\n");
                        break;
                    }
                case "dllist":
                case "/dllist":
                    {
                        Console.WriteLine("/dllist <path> - download beatmaps from a saved list.");
                        Console.WriteLine("    <path> - the path of the saved list, default is  {0}",
                            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
                        Console.WriteLine("    Use to quickly download the beatmaps listed in the file.\n");
                        break;
                    }
                case "exit":
                case "/exit":
                    {
                        Console.WriteLine("/exit - close the application, you always can use the TaskManager or Alt+F4 or the [X] instead... \\(^_^)/\n");
                        break;
                    }
                case "clear":
                case "/clear":
                    {
                        Console.WriteLine("/clear - clear the application window from text.");
                        Console.WriteLine("    Use if you want to clear the application window.\n");
                        break;
                    }
                default:
                    {
                        // if the parameter wasn't recognized just show help
                        Help();
                        break;
                    }
            }
        }

        static void SaveToList(string args)
        {
            // in case the user wants to use the default path, it will be the desktop
            if (args == String.Empty) args = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            // the default osu! path is %appdata%\Local\osu!\
            string osu_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\osu!";

            // ask the user if it's indeed the correct osu! path
            Console.WriteLine(@"Is this <{0}> your osu! folder path? [y/n]", osu_path);

            // read the answer
            char answer = Console.ReadLine()[0];
            answer = Char.ToLower(answer);
            Console.WriteLine();

            if (answer == 'n') // if no
            {
                Console.WriteLine("Please enter your osu! folder path as showed above: ");
                osu_path = Path.GetFullPath(Console.ReadLine()); // read new path
            }
            else if (answer != 'y') // if it's not y/n answer
            {
                Console.WriteLine("You haven't gave valid answer, re-enter the command to retry...");
                return; // return to the main function
            }
            // if the answer was yes, keep running the code..

            // code segment that writes the IDs to a file
            {
                // create a reference for the save path
                DirectoryInfo SavePath = new DirectoryInfo(args);
                // if the path doesn't exist, create it to avoid any errors..
                if (!SavePath.Exists) Directory.CreateDirectory(args);

                // create a list to hold the URLs
                List<string> URLs = new List<string>();
                URLs.Clear();

                // get the beatmapsets IDs
                List<int> IDs = GetSetIDs(osu_path);

                // if the method 'GetSetIDs' returned null, it means that there was an error
                if (IDs == null) return; // return to the main function

                // for each ID in the list we got before,
                foreach (int id in IDs)
                {
                    // add it to the URLs list
                    URLs.Add(GetDownloadURL(id));
                }

                // save the URLs list into a file at SavePath\beatmaps_list.txt
                File.WriteAllLines(Path.Combine(SavePath.FullName, "beatmaps_list.txt"), URLs);
            }

            // print a debug message to inform the user
            Console.WriteLine("List was saved at <{0}\\beatmaps_list.txt>.\n", args);
        }

        static List<int> GetSetIDs(string _path)
        {
            string path;
            // check the path and relocate it to point at osu!\Songs folder
            {
                // if the folder contains osu!.exe
                if (File.Exists(_path + @"\osu!.exe"))
                {
                    // set the corrected path
                    path = _path + @"\Songs";
                }
                // if the subfolder contains osu!.exe
                else if (File.Exists(_path + @"\..\osu!.exe"))
                {
                    // set the corrected path
                    path = _path.Substring(0, _path.LastIndexOf('\\')) + @"\Songs";
                }
                // if it's not an osu! folder at all
                else
                {
                    Console.WriteLine("The specified path <{0}> is incorrect, re-enter the command to retry...\n", _path);
                    return null; // return null as a sign of error
                }
            }

            // create a list to hold the beatmapset IDs
            List<int> IDs = new List<int>();
            IDs.Clear();

            // get the beatmapsetIDs and add them to the list
            {
                // get all first-level-subDirectories
                DirectoryInfo[] subFolders = new DirectoryInfo(path).GetDirectories();

                // foreach directoryinfo 'sub' in 'subFolders'
                foreach (DirectoryInfo sub in subFolders)
                {
                    // get the folder name
                    string name = sub.Name;

                    // get the ID from it by splitting the name of the folder to
                    // [BeatMapSetID] [Artist] [Title]
                    name = name.Split(' ')[0];

                    // try to parse the ID from the first segment of the name
                    // if it can parse an integer value,  the result will be true as a sign of success
                    bool result = Int32.TryParse(name, out int ID);

                    // if it's a valid beatmapsetID
                    if (result && ID > 0)
                    {
                        // add the ID to the list
                        IDs.Add(ID);
                    }
                }
            }

            // return the IDs list
            return IDs;
        }

        static bool Askcredentials()
        {
            Console.WriteLine("As mentioned before, to download the beatmapsets you have to enter your osu! username & password.");
            Console.WriteLine("If you don't want to enter them, just press [enter] to leave it blank and cancel the request.\n");

            Console.WriteLine("If you enter wrong username and/or password, the downloaded files will contain the HTML code of");
            Console.WriteLine("osu! website page, asking you to login.\n\n");

            // write the user input into 'username'
            Console.Write("Enter your osu! username: ");
            username = Console.ReadLine();

            // write the user input into 'password'
            Console.Write("Enter your osu! password: ");
            password = Console.ReadLine();


            // ask if the user wants to clear the application window to hide his credentials
            Console.WriteLine("Do you want to clear the application windown? [y/n]");

            // read the answer
            char answer = Console.ReadLine()[0];
            answer = Char.ToLower(answer);
            Console.WriteLine();

            // if yes,
            if (answer == 'y') Console.Clear(); // clear the application window
            // else keep running the code...

            // check if the user gave any information or left it blank
            if (username != String.Empty && password != String.Empty)
                return true; // return true if gave any
            else return false; // return false if left blank
        }

        static void Download(string args)
        {
            // reset the counters
            DL_Count = 0; DL_Complete = 0;

            // if the parameter wasn't given
            if (args == String.Empty)
            {
                Console.WriteLine("Opening the beatmap sets listing page...");
                // open the beatmap sets listing page
                System.Diagnostics.Process.Start("https://osu.ppy.sh/beatmapsets/");
                return; // return to the main function
            }

            // try to parse the ID from the first segment of the name
            // if it can parse an integer value,  the result will be true as a sign of success
            bool result = Int32.TryParse(args, out int ID);

            // if it's a valid beatmapsetID
            if (result && ID > 0)
            {
                // ask the user to enter his username & password
                bool flag = Askcredentials();

                // if the user entered any..
                if (flag)
                {
                    // warn the user NOT to clsoe the application while downloading
                    Console.WriteLine("Please DO NOT close the application until a notification will come up!!!");


                    // increase the download_count counter
                    DL_Count++;
                    // get the download URL
                    string url = GetDownloadURL(ID);
                    // download the beatmapset from the file...
                    DownloadFromURL(url, ID);

                    // inform the user that the download is finished
                    Console.WriteLine("Done!\n");
                }
                // if the user left the username and/or password blank,
                else return; // return to the main function
            }
            else
            {
                Console.WriteLine("The beatmapsetID <{0}> is invalid, opening the beatmap sets listing page instead...", args);
                // open the beatmap sets listing page
                System.Diagnostics.Process.Start("https://osu.ppy.sh/beatmapsets/");
            }

            // clear the credentials..
            username = String.Empty; password = String.Empty;
        }

        static string GetDownloadURL(int beatmapsetID)
        {
            // create a download URL by combining the beatmapsetID into the default URL
            string url = "https://osu.ppy.sh/beatmapsets/" + beatmapsetID + "/download";

            return url; // return the download URL
        }

        static async Task DownloadFromURL(string url, int id)
        {
            // create a reference for the download path
            string DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                + "\\OSU_DOWNLOADER_BEATMAPS\\" + id + ".osz";

            // try this segment of code and catch any exceptions
            try
            {
                // if the path doesn't exist, create it to avoid any errors..
                Directory.CreateDirectory(Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                    + "\\OSU_DOWNLOADER_BEATMAPS\\"));

                // if the file already exists,
                if (File.Exists(DownloadPath))
                {
                    // delete it to prevent any errors..
                    File.Delete(DownloadPath);
                }

                // use CookieAwareWebClient class to access the internet
                using (CookieAwareWebClient client = new CookieAwareWebClient())
                {
                    // assign DownloadComplete event
                    client.DownloadFileCompleted += WebClient_DownloadComplete;

                    // values for HTTP Request
                    var values = new NameValueCollection
                    {
                        { "login", "Login" },
                        { "password", password },
                        { "redirect", "index.php" },
                        { "sid", "" },
                        { "username", username }
                    };

                    // send the HTTP Request
                    client.UploadValues("https://osu.ppy.sh/forum/ucp.php?mode=login", values);

                    // if the previous call succeeded we now have a valid authentication cookie
                    // so we can download the protected beatmapset file

                    await client.DownloadFileTaskAsync(url, DownloadPath);
                }
            }
            catch (Exception ex) // if an exception (error) was catched,
            {
                // increase the downloads_failed counter
                DL_Failed++;

                // inform the user from what link it failed to download
                Console.WriteLine("Failed to download <{0}>.", url);

                if (ex.HResult == -2146233079)
                {
                    Console.WriteLine("Reason: Beatmap removed from listing.");

                    // delete the file.. it's empty after all...
                    File.Delete(DownloadPath);
                }
                else
                {
                    // if the failure reason is not the abundance of the file..
                    // print the error
                    Console.WriteLine(ex.Message);
                    // and keep the file.. it might contain some information as well.
                }
            }
        }

        static void WebClient_DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            // on file download_complete increase the downloads_complete counter
            DL_Complete++;

            // show the progress in the title bar
            Console.Title = $"Osu! Beatmap Downloader [Downloading.. {DL_Complete}/{DL_Count}]";

            // when done,
            if (DL_Complete + DL_Failed >= DL_Count)
            {
                // show the progress in the title bar
                Console.Title = $"Osu! Beatmap Downloader [Done downloading!]";
                // and pop a message box
                System.Windows.Forms.MessageBox.Show
                    ("Finished downloading {DL_Complete}/{DL_Count} beatmap sets ({DL_Failed} Failed)."
                    + "\nYou can safely close the downloader now :)", "Finished Downloading!");
            }
        }

        static async Task DownloadFromList(string args)
        {
            // reset the counters
            DL_Count = 0; DL_Complete = 0; DL_Failed = 0;

            // ask the user to enter his username & password
            bool flag = Askcredentials();
            if (!flag) return; // return to the main function in case the user haven't gave his credentials

            // warn the user NOT to clsoe the application while downloading
            Console.WriteLine("Please DO NOT close the application until a notification will come up!!!");


            // in case the user wants to use the default path, it will be the desktop
            if (args == String.Empty) args = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                    + "\\beatmaps_list.txt";

            // create a list to hold download URLs
            List<string> URLs = new List<string>();
            URLs.Clear();

            // try this segment of code and catch any exceptions
            try
            {
                // read the download URLs from the file
                URLs = File.ReadAllLines(args).ToList();

                // check if the URLs are valid
                foreach (string url in URLs)
                {
                    // if the URL is empty or doesn't contain the default osu! webpage address,
                    if (url == String.Empty || !url.Contains("https://osu.ppy.sh/beatmapsets/"))
                    {
                        // remove it from the list
                        URLs.Remove(url);
                    }
                }
            }
            catch (Exception ex) // if an exception (error) was catched,
            {
                // print the exception
                Console.WriteLine(ex);
                return;
            }


            DL_Count = URLs.Count; // set the downloads_count to the URLs count

            // if downloads_count is high, remind the user that it might take a lot of space and time to download them
            if (DL_Count > 20)
            {
                Console.WriteLine("\nPlease note that downloading high amounts of beatmaps will take some time to finish\n");
                Console.WriteLine("and might take some space.\n");
            }

            // ask if the user really wants to download all of those maps..
            Console.WriteLine("{0} URLs were found, start downloading the beatmapsets to <{1}>? [y/n]", DL_Count,
                Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                    + "\\OSU_DOWNLOADER_BEATMAPS\\"));

            // read the answer
            char answer = Console.ReadLine()[0];
            answer = Char.ToLower(answer);
            Console.WriteLine();

            // if the answer IS NOT yes,
            if (answer != 'y') return; // return to the main function
                                       // else keep running the code...


            // wait until all downloads are finished
            await Task.WhenAll(
                URLs.Select(url => DownloadFromURL(url, GetUrlID(url)))
                );


            // after trying to download everysingle URL,
            // check how many URLs it failed to download
            // if any, inform the user of the count
            if (DL_Failed > 0) Console.WriteLine("\nDone! Failed to download {0} beatmap sets. :(", DL_Failed);
            else Console.WriteLine("\nDone!"); // else everything is ok and just inform the user that we are done :)

            // clear the credentials..
            username = String.Empty; password = String.Empty;
        }

        static int GetUrlID(string url)
        {
            // remove the https://osu.ppy.sh/beatmapsets/ part
            string id = url.Remove(0, 31);
            // remove the last /download part
            id = id.Split('/')[0];

            return Int32.Parse(id);
        }
    }
}
