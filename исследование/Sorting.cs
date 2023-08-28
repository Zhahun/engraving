﻿using System;
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
        public int[] sortedX;
        public int[] sortedY;
        public int unsortedCount = 0;

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
            sortedX = new int[coords.imgX.Count];
            sortedY = new int[coords.imgX.Count];
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
            return (long)distance;
        }

        public void Sort1()
        {
            HashSet<(int, int)> coordsSet = new HashSet<(int, int)> { };
            var points = coords.imgX.Zip(coords.imgY, (x, y) => (x, y));
            foreach ((int, int) point in points)
            {
                coordsSet.Add(point);
            }
        
            (int, int) startPoint = (0, 0);
            for (int i = 0; i < coords.imgX.Count; i++)
            {
                startPoint = brutForceSearch(startPoint, coordsSet);
                (sortedX[i], sortedY[i]) = startPoint;
                coordsSet.Remove(startPoint);
                unsortedCount = coordsSet.Count;
            }
        }

        private (int, int) brutForceSearch((int, int) point, HashSet<(int, int)> coordsSet)
        {
            (int x, int y) = point;
            (int, int)[] neighbСoords = new[] {(-1,0), (-1,1), (0,1), (1,1), (1,0), (1,-1), (0,-1), (-1,-1)};
            foreach ((int px, int py) in neighbСoords)
            {
                (int, int) point1 = (x + px, y + py);
                if (coordsSet.Contains(point1))
                    return point1;   
            } 
            int minDistance = int.MaxValue;
            foreach ((int x1, int y1) in coordsSet)
            {
                int dx = x1 - x;
                int dy = y1 - y;
                int distance = dx*dx+dy*dy;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    point = (x1, y1);
                }
            }
            return point;
        }

        public void Sort2()
        {
            List<int> uniqY = new List<int>(coords.uniqY);
            Dictionary <int, List<int>> YAxisToXValues = new <int, List<int>> [uniqY.Count];
            // y - значение координаты;
            foreach (y in uniqY)
            {
                YAxisToXValues.Add(y, new List<int>{});
            }
            for (int i=0; i<coords.length; i++)
            {
                YAxisToXValues[coords.imgY[i]].Add(imgX[i]);
            }

            int yIndex = 0;
            int curY = uniqY[yIndex];
            int curX = -1;
            int minX, minY;
            int minDistance = int.MaxValue;

            for (int i=0; i< coords.length; i++)
            {
                while (dy*dy < minDistance)
                {

                    y = (i1 != 0 & dy1 < dy2) ? YAxisToXValues[--i1]
                                              : YAxisToXValues[++i1];
                    x = BinarySearch(YAxisToXValues[y], x)
                    int dx = curX - x;
                    int dy = curY - y;
                    minDistance = dx * dx + dy * dy;
                }
               
                sortedX[i] = x;
                sortedY[i] = y;
                curX = x;
                curY = y;
            }   
        }

        public int BinarySearch(List<int> list, int value)
        {
            // i, firs, last - индексы в списке list;
            int last = list.Count - 1;
            int first = 0;
            int i = value * last / (list[last] - list[first]);
           
            while (first - last > 1) 
            {
                if (value > list[i]):
                {
                    first = i;
                    i = last + value * (last - first) / (list[last] - list[first]);
                }
                else
                {
                    last = i;
                    i = first + value * (last - first) / (list[last] - list[first]);
                }
            }

            if (value - first < last-value)
                return first;
            else 
                return last;
        }

        public void Sort3() { }
        public void Sort4() { }
        public void Sort5() { }
        public void Sort6() { }
   }

    internal class Coordinates
    {
        public List<int> imgX;
        public List<int> imgY;
        public int length = 0;
        public List<int> uniqY = new List<int> { };

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
            int last_y = null;
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
                        if (y != last_y)
                            uniqY.Add(y);
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