using FastDev.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Copyfile
{
    public class Dowork
    {
        public static void WorkItem()
        {
            RunCopyNew runcopynew = new RunCopyNew();

            var a = runcopynew.Buildiisbat();
            var b = runcopynew.Buildexebat();
            List<string> c = new List<string>();
            c.AddRange(a);
            c.AddRange(b);
            LogHelper.WriteLog($"IIS程序个数{a.Count};EXE程序个数{b.Count};总个数{c.Count}", "Logs/runsuccess");

            Console.WriteLine(string.Join(System.Environment.NewLine, c).ToString());
            Console.WriteLine("      ");
            Console.WriteLine("      ");
            Console.WriteLine("      ");
            Console.WriteLine("      ");
            Console.Out.WriteLine("**********************************************");
            Console.Out.WriteLine("**********************************************");
            Console.Out.WriteLine("**********************************************");
            Console.Out.WriteLine("                 bat文件创建完成             ");
            Console.Out.WriteLine("**********************************************");
            Console.Out.WriteLine("**********************************************");
            Console.Out.WriteLine("**********************************************");
            Console.WriteLine("      ");
            Console.WriteLine("      ");
            Console.WriteLine("      ");
            Console.WriteLine("      ");

            #region 多线程执行bat

            ConcurrentDictionary<string, int> keyValues = new ConcurrentDictionary<string, int>();

            var dic = new Dictionary<string, int>();
            for (int i = 0; i < c.Count; i++)
            {
                keyValues.TryAdd(c[i], 0);
            }

            int count = c.Count / 4;
            ThreadPool.SetMaxThreads(count, count);
            var keylist = keyValues.Keys;
            RunCopyNew runcopynew2 = new RunCopyNew(keyValues);
            foreach (var key in keylist)
            {
                try
                {
                    ThreadPool.QueueUserWorkItem(runcopynew2.Runbat, key);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex, "异步item错误");
                }
            }

            #endregion
        }
    }
}
