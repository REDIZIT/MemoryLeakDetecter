using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ConsoleMemoryLeak
{
    class Program
    {
        private static Watcher watcher = new Watcher();

        private const string AUTORUN_SHORTCUT_FILENAME = "MemoryLeak_watch.lnk";

        static void Main(string[] args)
        {
            ConsoleWindowVisibility.Hide();
            Console.WriteLine("Memory Leak Watcher");;

            PrintBackgroundState();

            if (args.Length == 1 && args[0] == "watch")
            {
                watcher.StartWatching(true);
            }
            else
            {
                ConsoleWindowVisibility.Show();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[ Command line mode ]");
                Console.ResetColor();

                Console.WriteLine("This instance is working in command line mode.");
                Console.WriteLine("You can type commands and change settings for watching instance.");
                Console.WriteLine("Type command (help):");
                while (true)
                {
                    CommandHandling();
                }
            }

            Console.Read();
        }

        private static void CommandHandling()
        {
            Console.Write("> ");
            string cmd = Console.ReadLine();
            Console.WriteLine();
            

            if (cmd == "help")
            {
                Console.WriteLine("autorun (on/off/get) - enable or disable autorun");
                Console.WriteLine("watch (start/stop/get) - switch this instance to watch mode, kill watching instance or get state");
            }
            else if (cmd == "autorun on")
            {
                string sourceFilePath = AUTORUN_SHORTCUT_FILENAME;
                string targetFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), AUTORUN_SHORTCUT_FILENAME);

                File.Copy(sourceFilePath, targetFilePath, true);
                Console.WriteLine("Autorun enabled");
            }
            else if (cmd == "autorun off")
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/" + AUTORUN_SHORTCUT_FILENAME);
                Console.WriteLine("Autorun disabled");
            }
            else if (cmd == "autorun get")
            {
                Console.WriteLine("Autorun " + (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/" + AUTORUN_SHORTCUT_FILENAME) ? "enabled" : "disabled"));
            }
            else if (cmd == "watch start")
            {
                watcher.StartWatching(false);
            }
            else if (cmd == "watch stop")
            {
                GetWatchingProcess().Kill();
            }
            else if (cmd == "watch get")
            {
                Console.WriteLine("Background watching is " + (GetWatchingProcess() == null ? "disabled" : "enabled"));
            }
        }
        private static void PrintBackgroundState()
        {
            if (GetWatchingProcess() == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[State] : Watching is disabled");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[State] : Watching is enabled");
                Console.ResetColor();
            }
            Console.WriteLine();
        }



        public static Process GetWatchingProcess()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Process process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName.ToLower().Contains("memoryleak") && p.Id != currentProcess.Id && p.MainModule.FileName == currentProcess.MainModule.FileName);

            if (process != null/* && process.MainWindowTitle == "Watching"*/)
            {
                return process;
            }
            return null;
        }
    }
}
