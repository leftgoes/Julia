using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


namespace Buddhabrot
{
    class Buddhabrot
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

        private int h {get {return Convert.ToInt32(w * Math.Abs((imag[1] - imag[0])/(real[1] - real[0])));}}

        public Buddhabrot(int _w, int _k, int _n, int _p, int _threads = 1)
        {
            w = _w;
            p = _p;
            threads = _threads;
            if (_n > _k) {k = _k; n = _n;} else {k = _n; n = _k;}
        }

        private static double[] f(double[] z, double[] c)
        {
            return new double[2] {z[0]*z[0] - z[1]*z[1] + c[0], 2 * z[0] * z[1] + c[1]};
        }

        private double[,] thread(double[,] ComplexNumbers, int t)
        {   
            int _h = h;
            double[,] _arr = new double[_h, w];
            int _len = ComplexNumbers.GetLength(0);
            int _every = Convert.ToInt32(_len/(threads * 100));

            for (int i = 0; i < _len; i += threads)
            {
                if (i % _every == 0) {
                    Console.Write($"\r[INFO] calculate | {Convert.ToInt32(100 * i/_len)}%\r");
                }
                
                double[] z = new double[2] {0.0, 0.0};
                int _i = i + t;
                if (_i >= _len) {
                    continue;
                }
                double[] c = new double[2] {ComplexNumbers[_i, 0], ComplexNumbers[_i, 1]};
                
                List<int> PointsX = new List<int>();
                List<int> PointsY = new List<int>();

                for (int j = 0; j <= n; j++)
                {
                    z = f(z, c);
                    int[] pos = ComplexToXY(z, real, imag, w, _h);

                    if (0 <= pos[0] && pos[0] < w && 0 <= pos[1] && pos[1] < _h) {
                        PointsX.Add(pos[0]);
                        PointsY.Add(pos[1]);
                    } else {
                        if (k < j && j < n) {
                            for (int l = 0; l < PointsX.Count; l++)
                            {
                                _arr[PointsY[l], PointsX[l]] += 1;
                            }
                        }
                        break;
                    }
                }
            }
            Console.WriteLine("[INFO] calculate | 100% | finished");
            return _arr;
        }

        public void calculate()
        {
            long ticks = DateTime.Now.Ticks;
            double[,] numbers = new double[p, 2];
            int _every = Convert.ToInt32(p/100);
            for (int i = 0; i < p; i++)
            {   
                double[] z = new double[2];
                do
                {
                    z = new double[2] {RandomDouble(real), RandomDouble(imag)};
                } while (p_1(z) || p_2(z));
                numbers[i, 0] = z[0];
                numbers[i, 1] = z[1];
                if (i % _every == 0) {
                    Console.Write($"\r[INFO] get_complex | {Convert.ToInt32(100.0 * i/p)}%\r");
                }
                
            }
            Console.WriteLine("[INFO] get_complex | 100% | finished");
            arr = thread(numbers, 0);
            elapsed = (DateTime.Now.Ticks - ticks)/10000000;
        }

        public void stretch(double percentile = 3)
        {
            if (arr == null) return;
            double[,] _arr = Normalize2DArray(arr);
            
            double[] flattened = To1DArray(_arr);
            Array.Sort(flattened);
            double median = flattened[Convert.ToInt32(flattened.Length * (100 - percentile)/100)];

            if (median == 0.0) 
            {
                Console.WriteLine($"[INFO] cannot stretch with: percentile = {percentile} (array might be too dark)");
                return;
            } if (median == 0.5) return;

            Func<double, double> f;
            double a;
            if (median < 0.5) {
                f = Math.Asinh;
                a = Math.Sqrt(1 - 4 * median*median)/(2 * median*median);
            } else {
                f = Math.Sinh;
                a = Math.Sqrt(1 - 4 * (1 - median)*(1 - median))/(2 * (1 - median)*(1 - median));
            }

            double f_a = f(a);
            for (int i = 0; i < _arr.GetLength(0); i++) {
                for (int j = 0; j < _arr.GetLength(1); j++) {
                    double x = _arr[i, j];
                    if (x == 0) continue;
                    _arr[i, j] = f(a * x)/f_a;
                }
            }

            arr = _arr;
        }

        public void save(string file = "")
        {
            int _h = h;
            if (file == "") file = $"Buddhabrot; img({w}, {h}); k{k}; n{n}; th{threads}; t{elapsed}; p{p}.png";
            double maxValue = arr.Cast<double>().Max();
            Console.WriteLine(maxValue);
            Bitmap bitmap = new Bitmap(w, _h);
            for (int x = 0; x < w; x++) {
                for (int y = 0; y < _h; y++) {
                    int value = Convert.ToInt32(255.0 * arr[y, x]/maxValue);
                    bitmap.SetPixel(x, y, Color.FromArgb(255, value, value, value));
                }
            }
            bitmap.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            Console.WriteLine($"saved to '{file}'");
            
        }

        static bool p_1(double[] z)
        {
            double abs_z = ComplexAbs(z);
            z[0] -= 0.25;
            double a = 2.0 * ComplexAbs(z);
            double u = z[0]/a;
            double v = z[1]/a;

            return abs_z < ComplexAbs(new double[2] {u - u*u + v*v, v - 2*u*v});
        }

        static bool p_2(double[] z)
        {
            z[0] += 1;
            return ComplexAbs(z) < 0.25;
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

        static double[] To1DArray(double[,] input) // https://www.dotnetperls.com/flatten-array
        {
            int size = input.Length;
            double[] result = new double[size];
            
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
            Buddhabrot buddhabrot = new Buddhabrot(1200, 0, 1000, 100000000);
            buddhabrot.calculate();
            buddhabrot.stretch();
            buddhabrot.save();
        } 
    }
}
