// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace TrayApp
{
    public class SettingsManager
    {
        private static bool NeedUpgrade = true;

        // TODO: Add a "Clear Settings" button and set NeedUpgrade to false before calling this again
        public static void LoadSettings()
        {
            if (NeedUpgrade)
            {
                TrayApp.Properties.Settings.Default.Upgrade();
                TrayApp.Properties.Settings.Default.Save();
            }

            List<string> known_processes_from_settings = TrayApp.Properties.Settings.Default.KnownGPUProcesses;
            List<string> processes_list = new List<string>();
            //NotificationManager.PushNotificationToOS("Loading processes list...");

            foreach (string url_iter in known_processes_from_settings)
            {
                bool success = Helper.ValidateInput(url_iter);

                if (success)
                {
                    success = !processes_list.Contains(url_iter);
                }

                if (success)
                {
                    processes_list.Add(url_iter);
                }
            }

            // Must be saved after the foreach loop to prevent overwriting the working data
            SettingsManager.WriteNewProcessesList(processes_list);
            //List<string> processes_list_real = Helper.Convert(processes_list);
            List<string> processes_list_real = processes_list;
            string first = processes_list_real.First();
            string last = processes_list_real.Last();
            string processes_list_string = "";
            foreach (string process in processes_list_real)
            {
                if (process == first)
                {
                    processes_list_string = process;
                }
                else if (process != last)
                {
                    processes_list_string += ", " + process;
                }
                else
                {
                    processes_list_string += " and " + process;
                }
            }
            NotificationManager.PushNotificationToOS("Processes that will be killed: " + processes_list_string);
        }

        public static void WriteNewProcessesList(List<string> coll)
        {
            TrayApp.Properties.Settings.Default.KnownGPUProcesses.Clear();
            TrayApp.Properties.Settings.Default.KnownGPUProcesses = coll;
            TrayApp.Properties.Settings.Default.Save();
            TrayApp.Properties.Settings.Default.Reload();
        }
    }
}
