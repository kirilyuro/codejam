using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GracefulChainsawJugglers
{
	class Solver
	{
		public const int INPUT_BUFFER_SIZE = 1024 * 1024 * 5; // 5 MB

		public Solver(int number, Input input, Output output)
		{
			_number = number;
			_in = input;
			_out = output;
		}

		private int _number;
		private Input _in;
		private Output _out;

		private int _r;
		private int _b;

		private int _res;

		public void ReadData()
		{
			var rb = _in.ReadMany<int>();
			_r = rb[0];
			_b = rb[1];
		}

		public void WriteResult()
		{
			_out.WriteCaseResult(_number, "i", _res);
		}

		public void Solve()
		{
			_res = 0;

			int i;
			for (i = 1; ; i++)
			{
				var tot = (i * i + i) / 2;
				if (_r < tot || _b < tot) break;

				_r -= tot;
				_b -= tot;

				_res += (int)(Math.Pow(2, i));
				if (i % 2 == 0) _res--;
			}

			var allPos = new List<KeyValuePair<int, int>>();
			for (int j = 0; j <= i; j++)
			{
				allPos.Add(new KeyValuePair<int, int>(j, i - j));
			}

			allPos = allPos.Where(x => x.Key <= _r && x.Value <= _b).ToList();
			while (allPos.Any())
			{
				var curr = allPos.MinBy(x => Math.Abs(_r - x.Key - (_b - x.Value))).First();
				allPos.Remove(curr);
				if (curr.Key > _r || curr.Value > _b) break;
				allPos = allPos.Where(x => x.Key <= _r && x.Value <= _b).ToList();
				_r -= curr.Key;
				_b -= curr.Value;
				_res++;
			}

			var rem = Math.Max(_r, _b);
			if (_r > _b)
			{
				if (!allPos.Any(x => x.Key == i)) i++;
			}
			else if (_r < _b)
			{
				if (!allPos.Any(x => x.Value == i)) i++;
			}
			else
			{
				if (!allPos.Any(x => x.Key == i && x.Value == i)) i++;
			}

			while (i <= rem)
			{
				rem -= i;
				_res++;
				i++;
			}
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
