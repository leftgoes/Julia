using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;


namespace Buddhabrot
{
    class z2c
    {   
        public int w;
        public int p;

        public int k;
        public int n;
        public int threads;
        private float elapsed = 0;

        double[] real = new double[2] {-2.0, 2.0};
        double[] imag = new double[2] {-2.0, 2.0};
        double[,] arr;

        private int h {get => Convert.ToInt32(w * Math.Abs((imag[1] - imag[0])/(real[1] - real[0])));}

        public z2c(int _w, int _k, int _n, int _p, int _threads = 1)
        {
            w = _w;
            p = _p;
            threads = _threads;
            if (_n > _k) {k = _k; n = _n;} else {k = _n; n = _k;}
        }

        private static double[] f(double[] _z, double[] _c)
        {
            return new double[2] {_z[0]*_z[0] - _z[1]*_z[1] + _c[0], 2 * _z[0] * _z[1] + _c[1]};
        }
		
		private static bool InView(int[] pos, int _w, int _h)
		{
			return 0 <= pos[0] && pos[0] < _w && 0 <= pos[1] && pos[1] < _h;
		}
		
        private double[,] CalculateThread(double[,] ComplexNumbers, int t)
        {   
            int _h = h;
            double[,] _arr = new double[_h, w];

            int _len = ComplexNumbers.GetLength(0);
            int _every = Convert.ToInt32(_len/(threads * 100));

            DateTime _start = DateTime.Now;
            float _elapsed;

            for (int j = t; j < _len; j += threads)
            {
                double[] z = new double[2] {0.0, 0.0};
                double[] c = new double[2] {ComplexNumbers[j, 0], ComplexNumbers[j, 1]};

                List<int> PointsX = new List<int>();
                List<int> PointsY = new List<int>();

                for (int i = 0; i <= n; i++)
                {
                    z = f(z, c);
                    int[] pos = ComplexToXY(z, real, imag, w, _h);
                    if (InView(pos, w, _h)) {
                        PointsX.Add(pos[0]);
                        PointsY.Add(pos[1]);
                    } else {
                        if (k < i && i < n) {
                            for (int u = 0; u < PointsX.Count; u++)
                            {
                                _arr[PointsY[u], PointsX[u]] += 1;
                            }
                        }
                        break;
                    }
                }

                if (t == 0 && j % _every == 0) {
                    j++;
                    _elapsed = DateTime.Now.Subtract(_start).Ticks/10000000;
                    Console.Write($"\r[INFO] calculate | {100 * j/_len}% | {_elapsed}s | {Convert.ToInt32(_elapsed/j * (_len - j))}s            \r");
                }
            }
            Console.WriteLine("[INFO] calculate | 100% | finished");
            return _arr;
        }

        public void Calculate()
        {
            long ticks = DateTime.Now.Ticks;
            double[,] numbers = new double[p, 2];
            int _every = Convert.ToInt32(p/100);
			
			Console.WriteLine($"image = {w}x{h}, k = {k}, n = {n}, {threads} threads, {p}/{w * h} points");
			
            for (int i = 0; i < p; i++)
            {   
                double[] z;
                
                do {
                z = new double[2] {RandomDouble(real), RandomDouble(imag)};
                } while (p_1(z) || p_2(z) || p_3(z) || p_4_2(z));

                numbers[i, 0] = z[0];
                numbers[i, 1] = z[1];

                if (i % _every == 0) Console.Write($"\r[INFO] get_complex | {100.0 * i/p}%\r");
            }
            Console.WriteLine("[INFO] get_complex | 100% | finished");
            arr = CalculateThread(numbers, 0);
            elapsed = (DateTime.Now.Ticks - ticks)/10000000;
        }

        public void Stretch(double percentile = 3)
        {
            if (arr == null) return;
            double[,] _arr = Normalize2DArray(arr);
            
            double[] flattened = To1DArray<double>(_arr);
            Array.Sort(flattened);
            double median = flattened[Convert.ToInt32(flattened.Length * (100 - percentile)/100)];

            if (median == 0.0) 
            {
                Console.WriteLine($"[INFO] cannot stretch with: percentile = {percentile} (array might be too dark)");
                return;
            } else if (median == 0.5) return;

            Func<double, double> _f;
            double a;
            if (median < 0.5) {
                _f = Math.Asinh;
                a = Math.Sqrt(1 - 4 * median*median)/(2 * median*median);
            } else {
                _f = Math.Sinh;
                a = Math.Sqrt(1 - 4 * (1 - median)*(1 - median))/(2 * (1 - median)*(1 - median));
            }

            double f_a = _f(a);
            for (int i = 0; i < _arr.GetLength(0); i++) {
                for (int j = 0; j < _arr.GetLength(1); j++) {
                    double x = _arr[i, j];
                    if (x == 0) continue;
                    _arr[i, j] = _f(a * x)/f_a;
                }
            }

            arr = _arr;
        }

        public void SavePoints(double[,] _arr2d, string file)
        {
            int _h = h;
            double maxValue = _arr2d.Cast<double>().Max();

            Bitmap bitmap = new Bitmap(w, _h);
            for (int x = 0; x < w; x++) {
                for (int y = 0; y < _h; y++) {
                    int value = Convert.ToInt32(255.0 * _arr2d[y, x]/maxValue);
                    bitmap.SetPixel(x, y, Color.FromArgb(255, value, value, value));
                }
            }
            bitmap.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            Console.WriteLine($"saved points to '{file}'");
            
        }

        static Color Gray(int value)
        {
            return Color.FromArgb(value, value, value);
        }

        private void SaveToText(string path = "")
        {
            double maxValue = arr.Cast<double>().Max();
            string[] lines = new string[h];
            string pixel;
            
            using StreamWriter file = new(path);
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    pixel = Convert.ToString(Convert.ToInt32(LinMap(arr[y, x], 0, maxValue, 0, 65535)));
                    file.Write(pixel);
                    if (x != w - 1) file.Write(",");
                }
                file.Write("\n");
            }
        }

        public void Save(string path = "")
        {
            if (path == "") path = $"Buddhabrot; img({w}, {h}); k{k}; n{n}; th{threads}; t{elapsed}; p{p}.png";
            Console.WriteLine("[INFO] saving to .buddha");
            SaveToText("__temp__.buddha");

            var psi = new ProcessStartInfo();
            psi.FileName = @"C:\Program Files\Python39\python.exe";

            psi.Arguments = $"\"buddha2img.py\" \"{path}\"";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            using (Process process = Process.Start(psi))
            {
                string errors = process.StandardError.ReadToEnd();
                Console.WriteLine(errors == "" ? $"[INFO] saved to '{path}'" : $"[INFO] an error occured in buddha2img.py:\n{errors}");
            }
        }

        static byte[] ToBytes<T>(T[,] array) where T : struct
        {
        var buffer = new byte[array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))];
        Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
        return buffer;
        }
        
        public static void testPeriods(int _w = 3000, string file = "Test.png")
        {
            Bitmap bitmap = new Bitmap(_w, _w);
            for (int x = 0; x < _w; x++) {
                for (int y = 0; y < _w; y++) {
                    double[] z = new double[2] {LinMap(x, 0, _w, -2, 2), LinMap(y, 0, _w, -2, 2)};
                    if (p_1(z) || p_2(z) || p_3(z) || p_4_2(z)) {
                        bitmap.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                    } else {
                        bitmap.SetPixel(x, y, Color.FromArgb(255, 255, 255, 255));
                    }
                }
            }
            bitmap.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            Console.Write("saved");
        }
        
        static bool p_1(double[] _z) // https://en.wikipedia.org/wiki/Cardioid#Equations
        {
            double phi = Math.Atan2(_z[1], _z[0] - 0.25);
            return ComplexAbs(_z[0] - 0.25, _z[1]) < 0.5 * (1 - Math.Cos(phi));
        }

        static bool p_2(double[] _z) {return ComplexAbs(_z[0] + 1, _z[1]) < 0.25;}

        static bool p_3(double[] _z) {return ComplexAbs(_z[0] + 0.125, Math.Abs(_z[1]) - 0.7432) < 0.094;}

        static bool p_4_2(double[] _z) {return ComplexAbs(_z[0] + 1.309, _z[1]) < 0.058;}
        
        static double ComplexAbs(double _re, double _im)
        {
            return Math.Sqrt(_re*_re + _im*_im);
        }

        static double ComplexAbs(double[] _z)
        {
            return Math.Sqrt(_z[0]*_z[0] + _z[1]*_z[1]);
        }

        static int[] ComplexToXY(double[] _z, double[] _real, double[] _imag, int _w, int _h)
        {
            return new int[2] {Convert.ToInt32(LinMap(_z[0], _real[0], _real[1], 0, _w)), Convert.ToInt32(LinMap(_z[1], _imag[0], _imag[1], 0, _h))};
        }

        static double LinMap(double value, double from1, double from2, int to_1, int to_2)
        {
            double to1 = Convert.ToDouble(to_1);
            double to2 = Convert.ToDouble(to_2);
            return to1 + (value - from1) * (to2 - to1) / (from2 - from1);
        }

        static double RandomDouble(double[] range)
        { 
            Random random = new Random();
            return random.NextDouble() * (range[1] - range[0]) + range[0];
        }

        static T[] To1DArray<T>(T[,] input) // https://www.dotnetperls.com/flatten-array
        {
            int size = input.Length;
            T[] result = new T[size];
            
            int write = 0;
            for (int i = 0; i <= input.GetUpperBound(0); i++)
            {
                for (int z = 0; z <= input.GetUpperBound(1); z++)
                {
                    result[write++] = input[i, z];
                }
            }
            return result;
        }

        static double[,] Normalize2DArray(double[,] _arr)
        {
            double minValue = _arr.Cast<double>().Min();
            double maxValue = _arr.Cast<double>().Max();
            for (int i = 0; i < _arr.GetLength(0); i++)
            {
                for (int j = 0; j < _arr.GetLength(1); j++)
                {
                    _arr[i, j] = LinMap(_arr[i, j], minValue, maxValue, 0, 1);
                }
            }
            return _arr;
        }

    }

    class Program
    {
        static void Main()
        {   
            z2c buddhabrot = new z2c(1000, 0, 1000, 1000000);
            buddhabrot.Calculate();
            buddhabrot.Stretch(0.2);
            buddhabrot.Save();
        }
    }
}
