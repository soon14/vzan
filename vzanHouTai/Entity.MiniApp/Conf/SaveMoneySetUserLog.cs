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
    /// 储值记录表
    /// </summary>
    /// [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class SaveMoneySetUserLog
    {
        public SaveMoneySetUserLog() { }

        /// <summary>
        /// 
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }


        /// <summary>
        /// appid
        /// </summary>
        [SqlField]
        public string AppId { get; set; }
        /// <summary>
        /// 权限表Id
        /// </summary>
        [SqlField]
        public int AId { get; set; }

        /// <summary>
        /// 用户表Id
        /// </summary>
        [SqlField]
        public int UserId { get; set; }

        /// <summary>
        /// 用户储值情况记录表Id
        /// </summary>
        [SqlField]
        public int MoneySetUserId { get; set; }

        /// <summary>
        /// 储值总记录表Id
        /// </summary>
        [SqlField]
        public int MoneySetId { get; set; }

        /// <summary>
        /// 类型 0为储值 / -1消费 / 1为退款/
        /// </summary>
        [SqlField]
        public int Type { get; set; }


        /// <summary>
        /// 变更前金额
        /// </summary>
        [SqlField]
        public int BeforeMoney { get; set; }

        /// <summary>
        /// 变更前金额
        /// </summary>
        public string BeforeMoneyStr { get { return (BeforeMoney * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 变更后金额
        /// </summary>
        [SqlField]
        public int AfterMoney { get; set; }

        /// <summary>
        /// 变更后金额
        /// </summary>
        public string AfterMoneyStr { get { return (AfterMoney * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 变动额度
        /// </summary>
        [SqlField]
        public int ChangeMoney { get; set; }

        /// <summary>
        /// 变动额度
        /// </summary>
        public string ChangeMoneyStr { get { return (ChangeMoney * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 变动原因
        /// </summary>
        [SqlField]
        public string ChangeNote { get; set; }

        /// <summary>
        /// 变更时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }


        /// <summary>
        /// 变更时间
        /// </summary>
        public string CreateDateStr { get { return CreateDate.ToString("yyyy-MM-dd HH:mm:ss"); } }


        /// <summary>
        /// 0无效/1有效
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// CityMorder 订单Id
        /// </summary>
        [SqlField]
        public int OrderId { get; set; }


        /// <summary>
        /// 门店Id
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }

        /// <summary>
        /// 本次变动赠送金额
        /// </summary>
        [SqlField]
        public int GiveMoney { get; set; }


    }


    /// <summary>
    /// 储值记录表
    /// </summary>
    /// [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class SaveMoneySetUserLogView
    {
        public SaveMoneySetUserLogView() { }

        /// <summary>
        /// 昵称
        /// </summary>
        [SqlField]
        public string NickName { get; set; }

        /// <summary>
        /// 头像路径
        /// </summary>
        [SqlField]
        public string HeadImgUrl { get; set; }

        
        /// <summary>
        /// 变更前金额
        /// </summary>
        [SqlField]
        public int BeforeMoney { get; set; }

        /// <summary>
        /// 变更前金额
        /// </summary>
        public string BeforeMoneyStr { get { return (BeforeMoney * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 变更后金额
        /// </summary>
        [SqlField]
        public int AfterMoney { get; set; }

        /// <summary>
        /// 变更后金额
        /// </summary>
        public string AfterMoneyStr { get { return (AfterMoney * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 变动额度
        /// </summary>
        [SqlField]
        public int ChangeMoney { get; set; }

        /// <summary>
        /// 变动额度
        /// </summary>
        public string ChangeMoneyStr { get { return (ChangeMoney * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 变动原因
        /// </summary>
        [SqlField]
        public string ChangeNote { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        [SqlField]
        public int Type { get; set; }


        /// <summary>
        /// 类型
        /// </summary>
        [SqlField]
        public string TypeStr { get; set; }

        /// <summary>
        /// 变更时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }


        /// <summary>
        /// 变更时间
        /// </summary>
        public string CreateDateStr { get { return CreateDate.ToString("yyyy-MM-dd HH:mm:ss"); } }

    }
}
