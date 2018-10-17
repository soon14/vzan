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
    /// 同城模板 信息
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CityMsg
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
        /// 用户Id
        /// </summary>
        [SqlField]
        public int userId { get; set; }

        /// <summary>
        /// 发布人昵称
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// 发布人头像
        /// </summary>
        public string userHeaderImg { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [SqlField]
        public string phone { get; set; }


        /// <summary>
        /// 所属类别Id
        /// </summary>
        [SqlField]
        public int msgTypeId { get; set; }

        /// <summary>
        /// 所属类别名称
        /// </summary>
        public string msgTypeName { get; set; }

        /// <summary>
        /// 信息详情
        /// </summary>
        [SqlField]
        public string msgDetail { get; set; }


        /// <summary>
        /// 图片 以逗号分隔
        /// </summary>
        [SqlField]
        public string imgs { get; set; }

        public List<string> imgList
        {

            get
            {
                List<string> imgArry = new List<string>();
                if (!string.IsNullOrEmpty(imgs))
                {
                    imgArry = imgs.Split(new char[] { ','},StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                return imgArry;
            }

        }

        /// <summary>
        /// 地理位置
        /// </summary>
        [SqlField]
        public string location { get; set; }


        /// <summary>
        /// 地理位置 纬度
        /// </summary>
        [SqlField]
        public string lat { get; set; }

        /// <summary>
        /// 地理位置 经度
        /// </summary>
        [SqlField]
        public string lng { get; set; }


        /// <summary>
        /// 置顶时间
        /// </summary>
        [SqlField]
        public int topDay { get; set; }


        /// <summary>
        /// 置顶所花费金额 分为单位
        /// </summary>
        [SqlField]
        public int topCostPrice { get; set; }

        public string topCostPriceStr
        {
            get
            {
                return (topCostPrice * 0.01).ToString("0.00");
            }
        }


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
        /// 显示时间字段 几天前 几小时前等等
        /// </summary>
        public string showTimeStr { get; set; }
        

        /// <summary>
        /// 更新时间 一般记录删除的时间 前端用户没发布信息后不能编辑
        /// </summary>
        [SqlField]
        public DateTime updateTime { get; set; }


        /// <summary>
        /// 信息状态 0为无效 1为有效 -1为删除
        /// 如果是不需要支付的则直接为1  如果需要支付则在支付成功回调改变状态为1
        /// </summary>
        [SqlField]
        public int state { get; set; }



        /// <summary>
        /// 审核状态 0为无需审核 -1 审核不通过 1待审核 2审核通过    只能在后台操作改变状态
        /// </summary>
        [SqlField]
        public int Review { get; set; }

        /// <summary>
        /// 帖子审核状态结果
        /// </summary>
        public string ReviewState
        {
            get
            {
                string s = "";
                switch (Review)
                {
                    case -1:
                        s = "审核不通过";
                        break;
                    case 1:
                        s = "待审核";
                        break;
                    case 2:
                        s = "";
                        break;
                }
                
                return s;
            }
        }


        /// <summary>
        /// 置顶剩余天数
        /// </summary>
        public int BalanceDay
        {

            get
            {
                int d = addTime.AddDays(topDay).Subtract(DateTime.Now).Days;
                return d > 0 ? d : 0;
            }
        }

        public int BalanceHousrs
        {

            get
            {
                int h = addTime.AddDays(topDay).Subtract(DateTime.Now).Hours;
                return h > 0 ? h : 0;
            }
        }
        public int BalanceMinute
        {

            get
            {
                int m = addTime.AddDays(topDay).Subtract(DateTime.Now).Minutes;
                return m > 0 ? m : 0;
            }
        }

        /// <summary>
        /// 是否是后台手动置顶0表示不是 1表示是
        /// </summary>
        [SqlField]
        public int IsDoTop { get; set; }

        /// <summary>
        /// 是否是后台手动取消置顶0表示不是 1表示是
        /// </summary>
        [SqlField]
        public int IsDoNotTop { get; set; }


        /// <summary>
        /// CityModerId  微信支付订单Id
        /// </summary>
        [SqlField]
        public int cityModerId { get; set; }

        /// <summary>
        /// 距离目的地的差值
        /// </summary>
        public double distance { get; set; } = 0.00;
        public string distanceStr { get; set; }

        /// <summary>
        /// 是否过期
        /// </summary>
        public bool isExpired
        {
           
            get
            {
                return DateTime.Now > addTime.AddDays(topDay);
            }
        }
      


        /// <summary>
        /// 当前用户是否已经收藏了该帖子
        /// </summary>
        public bool isFavorited { get; set; } = false;


        /// <summary>
        /// 当前用户是否已经点赞了该帖子
        /// </summary>
        public bool isDzed { get; set; } = false;

        /// <summary>
        /// 当前用户对当前帖子是否举报过
        /// </summary>
        public bool isReported { get; set; } = false;
        /// <summary>
        /// 帖子总浏览量
        /// </summary>

        public int ViewCount { get; set; }


        /// <summary>
        /// 帖子总收藏量
        /// </summary>
 
        public int FavoriteCount { get; set; }

        /// <summary>
        /// 帖子总分享量
        /// </summary>

        public int ShareCount { get; set; }


        /// <summary>
        /// 帖子总点赞量
        /// </summary>

        public int DzCount { get; set; }


        /// <summary>
        /// 收藏记录Id
        /// </summary>
        public int FavoriteId { get; set; }

        /// <summary>
        /// 帖子评论列表
        /// </summary>
        public List<CityMsgComment> Comments = new List<CityMsgComment>();

    }

    public class SaveCityMsgModel
    {
        public int userId { get; set; }
        public int msgType { get; set; }

        public int isTop { get; set; }

        public string msgDetail { get; set; }

        public string phone { get; set; }


        public string imgs { get; set; }

        public string location { get; set; }
        public string lng { get; set; }
        public string lat { get; set; }

        public int ruleId { get; set; }

    }


}
