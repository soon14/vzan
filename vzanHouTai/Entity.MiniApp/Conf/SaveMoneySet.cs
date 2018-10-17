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
    /// 储值优惠设定表
    /// </summary>
    /// [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class SaveMoneySet
    {
        public SaveMoneySet() { }

        /// <summary>
        /// 
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }


        /// <summary>
        /// 权限表Id
        /// </summary>
        [SqlField]
        public string AppId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public string SetName { get; set; }


        /// <summary>
        /// 充值金额
        /// </summary>
        [SqlField]
        public int JoinMoney { get; set; }

        /// <summary>
        /// 充值金额
        /// </summary>
        public string JoinMoneyStr { get { return (JoinMoney * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 赠送金额
        /// </summary>
        [SqlField]
        public int GiveMoney { get; set; }

        /// <summary>
        /// 赠送金额
        /// </summary>
        public string GiveMoneyStr { get { return (GiveMoney * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 到帐金额
        /// </summary>
        [SqlField]
        public int AmountMoney { get; set; }

        /// <summary>
        /// 到帐金额
        /// </summary>
        public string AmountMoneyStr { get { return (AmountMoney * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 状态(-1 删除 / 0下架 / 1上架)
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// 记录创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }
        

    }
}
