using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Copyfile
{
    public class Ftp
    {
        public Ftp(string _username, string _psw, string _ip)
        {
            username = _username;
            psw = _psw;
            ip = _ip;
        }

        private string username;
        private string psw;
        private string ip;


        /// <summary>
        /// 新建目录 上一级必须先存在
        /// </summary>
        /// <param name="dirName"></param>
        public void MakeDir(string dirName)
        {
            try
            {
                string uri = $"ftp://{ip}{Path.DirectorySeparatorChar}{dirName}{Path.DirectorySeparatorChar}";
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                // 指定数据传输类型
                reqFTP.UseBinary = true;
                // ftp用户名和密码
                reqFTP.Credentials = new NetworkCredential(username, psw);
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("创建目录出错：" + ex);
            }
        }
        public void Connect(string dirName)
        {
            try
            {
                string uri = $"ftp://{ip}/{dirName}/";
                Console.WriteLine(uri);
                // 根据uri创建FtpWebRequest对象
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                // 指定数据传输类型
                reqFTP.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
                reqFTP.UseBinary = true;
                reqFTP.UsePassive = false;//表示连接类型为主动模式
                                          // ftp用户名和密码
                reqFTP.Credentials = new NetworkCredential(username, psw);
            }
            catch (Exception ex)
            {
                Console.WriteLine("连接目录出错：" + ex);
            }
        }

    }
}
