using Entity.MiniApp;
using System;
using System.Threading;
using Utility;

namespace BLL.MiniApp
{
    public static class C_TplMsgHelper
    {
        /// <summary>
        /// 写死微赞公众号推送，异步(支付成功)
        /// </summary>
        /// <param name="minisnsId"></param>
        /// <param name="openId"></param>
        /// <param name="url"></param>
        /// <param name="title"></param>
        /// <param name="name"></param>
        /// <param name="remark"></param>
        /// <param name="mc"></param>
        public static void SendTplMsgFromVzan(string openId, string url, string title,string ordernum,string buyMoney, string userName,string shopName, string remark = "")
        {
            //Action actionfunc = delegate
            //{
            try
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    if (!string.IsNullOrEmpty(openId))
                    {
                        string strMsg, urls;
                        urls = string.Format(WebSiteConfig.OpenDomain + "/cgibin/message/template?access_token={0}", WebSiteConfig.DZ_WxSerId);
                        string tid = WebSiteConfig.DZ_paySuccessTemplateId;
                        strMsg = GetTemplateModuleMsg2(tid, openId, url, title, userName, ordernum, buyMoney, shopName, remark);
                        string result = HttpHelper.PostData(urls, strMsg);
                    }
                    
                }, null);

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(C_TplMsgHelper), ex);
            }
        }


        /// <summary>
        /// 写死微赞公众号推送，异步
        /// </summary>
        /// <param name="minisnsId"></param>
        /// <param name="openId"></param>
        /// <param name="url"></param>
        /// <param name="title"></param>
        /// <param name="name"></param>
        /// <param name="remark"></param>
        /// <param name="mc"></param>
        public static void SendTplMsgFromVzan_OutOrderApply(string openId, string url, string title, string ordernum, string buyMoney, string userName, string shopName,int shopCount ,string remark = "")
        {
            try
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    if (!string.IsNullOrEmpty(openId))
                    {
                        string strMsg, urls;
                        urls = string.Format(WebSiteConfig.OpenDomain + "/cgibin/message/template?access_token={0}", WebSiteConfig.DZ_WxSerId);
                        string tid = WebSiteConfig.DZ_outOrderTemplateId;
                        strMsg = GetTemplateModuleMsg_OutOrder(tid, openId, url, title, userName, ordernum, buyMoney, shopName, shopCount, remark);
                        string result =  HttpHelper.PostData(urls, strMsg);
                    }

                }, null);

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(C_TplMsgHelper), ex);
            }
        } 
        //public static string GetTemplateModuleMsg(MinisnsConfig mc, string openId, string url, string title, string name, string remark)
        //{
        //    return Utility.Serialize.SerializeHelper.SerToJson(new C_TemplateModule
        //    {
        //        touser = openId,
        //        template_id = mc.TemplateId,
        //        url = url,
        //        data = new C_Data
        //        {
        //            first = new C_DataObj { color = "#111111", value = title },
        //            keyword1 = new C_DataObj { color = "#333333", value = name },
        //            keyword2 = new C_DataObj { color = "#333333", value = DateTime.Now.ToString("yyyy-MM-dd HH:mm") },
        //            keyword3 = new C_DataObj { color = "#333333", value = "点击查看详情" },
        //            remark = new C_DataObj { color = "#17b5ee", value = remark }
        //        }
        //    });
        //}

        public static string GetTemplateModuleMsg2(string template_id, string openId, string url, string title, string userName, string orderNum, string buyMoney, string shopName, string remark = "")
        {
            return  SerializeHelper.SerToJson(new C_TemplateModule2
            {
                touser = openId,
                template_id = template_id,
                url = url,
                data = new C_Data2
                {
                    first = new C_DataObj { color = "#111111", value = title },
                    keyword1 = new C_DataObj { color = "#333333", value = orderNum },
                    keyword2 = new C_DataObj { color = "#333333", value = shopName },
                    keyword3 = new C_DataObj { color = "#333333", value = buyMoney },
                    remark = new C_DataObj { color = "#17b5ee", value = remark }
                }
            });
        }

        public static string GetTemplateModuleMsg_OutOrder(string template_id, string openId, string url, string title, string userName, string orderNum, string buyMoney, string shopName, int shopCount, string remark = "")
        {
            return  SerializeHelper.SerToJson(new C_TemplateModule_OutOrder
            {
                touser = openId,
                template_id = template_id,
                url = url,
                data = new C_Data_OutOrder
                {
                    first = new C_DataObj { color = "#111111", value = title },
                    keyword1 = new C_DataObj { color = "#333333", value = shopName },
                    keyword2 = new C_DataObj { color = "#333333", value = $"{shopCount}" },
                    keyword3 = new C_DataObj { color = "#333333", value = buyMoney },
                    keyword4 = new C_DataObj { color = "#333333", value = userName },
                    keyword5 = new C_DataObj { color = "#333333", value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    remark = new C_DataObj { color = "#17b5ee", value = remark }
                }
            });
        }

    }
}
