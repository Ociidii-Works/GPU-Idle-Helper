// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace TrayApp
{
    public class ProcessDestroyer
    {
        public static void KillProcessByName(List<string> processToKill)
        {
            if (processToKill == null)
                return;
            List<string> wanted_dead = TrayApp.Properties.Settings.Default.KnownGPUProcesses;
            Console.WriteLine("Will attempt to kill:");
            foreach (var w in wanted_dead)
            {
                Console.WriteLine(w);
            }
            List<Process> listResult = new List<Process>();
            List<Process> processes_list = Process.GetProcesses().ToList();
            for (int i = 0; i < processes_list.Count; i++)
            {// Iterate through processes running on machine
                var rProcess = processes_list[i];
                var ID = rProcess.Id;
                var rName = rProcess.ProcessName;
                var rWTitle = rProcess.MainWindowTitle;
                for (int n = 0; n < wanted_dead.Count; n++)
                {// Compare against processes we want to kill
                    var wtitle = wanted_dead[n];
                    if (wtitle == rWTitle)
                    {
                        //Console.WriteLine("Yes! (title title match)");
                    }
                    else if (wtitle == rName)
                    {
                        //Console.WriteLine("Yes! (process name match)");
                    }
                    else
                    {
                        continue;
                    }
                    Console.WriteLine("Killing " + rName);
                    try
                    {
                        rProcess.Kill();
                        // Note: This will throw an exception if the process has already died.
                        rProcess.WaitForExit();
                        rProcess.Dispose();
                    }
                    catch
                    {
                        // Don't crash; do nothing.
                    }
                }
            }
            //Console.WriteLine("Done.");
        }
    }
}

public static class ProcessEx
{
    // This is used to find the command line (aka invoking params and all) a process was launched with. Currently unused, but will be useful later.
    public static string GetCommandLine(this Process process)
    {
        System.Text.StringBuilder commandLine = new System.Text.StringBuilder();
        try
        {
            commandLine = new System.Text.StringBuilder(process.MainModule.FileName);

            commandLine.Append(" ");
            using (var searcher = new System.Management.ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (var @object in searcher.Get())
                {
                    commandLine.Append(@object["CommandLine"]);
                    commandLine.Append(" ");
                }
            }
        }
        catch
        {
        }
        return commandLine.ToString();
    }
}
