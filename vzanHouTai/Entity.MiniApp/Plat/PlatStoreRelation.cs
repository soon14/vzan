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
    /// 平台版店铺信息关系表 店铺同步数据关系表
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatStoreRelation
    {

        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId   所属平台小程序
        /// </summary>
        [SqlField]
        public int Aid { get; set; }


        // <summary>
        /// 店铺ID 关联店铺表
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }


        // <summary>
        /// 数据来源 0表示通过平台入驻  1表示同步其它平台建立的关系
        /// </summary>
        [SqlField]
        public int FromType { get; set; }


        /// <summary>
        /// 状态 0屏蔽 1 显示
        /// </summary>
        [SqlField]
        public int State { get; set; }


        /// <summary>
        /// 该同步数据来自哪个代理商Id  同步过来的不为0
        ///  例如A B C 三个平台 B是A的上级代理  C是B的上级代理 当A平台有店铺入驻时候
        ///  则需把该店铺同时同步到B C平台个一份
        /// </summary>
        [SqlField]
        public int AgentId { get; set; }


        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }


      


        /// <summary>
        /// 店铺类别，这里是店铺显示在哪个类别根据这个类别来
        /// </summary>
        [SqlField]
        public int Category { get; set; }


        /// <summary>
        /// 店铺所属名片Id
        /// </summary>
        public int MyCardId { get; set; }

        public int StoreAid { get; set; }

        public int FirstCategory { get; set; }


        /// <summary>
        /// 原平台大类名称
        /// </summary>
        public string OwnerFirstCategoryName { get; set; }
        /// <summary>
        /// 原平台小类名称
        /// </summary>
        public string OwnerSecondCategoryName { get; set; }


        /// <summary>
        /// 现在所属平台大类名称
        /// </summary>
        public string CurFirstCategoryName { get; set; }
        /// <summary>
        /// 现在所属平台小类名称
        /// </summary>
        public string CurSecondCategoryName { get; set; }


        /// <summary>
        /// 店铺主人账号 手机号码
        /// </summary>
        public string StoreOwnerPhone { get; set; }

        /// <summary>
        /// 数据来源 如果是平台入驻显示 平台入驻 否则显示分销代理商名称
        /// </summary>
        public string FromTypeStr { get; set; }

        /// <summary>
        /// 绑定的小程序名称
        /// </summary>
        public string BindAppIdName { get; set; }


        /// <summary>
        /// 店铺访问量
        /// </summary>
        public int StorePV { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string StoreName { get; set; }


        public string IsViewStr
        {
            get
            {
                return State == 0 ? "屏蔽" : "显示";

            }
        }



     
        public int YearCount { get; set; } = 12;
   
        public int CostPrice { get; set; }

        /// <summary>
        /// 是否过期
        /// </summary>
        public bool IsExpired
        {

            get
            {
                //TODO 上线改为月
                return DateTime.Now > AddTime.AddMonths(YearCount);
            }
        }


        /// <summary>
        /// 入驻时间
        /// </summary>
        public string AddTimeStr
        {

            get
            {
                return AddTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 过期时间
        /// </summary>
        public string ExpireTimeStr
        {

            get
            {
                //TODO 上线改为月
                return AddTime.AddMonths(YearCount).ToString("yyyy-MM-dd HH:mm:ss");
            }
        }









    }
}
