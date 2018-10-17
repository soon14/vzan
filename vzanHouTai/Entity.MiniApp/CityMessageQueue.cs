/*----------------------------------------------------------------
    Copyright (C) 2016 City
    文件名：CityMessageQueue.cs
    文件功能描述：CityMessageQueue消息列队,执行一些后台功能。如模板消息推送!
   ----------------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Entity.MiniApp
{
    /// <summary>
    /// 消息列队
    /// </summary>
    public class CityMessageQueue
    {
        /// <summary>
        /// 列队数据集合
        /// </summary>
        private static ConcurrentDictionary<string, CityMessageQueueItem> CityMessageQueueDictionary = new ConcurrentDictionary<string, CityMessageQueueItem>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 同步执行锁
        /// </summary>
        private static object MessageQueueSyncLock = new object();
        /// <summary>
        /// 立即同步所有缓存执行锁（给OperateQueue()使用）
        /// </summary>
        private static object FlushCacheLock = new object();

      

        /// <summary>
        /// 操作列队
        /// </summary>
        public static void OperateQueue()
        {
            lock (FlushCacheLock)
            {
                //保证队列执行顺序
                CityMessageQueue mq = new CityMessageQueue();
                string key = mq.GetCurrentKey(); //获取最新的Key
                while (!string.IsNullOrEmpty(key))
                {
                    CityMessageQueueItem mqItem = mq.GetItem(key); //获取任务项
                 
                    mqItem.Action(); //执行
                    mq.Remove(key); //清除
                    key = mq.GetCurrentKey(); //获取最新的Key
                }
            }
        }

        /// <summary>
        /// 获取当前等待执行的Key
        /// </summary>
        /// <returns></returns>
        public string GetCurrentKey()
        {
            lock (MessageQueueSyncLock)
            {
                return CityMessageQueueDictionary.Keys.FirstOrDefault();
            }
        }

        /// <summary>
        /// 获取CityMessageQueueItem
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CityMessageQueueItem GetItem(string key)
        {
            lock (MessageQueueSyncLock)
            {
                if (CityMessageQueueDictionary.ContainsKey(key))
                {
                    return CityMessageQueueDictionary[key];
                }
                return null;
            }
        }

        /// <summary>
        /// 添加列队成员
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public CityMessageQueueItem Add(string key, Action action)
        {
            //保证一条一条加
            lock (MessageQueueSyncLock)
            {
                CityMessageQueueItem mqItem = new CityMessageQueueItem(key, action);
                CityMessageQueueDictionary[key] = mqItem;
                return mqItem;
            }
        }

        /// <summary>
        /// 移除列队成员
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            lock (MessageQueueSyncLock)
            {
                if (CityMessageQueueDictionary.ContainsKey(key))
                {
                    CityMessageQueueItem temp = new CityMessageQueueItem(key,null);
                    CityMessageQueueDictionary.TryRemove(key,out temp);
                    //MessageQueueList.Remove(key);
                }
            }
        }

        /// <summary>
        /// 获得当前列队数量
        /// </summary>
        /// <returns></returns>
        public int GetCount()
        {
            lock (MessageQueueSyncLock)
            {
                return CityMessageQueueDictionary.Count;
            }
        }

    }
}
