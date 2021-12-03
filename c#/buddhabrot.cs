using System;
using System.Collections.Generic;
using System.Drawing;


namespace Buddhabrot
{
    class Calculate
    {   
        public int w;
        public int p;

        public int k;
        public int n;
        public int threads = 1;
        double[] real = new double[2] {-2.0, 2.0};
        double[] imag = new double[2] {-2.0, 2.0};

        double[,] arr;

        public Calculate(int _w, int _k, int _n, int _p, int _threads = 8)
        {
            w = _w;
            k = _k;
            n = _n;
            p = _p;
            threads = _threads;
        }

        private static double[] f(double[] z, double[] c)
        {
            return new double[2] {z[0]*z[0] - z[1]*z[1] + c[0], 2 * z[0] * z[1] + c[1]};
        }

        private int h(int _w, double[] _real, double[] _imag)
        {
            return Convert.ToInt32(_w * Math.Abs((_imag[1] - _imag[0])/(_real[1] - _real[0])));
        }

        private double[,] thread(double[,] ComplexNumbers, int t)
        {   
            int _h = h(w, real, imag);
            double[,] _arr = new double[_h, w];

            for (int i = 0; i < ComplexNumbers.GetLength(0); i++)
            {
                // Console.WriteLine("i = " + Convert.ToString(i));
                double[] z = new double[2] {0.0, 0.0};
                int _i = i * threads + t;
                if (_i >= ComplexNumbers.GetLength(0)) {
                    continue;
                }
                double[] c = new double[2] {ComplexNumbers[_i, 0], ComplexNumbers[_i, 1]};
                
                List<int> PointsX = new List<int>();
                List<int> PointsY = new List<int>();

                for (int j = 0; j <= n; j++)
                {
                    // Console.WriteLine("j = " + Convert.ToString(j) + "; " + Convert.ToString(z[0]) + ", " + Convert.ToString(z[1]));
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
            return _arr;
        }

        public void calculate()
        {
            double[,] numbers = new double[p, 2];
            
            for (int i = 0; i < p; i++)
            {   
                double[] z = new double[2];
                do
                {
                    z = new double[2] {RandomDouble(real), RandomDouble(imag)};
                } while(p_1(z) || p_2(z));
                numbers[i, 0] = z[0];
                numbers[i, 1] = z[1];
            }
            Console.WriteLine("GotNumbers");
            arr = thread(numbers, 0);
            save(arr);
            Console.WriteLine("Finished");
        }

        public void save(int[,] integers)
        {
            int _h = h(w, real, imag);
            int maxValue = integers.Cast<int>().Max();
            Bitmap bitmap = new Bitmap(w, _h);
            for (int x = 0; x < w; x++) {
                for (int y = 0; y < _h; y++) {
                    int value = Convert.ToInt32(255 * integers[y, x]/maxValue);
                    bitmap.SetPixel(x, y, Color.FromArgb(255, value, value, value));
                }
            }
            bitmap.Save("C:\\Python\\Julia.png", System.Drawing.Imaging.ImageFormat.Png);

            
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

    }

    class Program
    {
        static void Main(string[] args)
        {   
            Calculate calculate = new Calculate(600, 200, 500, 300);
            calculate.calculate();
        } 
    }
}
