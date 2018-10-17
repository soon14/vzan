using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 模板消息发送参数记录
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class TemplateMsg_UserParam
    {
        public TemplateMsg_UserParam() { }

        /// <summary>
        /// 
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public string Form_Id { get; set; }


        /// <summary>
        /// 标识类型 ： 0 为 form_id 只能用一次, 1 为 prepay_id 可以用3次
        /// </summary>
        [SqlField]
        public int Form_IdType { get; set; }


        /// <summary>
        /// 接受者的openid
        /// </summary>
        [SqlField]
        public string Open_Id { get; set; }


        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// 使用次数
        /// </summary>
        [SqlField]
        public int SendCount { get; set; } = 0;


        /// <summary>
        /// 所属订单表Id(如餐饮 miniappfoodgoodsorder)
        /// </summary>
        [SqlField]
        public int OrderId { get; set; }

        /// <summary>
        /// 订单类型ID
        /// </summary>
        [SqlField]
        public int OrderIdType { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime AddDate { get; set; }

        /// <summary>
        /// 失效时间
        /// </summary>
        [SqlField]
        public DateTime LoseDateTime { get; set; }

        /// <summary>
        /// 属于哪个AppId
        /// </summary>
        [SqlField]
        public string AppId { get; set; }

        ///// <summary>
        ///// 此模板下什么类型的订单 0正常订单 1砍价 2拼团
        ///// </summary>
        ////[SqlField]
        //public int OrderType { get; set; }
    }
}
