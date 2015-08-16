using Agilix.Shared;
using Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace TurboTank
{

    /// <summary>
    /// Main class for TurboTank.
    /// </summary>
    public class Program
    {
        public const string DefaultConfigFilename = "config.json";

        private const string Syntax =
                "TurboTank <hostname> <port> <gamename> <username>\n";



        static public int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Directory.SetCurrentDirectory(IOUtil.GetProcessDirectory());

            JsonArgs commandLine = new JsonArgs(args);
            if (!commandLine.HasParam("?"))
            {
                string server = args[0];
                int port = int.Parse(args[1]);
                string gameId = args[2];
                string userId = args[3];

                while (true)
                {
                    try
                    {
                        Console.WriteLine("Starting game ...");
                        TankClient client = new HttpTankClient(server, port, gameId, userId);
                        Game game = new Game(client);
                        game.Run(new SignalWeights());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR: " + ex);
                    }

                    ConsoleKey key = ConsoleKey.N;
                    while (key != ConsoleKey.Y)
                    {
                        Console.WriteLine("\n\nWould you like to play again (y, or n)?\n");
                        key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.N)
                        {
                            Console.WriteLine("Thanks for playing!");
                            return 0;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine(Syntax);
            }

            return 0;
        }


        static public void Log(string format, params object[] args)
        {
#if DEBUG
            Console.WriteLine(format, args);
            Console.WriteLine();
#endif
        }


        static private void ExecuteService(JsonArgs commandLine)
        {
            string configFilename = IOUtil.ResolveRelativePath(commandLine.GetParam("config", DefaultConfigFilename));
            DynObject config = DynObject.Parse(File.ReadAllText(configFilename));

            WebServer server = new WebServer(config);
            server.Run();
        }



        /// <summary>
        /// Handle any exception that the code didn't handle so that we never terminate unexpectedly.
        /// </summary>
        static private void UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
        }

    }


}

