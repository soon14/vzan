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
    /// 申请独立小程序记录
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatApplyApp
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public long UserId { get; set; }
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
        public string Desc { get; set; }
        /// <summary>
        /// 绑定平台的AID
        /// </summary>
        [SqlField]
        public int BindAId { get; set; } = 0;
        /// <summary>
        /// 店铺ID
        /// </summary>
        [SqlField]
        public int StoreId { get; set; } = 0;
        /// <summary>
        /// 名片ID
        /// </summary>
        [SqlField]
        public int MycardId { get; set; } = 0;
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// 模板状态，-1：停用，大于0：启用
        /// </summary>
        public int XcxAppState { get; set; }
        public string XcxAppStateStr { get { return XcxAppState == -1 ? "停用" : "正常"; } }
        /// <summary>
        /// 开通状态，0：未开通，1：已开通
        /// </summary>
        [SqlField]
        public int OpenState { get; set; } = 0;

        /// <summary>
        /// 开通时间
        /// </summary>
        [SqlField]
        public DateTime OpenTime { get; set; }
        public string OpenTimeStr { get { return OpenTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 平台店铺名称
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// 平台会员账号
        /// </summary>
        public string LoginId { get; set; }
        /// <summary>
        /// 客户姓名
        /// </summary>
        public string CustomerName { get; set; }
        public DateTime OutTime { get; set; }
        /// <summary>
        /// 到期时间
        /// </summary>
        public string OutTimeStr { get { return OutTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 有效天数
        /// </summary>
        public int ValDayLength { get; set; }
    }
}
