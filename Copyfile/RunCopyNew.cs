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
using System.Collections.Concurrent;

namespace Copyfile
{
    /// <summary>
    /// 先生成所有的bat执行文件，在多线程方式执行
    /// </summary>
    public class RunCopyNew
    {
        private object objstate = new object();
        public RunCopyNew() { }
        public RunCopyNew(ConcurrentDictionary<string, int> data)
        {
            CDkeyValues = data;
        }
        private ConcurrentDictionary<string, int> cdkeyvalues;
        public ConcurrentDictionary<string, int> CDkeyValues
        {
            get
            {
                return cdkeyvalues;
            }
            private set
            {

                cdkeyvalues = value;

            }
        }

        public List<string> Buildiisbat()
        {
            List<string> reslist = new List<string>();
            CopyFile copy = new CopyFile();
            try
            {
                var iiss = copy.GetIISAppInfos();
                if (iiss == null || iiss.Count() <= 0)
                {
                    Console.WriteLine("暂无站点");
                }

                //存放地址，共享盘文件夹位置
                string desdic = ConfigurationManager.AppSettings["beifenmainpath"] ?? "";
                Directory.CreateDirectory(desdic);

                //复制iis配置文件
                var iisconfig = ConfigurationManager.AppSettings["iisprofix"] ?? "";
                FileInfo iisinfo = new FileInfo(iisconfig);
                string pa = Path.Combine(desdic, DateTime.Now.ToString("yyyyMMdd"), "iis");
                Directory.CreateDirectory(pa);
               // iisinfo.CopyTo(pa + @"\iis.config", true);

                #region 复制iis网站信息


                foreach (var item in iiss)
                {
                    #region 处理生成文件的路径
                    var patharr = item.AppPhysicalPath.Split('\\');
                    //最后文件夹名称
                    var lastone = patharr.Last();
                    //倒数第二次文件夹名称
                    var lastsecend = patharr[patharr.Length - 2];
                    var lasttow = $"{lastsecend}{Path.DirectorySeparatorChar}{lastone}";

                    string dstname = Path.Combine(desdic, DateTime.Now.ToString("yyyyMMdd"), "iis", lasttow);
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
                    if (bulidres)
                    {
                        reslist.Add(batpath);
                    }
                    else
                    {
                        LogHelper.WriteLog($"IIS程序 {item.AppName}创建失败", "createbatfailture");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                LogHelper.WriteLog(ex, "iisexception");
            }
            return reslist;

            #endregion
        }
        public List<string> Buildexebat()
        {
            List<string> reslist = new List<string>();
            try
            {
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

                string programpath = ConfigurationManager.AppSettings["programpath"] ?? "";
                string exceptfile = ConfigurationManager.AppSettings["exceptfile"] ?? "Logs Temp";
                string username = ConfigurationManager.AppSettings["username"] ?? "Administrator";
                string userpwd = ConfigurationManager.AppSettings["userpwd"] ?? "cjj.123456";
                string batpath = "";

                #region 创建拷贝programmanage程序文件bat

                var patharrp = programpath.Split('\\');
                //最后文件夹名称
                var lastonep = patharrp.Last();
                //倒数第二次文件夹名称
                var lastsecendp = patharrp[patharrp.Length - 2];
                var lasttowp = $"{lastsecendp}{Path.DirectorySeparatorChar}{lastonep}";

                string dstnamep = Path.Combine(desdic, DateTime.Now.ToString("yyyyMMdd"), "exe", lasttowp);
                var bulidresp = BatHelper.BuildBatFile(programpath, dstnamep, username, userpwd, exceptfile, out batpath);
                if (bulidresp)
                {
                    reslist.Add(batpath);
                }
                else
                {
                    LogHelper.WriteLog($"创建拷贝programmanage程序文件bat失败", "createbatfailture");
                }

                #endregion

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

                #region 创建exe bat
                foreach (var item in exelist)
                {
                    batpath = "";
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

                    var bulidres = BatHelper.BuildBatFile(item.Path, dstname, username, userpwd, exceptfile, out batpath);
                    if (bulidres)
                    {
                        reslist.Add(batpath);
                    }
                    else
                    {
                        LogHelper.WriteLog($"exe程序 {item.Name}创建失败", "createbatfailture");
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex, "创建exe bat错误");
            }

            return reslist;

            #endregion
        }

        /// <summary>
        /// 多线程执行iis项目拷贝  waitcallback
        /// </summary>
        /// <param name="itemobj"></param>
        public void Runbat(object batpathdata)
        {
            string batpath = batpathdata.ToString();
            int docount = 0;
            try
            {
                DateTime start1 = DateTime.Now;
                //Console.WriteLine("********************----开始item-----******");
                //Console.WriteLine(start1.ToString("yyyy-MM-dd HH:mm:ss"));
                //Console.WriteLine("**************************");


                string err = "";
                string res = string.Empty;

                res = BatHelper.ExcuteBatFile(batpath, ref err);
                Console.WriteLine(err);

                string name = batpath.Split('\\').Last();
                //失败
                if (string.IsNullOrEmpty(res))
                {
                    LogHelper.WriteLog($"{name}复制失败  bat无返回值-复制失败{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "Logs/copyiisfilefail", LogLevel.H调试信息);
                    //失败
                    Console.WriteLine($"{name}复制失败  bat无返回值");
                }

                Console.WriteLine("           ");
                Console.WriteLine("           ");
                Console.WriteLine("           ");
                Console.WriteLine("**********************************************");
                Console.WriteLine("**********************************************");
                Console.WriteLine("           ");
                Console.WriteLine("           ");
                Console.WriteLine("           ");
                Console.WriteLine("                      拷贝信息                       ");
                Console.WriteLine("           ");
                Console.WriteLine("           ");
                Console.WriteLine("           ");
                Console.WriteLine(res);
                Console.WriteLine("           ");
                Console.WriteLine("           ");
                Console.WriteLine("           ");
                Console.WriteLine("*********************拷贝信息结束*************************");
                Console.WriteLine("**********************************************");
                Console.WriteLine("           ");
                Console.WriteLine("           ");
                Console.WriteLine("           ");

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
                        LogHelper.WriteLog($"{name}执行未成功 执行bat返回状态为未成功{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "Logs/copyiisfilefail", LogLevel.H调试信息);
                    }
                    else
                    {
                        lock (objstate)
                        {
                            CDkeyValues[batpath] = 1;
                            docount = CDkeyValues.Count(x => x.Value > 0);
                        }

                        LogHelper.WriteLog($"{name}--执行成功");
                        DateTime end1 = DateTime.Now;
                        Console.WriteLine("**************************");
                        Console.WriteLine(end1.ToString("yyyy-MM-dd HH:mm:ss"));
                        Console.WriteLine("********************----结束---******");
                        var totle1 = end1 - start1;
                        Console.WriteLine($"{name}用时：{totle1.TotalSeconds }秒");
                    }
                    Console.WriteLine($"{name}复制{msg}");
                }
                //执行bat后未返回errorlevel状态
                else
                {
                    LogHelper.WriteLog($"{name}复制失败  bat返回值未包含copystate信息-复制失败{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "Logs/copyiisfilefail", LogLevel.H调试信息);
                    Console.WriteLine($"{name}复制失败  bat返回值未包含copystate信息");
                    //失败
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex, "run iis bat 错误");
            }
            finally
            {

                Console.WriteLine("docount---" + docount);
                LogHelper.WriteLog("docount---:" + docount, "Logs/dcount");
                if (docount > 0 && docount == CDkeyValues.Count)
                {
                    LogHelper.WriteLog(" 本次任务执行完成:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") ,"Logs/runsuccess");
                    Console.WriteLine("      ");
                    Console.WriteLine("      ");
                    Console.WriteLine("      ");
                    Console.WriteLine("      ");
                    Console.Out.WriteLine("**********************************************");
                    Console.Out.WriteLine("**********************************************");
                    Console.Out.WriteLine("**********************************************");
                    Console.Out.WriteLine("         本次任务执行完成" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "             ");
                    Console.Out.WriteLine("**********************************************");
                    Console.Out.WriteLine("**********************************************");
                    Console.Out.WriteLine("**********************************************");
                    Console.WriteLine("      ");
                    Console.WriteLine("      ");
                    Console.WriteLine("      ");
                    Console.WriteLine("      ");
                }
            }
            Console.WriteLine("    ");
            Console.WriteLine("    ");
        }

        /// <summary>
        /// 多线程执行exe拷贝 waitcallback
        /// </summary>
        /// <param name="itemobj"></param>
        //public void Runexe(object itemobj)
        //{
        //    try
        //    {
        //        DateTime start1 = new DateTime();
        //        Console.WriteLine("**************************");
        //        Console.WriteLine("开始时间:"+start1.ToString("yyyy-MM-dd HH:mm:ss"));
        //        Console.WriteLine("**************************");

        //        //执行bat
        //        string err = "";
        //        string res = string.Empty;
        //        string batpath = itemobj.ToString();

        //        res = BatHelper.ExcuteBatFile(batpath, ref err);
        //        Console.WriteLine(err);
        //        string name = batpath.Split('\\').Last();
        //        //失败
        //        if (string.IsNullOrEmpty(res))
        //        {
        //            LogHelper.WriteLog($"{name}复制失败  bat无返回值-复制失败{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "copyiisfilefail", LogLevel.H调试信息);
        //            //失败
        //            Console.WriteLine($"{name}复制失败  bat无返回值");
        //        }
        //        Console.WriteLine("           ");
        //        Console.WriteLine("           ");
        //        Console.WriteLine("           ");
        //        Console.WriteLine("**********************************************");
        //        Console.WriteLine("**********************************************");
        //        Console.WriteLine("           ");
        //        Console.WriteLine("           ");
        //        Console.WriteLine("           ");
        //        Console.WriteLine("                      拷贝信息                       ");
        //        Console.WriteLine("           ");
        //        Console.WriteLine(res);
        //        Console.WriteLine("           ");
        //        Console.WriteLine("           ");
        //        Console.WriteLine("           ");
        //        Console.WriteLine("**********************************************");
        //        Console.WriteLine("**********************************************");
        //        Console.WriteLine("           ");
        //        Console.WriteLine("           ");
        //        Console.WriteLine("           ");
        //        var arr = res.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        //        //解析最后一个copystate，和传输速度
        //        var statestring = arr[arr.Length - 1];
        //        int state = -1;
        //        if (statestring.IndexOf("copystate:") > -1)
        //        {
        //            state = Convert.ToInt32(statestring.Replace("copystate:", ""));
        //            bool suss = BatHelper.ParseCode(state);
        //            string msg = suss ? "成功" : "失败";
        //            //执行bat但是失败了
        //            if (!suss)
        //            {
        //                LogHelper.WriteLog($"{name}执行未成功 执行bat返回状态为未成功{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "copyiisfilefail", LogLevel.H调试信息);
        //            }
        //            else
        //            {
        //                LogHelper.WriteLog($"{name}--执行成功");
        //            }
        //            DateTime end1 = DateTime.Now;
        //            Console.WriteLine("**************************");
        //            Console.WriteLine("结束时间："+end1.ToString("yyyy-MM-dd HH:mm:ss"));
        //            Console.WriteLine("**************************");
        //            var totle1 = end1 - start1;
        //            Console.WriteLine($"{name}用时：{totle1.TotalSeconds }秒");
        //            Console.WriteLine($"{name}复制{msg}");
        //        }
        //        //执行bat后未返回errorlevel状态
        //        else
        //        {
        //            LogHelper.WriteLog($"{name}复制失败  bat返回值未包含copystate信息-复制失败{DateTime.Now}{Environment.NewLine} bat文件路径:{batpath}", "copyiisfilefail", LogLevel.H调试信息);
        //            Console.WriteLine($"{name}复制失败  bat返回值未包含copystate信息");
        //            //失败
        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        LogHelper.WriteLog(ex, "run exe bat 错误");
        //    }

        //}

    }
}
