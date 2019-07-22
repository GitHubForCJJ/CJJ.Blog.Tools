using BaseWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CJJ.Blog.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始");
            Run();


            string key = string.Empty;
            while (key != "exit")
            {
                if (!string.IsNullOrEmpty(key)) { Console.WriteLine("非退出命令自动忽略"); }
                key = Console.ReadLine();
            }
        }

        public static void Run()
        {
            new NewsJob().StartWork(1);
        }
    }
}
