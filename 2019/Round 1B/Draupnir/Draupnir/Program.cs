using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Draupnir
{
	class Solver
	{
		public const int INPUT_BUFFER_SIZE = 1024 * 1024 * 5; // 5 MB

		public Solver(int number, Input input, Output output, int w)
		{
			_number = number;
			_in = input;
			_out = output;
			_w = w;
		}

		private int _number;
		private Input _in;
		private Output _out;

		int _w;

		public void ReadData()
		{

		}

		public void WriteResult(long r1, long r2, long r3, long r4, long r5, long r6)
		{
			_out.WriteLine("llllll", r1, r2, r3, r4, r5, r6);
			var x = _in.ReadInt();
			if (x == -1) Environment.Exit(0);
		}

		private long Exchange(int d)
		{
			_out.WriteLine("i", d);
			var l = _in.ReadLong();
			if (l == -1L) Environment.Exit(0);
			return l;
		}

		public void Solve()
		{
			long r1, r2, r3, r4, r5, r6;
			long r1s, r2s, r3s;

			// 1
			r1 = r1s = Exchange(1);

			// 2
			r1s = r1s * r1;
			r2 = r2s = Exchange(2) - r1s;

			// 3
			r1s = r1s * r1;
			r3 = r3s = Exchange(3) - r1s - r2s;

			// 4
			r1s = r1s * r1;
			r2s = r2s * r2;
			r4 = Exchange(4) - r1s - r2s - r3s;

			// 5
			r1s = r1s * r1;
			r5 = Exchange(5) - r1s - r2s - r3s - r4;

			// 6
			r1s = r1s * r1;
			r2s = r2s * r2;
			r3s = r3s * r3;
			r6 = Exchange(6) - r1s - r2s - r3s - r4 - r5;

			WriteResult(r1, r2, r3, r4, r5, r6);
		}
	}

	#region General

	class Program
	{
		public static void Main(string[] args)
		{
			var input = new Input(new StreamReader(Console.OpenStandardInput(Solver.INPUT_BUFFER_SIZE)));
			var output = new Output(Console.Out);
			int[] tw = input.ReadMany<int>();
			int t = tw[0];
			int w = tw[1];

			foreach (var testCaseNumber in Enumerable.Range(1, t))
			{
				var solver = new Solver(testCaseNumber, input, output, w);
				solver.ReadData();
				solver.Solve();
				//solver.WriteResult();
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
