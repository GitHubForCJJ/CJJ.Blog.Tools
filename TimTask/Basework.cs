using FastDev.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimTask
{

    /// <summary>
    /// 基础任务类
    /// </summary>
    abstract public class BaseWork
    {
        /// <summary>
        /// 时间计时器
        /// </summary>
        System.Timers.Timer timer1 = new System.Timers.Timer();

        /// <summary>
        /// 运行成功的次数
        /// </summary>
        public int RunSuccessNums = 1;

        /// <summary>
        /// 当前工作单元运行周期
        /// </summary>
        public int RunCycleMinter = 60;

        /// <summary>
        /// 运行时间
        /// </summary>
        public string RunHours = "02:00:00";

        /// <summary>
        /// 执行监测类型 1按周期监测 2按时间点监测
        /// </summary>
        public int RunType = 1;

        /// <summary>
        /// 开始工作
        /// </summary>
        /// <param name="runHours">The run hours.</param>
        public void StartWork(string runHours)
        {
            RunType = 2;
            RunHours = runHours;
            timer1.Interval = RunCycleMinter * 1000 * 60;
            timer1.Elapsed += Timer1_Elapsed;
            timer1.Enabled = true;
            timer1.Start();
            Timer1_Elapsed(null, null);
        }

        /// <summary>
        /// 开始工作 按周期执行监测 单位分钟
        /// </summary>
        /// <param name="runCycle">The run cycle.</param>
        public void StartWork(int runCycle)
        {
            RunType = 1;
            RunCycleMinter = runCycle;
            timer1.Interval = RunCycleMinter * 1000 * 60;
            timer1.Elapsed += Timer1_Elapsed;
            timer1.Enabled = true;
            timer1.Start();
            Timer1_Elapsed(null, null);
        }

        /// <summary>
        /// 周期执行工作
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs" /> instance containing the event data.</param>
        public void Timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (RunType == 1)
                {
                    #region 按周期监测
                    timer1.Stop();
                    DoWork();
                    RunSuccessNums++;
                    #endregion
                }
                else
                {
                    #region 按时间点监测
                    var timeNow = DateTime.Now;
                    var runTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + RunHours);

                    if (timeNow > runTime && timeNow < runTime.AddHours(1))
                    {
                        Console.WriteLine("当前时间" + DateTime.Now.ToString("yyyyMMdd HHmmss") + ",执行统计");
                        timer1.Stop();
                        DoWork();
                        RunSuccessNums++;
                    }
                    else
                    {
                        Console.WriteLine("不是不执行，时候未到。配置执行时间每天：" + RunHours);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex, "统计程序抽象类捕获到异常");
            }
            finally
            {
                timer1.Start();
            }
        }

        /// <summary>
        /// 抽象类 执行工作
        /// </summary>
        abstract public void DoWork();
    }
}

