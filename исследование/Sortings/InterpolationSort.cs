using System;
using System.Collections.Generic;
using System.Linq;
using IO;

namespace sortings
{
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
                massCenterX = xSum / (i + 1);
                massCenterY = ySum / (i + 1);
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
}
