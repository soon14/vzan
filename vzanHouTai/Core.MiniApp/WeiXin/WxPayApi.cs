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

namespace Core.MiniApp
{
    public class WxPayApi
    {
        private static System.Random ran = new System.Random(new Guid().GetHashCode());

        /// <summary>
        /// 提交被扫支付API,收银员使用扫码设备读取微信用户刷卡授权码以后，二维码或条码信息传送至商户收银台，由商户收银台或者商户后台调用该接口发起支付。
        /// </summary>
        /// <param name="inputObj">提交给被扫支付API的参数</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns>成功时返回调用结果</returns>
        //public static WxPayData Micropay(WxPayData inputObj, PayCenterSetting setting = null, int timeOut = 20)
        //{
        //    string url = "https://api.mch.weixin.qq.com/pay/micropay";
        //    //检测必填参数
        //    if (!inputObj.IsSet("body"))
        //    {
        //        throw new WxPayException("提交被扫支付API接口中，缺少必填参数body！");
        //    }
        //    else if (!inputObj.IsSet("out_trade_no"))
        //    {
        //        throw new WxPayException("提交被扫支付API接口中，缺少必填参数out_trade_no！");
        //    }
        //    else if (!inputObj.IsSet("total_fee"))
        //    {
        //        throw new WxPayException("提交被扫支付API接口中，缺少必填参数total_fee！");
        //    }
        //    else if (!inputObj.IsSet("auth_code"))
        //    {
        //        throw new WxPayException("提交被扫支付API接口中，缺少必填参数auth_code！");
        //    }
        //    string appid = WxPayConfig.APPID;
        //    string mch_id = WxPayConfig.MCHID;
        //    string key = string.Empty;
        //    if (setting != null && setting.Id > 0)
        //    {
        //        appid = setting.Appid;
        //        mch_id = setting.Mch_id;
        //        key = setting.Key;
        //    }
        //    inputObj.SetValue("spbill_create_ip", WxPayConfig.IP);//终端ip
        //    inputObj.SetValue("appid", appid);//公众账号ID
        //    inputObj.SetValue("mch_id", mch_id);//商户号
        //    inputObj.SetValue("nonce_str", Guid.NewGuid().ToString().Replace("-", ""));//随机字符串
        //    inputObj.SetValue("sign", inputObj.MakeSign(key));//签名
        //    string xml = inputObj.ToXml();

        //    //var start = DateTime.Now;//请求开始时间
        //    string response = WxHelper.Post(xml, url, false, setting, timeOut);//调用HTTP通信接口以提交数据到API
        //    // var end = DateTime.Now;
        //    //int timeCost = (int)((end - start).TotalMilliseconds);//获得接口耗时
        //    //将xml格式的结果转换为对象以返回
        //    WxPayData result = new WxPayData();
        //    result.FromXml(response);
        //    //ReportCostTime(url, timeCost, result);//关掉测速上报,
        //    return result;
        //}

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
                PayTimeOutOrder addModel = new PayTimeOutOrder();
                addModel.transaction_id = inputObj.GetValue("transaction_id") == null ? "" : inputObj.GetValue("transaction_id").ToString();
                //记录查询超时的订单
                //new PayTimeOutOrderBLL().Add(addModel);
                result.SetValue("return_code", "FAILE");
                return result;

            }
            // var end = DateTime.Now;
            //int timeCost = (int)((end - start).TotalMilliseconds);//获得接口耗时

            //将xml格式的数据转化为对象以返回

            result.FromXml(response);

            //ReportCostTime(url, timeCost, result);//测速上报

            return result;
        }
        /// <summary>
        ///  查询订单
        /// </summary>
        /// <param name="inputObj">提交给查询订单API的参数</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns>成功时返回订单查询结果</returns>
        //public static string CompanyPayQuery(WxPayData inputObj, PayCenterSetting setting, int timeOut = 60)
        //{
        //    string url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/gettransferinfo";
        //    //检测必填参数
        //    if (!inputObj.IsSet("partner_trade_no"))
        //    {
        //        throw new WxPayException("订单查询接口中，partner_trade_no！");
        //    }
        //    string appid = WxPayConfig.APPID;
        //    string mch_id = WxPayConfig.MCHID;
        //    string key = string.Empty;
        //    if (setting != null && setting.Id > 0)
        //    {
        //        appid = setting.Appid;
        //        mch_id = setting.Mch_id;
        //        key = setting.Key;
        //    }
        //    inputObj.SetValue("appid", appid);//公众账号ID
        //    inputObj.SetValue("mch_id", mch_id);//商户号
        //    inputObj.SetValue("nonce_str", WxPayApi.GenerateNonceStr());//随机字符串
        //    inputObj.SetValue("sign", inputObj.MakeSign(key));//签名

        //    string xml = inputObj.ToXml();
        //    //log4net.LogHelper.WriteError(typeof(CompanyPay), new Exception(xml));
        //    //var start = DateTime.Now;

        //    string response = WxHelper.Post(xml, url, true, setting, timeOut);//调用HTTP通信接口提交数据

        //    //log4net.LogHelper.WriteError(typeof(CompanyPay), new Exception(response));
        //    //var end = DateTime.Now;
        //    //int timeCost = (int)((end - start).TotalMilliseconds);//获得接口耗时

        //    //将xml格式的数据转化为对象以返回
        //    // WxPayData result = new WxPayData();
        //    //result.FromXml(response);

        //    //ReportCostTime(url, timeCost, result);//测速上报

        //    //return result;
        //    return response;
        //}
        /// <summary>
        ///  查询订单
        /// </summary>
        /// <param name="inputObj">提交给查询订单API的参数</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns>成功时返回订单查询结果</returns>
        //public static WxPayData CompanyPayQueryData(WxPayData inputObj, PayCenterSetting setting, int timeOut = 60)
        //{
        //    string url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/gettransferinfo";
        //    //检测必填参数
        //    if (!inputObj.IsSet("partner_trade_no"))
        //    {
        //        throw new WxPayException("订单查询接口中，partner_trade_no！");
        //    }
        //    string appid = WxPayConfig.APPID;
        //    string mch_id = WxPayConfig.MCHID;
        //    string key = string.Empty;
        //    if (setting != null && setting.Id > 0)
        //    {
        //        appid = setting.Appid;
        //        mch_id = setting.Mch_id;
        //        key = setting.Key;
        //    }
        //    inputObj.SetValue("appid", appid);//公众账号ID
        //    inputObj.SetValue("mch_id", mch_id);//商户号
        //    inputObj.SetValue("nonce_str", WxPayApi.GenerateNonceStr());//随机字符串
        //    inputObj.SetValue("sign", inputObj.MakeSign(key));//签名

        //    string xml = inputObj.ToXml();
        //    //log4net.LogHelper.WriteError(typeof(CompanyPay), new Exception(xml))
        //    //log4net.LogHelper.WriteError(typeof(CompanyPay), new Exception(xml));
        //    //var start = DateTime.Now;

        //    string response = WxHelper.Post(xml, url, true, setting, timeOut);//调用HTTP通信接口提交数据

        //    //log4net.LogHelper.WriteError(typeof(CompanyPay), new Exception(response));
        //    //var end = DateTime.Now;
        //    //int timeCost = (int)((end - start).TotalMilliseconds);//获得接口耗时

        //    //将xml格式的数据转化为对象以返回
        //    WxPayData result = new WxPayData();
        //    result.FromXml(response, false);

        //    //ReportCostTime(url, timeCost, result);//测速上报

        //    return result;
        //}

        /// <summary>
        /// 撤销订单API接口
        /// </summary>
        /// <param name="inputObj">提交给撤销订单API接口的参数,out_trade_no和transaction_id必填一个</param>
        /// <param name="timeOut">接口超时时间</param>
        /// <returns>成功时返回API调用结果</returns>
        public static WxPayData Reverse(WxPayData inputObj, PayCenterSetting setting, int timeOut = 60)
        {
            string url = "https://api.mch.weixin.qq.com/secapi/pay/reverse";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no") && !inputObj.IsSet("transaction_id"))
            {
                throw new WxPayException("撤销订单API接口中，参数out_trade_no和transaction_id必须填写一个！");
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

            DateTime start = DateTime.Now;//请求开始时间


            string response = WxHelper.Post(xml, url, true, setting, timeOut);


            DateTime end = DateTime.Now;
            int timeCost = (int)((end - start).TotalMilliseconds);

            WxPayData result = new WxPayData();
            result.FromXml(response);

            ReportCostTime(url, timeCost, result, setting);//测速上报

            return result;
        }

        /// <summary>
        /// 申请退款
        /// </summary>
        /// <param name="inputObj">提交给申请退款API的参数</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns>成功时返回接口调用结果</returns>
        public static WxPayData Refund(WxPayData inputObj, PayCenterSetting setting, int timeOut = 20)
        {
            string url = "https://api.mch.weixin.qq.com/secapi/pay/refund";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no") && !inputObj.IsSet("transaction_id"))
            {
                throw new WxPayException("退款申请接口中，out_trade_no、transaction_id至少填一个！");
            }
            else if (!inputObj.IsSet("out_refund_no"))
            {
                throw new WxPayException("退款申请接口中，缺少必填参数out_refund_no！");
            }
            else if (!inputObj.IsSet("total_fee"))
            {
                throw new WxPayException("退款申请接口中，缺少必填参数total_fee！");
            }
            else if (!inputObj.IsSet("refund_fee"))
            {
                throw new WxPayException("退款申请接口中，缺少必填参数refund_fee！");
            }
            else if (!inputObj.IsSet("op_user_id"))
            {
                throw new WxPayException("退款申请接口中，缺少必填参数op_user_id！");
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
            inputObj.SetValue("nonce_str", Guid.NewGuid().ToString().Replace("-", ""));//随机字符串
            inputObj.SetValue("sign", inputObj.MakeSign(key));//签名
            string xml = inputObj.ToXml();
            //调用HTTP通信接口提交数据到API
            string response = WxHelper.Post(xml, url, true, setting, timeOut);
            //将xml格式的结果转换为对象以返回
            WxPayData result = new WxPayData();
            result.FromXml(response);
            return result;
        }

        /// <summary>
        /// 查询退款,提交退款申请后，通过该接口查询退款状态。退款有一定延时，用零钱支付的退款20分钟内到账，银行卡支付的退款3个工作日后重新查询退款状态。out_refund_no、out_trade_no、transaction_id、refund_id四个参数必填一个
        /// </summary>
        /// <param name="inputObj">提交给查询退款API的参数</param>
        /// <param name="timeOut">接口超时时间</param>
        /// <returns>成功时返回</returns>
        public static WxPayData RefundQuery(WxPayData inputObj, PayCenterSetting setting, int timeOut = 20)
        {
            string url = "https://api.mch.weixin.qq.com/pay/refundquery";
            //检测必填参数
            if (!inputObj.IsSet("out_refund_no") && !inputObj.IsSet("out_trade_no") &&
                !inputObj.IsSet("transaction_id") && !inputObj.IsSet("refund_id"))
            {
                throw new WxPayException("退款查询接口中，out_refund_no、out_trade_no、transaction_id、refund_id四个参数必填一个！");
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

            string response = WxHelper.Post(xml, url, false, setting, timeOut);//调用HTTP通信接口以提交数据到API


            //将xml格式的结果转换为对象以返回
            WxPayData result = new WxPayData();
            result.FromXml(response);

            return result;
        }

        /// <summary>
        /// 下载对账单
        /// </summary>
        /// <param name="inputObj">提交给下载对账单API的参数</param>
        /// <param name="timeOut">接口超时时间</param>
        /// <returns></returns>
        public static WxPayData DownloadBill(WxPayData inputObj, PayCenterSetting setting, int timeOut = 20)
        {
            string url = "https://api.mch.weixin.qq.com/pay/downloadbill";
            //检测必填参数
            if (!inputObj.IsSet("bill_date"))
            {
                throw new WxPayException("对账单接口中，缺少必填参数bill_date！");
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

            string response = WxHelper.Post(xml, url, false, setting, timeOut);//调用HTTP通信接口以提交数据到API

            WxPayData result = new WxPayData();
            //若接口调用失败会返回xml格式的结果
            if (response.Substring(0, 5) == "<xml>")
            {
                result.FromXml(response);
            }
            //接口调用成功则返回非xml格式的数据
            else
                result.SetValue("result", response);

            return result;
        }

        /// <summary>
        /// 转换短链接，该接口主要用于扫码原生支付模式一中的二维码链接转成短链接(weixin://wxpay/s/XXXXXX)，减小二维码数据量，提升扫描速度和精确度。
        /// </summary>
        /// <param name="inputObj">提交给转换短连接API的参数</param>
        /// <param name="timeOut">接口超时时间</param>
        /// <returns></returns>
        public static WxPayData ShortUrl(WxPayData inputObj, PayCenterSetting setting, int timeOut = 20)
        {
            string url = "https://api.mch.weixin.qq.com/tools/shorturl";
            //检测必填参数
            if (!inputObj.IsSet("long_url"))
            {
                throw new WxPayException("需要转换的URL，签名用原串，传输需URL encode！");
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


            //签名
            inputObj.SetValue("sign", inputObj.MakeSign(key));
            //log4net.LogHelper.WriteInfo(typeof(wxuserinfo),inputObj.ToJson());
            string xml = inputObj.ToXml();
            string response = WxHelper.Post(xml, url, false, setting, timeOut);
            //log4net.LogHelper.WriteInfo(typeof(WxPayApi), "UnifiedOrder().response=" + response);

            WxPayData result = new WxPayData();
            result.FromXml(response);
#if DEBUG
            log4net.LogHelper.WriteInfo(typeof(WxPayApi), "UnifiedOrder().response=" + response);
#endif
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
        /// 测速上报
        /// </summary>
        /// <param name="interface_url">接口URL</param>
        /// <param name="timeCost">接口耗时</param>
        /// <param name="inputObj">inputObj参数数组</param>
        private static void ReportCostTime(string interface_url, int timeCost, WxPayData inputObj, PayCenterSetting setting)
        {
            //如果不需要进行上报
            if (WxPayConfig.REPORT_LEVENL == 0)
            {
                return;
            }

            //如果仅失败上报
            if (WxPayConfig.REPORT_LEVENL == 1 && inputObj.IsSet("return_code") && inputObj.GetValue("return_code").ToString() == "SUCCESS" &&
             inputObj.IsSet("result_code") && inputObj.GetValue("result_code").ToString() == "SUCCESS")
            {
                return;
            }

            //上报逻辑
            WxPayData data = new WxPayData();
            data.SetValue("interface_url", interface_url);
            data.SetValue("execute_time_", timeCost);
            //返回状态码
            if (inputObj.IsSet("return_code"))
            {
                data.SetValue("return_code", inputObj.GetValue("return_code"));
            }
            //返回信息
            if (inputObj.IsSet("return_msg"))
            {
                data.SetValue("return_msg", inputObj.GetValue("return_msg"));
            }
            //业务结果
            if (inputObj.IsSet("result_code"))
            {
                data.SetValue("result_code", inputObj.GetValue("result_code"));
            }
            //错误代码
            if (inputObj.IsSet("err_code"))
            {
                data.SetValue("err_code", inputObj.GetValue("err_code"));
            }
            //错误代码描述
            if (inputObj.IsSet("err_code_des"))
            {
                data.SetValue("err_code_des", inputObj.GetValue("err_code_des"));
            }
            //商户订单号
            if (inputObj.IsSet("out_trade_no"))
            {
                data.SetValue("out_trade_no", inputObj.GetValue("out_trade_no"));
            }
            //设备号
            if (inputObj.IsSet("device_info"))
            {
                data.SetValue("device_info", inputObj.GetValue("device_info"));
            }

            try
            {
                Report(data, setting);
            }
            catch (WxPayException)
            {
                //不做任何处理
            }
        }

        /// <summary>
        /// 测速上报接口实现
        /// </summary>
        /// <param name="inputObj">提交给测速上报接口的参数</param>
        /// <param name="timeOut">测速上报接口超时时间</param>
        /// <returns></returns>
        public static WxPayData Report(WxPayData inputObj, PayCenterSetting setting, int timeOut = 20)
        {
            string url = "https://api.mch.weixin.qq.com/payitil/report";
            //检测必填参数
            if (!inputObj.IsSet("interface_url"))
            {
                throw new WxPayException("接口URL，缺少必填参数interface_url！");
            }
            if (!inputObj.IsSet("return_code"))
            {
                throw new WxPayException("返回状态码，缺少必填参数return_code！");
            }
            if (!inputObj.IsSet("result_code"))
            {
                throw new WxPayException("业务结果，缺少必填参数result_code！");
            }
            if (!inputObj.IsSet("user_ip"))
            {
                throw new WxPayException("访问接口IP，缺少必填参数user_ip！");
            }
            if (!inputObj.IsSet("execute_time_"))
            {
                throw new WxPayException("接口耗时，缺少必填参数execute_time_！");
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
            inputObj.SetValue("user_ip", WxPayConfig.IP);//终端ip
            inputObj.SetValue("time", DateTime.Now.ToString("yyyyMMddHHmmss"));//商户上报时间	 
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
        //public static WxPayData CompanyPay(WxPayData inputObj, PayCenterSetting setting, int timeOut = 20)
        //{
        //    string url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/promotion/transfers";
        //    string appid = WxPayConfig.APPID;
        //    string mch_id = WxPayConfig.MCHID;
        //    string key = string.Empty;
        //    if (setting != null && setting.Id > 0)
        //    {
        //        appid = setting.Appid;
        //        mch_id = setting.Mch_id;
        //        key = setting.Key;
        //    }
        //    inputObj.SetValue("mch_appid", appid);//公众账号ID
        //    inputObj.SetValue("mchid", mch_id);//商户号
        //    inputObj.SetValue("spbill_create_ip", WxPayConfig.IP);//IP
        //    inputObj.SetValue("nonce_str", GenerateNonceStr());//随机字符串	
        //    inputObj.SetValue("check_name", "NO_CHECK");//校验用户姓名,不需要校验,必选项，可选类型有:	
        //    inputObj.SetValue("sign", inputObj.MakeSign(key));//签名
        //                                                      //NO_CHECK：不校验真实姓名
        //                                                      //FORCE_CHECK：强校验真实姓名（未实名认证的用户会校验失败，无法转账） 
        //                                                      //OPTION_CHECK：针对已实名认证的用户才校验真实姓名（未实名认证用户不校验，可以转账成功）
        //                                                      //  log4net.LogHelper.WriteInfo(typeof(CompanyPay), "用户提现请求参数：" + inputObj.ToJson());
        //    DrawcashResult drawresult = new DrawcashResult();
        //    if (setting != null)
        //    {
        //        drawresult.mch_id = setting.Mch_id;
        //        drawresult.appid = setting.Appid;
        //    }
        //    //检测必填参数
        //    if (!inputObj.IsSet("mch_appid"))
        //    {
        //        throw new WxPayException("企业付款中，公众账号appID必填！");
        //    }
        //    if (!inputObj.IsSet("mchid"))
        //    {
        //        throw new WxPayException("企业付款中，微信支付分配的商户号mchid必填！");
        //    }
        //    if (!inputObj.IsSet("nonce_str"))
        //    {
        //        throw new WxPayException("企业付款中，随机字符串nonce_str必填！");
        //    }
        //    else
        //    {
        //        drawresult.nonce_str = inputObj.GetValue("nonce_str").ToString();
        //    }
        //    if (!inputObj.IsSet("sign"))
        //    {
        //        throw new WxPayException("企业付款中，签名sign必填！");
        //    }
        //    else
        //    {
        //        drawresult.sign = inputObj.GetValue("sign").ToString();
        //    }
        //    if (!inputObj.IsSet("partner_trade_no"))
        //    {
        //        throw new WxPayException("企业付款中，商户订单号partner_trade_no必填！");
        //    }
        //    else
        //    {
        //        drawresult.partner_trade_no = inputObj.GetValue("partner_trade_no").ToString();
        //    }
        //    if (!inputObj.IsSet("openid"))
        //    {
        //        throw new WxPayException("企业付款中，用户openid必填！");
        //    }
        //    else
        //    {
        //        drawresult.openid = inputObj.GetValue("openid").ToString();
        //    }
        //    string error = string.Empty;
        //    //if (new DrawBackUserBLL().CheckEnable(0, 0, drawresult.openid))
        //    //{
        //    //    throw new WxPayException("黑名单！");
        //    //}

        //    if (!inputObj.IsSet("check_name"))
        //    {
        //        throw new WxPayException("企业付款中，校验用户姓名check_name必填！");
        //    }
        //    else
        //    {
        //        drawresult.check_name = inputObj.GetValue("check_name").ToString();
        //    }
        //    if (!inputObj.IsSet("amount"))
        //    {
        //        throw new WxPayException("企业付款中，金额amount必填！");
        //    }
        //    else
        //    {
        //        drawresult.amount = Convert.ToInt32(inputObj.GetValue("amount").ToString());
        //    }
        //    if (!inputObj.IsSet("desc"))
        //    {
        //        throw new WxPayException("企业付款中，企业付款描述信息desc必填！");
        //    }
        //    else
        //    {
        //        drawresult.desc = inputObj.GetValue("desc").ToString();
        //        if (inputObj.GetValue("desc").ToString().Length >= 50)
        //        {
        //            inputObj.SetValue("desc", inputObj.GetValue("desc").ToString().Substring(0, 46) + "...");
        //        }
        //    }
        //    if (!inputObj.IsSet("spbill_create_ip"))
        //    {
        //        throw new WxPayException("企业付款中，Ip地址spbill_create_ip必填！");
        //    }
        //    else
        //    {
        //        drawresult.spbill_create_ip = inputObj.GetValue("spbill_create_ip").ToString();
        //    }
        //    if (inputObj.IsSet("re_user_name"))
        //    {
        //        drawresult.re_user_name = inputObj.GetValue("re_user_name").ToString();
        //    }
        //    string xml = inputObj.ToXml();
        //    drawresult.CreateTime = DateTime.Now;
        //    //post之前，增加记录
        //    DrawcashResultBLL bll = new DrawcashResultBLL();

        //    int id = Convert.ToInt32(bll.Add(drawresult));
        //    drawresult.id = id;
        //    //
        //    try
        //    {
        //        string response = WxHelper.Post(xml, url, true, setting, 30);
        //        if (string.IsNullOrEmpty(response))
        //        {
        //            return null;
        //        }
        //        WxPayData result = new WxPayData();
        //        result.FromXml(response, false);
        //        if (result != null)
        //        {
        //            drawresult.return_code = result.GetValue("return_code") == null ? "" : result.GetValue("return_code").ToString();
        //            drawresult.return_msg = result.GetValue("return_msg") == null ? "" : result.GetValue("return_msg").ToString();
        //            drawresult.nonce_str = result.GetValue("nonce_str") == null ? "" : result.GetValue("nonce_str").ToString();
        //            drawresult.result_code = result.GetValue("result_code") == null ? "" : result.GetValue("result_code").ToString();
        //            drawresult.err_code = result.GetValue("err_code") == null ? "" : result.GetValue("err_code").ToString();
        //            drawresult.err_code_des = result.GetValue("err_code_des") == null ? "" : result.GetValue("err_code_des").ToString();
        //            drawresult.payment_time = DateTime.Now;
        //            drawresult.payment_no = result.GetValue("payment_no") == null ? "" : result.GetValue("payment_no").ToString();
        //            bll.Update(drawresult);
        //        }
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        log4net.LogHelper.WriteError(typeof(WxPayApi), new Exception("提现出现错误，错误信息：" + ex.Message));
        //        return null;
        //    }
        //}
        /// <summary>
        /// 企业付款
        /// </summary>
        /// <param name="inputObj">企业付款接口的参数</param>
        /// <param name="timeOut">企业付款接口超时时间</param>
        /// <returns></returns>
        //public static WxPayData RedPack(WxPayData inputObj, PayCenterSetting setting, int timeOut = 20)
        //{
        //    string url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/sendredpack";
        //    string appid = WxPayConfig.APPID;
        //    string mch_id = WxPayConfig.MCHID;
        //    string key = string.Empty;
        //    if (setting != null && setting.Id > 0)
        //    {
        //        appid = setting.Appid;
        //        mch_id = setting.Mch_id;
        //        key = setting.Key;
        //    }
        //    inputObj.SetValue("wxappid", appid);//公众账号ID
        //    inputObj.SetValue("mch_id", mch_id);//商户号
        //    inputObj.SetValue("client_ip", WxPayConfig.IP);//IP
        //    inputObj.SetValue("nonce_str", GenerateNonceStr());//随机字符串	
        //    inputObj.SetValue("sign", inputObj.MakeSign(key));//签名
        //    //NO_CHECK：不校验真实姓名
        //    //FORCE_CHECK：强校验真实姓名（未实名认证的用户会校验失败，无法转账） 
        //    //OPTION_CHECK：针对已实名认证的用户才校验真实姓名（未实名认证用户不校验，可以转账成功）
        //    log4net.LogHelper.WriteInfo(typeof(CompanyPay), "用户提现请求参数：" + inputObj.ToJson());
        //    DrawcashResult drawresult = new DrawcashResult();

        //    //检测必填参数
        //    if (!inputObj.IsSet("nonce_str"))
        //    {
        //        throw new WxPayException("企业付款中，随机字符串nonce_str必填！");
        //    }
        //    else
        //    {
        //        drawresult.nonce_str = inputObj.GetValue("nonce_str").ToString();
        //    }
        //    if (!inputObj.IsSet("sign"))
        //    {
        //        throw new WxPayException("企业付款中，签名sign必填！");
        //    }
        //    else
        //    {
        //        drawresult.sign = inputObj.GetValue("sign").ToString();
        //    }
        //    if (!inputObj.IsSet("mch_billno"))
        //    {
        //        throw new WxPayException("企业付款中，商户订单号mch_billno必填！");
        //    }
        //    else
        //    {
        //        drawresult.partner_trade_no = inputObj.GetValue("mch_billno").ToString();
        //    }
        //    if (!inputObj.IsSet("re_openid"))
        //    {
        //        throw new WxPayException("企业付款中，用户re_openid必填！");
        //    }
        //    else
        //    {
        //        drawresult.openid = inputObj.GetValue("re_openid").ToString();
        //    }
        //    if (!inputObj.IsSet("total_amount"))
        //    {
        //        throw new WxPayException("企业付款中，金额total_amount必填！");
        //    }
        //    else
        //    {
        //        drawresult.amount = Convert.ToInt32(inputObj.GetValue("total_amount").ToString());
        //        if (drawresult.amount < 100 || drawresult.amount > 20000)
        //        {
        //            throw new WxPayException("企业付款中，金额total_amount范围：1-200元！");
        //        }
        //    }
        //    if (!inputObj.IsSet("client_ip"))
        //    {
        //        throw new WxPayException("企业付款中，Ip地址spbill_create_ip必填！");
        //    }
        //    else
        //    {
        //        drawresult.spbill_create_ip = inputObj.GetValue("client_ip").ToString();
        //    }
        //    if (!inputObj.IsSet("remark"))
        //    {
        //        throw new WxPayException("企业付款中，企业付款描述信息remark必填！");
        //    }
        //    else
        //    {
        //        drawresult.desc = inputObj.GetValue("remark").ToString();
        //    }
        //    if (!inputObj.IsSet("mch_id"))
        //    {
        //        throw new WxPayException("企业付款中，微信支付分配的商户号mchid必填！");
        //    }
        //    if (!inputObj.IsSet("send_name"))
        //    {
        //        throw new WxPayException("企业付款中，签名send_name必填！");
        //    }
        //    if (!inputObj.IsSet("wishing"))
        //    {
        //        throw new WxPayException("企业付款中，企业付款描述信息wishing必填！");
        //    }
        //    if (!inputObj.IsSet("act_name"))
        //    {
        //        throw new WxPayException("企业付款中，企业付款描述信息act_name必填！");
        //    }

        //    string xml = inputObj.ToXml();
        //    drawresult.CreateTime = DateTime.Now;
        //    //post之前，增加记录
        //    new DrawcashResultBLL().Add(drawresult);
        //    //
        //    string response = WxHelper.Post(xml, url, true, setting, 30);
        //    if (string.IsNullOrEmpty(response))
        //    {
        //        return null;
        //    }
        //    WxPayData result = new WxPayData();
        //    result.FromXml(response, false);
        //    return result;
        //}
    }
}