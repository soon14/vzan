using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{ 
    /// <summary>
    /// 缓存列表数据存储实体类
    /// </summary>
    public class RedisModel<T>
    {
        public List<T> DataList { get; set; }
        public int Count { get; set; } = 0;
        public int DataVersion { get; set; }
    } 
}
