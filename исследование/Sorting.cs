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

        public long GetDistance()
        {
            int x = sortedX[0];
            int y = sortedY[0];
            double distance = 0;

            for (int i = 0; i < sortedX.Length; i++)
            {
                int x1 = sortedX[i];
                int y1 = sortedY[i];
                distance += Math.Max(Math.Abs(x - x1), Math.Abs(y - y1));
                //distance += Math.Sqrt(Math.Pow(x1 - x, 2) + Math.Pow(y1 - y, 2));
                x = x1;
                y = y1;
            }
            return (long)distance;
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
        private readonly int length;
        private List<int> colY;
        private Dictionary<int, List<int>> YAxisToXValues;
        private bool priorityOX = true;
        private bool priorityUp = true;
        private bool priorityLeft = true;


        public InterpolationSort(Coordinates coords)
        {
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
                int y = colY[indY];
                List<int> rowX = YAxisToXValues[y];
                int x = rowX[indX];
                SetPriority(x, y);
                xVal = x;
                yVal = y;
                sortedX[i] = xVal;
                sortedY[i] = yVal;
                rowX.RemoveAt(indX);
                if (rowX.Count == 0)
                {
                    colY.RemoveAt(indY);
                    YAxisToXValues.Remove(yVal);
                }
                unsortedCount--;
            }
        }
        private void SetPriority(int x, int y)
        {
            if (y - yVal < 0)
            {
                priorityLeft = true;
                 priorityOX = false;
            }
            priorityLeft = (y - yVal > 0) ? true : false;
            if (x == xVal)
                priorityOX = true;
            else
            {
                priorityOX = false;
                priorityUp = (x - xVal > 0) ? true : false;
            }
        }

        private (int, int) GetNearestCoord()
        {
            // Ищет координаты ближайшей к (indX, indY) точки;
            // minX, minY индексы;
            int minX = 0;
            int minY = 0;
            int minDistance = int.MaxValue;

            foreach ((int indY1, int dy) in YSearch())
            {
                List<int> rowX1 = YAxisToXValues[colY[indY1]];
                (int indX1, int dx) = BinarySearch(rowX1);

                int distance = dx * dx + dy * dy;
                
                if (distance <= minDistance)
                {
                    minY = indY1;
                    minX = indX1;
                    minDistance = distance;
                }

                if (dy == 0) 
                { 
                    if (priorityOX) // Приоритет оси X
                    { 
                        if (dx == -1 & priorityLeft) break;
                        if (dx == 1 & !priorityLeft) break;
                    }
                }  
                else if(Math.Abs(dx) < 2) break;
                if (minDistance < dy * dy) break;
            }      
            return (minX, minY);
        }

        private IEnumerable<(int, int)> YSearch()
        {
            // Генератор, возвращающий индекы чисел в списке uniqY по возрастанию расстояния к uniqY[yIndex];
            // yIndex, up, down - индексы 
            int up = indY;
            int down = indY - 1;
            int dyUp, dyDown;

            while (true)
            {
                if (down >= 0 & up < colY.Count)
                {
                    dyUp = colY[up] - yVal;
                    dyDown = yVal - colY[down];
                    if (dyDown == dyUp)
                    {
                        if (priorityUp)
                        {
                            yield return (up, dyUp);
                            up++;
                        }
                        else
                        {
                            yield return (down, dyDown);
                            down--;
                        }
                    }
                    else if (dyDown < dyUp)
                    {
                        yield return (down, dyDown);
                        down--;
                    } 
                    else
                    {
                        yield return (up, dyUp);
                        up++;
                    }  
                }
                else if (down >= 0)
                {
                    dyDown = yVal - colY[down];
                    yield return (down, dyDown);
                    down--;
                }
                else if (up < colY.Count)
                {
                    dyUp = colY[up] - yVal;
                    yield return (up, dyUp);
                    up++;
                }
                else break;
            }
        }

        private (int, int) BinarySearch(List<int> list)
        {
            // Поиск индекса ближайшего к xVal числа в списке list;
            // i, firs, right - индексы в списке list;

            int right = list.Count - 1;
            int left = 0;

            if (xVal <= list[left])
                return (left, list[left] - xVal);

            if (xVal >= list[right])
                return (right,list[right] -  xVal);

            while (right - left > 1)
            {
                int i = left + 1 + (right - left - 2) * (xVal - list[left]) / (list[right] - list[left]);
                
                if (xVal > list[i]) 
                    left = i;
                else 
                    right = i;  
            }

            int dl = list[left] - xVal;
            int dr = list[right] - xVal;

            if (-dl == dr)
            { 
                if (priorityLeft)
                    return (left, dl);
                else
                    return (right, dr);
            }
                 
            if (-dl < dr)
                return (left, dl);
            else
                return (right, dr);
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
    }
}