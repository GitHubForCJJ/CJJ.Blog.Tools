using Copyfile.Common;
using Copyfile.Models;
using FastDev.Configer;
using FastDev.Log;
using FastDev.XmlHelper;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Copyfile
{
    class Program
    {

        /// <summary>
        /// 自动copy定时配置
        /// </summary>
        public static readonly string AutoCopyTime = ConfigHelper.GetConfigToString("AutoCopyTime");

        static void Main(string[] args)
        {
            ///
            ///
            try
            {
                #region 检查对IIS配置文件和program的xml配置的访问权限

                var proxmlpath = ConfigurationManager.AppSettings["exexmlprofix"] ?? "";

                FileInfo xml = new FileInfo(proxmlpath);

                if (!xml.Exists)
                {
                    Console.WriteLine("请检查 XML配置路径以及访问权限");
                    return;
                }
                #endregion

                StartTask();
                Console.WriteLine("**********************************************");
                Console.WriteLine("**********************************************");
                Console.WriteLine("**********************************************");
                Console.WriteLine("              DZXZ   19服务器 iis、exe备份程序启动              ");
                Console.WriteLine("**********************************************");
                Console.WriteLine("**********************************************");
                Console.WriteLine("**********************************************");
                Console.WriteLine("      ");
                Console.WriteLine("      ");
                Console.WriteLine("      ");
                Console.WriteLine("      ");

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex, "copy任务错误最外层catch");
            }

            Console.Out.WriteLine("");
            Console.WriteLine("         若需退出请输入 exit 按回车退出...\r\n");
            string userCommand = string.Empty;
            while (userCommand != "exit")
            {
                if (string.IsNullOrEmpty(userCommand) == false)
                    Console.WriteLine("                非退出指令,自动忽略...");
                userCommand = Console.ReadLine();
            }
        }


        private static void StartTask()
        {
            /// <summary>
            /// The scheduler
            /// </summary>
            IScheduler _scheduler = null;

            ISchedulerFactory sf = new StdSchedulerFactory();

            if (_scheduler == null)
            {
                _scheduler = sf.GetScheduler();
            }

            _scheduler.Start();

            #region 自动备份IIS、exe任务
            //创建任务对象
            IJobDetail job1 = JobBuilder.Create<EventCopyJob>().WithIdentity("job1", "group1").Build();
            //创建触发器
            ITrigger trigger1 = TriggerBuilder.Create().WithIdentity("trigger1", "group1").StartNow().WithCronSchedule(AutoCopyTime).Build();
            #endregion

            _scheduler.ScheduleJob(job1, trigger1);

        }




        /// <summary>
                /// 连接远程共享文件夹
                /// </summary>
                /// <param name="path">远程共享文件夹的路径</param>
                /// <param name="userName">用户名</param>
                /// <param name="passWord">密码</param>
                /// <returns></returns>
        public static bool connectState(string path, string userName, string passWord)
        {
            bool Flag = false;
            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string dosLine = "net use " + path + " " + passWord + " /user:" + userName;
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                while (!proc.HasExited)
                {
                    proc.WaitForExit(1000);
                }
                string errormsg = proc.StandardError.ReadToEnd();
                proc.StandardError.Close();
                if (string.IsNullOrEmpty(errormsg))
                {
                    Flag = true;
                }
                else
                {
                    throw new Exception(errormsg);
                }
            }
            catch (Exception ex)
            {
                //throw ex;
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
            return Flag;
        }

        /// <summary>
        /// 判断是否IP协议字符串
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public static string ValidIPAddress(string IP)
        {
            var res = "Neither";

            if (IP.Contains("."))
            {
                var sorc = IP.Split(new char[] { '.' });
                if (sorc.Length != 4)
                {
                    return res;
                }
                try
                {
                    for (var i = 0; i < sorc.Length; i++)
                    {
                        //判断是否有 01这样的错误
                        if (sorc[i].Length > 1 && sorc[i].StartsWith("0"))
                        {
                            return res;
                        }
                        var num = Convert.ToInt32(sorc[i]);
                        if (num < 0 || num > 255)
                        {
                            return res;
                        }
                    }
                    return "IPv4";
                }
                catch (Exception ex)
                {
                    return "报错";
                }
            }
            else if (IP.Contains(":"))
            {
                var sorc = IP.Split(new char[] { ':' });
                if (sorc.Length != 8)
                {
                    return res;
                }
                try
                {
                    for (var i = 0; i < sorc.Length; i++)
                    {
                        var sp = sorc[i].ToCharArray();
                        var len = sp.Length;
                        for (var j = 0; j < len; j++)
                        {
                            char c = sp[j];
                            if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                            {
                                return res;
                            }
                        }


                    }
                    return "IPv6";
                }
                catch
                {
                    return "报错2";
                }
            }

            else
            {
                return res;
            }
        }
    }
}
