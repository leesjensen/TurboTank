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
                "TurboTank [-config=<config file>]\n" +
                "    Starts TurboTank.\n" +
                "    config: The configuration file containing the class to execute. If no config is specified then + " + DefaultConfigFilename + " is used.\n";



        static public int Main(string[] args)
        {
            Logger.Reset(DynObject.Parse("{'level':'info','targets':{'console':{}}}"), false);

            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Directory.SetCurrentDirectory(IOUtil.GetProcessDirectory());

            JsonArgs commandLine = new JsonArgs(args);
            if (!commandLine.HasParam("?"))
            {
                try
                {
                    ExecuteService(commandLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex);
                    Logger.Info("ERROR: {0}", ex.ToString());
                }
            }
            else
            {
                Console.WriteLine(Syntax);
            }

            Logger.Reset(new DynObject(), true);
            return 0;
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

