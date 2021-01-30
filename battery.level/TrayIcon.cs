using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace battery.level
{
    class TrayIcon
    {
        private int oldPercentage;
        public NotifyIcon notifyIcon;
        public static string logpath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + @"\BatteryLevelLog.txt";
        RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        public TrayIcon()
        {
            // initialize menuItem
            MenuItem menuItemExit = new MenuItem() { Index = 0, Text = "Exit" };
            menuItemExit.Click += new System.EventHandler(menuItem_Click);

            MenuItem menuItemHistory = new MenuItem() { Index = 1, Text = "History" };
            menuItemHistory.Click += new EventHandler(menuItemHistory_Click);

            MenuItem menuItemStartup = new MenuItem() { Index = 2, Text = "Run at Startup" };
            menuItemStartup.Checked = (rk.GetValue(Application.ProductName) != null && rk.GetValue(Application.ProductName).ToString() == Application.ExecutablePath);
            menuItemStartup.Click += new EventHandler(menuItemStartup_Click);

            MenuItem menuItemLog = new MenuItem() { Index = 3, Text = "Log" };
            menuItemLog.Click += new EventHandler(MenuItemLog_Click);

            // initialize contextMenu
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItemExit, menuItemHistory, menuItemStartup, menuItemLog });

            // initialize NotifyIcon
            notifyIcon = new NotifyIcon();
            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            //new season
            File.AppendAllText(logpath, "" + Environment.NewLine);

            //timer
            Timer timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 1000; // in miliseconds
            timer.Start();

            //new season for sleep event
            SystemEvents.PowerModeChanged += OnPowerChange;
        }
        private void OnPowerChange(object s, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    File.AppendAllText(logpath, "" + Environment.NewLine);
                    logbattery();
                    break;
                case PowerModes.Resume:
                    logbattery();
                    File.AppendAllText(logpath, "" + Environment.NewLine);
                    logbattery();
                    break;
            }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Form1 frm = new Form1();
            frm.Show();
        }
        private void menuItemHistory_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();
            frm.Show();
        }

        private void MenuItemLog_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(logpath);
        }

        private void logbattery()
        {
            int Percent = Convert.ToInt32(SystemInformation.PowerStatus.BatteryLifePercent * 100);
            oldPercentage = Percent;
            File.AppendAllText(logpath, Percent.ToString() + "\t" + DateTime.Now.ToLongTimeString() + "\t" + DateTime.Now.ToShortDateString() + Environment.NewLine);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            int Percent = Convert.ToInt32(powerStatus.BatteryLifePercent * 100);
            if (oldPercentage == Percent)
                return;

            logbattery();

            //icon draw
            string text = (Percent == 100) ? " F" : (Percent < 10) ? "  " + Percent.ToString() : Percent.ToString();

            Bitmap image = new Bitmap(16, 16);

            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                g.DrawString(text, new Font("Microsoft Sans Serif", 16, FontStyle.Regular, GraphicsUnit.Pixel), new SolidBrush(Color.White), -4, -2);
            }

            notifyIcon.Icon = Icon.FromHandle(image.GetHicon());

            //notifications
            if (Percent >= 80 && powerStatus.PowerLineStatus == PowerLineStatus.Online)
            {
                notifyIcon.BalloonTipText = "Release Charger for Better Battery life";
                notifyIcon.BalloonTipTitle = "Battery Charged";
                notifyIcon.ShowBalloonTip(3);
            }
            else if (Percent <= 40 && powerStatus.PowerLineStatus == PowerLineStatus.Offline)
            {
                notifyIcon.BalloonTipText = "Connect Charger for Better Battery life";
                notifyIcon.BalloonTipTitle = "Battery Finished";
                notifyIcon.ShowBalloonTip(3);
            }
        }
        private void menuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }
        
        private void menuItemStartup_Click(object sender, EventArgs e)
        {
            MenuItem m = (MenuItem)sender;
            if (m.Checked)
                rk.DeleteValue(Application.ProductName, false);
            else
                rk.SetValue(Application.ProductName, Application.ExecutablePath);
            m.Checked = (rk.GetValue(Application.ProductName) != null && rk.GetValue(Application.ProductName).ToString() == Application.ExecutablePath);
        }
    }
}
