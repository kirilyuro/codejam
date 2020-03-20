using System;
using System.Collections.Generic;
using System.Linq;

namespace BathroomStalls
{
	class Program
	{
		static void Main(string[] args)
		{
			var t = int.Parse(Console.ReadLine());
			for (int i = 1; i <= t; i++)
			{
				var nk = Console.ReadLine().Split(' ').Select(_ => long.Parse(_)).ToArray();
				var n = nk[0];
				var k = nk[1];

				long[] sol = Solve(n, k);
				Console.WriteLine("Case #{0}: {1} {2}", i, Math.Max(sol[0], sol[1]), Math.Min(sol[0], sol[1]));
				Console.OpenStandardOutput().Flush();
			}
		}

		private static long[] Solve(long n, long k)
		{
			//var level = Math.Floor(Math.Log(k, 2));
			//var parts = Math.Pow(2, level);
			//var currPart = k - Math.Pow(2, level) + 1;


			//---------------------------------------------
			var sol = new bool[n];
			return SolveAux(k, new bool[n]);
		}

		private static long[] SolveAux(long k, bool[] v)
		{
			//long[][] LsRs = ComputeLsRs(v);
			//var indexedLsRs = LongRange(LsRs.LongLength).Zip(LsRs, (i, lsrs) => new KeyValuePair<long, long[]>(i, lsrs)).Where(z => !v[z.Key]);

			var indexedLsRs = ComputeLsRs(v).Where(z => !v[z.Key]);

			var x = MaxBy(indexedLsRs, z => Math.Min(z.Value[0], z.Value[1]));
			var xx = MaxBy(x, z => Math.Max(z.Value[0], z.Value[1]));
			var xxx = MinBy(xx, z => z.Key).Single();
			if (k == 1)
			{
				return xxx.Value;
			}
			v[xxx.Key] = true;
			return SolveAux(k - 1, v);
		}

		private static IEnumerable<KeyValuePair<long, long[]>> ComputeLsRs(bool[] v)
		{
			var res = new Dictionary<long, long[]>(v.Length);
			//var res = new long[v.Length][];
			for (int i = 0; i < v.Length; i++)
			{
				res[i] = new long[2];
			}
			for (int i = 0; i < v.Length; i++)
			{
				long Ls = 0;
				for (int j = 1; i - j >= 0; j++)
				{
					if (v[i - j]) break;
					Ls++;
				}

				long Rs = 0;
				for (int j = 1; i + j < v.Length; j++)
				{
					if (v[i + j]) break;
					Rs++;
				}

				res[i][0] = Ls;
				res[i][1] = Rs;
			}
			return res;
		}

		private static IEnumerable<T> MaxBy<T, R>(IEnumerable<T> arr, Func<T, R> s)
		{
			var max = arr.Max(s);
			return arr.Where(x => s(x).Equals(max));
		}

		private static IEnumerable<T> MinBy<T, R>(IEnumerable<T> arr, Func<T, R> s)
		{
			var max = arr.Min(s);
			return arr.Where(x => s(x).Equals(max));
		}

		private static long[] LongRange(long size)
		{
			var arr = new long[size];
			for (int i = 0; i < size; i++)
			{
				arr[i] = i;
			}
			return arr;
		}
	}
}
