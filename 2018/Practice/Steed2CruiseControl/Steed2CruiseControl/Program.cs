using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steed2CruiseControl
{
	class Program
	{
		static void Main(string[] args)
		{
			var t = int.Parse(Console.ReadLine());
			for (int i = 1; i <= t; i++)
			{
				var dn = Console.ReadLine().Split(' ').Select(_ => long.Parse(_)).ToArray();
				var d = dn[0];
				var n = dn[1];
				var horses = ReadKS(n).ToArray();
				var currentSpeed = double.MaxValue;
				currentSpeed = UpdateSpeed(d, currentSpeed, horses);
				Console.WriteLine("Case #{0}: {1:0.000000}", i, currentSpeed);
			}
		}

		private static double UpdateSpeed(long d, double currentSpeed, Horse[] horses)
		{
			foreach (var horse in horses)
			{
				var maxSpeed = d * horse.Speed * 1.0 / (d - horse.InitPos);
				if (currentSpeed > maxSpeed)
				{
					currentSpeed = maxSpeed;
				}
			}

			return currentSpeed;
		}

		private static IEnumerable<Horse> ReadKS(long n)
		{
			for (int i = 0; i < n; i++)
			{
				var ks = Console.ReadLine().Split(' ').Select(_ => long.Parse(_)).ToArray();
				var k = ks[0];
				var s = ks[1];
				yield return new Horse() { InitPos = k, Speed = s };
			}
		}
	}

	class Horse
	{
		public long InitPos { get; set; }
		public long Speed { get; set; }
	}
}
