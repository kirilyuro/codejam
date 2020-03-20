using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Template
{
	class Solver : ISolver
	{
		public Solver(int number)
		{
			_number = number;
		}

		private int _number;
		private long _n, _k;
		private long _solMin, _solMax;


		public void ReadData(Input input)
		{
			var nk = input.ReadMany<long>();
			_n = nk[0];
			_k = nk[1];
		}

		public void WriteResult(Output output)
		{
			output.WriteCaseResult(_number, "ll", _solMax, _solMin);
		}

		public void Solve()
		{
			var sol = new bool[_n];
			var res = SolveAux(_k, new bool[_n]);
			_solMax = Math.Max(res[0], res[1]);
			_solMin = Math.Min(res[0], res[1]);
		}

		private static long[] SolveAux(long k, bool[] v)
		{
			var indexedLsRs = ComputeLsRs(v).Where(z => !v[z.Key]);

			var x = indexedLsRs.MaxBy(z => Math.Min(z.Value[0], z.Value[1]));
			var xx = x.MaxBy(z => Math.Max(z.Value[0], z.Value[1]));
			var xxx = xx.MinBy(z => z.Key).Single();
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
	}

	#region General

	class Program
	{
		public static void Main(string[] args)
		{
			var input = new Input(File.OpenText("./input.txt"));
			//var input = new Input(Console.In);
			var output = new Output(Console.Out);
			var t = input.ReadInt();
			var solvers = Enumerable.Range(1, t).Select(testCaseNumber =>
			{
				var solver = new Solver(testCaseNumber);
				solver.ReadData(input);
				return solver;
			}).ToArray();

			solvers.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount)
				.ForAll(solver =>
				{
					solver.Solve();
				});

			foreach (var solver in solvers)
			{
				solver.WriteResult(output);
			}

			Console.Out.Flush();
			Console.Out.Close();
		}
	}

	interface ISolver
	{
		void ReadData(Input input);
		void Solve();
		void WriteResult(Output output);
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
			return ReadParsed(int.Parse);
		}

		public long ReadLong()
		{
			return ReadParsed(long.Parse);
		}

		public double ReadDouble()
		{
			return ReadParsed(double.Parse);
		}

		private T ReadParsed<T>(Func<string, T> parse)
		{
			var line = _reader.ReadLine();
			return parse(line);
		}

		public T[] ReadMany<T>()
		{
			var typeChar = 
				typeof(T) == typeof(int) ?		"i" :
				typeof(T) == typeof(long) ?		"l" :
				typeof(T) == typeof(double) ?	"d" : 
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

		public void WriteCaseResult(int caseNumber, string types, params object[] values)
		{
			var stringValues = values.Index().Select(value =>
			{
				string format = string.Empty;
				switch (types[value.Index])
				{
					case 'i': format = "{0}"; break;
					case 'l': format = "{0}"; break;
					case 'd': format = "{0:0.000000}"; break;
					default: return null;
				}
				return string.Format(format, value.Value);
			});

			_writer.WriteLine("Case #{0}: {1}", caseNumber, string.Join(" ", stringValues.ToArray()));
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
