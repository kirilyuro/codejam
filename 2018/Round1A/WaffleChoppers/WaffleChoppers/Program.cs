using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WaffleChoppers
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
		private int _c;
		private int _h;
		private int _v;

		private int[,] _w;

		private bool _result;

		public void ReadData()
		{
			var _rchv = _in.ReadMany<int>();
			_r = _rchv[0];
			_c = _rchv[1];
			_h = _rchv[2];
			_v = _rchv[3];

			_w = new int[_r, _c];
			for (int i = 0; i < _r; i++)
			{
				var row = _in.ReadString();
				for (int j = 0; j < _c; j++)
				{
					if (row[j] == '@') _w[i, j] = 1;
				}
			}
		}

		public void WriteResult()
		{
			var output = _result ? "POSSIBLE" : "IMPOSSIBLE";
			_out.WriteCaseResult(_number, "s", output);
		}

		public void Solve()
		{
			var possibleHCuts = Choose(Enumerable.Range(0, _r - 1), _h);
			var possibleVCuts = Choose(Enumerable.Range(0, _c - 1), _v);

			bool broken = false;

			foreach (var hCut in possibleHCuts)
			{
				foreach (var vCut in possibleVCuts)
				{
					if (CheckSolution(hCut.ToArray(), vCut.ToArray()))
					{
						_result = true;
						broken = true;
						break;
					}
				}

				if (broken) break;
			}
		}

		private bool CheckSolution(int[] hCut, int[] vCut)
		{
			int prevCount = -1;

			for (int i = 0; i <= hCut.Length; i++)
			{
				var hFrom = i == 0 ? 0 : hCut[i - 1] + 1;
				var hTo = i == hCut.Length ? _r - 1 : hCut[i];

				for (int j = 0; j <= vCut.Length; j++)
				{
					var vFrom = j == 0 ? 0 : vCut[j - 1] + 1;
					var vTo = j == vCut.Length ? _c - 1 : vCut[j];

					int count = 0;

					for (int h = hFrom; h <= hTo; h++)
					{
						for (int v = vFrom; v <= vTo; v++)
						{
							count += _w[h, v];
						}
					}

					if (prevCount < 0) prevCount = count;
					else if (count != prevCount) return false;
				}
			}

			return true;
		}

		private IEnumerable<IEnumerable<T>> Choose<T>(IEnumerable<T> source, int k)
		{
			if (k == 0) return new List<IEnumerable<T>>(1) { new List<T>(0) };
			if (k > source.Count()) return new List<IEnumerable<T>>(0);
			if (k == source.Count()) return new List<IEnumerable<T>> { source.ToList() };

			List<IEnumerable<T>> choice = new List<IEnumerable<T>>(k);

			var next = Choose(source.Skip(1), k - 1);
			foreach (var nextOption in next)
			{
				choice.Add(new List<T>(1) { source.First() }.Concat(nextOption).ToList());
			}

			choice.AddRange(Choose(source.Skip(1), k));

			return choice;
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
