using Entity.MiniApp.Conf;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.MiniApp.ViewModel
{
    public class ConfigViewModel
    {
        /// <summary>
        /// 小程序授权信息
        /// </summary>
        public List<Entity.MiniApp.OpenAuthorizerConfig> Openconfigs { get; set; }
        /// <summary>
        /// 小程序授权信息
        /// </summary>
        public Entity.MiniApp.OpenAuthorizerConfig Openconfig { get; set; }
        /// <summary>
        /// 小程序模板
        /// </summary>
        public XcxTemplate XcxTemplate { get; set; }
        /// <summary>
        /// 上传记录
        /// </summary>
        public UserXcxTemplate UserXcxTemplate { get; set; }
        /// <summary>
        /// 小程序二维码图片路径
        /// </summary>
        public string miniappqrcode { get; set; }


        /// <summary>
        /// 授权链接
        /// </summary>
        public string AuthodUrl { get; set; }
        /// <summary>
        /// 商户账号信息
        /// </summary>

        public PayCenterSetting paycenter { get; set; }
        /// <summary>
        /// 安装证书信息
        /// </summary>
        public CertInstall CerInstallInfo{get;set;}
        
        public XcxAppAccountRelation XcxRelationModel { get; set; }
    }
}