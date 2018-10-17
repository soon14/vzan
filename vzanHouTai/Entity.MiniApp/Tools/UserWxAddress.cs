using Entity.Base;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    /// <summary>
    /// 小程序用户微信地址
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class UserWxAddress
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 用户Id 关联 C_userInfo 里的Id
        /// </summary>
        [SqlField]
        public int UserId { get; set; } = 0;

        /// <summary>
        /// 微信地址
        /// </summary>
        [SqlField]
        public string WxAddress { get; set; } = string.Empty;


    }

    public class WxAddress
    {
        public WxAddress()
        {

        }
        
        public string userName { get; set;}
        
        public string postalCode { get; set; }

        public string provinceName { get; set; }

        public string cityName { get; set; }

        public string countyName { get; set; }

        public string detailInfo { get; set; }

        public string telNumber { get; set; }
    }
}
