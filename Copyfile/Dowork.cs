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
    /// <summary>
    /// 拷贝程序任务
    /// </summary>
    public class Dowork
    {
        /// <summary>
        /// 拷贝任务
        /// </summary>
        public static void WorkItem()
        {
            try
            {
                RunCopyNew runcopynew = new RunCopyNew();
                DateTime start1 = DateTime.Now;
                LogHelper.WriteLog(" 本次任务开始备份时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Logs/runsuccess");
                Console.WriteLine("  ");
                Console.WriteLine("  ");
                Console.WriteLine("********************----开始备份:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-----******");
                Console.WriteLine(start1.ToString("yyyy-MM-dd HH:mm:ss"));
                Console.WriteLine("**************************");
                Console.WriteLine("  ");
                Console.WriteLine("  ");

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

                //int count = c.Count / 4;
                //ThreadPool.SetMaxThreads(count, count);
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
            catch(Exception ex)
            {
                LogHelper.WriteLog(ex, "WorkItem错误");
            }

        }
    }
}
