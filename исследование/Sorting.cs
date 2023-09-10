﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace исследование
{
    internal class Sorting
    {
        Coordinates coords;
        public Dictionary<int, Action> sortTypes;
        public int[] sortedX;
        public int[] sortedY;
        public int unsortedCount;
        public string lastSortName = "";

        public Sorting(Coordinates coords)
        {
            sortTypes = new Dictionary<int, Action>
            {
                {0, NonSort},
                {1, BFSort},
                {2, IntSort},
                {3, Sort3},
                {4, Sort4},
                {5, Sort5},
                {6, Sort6},
            };
            this.coords = coords;
            sortedX = new int[coords.X.Count];
            sortedY = new int[coords.X.Count];
        }

        public TimeSpan GetTime()
        {
            int x = sortedX[0];
            int y = sortedY[0];
            long ms = 0;
            for (int i = 0; i < sortedX.Length; i++)
            {
                int x1 = sortedX[i];
                int y1 = sortedY[i];
                //distance += Math.Sqrt(Math.Pow(x1 - x, 2) + Math.Pow(y1 - y, 2));
                int distance = Math.Max(Math.Abs(x - x1), Math.Abs(y - y1));
                ms += distance * 48 + 10;
                x = x1;
                y = y1;
            }
            int seconds = (int) ms / 1000;
            TimeSpan time = new TimeSpan(0, 0, seconds);
            return time;
        }

        public void NonSort()
        {
            sortedX = coords.X.ToArray();
            sortedY = coords.Y.ToArray();
            lastSortName = "без сортировки";
        }

        public void BFSort()
        {
            BruteForceSort sort = new BruteForceSort(coords);
            sort.Sort(sortedX, sortedY, ref unsortedCount);
            lastSortName = "перебор";
        }

        public void IntSort()
        {
            InterpolationSort sort = new InterpolationSort(coords);
            sort.Sort(sortedX, sortedY, ref unsortedCount);
            lastSortName = "бинарный поиск с приближением";
        }

        public void Sort3() { }
        public void Sort4() { }
        public void Sort5() { }
        public void Sort6() { }
    }

    internal class BruteForceSort
    {
        private HashSet<(int, int)> coordsSet;
        readonly (int, int)[] neighbors = new[]
            {(-1, 0), (-1, 1), (0, 1), (1, 1),
              (1, 0), (1, -1),(0, -1), (-1, -1)};
        private readonly int length;

        public BruteForceSort(Coordinates coords)
        {
            length = coords.length;
            coordsSet = new HashSet<(int, int)> { };
            for (int i = 0; i < length; i++)
                coordsSet.Add((coords.X[i], coords.Y[i]));
        }

        public void Sort(int[] sortedX, int[] sortedY, ref int unsortedCount)
        {
            (int, int) point = (0, 0);
            for (int i = 0; i < length; i++)
            {
                point = FindNearestPoint(point);
                (sortedX[i], sortedY[i]) = point;
                coordsSet.Remove(point);
                unsortedCount = coordsSet.Count;
            }
        }

        private (int, int) FindNearestPoint((int, int) point)
        {
            (int x, int y) = point;
            foreach ((int px, int py) in neighbors)
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
                int distance = dx * dx + dy * dy;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    point = (x1, y1);
                }
            }
            return point;
        }
    }

    internal class InterpolationSort
    {
        private int indX;
        private int indY;
        private int xVal;
        private int yVal;
        private int xSum = 0;
        private int ySum = 0;
        double massCenterX = 0;
        double massCenterY = 0;
        private readonly int length;
        private Coordinates coords;
        private List<int> colY;
        private Dictionary<int, List<int>> YAxisToXValues;
        private delegate bool Operation(out int indY1, out int yVal1, out int dy);

        public InterpolationSort(Coordinates coords)
        {
            this.coords = coords;
            length = coords.length;
            colY = coords.uniqY.ToList();
            YAxisToXValues = new Dictionary<int, List<int>> { };

            foreach (int y in colY)
                YAxisToXValues.Add(y, new List<int> { });
            
            for (int i = 0; i < length; i++)
                YAxisToXValues[coords.Y[i]].Add(coords.X[i]);

            xVal = 0;
            indY = 0;
            yVal = colY[0];
        }

        public void Sort(int[] sortedX, int[] sortedY, ref int unsortedCount)
        {
            unsortedCount = length;
            for (int i = 0; i < length; i++)
            {
                (indX, indY) = GetNearestCoord();
                yVal = colY[indY];
                List<int> rowX = YAxisToXValues[yVal];
                xVal = rowX[indX];
                sortedX[i] = xVal;
                sortedY[i] = yVal;
                rowX.RemoveAt(indX);
                if (rowX.Count == 0)
                {
                    colY.RemoveAt(indY);
                    YAxisToXValues.Remove(yVal);
                }
                unsortedCount--;

                xSum += xVal;
                ySum += xVal;
                massCenterX = xSum / (i+1);
                massCenterY = ySum / (i+1);
            }
        }

        private (int, int) GetNearestCoord()
        {
            // Ищет координаты ближайшей к (indX, indY) точки;
            // minX, minY индексы;
            int minX = 0;
            int minY = 0;

            int distMin = int.MaxValue;
            double radMin = double.PositiveInfinity;

            Operation Search = YSearch();
            while (Search(out int indY1, out int yVal1, out int dy))
            {
                if (distMin < dy * dy) break;
                List<int> rowX1 = YAxisToXValues[colY[indY1]];
                BinarySearch(rowX1, out int indX1, out int xVal1, out int dx);

                int distance = dx * dx + dy * dy;
                double dx1 = massCenterX - xVal1;
                double dy1 = massCenterY - yVal1;
                double radius = dx1 * dx1 + dy1 * dy1;

                if (distance > distMin)
                    continue;
                if (distance == distMin & radius > radMin)
                    continue;

                minY = indY1;
                minX = indX1;
                distMin = distance;
                radMin = radius;
            }
            return (minX, minY);
        }

        private Operation YSearch()
        {
            int up = indY;
            int down = indY - 1;

            bool Inner(out int indY1, out int yVal1, out int dy)
            {
                indY1 = 0;
                yVal1 = 0;
                dy = 0;

                if (down >= 0 & up < colY.Count)
                {
                    if (yVal - colY[down] < colY[up] - yVal)
                        indY1 = down--;
                    else
                        indY1 = up++;
                }
                else if (down >= 0) 
                    indY1 = down--;
                else if (up < colY.Count) 
                    indY1 = up++;
                else 
                    return false;

                yVal1 = colY[indY1];
                dy = yVal1 - yVal;
                return true;
            }
            return Inner;
        }
        
        private void BinarySearch(List<int> list, out int indX1, out int xVal1, out int dx)
        {
            // Поиск индекса ближайшего к xVal числа в списке list;
            // i, firs, right - индексы в списке list;

            int right = list.Count - 1;
            int left = 0;

            if (xVal <= list[left])
                indX1 = left;
            else if (xVal >= list[right])
                indX1 = right;
            else
            {
                while (right - left > 1)
                {
                    int i = left + 1 + (right - left - 2) * (xVal - list[left]) / (list[right] - list[left]);
                    if (xVal > list[i])
                        left = i;
                    else
                        right = i;
                }
                int dr = list[right] - xVal;
                int dl = xVal - list[left];
                if (dr == dl)
                {
                    if (Math.Abs(massCenterX - list[right]) < Math.Abs(massCenterX - list[left]))
                        indX1 = right;
                    else
                        indX1 = left;
                }
                else if (dr < dl)
                    indX1 = right;
                else
                    indX1 = left;
            }

            xVal1 = list[indX1];
            dx = xVal1 - xVal;
        }
    }

    internal class Coordinates
    {
        public List<int> X;
        public List<int> Y;
        public int width = 0;
        public int height = 0;
        public int length = 0;
        public HashSet<int> uniqY = new HashSet<int> { };
        public string filename;
        public Coordinates()
        {
        }

        public void GetCoordsFromFile()
        {
            byte[] imgBytes = Readlile();
            SetupCoodrds(imgBytes);
        }

        private byte[] Readlile()
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() != DialogResult.OK)
                throw new Exception("Файл не выбран");

            BinaryReader reader = new BinaryReader(File.Open(
                fileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            byte[] imgBytes = reader.ReadBytes((int)reader.BaseStream.Length);
            reader.Close();
            filename = Path.GetFileName(fileDialog.FileName);
            fileDialog.Dispose();

            if (imgBytes[0] != 0x42 || imgBytes[1] != 0x4D)
                throw new Exception("Файл не являяется mono bmp");
            return imgBytes;
        }

        private void SetupCoodrds(byte[] imgBytes)
        {
            uniqY.Clear();
            X = new List<int>();
            Y = new List<int>();

            width = BitConverter.ToInt32(imgBytes, 18);
            height = BitConverter.ToInt32(imgBytes, 22);
            int padding = (imgBytes.Length - 62) / height - width / 8;
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
                        X.Add(x);
                        Y.Add(y);
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
            length = X.Count;
        }
        public string GetFileInfo()
        {
             return $"Файл: {filename}\r\nРазмер {width} на {height}\r\nКоличество точек: {length}\r\n";
        }
    }
}