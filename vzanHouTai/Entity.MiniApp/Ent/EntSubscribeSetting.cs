using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 产品预约配置（暂时无用）
    /// </summary>
    public class EntSubscribeSetting
    {
        /// <summary>
        /// 是否填写姓名
        /// </summary>
        public int username { get; set; } = 0;
        /// <summary>
        /// 是否填写手机号
        /// </summary>
        public int phone { get; set; } = 0;
        /// <summary>
        /// 是否填写性别
        /// </summary>
        public int sex { get; set; } = 0;
        /// <summary>
        /// 是否填写年龄
        /// </summary>
        public int age { get; set; } = 0;
        /// <summary>
        /// 是否填写备注
        /// </summary>
        public int remark { get; set; } = 0;
        /// <summary>
        /// 是否填写地址
        /// </summary>
        public int address { get; set; } = 0;
        /// <summary>
        /// 是否精确到经纬度
        /// </summary>
        public int addressitude { get; set; } = 0;
        /// <summary>
        /// 是否预约
        /// </summary>
        public int subsrcibe { get; set; } = 0;
        /// <summary>
        /// 预约提前时间 单位：分钟
        /// </summary>
        public int subscribeTime { get; set; } = 10;
    }
}
