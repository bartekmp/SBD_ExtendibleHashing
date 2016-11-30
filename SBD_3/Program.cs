using System;
using System.IO;

namespace SBD_3
{
    internal static class Program
    {
        public static long Reads, Writes;
        private static readonly Random Random = new Random();

        private static void Main()
        {
            Directory dir = null;
            string readLine;
            while ((readLine = Console.ReadLine()) != null)
            {
                string[] s = readLine.Split(new[] {' '});
                switch (s[0])
                {
                    case "N":
                        if (dir == null)
                        {
                            if (File.Exists(s[1])) File.Delete(s[1]);
                            if (File.Exists(s[2])) File.Delete(s[2]);
                            dir = new Directory(s[1], int.Parse(s[2]));
                        }
                        break;

                    case "A":
                        if (dir != null)
                        {
                            switch (s.Length)
                            {
                                case 2:
                                    int[] co = GenerateCoefficients();
                                    dir.Add(new Record(int.Parse(s[1]), co[0], co[1], co[2]));
                                    break;
                                case 5:
                                    dir.Add(new Record(int.Parse(s[1]), int.Parse(s[2]), int.Parse(s[3]),
                                        int.Parse(s[4])));
                                    break;
                            }
                        }
                        break;

                    case "U":
                        if (dir != null)
                        {
                            try
                            {
                                dir.Update(new Record(int.Parse(s[1]), int.Parse(s[2]), int.Parse(s[3]), int.Parse(s[4])));
                            }
                            catch (RecordNotFoundException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        break;

                    case "R":
                        if (dir != null)
                        {
                            try
                            {
                                dir.Remove(int.Parse(s[1]));
                            }
                            catch (RecordNotFoundException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        break;

                    case "G":
                        if (dir != null)
                        {
                            try
                            {
                                Record record = dir.Get(int.Parse(s[1]));
                                Console.WriteLine(record.ToString());
                            }
                            catch (RecordNotFoundException ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                        break;

                    case "PD":
                        if (dir != null)
                            dir.PrintDirectory();
                        break;

                    case "PF":
                        if (dir != null)
                            dir.PrintFile();
                        break;

                    case "S":
                        Console.WriteLine("--------------");
                        Console.WriteLine("R: {0}, W: {1}", Reads, Writes);
                        break;

                    case "TEST":
                        break;

                    default:
                        Console.WriteLine("Bad command, try again!");
                        break;
                }
            }
            if (dir != null)
            {
                dir.Dispose();
            }
        }

        private static int[] GenerateCoefficients()
        {
            var w = new[] {Random.Next(-10000, 10000), Random.Next(-10000, 10000), Random.Next(-10000, 10000)};
            while (w[1]*w[1] - 4*w[0]*w[2] <= 1e-15)
            {
                w[0] = Random.Next(-10000, 10000);
                w[1] = Random.Next(-10000, 10000);
                w[2] = Random.Next(-10000, 10000);
            }
            return w;
        }
    }
}