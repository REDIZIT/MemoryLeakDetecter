using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ConsoleMemoryLeak
{
    public class Watcher
    {
        private NotificationManager notificationManager = new NotificationManager();

        private static Dictionary<int, History> historyByProcess = new Dictionary<int, History>();
        private static HashSet<int> deniedProcesses = new HashSet<int>();


        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

       




        public void StartWatching(bool closeIfClone)
        {
            if (Program.GetWatchingProcess() != null)
            {
                Console.WriteLine("Can't switch to watch mode due to another program instance already is watching");
                if (closeIfClone) 
                    Process.GetCurrentProcess().Kill();
                return;
            }

            Console.WriteLine("Start watching");
            Console.Title = "Watching";
            ConsoleWindowVisibility.Hide();

            Process[] processes = Process.GetProcesses();
            GetPhysicallyInstalledSystemMemory(out long totalRAM);

            int iteration = 0;

            while (true)
            {
                iteration++;
                Thread.Sleep(500);

                // Everty 5 iterations update processes list
                if (iteration == 5)
                {
                    iteration = 0;
                    processes = Process.GetProcesses();
                }

                long availableRAM = totalRAM;

                foreach (Process process in processes)
                {
                    if (deniedProcesses.Contains(process.Id)) continue;
                    if (IsExited(process)) continue;

                    availableRAM -= process.WorkingSet64 / 1024;
                    if (process.WorkingSet64 < 100 * 1024 * 1024) continue;

                    Thread.Sleep(30);

                    process.Refresh();



                    if (historyByProcess.ContainsKey(process.Id) == false)
                    {
                        History newHistory = new History(10);
                        newHistory.lastRAM = process.WorkingSet64;

                        historyByProcess.Add(process.Id, newHistory);
                    }


                    History history = historyByProcess[process.Id];


                    int speedInMB = (int)(process.WorkingSet64 - history.lastRAM) / 1024 / 1024;
                    history.lastRAM = process.WorkingSet64;

                    history.Append(speedInMB);

                    float rangeMin = speedInMB - speedInMB * 0.5f;
                    float rangeMax = speedInMB + speedInMB * 0.5f;

                    if (speedInMB > 0)
                    {
                        if (history.values.All(v => rangeMin <= v && v <= rangeMax))
                        {

                            if (history.isNotificationSent == false)
                            {
                                history.isNotificationSent = true;

                                notificationManager.SendSusNotify(process, history.values.Average());

                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("[LEAK DETECTED] " + process.ProcessName);
                                Console.ResetColor();
                            }
                        }
                    }
                    else
                    {
                        history.isNotificationSent = false;
                    }
                }


                // If available ram less than 1024 MB
                if (availableRAM <= 1024 * 1024)
                {
                    Process processToKill = processes.OrderByDescending(p => p.WorkingSet64).First();

                    notificationManager.SendKillNotify(processToKill);

                    processToKill.Kill();
                }
            }
        }
        private static bool IsExited(Process process)
        {
            try
            {
                if (process.HasExited)
                {
                    historyByProcess.Remove(process.Id);
                    return true;
                }
            }
            catch
            {
                historyByProcess.Remove(process.Id);
                deniedProcesses.Add(process.Id);
                return true;
            }
            return false;
        }
    }
}
