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

            //ProcessStartInfo pro = new System.Diagnostics.ProcessStartInfo("cmd.exe");
            //pro.UseShellExecute = false;
            //pro.RedirectStandardOutput = true;
            //pro.RedirectStandardError = true;
            //pro.CreateNoWindow = true;
            //pro.FileName = batPath;
            //pro.Arguments = "argname";
            ////pro.WorkingDirectory = System.Environment.CurrentDirectory;
            //System.Diagnostics.Process proc = System.Diagnostics.Process.Start(pro);
            //System.IO.StreamReader sOut = proc.StandardOutput;
            //proc.Close();
            //string results = sOut.ReadToEnd().Trim(); //回显内容  
            //sOut.Close();
            ////string[] values = results.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            ////return values[values.Length - 1];
            //return results;

            #region lod
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
                    process.Start();
                    // process.WaitForExit();
                    output = process.StandardOutput.ReadToEnd();
                    errMsg = process.StandardError.ReadToEnd();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
    
            return output;
            #endregion
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
            #region 处理文件
            batfilepath = string.Empty;
            string dic = AppDomain.CurrentDomain.BaseDirectory + "Temp" + Path.DirectorySeparatorChar;
            DirectoryInfo info = new DirectoryInfo(dic);
            if (!info.Exists)
            {
                info.Create();
            }
            string name = sourcepath.Replace(':','-').Replace(Path.DirectorySeparatorChar, '-') + DateTime.Now.ToString("yyyyMMdd")+@".bat";
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
                sw.WriteLine($" robocopy %srcdir% %dstdir% /e /s /xo ");
                sw.WriteLine("pause");
                sw.WriteLine(" rem 结束copy");
                sw.Close();

                fileStream.Close();
                res = true;
            }
            catch { res = false; batfilepath = ""; }

            finally
            {
                sw.Dispose();
                fileStream.Dispose();
            }
            #endregion
            return res;
        }
    }
}
