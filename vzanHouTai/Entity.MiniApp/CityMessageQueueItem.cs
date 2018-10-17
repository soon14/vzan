using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    /// <summary>
    /// CityMessageQueue消息列队项
    /// </summary>
    public class CityMessageQueueItem
    {
        /// <summary>
        /// 列队项唯一标识
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 列队项目命中触发时执行的委托
        /// </summary>
        public Action Action { get; set; }
        /// <summary>
        /// 此实例对象的创建时间
        /// </summary>
        public DateTime AddTime { get; set; }
          /// <summary>
        /// 初始化CityMessageQueue消息列队项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="description"></param>
        public CityMessageQueueItem(string key, Action action, string description = null)
        {
            Key = key;
            Action = action;
            AddTime = DateTime.Now;
        }
    }
}
