using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeJamTemplate1
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
        private string _p;

        private string _y;

        public void ReadData()
        {
            _n = _in.ReadInt();
            _p = _in.ReadString();
        }

        public void WriteResult()
        {
            _out.WriteCaseResult(_number, "s", _y);
        }

        private void Permute(string str,
                                int l, int r, List<string> list)
        {
            if (l == r)
            {
                list.Add(str);
            }
            else
            {
                for (int i = l; i <= r; i++)
                {
                    str = Swap(str, l, i);
                    Permute(str, l + 1, r, list);
                    str = Swap(str, l, i);
                }
            }
        }

        public string Swap(string a, int i, int j)
        {
            char temp;
            char[] charArray = a.ToCharArray();
            temp = charArray[i];
            charArray[i] = charArray[j];
            charArray[j] = temp;
            string s = new string(charArray);
            return s;
        }

        public void Solve1()
        {
            int n = _n = 5;
            string s = string.Join("", Enumerable.Repeat("S", n - 1).Concat(Enumerable.Repeat("E", n - 1)));
            var list = new List<string>();
            Permute(s, 0, s.Length - 1, list);

            var options = list.Distinct();

            foreach (var option in options)
            {
                _p = option;
                Solve1();
                if (_y.Count(c => c == 'S') != _y.Count(c => c == 'E')) Console.WriteLine("ERRORORORORORORR!!!!!!!!!!!!!!");
                Console.WriteLine("{0}: {1}", _p, _y);
            }
        }

        public void Solve()
        {
            _y = "";
            int lydiaSouth = 0;
            int mySouth = 0;
            int totalMoves = 0;

            foreach (var lydiaMove in _p)
            {
                char myMove;
                if (lydiaSouth == mySouth)
                {
                    myMove = (char)('S' + 'E' - lydiaMove);
                }
                else
                {
                    myMove = mySouth > totalMoves / 2.0 ? 'E' : 'S';

                    if (totalMoves + 1 != _p.Length &&
                        ((myMove == 'S' && mySouth + 1 == _n - 1 && lydiaSouth + (lydiaMove == 'S' ? 1 : 0) == _n - 1) ||
                        (myMove == 'E' && totalMoves - mySouth + 1 == _n - 1 && totalMoves - lydiaSouth + (lydiaMove == 'E' ? 1 : 0) == _n - 1)))
                    {
                        myMove = (char)('S' + 'E' - myMove);
                    }
                    else if (totalMoves % 2 == 0 && mySouth == totalMoves / 2 && (mySouth + (myMove == 'S' ? 1 : 0) == lydiaSouth + (lydiaMove == 'S' ? 1 : 0)))
                    {
                        myMove = (char)('S' + 'E' - myMove);
                    }
                }

                _y += myMove;

                if (lydiaMove == 'S') lydiaSouth++;
                if (myMove == 'S') mySouth++;
                totalMoves++;
            }
        }
    }

    #region General

    class Program
    {
        public static void Main(string[] args)
        {
            //using (var input = new Input(File.OpenText("./input.txt")))
            using (var input = new Input(new StreamReader(Console.OpenStandardInput(Solver.INPUT_BUFFER_SIZE))))
            using (var output = new Output(Console.Out))
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
    }

    class Input : IDisposable
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
                    case 'S': return token.Value;
                    default: return null;
                }
            }).ToArray();
        }

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

        public void Dispose()
        {
            _writer.Flush();
            _writer.Dispose();

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
