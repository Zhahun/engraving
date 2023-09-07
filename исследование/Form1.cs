using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;


namespace исследование
{
    public partial class Form1 : Form
    {
        Coordinates coords = null;
        Sorting sorting = null;
        Thread sortingThread = null;
        int Kx, Ky;
        private int ris_tik;
        int pointPerTik = 5;

        public Form1()
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(this_MouseWheel);
            ris.Interval = 50;
            ost_sort.Interval = 100;
            textBox1.Text = "Откройте файл ...";
            Kx = Convert.ToInt32(textBoxKX.Text);
            Ky = Convert.ToInt32(textBoxKY.Text);
            labelSpeed.Text = pointPerTik.ToString();
            listBox1.SelectedIndex = 0;
            drivesComboBox.SelectedIndex = 0;
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            sortingThread?.Abort();
            ris.Stop();
            chart1.Series["engraved"].Points.Clear();
            checkBox3.Checked = false;
            try
            {
                coords = new Coordinates();
                coords.GetCoordsFromFile();
                sorting = null;
                chart1.Series["raw"].Points.DataBindXY(coords.X, coords.Y);
                textBox1.Text = coords.GetFileInfo();
            }
            catch (Exception ex)
            {
                coords = null;
                textBox1.Text = ex.Message;
            }
        }
        private void buttonSort_Click(object sender, EventArgs e)
        {
            if (coords == null)
            {
                textBox1.Text = "Выберите файл...";
                return;
            }

            sortingThread?.Abort();
            chart1.Series["engraved"].Points.Clear();
            checkBox3.Checked = false;

            sorting = new Sorting(coords);
            ThreadStart threadStart = new ThreadStart(sorting.sortTypes[listBox1.SelectedIndex]);
            sortingThread = new Thread(threadStart);
            sortingThread.Start();
            ost_sort.Start();
        }


        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (sorting == null)
            {
                textBox1.Text = "Проведите сортировку...";
                return;
            }
            if (drivesComboBox.SelectedIndex == 0)
            {
                textBox1.Text = "Выберите диск";
                return;
            }

            try
            {
                SDFileManager saver = new SDFileManager(sorting.sortedX, sorting.sortedY, Kx, Ky);
                if (saver.Write(drivesComboBox.Text))
                    textBox1.Text = "Файл сохранён";
                else
                    textBox1.Text = "Не удалось сохранить файл";
            }
            catch (Exception ex)
            {
                coords = null;
                textBox1.Text = ex.Message;
            }
        }
        /*
        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (sorting == null)
            {
                textBox1.Text = "Проведите сортировку...";
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = coords.filename;

            saveFileDialog1.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1; // Выберите начальный фильтр

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog1.FileName;
                string dataToSave = "Пример текста для сохранения в файле.";
                File.WriteAllText(filePath, dataToSave);
                MessageBox.Show("Файл успешно сохранен!");
            }
        }
        */
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            ris.Stop();
            ris_tik = 0;
            if (checkBox3.Checked)
            {
                if (sorting == null)
                {
                    textBox1.Text = "Проведите сортировку...";
                    return;
                }
                chart1.Series["engraved"].Points.Clear();
                ris.Start();
            }
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
            if (sortingThread.IsAlive)
                textBox1.Text = $"Выполнение, осталось отсортировать точек: {sorting.unsortedCount}";
            else
            {
                textBox1.Text = (   
                    coords.GetFileInfo() +
                    "Сортировка завершена!\r\n" +
                    $"Сортировка: {sorting.lastSortName}\r\n" +
                    $"Приблизительное время прожига: {sorting.GetTime()}\r\n"
                );
                ost_sort.Stop();
            }
        }

        private void this_MouseWheel(object sender, MouseEventArgs e)
        {
            pointPerTik = (e.Delta > 0) ? (pointPerTik * 11 / 10 + 1)
                                        : (pointPerTik * 9 / 10);
            if (pointPerTik < 1)
                pointPerTik = 1;
            labelSpeed.Text = pointPerTik.ToString();
        }

        private void textBoxKX_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(textBoxKX.Text, out Kx);
        }

        private void textBoxKY_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(textBoxKY.Text, out Ky);
        }


        private void drivesComboBox_DropDown(object sender, EventArgs e)
        {
            var removable = DriveInfo.GetDrives().Where(drive => drive.DriveType == DriveType.Removable);
            foreach (var drive in removable)
            {
                if (!drivesComboBox.Items.Contains(drive.Name))
                    drivesComboBox.Items.Add(drive.Name);
            }

            if (drivesComboBox.Items.Count == 1)
                drivesComboBox.Items[0] = "Съёмные диски не найдены";
            else
                drivesComboBox.Items[0] = "Диск не выбран";
        }
    }

    class SDFileManager
    {
        private readonly byte[] buffer;
        public SDFileManager(int[] X, int[] Y, int Kx, int Ky)
        {
            List<byte> byteList = new List<byte>();

            int secEnd = 16479 + X.Length / 64; // Результирующее число байт / 512
            byteList.AddRange(BitConverter.GetBytes(secEnd));
            byteList.AddRange(BitConverter.GetBytes(0));
            for (int i = 0; i < X.Length; i++)
            {
                byteList.AddRange(BitConverter.GetBytes(X[i] * Kx));
                byteList.AddRange(BitConverter.GetBytes(Y[i] * Ky));
            }
            int padding = (512 - byteList.Count % 512) % 512; 
            byte[] zeroes = new byte[padding];
            byteList.AddRange(zeroes);
            buffer = byteList.ToArray();
        }

        public bool Write(string driveName)
        {
            SafeFileHandle fileHandle = CreateFile(
                @"\\.\" + driveName,
                FileAccess.Write,
                FileShare.None,
                IntPtr.Zero,
                FileMode.Create,
                FileAttributes.Normal,
                IntPtr.Zero
            );
            
            using (fileHandle)
            {
                if (fileHandle.IsInvalid)
                    return false;

                return WriteFile(
                    fileHandle,
                    buffer,
                    (uint) buffer.Length,
                    out uint bytesWritten, 
                    IntPtr.Zero
                );
            }
            
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteFile(
            SafeHandle hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            FileAccess dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            FileMode dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );
    }
}

