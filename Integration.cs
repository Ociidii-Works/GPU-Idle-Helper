// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace TrayApp
{
    internal class Integration
    {
        private static readonly ProcessStartInfo PowerCfgStartInfo = new ProcessStartInfo
        {
            FileName = "powercfg.exe",
            Arguments = "-setactive 381b4222-f694-41f0-9685-ff5bb260df2e",
            CreateNoWindow = true
        };

        public static void AddToStartup()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            // Empty a few directories. Yes. If your shit is missing, this is the line that does it.
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
            if (t != null)
            {
                dynamic shell = Activator.CreateInstance(t);
                try
                {
                    if (shell != null)
                    {
                        string LinkName = startupFolder + "\\" + Program.ProductName + ".lnk";
                        File.Delete(LinkName);
                        if (TrayApp.Properties.Settings.Default.AutomaticStartup)
                        {
                            dynamic startupEntry = shell.CreateShortcut(LinkName);
                            try
                            {
                                var currentPathToExe = System.Reflection.Assembly.GetExecutingAssembly().Location;
                                startupEntry.TargetPath = currentPathToExe;
                                startupEntry.IconLocation = currentPathToExe;
                                startupEntry.Save();
                            }
                            finally
                            {
                                Marshal.FinalReleaseComObject(startupEntry);
                            }
                        }
                    }
                }
                finally
                {
                    if (shell != null)
                    {
                        Marshal.FinalReleaseComObject(shell);
                    }
                }
            }
            //LogStats();
        }

        public static void SetPowerPlanToOnDemand()
        {
            Process powercfgProcess = new Process
            {
                StartInfo = PowerCfgStartInfo,
            };
            powercfgProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            powercfgProcess.Start();
        }
    }
}
