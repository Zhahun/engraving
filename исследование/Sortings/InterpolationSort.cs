using System;
using System.Collections.Generic;
using System.Linq;
using IO;

namespace sortings
{
    internal class InterpolationSort
    {
        private int xVal;
        private int yVal;
        private double refX;
        private double refY;
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
        }

        public void Sort(int[] sortedX, int[] sortedY, ref int unsortedCount)
        {
            xVal = 0;
            yVal = colY[0];
            int indY = 0;
            int xSum = 0;
            int ySum = 0;
            int k = 1;

            unsortedCount = length;
            for (int i = 0; i < length; i++)
            {
                xSum += xVal;
                ySum += xVal;
                refX = (xSum / (i + k) * 0.1 + 0.9 * xVal);
                refY = (ySum / (i + k) * 0.1 + 0.9 * yVal);
                GetNearestCoord(out int indX, out indY, out double distance, indY);
                if (distance > 3)
                {
                    xSum = 0;
                    ySum = 0;
                    k = -i;
                }

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
            }
        }
        private void GetNearestCoord(out int indMinX, out int indMinY, out double distMin, int indY)
        {
            indMinX = 0;
            indMinY = 0;
            distMin = double.PositiveInfinity;
            foreach (int indY1 in YSearch(indY))
            {
                int y = colY[indY1];
                List<int> rowX1 = YAxisToXValues[y];
                foreach (int indX1 in BinarySearch(rowX1, (int)refX))
                {
                    int x = rowX1[indX1];
                    double dx = refX - x;
                    double dy = refY - y;
                    double distance = dx * dx + dy * dy;
                    if (distance < distMin)
                    {
                        indMinX = indX1;
                        indMinY = indY1;
                        distMin = distance;
                    }
                    if (distMin < dy * dy)
                    {
                        return;
                    }
                }             
            }
        }

        private IEnumerable<int> YSearch(int indY)
        {
            int i;
            int up = indY;
            int down = indY - 1;
            while (true)
            {
                if (down >= 0 & up < colY.Count)
                {
                    if (yVal - colY[down] < colY[up] - yVal)
                        i = down--;
                    else
                        i = up++;
                }
                else if (down >= 0)
                    i = down--;
                else if (up < colY.Count)
                    i = up++;
                else break;
                yield return i;
            }
        }

        private int[] BinarySearch(List<int> list, int x)
        {
            int right = list.Count - 1;
            int left = 0;

            if (x <= list[left])
                right = left;
            else if (x >= list[right])
                left = right;

            while (right - left > 1)
            {
                int i = left + 1 + (right - left - 2) * (x - list[left]) / (list[right] - list[left]);
                if (x > list[i])
                    left = i;
                else
                    right = i;
            }
            return new int[] {left, right};
        }
    }
}
