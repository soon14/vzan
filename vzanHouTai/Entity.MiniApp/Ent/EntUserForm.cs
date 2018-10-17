using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;

  
namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 页面设置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntUserForm
    {
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 表单类型 1：预约表单
        /// </summary>
        [SqlField]
        public int type { get; set; }

        [SqlField]
        public int aid { get; set; } = 0;
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int uid { get; set; } = 0;
        /// <summary>
        /// 页面名称
        /// </summary>
        [SqlField]
        public string pagename { get; set; } = string.Empty;
        /// <summary>
        /// 页面索引
        /// </summary>
        [SqlField]
        public int pageindex { get; set; } = -1;
        /// <summary>
        /// 组件名称
        /// </summary>
        [SqlField]
        public string comename { get; set; } = string.Empty;
        /// <summary>
        /// 组件索引
        /// </summary>
        [SqlField]
        public int comindex { get; set; } = -1;
        /// <summary>
        /// 表单结构
        /// </summary>
        [SqlField]
        public string formjson { get; set; } = string.Empty;
        /// <summary>
        /// 表单数据
        /// </summary>
        [SqlField]
        public string formdatajson { get; set; } = string.Empty;
        /// <summary>
        /// 提交时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;
        /// <summary>
        /// 表单创建时间
        /// </summary>
        [SqlField]
        public DateTime formtime { get; set; }

        public string showtime {
            get { return formtime.ToString("yyyy-MM-dd HH:mm:ss"); }            
        }
        /// <summary>
        /// 状态 1 正常(未处理)，0 删除， 2 已处理
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;
        /// <summary>
        /// 附加信息
        /// </summary>
        [SqlField]
        public string remark { get; set; } = string.Empty;
        /// <summary>
        /// 附加信息反序列化
        /// </summary>
        public EntFormRemark formremark { get; set; }

        /// <summary>
        /// 记录下有意义的执行过的操作,Contains以下char表示执行该节点过操作:
        ///     a:已经发送过模板消息通知客户已处理预约
        /// </summary>
        [SqlField]
        public string excedHandle { get; set; } = string.Empty;
        /// <summary>
        /// 店铺二维码关联ID
        /// </summary>
        [SqlField]
        public int StoreCodeId { get; set; } = 0;
        public string StoreCodeName { get; set; } = "";
        /// <summary>
        /// 付费表单 订单Id
        /// </summary>
        [SqlField]
        public int orderId { get; set; } = 0;
        /// <summary>
        /// 是否需要付费 0：不需要 ，1：需要
        /// </summary>
        [SqlField]
        public int isPay { get; set; } = 0;

        public string money { get; set; } = string.Empty;
    }
    public class EntFormRemark
    {
        /// <summary>
        /// 预约表单：预约产品信息
        /// </summary>
        public EntGoods goods { get; set; } = new EntGoods();
        /// <summary>
        /// 预约表单：操作者备注
        /// </summary>
        public string operationremark { get; set; } = string.Empty;
        /// <summary>
        /// 所选商品属性规格id串
        /// </summary>
        public string attrSpacStr { get; set; } = string.Empty;
        /// <summary>
        /// 所选商品属性规格说明  
        /// </summary>
        public string SpecInfo { get; set; } = string.Empty;
        /// <summary>
        /// 所选商品规格图片
        /// </summary>
        public string SpecImg { get; set; } = string.Empty;
        /// <summary>
        /// 预约产品数量
        /// </summary>
        public int count { get; set; } = 1;
    }

}

