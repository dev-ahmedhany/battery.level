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
            
            string[] lines = File.ReadAllLines(TrayIcon.logpath);
            int j = 0;
            TimeSpan timeSpan = new TimeSpan();
            DateTime currentdatetimebeforetimespan = new DateTime();
            bool a = true;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "")//new season
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
                    DateTime currentdatetime = new DateTime();
                    currentdatetime = Convert.ToDateTime(text[1] + text[2]);
                    if (a)
                    {
                        currentdatetimebeforetimespan = currentdatetime - timeSpan;
                        a = false;
                    }
                    timeSpan = currentdatetime - currentdatetimebeforetimespan;
                    chart1.Series[j].Points.AddXY(timeSpan.TotalSeconds/60, text[0]);

                }
            }

            //to reduce chart to last 1000 min
            int final = Convert.ToInt32(timeSpan.TotalSeconds/60);
            if (final > 1000)
            {
                foreach (var series in chart1.Series)
                {
                    for (int i = 0; i < series.Points.Count; i++)
                    {
                        if(series.Points[i].XValue < final - 1000)
                        {
                            series.Points.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            //say point 4000 of 5000
                            //make it 4000 + 1000 - 5000 = 0
                            //say point 5000 of 5000
                            //make it 5000 + 1000- 5000 = 1000
                            series.Points[i].XValue = series.Points[i].XValue + 1000 - final;

                        }
                    }
                }
                chart1.Update();
            }
        }
    }
}
