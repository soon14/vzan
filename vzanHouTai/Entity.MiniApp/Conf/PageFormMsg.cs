using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]

    ///小程序首页控件配置
    public class PageFormMsg
    {

        public PageFormMsg() { }
        /// <summary>
        /// ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

      

        /// <summary>
        /// 表单信息
        /// </summary>
        [SqlField]
        public string FormMsg { get; set; } = string.Empty;

        /// <summary>
        /// 表单名称
        /// </summary>
        [SqlField]
        public string FormTitle { get; set; } = string.Empty;



        /// <summary>
        ///提交时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; } = DateTime.Now;


        /// <summary>
        /// 权限Id
        /// </summary>
        [SqlField]
        public int Rid { get; set; } = 0;


        /// <summary>
        /// userId
        /// </summary>
        [SqlField]
        public int UserId { get; set; } =0;

        /// <summary>
        /// 状态 -1表示删除
        /// </summary>
        [SqlField]
        public int State { get; set; } = 1;
    }





}
