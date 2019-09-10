//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Copyfile.Common
//{
//    public class EventCopyJob: IJob
//    {
//        /// <summary>
//        /// 是否记录日志
//        /// </summary>
//        public static readonly bool IsLog = ConfigHelper.GetConfigToBool("IsLog");
//        /// <summary>
//        /// 作业调度定时执行的方法
//        /// </summary>
//        /// <param name="context">The execution context.</param>
//        /// <returns>Task.</returns>
//        /// <remarks>The implementation may wish to set a  result object on the
//        /// JobExecutionContext before this method exits.  The result itself
//        /// is meaningless to Quartz, but may be informative to
//        /// <see cref="T:Quartz.IJobListener" />s or
//        /// <see cref="T:Quartz.ITriggerListener" />s that are watching the job's
//        /// execution.</remarks>
//        public virtual Task Execute(IJobExecutionContext context)
//        {
//            return Task.Factory.StartNew(() =>
//            {
//                EveryTask();
//            });
//        }

//        /// <summary>
//        ///任务实现
//        /// </summary>
//        public void EveryTask()
//        {
//            try
//            {
//                Result ret = OrderWcfHelper.AutoCancelOrd();
//                if (IsLog)
//                {
//                    LogHelper.WriteLog($"创客取消订单-{ret.SerializeObject()}:{DateTime.Now.Ticks}");
//                }
//            }
//            catch
//            {

//            }
//        }

//    }
//}
