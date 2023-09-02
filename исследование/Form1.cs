using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace исследование
{
    public partial class Form1 : Form
    {
        Coordinates coords = new Coordinates();
        Sorting sorting = null;
        Thread sortingThread = null;
        int k_pr_x, k_pr_y;
        private int ris_tik;
        int pointPerTik = 5;

        public Form1()
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(this_MouseWheel);
            ris.Interval = 50;
            ost_sort.Interval=100;
            k_pr_x = Convert.ToInt32(textBox11.Text);
            k_pr_y = Convert.ToInt32(textBox12.Text);
            label23.Text = pointPerTik.ToString();
            listBox1.SelectedIndex = 0;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (sortingThread != null)
                sortingThread.Abort();

            ris.Stop();
            chart1.Series["raw"].Points.Clear();
            chart1.Series["engraved"].Points.Clear();
            textBox1.Clear();
            checkBox3.Checked = false;
            try
            {
                coords.GetCoordsFromFile();
                chart1.Series["raw"].Points.DataBindXY(coords.X, coords.Y);
                sorting = new Sorting(coords);
                textBox1.Text = "Идёт сортировка...";
                ThreadStart threadStart = new ThreadStart(sorting.sortTypes[listBox1.SelectedIndex]);
                sortingThread = new Thread(threadStart);
                sortingThread.Start();
                ost_sort.Start();
            }
            catch (Exception ex)
            {
                sorting = null;
                textBox1.Text = ex.Message;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            ris.Stop();
            ris_tik = 0;

            if (checkBox3.Checked && sorting != null)
            { 
                chart1.Series["engraved"].Points.Clear();
                ris.Start();
            }
               
            else
                checkBox3.Checked = false;
        }

        private void ris_Tick(object sender, EventArgs e)
        {
            chart1.Series.SuspendUpdates();
            for (int i = 0; i < pointPerTik; i++)
            {
                if (ris_tik >= coords.length)
                {
                    ris.Stop();
                    break;
                }
                chart1.Series["engraved"].Points.AddXY(sorting.sortedX[ris_tik], sorting.sortedY[ris_tik]);
                ris_tik++;
            }
            chart1.Series.ResumeUpdates();
        }

        private void ost_sort_Tick(object sender, EventArgs e)
        {
            if(sortingThread.IsAlive)
                textBox1.Text = $"Выполнение, осталось отсортировать точек: {sorting.unsortedCount}";
            else
            { 
                textBox1.Text = $"Точек: {coords.length} \r\n Итоговый путь: {sorting.GetDistance()}";
                ost_sort.Stop();
            }
        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ris.Stop();
            chart1.Series["raw"].Points.Clear();
            chart1.Series["engraved"].Points.Clear();
            sorting = null;
        }

        private void this_MouseWheel(object sender, MouseEventArgs e)
        {
            pointPerTik = (e.Delta > 0) ? (pointPerTik * 11 / 10 + 1)
                                        : (pointPerTik * 9 / 10);
            if (pointPerTik < 1)
                pointPerTik = 1;
            label23.Text = pointPerTik.ToString();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

