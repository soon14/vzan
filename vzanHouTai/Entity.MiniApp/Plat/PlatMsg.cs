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
    /// 帖子详情
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatMsg
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
        public int Aid { get; set; }

        /// <summary>
        /// myCardId  关联表 获取注册用户
        /// </summary>
        [SqlField]
        public int MyCardId { get; set; }

        /// <summary>
        /// 发布人昵称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 发布人头像
        /// </summary>
        public string UserHeaderImg { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [SqlField]
        public string Phone { get; set; }


        /// <summary>
        /// 所属类别Id
        /// </summary>
        [SqlField]
        public int MsgTypeId { get; set; }

        /// <summary>
        /// 所属类别名称
        /// </summary>
        public string MsgTypeName { get; set; }

        /// <summary>
        /// 信息详情
        /// </summary>
        [SqlField]
        public string MsgDetail { get; set; }


        /// <summary>
        /// 图片 以逗号分隔
        /// </summary>
        [SqlField]
        public string Imgs { get; set; }

        public List<string> ImgList
        {

            get
            {
                List<string> imgArry = new List<string>();
                if (!string.IsNullOrEmpty(Imgs))
                {
                    imgArry = Imgs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                return imgArry;
            }

        }

        /// <summary>
        /// 地理位置
        /// </summary>
        [SqlField]
        public string Location { get; set; }


        /// <summary>
        /// 地理位置 纬度
        /// </summary>
        [SqlField]
        public string Lat { get; set; }

        /// <summary>
        /// 地理位置 经度
        /// </summary>
        [SqlField]
        public string Lng { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        [SqlField]
        public int IsTop { get; set; }


        /// <summary>
        /// 置顶时间
        /// </summary>
        [SqlField]
        public int TopDay { get; set; }




        /// <summary>
        /// 置顶所花费金额 分为单位
        /// </summary>
        [SqlField]
        public int TopCostPrice { get; set; }

        public string TopCostPriceStr
        {
            get
            {
                return (TopCostPrice * 0.01).ToString("0.00");
            }
        }


        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        public string AddTimeStr
        {

            get
            {
                return AddTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 显示时间字段 几天前 几小时前等等
        /// </summary>
        public string ShowTimeStr { get; set; }


        /// <summary>
        /// 更新时间 一般记录删除的时间 前端用户没发布信息后不能编辑
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }


        /// <summary>
        /// 信息状态 0为无效 1为有效 -1为删除
        /// 
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// 支付状态 0表示未支付 1表示已经支付了 
        /// 前端帖子显示 只有state=1 and paystate=1 才显示  
        /// 后台显示 state<>-1 and paystate=1,其中 state<>-1 包括审核通过和不通过的
        /// </summary>
        [SqlField]
        public int PayState { get; set; }
        

        /// <summary>
        /// 审核状态 0为无需审核 -1 审核不通过 1待审核 2审核通过    只能在后台操作改变状态
        /// </summary>
        [SqlField]
        public int Review { get; set; }

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
        /// CityModerId  微信支付订单Id
        /// </summary>
        [SqlField]
        public int CityModerId { get; set; }

        /// <summary>
        /// 距离目的地的差值
        /// </summary>
        public double Distance { get; set; } = 0.00;
        public string DistanceStr { get; set; }


        /// <summary>
        /// 审核时间
        /// </summary>
        [SqlField]
        public DateTime ReviewTime { get; set; }

        public string ReviewTimeStr
        {

            get
            {
                return ReviewTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 是否过期
        /// </summary>
        public bool IsExpired
        {
            //TODO 测试时的时间单位为分钟 线上为天
            get
            {
                return DateTime.Now > ReviewTime.AddDays(TopDay);
            }
        }

        /// <summary>
        /// 置顶剩余天数
        /// </summary>
        public int BalanceDay
        {
            
            get
            {
                int d = ReviewTime.AddDays(TopDay).Subtract(DateTime.Now).Days;
                return d>0?d:0 ;
            }
        }

        public int BalanceHousrs
        {

            get
            {
                int h = ReviewTime.AddDays(TopDay).Subtract(DateTime.Now).Hours;
                return h>0?h:0;
            }
        }
        public int BalanceMinute
        {

            get
            {
                int m= ReviewTime.AddDays(TopDay).Subtract(DateTime.Now).Minutes;
                return m>0?m:0;
            }
        }

        /// <summary>
        /// 当前用户是否已经收藏了该帖子
        /// </summary>
        public bool IsFavorited { get; set; } = false;


        /// <summary>
        /// 当前用户是否已经点赞了该帖子
        /// </summary>
        public bool IsDzed { get; set; } = false;

        /// <summary>
        /// 当前用户对当前帖子是否举报过
        /// </summary>
        public bool IsReported { get; set; } = false;
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
        /// 帖子对应的评论
        /// </summary>
        public List<PlatMsgComment> Comments { get; set; }


        /// <summary>
        /// 帖子对应的点赞用户
        /// </summary>
        public List<PlatUserFavoriteMsg> DzUsers { get; set; }


        /// <summary>
        /// 发帖用户Id
        /// </summary>
        public long UserId { get; set; }


    }

    public class SavePlatMsgModel
    {
        public int UserId { get; set; }
        public int MsgType { get; set; }

        public int IsTop { get; set; }

        public string MsgDetail { get; set; }

        public string Phone { get; set; }


        public string Imgs { get; set; }

        public string Location { get; set; }
        public string Lng { get; set; }
        public string Lat { get; set; }

        public int RuleId { get; set; }

    }
}
