using System;
using DittoSDK;
using System.Collections.Generic;

// now uses appID instead of appName in DittoIdentity.Development
// how does TryStartSync work.  Does it error off

namespace Ditto1
{
    class Program
    {
        static Ditto ditto;
        static bool isAskedToExit = false;
        static List<Task> tasks = new List<Task>();
        static DittoLiveQuery liveQuery;
        public static void Main(string[] args)
        {

            String token = "o2d1c2VyX2lkZmFsYXNrYWZleHBpcnl0MjAyMS0xMi0xNVQwMDowMDowMFppc2lnbmF0dXJleFhGNVFHempCQi9DVDJzMXo2T09FcFdsZmh4SUJkMUhoZWw1L1U1OG5zUkVpb21QR2NiRWZNaXAzOERzWGUvY0FTS0pkNjQzaVRoQXF1eXBpVUNMeE1rdz09";

            DittoTransportConfig transportConfig = new DittoTransportConfig();
            transportConfig.PeerToPeer.Lan.Enabled = true;
            transportConfig.PeerToPeer.Lan.MulticastEnabled = true;

            DittoLogger.SetLoggingEnabled(true);
            DittoLogger.SetMinimumLogLevel(DittoLogLevel.Debug);


            ditto = new Ditto(identity: DittoIdentity.Development(appID: "live.ditto.tasks"));
            ditto.SetTransportConfig(transportConfig);

            try
            {
                ditto.SetLicenseToken(token);
                ditto.TryStartSync();

            }
            catch (DittoException ex)
            {
                Console.WriteLine("There was an error starting Ditto.");
                Console.WriteLine("Here's the following error");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Ditto cannot start sync but don't worry.");
                Console.WriteLine("Ditto will still work as a local database.");
            }

            Console.WriteLine("Welcome to Ditto's Task App");

            liveQuery = ditto.Store["tasks"].FindAll().Observe((docs, _event) => {
                tasks = docs.ConvertAll(document => new Task(document));
            });

            ListCommands();

            while (!isAskedToExit)
            {

                // 2.
                Console.Write("\nYour command: ");
                string command = Console.ReadLine();

                switch (command)
                {

                    // 3. 4.
                    case string s when command.StartsWith("--insert"):
                        string taskBody = s.Replace("--insert ", "");
                        ditto.Store["tasks"].Insert(new Task(taskBody, false).ToDictionary());
                        break;
                    // 5.
                    case string s when command.StartsWith("--toggle"):
                        string _idToToggle = s.Replace("--toggle ", "");
                        ditto.Store["tasks"]
                            .FindById(new DittoDocumentID(_idToToggle))
                            .Update((mutableDoc) =>
                            {
                                if (mutableDoc == null) return;
                                mutableDoc["isCompleted"].Set(!mutableDoc["isCompleted"].BooleanValue);
                            });
                        break;
                    // 6.
                    case string s when command.StartsWith("--delete"):
                        string _idToDelete = s.Replace("--delete ", "");
                        ditto.Store["tasks"]
                            .FindById(new DittoDocumentID(_idToDelete))
                            .Remove();
                        break;
                    case { } when command.StartsWith("--list"):
                        tasks.ForEach(task =>
                        {
                            Console.WriteLine(task.ToString());
                        });
                        break;
                    // 1.
                    case { } when command.StartsWith("--exit"):
                        Console.WriteLine("Good bye!");
                        isAskedToExit = true;
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        ListCommands();
                        break;
                }
            }

        }

        public static void ListCommands()
        {
            Console.WriteLine("************* Commands *************");
            Console.WriteLine("--insert my new task");
            Console.WriteLine("   Inserts a task");
            Console.WriteLine("   Example: \"--insert Get Milk\"");
            Console.WriteLine("--toggle myTaskTd");
            Console.WriteLine("   Toggles the isComplete property to the opposite value");
            Console.WriteLine("   Example: \"--toggle 1234abc\"");
            Console.WriteLine("--delete myTaskTd");
            Console.WriteLine("   Deletes a task");
            Console.WriteLine("   Example: \"--delete 1234abc\"");
            Console.WriteLine("--list");
            Console.WriteLine("   List the current tasks");
            Console.WriteLine("--exit");
            Console.WriteLine("   Exits the program");
            Console.WriteLine("************* Commands *************");
        }
    }
}
