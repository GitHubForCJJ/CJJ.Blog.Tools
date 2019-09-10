using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Copyfile.Models;
using FastDev.Log;

namespace Copyfile
{
    public class CopyFile
    {
        /// <summary>
        /// 获取所有iis应用程序名称
        /// </summary>
        /// <returns>应用程序信息</returns>
        public List<AppView> GetIISAppInfos()
        {
            List<AppView> res = new List<AppView>();
            try
            {
                using (var mgr = new ServerManager(@"D:\applicationHost.config"))
                {
                    var sites = mgr.Sites;
                    if (mgr.Sites.Count() > 0)
                    {
                        foreach(var item in sites)
                        {
                            var av = new AppView
                            {
                                AppName = item.Name,
                                AppPhysicalPath = item.Applications["/"]?.VirtualDirectories["/"]?.PhysicalPath ?? string.Empty,
                                AppAlias = item.Name,
                                Id = item.Id.ToString(),
                                Status = item.State == ObjectState.Started ? 0 : 1,
                            };
                            res.Add(av);
                        }
                    }
                    
                }
                
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex, "GetIISAppInfos报错");
            }
            return res;
        }


        public void BackupIIS(List<AppView> sites)
        {
            string temppath = $"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}temp";
            DirectoryInfo dicinfo = new DirectoryInfo(temppath);
            if (dicinfo == null)
            {
                dicinfo.Create();
            }
            foreach(var item in sites)
            {
                if (item.Status > 0)
                {

                }
            }
        }





        #region old
        /// <summary>
        /// 递归复制文件夹
        /// </summary>
        /// <param name="dicpath"></param>
        public void Copy(string smain, string dmain, string dicpath)
        {
            var spath = Path.Combine(smain, dicpath);
            string dpath = Path.Combine(dmain, dicpath);
            int isdic = IsDic(spath);
            //文件
            if (isdic == 1)
            {
                Task.Run(() =>
                {
                    FileInfo info = new FileInfo(spath);
                    info.CopyTo(dpath, true);
                    Console.WriteLine($"当前资源文件{spath}复制成功");
                });
            }
            //文件夹
            else if (isdic == 2)
            {
                DirectoryInfo info = new DirectoryInfo(spath);
                if (!Directory.Exists(dpath))
                {
                    Directory.CreateDirectory(dpath);
                }
                var files = info.GetFiles();
                var dics = info.GetDirectories();
                if (dics.Count() > 0)
                {
                    foreach (var item in dics)
                    {
                        var name = item.FullName.Replace(smain, "");
                        Copy(smain, dmain, name);
                    }
                }
                if (files.Count() > 0)
                {
                    foreach (var item in files)
                    {
                        var name = item.FullName.Replace(smain, "");
                        Copy(smain, dmain, name);
                    }
                }

            }
            //其它
            else
            {
                Console.WriteLine($"当前资源文件或者文件夹不存在：{spath}");
                return;
            }


        }

        /// <summary>
        /// 1文件，2文件夹
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public int IsDic(string dic)
        {
            int res = 0;
            if (System.IO.Directory.Exists(dic))
            {
                res = 2;
            }
            else if (System.IO.File.Exists(dic))
            {
                res = 1;
            }
            return res;
        }

        #endregion

    }
}
