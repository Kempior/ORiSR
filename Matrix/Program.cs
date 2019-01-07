using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matrix
{
	class Program
	{
		static void Main(string[] args)
		{
			object _ = new object();
			int N = 3;
			Random rand = new Random();

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

			double[][] result;

			Stopwatch sw = new Stopwatch();
			
			//Sequential
			result = MakeMatrixGreatAgain(N);
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
			Print(result, sw);
			
			//Parallel For
			result = MakeMatrixGreatAgain(N);
			sw.Reset();
			sw.Start();
			Parallel.For(0, N, row =>
			{
				for (int i = 0; i < N; i++)
				{
					for (int column = 0; column < N; column++)
					{
						result[row][column] += mat1[row][i] * mat2[i][column];
					}
				}
			});
			sw.Stop();
			Print(result, sw);

			//Parallel For Local
			result = MakeMatrixGreatAgain(N);
			sw.Reset();
			sw.Start();

			Parallel.For(0, N, () => new double[N], (row, state, local) =>
			{
				for (int i = 0; i < N; i++)
				{
					for (int column = 0; column < N; column++)
					{
						local[row] += mat1[row][i] * mat2[i][column];
					}
				}

				return local;
			}, local =>
			{
				lock(_)
				{
					Console.WriteLine("Dl" + local.Length);
					foreach (var item in local)
					{
						Console.Write(item + "\t");
					}
					Console.WriteLine();
				}
			});

			Console.ReadLine();
		}

		static void Print(double[][] matrix, Stopwatch sw)
		{
			Console.WriteLine("Time: " + sw.ElapsedMilliseconds + " ms");
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

		static double[][] MakeMatrixGreatAgain(int n)
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
