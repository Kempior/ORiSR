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
			const int N = 100_000_000;
			Random rand = new Random();

			Console.Write("Generating vectors...\t");
			double[] vec1 = new double[N], vec2 = new double[N];
			for (int i = 0; i < N; i++)
			{
				vec1[i] = rand.NextDouble() * 100;
				vec2[i] = rand.NextDouble() * 100;
			}
			Console.WriteLine("Done!\n");

			double sum = 0;

			Stopwatch sw = new Stopwatch();

			//=======================================================
			//Sequential
			sw.Start();
			for (int i = 0; i < N; i++)
			{
				sum += vec1[i] * vec2[i];
			}
			sw.Stop();
			Console.WriteLine("Sequential");
			Print(sum, sw);
			Console.WriteLine();

			//=======================================================
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
			Console.WriteLine("Simple Parallel.For with locks every cycle");
			Print(sum, sw);
			Console.WriteLine();

			//=======================================================
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
			Console.WriteLine("Parallel.For with an internal variable");
			Print(sum, sw);
			Console.WriteLine();

			//=======================================================
			//Task
			sum = 0;
			sw.Reset();
			sw.Start();

			int taskCount = 64;
			List<Task> tasks = new List<Task>();
			
			for (int i = 0; i < taskCount; i++)
			{
				int chunkCount = (int)Math.Ceiling((double)N / taskCount);
				int chunkOffset = (int)Math.Ceiling(i * ((double)chunkCount));

				tasks.Add(new Task((object chunk) =>
				{
					int offset = (((int, int))chunk).Item1;
					int count = (((int, int))chunk).Item2;
					int end = offset + count;
					if (N <= end)
						end = N - 1;

					double partialSum = 0;

					for (int j = offset; j < end; j++)
					{
						partialSum += vec1[j] * vec2[j];
					}

					lock (_)
					{
						sum += partialSum;
					}
				}, (chunkOffset, chunkCount)));

				tasks[i].Start();
			}

			foreach (Task task in tasks)
			{
				task.Wait();
			}
			
			sw.Stop();
			Console.WriteLine("Task");
			Print(sum, sw);
			Console.WriteLine();

			Console.ReadLine();
		}

		static void Print(double sum, Stopwatch sw)
		{
			Console.WriteLine("Wynik: " + sum + "\tTime: " + sw.ElapsedMilliseconds + " ms");
		}
	}
}
