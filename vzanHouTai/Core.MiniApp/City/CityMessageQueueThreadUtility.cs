/*----------------------------------------------------------------
    Copyright (C) 2016 City

    文件名：CityMessageQueueThreadUtility.cs
    文件功能描述：CityMessageQueue消息列队线程处理


    创建标识：City - 20160210

----------------------------------------------------------------*/

using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Core.MiniApp
{
    /// <summary>
    /// CityMessageQueue线程自动处理
    /// </summary>
    public class CityMessageQueueThreadUtility
    {
        private readonly int _sleepMilliSeconds;


        public CityMessageQueueThreadUtility(int sleepMilliSeconds = 2000)
        {//默认每两秒一跑
            _sleepMilliSeconds = sleepMilliSeconds;
        }

        /// <summary>
        /// 析构函数，将未处理的列队处理掉
        /// </summary>
        ~CityMessageQueueThreadUtility()
        {
            try
            {
                CityMessageQueue mq = new CityMessageQueue();
              
                CityMessageQueue.OperateQueue();//处理列队
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), string.Format("CityMessageQueueThreadUtility执行析构函数错误：{0}", ex.Message));
                
            }
        }

        /// <summary>
        /// 启动线程轮询
        /// </summary>
        public void Run()
        {
            do
            {
                CityMessageQueue.OperateQueue();
                Thread.Sleep(_sleepMilliSeconds);
            } while (true);
        }
    }
}
