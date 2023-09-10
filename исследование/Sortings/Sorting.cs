using System;
using System.Collections.Generic;
using IO;

namespace sortings
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
}