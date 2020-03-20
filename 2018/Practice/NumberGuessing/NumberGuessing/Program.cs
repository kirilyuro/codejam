using System;

namespace NumberGuessing
{
	class Program
	{
		static void Main(string[] args)
		{
			var t = int.Parse(Console.ReadLine());

			for (int i = 0; i < t; i++)
			{
				int a, b;
				ReadAB(out a, out b);
				var n = int.Parse(Console.ReadLine());
				for (int j = 0; j < n; j++)
				{
					var q = (int)(Math.Ceiling((a + b) / 2.0));
					Console.WriteLine(q);
					Console.OpenStandardOutput().Flush();
					var r = Console.ReadLine();
					var success = false;
					switch (r)
					{
						case "CORRECT":
							{
								success = true;
								break;
							}
						case "TOO_SMALL":
							{
								a = q;
								break;
							}
						case "TOO_BIG":
							{
								b = q;
								break;
							}
						default:
							{
								Console.Error.WriteLine(r);
								return;
							}
					}
					if (success) break;
				}
			}
		}

		private static void ReadAB(out int a, out int b)
		{
			var ab = Console.ReadLine().Split(' ');
			a = int.Parse(ab[0]);
			b = int.Parse(ab[1]);
		}
	}
}
