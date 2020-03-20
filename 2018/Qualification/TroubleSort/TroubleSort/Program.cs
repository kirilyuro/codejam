using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeJamProblem1
{
	class Solver : ISolver
	{
		public Solver(int number)
		{
			_number = number;
		}

		private int _number;
		private long _n;
		private long[] _v;

		private long _result;

		public void ReadData(Input input)
		{
			_n = input.ReadLong();
			_v = input.ReadMany<long>();
		}

		public void WriteResult(Output output)
		{
			output.WriteCaseResult(_number, "s", _result < 0 ? "OK" : _result.ToString());
		}

		public void Solve()
		{
			var even = new long[(int)(Math.Ceiling(_v.Length / 2.0))];
			var odd = new long[(int)(Math.Floor(_v.Length / 2.0))];

			for (int i = 0; i < odd.Length; i++)
			{
				odd[i] = _v[i * 2 + 1];
				even[i] = _v[i * 2];
			}

			if (odd.Length < even.Length) even[even.Length-1]=_v[_v.Length-1];

			Array.Sort(even);
			Array.Sort(odd);

			for (int i = 0; i < odd.Length; i++)
			{
				_v[i * 2 + 1] = odd[i];
				_v[i * 2] = even[i];
			}

			if (odd.Length < even.Length) _v[_v.Length - 1] = even[even.Length - 1];

			_result = FindError(_v);
		}

		public int FindError(long[] v)
		{
			for (int i = 0; i < v.Length - 1; i++)
			{
				if (v[i] > v[i + 1]) return i;
			}

			return -1;
		}
	}

	#region General

	class Program
	{
		public static void Main(string[] args)
		{
			//var input = new Input(File.OpenText("./input.txt"));
			var input = new Input(new StreamReader(Console.OpenStandardInput(1024 * 1024 * 5)));
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

		public string ReadString()
		{
			return ReadParsed(line => line);
		}

		private T ReadParsed<T>(Func<string, T> parse)
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

		public void WriteCaseResult(int caseNumber, string types, params object[] values)
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

	interface ISolver
	{
		void ReadData(Input input);
		void Solve();
		void WriteResult(Output output);
	}

	#endregion General
}
