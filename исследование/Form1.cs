using System;
using System.Threading;
using System.Windows.Forms;
using IO;
using sortings;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace исследование
{
    public partial class Form1 : Form
    {
        
        private Image originalImage;
        Coordinates coords = null;
        Sorting sorting = null;
        Thread sortingThread = null;
        int Kx, Ky;
        private int ris_tik;
        int pointPerTik = 5;

        // 
        // PictureBox1
        // 
        private Point startMovePoint;
        private Point startСursorPoint;
        private Rectangle cropArea;
        private float zoom = 1.0f;

        public Form1()
        {
            InitializeComponent();
            /*
            ris.Interval = 50;
            ost_sort.Interval = 100;
            textBox1.Text = "Откройте файл ...";
            Kx = Convert.ToInt32(textBoxKX.Text);
            Ky = Convert.ToInt32(textBoxKY.Text);
            labelSpeed.Text = pointPerTik.ToString();
            listBox1.SelectedIndex = 0;
            drivesComboBox.SelectedIndex = 0;*/
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            sortingThread?.Abort();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                originalImage = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = originalImage;
            }
            cropArea = new Rectangle(0, 0, originalImage.Width, originalImage.Height);
           /* coords = new Coordinates(monoBMP, filename);
            sorting = null;
            textBox1.Text = coords.GetFileInfo();
            coords = null;
            textBox1.Text = ex.Message;*/

        }

        private void buttonSort_Click(object sender, EventArgs e)
        {

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {

        }

        // 
        // PictureBox1
        // 

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                startСursorPoint = e.Location;
                startMovePoint = cropArea.Location;
            }
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                cropArea.X = startMovePoint.X + cropArea.Width * (startСursorPoint.X - e.X) / pictureBox1.Width;
                cropArea.Y = startMovePoint.Y + cropArea.Height * (startСursorPoint.Y - e.Y) / pictureBox1.Height;
                CheckAriaBorder(ref cropArea);
                pictureBox1.Image = new Bitmap(originalImage).Clone(cropArea, originalImage.PixelFormat);
            }
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (originalImage is null)
                return;

            if (e.Delta > 0)
            {
                zoom *= 1.1f;
            }
            else
            {
                zoom /= 1.1f;
                if (zoom <= 1f)
                {
                    zoom = 1f;
                    pictureBox1.Image = originalImage;
                    return;
                }
            }

            cropArea.Width = (int)(originalImage.Width / zoom);
            cropArea.Height = (int)(originalImage.Height / zoom);
            cropArea.X += (pictureBox1.Image.Width - cropArea.Width) * e.X / pictureBox1.Width;
            cropArea.Y += (pictureBox1.Image.Height - cropArea.Height) * e.Y / pictureBox1.Height;
            CheckAriaBorder(ref cropArea);

            pictureBox1.Image = new Bitmap(originalImage).Clone(cropArea, originalImage.PixelFormat);
        }
        private void CheckAriaBorder(ref Rectangle aria)
        {
            if (aria.X < 0)
            {
                aria.X = 0;
            }
            else if (aria.X > originalImage.Width - aria.Width)
            {
                aria.X = originalImage.Width - aria.Width;
            }

            if (aria.Y < 0)
            {
                aria.Y = 0;
            }
            else if (aria.Y > originalImage.Height - aria.Height)
            {
                aria.Y = originalImage.Height - aria.Height;
            }
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBox1.Image is null)
                return;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.DrawImage(pictureBox1.Image, pictureBox1.ClientRectangle);
        }

    }
}


