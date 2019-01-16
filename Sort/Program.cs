using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sort
{
	class Program
	{
		static Random rand = new Random();

		static double[] data;
		static double[] helper;

		static void Main(string[] args)
		{
			int N = 10000000;
			helper = new double[N];
			Stopwatch sw = new Stopwatch();

			int preSortSize = 16;

			//Sequential
			data = MakeData(N);
			sw.Start();
			for (int i = 0; i < N; i += preSortSize)
			{
				Array.Sort(data, i, preSortSize);
			}
			int subSize = preSortSize;
			do
			{
				subSize *= 2;
				for (int i = 0; i < N; i += subSize)
				{
					int it = i;
					int it1 = i;
					int it2 = i + subSize / 2;

					while ((it1 != i + subSize / 2) && (it1 < N) && (it2 != i + subSize) && (it2 < N))
					{
						if (data[it1] <= data[it2])
						{
							helper[it++] = data[it1++];
						}
						else
						{
							helper[it++] = data[it2++];
						}
					}

					while ((it1 != i + subSize / 2) && (it1 < N))
					{
						helper[it++] = data[it1++];
					}

					while ((it2 != i + subSize) && (it2 < N))
					{
						helper[it++] = data[it2++];
					}
				}

				double[] tmp = data;
				data = helper;
				helper = tmp;
			} while (subSize < N);
			sw.Stop();
			Print(data, sw);

			//Parallel For
			data = MakeData(N);
			sw.Start();
			for(int i = 0; i < N; i += preSortSize)
			{
				Array.Sort(data, i, preSortSize);
			}
			subSize = preSortSize;
			do
			{
				subSize *= 2;
				Parallel.For(0, (N - 1) / subSize + 1, i =>
				{
					i *= subSize;

					int it = i;
					int it1 = i;
					int it2 = i + subSize / 2;

					while ((it1 != i + subSize / 2) && (it1 < N) && (it2 != i + subSize) && (it2 < N))
					{
						if (data[it1] <= data[it2])
						{
							helper[it++] = data[it1++];
						}
						else
						{
							helper[it++] = data[it2++];
						}
					}

					while ((it1 != i + subSize / 2) && (it1 < N))
					{
						helper[it++] = data[it1++];
					}

					while ((it2 != i + subSize) && (it2 < N))
					{
						helper[it++] = data[it2++];
					}
				});

				double[] tmp = data;
				data = helper;
				helper = tmp;
			} while (subSize < N);
			sw.Stop();
			Print(data, sw);

			Console.ReadLine();
		}

		static void Print(double[] data, Stopwatch sw)
		{
			Console.WriteLine("Time: " + sw.ElapsedMilliseconds + " ms");
			if(data.Length < 20)
			{
				foreach(double d in data)
				{
					Console.WriteLine(d);
				}
			}
		}

		static double[] MakeData(int n)
		{
			double[] data = new double[n];
			for(int i = 0; i < n; ++i)
			{
				data[i] = rand.NextDouble();
			}

			return data;
		}
	}
}
