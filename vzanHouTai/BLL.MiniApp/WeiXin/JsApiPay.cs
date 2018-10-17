using System;
using System.Web;
using System.Web.Security;
using Entity.MiniApp;
using Core.MiniApp;
using Newtonsoft.Json;
//using BLL.MiniApp; 

namespace BLL.MiniApp
{
    public class JsApiPay
    {
        /// <summary>
        /// 保存页面对象，因为要在类的方法中使用Page的Request对象
        /// </summary>
        private HttpContextBase context { get; set; }

        /// <summary>
        /// openid用于调用统一下单接口
        /// </summary>
        public string openid { get; set; }

        /// <summary>
        /// access_token用于获取收货地址js函数入口参数
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// 商品金额，用于统一下单
        /// </summary>
        public int total_fee { get; set; }

        /// <summary>
        /// 统一下单接口返回结果
        /// </summary>
        public WxPayData unifiedOrderResult { get; set; }

        public JsApiPay(HttpContextBase context)
        {
            this.context = context;
        }

        /// <summary>
        /// 抛弃使用（用GetUnifiedOrderResult）
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="morder"></param>
        /// <param name="notify_url"></param>
        /// <returns></returns>
        public WxPayData GetUnifiedOrderResultByCity(PayCenterSetting setting, CityMorders morder,string notify_url)
        {
            //统一下单
            string out_trade_no = morder.orderno;//商户订单号
            if (string.IsNullOrEmpty(morder.orderno))
            {
                throw new WxPayException("UnifiedOrder response error!");
            }
            string body = string.Empty;
            if (!string.IsNullOrEmpty(morder.ShowNote))
            {
                body = morder.ShowNote;
            }
            else
            {
                string paytype = string.Empty;
                switch (morder.OrderType)
                {
                    case (int)ArticleTypeEnum.MiniappGoods:
                        paytype = "小程序电商模板订单";
                        break;
                    case (int)ArticleTypeEnum.MiniappFoodGoods:
                        paytype = "小程序餐饮模板订单";
                        break;
                    case (int)ArticleTypeEnum.MiniappSaveMoneySet:
                        paytype = "小程序餐饮储值";
                        break;
                    case (int)ArticleTypeEnum.MiniappBargain:
                        paytype = "小程序砍价";
                        break;
                    case (int)ArticleTypeEnum.MiniappEnt:
                        paytype = "小程序专业版";
                        break;
                    case (int)ArticleTypeEnum.MiniappWXDirectPay:
                        paytype = "小程序直接微信转账";
                        break;
                    case (int)ArticleTypeEnum.PlatChildOrderPay:
                        paytype = "平台子模版支付";
                        break;
                    case (int)ArticleTypeEnum.PlatChildOrderInPlatPay:
                        paytype = "子模版订单平台支付";
                        break;
                }
                body = string.Format("支付中心，{0}支付{1}元", paytype, (morder.payment_free * 0.01));//商品描述;
            }
            string attach = string.Format("paytype={0}&orderid={1}&orderno={2}&from=city", morder.OrderType, morder.Id, morder.orderno);//自带的信息
            //统一下单
            WxPayData data = new WxPayData();
            data.SetValue("body", body);
            data.SetValue("attach", attach);
            data.SetValue("out_trade_no", out_trade_no);
            data.SetValue("total_fee", total_fee);
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
            data.SetValue("goods_tag", "test");
            data.SetValue("trade_type", "JSAPI");
            data.SetValue("openid", openid);
            data.SetValue("notify_url", notify_url);
            WxPayData result = WxPayApi.UnifiedOrder(data, setting);
            
            if (result!=null&&(!result.IsSet("appid") || !result.IsSet("prepay_id") || !result.IsSet("prepay_id")))
            {
                throw new WxPayException(result.ToJson());
            }
            unifiedOrderResult = result;
            return result;
        }

        public WxPayData GetUnifiedOrderResult(PayCenterSetting setting, CityMorders morder, string notify_url,ref string erromsg)
        {
            //统一下单
            string out_trade_no = morder.orderno;//商户订单号
            if (string.IsNullOrEmpty(morder.orderno))
            {
                erromsg = "订单号不能为空";
                return new WxPayData();
            }
            string body = string.Empty;
            if (!string.IsNullOrEmpty(morder.ShowNote))
            {
                body = morder.ShowNote;
            }
            else
            {
                string paytype = string.Empty;
                switch (morder.OrderType)
                {
                    case (int)ArticleTypeEnum.MiniappGoods:
                        paytype = "小程序电商模板订单";
                        break;
                    case (int)ArticleTypeEnum.MiniappFoodGoods:
                        paytype = "小程序餐饮模板订单";
                        break;
                    case (int)ArticleTypeEnum.MiniappSaveMoneySet:
                        paytype = "小程序餐饮储值";
                        break;
                    case (int)ArticleTypeEnum.MiniappBargain:
                        paytype = "小程序砍价";
                        break;
                    case (int)ArticleTypeEnum.MiniappEnt:
                        paytype = "小程序专业版";
                        break;
                    case (int)ArticleTypeEnum.MiniappWXDirectPay:
                        paytype = "小程序直接微信转账";
                        break;
                    case (int)ArticleTypeEnum.PlatChildOrderPay:
                        paytype = "平台子模版支付";
                        break;
                    case (int)ArticleTypeEnum.PlatChildOrderInPlatPay:
                        paytype = "子模版订单平台支付";
                        break;
                }
                body = string.Format("支付中心，{0}支付{1}元", paytype, (morder.payment_free * 0.01));//商品描述;
            }
            string attach = string.Format("paytype={0}&orderid={1}&orderno={2}&from=city", morder.OrderType, morder.Id, morder.orderno);//自带的信息
            //统一下单
            WxPayData data = new WxPayData();
            data.SetValue("body", body);
            data.SetValue("attach", attach);
            data.SetValue("out_trade_no", out_trade_no);
            data.SetValue("total_fee", total_fee);
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
            data.SetValue("goods_tag", "test");
            data.SetValue("trade_type", "JSAPI");
            data.SetValue("openid", openid);
            data.SetValue("notify_url", notify_url);
            WxPayData result = WxPayApi.UnifiedOrder(data, setting);

            if (result != null && (!result.IsSet("appid") || !result.IsSet("prepay_id") || !result.IsSet("prepay_id")))
            {
                erromsg = result.ToJson();
            }
            unifiedOrderResult = result;

            if (!string.IsNullOrEmpty(erromsg))
            {
                PayReturnMsg payreturnmsg = JsonConvert.DeserializeObject<PayReturnMsg>(erromsg);
                if (payreturnmsg.return_msg.Contains("签名错误"))
                {
                    erromsg = "商户号秘钥错误";
                }
                else if (payreturnmsg.return_msg.Contains("商户号mch_id或sub_mch_id不存在"))
                {
                    erromsg = "商户号不正确";
                }
                else
                {
                    erromsg = payreturnmsg.return_msg;
                }
            }

            return result;
        }

        /// <summary>
        /// 返回是否关闭成功
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="morder"></param>
        /// <param name="notify_url"></param>
        /// <returns></returns>
        public bool CloseCityMorder(int citymorederId,ref string errorMsg)
        {
            CityMorders morder = new CityMordersBLL().GetModel(citymorederId);
            if (morder == null)
            {
                errorMsg = "未找到citymorder订单";
                return false;
            }
            PayCenterSetting setting = PayCenterSettingBLL.SingleModel.GetPayCenterSetting(morder.appid);
            if (setting == null)
            {
                errorMsg = "未找到用户的商户配置";
                return false;
            }


            string out_trade_no = morder.orderno;//商户订单号
            if (string.IsNullOrEmpty(morder.orderno))
            {
                return false;
            }
            WxPayData data = new WxPayData();
            data.SetValue("out_trade_no", out_trade_no);
            WxPayData result = WxPayApi.CloseOrder(data, setting);
            
            if (result==null || !result.GetValue("return_code").ToString().Equals("SUCCESS"))
            {
                errorMsg = "请求关闭订单失败,原因为：";
                switch (data.GetValue("return_code").ToString())
                {
                    case "ORDERPAID":
                        errorMsg += "订单已支付，不能发起关单";
                        break;
                    case "SYSTEMERROR":
                        errorMsg += "系统异常，请重新调用该API";
                        break;
                    case "ORDERCLOSED":
                        errorMsg += "订单已关闭，无法重复关闭";
                        break;
                    case "SIGNERROR":
                        errorMsg += "签名错误";
                        break;
                    case "REQUIRE_POST_METHOD":
                        errorMsg += "未使用post传递参数";
                        break;
                    case "XML_FORMAT_ERROR":
                        errorMsg += "XML格式错误";
                        break;
                }
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 修改下单金额,返回新的citymorders id
        /// </summary>
        public Int32 updateWxOrderMoney(int citymorderId,int price, ref string errorMsg)
        {
            int newOrderId = 0;
            CityMordersBLL citymordersBLL = new CityMordersBLL();

            CityMorders buyMorder = citymordersBLL.GetModel(citymorderId);
            if (buyMorder == null)
            {
                errorMsg = "该订单的支付资料丢失,无法修改金额";
                return newOrderId;
            }

            //关闭原微信订单
            bool isColseOrderSuccess = CloseCityMorder(buyMorder.Id, ref errorMsg);
            if (!isColseOrderSuccess)
            {
                return newOrderId;
            }
            buyMorder.Status = -1;
            citymordersBLL.Update(buyMorder, "Status");

            //开新单
            buyMorder.trade_no = buyMorder.orderno = WxPayApi.GenerateOutTradeNo(); //生成新的订单号

            buyMorder.payment_free = price;
            buyMorder.Status = 0;
            newOrderId = buyMorder.Id = Convert.ToInt32(citymordersBLL.Add(buyMorder));
            if (buyMorder.Id <= 0)
            {
                errorMsg = "生成新的微信订单失败";
                return newOrderId;
            }
            return newOrderId;
        }
        
        public string GetJsApiParametersnew(PayCenterSetting setting)
        {
            WxPayData jsApiParam = new WxPayData();
            jsApiParam.SetValue("appId", unifiedOrderResult.GetValue("appid"));
            jsApiParam.SetValue("timeStamp", WxPayApi.GenerateTimeStamp());
            jsApiParam.SetValue("nonceStr", unifiedOrderResult.GetValue("nonce_str"));
            //jsApiParam.SetValue("nonceStr2", WxPayApi.GenerateNonceStr());
            jsApiParam.SetValue("package", "prepay_id=" + unifiedOrderResult.GetValue("prepay_id"));
            jsApiParam.SetValue("signType", "MD5");
            //第三方支付需要查询KEY的值
            string key = string.Empty;
            string appid = unifiedOrderResult.GetValue("appid").ToString();
            //PayCenterSetting setting = new PayCenterSettingBLL().GetPayCenterSetting(appid);
            if (setting != null && setting.Id > 0)
            {
                key = setting.Key;
            }
            jsApiParam.SetValue("paySign", jsApiParam.MakeSign(key));
            //jsApiParam.SetValue("openid", openid);
            return jsApiParam.ToJson();
        }
    }

    public class PayReturnMsg
    {
        public string return_code { get; set; }
        public string return_msg { get; set; }
    }
}