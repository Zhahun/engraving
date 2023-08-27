using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace исследование
{
    public partial class Form1 : Form
    {

        List<byte> temp_file = new List<byte>();

        public byte[] comand = new byte[536];

        int z = 0;

        int min_nom;
        int x1 = 0;
        int y1 = 0;
        int x2 = 0;
        int y2 = 0;



        List<int> shim_sort = new List<int>();

        List<int> cikl_zadanie_x = new List<int>();
        List<int> cikl_zadanie_y = new List<int>();
        List<int> cikl_zadanie_shim = new List<int>();
        List<int> cikl_zadanie_r = new List<int>();
        List<int> cikl_zadanie_g = new List<int>();
        List<int> cikl_zadanie_b = new List<int>();
        List<int> sort_x = new List<int>();
        List<int> sort_y = new List<int>();
        List<int> OTSORT = new List<int>();
        List<int> otr_x = new List<int>();
        List<int> otr_y = new List<int>();

        public float zoom;
        int k_pr_x;
        int k_pr_y;
        private int ris_tik;
        int mili;

        public Form1()
        {
            InitializeComponent();
            label23.Text = ris.Interval.ToString();
            mili = ris.Interval;
            this.MouseWheel += new MouseEventHandler(this_MouseWheel);
            listBox1.SelectedIndex = 0;
            InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new System.Globalization.CultureInfo("en-US"));//что б по умолчанию был англиский язык
            textBox1.Clear();
            k_pr_x = Convert.ToInt32(textBox11.Text);
            k_pr_y = Convert.ToInt32(textBox12.Text);
            //chart1.Visible = false;
            ris.Stop();
            chart1.Series[0].Points.Clear();
            chart1.Series[0].BorderWidth = 1;
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            chart1.Series[0].BorderWidth = 1;
            Axis ax = new Axis();
            ax.Title = "Точки по оси Х (см)";
            chart1.ChartAreas[0].AxisX = ax;
            Axis ay = new Axis();
            ay.Title = "Точки по оси Y (см)";
            chart1.ChartAreas[0].AxisY = ay;

        }
        void this_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                mili++;
                label23.Text = mili.ToString();
                ris.Interval = mili;
            }

            else
            {
                mili--;
                label23.Text = mili.ToString();
                ris.Interval = mili;
                if (mili < 10) mili = 10;
            }

        }

        private void button10_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            ris.Stop();
            OTSORT.Clear();
            // Открыть файл для анализа
            BinaryReader reader;
            try
            {
                textBox1.Text = "";
                temp_file.Clear();
                cikl_zadanie_x.Clear();
                cikl_zadanie_y.Clear();
                cikl_zadanie_shim.Clear();
                cikl_zadanie_r.Clear();
                cikl_zadanie_g.Clear();
                cikl_zadanie_b.Clear();
                otr_x.Clear();
                otr_y.Clear();
                var ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (reader = new BinaryReader(File.Open(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            temp_file.Add(reader.ReadByte());
                        }
                        reader.Close();
                    }
                    if (temp_file[0] == 0x42 && temp_file[1] == 0x4D)
                    {
                        int _x1 = 0, _x2 = 0, _x3 = 0, _x4 = 0;
                        _x1 = (int)temp_file[21];
                        _x1 = _x1 << 24;
                        _x2 = (int)temp_file[20];
                        _x2 = _x2 << 16;
                        _x3 = (int)temp_file[19];
                        _x3 = _x3 << 8;
                        _x4 = (int)temp_file[18];
                        int shirina = (int)_x1 + (int)_x2 + (int)_x3 + (int)_x4;
                        _x1 = (int)temp_file[25];
                        _x1 = _x1 << 24;
                        _x2 = (int)temp_file[24];
                        _x2 = _x2 << 16;
                        _x3 = (int)temp_file[23];
                        _x3 = _x3 << 8;
                        _x4 = (int)temp_file[22];
                        int visota = (int)_x1 + (int)_x2 + (int)_x3 + (int)_x4;
                        temp_file.RemoveRange(0, 62);
                        int perebor_x = 0;
                        int ostatok = 0;
                        if (shirina % 32 == 0) perebor_x = shirina / 8;
                        else
                        {
                            perebor_x = ((shirina / 32) * 32 + 32) / 8;
                            ostatok = (perebor_x * 8 - shirina) / 8;
                        }
                        int perebor_all = 0;
                        int koord_y = 0;
                        while (perebor_all < temp_file.Count())
                        {
                            koord_x = 0;
                            for (int stroka = 0; stroka < perebor_x; stroka++)
                            {
                                if ((temp_file[perebor_all] & 0b10000000) == 0) zapis_koord(koord_x, koord_y, shirina);
                                koord_x++;
                                if (koord_x == shirina) goto dalee;
                                if ((temp_file[perebor_all] & 0b01000000) == 0) zapis_koord(koord_x, koord_y, shirina);
                                koord_x++;
                                if (koord_x == shirina) goto dalee;
                                if ((temp_file[perebor_all] & 0b00100000) == 0) zapis_koord(koord_x, koord_y, shirina);
                                koord_x++;
                                if (koord_x == shirina) goto dalee;
                                if ((temp_file[perebor_all] & 0b00010000) == 0) zapis_koord(koord_x, koord_y, shirina);
                                koord_x++;
                                if (koord_x == shirina) goto dalee;
                                if ((temp_file[perebor_all] & 0b00001000) == 0) zapis_koord(koord_x, koord_y, shirina);
                                koord_x++;
                                if (koord_x == shirina) goto dalee;
                                if ((temp_file[perebor_all] & 0b00000100) == 0) zapis_koord(koord_x, koord_y, shirina);
                                koord_x++;
                                if (koord_x == shirina) goto dalee;
                                if ((temp_file[perebor_all] & 0b00000010) == 0) zapis_koord(koord_x, koord_y, shirina);
                                koord_x++;
                                if (koord_x == shirina) goto dalee;
                                if ((temp_file[perebor_all] & 0b00000001) == 0) zapis_koord(koord_x, koord_y, shirina);
                                koord_x++;
                                if (koord_x == shirina) goto dalee;
                                perebor_all++;
                            }
                        dalee:
                            perebor_all = perebor_all + ostatok;
                            koord_y++;
                            perebor_all++;
                        }
                    }
                    textBox1.Text = "Кадров=" + cikl_zadanie_x.Count().ToString();

                    ofd.Dispose();
                    if (cikl_zadanie_x.Count() == 0)
                    {
                        textBox1.Text = "Файл не являяется mono bmp";
                        return;
                    }
                    int minX = cikl_zadanie_x.Min();
                    int minY = cikl_zadanie_y.Min();
                    int maxX = cikl_zadanie_x.Max();
                    int maxY = cikl_zadanie_y.Max();
                }

                ofd.Dispose();

                chart1.Visible = true;
                chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
                chart1.Series[0].Points.Clear();

                switch (listBox1.SelectedIndex)
                {
                    case 0:
                        textBox1.Text = "Ждите идет сортировка!!!";
                        Thread t0 = new Thread(new ThreadStart(fun_non_sort));
                        t0.Start();
                        ost_sort.Start();
                        break;
                    case 1:
                        textBox1.Text = "Ждите идет сортировка!!!";
                        Thread t1 = new Thread(new ThreadStart(fun_sort_1));
                        t1.Start();
                        ost_sort.Start();
                        break;
                    case 2:
                        textBox1.Text = "Ждите идет сортировка!!!";
                        Thread t2 = new Thread(new ThreadStart(fun_sort_2));
                        t2.Start();
                        ost_sort.Start();
                        break;
                    case 3:
                        textBox1.Text = "Ждите идет сортировка!!!";
                        Thread t3 = new Thread(new ThreadStart(fun_sort_3));
                        t3.Start();
                        ost_sort.Start();
                        break;
                    case 4:
                        textBox1.Text = "Ждите идет сортировка!!!";
                        Thread t4 = new Thread(new ThreadStart(fun_sort_4));
                        t4.Start();
                        ost_sort.Start();
                        break;
                    case 5:
                        textBox1.Text = "Ждите идет сортировка!!!";
                        Thread t5 = new Thread(new ThreadStart(fun_sort_5));
                        t5.Start();
                        ost_sort.Start();
                        break;
                    case 6:
                        textBox1.Text = "Ждите идет сортировка!!!";
                        Thread t6 = new Thread(new ThreadStart(fun_sort_6));
                        t6.Start();
                        ost_sort.Start();
                        break;

                }




            }
            catch (IOException ex)
            {
                textBox1.Text = ex.ToString();
            }

        }
        private void fun_non_sort()
        {
            temp_file.Clear();
            int temp = 0;
            OTSORT.Clear();
            textBox1.BeginInvoke(new Action(() => { textBox1.Text = "Идет преобразование->" + cikl_zadanie_x.Count.ToString(); }));
            while (temp != cikl_zadanie_x.Count)
            {
                OTSORT.Add(cikl_zadanie_x[temp]);
                OTSORT.Add(cikl_zadanie_y[temp]);
                temp++;
            }
            chart1.BeginInvoke(new Action(() => { chart1.Series[0].Points.DataBindXY(cikl_zadanie_x, cikl_zadanie_y); }));
            textBox1.BeginInvoke(new Action(() => { textBox1.Text = "Выполнено!!!" + cikl_zadanie_x.Count.ToString(); }));
            ost_sort.Stop();
        }
        private void fun_sort_1()
        {
            try
            {

                sort_x.Clear();
                sort_y.Clear();
                OTSORT.Clear();
                shim_sort.Clear();
                x1 = cikl_zadanie_x[0];
                y1 = cikl_zadanie_y[0];
                OTSORT.Add(x1);
                OTSORT.Add(y1);
                sort_x.Add(x1);
                sort_y.Add(y1);
                cikl_zadanie_x.RemoveAt(0);
                cikl_zadanie_y.RemoveAt(0);

                while (cikl_zadanie_x.Count != 0)
                {
                    double min = 1000000;
                    for (z = 0; z < cikl_zadanie_x.Count; z++)
                    {
                        x2 = cikl_zadanie_x[z];
                        y2 = cikl_zadanie_y[z];
                        int min_temp = Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
                        if (min > min_temp)
                        {
                            min = min_temp;
                            min_nom = z;
                            if (x2 == x1 && (Math.Abs(y2 - y1) <= k_pr_y)) break;
                            if (y2 == y1 && (Math.Abs(x2 - x1) <= k_pr_x)) break;
                        }
                    }
                    OTSORT.Add(cikl_zadanie_x[min_nom]);
                    OTSORT.Add(cikl_zadanie_y[min_nom]);
                    //chart1.Series[0].Points.AddXY(cikl_zadanie_x[min_nom], cikl_zadanie_y[min_nom]);
                    sort_x.Add(cikl_zadanie_x[min_nom]);
                    sort_y.Add(cikl_zadanie_y[min_nom]);
                    x1 = cikl_zadanie_x[min_nom];
                    y1 = cikl_zadanie_y[min_nom];
                    cikl_zadanie_x.RemoveAt(min_nom);
                    cikl_zadanie_y.RemoveAt(min_nom);


                }
                textBox1.BeginInvoke(new Action(() => { textBox1.Text = "Выполнено!!!" + sort_x.Count.ToString(); }));
                chart1.BeginInvoke(new Action(() => { chart1.Series[0].Points.DataBindXY(sort_x, sort_y); }));
                ost_sort.Stop();


            }
            catch (Exception ex)
            {
                textBox1.BeginInvoke(new Action(() => { textBox1.Text = ex.ToString(); }));
            }

        }
        private void fun_sort_2()
        {
            try
            {

                sort_x.Clear();
                sort_y.Clear();
                OTSORT.Clear();
                shim_sort.Clear();
                x1 = cikl_zadanie_x[Convert.ToInt32(cikl_zadanie_x.Count / 2)];
                y1 = cikl_zadanie_y[Convert.ToInt32(cikl_zadanie_x.Count / 2)];
                OTSORT.Add(x1);
                OTSORT.Add(y1);
                sort_x.Add(x1);
                sort_y.Add(y1);
                cikl_zadanie_x.RemoveAt(0);
                cikl_zadanie_y.RemoveAt(0);
                List<int> temp_cikl_zadanie_x = new List<int>();
                List<int> temp_cikl_zadanie_y = new List<int>();
                List<int> отсортировано_X = new List<int>();
                List<int> отсортировано_Y = new List<int>();
                int temp_z = 0;
                отсортировано_X.Add(cikl_zadanie_x[temp_z]);
                отсортировано_Y.Add(cikl_zadanie_y[temp_z]);
                int poisk_x = k_pr_x;
                int poisk_y = k_pr_y;
                int n = 0;
                for (int q = 0; q < cikl_zadanie_x.Count; q++)
                {
                    temp_cikl_zadanie_x.Add(cikl_zadanie_x[q]);
                    temp_cikl_zadanie_y.Add(cikl_zadanie_y[q]);

                }
                while (temp_cikl_zadanie_x.Count != 160)
                {
                    for (int a = 1; a < temp_cikl_zadanie_x.Count; a++)
                    {
                        x2 = temp_cikl_zadanie_x[a];
                        y2 = temp_cikl_zadanie_y[a];
                        temp_z = a;
                        if (x2 - x1 == (a * poisk_x) && y2 == y1 && n == 0)// право
                        {
                            n = 1;
                            goto найдена_точка;
                        }
                        if (x2 - x1 == (a * poisk_x) && y2 - y1 == (a * poisk_y) && n == 1)// диоганаль право верх
                        {
                            n = 2;
                            goto найдена_точка;
                        }
                        if (x2 == x1 && y2 - y1 == (a * poisk_y) && n == 2)// вверх
                        {
                            n = 3;
                            goto найдена_точка;
                        }
                        if (x1 - x2 == (a * poisk_x) && y2 - y1 == (a * poisk_y) && n == 3)// диоганаль лево верх
                        {
                            n = 4;
                            goto найдена_точка;
                        }
                        if (x1 - x2 == (a * poisk_x) && y1 == y2 && n == 4)// лево
                        {
                            n = 5;
                            goto найдена_точка;
                        }
                        if (x1 - x2 == (a * poisk_x) && y1 - y2 == (a * poisk_y) && n == 5)// диоганаль лево низ
                        {
                            n = 6;
                            goto найдена_точка;
                        }
                        if (x1 == x2 && y1 - y2 == (a * poisk_y) && n == 6)//  низ
                        {
                            n = 7;
                            goto найдена_точка;
                        }
                        if (x2 - x1 == (a * poisk_x) && y1 - y2 == (a * poisk_y) && n == 7)// диоганаль право низ
                        {
                            n = 0;
                            goto найдена_точка;
                        }
                    }
                найдена_точка:
                    label23.BeginInvoke(new Action(() => { label23.Text = "Выполнено!!!" + temp_cikl_zadanie_x.Count.ToString(); }));
                    отсортировано_X.Add(temp_cikl_zadanie_x[temp_z]);
                    отсортировано_Y.Add(temp_cikl_zadanie_y[temp_z]);
                    OTSORT.Add(temp_cikl_zadanie_x[temp_z]);
                    OTSORT.Add(temp_cikl_zadanie_y[temp_z]);
                    x1 = temp_cikl_zadanie_x[poisk_x];
                    y1 = temp_cikl_zadanie_y[poisk_y];
                    temp_cikl_zadanie_x.RemoveAt(temp_z);
                    temp_cikl_zadanie_y.RemoveAt(temp_z);
                }
                textBox1.BeginInvoke(new Action(() => { textBox1.Text = "Выполнено!!!" + отсортировано_X.Count.ToString(); }));
                chart1.BeginInvoke(new Action(() => { chart1.Series[0].Points.DataBindXY(отсортировано_X, отсортировано_Y); }));
                ost_sort.Stop();


            }
            catch (Exception ex)
            {
                textBox1.BeginInvoke(new Action(() => { textBox1.Text = ex.ToString(); }));

            }
        }
        private void fun_sort_3()
        {
        }
        private void fun_sort_4()
        {
        }
        private void fun_sort_5()
        {
        }
        private void fun_sort_6()
        {
        }


        void zapis_koord(int x, int y, int shir)
        {
            cikl_zadanie_x.Add((int)(x * k_pr_x));
            cikl_zadanie_y.Add((int)(y * k_pr_y));
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            chart1.Series[1].Points.Clear();
            ris_tik = 0;
            ris.Interval = mili;
            if (checkBox3.Checked == true) ris.Start();
            else ris.Stop();
        }

        private void ris_Tick(object sender, EventArgs e)
        {
            if (OTSORT.Count == 0)
            {
                ris.Stop();
                checkBox3.Checked = false;
                return;

            }
            chart1.Series[1].Points.AddXY(OTSORT[ris_tik], OTSORT[ris_tik + 1]);
            ris_tik += 2;
            if (ris_tik == OTSORT.Count) ris.Stop();
        }

        private void ost_sort_Tick(object sender, EventArgs e)
        {
            textBox1.Text = "Выполнение, осталось отсортировать точек: " + cikl_zadanie_x.Count.ToString();
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            ris.Stop();
        }
    }

}
