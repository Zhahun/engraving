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
            Dictionary <int, List<int>> YAxisToXValues = new Dictionary <int, List<int>> {};
            // y - значение координаты;
            foreach (int y in uniqY)
            {
                YAxisToXValues.Add(y, new List<int>{});
            }
            for (int i=0; i<coords.length; i++)
            {
                YAxisToXValues[coords.imgY[i]].Add(coords.imgX[i]);
            }

            // indX, indY индексы YAxisToXValues[Val], YAxisToXValues;
            int indX = 0;
            int indY = 0;
            int yVal = uniqY[0];

            for (int i=0; i< coords.length; i++)
            {
                (indX, indY) = GetNearestCoord(indX, indY, yVal, uniqY, YAxisToXValues);
                yVal = uniqY[indY];
                List <int> rowX = YAxisToXValues[yVal];
                sortedX[i] = rowX[indX];
                sortedY[i] = yVal;
                rowX.RemoveAt(indX);
                if (rowX.Count == 0)
                {
                    YAxisToXValues.Remove(uniqY[indY]);
                    uniqY.RemoveAt(indY);
                }
                    
            }   
        }

        public (int, int) GetNearestCoord(int indX, int indY, int yVal, List<int> uniqY,
                                          Dictionary<int, List<int>> YAxisToXValues)
        {
            // Ищет координаты ближайшей к (indX, indY) точки;
            int minDistance = int.MaxValue;
            // minX, minY индексы YAxisToXValues[Val], YAxisToXValues;
            int minX = 0;
            int minY = 0;

            foreach ((int y, int dy) in YSearch(uniqY, indY, yVal))
            {
                List<int> rowX = YAxisToXValues[uniqY[y]];
                (int x, int dx) = BinarySearch(rowX, indX);

                int distance = dx * dx + dy * dy;

                if (dx == 0 || minDistance < dy * dy)
                {
                    minY = y;
                    minX = x;
                    break;
                }
                if (distance < minDistance) 
                {   
                    minY = y;
                    minX = x;
                    minDistance = distance;
                }
            }
            return (minX, minY);
        }


        public IEnumerable<(int, int)> YSearch(List<int> uniqY, int yIndex, int yVal)
        {
            // Генератор, возвращающий индекы чисел в списке uniqY по возрастанию расстояния к uniqY[yIndex];
            // yIndex, up, down - индексы 
            int up = yIndex;
            int down = yIndex - 1;
            int dyUp, dyDown;

            while (true)
			{
                if (down > 0 & up < uniqY.Count)
                {
                    dyUp = uniqY[up] - yVal;
                    dyDown = yVal - uniqY[down];
                    if (dyDown < dyUp)
                        yield return (down--, dyDown);
                    else
                        yield return (up++, dyUp);
                }
                else if (down > 0)
                {
                    dyDown = yVal - uniqY[down];
                    yield return (down--, dyDown);
                }
                else if (up < uniqY.Count)
                {
                    dyUp = uniqY[up] - yVal;
                    yield return (up++, dyUp);
                }
                else break;
			}
        }
       
        public (int, int) BinarySearch(List<int> list, int value)
        {
            // Поиск индекса ближайшего к value числа в списке list;
            // i, firs, last - индексы в списке list;
            if (list.Count == 1)
                return (0, value - list[0]);

            int last = list.Count - 1;
            int first = 0;
            int i = value * last / (list[last] - list[first]);

            while (first - last > 1) 
            {
                if (value > list[i])
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

            int df = value - list[first];
            int dl = list[last] - value;

            if (df < dl)
                return (first, df);
            else 
                return (last, dl);
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
            uniqY.Clear();
            imgX = new List<int>();
            imgY = new List<int>();
            
            int width = BitConverter.ToInt32(imgBytes, 18);
            int height = BitConverter.ToInt32(imgBytes, 22);
            int padding = (4 - width % 32 / 8) % 4; //дополнение байт до 4
            int byteInd = 62;
            int bitInd = 0;
            int last_y = -1;
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
                        {
                            uniqY.Add(y);
                            last_y= y;
                        }
                            
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