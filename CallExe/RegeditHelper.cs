using System;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace CJJ.Blog.Tools.CallExe
{
    public class RegeditHelper
    {
        /// <summary>
        /// 注册协议
        /// </summary>
        /// <param name="Root_Key">根节点</param>
        /// <param name="file_application_path">应用程序路径</param>
        /// <param name="file_application_ico">应用程序打开图标,可选值</param>
        /// <returns></returns>
        public static bool RegeditAdd(string Root_Key, string file_application_path, string file_application_ico)
        {
            //获取注册表HKEY_CLASSES_ROOT
            RegistryKey reg_ClassRoot = Registry.ClassesRoot;

            try
            {
                RegistryKey reg_key = reg_ClassRoot.OpenSubKey(Root_Key, true);
                if (reg_key == null)
                {
                    //创建子节点HKEY_CLASSES_ROOT\tpswftest
                    RegistryKey reg_sjbs = reg_ClassRoot.CreateSubKey(Root_Key);
                    //添加默认项
                    reg_sjbs.SetValue("", "URL: " + Root_Key + " Protocol Handler");
                    //协议别名
                    reg_sjbs.SetValue("URL Protocol", file_application_path);
                    //创建[HKEY_CLASSES_ROOT\tpswftest\DefaultIcon]
                    RegistryKey reg_DefaultIcon = reg_sjbs.CreateSubKey("DefaultIcon");
                    if (!String.IsNullOrEmpty(file_application_ico))
                    {
                        //设置自定义图标
                        reg_DefaultIcon.SetValue("", file_application_ico);
                    }
                    else
                    {
                        //设置系统定义图标
                        reg_DefaultIcon.SetValue("", file_application_path + ",1");
                    }
                    //创建呼出处理程序[HKEY_CLASSES_ROOT\tpswftest\shell\open\command]
                    RegistryKey reg_command = reg_sjbs.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command");
                    //%1 表示传递的参数，再次%1表示调用处显示链接文本
                    reg_command.SetValue("", "\"" + file_application_path + "\" \"%1\"");
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally { reg_ClassRoot.Close(); }
        }

        /// <summary>
        /// 删除协议
        /// </summary>
        /// <param name="Root_Key">根节点</param>
        /// <returns></returns>
        public static bool RegeditDelete(string Root_Key)
        {
            //获取注册表HKEY_CLASSES_ROOT
            RegistryKey reg_ClassRoot = Registry.ClassesRoot;
            try
            {
                //获取注册表[HKEY_CLASSES_ROOT\tpswftest]项
                RegistryKey reg_sjbs = reg_ClassRoot.OpenSubKey(Root_Key, true);
                if (reg_sjbs != null)
                {
                    reg_ClassRoot.DeleteSubKeyTree(Root_Key);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                //添加错误日志
                return false;
            }
            finally { reg_ClassRoot.Close(); }
        }

        /// <summary>
        /// 处理页面传回的参数的非法字符
        /// </summary>
        /// <param name="sParameterValue">The s parameter value.</param>
        public static void FilterInvalidCharacter(ref string sParameterValue)
        {
            int nStrLength = sParameterValue.Length;
            if (nStrLength > 0)
            {
                if ('/' == sParameterValue[nStrLength - 1])
                {
                    if (1 == nStrLength)
                    {
                        sParameterValue = "";
                    }
                    else
                    {
                        sParameterValue = sParameterValue.Substring(0, nStrLength - 1);
                    }
                }
            }
        }


        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static void SetValue(string args)
        {
            string sParameterValue = Regex.Match(args, "^[0-9a-zA-Z]+://(.+)$").Groups[1].Value;
            RegeditHelper.FilterInvalidCharacter(ref sParameterValue);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\OraAnsParameters", "", sParameterValue); //将经过处理的传入参数写入注册表
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns></returns>
        public static string getValue()
        {
            string val = "";
            object Obj = Registry.GetValue(@"HKEY_CURRENT_USER\Software\OraAnsParameters", "", string.Empty);
            if (Obj != null)
            {
                val = Obj as string;
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\OraAnsParameters", "", string.Empty);
            }
            return val;
        }
    }
}