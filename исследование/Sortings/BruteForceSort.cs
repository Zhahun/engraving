using System;
using System.Collections.Generic;
using IO;

namespace sortings
{
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
}