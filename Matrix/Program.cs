#define _DEBUG_PRINT_

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Matrix
{
	class Program
	{
		static void Main(string[] args)
		{
			object _ = new object();
			const int N = 100;
			Random rand = new Random();

			Console.Write("Generating matrices...\t");
			double[][] mat1 = new double[N][], mat2 = new double[N][];
			for (int i = 0; i < N; i++)
			{
				mat1[i] = new double[N];
				mat2[i] = new double[N];

				for (int j = 0; j < N; j++)
				{
					mat1[i][j] = rand.NextDouble() * 100;
					mat2[i][j] = rand.NextDouble() * 100;
				}
			}
			Console.WriteLine("Done!\n");

			double[][] result;

			Stopwatch sw = new Stopwatch();

			//=======================================================
			//Sequential
			result = CreateResultMatrix(N);
			sw.Start();

			for (int row = 0; row < N; row++)
			{
				for (int i = 0; i < N; i++)
				{
					for (int column = 0; column < N; column++)
					{
						result[row][column] += mat1[row][i] * mat2[i][column];
					}
				}
			}

			sw.Stop();
			Console.WriteLine("Sequential");
			PrintSw(sw);
#if _DEBUG_PRINT
			PrintMatrix(result);
#endif
			Console.WriteLine();

			//=======================================================
			//Parallel For
			result = CreateResultMatrix(N);
			sw.Reset();
			sw.Start();

			Parallel.For(0, N, row =>
			{
				for (int i = 0; i < N; i++)
				{
					for (int col = 0; col < N; col++)
					{
						result[row][col] += mat1[row][i] * mat2[i][col];
					}
				}
			});

			sw.Stop();
			Console.WriteLine("Parallel.For with iteraring over rows (faster than columns because cache).");
			PrintSw(sw);
#if _DEBUG_PRINT
			PrintMatrix(result);
#endif
			Console.WriteLine();
			
			//=======================================================
			//Task
			result = CreateResultMatrix(N);
			sw.Reset();
			sw.Start();

			List<Task> tasks = new List<Task>(result.Length);

			for (int row = 0; row < result.Length; row++)
			{
				tasks.Add(new Task((object rowObj) => {

					int thisRow = (int)rowObj;
					for (int i = 0; i < mat2.Length; i++)	// Iterates over every element in row
					{
						for (int col = 0; col < result[thisRow].Length; col++)
						{
							result[thisRow][col] += mat1[thisRow][i] * mat2[i][col];
						}
					}
				}, row));

				tasks[row].Start();
			}


			foreach (Task task in tasks)
			{
				task.Wait();
			}

			sw.Stop();
			Console.WriteLine("Task");
			PrintSw(sw);
#if _DEBUG_PRINT
			PrintMatrix(result);
#endif
			Console.WriteLine();

			//=======================================================
			//ThreadPool with manual counter
			result = CreateResultMatrix(N);
			sw.Reset();
			sw.Start();

			int remainingRows = result.Length;
			for (int row = 0; row < result.Length; row++)
			{
				ThreadPool.QueueUserWorkItem((object rowObj) => {

					int thisRow = (int)rowObj;
					for (int i = 0; i < mat2.Length; i++)	// Iterates over every element in row
					{
						for (int col = 0; col < result[thisRow].Length; col++)
						{
							result[thisRow][col] += mat1[thisRow][i] * mat2[i][col];
						}
					}

					lock (_)
						remainingRows--;
				}, row);
			}
			while (remainingRows > 0)
				Thread.Sleep(1);

			sw.Stop();
			Console.WriteLine("Threadpool with manual counter");
			PrintSw(sw);
#if _DEBUG_PRINT
			PrintMatrix(result);
#endif
			Console.WriteLine();

			Console.ReadLine();
		}

		static void PrintMatrix(double[][] matrix)
		{
			if(matrix.GetLength(0) < 20)
			{
				foreach (var row in matrix)
				{
					foreach (var val in row)
					{
						Console.Write((int)val + "\t");
					}
					Console.WriteLine();
				}
			}
		}

		static void PrintSw(Stopwatch sw)
		{
			Console.WriteLine("Time: " + sw.ElapsedMilliseconds + " ms");
		}

		static double[][] CreateResultMatrix(int n)
		{
			double[][] matrix = new double[n][];
			for (int i = 0; i < n; i++)
			{
				matrix[i] = new double[n];
			}

			return matrix;
		}
	}
}
