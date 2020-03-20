using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BitParty
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

		private int _r, _b, _c;
		private Cashier[] _cashiers;

		private int _result;

		public void ReadData()
		{
			var rbc = _in.ReadMany<int>();
			_r = rbc[0];
			_b = rbc[1];
			_c = rbc[2];

			_cashiers = new Cashier[_c];
			for (int i = 0; i < _c; i++)
			{
				var msp = _in.ReadMany<int>();
				_cashiers[i] = new Cashier
				{
					MaxBits = msp[0],
					ScanTime = msp[1],
					PaymentTime = msp[2],
					Index = i
				};
			}
		}

		public void WriteResult()
		{
			_out.WriteCaseResult(_number, "i", _result);
		}

		public void Solve()
		{
			var robots = new Robot[_r];
			robots[0] = new Robot { Bits = (int)(Math.Ceiling(_b * 1.0 / _r)) };

			for (int i = 1; i < _r; i++)
			{
				robots[i] = new Robot { Bits = _b / _r };
			}

			IEnumerable<Cashier> availableCashiers = _cashiers;
			for (int i = 0; i < robots.Length; i++)
			{
				robots[i].Cashier = availableCashiers.MinBy(c => c.ScanTime).First();
				availableCashiers = availableCashiers.Except(new List<Cashier> { robots[i].Cashier });
			}

			EnsureMaxBits(robots, _cashiers);
			Improve(robots, _cashiers);

			_result = robots.Select(r => r.Cashier.TimeSpent(r.Bits)).Max();
		}

		private void Improve(Robot[] robots, Cashier[] cashiers)
		{
			bool improved = false;

			do
			{
				var b1 = ImproveCashierSelection(robots, cashiers);
				var b2 = ImproveBitsDistribution(robots, cashiers);
			}
			while (improved);
		}

		private bool ImproveBitsDistribution(Robot[] robots, Cashier[] cashiers)
		{
			bool improved = false;

			foreach (var srcRobot in robots)
			{
				var dstRobot = robots.FirstOrDefault(r => r.Bits < r.Cashier.MaxBits && r.Cashier.ScanTime < srcRobot.Cashier.ScanTime);
				if (dstRobot == null) continue;

				var transaction = Math.Min(dstRobot.Cashier.MaxBits - dstRobot.Bits, srcRobot.Bits);
				dstRobot.Bits += transaction;
				srcRobot.Bits -= transaction;
				improved = true;
			}

			return improved;
		}

		private bool ImproveCashierSelection(Robot[] robots, Cashier[] cashiers)
		{
			bool improved = false;

			foreach (var robot in robots)
			{
				foreach (var cashier in cashiers.Where(c => !robots.Any(r => r.Cashier == c)))
				{
					if (cashier.TimeSpent(robot.Bits) < robot.Cashier.TimeSpent(robot.Bits))
					{
						robot.Cashier = cashier;
						improved = true;
					}
				}
			}

			return improved;
		}

		private void EnsureMaxBits(Robot[] robots, Cashier[] _cashiers)
		{
			var invalidRobot = robots.FirstOrDefault(r => r.Bits > r.Cashier.MaxBits);
			while (invalidRobot != null)
			{
				var diff = invalidRobot.Bits - invalidRobot.Cashier.MaxBits;
				while (diff > 0)
				{
					foreach (var robot in robots)
					{
						if (robot.Bits < robot.Cashier.MaxBits)
						{
							var maxTransaction = robot.Cashier.MaxBits - robot.Bits;
							var transaction = Math.Min(maxTransaction, diff);
							robot.Bits += transaction;
							invalidRobot.Bits -= transaction;
							diff -= transaction;

							if (diff == 0) break;
						}
					}
				}

				invalidRobot = robots.FirstOrDefault(r => r.Bits > r.Cashier.MaxBits);
			}
		}

		class Cashier
		{
			public int MaxBits { get; set; }
			public int ScanTime { get; set; }
			public int PaymentTime { get; set; }
			public int Index { get; set; }

			public int TimeSpent(int bits)
			{
				return ScanTime * bits + PaymentTime;
			}
		}

		class Robot
		{
			public int Bits { get; set; }
			public Cashier Cashier { get; set; }
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
