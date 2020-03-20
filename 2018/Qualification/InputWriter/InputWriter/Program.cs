using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputWriter
{
	class Program
	{
		static void Main(string[] args)
		{
			var rnd = new Random();
			using (var s = File.OpenWrite("./input.txt"))
			using (var i = new StreamWriter(s))
			{
				i.WriteLine("1");
				i.WriteLine("10000");
				for (int j = 0; j < 10000; j++)
				{
					var x = rnd.Next(1, 1000000000);
					i.Write(x + " ");
				}	 
			}
		}
	}
}
