using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Copyfile.Models
{
    /// <summary>
    /// ProcessManager xml配置文件辅助类
    /// </summary>
    public class ProcessView
    {
        /// <summary>
        /// ProcessInfo 的id
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 程序名称  如Go.Wallet.Dispatch.RefundRetry.exe.exe
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Path { get; set; }
        public string MaxCPU { get; set; }
        public string MaxMemory { get; set; }
        public string PID { get; set; }
        /// <summary>
        /// exe描述
        /// </summary>
        public string Desc { get; set; }
        public string RestartTime { get; set; }
        public string IsRestart { get; set; }
        public string StartTime { get; set; }
        public string IsDie { get; set; }
    }
}
