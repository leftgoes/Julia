using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;


namespace Buddhabrot
{   
    class Buddhabrot
    {   
        const string PATH = "C:\\Program Files\\Python39\\python.exe";
        const string DATA2IMG = "C:\\Coding\\Julia\\common\\data2img.py";

        public int w;
        public int p;

        public int k;
        public int n;
        public int threads;
        private float elapsed = 0;

        readonly double[] real = new double[2] {-3.5, 3.0};
        readonly double[] imag = new double[2] {-2.0, 2.0};
        double[,] arr;

        private int h {get => Convert.ToInt32(this.w * Math.Abs((this.imag[1] - this.imag[0])/(this.real[1] - this.real[0])));}

        public Buddhabrot(int w, int k, int n, int p = 0, int threads = 8)
        {
            this.w = w;
            this.p = p == 0 ? this.w * this.h / threads : p / threads;
            this.threads = threads;
            if (n > k) {this.k = k; this.n = n;} else {this.k = n; this.n = k;}
        }

        public virtual double[] f(double[] z, double[] c)
        {
            return new double[2] {0.0, 0.0};
        }

        public virtual bool Periods(double[] z)
        {
            return false;
        }

		private static bool InView(int[] pos, int w, int h)
		{
			return 0 <= pos[0] && pos[0] < w && 0 <= pos[1] && pos[1] < h;
		}

        private double[,] GetNumbers(int t)
        {
            double[,] numbers = new double[this.p, 2];
            int every = Convert.ToInt32(this.p/100);
            if (t == 0) Console.WriteLine($"image = {this.w}x{this.h}, k = {this.k}, n = {this.n}, {this.threads} threads, {this.threads * this.p}/{this.w * this.h} points");
            for (int i = 0; i < numbers.GetLength(0); i++)
            {   
                double[] z;
                
                do {
                z = new double[2] {Buddhabrot.RandomDouble(this.real), Buddhabrot.RandomDouble(this.imag)};
                } while (this.Periods(z));

                numbers[i, 0] = z[0];
                numbers[i, 1] = z[1];

                if (t == 0 && i % every == 0) Console.Write($"\r[INFO] get_complex | {100.0 * i/this.p}%\r");
            }
            if (t == 0) Console.WriteLine("[INFO] get_complex | 100% | finished");
            return numbers;
        }

        private double[,] CalculateThread(double[,] ComplexNumbers, int t)
        {   
            int h = this.h;
            double[,] array = new double[h, this.w];

            int len = ComplexNumbers.GetLength(0);
            int every = Convert.ToInt32(len/(this.threads * 1000));

            DateTime _start = DateTime.Now;
            float elapsed;

            for (int j = t; j < len; j += this.threads)
            {
                double[] z = new double[2] {0.0, 0.0};
                double[] c = new double[2] {ComplexNumbers[j, 0], ComplexNumbers[j, 1]};

                List<int> PointsX = new List<int>();
                List<int> PointsY = new List<int>();
                
                for (int i = 0; i <= n; i++)
                {
                    z = this.f(z, c);
                    int[] pos = Buddhabrot.ComplexToXY(z, this.real, this.imag, this.w, h);
                    if (Buddhabrot.InView(pos, w, h)) {
                        PointsX.Add(pos[0]);
                        PointsY.Add(pos[1]);
                    } else {
                        // int[] c_pos = Buddhabrot.ComplexToXY(c, this.real, this.imag, this.w, h); // mandelbrot
                        // if (Buddhabrot.InView(c_pos, this.w, h)) array[c_pos[1], c_pos[0]] = i;
                        if (this.k < i && i < this.n) { // buddhabrot
                            for (int u = 0; u < PointsX.Count; u++)
                            {
                                array[PointsY[u], PointsX[u]] += 1;
                            }
                        }
                        break;
                    }
                }

                if (t == 0 && j % every == 0) {
                    j++;
                    elapsed = DateTime.Now.Subtract(_start).Ticks/10000000;
                    Console.Write($"\r[INFO] calculate | {100 * j/len}% | {elapsed}s | {Convert.ToInt32(elapsed/j * (len - j))}s            \r");
                }
            }
            if (t == 0) Console.WriteLine("[INFO] calculate | 100% | finished");
            return array;
        }

        public void Calculate()
        {
            long ticks = DateTime.Now.Ticks;
            double[,] numbers = new double[this.threads * this.p, 2];

            // get numbers
            Task<double[,]>[] numbers_tasks = new Task<double[,]>[this.threads];
            for (int i = 0; i < this.threads; i++) {
                numbers_tasks[i] = new Task<double[,]>(() => this.GetNumbers(i));
                numbers_tasks[i].Start();
                Thread.Sleep(1);
            }
            Task.WaitAll(numbers_tasks);

            for (int i = 0; i < this.threads; i++) {
                double[,] result = numbers_tasks[i].Result;
                for (int j = 0; j < result.GetLength(0); j++) {
                    numbers[j + i * this.p, 0] = result[j, 0];
                    numbers[j + i * this.p, 1] = result[j, 1];
                }
            }

            // calculate
            Task<double[,]>[] tasks = new Task<double[,]>[this.threads];
            for (int i = 0; i < this.threads; i++) {
                tasks[i] = new Task<double[,]>(() => this.CalculateThread(numbers, i));
                tasks[i].Start();
                Thread.Sleep(1);
            }
            Task.WaitAll(tasks);
            
            this.arr = new double[this.h, this.w];
            foreach (Task<double[,]> task in tasks) {
                double[,] result = task.Result;
                for (int i = 0; i < this.h; i++) {
                    for (int j = 0; j < this.w; j++) {
                        this.arr[i, j] += result[i, j];
                    }
                }
            }
            this.elapsed = (DateTime.Now.Ticks - ticks)/10000000;
        }

        public void Stretch(double percentile = 3)
        {
            if (this.arr == null) return;
            double[,] arr_s = Buddhabrot.Normalize2DArray(this.arr);
            
            double[] flattened = Buddhabrot.To1DArray<double>(arr_s);
            Array.Sort(flattened);
            double median = flattened[Convert.ToInt32(flattened.Length * (100 - percentile)/100)];

            if (median == 0.0) 
            {
                Console.WriteLine($"[INFO] cannot stretch with: percentile = {percentile} (array might be too dark)");
                return;
            } else if (median == 0.5) return;

            Func<double, double> f_s;
            double a;
            if (median < 0.5) {
                f_s = Math.Asinh;
                a = Math.Sqrt(1 - 4 * median*median)/(2 * median*median);
            } else {
                f_s = Math.Sinh;
                a = Math.Sqrt(1 - 4 * (1 - median)*(1 - median))/(2 * (1 - median)*(1 - median));
            }

            double f_a = f_s(a);
            for (int i = 0; i < arr_s.GetLength(0); i++) {
                for (int j = 0; j < arr_s.GetLength(1); j++) {
                    double x = arr_s[i, j];
                    if (x == 0) continue;
                    arr_s[i, j] = f_s(a * x)/f_a;
                }
            }

            this.arr = arr_s;
        }

        public void SavePoints(double[,] arr2d, string file)
        {
            double maxValue = arr2d.Cast<double>().Max();

            Bitmap bitmap = new Bitmap(w, this.h);
            for (int x = 0; x < this.w; x++) {
                for (int y = 0; y < this.h; y++) {
                    int value = Convert.ToInt32(255.0 * arr2d[y, x]/maxValue);
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
            double maxValue = this.arr.Cast<double>().Max();
            string[] lines = new string[this.h];
            string pixel;
            
            using StreamWriter file = new(path);
            for (int y = 0; y < this.h; y++) {
                for (int x = 0; x < this.w; x++) {
                    pixel = Convert.ToString(Convert.ToInt32(Buddhabrot.LinMap(this.arr[y, x], 0, maxValue, 0, 65535)));
                    file.Write(pixel);
                    if (x != this.w - 1) file.Write(",");
                }
                file.Write("\n");
            }
        }

        public void Save(string path = "")
        {
            if (path == "") path = $"C:\\Coding\\Julia.other\\Buddhabrot; img({this.w}, {this.h}); k{this.k}; n{this.n}; th{this.threads}; t{this.elapsed}; p{this.threads * this.p}.png";
            Console.WriteLine("[INFO] saving to .buddha");
            string temporary = $"{Directory.GetCurrentDirectory()}\\__temp__.buddha";
            this.SaveToText(temporary);
            var psi = new ProcessStartInfo();
            psi.FileName = PATH;

            psi.Arguments = $"\"{DATA2IMG}\" \"{temporary}\" \"{path}\"";
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
        
        public void TestPeriods(int w = 3000, string file = "Test.png")
        {
            Bitmap bitmap = new Bitmap(w, w);
            for (int x = 0; x < w; x++) {
                for (int y = 0; y < w; y++) {
                    double[] z = new double[2] {Buddhabrot.LinMap(x, 0, w, -2, 2), Buddhabrot.LinMap(y, 0, w, -2, 2)};
                    if (this.Periods(z)) {
                        bitmap.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                    } else {
                        bitmap.SetPixel(x, y, Color.FromArgb(255, 255, 255, 255));
                    }
                }
            }
            bitmap.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            Console.Write("saved");
        }
        
        public static double ComplexAbs(double re, double im)
        {
            return Math.Sqrt(re*re + im*im);
        }

        public static double ComplexAbs(double[] z)
        {
            return Math.Sqrt(z[0]*z[0] + z[1]*z[1]);
        }

        static int[] ComplexToXY(double[] z, double[] real, double[] imag, int w, int h)
        {
            return new int[2] {Convert.ToInt32(Buddhabrot.LinMap(z[0], real[0], real[1], 0, w)), Convert.ToInt32(Buddhabrot.LinMap(z[1], imag[0], imag[1], 0, h))};
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

        static double[,] Normalize2DArray(double[,] array)
        {
            double minValue = array.Cast<double>().Min();
            double maxValue = array.Cast<double>().Max();
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] = Buddhabrot.LinMap(array[i, j], minValue, maxValue, 0, 1);
                }
            }
            return array;
        }
    }

    class z2c : Buddhabrot
    {
        public z2c(int w, int k, int n, int p, int threads = 8)
            :base(w, k, n, p, threads)
        {}

        public override double[] f(double[] z, double[] c)
        {
            return new double[2] {z[0]*z[0] - z[1]*z[1] + c[0], 2 * z[0] * z[1] + c[1]};
        }

        public override bool Periods(double[] z)
        {
            double theta = Math.Atan2(z[1], z[0] - 0.25);
            if (Buddhabrot.ComplexAbs(z[0] - 0.25, z[1]) < 0.5 * (1 - Math.Cos(theta))) return true; // p = 1 https://en.wikipedia.org/wiki/Cardioid#Equations
            if (Buddhabrot.ComplexAbs(z[0] + 1, z[1]) < 0.25) return true; // p = 2
            if (Buddhabrot.ComplexAbs(z[0] + 0.125, Math.Abs(z[1]) - 0.7432) < 0.094) return true; // p = 3
            if (Buddhabrot.ComplexAbs(z[0] + 1.309, z[1]) < 0.058) return true; // p = 4, q = 2
            return false;
        }
    }

    class z3c : Buddhabrot
    {
        public z3c(int w, int k, int n, int p, int threads = 8)
            :base(w, k, n, p, threads)
        {}

        public override double[] f(double[] z, double[] c)
        {
            double a2 = z[0]*z[0];
            double b2 = z[1]*z[1];
            return new double[2] {z[0] * (a2 - 3 * b2) + c[0], z[1] * (3 * a2 - b2) + c[1]};
        }

        public override bool Periods(double[] z) // https://mathworld.wolfram.com/Nephroid.html
        {
            double thetaHalfs = Math.Atan2(Math.Abs(z[1]), z[0])/2;
            if (Buddhabrot.ComplexAbs(z) < 0.38 * Math.Pow(Math.Pow(Math.Sin(thetaHalfs), 0.6666666666666) + Math.Pow(Math.Cos(thetaHalfs), 0.6666666666666), 1.5)) return true;
            return false;
        }

    }

    class Program
    {
        static void Main()
        {   
            z2c buddhabrot = new z2c(3000, 0, 500, 9000000);
            buddhabrot.Calculate();
            buddhabrot.Stretch(.2);
            buddhabrot.Save();
        }
    }
}
