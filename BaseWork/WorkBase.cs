using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseWork
{
    public abstract class WorkBase
    {
        public System.Timers.Timer Timer = new System.Timers.Timer();
        /// <summary>
        /// 1 按时间点运行，2按时间周期运行
        /// </summary>
        public int RunType = 1;
        //运行时间点
        public string RunHour = "00:00:00";
        //运行的周期分钟
        public int RunCycle = 60;

        public int RunSuccessTime = 0;
        /// <summary>
        /// 执行任务方法
        /// </summary>
        public abstract void RunWork();

        /// <summary>
        /// 时间点运行
        /// </summary>
        /// <param name="runhour"></param>
        public void StartWork(string runhour)
        {
            RunType = 1;
            RunHour = runhour;
            Timer.Interval = RunCycle * 1000 * 60;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Enabled = true;
            Timer.Start();
            //开始时就执行一次
            Timer_Elapsed(null, null);
        }

        /// <summary>
        /// 周期运行
        /// </summary>
        /// <param name="runcycle"></param>
        public void StartWork(int runcycle)
        {
            RunType = 2;
            RunCycle = runcycle;
            Timer.Interval = RunCycle * 1000 * 60;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Enabled = true;
            Timer.Start();
            //开始时就执行一次
            Timer_Elapsed(null, null);
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                //时间点
                if (RunType == 1)
                {
                    var runhour = DateTime.TryParse($"{DateTime.Now.ToString("yyyy-MM-dd")} {RunHour}", out DateTime time);
                    if (time > DateTime.Now && time < DateTime.Now.AddHours(1))
                    {
                        Timer.Stop();
                        RunWork();
                        RunSuccessTime++;
                        Console.WriteLine($"运行成功次数:{RunSuccessTime}");
                    }
                    else
                    {
                        Console.WriteLine("时间未到不执行");
                    }
                }
                //周期
                else if (RunType == 2)
                {
                    Timer.Stop();
                    RunWork();
                    RunSuccessTime++;
                    Console.WriteLine($"运行成功次数:{RunSuccessTime}");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Timer.Start();
            }

        }

    }
}
