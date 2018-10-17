using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Plat
{
    /// <summary>
    /// 体现配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatDrawConfig
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        
        /// <summary>
        /// 权限表ID
        /// </summary>
        [SqlField]
        public int AId { get; set; }
        /// <summary>
        /// 最低体现金额
        /// </summary>
        [SqlField]
        public int MinMoney { get; set; }
        public string MinMoneyStr { get { return (MinMoney * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 最快处理时间
        /// </summary>
        [SqlField]
        public int CommandTime { get; set; }
        /// <summary>
        /// 体现手续费
        /// </summary>
        [SqlField]
        public int Fee { get; set; }
        public string FeeStr { get { return (Fee * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 0：百分比，1：限定金额
        /// </summary>
        [SqlField]
        public int FeeType { get; set; }
        /// <summary>
        /// 提现方式
        /// </summary>
        [SqlField]
        public string DrawCashWay { get; set; }
        public List<DrawCashWayItem> DrawCashWayList { get { return (string.IsNullOrEmpty(DrawCashWay) ? new List<DrawCashWayItem>() : Newtonsoft.Json.JsonConvert.DeserializeObject<List<DrawCashWayItem>>(DrawCashWay)); } }
        [SqlField]
        public int State { get; set; } = 0;
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
    }
    public class DrawCashWayItem
    {
        public string Name { get; set; }
        /// <summary>
        /// 0：不开启，1：开启
        /// </summary>
        public int IsOpen { get; set; }
        /// <summary>
        /// 提现方式 MiniAppEnum.DrawCashWay
        /// </summary>
        public int DrawCashWay { get; set; }
    }
}
