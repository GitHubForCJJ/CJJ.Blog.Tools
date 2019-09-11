using Copyfile.Models;
using FastDev.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Configuration;
using System.Threading;

namespace Copyfile
{
    /// <summary>
    /// 先整体吧bat都同步创建出来，存一个全局变量，在多线程执行
    /// </summary>
    public class Runcopy
    {
        public static void Copyiis()
        {
            CopyFile copy = new CopyFile();
            try
            {
                var iiss = copy.GetIISAppInfos();
                if (iiss == null || iiss.Count() <= 0)
                {
                    Console.WriteLine("暂无站点");
                }

                //存放地址，共享盘文件夹位置

                //string desdic = $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}192.168.10.34{Path.DirectorySeparatorChar}go{Path.DirectorySeparatorChar}{DateTime.Now.ToString("yyyyMMdd")}{Path.DirectorySeparatorChar}";
                string desdic = ConfigurationManager.AppSettings["beifenmainpath"] ?? "";
                Directory.CreateDirectory(desdic);

                //复制iis配置文件
                var iisconfig = ConfigurationManager.AppSettings["iisprofix"] ?? "";
                FileInfo iisinfo = new FileInfo(iisconfig);
                string pa=Path.Combine(desdic, DateTime.Now.ToString("yyyyMMdd"),"iis");
                Directory.CreateDirectory(pa);
                iisinfo.CopyTo(pa + @"\iis.config", true);


                #region 复制iis网站信息

                DateTime start = DateTime.Now;

                ThreadPool.SetMaxThreads(6, 6);

                foreach (var item in iiss)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Runiis),item);
                }

                DateTime end = DateTime.Now;

                var totle = end - start;
                Console.WriteLine($"IIS统计总计用时：{totle.TotalSeconds }秒");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                LogHelper.WriteLog(ex, "iisexception");
            }



            #endregion
        }
        public static void Copyexe()
        {
            //存放地址，共享盘文件夹位置
            //string desdic = $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}192.168.10.34{Path.DirectorySeparatorChar}go{Path.DirectorySeparatorChar}{DateTime.Now.ToString("yyyyMMdd")}{Path.DirectorySeparatorChar}";
            string desdic = ConfigurationManager.AppSettings["beifenmainpath"] ?? "";
            Directory.CreateDirectory(desdic);

            #region 复制WCF服务文件

            //ProcessManager XML文件位置
            var proxmlpath = ConfigurationManager.AppSettings["exexmlprofix"] ?? "";

            //复制exe配置文件
            FileInfo exeinfo = new FileInfo(proxmlpath);
            string pa = Path.Combine(desdic, DateTime.Now.ToString("yyyyMMdd"), "exe");
            Directory.CreateDirectory(pa);
            exeinfo.CopyTo(pa + @"\ProcessInfo.xml", true);

            #region 复制programmanage程序文件

            ThreadPool.QueueUserWorkItem(new WaitCallback(RunProgram));


            #endregion

            DateTime start = DateTime.Now;
            try
            {

                XmlDocument xmldoc = new XmlDocument();
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;//忽略文档里面的注释
                XmlReader reader = XmlReader.Create(proxmlpath, settings);
                xmldoc.Load(reader);
                XmlNode prosnode = xmldoc.SelectSingleNode("ProcessInfos");


                var exelist = new List<ProcessView>();
                if (prosnode != null && prosnode.ChildNodes.Count > 0)
                {
                    foreach (XmlNode item in prosnode.ChildNodes)
                    {
                        var vitem = new ProcessView();
                        vitem.Path = item.Attributes[nameof(ProcessView.Path)].Value;
                        vitem.ID = item.Attributes[nameof(ProcessView.ID)].Value;
                        vitem.Name = item.Attributes[nameof(ProcessView.Name)].Value;
                        vitem.Desc = item.Attributes[nameof(ProcessView.Desc)].Value;

                        exelist.Add(vitem);
                    }
                }
                //暂无服务配置
                if (exelist.Count() <= 0)
                {
                    LogHelper.WriteLog($"暂无服务配置{DateTime.Now}{Environment.NewLine}", "copyexefilefail", LogLevel.H调试信息);
                }

                #region 执行exe拷贝
                ThreadPool.SetMaxThreads(5, 5);
                foreach (var item in exelist)
                {

                    ThreadPool.QueueUserWorkItem(new WaitCallback(Runexe), item);
                }

                DateTime end = DateTime.Now;

                var totle = end - start;
                Console.WriteLine($"exe统计总计用时：{totle.TotalSeconds }秒");
                #endregion

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex, "exe拷贝报错");
            }


            #endregion
        }

        /// <summary>
        /// 多线程执行iis项目拷贝
        /// </summary>
        /// <param name="itemobj"></param>
        private static void Runiis(object itemobj)
        {
            try
            {
                AppView item = (AppView)itemobj;
                string desdic = ConfigurationManager.AppSettings["beifenmainpath"] ?? "";
                DateTime start1 = DateTime.Now;
                Console.WriteLine("**************************");
                Console.WriteLine(start1.ToString("yyyy-MM-dd HH:mm:ss"));
                Console.WriteLine("**************************");
                #region 处理生成文件的路径
                var patharr = item.AppPhysicalPath.Split('\\');
                //最后文件夹名称
                var lastone = patharr.Last();
                //倒数第二次文件夹名称
                var lastsecend = patharr[patharr.Length - 2];
                var lasttow = $"{lastsecend}{Path.DirectorySeparatorChar}{lastone}";

                string dstname = Path.Combine(desdic, DateTime.Now.ToString("yyyyMMdd"),"iis", lasttow);
                #endregion
                if (!Directory.Exists(dstname))
                {
                    Directory.CreateDirectory(dstname);
                }
                string exceptfile = ConfigurationManager.AppSettings["exceptfile"] ?? "Logs Temp";
                string batpath = "";
                string username = ConfigurationManager.AppSettings["username"] ?? "Administrator";
                string userpwd = ConfigurationManager.AppSettings["userpwd"] ?? "cjj.123456";
                var bulidres = BatHelper.BuildBatFile(item.AppPhysicalPath, dstname, username, userpwd, exceptfile, out batpath);
                //执行bat
                string err = "";
                string res = string.Empty;
                if (bulidres)
                {
                    res = BatHelper.ExcuteBatFile(batpath, ref err);
                    Console.WriteLine(err);
                }
                //bat文件创建失败
                else
                {
                    Console.WriteLine($"{item.AppName}创建bat文件失败");
                    LogHelper.WriteLog($"{item.AppName}创建bat文件失败-复制失败{DateTime.Now}", "copyiisfilefail", LogLevel.H调试信息);
                }
                //失败
                if (string.IsNullOrEmpty(res))
                {
                    LogHelper.WriteLog($"{item.AppName}复制失败  bat无返回值-复制失败{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "copyiisfilefail", LogLevel.H调试信息);
                    //失败
                    Console.WriteLine($"{item.AppName}复制失败  bat无返回值");
                }

                Console.WriteLine("**********************************************");
                Console.WriteLine("**********************************************");
                Console.WriteLine("                      拷贝信息                       ");
                Console.WriteLine(res);
                Console.WriteLine("");
                Console.WriteLine("**********************************************");
                Console.WriteLine("**********************************************");

                var arr = res.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                //解析最后一个copystate，和传输速度
                var statestring = arr[arr.Length - 1];
                int state = -1;
                if (statestring.IndexOf("copystate:") > -1)
                {
                    state = Convert.ToInt32(statestring.Replace("copystate:", ""));
                    bool suss = BatHelper.ParseCode(state);
                    string msg = suss ? "成功" : "失败";
                    //执行bat但是失败了
                    if (!suss)
                    {
                        LogHelper.WriteLog($"{item.AppName}执行未成功 执行bat返回状态为未成功{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "copyiisfilefail", LogLevel.H调试信息);
                    }
                    else
                    {
                        LogHelper.WriteLog($"{item.AppName}--执行成功");
                        DateTime end1 = DateTime.Now;
                        Console.WriteLine("**************************");
                        Console.WriteLine(end1.ToString("yyyy-MM-dd HH:mm:ss"));
                        Console.WriteLine("**************************");
                        var totle1 = end1 - start1;
                        Console.WriteLine($"{item.AppName}用时：{totle1.TotalSeconds }秒");
                    }
                    Console.WriteLine($"{item.AppName}复制{msg}");
                }
                //执行bat后未返回errorlevel状态
                else
                {
                    LogHelper.WriteLog($"{item.AppName}复制失败  bat返回值未包含copystate信息-复制失败{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "copyiisfilefail", LogLevel.H调试信息);
                    Console.WriteLine($"{item.AppName}复制失败  bat返回值未包含copystate信息");
                    //失败
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex, "iisitemexcption");
            }
        }

        /// <summary>
        /// 多线程执行exe拷贝
        /// </summary>
        /// <param name="itemobj"></param>
        private static void Runexe(object itemobj)
        {
            ProcessView item = (ProcessView)itemobj;
            string desdic = ConfigurationManager.AppSettings["beifenmainpath"] ?? "";
            DateTime start1 = DateTime.Now;
            Console.WriteLine("**************************");
            Console.WriteLine(start1.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.WriteLine("**************************");
            var na = string.Empty;
            if (item.Name.IndexOf(".") > -1)
            {
                na = item.Name.Replace(".", "");
            }
            else
            {
                na = item.Name;
            }

            #region 处理生成文件的路径
            var patharr = item.Path.Split('\\');
            //最后文件夹名称
            var lastone = patharr.Last();
            //倒数第二次文件夹名称
            var lastsecend = patharr[patharr.Length - 2];
            var lasttow = $"{lastsecend}{Path.DirectorySeparatorChar}{lastone}";

            string dstname = Path.Combine(desdic, DateTime.Now.ToString("yyyyMMdd"), "exe", lasttow);
            #endregion

            if (!Directory.Exists(dstname))
            {
                Directory.CreateDirectory(dstname);
            }
            string exceptfile = ConfigurationManager.AppSettings["exceptfile"] ?? "Logs Temp";
            string batpath = "";
            string username= ConfigurationManager.AppSettings["username"] ?? "Administrator";
            string userpwd = ConfigurationManager.AppSettings["userpwd"] ?? "cjj.123456";
            var bulidres = BatHelper.BuildBatFile(item.Path, dstname, username, userpwd, exceptfile, out batpath);
            //执行bat
            string err = "";
            string res = string.Empty;
            if (bulidres)
            {
                res = BatHelper.ExcuteBatFile(batpath, ref err);
                Console.WriteLine(err);
            }
            //bat文件创建失败
            else
            {
                Console.WriteLine($"{item.Desc}创建bat文件失败");
                LogHelper.WriteLog($"{item.Desc}创建bat文件失败-复制失败{DateTime.Now}", "copyiisfilefail", LogLevel.H调试信息);
            }
            //失败
            if (string.IsNullOrEmpty(res))
            {
                LogHelper.WriteLog($"{item.Desc}复制失败  bat无返回值-复制失败{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "copyiisfilefail", LogLevel.H调试信息);
                //失败
                Console.WriteLine($"{item.Desc}复制失败  bat无返回值");
            }
            Console.WriteLine("**********************************************");
            Console.WriteLine("**********************************************");
            Console.WriteLine("                      拷贝信息                       ");
            Console.WriteLine(res);
            Console.WriteLine("");
            Console.WriteLine("**********************************************");
            Console.WriteLine("**********************************************");
            var arr = res.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            //解析最后一个copystate，和传输速度
            var statestring = arr[arr.Length - 1];
            int state = -1;
            if (statestring.IndexOf("copystate:") > -1)
            {
                state = Convert.ToInt32(statestring.Replace("copystate:", ""));
                bool suss = BatHelper.ParseCode(state);
                string msg = suss ? "成功" : "失败";
                //执行bat但是失败了
                if (!suss)
                {
                    LogHelper.WriteLog($"{item.Desc}执行未成功 执行bat返回状态为未成功{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "copyiisfilefail", LogLevel.H调试信息);
                }
                else
                {
                    LogHelper.WriteLog($"{item.Desc}--执行成功");
                }
                DateTime end1 = DateTime.Now;
                Console.WriteLine("**************************");
                Console.WriteLine(end1.ToString("yyyy-MM-dd HH:mm:ss"));
                Console.WriteLine("**************************");
                var totle1 = end1 - start1;
                Console.WriteLine($"{item.Desc}用时：{totle1.TotalSeconds }秒");
                Console.WriteLine($"{item.Desc}复制{msg}");
            }
            //执行bat后未返回errorlevel状态
            else
            {
                LogHelper.WriteLog($"{item.Desc}复制失败  bat返回值未包含copystate信息-复制失败{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "copyiisfilefail", LogLevel.H调试信息);
                Console.WriteLine($"{item.Desc}复制失败  bat返回值未包含copystate信息");
                //失败
            }
            //});
        }

        /// <summary>
        /// 复制programmanager程序
        /// </summary>
        private static void RunProgram(object data)
        {
            DateTime start = DateTime.Now;
            string desdic = ConfigurationManager.AppSettings["beifenmainpath"] ?? "";
            string programpath = ConfigurationManager.AppSettings["programpath"] ?? "";

            string exceptfile = ConfigurationManager.AppSettings["exceptfile"] ?? "Logs Temp";
            string batpath = "";
            string username = ConfigurationManager.AppSettings["username"] ?? "Administrator";
            string userpwd = ConfigurationManager.AppSettings["userpwd"] ?? "cjj.123456";

            #region 处理生成文件的路径
            var patharr = programpath.Split('\\');
            //最后文件夹名称
            var lastone = patharr.Last();
            //倒数第二次文件夹名称
            var lastsecend = patharr[patharr.Length - 2];
            var lasttow = $"{lastsecend}{Path.DirectorySeparatorChar}{lastone}";

            string dstname = Path.Combine(desdic, DateTime.Now.ToString("yyyyMMdd"), "exe", lasttow);
            #endregion

            var bulidres = BatHelper.BuildBatFile(programpath, dstname, username, userpwd, exceptfile, out batpath);
            //执行bat
            string err = "";
            string res = string.Empty;
            if (bulidres)
            {
                res = BatHelper.ExcuteBatFile(batpath, ref err);
                Console.WriteLine(err);
            }
            //bat文件创建失败
            else
            {
                Console.WriteLine($"创建bat文件失败");
                LogHelper.WriteLog($"programmanager程序-创建bat文件失败-复制失败{DateTime.Now}", "copyiisfilefail", LogLevel.H调试信息);
            }
            //失败
            if (string.IsNullOrEmpty(res))
            {
                LogHelper.WriteLog($"programmanager程序----复制失败  bat无返回值-复制失败{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "copyiisfilefail", LogLevel.H调试信息);
                //失败
                Console.WriteLine($"programmanager程序----复制失败  bat无返回值");
            }

            Console.WriteLine("**********************************************");
            Console.WriteLine("**********************************************");
            Console.WriteLine("                      拷贝信息                       ");
            Console.WriteLine(res);
            Console.WriteLine("");
            Console.WriteLine("**********************************************");
            Console.WriteLine("**********************************************");

            var arr = res.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            //解析最后一个copystate，和传输速度
            var statestring = arr[arr.Length - 1];
            int state = -1;
            if (statestring.IndexOf("copystate:") > -1)
            {
                state = Convert.ToInt32(statestring.Replace("copystate:", ""));
                bool suss = BatHelper.ParseCode(state);
                string msg = suss ? "成功" : "失败";
                //执行bat但是失败了
                if (!suss)
                {
                    LogHelper.WriteLog($"programmanager程序----执行未成功 执行bat返回状态为未成功{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "copyiisfilefail", LogLevel.H调试信息);
                }
                else
                {
                    LogHelper.WriteLog($"programmanager程序------执行成功");
                    DateTime end1 = DateTime.Now;
                    Console.WriteLine("**************************");
                    Console.WriteLine(end1.ToString("yyyy-MM-dd HH:mm:ss"));
                    Console.WriteLine("**************************");
                    var totle1 = end1 - start;
                    Console.WriteLine($"programmanager程序----用时：{totle1.TotalSeconds }秒");
                }
                Console.WriteLine($"programmanager程序----复制{msg}");
            }
            //执行bat后未返回errorlevel状态
            else
            {
                LogHelper.WriteLog($"programmanager程序----复制失败  bat返回值未包含copystate信息-复制失败{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "copyiisfilefail", LogLevel.H调试信息);
                Console.WriteLine($"programmanager程序----复制失败  bat返回值未包含copystate信息");
                //失败
            }
        }


        //public static void CheckFileNum()
        //{
        //    try
        //    {
        //        string desdic = $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}192.168.10.34{Path.DirectorySeparatorChar}go{Path.DirectorySeparatorChar}";
        //        DirectoryInfo directory = new DirectoryInfo(desdic);
        //        var dirinfos = directory.GetDirectories().OrderByDescending(x=>x.CreationTime).ToList();

        //    }
        //    catch(Exception ex)
        //    {

        //    }


        //}
    }
}
