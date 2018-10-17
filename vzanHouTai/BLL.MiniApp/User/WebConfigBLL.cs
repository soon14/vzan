using DAL.Base;
using Entity.MiniApp;
using System;
using System.Configuration;
using Utility;

namespace BLL.MiniApp
{
    public class WebConfigBLL : BaseMySql<WebConfig>
    {
        #region 单例模式
        private static WebConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private WebConfigBLL()
        {

        }

        public static WebConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new WebConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public static string WxPayAppId
        {
            get
            {
                string value = RedisUtil.Get<string>("WebConfig_WxPayAppId");
                if (string.IsNullOrEmpty(value))
                {
                    WebConfig model = SingleModel.GetModel("WebKey='WxPayAppId'");
                    if (model == null)
                    {
                        value = string.Empty;
                    }
                    else
                    {
                        value = model.WebValue;
                    }
                    RedisUtil.Set<string>("WebConfig_WxPayAppId", value);
                }
                return value.Trim('"');
            }
        }
        public static string WxAppSecret
        {
            get
            {
                string value = RedisUtil.Get<string>("WebConfig_WxAppSecret");
                if (string.IsNullOrEmpty(value))
                {
                    WebConfig model = SingleModel.GetModel("WebKey='WxAppSecret'");
                    if (model == null)
                    {
                        value = string.Empty;
                    }
                    else
                    {
                        value = model.WebValue;
                    }
                    RedisUtil.Set<string>("WebConfig_WxAppSecret", value);
                }
                return value.Trim('"');
            }
        }
        public static string MCHID
        {
            get
            {
                string value = RedisUtil.Get<string>("WebConfig_MCHID");
                if (string.IsNullOrEmpty(value))
                {
                    WebConfig model = SingleModel.GetModel("WebKey='MCHID'");
                    if (model == null)
                    {
                        value = string.Empty;
                    }
                    else
                    {
                        value = model.WebValue;
                    }
                    RedisUtil.Set<string>("WebConfig_MCHID", value);
                }
                return value.Trim('"');
            }
        }
        public static string OssReplaceOrgDomain
        {
            get
            {
                string value = RedisUtil.Get<string>("WebConfig_OssReplaceOrgDomain");
                if (string.IsNullOrEmpty(value))
                {
                    WebConfig model = SingleModel.GetModel("WebKey='OssReplaceOrgDomain'");
                    if (model == null)
                    {
                        value = "oss.vzan";
                    }
                    else
                    {
                        value = model.WebValue;
                    }
                    RedisUtil.Set<string>("WebConfig_OssReplaceOrgDomain", value);
                }
                return value.Trim('"');
            }
        }
        public static string OssReplaceThumbnailDomain
        {
            get
            {
                string value = RedisUtil.Get<string>("WebConfig_OssReplaceThumbnailDomain");
                if (string.IsNullOrEmpty(value))
                {
                    WebConfig model = SingleModel.GetModel("WebKey='OssReplaceThumbnailDomain'");
                    if (model == null)
                    {
                        value = "i.vzan";
                    }
                    else
                    {
                        value = model.WebValue;
                    }
                    RedisUtil.Set<string>("OssReplaceThumbnailDomain", value);
                }
                return value.Trim('"');
            }
        }
        public static string VZAN_DOMAIN
        {
            get
            {
                string value = GetValue("VZAN_DOMAIN");
                if (string.IsNullOrEmpty(value))
                {
                    value = "vzan.com";
                }
                return value;
            }
        }
        public static bool UseSameVoiceBucket
        {
            get
            {
                string value = GetValue("UseSameVoiceBucket");
                if (string.IsNullOrEmpty(value))
                {
                    value = "0";
                }
                return value == "1";
            }
        }
        public static string citynotify_url
        {
            get
            {
                return WebSiteConfig.WebConfigMiappNotifyUrl;
                //TODO 测试回调地址
                //return "http://testwtApi.vzan.com/apiMiapp/paynotify"; //小程序回调地址
                //string value = RedisUtil.Get<string>("WebConfig_MiappNotify_Url");
                
                //if (string.IsNullOrEmpty(value))
                //{
                //    WebConfig model = SingleModel.GetModel("WebKey='MiappNotify_Url'");
                //    if (model == null)
                //    {
                //        value = string.Empty;
                //    }
                //    else
                //    {
                //        value = model.WebValue;
                //    }
                //    RedisUtil.Set<string>("WebConfig_MiappNotify_Url", value);
                //}
                //return value.Trim('"');
            }
        }
        public static string GetValue(string key)
        {
            string value = RedisUtil.Get<string>("WebConfig_" + key);
            if (string.IsNullOrEmpty(value))
            {
                WebConfig model = SingleModel.GetModel("WebKey='" + key + "'");
                if (model == null)
                {
                    value = string.Empty;
                }
                else
                {
                    value = model.WebValue;
                }
                RedisUtil.Set<string>("WebConfig_" + key, value);
            }
            return value.Trim('"');
        }
        public static string KEY
        {
            get
            {
                string value = RedisUtil.Get<string>("WebConfig_KEY");
                if (string.IsNullOrEmpty(value))
                {
                    WebConfig model = SingleModel.GetModel("WebKey='KEY'");
                    if (model == null)
                    {
                        value = string.Empty;
                    }
                    else
                    {
                        value = model.WebValue;
                    }
                    RedisUtil.Set<string>("WebConfig_KEY", value);
                }
                return value.Trim('"');
            }
        }
        public static bool WzLivePay
        {
            get
            {
                int value = RedisUtil.Get<int>("WebConfig_WzLivePay");
                if (value == 0)
                {
                    WebConfig model = SingleModel.GetModel("WebKey='WzLivePay'");
                    if (model == null)
                    {
                        value = 0;
                    }
                    else
                    {
                        value = int.Parse(model.WebValue);
                    }
                    RedisUtil.Set<int>("WebConfig_WzLivePay", value == 0 ? -1 : value, TimeSpan.FromDays(1));
                }
                return value == 1 ? true : false;
            }
        }
        

        /// <summary>
        /// 黄图的报警值
        /// </summary>
        public static float ImageDetectionRate
        {
            get
            {
                float value = RedisUtil.Get<int>("WebConfig_ImageDetectionRate");
                if (value == 0)
                {
                    WebConfig model = SingleModel.GetModel("WebKey='ImageDetectionRate'");
                    if (model == null)
                    {
                        value = 95;
                    }
                    else
                    {
                        float.TryParse(model.WebValue, out value);
                    }
                    RedisUtil.Set("WebConfig_ImageDetectionRate", value, TimeSpan.FromDays(1));
                }
                return value;
            }
        }
        /// <summary>
        /// cookiedomain
        /// </summary>
        public static string CookieDomain
        {
            get
            {
                return ConfigurationManager.AppSettings["cookiedomain"] ?? ".vzan.com";
            }
        }
        /// <summary>
        /// cookiedomain
        /// </summary>
        public static string ZBtopapi
        {
            get
            {
                return ConfigurationManager.AppSettings["zbtpcapi"];
            }
        }
        
        /// <summary>
        /// 物流信息查询接口:商户号
        /// </summary>
        public static string MerchIdForDeliveryAPi
        {
            get
            {
                return GetValueOrDefault(key: "MerchIdForDeliveryAPi", defaultValue: "1336595");
            }
        }

        /// <summary>
        /// 物流信息查询接口:签名密钥
        /// </summary>
        public static string AuthKeyForDeliveryAPI
        {
            get
            {
                return GetValueOrDefault(key: "AuthKeyForDeliveryAPI", defaultValue: "dcf8d02b-c3ed-4f48-8964-6e4ee6146a38");
            }
        }

        public static string GetValueOrDefault(string key, string defaultValue)
        {
            string value = RedisUtil.Get<string>("WebConfig_" + key);
            if (string.IsNullOrWhiteSpace(value))
            {
                WebConfig model = SingleModel.GetModel("WebKey='" + key + "'");
                if (model == null)
                {
                    value = defaultValue;
                }
                else
                {
                    value = model.WebValue;
                }
                RedisUtil.Set<string>("WebConfig_" + key, value);
            }
            return value.Trim('"');
        }
    }
}
