using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sort
{
	class Program
	{
		static Random rand = new Random();

		static void Main(string[] args)
		{

			double[] data;
			double[] helper;

			int N = 1_000_000;
			Stopwatch sw = new Stopwatch();

			#region Sequential
			//=======================================================
			data = MakeData(N);
			helper = new double[N];
			sw.Start();

			for (int subSize = 2; subSize < N * 2; subSize *= 2)
			{
				for (int i = 0; i < N; i += subSize)
				{
					int bufferIt = i;
					int leftIt = i;
					int rightIt = i + subSize / 2;
					if (N < rightIt)
						rightIt = N;
					int halfPoint = rightIt;
					int endPoint = i + subSize;
					if (N < endPoint)
						endPoint = N;

					while (leftIt != halfPoint && rightIt != endPoint)
					{
						if (data[leftIt] <= data[rightIt])
							helper[bufferIt++] = data[leftIt++];
						else
							helper[bufferIt++] = data[rightIt++];
					}

					while (leftIt != halfPoint)
						helper[bufferIt++] = data[leftIt++];

					while (rightIt != endPoint)
						helper[bufferIt++] = data[rightIt++];
				}

				double[] tmp = data;
				data = helper;
				helper = tmp;
			}

			sw.Stop();
			Console.Write("Sequential\t");
			Print(data, sw);
			#endregion

			#region Parallel.For
			//=======================================================
			data = MakeData(N);
			helper = new double[N];
			sw.Restart();
			sw.Start();

			for (int subSize = 2; subSize < N * 2; subSize *= 2)
			{
				Parallel.For(0, (N - 1) / subSize + 1, i =>
				{
					Merge(data, helper, i * subSize, subSize);
				});

				double[] tmp = data;
				data = helper;
				helper = tmp;
			}

			sw.Stop();
			Console.Write("Parallel.For\t");
			Print(data, sw);
			#endregion

			#region Task naïve
			//=======================================================
			data = MakeData(N);
			helper = new double[N];
			sw.Restart();
			sw.Start();

			for (int subSize = 2; subSize < N * 2; subSize *= 2)
			{
				List<Task> tasks = new List<Task>();
				for (int startPoint = 0; startPoint < N; startPoint += subSize)
					tasks.Add(Task.Factory.StartNew((object o) =>
					{
						Merge(data, helper, ((dynamic)o).startPoint, ((dynamic)o).subSize);
					}, new { startPoint, subSize }));

				foreach (Task task in tasks)
					task.Wait();

				double[] tmp = data;
				data = helper;
				helper = tmp;
			}

			sw.Stop();
			Console.Write("Task naive\t");
			Print(data, sw);
			#endregion

			#region Task slightly less naïve
			//=======================================================
			{
				data = MakeData(N);
				helper = new double[N];
				sw.Restart();
				sw.Start();

				const int maxTasks = 64;

				List<Task> tasks = new List<Task>();

				int subSize = (int)Math.Ceiling((double)N / maxTasks + 0.5);
				if (N > maxTasks)
					for (int i = 0; i < N; i += subSize)
						tasks.Add(Task.Factory.StartNew((object o) =>
						{
							SubSort(data, helper, ((dynamic)o).startPoint, ((dynamic)o).subSize);
						}, new { startPoint = i, subSize = i + subSize < N ? subSize : N - i }));

				foreach (Task task in tasks)
					task.Wait();

				for (; subSize < N * 2; subSize *= 2)
				{
					for (int startPoint = 0; startPoint < N; startPoint += subSize)
						tasks.Add(Task.Factory.StartNew((object o) =>
						{
							Merge(data, helper, ((dynamic)o).startPoint, ((dynamic)o).subSize);
						}, new { startPoint, subSize }));

					foreach (Task task in tasks)
						task.Wait();

					double[] tmp = data;
					data = helper;
					helper = tmp;
				}
			}

			sw.Stop();
			Console.Write("Task less naive\t");
			Print(data, sw);
			#endregion

			Console.ReadLine();
		}

		static double[] MakeData(int n)
		{
			double[] data = new double[n];
			for (int i = 0; i < n; ++i)
			{
				data[i] = rand.NextDouble();
			}

			return data;
		}

		// Merges two sorted areas into one
		static void Merge(double[] oldArray, double[] newArray, int startPoint, int subSize)
		{
			int bufferIt = startPoint;
			int leftIt = startPoint;
			int rightIt = startPoint + subSize / 2;
			if (oldArray.Length < rightIt)
				rightIt = oldArray.Length;

			int halfPoint = rightIt;
			int endPoint = startPoint + subSize;
			if (oldArray.Length < endPoint)
				endPoint = oldArray.Length;

			while (leftIt != halfPoint && rightIt != endPoint)
			{
				if (oldArray[leftIt] <= oldArray[rightIt])
					newArray[bufferIt++] = oldArray[leftIt++];
				else
					newArray[bufferIt++] = oldArray[rightIt++];
			}

			while (leftIt != halfPoint)
				newArray[bufferIt++] = oldArray[leftIt++];

			while (rightIt != endPoint)
				newArray[bufferIt++] = oldArray[rightIt++];
		}

		// Sorts a part of and array
		static void SubSort(double[] originalArr, double[] helperArr, int startPoint, int sortSize)
		{
			double[] startingOriginal = originalArr;

			int segmentEndPoint = startPoint + sortSize;

			for (int subSize = 2; subSize < sortSize * 2; subSize *= 2)
			{
				for (int i = startPoint; i < segmentEndPoint; i += subSize)
				{
					int bufferIt = i;
					int leftIt = i;
					int rightIt = i + subSize / 2;
					if (segmentEndPoint < rightIt)
						rightIt = segmentEndPoint;
					int halfPoint = rightIt;
					int endPoint = i + subSize;
					if (segmentEndPoint < endPoint)
						endPoint = segmentEndPoint;

					while (leftIt != halfPoint && rightIt != endPoint)
					{
						if (originalArr[leftIt] <= originalArr[rightIt])
							helperArr[bufferIt++] = originalArr[leftIt++];
						else
							helperArr[bufferIt++] = originalArr[rightIt++];
					}

					while (leftIt != halfPoint)
						helperArr[bufferIt++] = originalArr[leftIt++];

					while (rightIt != endPoint)
						helperArr[bufferIt++] = originalArr[rightIt++];
				}
				
				double[] tmp = originalArr;
				originalArr = helperArr;
				helperArr = tmp;
			}

			if (startingOriginal != originalArr)
				Array.Copy(originalArr, startPoint, startingOriginal, startPoint, sortSize);
		}

		static void Print(double[] data, Stopwatch sw)
		{
			Console.WriteLine("Time: " + sw.ElapsedMilliseconds + " ms");
			if (data.Length < 20)
			{
				foreach (double d in data)
				{
					Console.WriteLine(d);
				}
			}
		}
	}
}
