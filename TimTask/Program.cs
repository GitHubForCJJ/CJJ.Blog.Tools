using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimTask
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime start = DateTime.Parse("2013-06-06");
            var totle = DateTime.Now - start;
            Console.WriteLine(totle.TotalDays);
            Console.WriteLine(totle.TotalMinutes);
            Console.WriteLine(totle.TotalSeconds);
            Console.WriteLine();
            Console.ReadLine();
        }
    }
}
