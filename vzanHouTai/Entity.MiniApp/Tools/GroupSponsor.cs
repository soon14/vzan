using Entity.Base;
using Entity.MiniApp.Stores;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class GroupSponsor
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 拼团id
        /// </summary>
        [SqlField]
        public int GroupId { get; set; }
        /// <summary>
        /// 拼团名称
        /// </summary>
        [SqlField]
        public string GroupName { get; set; } = string.Empty;
        /// <summary>
        /// 发起拼团的用户
        /// </summary>
        [SqlField]
        public int SponsorUserId { get; set; }
        /// <summary>
        /// 成团人数
        /// </summary>
        [SqlField]
        public int GroupSize { get; set; }
        /// <summary>
        /// 开团时间
        /// </summary>
        [SqlField]
        public DateTime StartDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 结束时间
        /// </summary>
        [SqlField]
        public DateTime EndDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 4待付款，1开团成功，2团购成功，-1成团失败(GroupState)
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        //View 显示数据
        public string UserName { get; set; }
        public string UserLogo { get; set; }
        public int NeedNum { get; set; }
        public string ShowStartTime { get; set; }
        public string ShowEndTime { get; set; }

        public string ImgUrl { get; set; }
        public Store Store { get; set; }
        public string Description { get; set; }
        public int DiscountPrice { get; set; }
        public int OriginalPrice { get; set; }
        public List<C_Attachment> DescImgList { get; set; }
        public C_UserInfo LoginUser { get; set; }
        public int LimitNum { get; set; } = 0;
        public int CreateNum { get; set; } = 0;
        //猜你喜欢
        public List<Groups> GroupsList { get; set; }

        public GroupUser GUInfo { get; set; }

        //是否过期
        public int IsExpire { get; set; }

        //单个团的详情
        public List<GroupUser> GroupUserList { get; set; }

        //自己已购买此团总量
        public int GroupsNum { get; set; } = 0;
        //商品剩余数量
        public int RemainNum { get; set; } = 0;
        //public C_Groups Groups { get; set; }      
    }
}
