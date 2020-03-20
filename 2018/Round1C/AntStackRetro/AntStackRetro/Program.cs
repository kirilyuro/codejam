using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AntStackRetro
{
	class Solver
	{
		public const int INPUT_BUFFER_SIZE = 1024 * 1024 * 50; // 5 MB

		public Solver(int number, Input input, Output output)
		{
			_number = number;
			_in = input;
			_out = output;
		}

		private int _number;
		private Input _in;
		private Output _out;

		private int _n;
		private int[] _w;

		private Dictionary<DpParams, int> _memoization;
		private int _maxAnts;

		public void ReadData()
		{
			_n = _in.ReadInt();
			_w = _in.ReadMany<int>().Take(_n).ToArray();
		}

		public void WriteResult()
		{
			_out.WriteCaseResult(_number, "i", _maxAnts);
		}

		public void Solve()
		{
			/* Recursion */
			//_memoization = new Dictionary<DpParams, int>();
			//_maxAnts = SolveAux(_n - 1, long.MaxValue);


			/* DP Test Set 1 */
			//var dp = new int[_n, 7002];

			//for (int w = _w[0]; w < dp.GetLength(1); w++)
			//{
			//	dp[0, w] = 1;
			//}

			//for (int i = 1; i < dp.GetLength(0); i++)
			//{
			//	for (int w = 1; w < dp.GetLength(1); w++)
			//	{
			//		var t1 = dp[i - 1, w];
			//		var t2 = t1;

			//		if (w >= _w[i])
			//			t2 = dp[i - 1, Math.Min(6 * _w[i], w - _w[i])] + 1;

			//		dp[i, w] = Math.Max(t1, t2);
			//	}
			//}

			//_maxAnts = dp[_n - 1, 7001];

			/* DP Test Set 2 */
			var dp = new int[_n, 140];

			for (int i = 0; i < dp.GetLength(0); i++)
			{
				for (int j = 1; j < dp.GetLength(1); j++)
				{
					dp[i, j] = int.MaxValue;
				}
			}

			dp[0, 1] = _w[0];

			for (int i = 1; i < dp.GetLength(0); i++)
			{
				for (int j = 1; j < dp.GetLength(1); j++)
				{
					var t1 = dp[i - 1, j];
					var t2 = t1;

					if (dp[i - 1, j - 1] <= 6 * _w[i])
						t2 = dp[i - 1, j - 1] + _w[i];

					dp[i, j] = Math.Min(t1, t2);
				}
			}

			_maxAnts = 139;
			while (dp[_n - 1, _maxAnts] == int.MaxValue) _maxAnts--;
		}

		private int SolveAux(int i, long m)
		{
			var dpParams = new DpParams { i = i, m = m };
			if (_memoization.ContainsKey(dpParams))
			{
				return _memoization[dpParams];
			}

			if (i < 0) return 0;

			var t1 = SolveAux(i - 1, m);
			var t2 = t1;

			if (m >= _w[i])
				t2 = SolveAux(i - 1, Math.Min(6 * _w[i], m - _w[i])) + 1;

			return _memoization[dpParams] = Math.Max(t1, t2);
		}

		struct DpParams
		{
			public int i { get; set; }
			public long m { get; set; }
		}
	}

	#region General

	class Program
	{
		public static void Main(string[] args)
		{
			//var input = new Input(File.OpenText("./input.txt"));
			var input = new Input(new StreamReader(Console.OpenStandardInput(Solver.INPUT_BUFFER_SIZE)));
			var output = new Output(Console.Out);
			var t = input.ReadInt();
			var solvers = Enumerable.Range(1, t).Select(testCaseNumber =>
			{
				var solver = new Solver(testCaseNumber, input, output);
				solver.ReadData();
				return solver;
			}).ToArray();

			solvers.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount)
				.ForAll(solver =>
				{
					solver.Solve();
				});

			foreach (var solver in solvers)
			{
				solver.WriteResult();
			}

			Console.Out.Flush();
			Console.Out.Close();
		}
	}

	class Input
	{
		private TextReader _reader;

		public Input(TextReader reader)
		{
			_reader = reader;
		}

		public int ReadInt()
		{
			return Read(int.Parse);
		}

		public long ReadLong()
		{
			return Read(long.Parse);
		}

		public double ReadDouble()
		{
			return Read(double.Parse);
		}

		public string ReadString()
		{
			return Read(line => line);
		}

		private T Read<T>(Func<string, T> parse)
		{
			var line = _reader.ReadLine();
			return parse(line);
		}

		public T[] ReadMany<T>()
		{
			var typeChar =
				typeof(T) == typeof(int) ? "i" :
				typeof(T) == typeof(long) ? "l" :
				typeof(T) == typeof(double) ? "d" :
				typeof(T) == typeof(string) ? "s" :
				null;

			return ReadMany(typeChar).Cast<T>().ToArray();
		}

		public object[] ReadMany(string types)
		{
			var tokens = _reader.ReadLine().Split(' ').Index();
			return tokens.Select<IndexedValue<string>, object>(token =>
			{
				switch (token.Index < types.Length ? types[token.Index] : types.First())
				{
					case 'i': return int.Parse(token.Value);
					case 'l': return long.Parse(token.Value);
					case 'd': return double.Parse(token.Value);
					case 's': return token.Value;
					default: return null;
				}
			}).ToArray();
		}
	}

	class Output
	{
		private TextWriter _writer;

		public Output(TextWriter writer)
		{
			_writer = writer;
		}

		public void WriteCaseResult(int caseNumber)
		{
			WriteCaseResult(caseNumber, "");
		}

		public void WriteCaseResult(int caseNumber, string types, params object[] values)
		{
			_writer.Write("Case #{0}:", caseNumber);

			if (values.Any()) _writer.Write(" ");

			WriteLine(types, values);
		}

		public void WriteLine(string types, params object[] values)
		{
			Write(types, values);
			_writer.WriteLine();
			_writer.Flush();
		}

		public void Write(string types, params object[] values)
		{
			var stringValues = values.Index().Select(value =>
			{
				string format = string.Empty;
				switch (types[value.Index])
				{
					case 'i': format = "{0}"; break;
					case 'l': format = "{0}"; break;
					case 's': format = "{0}"; break;
					case 'd': format = "{0:0.000000}"; break;
					default: return null;
				}
				return string.Format(format, value.Value);
			});

			_writer.Write(string.Join(" ", stringValues.ToArray()));
			_writer.Flush();
		}
	}

	static class MyEnumerableExtensions
	{
		public static IEnumerable<IndexedValue<T>> Index<T>(this IEnumerable<T> source)
		{
			using (var enumerator = source.GetEnumerator())
			{
				for (int i = 0; enumerator.MoveNext(); i++)
				{
					yield return new IndexedValue<T>(i, enumerator.Current);
				}
			}
		}

		public static IEnumerable<T> MaxBy<T, K>(this IEnumerable<T> source, Func<T, K> selector, IComparer<K> comparer = null)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			comparer = comparer ?? Comparer<K>.Default;
			return source.ExtremaBy(selector, (x, y) => comparer.Compare(x, y));
		}

		public static IEnumerable<T> MinBy<T, K>(this IEnumerable<T> source, Func<T, K> selector, IComparer<K> comparer = null)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			comparer = comparer ?? Comparer<K>.Default;
			return source.ExtremaBy(selector, (x, y) => -Math.Sign(comparer.Compare(x, y)));
		}

		public static IEnumerable<T> ExtremaBy<T, K>(this IEnumerable<T> source, Func<T, K> selector, Func<K, K, int> comparer)
		{
			using (var e = source.GetEnumerator())
			{
				if (!e.MoveNext())
					return new List<T>();

				var extrema = new List<T> { e.Current };
				var extremaKey = selector(e.Current);

				while (e.MoveNext())
				{
					var item = e.Current;
					var key = selector(item);
					var comparison = comparer(key, extremaKey);
					if (comparison > 0)
					{
						extrema = new List<T> { item };
						extremaKey = key;
					}
					else if (comparison == 0)
					{
						extrema.Add(item);
					}
				}

				return extrema;
			}
		}
	}

	class IndexedValue<T>
	{
		public int Index { get; set; }
		public T Value { get; set; }

		public IndexedValue(int index, T value)
		{
			Index = index;
			Value = value;
		}
	}

	#endregion General
}
