using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vector
{
	class Program
	{
		static void Main(string[] args)
		{
			object _ = new object();
			int N = 100_000_000;
			Random rand = new Random();

			double[] vec1 = new double[N], vec2 = new double[N];
			for (int i = 0; i < N; i++)
			{
				vec1[i] = rand.NextDouble() * 100;
				vec2[i] = rand.NextDouble() * 100;
			}

			double sum = 0;

			Stopwatch sw = new Stopwatch();

			//Sequential
			sw.Start();
			for (int i = 0; i < N; i++)
			{
				sum += vec1[i] * vec2[i];
			}
			sw.Stop();
			Print(sum, sw);

			//Slow Parallel For
			sum = 0;
			sw.Reset();
			sw.Start();

			Parallel.For(0, N, i =>
			{
				double tmp = vec1[i] * vec2[i];

				lock(_)
				{
					sum += tmp;
				}
			});

			sw.Stop();
			Print(sum, sw);

			//Fast Parallel For
			sum = 0;
			sw.Reset();
			sw.Start();

			Parallel.For(0, N, () => 0.0, (i, state, local) =>
			{
				return local + vec1[i] * vec2[i];
			}, local =>
			{
				lock(_)
				{
					sum += local;
				}
			});

			sw.Stop();
			Print(sum, sw);

			//Parallel x
			sum = 0;
			sw.Reset();
			sw.Start();

			Parallel.For(0, N, () => 0.0, (i, state, local) =>
			{
				return local + vec1[i] * vec2[i];
			}, local =>
			{
				lock (_)
				{
					sum += local;
				}
			});

			sw.Stop();
			Print(sum, sw);

			Console.ReadLine();
		}

		static void Print(double sum, Stopwatch sw)
		{
			Console.WriteLine("Wynik: " + sum + "\tTime: " + sw.ElapsedMilliseconds + " ms");
		}
	}
}
