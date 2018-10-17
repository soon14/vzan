using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.MiniApp.ViewModel
{
    public class OpenTemplateView
    {
        /// <summary>
        /// 自定义表单列表
        /// </summary>
        public string username { get; set; }
        public string LoginId { get; set; }
        public string outtime { get; set; }
        public string addtime { get; set; }
        /// <summary>
        /// 剩余有效天数
        /// </summary>
        public int validday { get; set; } = 0;
        /// <summary>
        /// 买模板时价格
        /// </summary>
        public int price { get; set; }
        public string tname { get; set; }
        /// <summary>
        /// -1：停用，1：正常，2：过期
        /// </summary>
        public int state { get; set; }
        public int id { get; set; }
        public string accountid { get; set; }
        /// <summary>
        /// 模板单价
        /// </summary>
        public int singleprice { get; set; }
        public int tid { get; set; }
        /// <summary>
        /// 模板类型
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 开通单门店价格
        /// </summary>
        public int sprice { get; set; }
        /// <summary>
        /// 开通门店数量
        /// </summary>
        public int scount { get; set; }
        /// <summary>
        /// 是否是体验版
        /// </summary>
        public bool IsExperience { get; set; } = false;

        /// <summary>
        /// 专业版里的 级别 0旗舰版 1尊享版 2高级版 3基础版
        /// </summary>
        public int VersionId { get; set; } = 0;

        /// <summary>
        /// xcxappaccountrelation对应的Id
        /// </summary>
        public int XcxRelationId { get; set; } = 0;
        /// <summary>
        /// 是否已开启水印，0：否，1：是
        /// </summary>
        public int OpenCustomBottom { get; set; }
        public List<ConfParam> ConfParamList { get; set; }
        public string AppId { get; set; } = "";
        public string NickName { get; set; } = "";
    }
}