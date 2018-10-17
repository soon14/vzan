using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
//using BLL.MiniApp;
using BLL.MiniApp.Ent;
using Entity.MiniApp.Ent;
using Core.MiniApp;

namespace BLL.MiniApp
{
    public class WxPayApi
    {
        private static System.Random ran = new System.Random(new Guid().GetHashCode());
        /// <summary>
        ///  查询订单
        /// </summary>
        /// <param name="inputObj">提交给查询订单API的参数</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns>成功时返回订单查询结果</returns>
        public static WxPayData OrderQuery(WxPayData inputObj, PayCenterSetting setting, int timeOut = 60)
        {
            WxPayData result = new WxPayData();
            string url = "https://api.mch.weixin.qq.com/pay/orderquery";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no") && !inputObj.IsSet("transaction_id"))
            {
                throw new WxPayException("订单查询接口中，out_trade_no、transaction_id至少填一个！");
            }
            string appid = WxPayConfig.APPID;
            string mch_id = WxPayConfig.MCHID;
            string key = string.Empty;
            if (setting != null && setting.Id > 0)
            {
                appid = setting.Appid;
                mch_id = setting.Mch_id;
                key = setting.Key;
            }
            inputObj.SetValue("appid", appid);//公众账号ID
            inputObj.SetValue("mch_id", mch_id);//商户号
            inputObj.SetValue("nonce_str", WxPayApi.GenerateNonceStr());//随机字符串
            inputObj.SetValue("sign", inputObj.MakeSign(key));//签名

            string xml = inputObj.ToXml();

            DateTime start = DateTime.Now;
            string response = string.Empty;
            try
            {
                response = WxHelper.Post(xml, url, false, setting, timeOut);//调用HTTP通信接口提交数据
            }
            catch (WxPayException)
            {
                log4net.LogHelper.WriteInfo(typeof(WxPayApi),$"微信订单查询超时：{(inputObj.GetValue("transaction_id") == null ? "" : inputObj.GetValue("transaction_id").ToString())}");
                result.SetValue("return_code", "FAILE");
                return result;
            }

            //将xml格式的数据转化为对象以返回
            result.FromXml(response);
            return result;
        }
        
        /// <summary>
        /// 统一下单
        /// </summary>
        /// <param name="inputObj">提交给统一下单API的参数</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        public static WxPayData UnifiedOrder(WxPayData inputObj, PayCenterSetting setting, int timeOut = 60, bool livePay = false)
        {
            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no"))
            {
                throw new WxPayException("缺少统一支付接口必填参数out_trade_no！");
            }
            else if (!inputObj.IsSet("body"))
            {
                throw new WxPayException("缺少统一支付接口必填参数body！");
            }
            else if (!inputObj.IsSet("total_fee"))
            {
                throw new WxPayException("缺少统一支付接口必填参数total_fee！");
            }
            else if (!inputObj.IsSet("trade_type"))
            {
                throw new WxPayException("缺少统一支付接口必填参数trade_type！");
            }

            //关联参数
            if (inputObj.GetValue("trade_type").ToString() == "JSAPI" && !inputObj.IsSet("openid"))
            {
                throw new WxPayException("统一支付接口中，缺少必填参数openid！trade_type为JSAPI时，openid为必填参数！");
            }
            if (inputObj.GetValue("trade_type").ToString() == "NATIVE" && !inputObj.IsSet("product_id"))
            {
                throw new WxPayException("统一支付接口中，缺少必填参数product_id！trade_type为JSAPI时，product_id为必填参数！");
            }
            //异步通知url未设置，则使用配置文件中的url
            if (!inputObj.IsSet("notify_url") || string.IsNullOrEmpty(inputObj.GetValue("notify_url").ToString()))
            {
                inputObj.SetValue("notify_url", WxPayConfig.NOTIFY_URL);//异步通知url
            }
            if (livePay)
            {
                inputObj.SetValue("notify_url", inputObj.GetValue("notify_url").ToString().Replace("/pay/", "/live/"));//异步通知url,直播要跳到直播回调
            }
            string appid = WxPayConfig.APPID;
            string mch_id = WxPayConfig.MCHID;
            string key = string.Empty;
            if (setting != null && setting.Id > 0)
            {
                appid = setting.Appid;
                mch_id = setting.Mch_id;
                key = setting.Key;
            }
            inputObj.SetValue("appid", appid);//公众账号ID
            inputObj.SetValue("mch_id", mch_id);//商户号
            inputObj.SetValue("spbill_create_ip", WxPayConfig.IP);//终端ip	  	    
            inputObj.SetValue("nonce_str", GenerateNonceStr());//随机字符串


            //log4net.LogHelper.WriteInfo(typeof(WxPayApi), inputObj.ToJson());
            //签名
            inputObj.SetValue("sign", inputObj.MakeSign(key));
            string xml = inputObj.ToXml();
            string response = WxHelper.Post(xml, url, false, setting, timeOut);
            //log4net.LogHelper.WriteInfo(typeof(WxPayApi), "UnifiedOrder().response=" + response);

            WxPayData result = new WxPayData();
            result.FromXml(response);
            return result;
        }

        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="inputObj">提交给关闭订单API的参数</param>
        /// <param name="timeOut">接口超时时间</param>
        /// <returns></returns>
        public static WxPayData CloseOrder(WxPayData inputObj, PayCenterSetting setting, int timeOut = 20)
        {
            string url = "https://api.mch.weixin.qq.com/pay/closeorder";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no"))
            {
                throw new WxPayException("关闭订单接口中，out_trade_no必填！");
            }
            string appid = WxPayConfig.APPID;
            string mch_id = WxPayConfig.MCHID;
            string key = string.Empty;
            if (setting != null && setting.Id > 0)
            {
                appid = setting.Appid;
                mch_id = setting.Mch_id;
                key = setting.Key;
            }
            inputObj.SetValue("appid", appid);//公众账号ID
            inputObj.SetValue("mch_id", mch_id);//商户号
            inputObj.SetValue("nonce_str", GenerateNonceStr());//随机字符串		
            inputObj.SetValue("sign", inputObj.MakeSign(key));//签名
            string xml = inputObj.ToXml();
            string response = WxHelper.Post(xml, url, false, setting, timeOut);

            WxPayData result = new WxPayData();
            result.FromXml(response);

            return result;
        }
        
        /// <summary>
        /// 根据当前系统时间加随机序列来生成订单号
        /// </summary>
        /// <returns>订单号</returns>
        public static string GenerateOutTradeNo()
        {
            return string.Format("{0}{1}{2}", WxPayConfig.MCHID, DateTime.Now.ToString("yyMMddHHmmssff"), ran.Next(999));
        }

        /// <summary>
        /// 生成时间戳，标准北京时间，时区为东八区，自1970年1月1日 0点0分0秒以来的秒数
        /// </summary>
        /// <returns>时间戳</returns>
        public static string GenerateTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// 生成随机串，随机串包含字母或数字
        /// </summary>
        /// <returns>随机串</returns>
        public static string GenerateNonceStr()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        /// <summary>
        /// 企业付款
        /// </summary>
        /// <param name="inputObj">企业付款接口的参数</param>
        /// <param name="timeOut">企业付款接口超时时间</param>
        /// <returns></returns>
        public static WxPayData CompanyPay(WxPayData inputObj, PayCenterSetting setting, int timeOut = 20)
        {
            string url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/promotion/transfers";
            string appid = WxPayConfig.APPID;
            string mch_id = WxPayConfig.MCHID;
            string key = string.Empty;
            if (setting != null && setting.Id > 0)
            {
                appid = setting.Appid;
                mch_id = setting.Mch_id;
                key = setting.Key;
            }
            inputObj.SetValue("mch_appid", appid);//公众账号ID
            inputObj.SetValue("mchid", mch_id);//商户号
            inputObj.SetValue("spbill_create_ip", WxPayConfig.IP);//IP
            inputObj.SetValue("nonce_str", GenerateNonceStr());//随机字符串	
            inputObj.SetValue("check_name", "NO_CHECK");//校验用户姓名,不需要校验,必选项，可选类型有:	
            inputObj.SetValue("sign", inputObj.MakeSign(key));//签名
                                                              //NO_CHECK：不校验真实姓名
                                                              //FORCE_CHECK：强校验真实姓名（未实名认证的用户会校验失败，无法转账） 
                                                              //OPTION_CHECK：针对已实名认证的用户才校验真实姓名（未实名认证用户不校验，可以转账成功）
                                                              //  log4net.LogHelper.WriteInfo(typeof(CompanyPay), "用户提现请求参数：" + inputObj.ToJson());
            DrawcashResult drawresult = new DrawcashResult();
            if (setting != null)
            {
                drawresult.mch_id = setting.Mch_id;
                drawresult.appid = setting.Appid;
            }
            //检测必填参数
            if (!inputObj.IsSet("mch_appid"))
            {
                throw new WxPayException("企业付款中，公众账号appID必填！");
            }
            if (!inputObj.IsSet("mchid"))
            {
                throw new WxPayException("企业付款中，微信支付分配的商户号mchid必填！");
            }
            if (!inputObj.IsSet("nonce_str"))
            {
                throw new WxPayException("企业付款中，随机字符串nonce_str必填！");
            }
            else
            {
                drawresult.nonce_str = inputObj.GetValue("nonce_str").ToString();
            }
            if (!inputObj.IsSet("sign"))
            {
                throw new WxPayException("企业付款中，签名sign必填！");
            }
            else
            {
                drawresult.sign = inputObj.GetValue("sign").ToString();
            }
            if (!inputObj.IsSet("partner_trade_no"))
            {
                throw new WxPayException("企业付款中，商户订单号partner_trade_no必填！");
            }
            else
            {
                drawresult.partner_trade_no = inputObj.GetValue("partner_trade_no").ToString();
            }
            if (!inputObj.IsSet("openid"))
            {
                throw new WxPayException("企业付款中，用户openid必填！");
            }
            else
            {
                drawresult.openid = inputObj.GetValue("openid").ToString();
            }
            string error = string.Empty;
            //if (new DrawBackUserBLL().CheckEnable(0, 0, drawresult.openid))
            //{
            //    throw new WxPayException("黑名单！");
            //}

            if (!inputObj.IsSet("check_name"))
            {
                throw new WxPayException("企业付款中，校验用户姓名check_name必填！");
            }
            else
            {
                drawresult.check_name = inputObj.GetValue("check_name").ToString();
            }
            if (!inputObj.IsSet("amount"))
            {
                throw new WxPayException("企业付款中，金额amount必填！");
            }
            else
            {
                drawresult.amount = Convert.ToInt32(inputObj.GetValue("amount").ToString());
            }
            if (!inputObj.IsSet("desc"))
            {
                throw new WxPayException("企业付款中，企业付款描述信息desc必填！");
            }
            else
            {
                drawresult.desc = inputObj.GetValue("desc").ToString();
                if (inputObj.GetValue("desc").ToString().Length >= 50)
                {
                    inputObj.SetValue("desc", inputObj.GetValue("desc").ToString().Substring(0, 46) + "...");
                }
            }
            if (!inputObj.IsSet("spbill_create_ip"))
            {
                throw new WxPayException("企业付款中，Ip地址spbill_create_ip必填！");
            }
            else
            {
                drawresult.spbill_create_ip = inputObj.GetValue("spbill_create_ip").ToString();
            }
            if (inputObj.IsSet("re_user_name"))
            {
                drawresult.re_user_name = inputObj.GetValue("re_user_name").ToString();
            }
            string xml = inputObj.ToXml();
            drawresult.CreateTime = DateTime.Now;
            //post之前，增加记录
            

            int id = Convert.ToInt32(DrawcashResultBLL.SingleModel.Add(drawresult));
            drawresult.id = id;
            //
            try
            {
                string response = WxHelper.Post(xml, url, true, setting, 30);
                if (string.IsNullOrEmpty(response))
                {
                    return null;
                }
                WxPayData result = new WxPayData();
                result.FromXml(response, false);
                if (result != null)
                {
                    drawresult.return_code = result.GetValue("return_code") == null ? "" : result.GetValue("return_code").ToString();
                    drawresult.return_msg = result.GetValue("return_msg") == null ? "" : result.GetValue("return_msg").ToString();
                    drawresult.nonce_str = result.GetValue("nonce_str") == null ? "" : result.GetValue("nonce_str").ToString();
                    drawresult.result_code = result.GetValue("result_code") == null ? "" : result.GetValue("result_code").ToString();
                    drawresult.err_code = result.GetValue("err_code") == null ? "" : result.GetValue("err_code").ToString();
                    drawresult.err_code_des = result.GetValue("err_code_des") == null ? "" : result.GetValue("err_code_des").ToString();
                    drawresult.payment_time = DateTime.Now;
                    drawresult.payment_no = result.GetValue("payment_no") == null ? "" : result.GetValue("payment_no").ToString();
                    DrawcashResultBLL.SingleModel.Update(drawresult);
                }
                return result;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(WxPayApi), new Exception("提现出现错误，错误信息：" + ex.Message));
                return null;
            }
        }
    }
}