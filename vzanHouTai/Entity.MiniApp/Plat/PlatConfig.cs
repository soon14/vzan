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
    /// 平台首页店铺配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatConfig
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 小程序appId 所属平台小程序
        /// </summary>
        [SqlField]
        public int Aid { get; set; }

        /// <summary>
        /// 对象Id 如果是轮播图绑定的是店铺则为店铺Id,如果绑定的是帖子则为帖子Id
        /// 如果是推荐商家,置顶商家 则绑定是店铺Id
        /// 如果ConfigType=3则这里表示店铺公告弹窗开关
        /// 如果表示平台数据设置 则这里表示虚拟发帖量
        /// 如果是关注公众号 则这里表示 名片展示开关 0关闭 1开启
        /// </summary>
        [SqlField]
        public int ObjId { get; set; }

        /// <summary>
        /// 排序顺序
        /// </summary>
        [SqlField]
        public int SortNumber { get; set; }

        /// <summary>
        /// 配置类别 0 广告图 1推荐商家 2置顶商家 3表示平台公告 4表示广告流量嵌入 5表示平台数据设置 6关注公众号
        /// </summary>
        [SqlField]
        public int ConfigType { get; set; }


        /// <summary>
        /// 广告图跳转目标类别 -1不跳转 0店铺 1帖子 2小程序
        /// 流量广告类别 0首页流量广告嵌入 1分类信息页流量广告嵌入 2我的页面底部流量广告嵌入
        /// 如果表示平台数据设置 则这里表示虚拟访问量
        /// 如果是关注公众号 则这里表示 首页展示开关 0关闭 1开启
        /// </summary>
        [SqlField]
        public int ADImgType { get; set; }

        /// <summary>
        /// 广告图 如果ConfigType=3则这里表示店铺公告，ConfigType=4则表示流量广告
        /// 
        /// </summary>
        [SqlField]
        public string ADImg { get; set; }

        /// <summary>
        /// ConfigType=2 广告轮播图 跳转到小程序 小程序appId
        /// </summary>
        [SqlField]
        public string Name { get; set; }

        /// <summary>
        /// 新增时间
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
        /// 状态 0正常 -1删除
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        ///选择店铺时候 objId保存的是否是店铺ID 
        ///0表示不是为之前的数据保存为名片Id 1表示为店铺ID 后台店铺可以新增没有名片的店铺 兼容旧数据
        /// </summary>
        [SqlField]
        public int isStoreID { get; set; }


        /// <summary>
        /// 对应objId 标题
        /// </summary>
        public string ObjName { get; set; }

        /// <summary>
        /// 是否显示编辑排序输入框
        /// </summary>
        public bool IsShowEditSort { get; set; } = false;

        public int storeId { get; set; }

    }

    public class PlatOtherConfig
    {
        public int PlatConfigId { get; set; }
        public int PV { get; set; }

        public int VirtualPV { get; set; }

        public int PlatMsgCount { get; set; }

        public int VirtualPlatMsgCount { get; set; }


    }

}
