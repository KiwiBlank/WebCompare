using System;
using System.Linq;
using System.IO;
using IniParser;
using IniParser.Model;
using System.Net;

namespace WebCompare
{
    class Program
    {

        static void Main(string[] args)
        {
            if (!File.Exists("config.ini")) // Create config.ini if it doesn't exist and append the correct default info.
            {
                using (StreamWriter w = File.AppendText("config.ini"))
                {
                    w.WriteLine("[Configuration]\n");

                    w.WriteLine(";Set the web url to save for comparison");

                    w.WriteLine("WebPage = http://example.com/");
                }
            }
            System.IO.Directory.CreateDirectory("WebStorage"); // Create the WebStorage directory if it doesn't exist.

            Console.Title = "Kiwi's Web Comparator";
            Console.WriteLine("\n\nTo view a list of available commands, type: help");

            bool quitNow = false;
            while (!quitNow)
            {
                string command = Console.ReadLine(); // Handle the user's commands.
                switch (command)
                {
                    case "help":
                        Console.WriteLine("\nhelp : ");
                        Console.WriteLine("View available commands.");
                        Console.WriteLine("\ninfo :");
                        Console.WriteLine("View information about this application.");
                        Console.WriteLine("\nsetup :");
                        Console.WriteLine("View information about how to configure this application.");
                        Console.WriteLine("\nstart :");
                        Console.WriteLine("Start web comparison!\n");
                        break;

                    case "info":
                        Console.WriteLine("\nThis application was created by https://github.com/KiwiBlank and is meant to compare websites at different times.\n");
                        break;

                    case "setup":
                        Console.WriteLine("\nTo setup this application, navigate to the config.ini file in the directory of this application.");
                        Console.WriteLine("Then paste your URL inside the WebPage parameter.\n");
                        break;

                    case "start":
                        RunComparison();
                        Console.WriteLine("\nNavigate to the WebStorage folder in the application directory to view the compared files!");
                        break;

                    case "quit":
                        quitNow = true;
                        break;

                    default:
                        Console.WriteLine("\nUnknown Command " + command + "\n");
                        break;
                }
            }
        }
        public static void RunComparison() // Start the comparison part.
        {

            string pathofEXE = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string EXEDirectory = System.IO.Path.GetDirectoryName(pathofEXE);
            // Change directory of the exe to have double slashes instead of single.
            string EXEDirectoryReplaced = EXEDirectory.Replace(@"\", @"\\"); 

            var parser = new FileIniDataParser();
            // Find the config file.
            IniData data = parser.ReadFile(EXEDirectoryReplaced + "\\config.ini"); 

            // Grab the WebPage parameter under Configuration
            string webURL = data["Configuration"]["WebPage"];
            Console.WriteLine("\nComparing: " + webURL);

            WebClient client = new WebClient();
            string downloadString = "";
            try
            {
                // Grab the entire html file from the user's URL.
                downloadString = client.DownloadString(webURL); 
            }
            catch
            {
                // If no URL has been given, throw error.
                Console.WriteLine("\nERROR: The config.ini file is not properly configured!");
            }

            int num = 0;
            string fileNum = num + ".txt";
            // file 0.txt
            string fileEnum = Path.Combine(EXEDirectory, "WebStorage", fileNum); 

            // If file 0.txt exists, check if 1.txt exists . then continue until a file is not found.
            while (File.Exists(fileEnum)) 
            {
                num++;
                fileNum = num + ".txt";
                fileEnum = Path.Combine(EXEDirectory, "WebStorage", fileNum);

            }

            var directory = new DirectoryInfo(Path.Combine(EXEDirectory, "WebStorage"));
            // Write the new file with a greater file name than the last.
            File.WriteAllText(Path.Combine(EXEDirectory, "WebStorage", fileNum), downloadString); 

            // Check if there is anything to compare against, otherwise do nothing.
            if (Directory.GetFileSystemEntries(Path.Combine(EXEDirectory, "WebStorage")).Length != 1 && Directory.GetFileSystemEntries(Path.Combine(EXEDirectory, "WebStorage")).Length != 0)
            {
                var myFile = directory.GetFiles()
                         .OrderByDescending(f => f.LastWriteTime)
                         .First();
                String contents = File.ReadAllText(myFile.FullName);
                string finalOutputComparison = "";
                try 
                { 
                    // Try to compare differences between the two files.
                finalOutputComparison = downloadString.Replace(contents, "");

                }
                catch (ArgumentException e)
                {
                    // If the original file is empty, throw error.
                    Console.WriteLine("\nERROR: The last comparison file is empty!");

                }
                // Create a new file for all the differences between the two files.
                int comparisonNumber = num - 1;
                string comparisonString = comparisonNumber + " - Comparison - " + num + ".txt";
                File.WriteAllText(Path.Combine(EXEDirectory, "WebStorage", comparisonString), finalOutputComparison);
            }
            Console.WriteLine("\nComparison Completed!");
        }
    }
}
