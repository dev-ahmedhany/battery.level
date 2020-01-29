using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace battery.level
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] lines = File.ReadAllLines(Application.StartupPath + @"\BatteryLevelLog.txt");
            int j = 0;
            TimeSpan x = new TimeSpan();
            DateTime y = new DateTime();
            bool a = true;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "")
                {
                    a = true;
                    j++;
                    chart1.Series.Add("season" + j.ToString());
                    chart1.Series[j].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                }
                else
                {
                    string[] text = lines[i].Split('\t');
                    if (text.Length != 3)
                    {
                        continue;
                    }
                    DateTime z = new DateTime();
                    z = Convert.ToDateTime(text[1] + text[2]);
                    if (a)
                    {
                        y = z - x;
                        a = false;
                    }
                    x = z - y;
                    chart1.Series[j].Points.AddXY(x.TotalMinutes, text[0]);

                }
            }

        }
    }
}
