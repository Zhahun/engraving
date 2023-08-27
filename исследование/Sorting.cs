using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace исследование
{
    internal class Sorting
    { 
        Coordinates coords;
        public Dictionary<int, Action> sortTypes;
        public int unsortedCount = 0;
        public int[] sortedX;
        public int[] sortedY;

        public Sorting(Coordinates coords)
        {
            sortTypes = new Dictionary<int, Action>
            {
                {0, NonSort},
                {1, Sort1 },
                {2, Sort2 },
                {3, Sort3 },
                {4, Sort4 },
                {5, Sort5 },
                {6, Sort6 },
            };
            this.coords = coords;
        }

        public void NonSort()
        {
            sortedX = coords.imgX.ToArray();
            sortedY = coords.imgY.ToArray();
        }

        public long getDistance()
        {
            int x = sortedX[0];
            int y = sortedY[0];
            double distance = 0;

            for (int i = 0; i < sortedX.Length; i++)
            {
                int x1 = sortedX[i];
                int y1 = sortedY[i];
                distance += Math.Sqrt(Math.Pow(x1 - x, 2) + Math.Pow(y1 - y, 2));
                x = x1;
                y = y1;
            }
            return (long) distance;
        }

        public void Sort1()
        {
         /*   while (unsortedPoints.Count != 0)
            {
                foreach ((int a, int b) in neighbСoords)
                {
                    if (x + a, y + b) in cikl_zadanie
                    {
                        min_nom = (x + a, y + b);
                        break;
                    }
                }
                int min = int.MaxValue;
                foreach ((int x1, int y1) in cikl_zadanie)
                {
                    int dx = x1 - x;
                    int dy = y1 - y;
                    int min_temp = dx * dx + dy * dy;
                    if (min > min_temp)
                    {
                        min = min_temp;
                        min_nom = (x1, y1);
                    }
                }
                (x, y) = min_nom;
                cikl_zadanie.Remove(min_nom);
            }*/
        }

        public void Sort2() {}
        public void Sort3() {}
        public void Sort4() {}
        public void Sort5() {}
        public void Sort6() {}
    }

    internal class Coordinates
    {
        public List<int> imgX;
        public List<int> imgY;
        public int length = 0;

        public Coordinates()
        {
        }

        public void getCoords()
        {
            byte[] imgBytes = readFile();
            setupPoints(imgBytes);   // записывает точки из картинки в chart1.Series["raw"].Points
        }

        private byte[] readFile()
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() != DialogResult.OK)
                throw new Exception("Файл не выбран");

            BinaryReader reader = new BinaryReader(File.Open(
                fileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            byte[] imgBytes = reader.ReadBytes((int)reader.BaseStream.Length);
            reader.Close();
            fileDialog.Dispose();
            
            if (imgBytes[0] != 0x42 || imgBytes[1] != 0x4D)
                throw new Exception("Файл не являяется mono bmp");

            return imgBytes;
        }

        private void setupPoints(byte[] imgBytes)
        {
            imgX = new List<int>();
            imgY = new List<int>(); 
            int width = BitConverter.ToInt32(imgBytes, 18);
            int height = BitConverter.ToInt32(imgBytes, 22);
            int padding = (4 - width % 32 / 8) % 4; //дополнение байт до 4
            int byteInd = 62;
            int bitInd = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte imgByte = imgBytes[byteInd];
                    int mask = 128 >> bitInd;
                    if ((imgByte & mask) == 0)
                    {
                        imgX.Add(x);
                        imgY.Add(y);
                    }
                    bitInd++;
                    if (bitInd == 8)
                    {
                        bitInd = 0;
                        byteInd++;
                    }
                }
                bitInd = 0;
                byteInd += padding;
            }
            length = imgX.Count;
        }
    }
}
