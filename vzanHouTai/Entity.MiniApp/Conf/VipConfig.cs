using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 会员配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class VipConfig
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        [SqlField]
        public string appId { get; set; } = string.Empty;

        /// <summary>
        /// 行业类型
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;

        /// <summary>
        /// 累计消费金额是否自动升级 0：关闭    1:开启
        /// </summary>
        [SqlField]
        public int autoupdate { get; set; } = 0;

        public bool autoswitch
        {
            get
            {
                return autoupdate == 1;
            }
        }
        /// <summary>
        /// 是否自动同步到微信卡包 0:关闭 1:开启
        /// </summary>
        [SqlField]
        public int autosynccard { get; set; } = 0;

        public bool autosynccardswitch
        {
            get { return this.autosynccard == 1; }
        }

        /// <summary>
        /// 状态 0:正常
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updatetime { get; set; }


        /// <summary>
        /// 充值金额是否自动升级 0：关闭    1:开启
        /// </summary>
        [SqlField]
        public int SaveMoneyAutoUpdate { get; set; } = 0;

        public bool SaveMoneyAutoUpdateSwtich
        {
            get
            {
                return SaveMoneyAutoUpdate == 1;
            }
        }




        /// <summary>
        /// 0表示单次储值金额计算  1表示累计储值金额计算
        /// </summary>
        [SqlField]
        public int SaveMoneyType { get; set; } = 0;

        public string SaveMoneyTypeTxt
        {
          
            get
            {
              return  SaveMoneyType == 0 ? "单次充值金额" : "累计充值金额";
            }
        }


    }
}
