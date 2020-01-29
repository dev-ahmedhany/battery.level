using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace battery.level
{
    class TrayIcon
    {
        private int oldPercentage;
        private NotifyIcon notifyIcon;
        public static string logpath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + @"\BatteryLevelLog.txt";
        public TrayIcon()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItemExit = new MenuItem(), menuItemStartup = new MenuItem(), menuItemHistory = new MenuItem();
            notifyIcon = new NotifyIcon();
            // initialize contextMenu
            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItemExit, menuItemHistory, menuItemStartup });
            // initialize menuItem
            menuItemExit.Index = 0;
            menuItemExit.Text = "Exit";
            menuItemExit.Click += new System.EventHandler(menuItem_Click);
            menuItemHistory.Index = 1;
            menuItemHistory.Text = "History";
            menuItemHistory.Click += new EventHandler(menuItemHistory_Click);
            menuItemStartup.Index = 2;
            menuItemStartup.Text = "Run at Startup";
            menuItemStartup.Click += new EventHandler(menuItemStartup_Click);

            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk.GetValue(Application.ProductName) != null && rk.GetValue(Application.ProductName).ToString() == Application.ExecutablePath)
            {
                menuItemStartup.Checked = true;
            }
            else
                menuItemStartup.Checked = false;

            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Visible = true;
            File.AppendAllText(logpath, "" + Environment.NewLine);
            //timer
            Timer timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 1000; // in miliseconds
            timer.Start();
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            int batteryPercentage = Convert.ToInt32(powerStatus.BatteryLifePercent * 100);
            if (oldPercentage != batteryPercentage)
            {
                oldPercentage = batteryPercentage;

                Brush textBrush = new SolidBrush(Color.White);
                Bitmap image = new Bitmap(16, 16);
                Graphics g = Graphics.FromImage(image);
                g.Clear(Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                string text = batteryPercentage.ToString();
                if (batteryPercentage == 100)
                {
                    text = " F";
                }
                else if (batteryPercentage < 10)
                {
                    text = "  " + text;
                }
                g.DrawString(text, new Font("Microsoft Sans Serif", 16, FontStyle.Regular, GraphicsUnit.Pixel), textBrush, -4, -2);

                System.IntPtr intPtr = image.GetHicon();
                notifyIcon.Icon = Icon.FromHandle(intPtr);

                File.AppendAllText(logpath, batteryPercentage.ToString() + "\t" + DateTime.Now.ToShortTimeString() + "\t" + DateTime.Now.ToShortDateString() + Environment.NewLine);
            }
        }
        private void menuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }
        private void menuItemHistory_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();
            frm.Show();
        }
        private void menuItemStartup_Click(object sender, EventArgs e)
        {
            MenuItem m = (MenuItem)sender;
            m.Checked = !m.Checked;
                RegistryKey rk = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (m.Checked)
                    rk.SetValue(Application.ProductName, Application.ExecutablePath);
                else
                    rk.DeleteValue(Application.ProductName, false);
        }
    }
}
