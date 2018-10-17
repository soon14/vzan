using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Fds
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
   public class FoodOrderPrintLog
    {

        public FoodOrderPrintLog() { }
        /// <summary>
        /// 自增Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 打印单号（与打印接口中返回的id一一对应）
        /// </summary>
        [SqlField]
        public string Dataid { get; set; } = string.Empty;
        /// <summary>
        /// 终端号
        /// </summary>
        [SqlField]
        public string machine_code { get; set; } = string.Empty;

        /// <summary>
        /// 添加时间|发送成功时间|打印完成时间|
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }

        /// <summary>
        /// 状态 -1:发送失败， 0:发送成功， 1:打印成功， 2:打印失败
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 数据格式 0:插入  1:更新
        /// </summary>
        [SqlField]
        public int isupdate { get; set; } = 0;

        /// <summary>
        /// 固定表示finish，用于识别提交参数的用途
        /// </summary>
        [SqlField]
        public string cmd { get; set; } = string.Empty;

        /// <summary>
        /// 订单ID
        /// </summary>
        [SqlField]
        public int orderId { get; set; } = 0;
        /// <summary>
        /// 打印机id 对应miniappfoodprints表 id
        /// </summary>
        [SqlField]
        public int printsId { get; set; } = 0;
        /// <summary>
        /// 状态明细
        /// </summary>
        [SqlField]
        public string remark { get; set; } = string.Empty;
    }
}
