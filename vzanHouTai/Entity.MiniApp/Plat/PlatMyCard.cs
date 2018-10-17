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
    /// 名片
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatMyCard
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        
        /// <summary>
        /// 名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        [SqlField]
        public string NickName { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [SqlField]
        public int Sex { get; set; } = 0;
        /// <summary>
        /// 地址
        /// </summary>
        [SqlField]
        public string Address { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        [SqlField]
        public double Lat { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        [SqlField]
        public double Lng { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        [SqlField]
        public string Phone { get; set; }
        /// <summary>
        /// 权限便ID
        /// </summary>
        [SqlField]
        public int AId { get; set; } = 0;
        /// <summary>
        /// 状态，-1：屏蔽，0或1正常
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        /// <summary>
        /// 公司名称
        /// </summary>
        [SqlField]
        public string CompanyName { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; }
        /// <summary>
        /// 行业类型
        /// </summary>
        [SqlField]
        public int JobType { get; set; } = 0;

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
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string Desc { get; set; }
        /// <summary>
        /// c_userinfo表Id
        /// </summary>
        [SqlField]
        public long UserId { get; set; }
        /// <summary>
        /// 后台店铺登陆ID
        /// </summary>
        [SqlField]
        public string LoginId { get; set; }
        /// <summary>
        /// 职位
        /// </summary>
        [SqlField]
        public string Job { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        [SqlField]
        public string Department { get; set; }
        /// <summary>
        /// 行业
        /// </summary>
        [SqlField]
        public int IndustryId { get; set; }
        public string IndustryName { get; set; }
        /// <summary>
        /// 是否隐藏手机号,-1:隐藏，0：显示
        /// </summary>
        [SqlField]
        public int HiddenPhone { get; set; }

        [SqlField]
        public int ProvinceCode { get; set; }
        [SqlField]
        public int CityCode { get; set; }
        [SqlField]
        public int CountryCode { get; set; }
        /// <summary>
        /// 虚拟人气
        /// </summary>
        [SqlField]
        public int FictitiousCount { get; set; }
        /// <summary>
        /// 名片码
        /// </summary>
        [SqlField]
        public string QrCodeImgUrl { get; set; }
        /// <summary>
        /// 小程序appid
        /// </summary>
        [SqlField]
        public string AppId { get; set; }
        /// <summary>
        /// 距离
        /// </summary>
        public string Distance { get; set; }

        /// <summary>
        /// 关注
        /// </summary>
        public int FollowCount { get;set; }
        public bool IsFollow { get; set; }
        /// <summary>
        /// 收藏
        /// </summary>
        public int FavoriteCount { get; set; }
        public bool IsFavorite { get; set; }
        /// <summary>
        /// 点赞
        /// </summary>
        public int DzCount { get; set; }
        public bool IsDz { get; set; }
        /// <summary>
        /// 浏览
        /// </summary>
        public int ViewCount { get; set; }
        /// <summary>
        /// 私信
        /// </summary>
        public int SiXinCount { get; set; }

        public HaveNewData NewData { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// 店铺收藏数
        /// </summary>
        public int StoreFavoriteCount { get; set; }
        /// <summary>
        /// 店铺浏览量
        /// </summary>
        public int StoreViewCount { get; set; }
        /// <summary>
        /// 店铺访客量
        /// </summary>
        public int StoreVistorCount { get; set; }

        /// <summary>
        /// 发帖总量
        /// </summary>
        public int MsgCount { get; set; }
        /// <summary>
        /// 评论总量
        /// </summary>
        public int ContentCount{get;set;}
        /// <summary>
        /// 点赞帖子总数
        /// </summary>
        public int MsgDzCount { get; set; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        public int StoreId { get; set; }
    }

    public class HaveNewData
    {
        public bool ViewData { get; set; } = false;
        public bool FollowData { get; set; } = false;
        public bool DzData { get; set; } = false;
        public bool SiXinData { get; set; } = false;
    }

    public class HaveNewDataV2
    {
        public int ViewData { get; set; }
        public int FollowData { get; set; }
        public int DzData { get; set; }
        public int SiXinData { get; set; }
    }
}
