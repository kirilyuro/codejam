using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoundingErrorRetro
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

		private int _n;
		private int _l;
		private List<IndexedValue<int>> _ci;

		private int _maxRoundedValue;

		public void ReadData()
		{
			var nl = _in.ReadMany<int>();
			_n = nl[0];
			_l = nl[1];

			_ci = _in.ReadMany<int>().Index().ToList();
		}

		public void WriteResult()
		{
			_out.WriteCaseResult(_number, "i", _maxRoundedValue);
		}

		public void Solve()
		{
			if (DecimalPart(1) < double.Epsilon)
			{
				_maxRoundedValue = 100;
				return;
			}

			var ciUnderHalf = _ci.Where(x => DecimalPart(x.Value) < 0.5).ToList();
			var numRemainingAnswers = _n - _ci.Sum(x => x.Value);

			var existingVotesToMaximize = ComputeMinVotes(ciUnderHalf);
			var votesToMaximizeNewLang = ComputeMinVotes(new List<IndexedValue<int>> { new IndexedValue<int>(0, 0) }).Single().Value;

			while (numRemainingAnswers > 0 && existingVotesToMaximize.Any())
			{
				var minAdd = existingVotesToMaximize.MinBy(x => x.Value).First();

				if (minAdd.Value > votesToMaximizeNewLang) break;

				if (numRemainingAnswers < minAdd.Value) break;

				ciUnderHalf[minAdd.Index].Value += minAdd.Value;
				numRemainingAnswers -= minAdd.Value;
				existingVotesToMaximize.Remove(minAdd);
			}
						
			var numNewMaximizedNewVotes = Math.Floor(numRemainingAnswers * 1.0 / votesToMaximizeNewLang);

			var t1 = (_ci.Sum(x => Math.Round(x.Value * 100.0 / _n, MidpointRounding.AwayFromZero)));
			var t2 = (Math.Ceiling(numNewMaximizedNewVotes * 100.0 / _n));
			var t3 = (Math.Round((numRemainingAnswers - numNewMaximizedNewVotes * votesToMaximizeNewLang) * 100.0 / _n, MidpointRounding.AwayFromZero));

			_maxRoundedValue = (int)(Math.Round(t1 + t2 + t3, MidpointRounding.AwayFromZero));
		}

		private List<IndexedValue<int>> ComputeMinVotes(List<IndexedValue<int>> ciUnderHalf)
		{
			var res = new List<IndexedValue<int>>(ciUnderHalf.Count);

			int i = 0;
			foreach (var item in ciUnderHalf)
			{
				var curr = item.Value;
				var add = 1;
				while (DecimalPart(curr + add) < 0.5) add++;
				res.Add(new IndexedValue<int>(i++, add));
			}

			return res;
		}

		private double DecimalPart(int votes)
		{
			var t = votes * 100.0 / _n;
			return t - Math.Floor(t);
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
