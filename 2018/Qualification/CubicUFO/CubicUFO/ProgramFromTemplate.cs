using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Math;

namespace CubicUFO
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

		private double _a;

		private Point3D[] _result;

		public void ReadData()
		{
			_a = _in.ReadDouble();
		}

		public void WriteResult()
		{
			_out.WriteCaseResult(_number);

			foreach (var point in _result)
			{
				_out.WriteLine("ddd", point.x, point.y, point.z);
			}
		}

		public void Solve()
		{
			var cube = new Cube3D();

			if (_a < Sqrt(2))
			{
				FindCorrectRotation(cube, cube.RotateX);
			}
			else
			{
				cube.RotateX(PI / 4);
				FindCorrectRotation(cube, cube.RotateZ);
			}

			ComputeResult(cube);
		}

		private void ComputeResult(Cube3D cube)
		{
			var v = cube.vertices;

			var p1 = new Point3D()
			{
				x = (v[7].x + v[4].x + v[5].x + v[1].x) / 4,
				y = (v[7].y + v[4].y + v[5].y + v[1].y) / 4,
				z = (v[7].z + v[4].z + v[5].z + v[1].z) / 4
			};

			var p2 = new Point3D()
			{
				x = (v[7].x + v[4].x + v[2].x + v[6].x) / 4,
				y = (v[7].y + v[4].y + v[2].y + v[6].y) / 4,
				z = (v[7].z + v[4].z + v[2].z + v[6].z) / 4
			};

			var p3 = new Point3D()
			{
				x = (v[7].x + v[6].x + v[5].x + v[3].x) / 4,
				y = (v[7].y + v[6].y + v[5].y + v[3].y) / 4,
				z = (v[7].z + v[6].z + v[5].z + v[3].z) / 4
			};

			_result = new Point3D[3] { p1, p2, p3 };
		}

		private void FindCorrectRotation(Cube3D cube, Action<double> rotationFunc)
		{
			double currRotation = PI / 8;
			double cubeArea = cube.ComputeShadowArea();

			while (Abs(cubeArea - _a) > 0.00000001)
			{
				if (cubeArea < _a) rotationFunc(currRotation);
				else rotationFunc(-currRotation);

				currRotation /= 2;
				cubeArea = cube.ComputeShadowArea();
			}
		}

		class Point3D
		{
			public double x, y, z;

			public Point3D() : this(0, 0, 0)
			{ }

			public Point3D(double x, double y, double z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}
		}

		class Cube3D
		{
			public Point3D[] vertices;

			public Cube3D()
			{
				vertices = new Point3D[8]
				{
					new Point3D(-0.5, -0.5, -0.5),
					new Point3D(-0.5, -0.5, +0.5),
					new Point3D(-0.5, +0.5, -0.5),
					new Point3D(+0.5, -0.5, -0.5),
					new Point3D(-0.5, +0.5, +0.5),
					new Point3D(+0.5, -0.5, +0.5),
					new Point3D(+0.5, +0.5, -0.5),
					new Point3D(+0.5, +0.5, +0.5)
				};
			}

			public void RotateX(double angle)
			{
				foreach (var v in vertices)
				{
					var ry = v.y * Cos(angle) - v.z * Sin(angle);
					var rz = v.y * Sin(angle) + v.z * Cos(angle);

					v.y = ry;
					v.z = rz;
				}
			}

			public void RotateZ(double angle)
			{
				foreach (var v in vertices)
				{
					var rx = v.x * Cos(angle) - v.y * Sin(angle);
					var ry = v.x * Sin(angle) + v.y * Cos(angle);

					v.x = rx;
					v.y = ry;
				}
			}

			public double ComputeShadowArea()
			{
				double s1, s2, s3;
				s1 = Abs((vertices[1].x - vertices[4].x) * (vertices[7].z - vertices[4].z) - (vertices[1].z - vertices[4].z) * (vertices[7].x - vertices[4].x));
				s2 = Abs((vertices[4].x - vertices[2].x) * (vertices[0].z - vertices[2].z) - (vertices[4].z - vertices[2].z) * (vertices[0].x - vertices[2].x));
				s3 = Abs((vertices[1].x - vertices[0].x) * (vertices[3].z - vertices[0].z) - (vertices[1].z - vertices[0].z) * (vertices[3].x - vertices[0].x));

				return s1 + s2 + s3;
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
