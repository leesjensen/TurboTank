using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Shared
{
    public static class IOUtil
    {
        public static Encoding Utf8Encoding = new UTF8Encoding(false);


        public static MemoryStream ToGzipMemoryStream(string value)
        {
            MemoryStream output = new MemoryStream();
            using (GZipStream zipStream = new GZipStream(output, CompressionMode.Compress, true))
            using (StreamWriter writer = new StreamWriter(zipStream))
            {
                writer.Write(value);
            }
            return output;
        }


        public static string FromGzipMemoryStream(MemoryStream stream)
        {
            using (GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress))
            using (StreamReader reader = new StreamReader(zipStream))
            {
                return reader.ReadToEnd();
            }
        }


        public static string ResolveRelativePath(string filename)
        {
            if (!filename.Contains(":"))
            {
                string moduleDirectory = IOUtil.GetModuleDirectory("TurboTank.exe");
                filename = Path.Combine(moduleDirectory, filename);
            }

            return filename;
        }



        public static int RunProcess(string filename, string arguments, bool waitForExit = true)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = filename;
                if (arguments != null)
                {
                    process.StartInfo.Arguments = arguments;
                }

                process.Start();

                if (waitForExit)
                {
                    process.WaitForExit();
                }

                return process.ExitCode;
            }
        }


        public static void DeleteDirectory(string targetDirectory)
        {
            if (Directory.Exists(targetDirectory))
            {
                string[] files = Directory.GetFiles(targetDirectory);
                foreach (string sourceFilename in files)
                {
                    string targetFilename = Path.Combine(targetDirectory, Path.GetFileName(sourceFilename));
                    File.SetAttributes(sourceFilename, FileAttributes.Normal);
                    File.Delete(sourceFilename);
                }

                // Make sure we wait long enough for the OS to release its references to the files.
                int waitTime = 0;
                while (Directory.GetFiles(targetDirectory).Length > 0 && (waitTime < 250))
                {
                    Thread.Sleep(1);
                    waitTime++;
                }

                string[] subDirectories = Directory.GetDirectories(targetDirectory);
                foreach (string subDirectory in subDirectories)
                {
                    string targetSubdirectory = Path.Combine(targetDirectory, Path.GetFileName(subDirectory));
                    DeleteDirectory(targetSubdirectory);
                }

                Directory.Delete(targetDirectory);
            }
        }



        public static string GetProcessDirectory()
        {
            string currentProcessFilename = Process.GetCurrentProcess().MainModule.FileName;
            return Path.GetDirectoryName(currentProcessFilename);
        }


        public static string GetModuleDirectory(string moduleName)
        {
            string processDirectory = GetProcessDirectory();
            if (Directory.GetFiles(processDirectory, moduleName).FirstOrDefault() != null)
            {
                return processDirectory;
            }
            else
            {
                AppDomain MyDomain = AppDomain.CurrentDomain;
                Assembly[] assembliesLoaded = MyDomain.GetAssemblies();
                foreach (Assembly assembly in assembliesLoaded)
                {
                    if (string.Compare(moduleName, assembly.ManifestModule.Name, true) == 0)
                    {
                        return Path.GetDirectoryName(assembly.Location);
                    }
                }

                throw new Exception("Unable to get directory for module " + moduleName);
            }
        }

    }
}
