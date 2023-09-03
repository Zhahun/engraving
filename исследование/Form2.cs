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
using Microsoft.VisualBasic;
using System.Media;
using System.Management;
using System.Runtime.InteropServices;



namespace исследование
{
    public partial class Form1 : Form
    {

        List<byte> temp_file = new List<byte>();
        List<uint> cikl_zadanie_x = new List<uint>();
        List<uint> cikl_zadanie_y = new List<uint>();
        List<uint> sort_x = new List<uint>();
        List<uint> sort_y = new List<uint>();
        List<uint> OTSORT = new List<uint>();


        private int ris_tik;
        int mili;
        private string число_секторов = "4";
        private int vip_sekt;

        public Form1()
        {
            InitializeComponent();
            label23.Text = ris.Interval.ToString();
            mili = ris.Interval;
            this.MouseWheel += new MouseEventHandler(this_MouseWheel);
            listBox1.SelectedIndex = 0;
            InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new System.Globalization.CultureInfo("en-US"));//что б по умолчанию был англиский язык
            textBox1.Clear();
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
        

        private void _()
        {
            temp_file.Clear();
            int temp = 0;
            int kon_sec = 16479 + OTSORT.Count / 512;

            temp_file.AddRange(BitConverter.GetBytes(kon_sec));
            temp_file.AddRange(BitConverter.GetBytes(0));

            temp_file.Insert(0, (byte)(kon_sec >> 24));
            temp_file.Insert(1, (byte)((kon_sec & 0x00ff0000) >> 16));
            temp_file.Insert(2, (byte)((kon_sec & 0x0000ff00) >> 8));
            temp_file.Insert(3, (byte)(kon_sec & 0x000000ff));
            temp_file.Insert(4, 0);
            temp_file.Insert(5, 0);
            temp_file.Insert(6, 0);
            temp_file.Insert(7, 0);
        }  

        private void button1_Click(object sender, EventArgs e)
        {   
            DriveInfo[] info = DriveInfo.GetDrives();
            string usb = null;
            for (int i = 0; i < info.Count(); i++)
            {
                if (info[i].DriveType.ToString() == "Removable") usb += info[i].ToString().Substring(0, 1);
            }

            IntPtr handle = CreateFile(
                @"\\.\" + usb[0] + ":", 
                (uint)(DesiredAccess.GENERIC_READ | DesiredAccess.GENERIC_WRITE),
                (uint)(ShareMode.FILE_SHARE_READ | ShareMode.FILE_SHARE_WRITE),
                0,
                (uint)CreationDisposition.OPEN_EXISTING, 
                (uint)FlagsAndAttributes.FILE_ATTRIBUTE_NORMAL,
                0
            );

            bool OK = WriteBuffer(handle, temp_file);
        }

        static unsafe bool WriteBuffer(IntPtr handle, List<byte> temp_)
        {
            int n = 0;
            int kr1 = temp_.Count();
            while (kr1 % 512 != 0) kr1++;

            byte[] buffer = new byte[kr1 + 512];
            for (int i = 0; i < temp_.Count; i++)
            {
                buffer[i] = temp_[i];
            }
            for (int i = temp_.Count; i < kr1 + 512; i++)
            {
                buffer[i] = 0;
            }
            fixed (byte* pointer = buffer)
            {
                return WriteFile(handle, pointer, kr1 + 512, &n, 0);
            }
        }


        enum DesiredAccess : uint
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000
        }

        enum ShareMode : uint
        {
            FILE_SHARE_READ = 0x00000001,
            FILE_SHARE_WRITE = 0x00000002,
            FILE_SHARE_DELETE = 0x00000004
        }

        enum CreationDisposition : uint
        {
            CREATE_ALWAYS = 2,
            CREATE_NEW = 1,
            OPEN_ALWAYS = 4,
            OPEN_EXISTING = 3,
            TRUNCATE_EXISTING = 5
        }

        enum FlagsAndAttributes : uint
        {
            FILE_ATTRIBUTE_ARCHIVE = 0x20,
            FILE_ATTRIBUTE_ENCRYPTED = 0x4000,
            FILE_ATTRIBUTE_HIDDEN = 0x2,
            FILE_ATTRIBUTE_NORMAL = 0x80,
            FILE_ATTRIBUTE_OFFLINE = 0x1000,
            FILE_ATTRIBUTE_READONLY = 0x1,
            FILE_ATTRIBUTE_SYSTEM = 0x4,
            FILE_ATTRIBUTE_TEMPORARY = 0x100
        }

        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe System.IntPtr CreateFile
        (
            string FileName,          // file name
            uint DesiredAccess,       // access mode
            uint ShareMode,           // share mode
            uint SecurityAttributes,  // Security Attributes
            uint CreationDisposition, // how to create
            uint FlagsAndAttributes,  // file attributes
            int hTemplateFile         // handle to template file
        );


        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe bool WriteFile
        (
            System.IntPtr hFile,      // handle to file
            void* pBuffer,            // data buffer
            int NumberOfBytesToRead,  // number of bytes to read
            int* pNumberOfBytesRead,  // number of bytes read
            int Overlapped            // overlapped buffer
        );

        public enum EMoveMethod : uint
        {
            Begin = 0,
            Current = 1,
            End = 2
        }

        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe bool CloseHandle
        (
            IntPtr hObject // handle to object
        );
    }
}
