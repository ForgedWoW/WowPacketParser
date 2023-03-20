using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Org.BouncyCastle.Bcpg;
using WowPacketParser.Enums;
using WowPacketParser.Loading;
using WowPacketParser.Misc;
using WowPacketParser.Parsing.Parsers;
using WowPacketParser.SQL;

namespace WowPacketParser
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            SetUpWindowTitle();
            SetUpConsole();

            var files = args.ToList();
            if (files.Count == 0)
            {
                PrintUsage();
                return;
            }

            // config options are handled in Misc.Settings
            Utilities.RemoveConfigOptions(ref files);

            if (!Utilities.GetFiles(ref files))
            {
                EndPrompt();
                return;
            }

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            if (Settings.UseDBC)
            {
                var startTime = DateTime.Now;

                DBC.DBC.Load();

                var span = DateTime.Now.Subtract(startTime);
                Trace.WriteLine($"DBC loaded in { span.ToFormattedString() }.");
            }

            // Disable DB when we don't need its data (dumping to a binary file)
            if (!Settings.DumpFormatWithSQL())
            {
                SQLConnector.Enabled = false;
                SSHTunnel.Enabled = false;
            }
            else
                Filters.Initialize();

            SQLConnector.ReadDB();

            var processStartTime = DateTime.Now;
            var count = 0;

            Dictionary<string, Dictionary<Direction, HashSet<string>>> opcodeMappings = new();

            foreach (var file in files)
            {
                SessionHandler.ZStreams.Clear();
                if (Settings.ClientBuild != Enums.ClientVersionBuild.Zero)
                    ClientVersion.SetVersion(Settings.ClientBuild);

                ClientLocale.SetLocale(Settings.ClientLocale.ToString());

                try
                {
                    var sf = new SniffFile(file, Settings.DumpFormat, Tuple.Create(++count, files.Count));
                    sf.ProcessFile();

                    foreach (var opcodeMapping in sf.OpcodeMappings)
                    {
                        foreach (var map in opcodeMapping.Value)
                        {
                            if (!opcodeMappings.TryGetValue(opcodeMapping.Key, out var direc))
                            {
                                direc = new Dictionary<Direction, HashSet<string>>();
                                opcodeMappings.Add(opcodeMapping.Key, direc);
                            }

                            if (!direc.TryGetValue(map.Key, out var dir))
                            {
                                dir = map.Value;
                                direc.Add(map.Key, dir);
                            }
                        }
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Can't process {file}. Skipping. Message: {ex.Message}");
                }
            }

            if (!string.IsNullOrWhiteSpace(Settings.SQLFileName) && Settings.DumpFormatWithSQL())
                Builder.DumpSQL("Dumping global sql", Settings.SQLFileName, SniffFile.GetHeader("multi"));

            var processTime = DateTime.Now.Subtract(processStartTime);
            Trace.WriteLine($"Processing {files.Count} sniffs took { processTime.ToFormattedString() }.");

            using (var file = new StreamWriter($"opcode_mappings_{DateTime.Now.ToString("mmddyyyyHHmmss")}.txt"))
            {
                foreach (var opcodeMapping in opcodeMappings)
                {
                    file.WriteLine(opcodeMapping.Key);

                    foreach (var map in opcodeMapping.Value)
                    {
                        file.WriteLine($"   {map.Key}:");

                        foreach (var code in map.Value.OrderBy(n => n))
                        {
                            file.WriteLine($"       {code}");
                        }
                    }
                }
            }

            SQLConnector.Disconnect();
            SSHTunnel.Disconnect();

            if (Settings.LogErrors)
                Logger.WriteErrors();

            Trace.Listeners.Remove("ConsoleMirror");

            EndPrompt();
        }

        private static void EndPrompt(bool forceKey = false)
        {
            if (Settings.ShowEndPrompt || forceKey)
            {
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                Console.WriteLine();
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Error: No files selected to be parsed.");
            Console.WriteLine("Usage: Drag a file, or group of files on the executable to parse it.");
            Console.WriteLine("Command line usage: WowPacketParser.exe [--ConfigFile path --Option1 value1 ...] filetoparse1 ...");
            Console.WriteLine("--ConfigFile path - file to read config from, default: WowPacketParser.exe.config.");
            Console.WriteLine("--Option1 value1 - override Option1 setting from config file with value1.");
            Console.WriteLine("Configuration: Modify WowPacketParser.exe.config file.");
            EndPrompt(true);
        }

        private static void SetUpWindowTitle()
        {
            Console.Title = "WowPacketParser";
        }

        public static void SetUpConsole()
        {

            Trace.Listeners.Clear();

            using (ConsoleTraceListener consoleListener = new ConsoleTraceListener(true))
                Trace.Listeners.Add(consoleListener);

            if (Settings.ParsingLog)
            {
                using (TextWriterTraceListener fileListener = new TextWriterTraceListener($"{Utilities.FormattedDateTimeForFiles()}_log.txt"))
                {
                    fileListener.Name = "ConsoleMirror";
                    Trace.Listeners.Add(fileListener);
                }
            }

            Trace.AutoFlush = true;
        }
    }
}
