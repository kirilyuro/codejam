using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Math;

namespace CodeJamTemplate1
{
    class Solver : SolverBase
    {
        public const int INPUT_BUFFER_SIZE = 1024 * 1024 * 5; // 5 MB
        public const bool USE_STDIN = false;

        public Solver(int caseNumber, Input input, Output output)
            : base(caseNumber, input, output)
        { }



        public override void ReadData()
        {

        }

        public override void WriteResult()
        {
            output.WriteCaseResult(caseNumber, 0);
        }

        public override void Solve()
        {

        }
    }

    #region General

    #region Core

    class Program
    {
        public static void Main(string[] args)
        {
            using (var input = InitializeInput())
			using (var output = InitializeOutput())
            {
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
            }
        }

        private static Input InitializeInput()
        {
            return Solver.USE_STDIN ?
                new Input(new StreamReader(Console.OpenStandardInput(Solver.INPUT_BUFFER_SIZE))) :
                new Input(File.OpenText("./input.txt"));
        }

		private static Output InitializeOutput()
		{
			return new Output(Console.Out);
		}
    }

    abstract class SolverBase
    {
        protected SolverBase(int caseNumber, Input input, Output output)
        {
            this.caseNumber = caseNumber;
            this.input = input;
            this.output = output;
        }

        protected readonly int caseNumber;
        protected readonly Input input;
        protected readonly Output output;

        public abstract void ReadData();
        public abstract void WriteResult();
        public abstract void Solve();
    }

    #endregion Core

    #region I/O

    static class IO
    {
        public static string GetTypesString(params Type[] types)
        {
            return string.Join(string.Empty, types.Select(type =>
            {
                return
                    type == typeof(int) ? "i" :
                    type == typeof(long) ? "l" :
                    type == typeof(double) ? "d" :
                    type == typeof(string) ? "s" :
                    null;
            }));
        }
    }

    class Input : IDisposable
    {
        private TextReader _reader;

        public Input(TextReader reader)
        {
            _reader = reader;
        }

        #region Read

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

        #endregion Read

        #region ReadMany

        public T[] ReadMany<T>()
        {
            return ReadMany(IO.GetTypesString(typeof(T)))
                .Cast<T>().ToArray();
        }

        public void ReadMany<T1, T2>(out T1 o1, out T2 o2)
        {
            var objs = ReadMany(IO.GetTypesString(typeof(T1), typeof(T2)));
            o1 = (T1)objs[0]; o2 = (T2)objs[1];
        }

        public void ReadMany<T1, T2, T3>(out T1 o1, out T2 o2, out T3 o3)
        {
            var objs = ReadMany(IO.GetTypesString(typeof(T1), typeof(T2), typeof(T3)));
            o1 = (T1)objs[0]; o2 = (T2)objs[1]; o3 = (T3)objs[2];
        }

        public void ReadMany<T1, T2, T3, T4>(out T1 o1, out T2 o2, out T3 o3, out T4 o4)
        {
            var objs = ReadMany(IO.GetTypesString(typeof(T1), typeof(T2), typeof(T3), typeof(T4)));
            o1 = (T1)objs[0]; o2 = (T2)objs[1]; o3 = (T3)objs[2]; o4 = (T4)objs[3];
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

        #endregion ReadMany

        public void Dispose()
        {
            _reader.Dispose();
        }
    }

    class Output : IDisposable
    {
        private TextWriter _writer;

        public Output(TextWriter writer)
        {
            _writer = writer;
        }

        public void WriteCaseResultHeader(int caseNumber)
        {
            WriteCaseResultExplicit(caseNumber, "");
        }

        #region WriteCaseResult

        public void WriteCaseResult<T>(int caseNumber, T value)
        {
            WriteCaseResultExplicit(caseNumber, IO.GetTypesString(typeof(T)), value);
        }

        public void WriteCaseResult<T1, T2>(int caseNumber, T1 v1, T2 v2)
        {
            WriteCaseResultExplicit(caseNumber, IO.GetTypesString(typeof(T1), typeof(T2)), v1, v2);
        }

        public void WriteCaseResult<T1, T2, T3>(int caseNumber, T1 v1, T2 v2, T3 v3)
        {
            WriteCaseResultExplicit(caseNumber, IO.GetTypesString(typeof(T1), typeof(T2), typeof(T3)), v1, v2, v3);
        }

        public void WriteCaseResult<T1, T2, T3, T4>(int caseNumber, T1 v1, T2 v2, T3 v3, T4 v4)
        {
            WriteCaseResultExplicit(caseNumber, IO.GetTypesString(typeof(T1), typeof(T2), typeof(T3), typeof(T4)), v1, v2, v3, v4);
        }

        public void WriteCaseResultExplicit(int caseNumber, string types, params object[] values)
        {
            _writer.Write("Case #{0}:", caseNumber);

            if (values.Any()) _writer.Write(" ");

            WriteLineExplicit(types, values);
        }

        #endregion WriteCaseResult

        #region WriteLine

        public void WriteLine<T>(T value)
        {
            WriteLineExplicit(IO.GetTypesString(typeof(T)), value);
        }

        public void WriteLine<T1, T2>(T1 v1, T2 v2)
        {
            WriteLineExplicit(IO.GetTypesString(typeof(T1), typeof(T2)), v1, v2);
        }

        public void WriteLine<T1, T2, T3>(T1 v1, T2 v2, T3 v3)
        {
            WriteLineExplicit(IO.GetTypesString(typeof(T1), typeof(T2), typeof(T3)), v1, v2, v3);
        }

        public void WriteLine<T1, T2, T3, T4>(T1 v1, T2 v2, T3 v3, T4 v4)
        {
            WriteLineExplicit(IO.GetTypesString(typeof(T1), typeof(T2), typeof(T3), typeof(T4)), v1, v2, v3, v4);
        }

        public void WriteLineExplicit(string types, params object[] values)
        {
            WriteExplicit(types, values);
            _writer.WriteLine();
            _writer.Flush();
        }

        #endregion WriteLine

        #region Write

        public void Write<T>(T value)
        {
            WriteExplicit(IO.GetTypesString(typeof(T)), value);
        }

        public void Write<T1, T2>(T1 v1, T2 v2)
        {
            WriteExplicit(IO.GetTypesString(typeof(T1), typeof(T2)), v1, v2);
        }

        public void Write<T1, T2, T3>(T1 v1, T2 v2, T3 v3)
        {
            WriteExplicit(IO.GetTypesString(typeof(T1), typeof(T2), typeof(T3)), v1, v2, v3);
        }

        public void Write<T1, T2, T3, T4>(T1 v1, T2 v2, T3 v3, T4 v4)
        {
            WriteExplicit(IO.GetTypesString(typeof(T1), typeof(T2), typeof(T3), typeof(T4)), v1, v2, v3, v4);
        }

        public void WriteExplicit(string types, params object[] values)
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

        #endregion Write

        public void Dispose()
        {
            _writer.Flush();
            _writer.Dispose();
        }
    }

    #endregion I/O

    #region Extensions

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

    static class MyNumberExtensions
    {
        public static bool IsWhole(this double value)
        {
            return Abs(Floor(value) - value) < double.Epsilon;
        }
    }

    #endregion Extensions

    #region Models

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

    #endregion Models

    #endregion General
}
