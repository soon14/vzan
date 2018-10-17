using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Fds
{
    /// <summary>
    /// 排队表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class SortQueue
    {

        public SortQueue() { }
        /// <summary>
        /// 餐饮店铺Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int aId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int storeId { get; set; }


        [SqlField]
        //当前队列所属模板类型 默认餐饮模板
        public int pageType { get; set; } = (int)TmpType.小程序餐饮模板;

        /// <summary>
        /// 排队号码
        /// </summary>
        [SqlField]
        public int sortNo { get; set; }

        /// <summary>
        /// 发起排队的用户Id c_userinfo
        /// </summary>
        [SqlField]
        public int userId { get; set; }
        /// <summary>
        /// 用户微信昵称
        /// </summary>
        public string nickName { get; set; }

        /// <summary>
        /// 用餐人数
        /// </summary>
        [SqlField]
        public int pCount { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [SqlField]
        public string telephone { get; set; }

        /// <summary>
        /// 排队状态 (-1 已取消,0排队中,1 已就餐/已接待)
        /// </summary>
        [SqlField]
        public int state { get; set; }
        /// <summary>
        /// 状态文案
        /// </summary>
        public string stateRemark
        {
            get
            {
                switch (state)
                {
                    case 1:
                        switch (pageType)
                        {
                            case (int)TmpType.小程序专业模板:
                                return "已接待";
                            default:
                                return "已就餐";
                        }
                    case -1:
                        return "已取消";
                    case 0:
                    default:
                        return "排队中";
                }
            }
        }

        /// <summary>
        /// 发起排队的时间
        /// </summary>
        [SqlField]
        public DateTime createDate { get; set; }
        /// <summary>
        /// 排队时间字符串
        /// </summary>
        public string createDateStr
        {
            get
            {
                return createDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 处理排队的时间
        /// </summary>
        [SqlField]
        public DateTime updateDate { get; set; }

    }
}
