using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace IO
{
    class Coordinates
    {
        public List<int> X;
        public List<int> Y;
        public HashSet<int> uniqY;
        public int width;
        public int height;
        public int length;
        public string filename;
        
        public Coordinates(byte[] bmpBytes, string filename = "")
        {
            this.filename = filename;
            X = new List<int>();
            Y = new List<int>();
            uniqY = new HashSet<int> { };
            SetupCoodrds(bmpBytes);
        }

        private void SetupCoodrds(byte[] bmpBytes)
        {
            width = BitConverter.ToInt32(bmpBytes, 18);
            height = BitConverter.ToInt32(bmpBytes, 22);
            int padding = (bmpBytes.Length - 62) / height - width / 8;
            int byteInd = 62;
            int bitInd = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte bmpByte = bmpBytes[byteInd];
                    int mask = 128 >> bitInd;
                    if ((bmpByte & mask) == 0)
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

    class BMPReader
    {
        public static byte[] ReadFile(out string filename)
        {
            byte[] monoBMP;
            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                if (openFD.ShowDialog() != DialogResult.OK)
                    throw new Exception("Файл не выбран");

                BinaryReader reader = new BinaryReader(
                    File.Open( openFD.FileName, FileMode.Open,FileAccess.Read, FileShare.ReadWrite)
                );
                using (reader)
                {
                    monoBMP = reader.ReadBytes((int)reader.BaseStream.Length);
                }
                filename = Path.GetFileName(openFD.FileName);
            }
            if (monoBMP[0] != 0x42 || monoBMP[1] != 0x4D)
                throw new Exception("Файл не являяется mono bmp");
            return monoBMP;
        }
    }

    class SDFileWriter
    {
        public static byte[] MergeCoords(int[] X, int[] Y, int Kx, int Ky)
        {
            List<byte> byteList = new List<byte>();
            int secEnd = 16479 + X.Length / 64; // Результирующее число байт / 512
            byteList.AddRange(BitConverter.GetBytes(secEnd));
            byteList.AddRange(BitConverter.GetBytes(0));
            for (int i = 0; i < X.Length; i++)
            {
                byteList.AddRange(BitConverter.GetBytes(X[i] * Kx));
                byteList.AddRange(BitConverter.GetBytes(Y[i] * Ky));
            }
            int padding = (512 - byteList.Count % 512) % 512;
            byte[] zeroes = new byte[padding];
            byteList.AddRange(zeroes);

            return byteList.ToArray();
        }

        public static bool WriteToDrive(byte[] buffer, string driveName)
        {
            SafeFileHandle fileHandle = CreateFile(
                @"\\.\" + driveName,
                FileAccess.Write,
                FileShare.None,
                IntPtr.Zero,
                FileMode.Create,
                FileAttributes.Normal,
                IntPtr.Zero
            );

            using (fileHandle)
            {
                if (fileHandle.IsInvalid)
                    return false;

                return WriteFile(
                    fileHandle,
                    buffer,
                    (uint)buffer.Length,
                    out uint bytesWritten,
                    IntPtr.Zero
                );
            }
        }
       
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteFile(
            SafeHandle hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            FileAccess dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            FileMode dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );
    }
}
