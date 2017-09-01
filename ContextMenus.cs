﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using MaxwellGPUIdle.Properties;

namespace MaxwellGPUIdle
{
    /// <summary>
    /// </summary>
    internal class ContextMenus
    {
        /// <summary>
        /// Is the About box displayed?
        /// </summary>
        private bool isAboutLoaded = false;

        //private string titles = "";

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>ContextMenuStrip</returns>
        public ContextMenuStrip CreateFeedsMenu(bool allow_notifications = true, bool has_checkbox = true)
        {
            // Add the default menu options.
            // TODO: Cache the feeds results to avoid heavy rebuilding every time a checkbox value
            //       changes :(
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ShowImageMargin = false;
            ToolStripMenuItem item;

            // Add one entry to this menu to kill everything
            item = new ToolStripMenuItem
            {
                Text = "Force Idle Now!",
                //Image = Resources.Exit
            };
            item.Click += delegate (object sender, EventArgs e) { Kill_Click(sender, e); };
            menu.Items.Add(item); // Add menu entry with the feed name
            menu.Items.Add(new ToolStripSeparator()); // Separator.
            string temporaryRssFile = System.IO.Path.GetTempFileName();

            foreach (string process_name in Settings.Default.KnownGPUProcesses)
            {
                item = new ToolStripMenuItem
                {
                    Text = process_name,
                    //Image = Resources.Rss
                };
                item.Click += delegate (object sender, EventArgs e) { FeedEntry_Click(sender, e, process_name); };
                menu.Items.Add(item); // Add menu entry with the feed name
            }

            return menu;
        }

        public ContextMenuStrip CreateLoadingMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ShowImageMargin = false;
            ToolStripMenuItem item;
            // Add a feed.
            item = new ToolStripMenuItem()
            {
                Text = "Loading..."
            };
            item.Enabled = false;
            menu.Items.Add(item);
            return menu;
        }

        public ContextMenuStrip CreateOptionsMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ShowImageMargin = true;
            ToolStripMenuItem item;

            // Kill background processes.
            item = new ToolStripMenuItem()
            {
                Text = "Kill Background Processes",
                Image = Resources.Exit
            };
            item.Click += new System.EventHandler(Kill_Click);
            menu.Items.Add(item);

            // Add a feed.
            item = new ToolStripMenuItem()
            {
                Text = "Add Executable",
                Image = Resources.Rss
            };
            item.Click += new EventHandler(AddFeed_Click);
            menu.Items.Add(item);

            // About box
            item = new ToolStripMenuItem()
            {
                Text = "About",
                Image = Resources.About
            };
            item.Click += new EventHandler(About_Click);
            menu.Items.Add(item);

            // Notifications On/Off
            item = new ToolStripMenuItem()
            {
                Text = "Notifications",
                Checked = Settings.Default.ShowNotifications,
            };
            if (item.Checked)
            {
                item.Image = Resources.checkmark;
            }
            item.Click += new EventHandler(Notification_Setting_Click);
            menu.Items.Add(item);

            // Add to Startup
            item = new ToolStripMenuItem()
            {
                Text = "Run at Login",
                Checked = Settings.Default.AutomaticStartup,
            };
            if (item.Checked)
            {
                item.Image = Resources.checkmark;
            }
            item.Click += new EventHandler(Startup_Click);
            menu.Items.Add(item);

            // Add to Startup
            item = new ToolStripMenuItem()
            {
                Text = "Kill on Idle",
                Checked = Settings.Default.KillOnIdle,
            };
            if (item.Checked)
            {
                item.Image = Resources.checkmark;
            }
            item.Click += new EventHandler(KillOnIdle_Click);
            menu.Items.Add(item);

            // Separator.
            menu.Items.Add(new ToolStripSeparator());

            // Exit.
            item = new ToolStripMenuItem()
            {
                Text = "Exit",
                Image = Resources.Exit
            };
            item.Click += new System.EventHandler(Exit_Click);
            menu.Items.Add(item);

            System.GC.Collect(3, System.GCCollectionMode.Forced);
            System.GC.WaitForFullGCComplete();

            return menu;
        }

        /// <summary>
        /// Handles the Click event of the About control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void About_Click(object sender, EventArgs e)
        {
            if (!isAboutLoaded)
            {
                isAboutLoaded = true;
                new AboutBox().ShowDialog();
                isAboutLoaded = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the Add Feed control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void AddFeed_Click(object sender, EventArgs e)
        {
            new AddFeed().ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the Exit control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void Exit_Click(object sender, EventArgs e)
        {
            // Quit without further ado.
            MaxwellGPUIdle.ProcessIcon.ni.Visible = false;
            Application.Exit();
        }

        /// <summary>
        /// Handles the Click event of the Explorer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void Explorer_Click(object sender, EventArgs e)
        {
            Process.Start("explorer", null);
        }

        private void FeedEntry_Click(object sender, EventArgs e, string u)
        {
            try
            {
                ProcessDestroyer.KillProcessByName(u);
            }
            catch (Exception ex)
            {
                Program.ExceptionHandler(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the Kill Background Processes control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void Kill_Click(object sender, EventArgs e)
        {
            MaxwellGPUIdle.ProcessDestroyer.KillCompilerProcesses();
        }

        /// <summary>
        /// Handles the Click event of the KillOnIdle control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void KillOnIdle_Click(object sender, EventArgs e)
        {
            // TODO: Shouldn't we use the event data?
            Settings.Default.KillOnIdle = !Settings.Default.KillOnIdle;
            Settings.Default.Save();
            MaxwellGPUIdle.ProcessIcon.ni.ContextMenuStrip = new ContextMenus().CreateFeedsMenu(false);
        }

        /// <summary>
        /// Handles the Click event of the Notification Setting control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void Notification_Setting_Click(object sender, EventArgs e)
        {
            // Flipping here can cause bugs, be more explicit so that the value is always right.
            if (Settings.Default.ShowNotifications)
            {
                Settings.Default.ShowNotifications = false;
            }
            else
            {
                Settings.Default.ShowNotifications = true;
            }
            Settings.Default.Save();
            MaxwellGPUIdle.ProcessIcon.ni.ContextMenuStrip = new ContextMenus().CreateFeedsMenu(false);
        }

        /// <summary>
        /// Handles the Click event of the Startup control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void Startup_Click(object sender, EventArgs e)
        {
            bool startUp = !Settings.Default.AutomaticStartup;
            Integration.AddToStartup(startUp);
            Settings.Default.AutomaticStartup = startUp;
            Settings.Default.Save();
            MaxwellGPUIdle.ProcessIcon.ni.ContextMenuStrip = new ContextMenus().CreateFeedsMenu(false);
        }

        private class MyXmlReader : XmlTextReader
        {
            private const string CustomUtcDateTimeFormat = "ddd MMM dd HH:mm:ss Z yyyy";
            private bool readingDate = false;
            // Wed Oct 07 08:00:07 GMT 2009

            public MyXmlReader(Stream s) : base(s)
            {
            }

            public MyXmlReader(string inputUri) : base(inputUri)
            {
            }

            public override void ReadEndElement()
            {
                if (readingDate)
                {
                    readingDate = false;
                }
                base.ReadEndElement();
            }

            public override void ReadStartElement()
            {
                if (string.Equals(base.NamespaceURI, string.Empty, StringComparison.InvariantCultureIgnoreCase) &&
                    (string.Equals(base.LocalName, "lastBuildDate", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(base.LocalName, "pubDate", StringComparison.InvariantCultureIgnoreCase)))
                {
                    readingDate = true;
                }
                base.ReadStartElement();
            }

            public override string ReadString()
            {
                if (readingDate)
                {
                    string dateString = base.ReadString();
                    DateTime dt;
                    if (!DateTime.TryParse(dateString, out dt))
                        dt = DateTime.ParseExact(dateString, CustomUtcDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                    return dt.ToUniversalTime().ToString("R", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    return base.ReadString();
                }
            }
        }
    }
}
