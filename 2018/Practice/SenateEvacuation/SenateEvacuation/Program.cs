using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenateEvacuation
{
	class Program
	{
		static void Main(string[] args)
		{
			var t = int.Parse(Console.ReadLine());
			for (int i = 1; i <= t; i++)
			{
				var n = int.Parse(Console.ReadLine());
				var pi = Console.ReadLine().Split(' ').Select(p => int.Parse(p)).ToArray();
				var solution = new StringBuilder();
				Solve(
					Enumerable.Range(0, n).Zip(pi, (k, v) => new KeyValuePair<char, int>((char)('A' + k), v)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
					solution
				);
				Console.WriteLine("Case #{0}: {1}", i, solution.ToString());
				Console.OpenStandardOutput().Flush();
			}
		}

		private static void Solve(Dictionary<char, int> s, StringBuilder r)
		{
			if (s.All(kvp => kvp.Value == 0)) return;

			if (s.Count(x => x.Value == 1) == 3)
			{
				var max = MaxBy(s, kvp => kvp.Value);
				r.Append(string.Format(" {0}", max.Key));
				s[max.Key] = s[max.Key] - 1;
			}
			else
			{
				var max1 = MaxBy(s, kvp => kvp.Value);
				s[max1.Key] = s[max1.Key] - 1;

				var max2 = MaxBy(s, kvp => kvp.Value);
				s[max2.Key] = s[max2.Key] - 1;

				r.Append(string.Format(" {0}{1}", max1.Key, max2.Key));
			}

			Solve(s, r);
		}

		private static KeyValuePair<K, V> MaxBy<K, V, T>(Dictionary<K, V> s, Func<KeyValuePair<K, V>, T> p)
		{
			var max = s.Max(p);
			return s.Where(kvp => p(kvp).Equals(max)).First();
		}
	}
}
