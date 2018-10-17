using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entity.MiniApp.Dish;
using Entity.MiniApp.cityminiapp;

namespace User.MiniApp.Areas.Plat.Models
{
    [Serializable]
    public class ApplyAppView
    {
        /// <summary>
        /// 预存款
        /// </summary>
        public int Deposit { get; set; }
        public string DepositStr { get { return (Deposit * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 是否有开通记录
        /// </summary>
        public bool ExitLog { get; set; }
        /// <summary>
        /// 是否是代理商
        /// </summary>
        public bool IsAgent { get; set; }
        public int AccountRId { get; set; }
    }
}