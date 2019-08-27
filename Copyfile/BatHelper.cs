using FastDev.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Copyfile
{
    public class BatHelper
    {
        /// <summary>
        /// 执行BAT批处理文件
        /// </summary>
        /// <param name="batPath"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static string ExcuteBatFile(string batPath, ref string errMsg)
        {

            if (errMsg == null) throw new ArgumentNullException("errMsg");
            string output="";
            try
            {
                using (Process process = new Process())
                {
                    FileInfo file = new FileInfo(batPath);
                    if (file.Directory != null)
                    {
                        process.StartInfo.WorkingDirectory = file.Directory.FullName;
                    }
                    process.StartInfo.FileName = batPath;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    process.Start();
                    process.WaitForExit();
                    //while (process.WaitForExit(0) == false)
                    //{
                    //    output+= process.StandardOutput.ReadLine() + "\r\n";
                    //}
                    output = process.StandardOutput.ReadToEnd();
                    errMsg = process.StandardError.ReadToEnd();
                    LogHelper.WriteLog(batPath + "：：：走完excute");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                LogHelper.WriteLog(ex, "ExcuteBatFile");
            }
    
            return output;
        }
        /// <summary>
        /// 创建bat
        /// </summary>
        /// <param name="sourcepath"></param>
        /// <param name="dstpath"></param>
        /// <param name="username"></param>
        /// <param name="userpsw"></param>
        /// <param name="batfilepath"></param>
        /// <returns></returns>
        public static bool BuildBatFile(string sourcepath, string dstpath, string username, string userpsw, out string batfilepath)
        {
            bool res = false;
            if (dstpath.IndexOf("%SystemDrive%") > -1)
            {
                dstpath = dstpath.Replace(@"%SystemDrive%", @"C:");
            }
            #region 处理文件
            batfilepath = string.Empty;
            string dic = AppDomain.CurrentDomain.BaseDirectory + "Temp" + Path.DirectorySeparatorChar;
            DirectoryInfo info = new DirectoryInfo(dic);
            if (!info.Exists)
            {
                info.Create();
            }
            string name = sourcepath.Replace(':','-').Replace(Path.DirectorySeparatorChar, '-') + DateTime.Now.ToString("yyyyMMdd HH-mm-ss")+@".bat";
            batfilepath = Path.Combine(dic, name);
            #endregion

            #region 创建文件
            FileInfo fileInfo = new FileInfo(batfilepath);
            FileStream fileStream = null;
            StreamWriter sw = null;
            if (!fileInfo.Exists)
            {
                fileStream = fileInfo.Create();

            }
            else
            {
                fileStream = fileInfo.OpenWrite();
            }
            try
            {
                sw = new StreamWriter(fileStream,Encoding.ASCII);
                sw.WriteLine(@"rem 实现远程拷贝的批处理代码");
                sw.WriteLine(@"rem 开始copy");
                sw.WriteLine($"@echo off");
                sw.WriteLine($"SETLOCAL ENABLEDELAYEDEXPANSION ");
                sw.WriteLine($"set srcdir=\"{sourcepath}\" ");
                sw.WriteLine($"set dstdir=\"{dstpath}\"");
                sw.WriteLine($"rem 使用共享文件");
                //登录共享文件夹
                sw.WriteLine($"net use \"{dstpath}\"  \"{userpsw}\" /user:{username}");
                //执行复制
                sw.WriteLine($" robocopy %srcdir% %dstdir% /mir /NC /NS /NFL /NDL /MT:128");
                sw.WriteLine($" echo  copystate:%errorlevel%");

                sw.WriteLine(" rem 结束copy");
                sw.Close();

                fileStream.Close();
                res = true;
            }
            catch(Exception ex)
            {
                res = false;
                batfilepath = "";
                LogHelper.WriteLog(ex, "BuildBatFile");
            }

            finally
            {
                sw.Dispose();
                fileStream.Dispose();
            }
            #endregion
            return res;
        }

        /// <summary>
        /// 解析bat执行完成后最后的copystate
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool ParseCode(int code)
        {
            bool res = false;
            switch (code)
            {
                case 0:
                    res = true;
                    break;
                case 1:
                    res = true;
                    break;
                case 3:
                    res = true;
                    break;
                case 5:
                    res = true;
                    break;
                case 6:
                    res = true;
                    break;
                default:
                    res = false;
                    break;

            }
            return res;
        }
    }
}
