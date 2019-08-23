using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Copyfile
{
    class Program
    {
        static void Main(string[] args)
        {
            //资源主路径
            string sourcepath = @"\\192.168.10.34\";
            //目标主路径
            string destpath = @"E:\ceshifile\";
            //待复制的文件夹
            string inpath = @"go\ceshi";
            try
            {
                //var status = connectState(@"\\172.18.20.121", @"kin\wanghaidong", "123456");
                DateTime start = DateTime.Now;

                CopyFile copy = new CopyFile();
                var a = copy.GetIISAppInfos();

                //CopyFile(sourcepath, destpath, inpath);

                //var a = FtpHelper.GetDirctory(@"ces");

                DateTime end = DateTime.Now;
                var totle = end - start;
                Console.WriteLine($"总计用时：{totle.TotalSeconds }M");
            }
            catch (Exception ex)
            {

            }
            Console.ReadLine();
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
