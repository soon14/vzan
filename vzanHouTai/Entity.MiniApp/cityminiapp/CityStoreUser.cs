using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.cityminiapp
{
    /// <summary>
    /// 同城模板 用户信息表
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CityStoreUser
    {

    
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int aid { get; set; }

        /// <summary>
        /// 用户绑定的电话号码
        /// </summary>
        [SqlField]
        public string phone { get; set; } = string.Empty;

        /// <summary>
        /// 小程序用户Id
        /// </summary>
        [SqlField]
        public int userId { get; set; } = 0;


        /// <summary>
        /// 用户昵称
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// 用户发布信息总数量
        /// </summary>
        public int msgTotalCount { get; set; } = 0;

        /// <summary>
        /// 用户发布的置顶信息总数量
        /// </summary>
        public int topMsgTotalCount { get; set; } = 0;

        /// <summary>
        /// 用户发布的置顶信息所花费的总金额
        /// </summary>
        public string topMsgCostTotalPrice { get; set; } = "0.00";



        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }

        public string addTimeStr
        {
            get
            {
                  return addTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime updateTime { get; set; }

        /// <summary>
        /// 状态 0表示正常 -1表示删除 1表示被拉黑不能发帖
        /// </summary>
        [SqlField]
        public int state { get; set; }


    }


    public class Ccity_userMsgCountViewMolde
    {
        /// <summary>
        /// 用户发布信息总数量
        /// </summary>
        public int msgTotalCount { get; set; } = 0;

        /// <summary>
        /// 用户发布的置顶信息总数量
        /// </summary>
        public int topMsgTotalCount { get; set; } = 0;

        /// <summary>
        /// 用户发布的置顶信息所花费的总金额
        /// </summary>
        public string topMsgCostTotalPrice { get; set; } = "0.00";

    }

}
