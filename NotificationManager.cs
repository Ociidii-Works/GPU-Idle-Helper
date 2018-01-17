// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace TrayApp
{
    public class NotificationManager
    {
        public static void PushNotificationToOS(string content, string title = "")
        {
            if (!TrayApp.Properties.Settings.Default.ShowNotifications)
            {
                return;
            }
            if (title == "")
            {
                title = Program.ProductName;
            }
            Program.sTrayIcon.BalloonTipTitle = title;
            Program.sTrayIcon.BalloonTipText = content;
            Program.sTrayIcon.ShowBalloonTip(1);
        }
    }
}
