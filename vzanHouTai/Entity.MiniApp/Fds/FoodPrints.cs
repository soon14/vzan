using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Fds
{
    /// <summary>
    /// 小程序餐饮版-打印机管理
    /// </summary>
    [Serializable]
    [SqlTable(Utility.dbEnum.MINIAPP)]
    public class FoodPrints
    {
        public FoodPrints() { }
        /// <summary>
        /// 属性ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 商家id
        /// </summary>
        [SqlField]
        public int FoodStoreId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }

        /// <summary>
        /// 打印机类型:属于什么牌子的打印机
        /// </summary>
        [SqlField]
        public int Type { get; set; } = 1;

        /// <summary>
        /// 打印机终端号
        /// </summary>
        [SqlField]
        public string PrintNo { get; set; }

        /// <summary>
        /// 打印机密钥
        /// </summary>
        [SqlField]
        public string PrintKey { get; set; }

        /// <summary>
        /// 打印机注册用户ID
        /// </summary>
        [SqlField]
        public string UserId { get; set; }


        /// <summary>
        /// 打印机注册用户名称
        /// </summary>
        [SqlField]
        public string UserName { get; set; }

        /// <summary>
        /// api密钥
        /// </summary>
        [SqlField]
        public string APIKey { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; }


        /// <summary>
        /// 打印单数 --无用
        /// </summary>
        public int PrintCount { get; set; }

        /// <summary>
        /// 状态 0正常 -1删除
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        [SqlField]
        public string Telphone { get; set; }

        /// <summary>
        /// 自动打印 默认1为是  --留待扩展,暂无用
        /// </summary>
        [SqlField]
        public int AutoPrint { get; set; } = 1;

        /// <summary>
        /// account表的openid
        /// </summary>
        [SqlField]
        public string accountId { get; set; }

        /// <summary>
        /// 行业类型：1:餐饮   2:电商   --作废,不需以此列来标识
        /// </summary>
        [SqlField]
        public int industrytype { get; set; } = 0;

        /// <summary>
        /// 权限表ID
        /// </summary>
        [SqlField]
        public int appId { get; set; }


        /// <summary>
        /// 打印机的功能类型：默认 0总票打印机  1单票打印机(按份数 2单票打印机(按菜品)
        /// </summary>
        [SqlField]
        public int printType { get; set; } = 0;

    }
}
