//using BLL.MiniApp;  
using DAL.Base;
using Entity.MiniApp;  
using Newtonsoft.Json;
using System;
using System.Web;
using Utility;

namespace Core.MiniApp
{
    /// <summary>
    /// 支付结果通知回调处理类
    /// 负责接收微信支付后台发送的支付结果并对订单有效性进行验证，将验证结果反馈给微信支付后台
    /// </summary>
    public class ResultNotify : Notify
    {
        public ResultNotify(HttpContextBase context) : base(context)
        {
        }
        public ResultNotify()
        {
        }
        public override void ProcessNotify()
        {
            try
            {

                WxPayData notifyData = GetNotifyData();

                //检查支付结果中transaction_id是否存在
                if (!notifyData.IsSet("transaction_id") || !notifyData.IsSet("appid"))
                {
                    //若transaction_id不存在，则立即返回结果给微信支付后台
                    WxPayData res = new WxPayData();
                    res.SetValue("return_code", "FAIL");
                    res.SetValue("return_msg", "支付结果中微信订单号不存在");
                    log4net.LogHelper.WriteError(GetType(), new Exception("transaction_id不存在 : " + res.ToXml()));
                    context.Response.Write(res.ToXml());
                    context.Response.End();
                    return;
                }
                string transactionId = notifyData.GetValue("transaction_id").ToString();
                string appid = notifyData.GetValue("appid").ToString();
                PayCenterSetting setting = new PayCenterSettingBLL().GetPayCenterSetting(appid);
                //增加重复回调判断
                int re = RedisUtil.Get<int>(string.Format(MemCacheKey.ProcessNotify, transactionId));
                if (re != 0)
                {
                    return;
                }
                RedisUtil.Set(string.Format(MemCacheKey.ProcessNotify, transactionId), 1, TimeSpan.FromMinutes(30));
                //查询订单，判断订单真实性
                if (!QueryOrder(transactionId, setting))
                {
                    //若订单查询失败，则立即返回结果给微信支付后台
                    WxPayData res = new WxPayData();
                    res.SetValue("return_code", "FAIL");
                    res.SetValue("return_msg", "订单查询失败");
                    log4net.LogHelper.WriteError(GetType(), new Exception("订单查询失败: " + notifyData.ToJson()));
                    context.Response.Write(res.ToXml());
                    context.Response.End();
                }
                //查询订单成功
                else
                {
                    //说付款成功：插入记录
                    //这里要注意，微信通知过来之后，15秒之内没有给微信回复处理状态，微信还会第二次，第三次通知。
                    //带过来的信息一模一样，所以这要做标志判断，万一处理过程出现问题没有给微信回复。
                    //在以后多次请求的时候避免多次进行业务处理,插入多条记录
                    PayResult result = notifyData.ToPayResult();

                    //log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(result));
                    if (result == null) return;
                    if ((WxUtils.getAttachValue(result.attach, "from") == "shop"))//商城支付
                    {

                    }
                    else
                    {
                        int id = Convert.ToInt32(new PayResultBLL().Add(result));//插入记录，论坛，直播、有约
                        result.Id = id;
                        NotifyOper(result);
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(ResultNotify), ex);
            }
            finally
            {
                //最后要给微信放回接收成功数据，不然微信会连续多次发送同样请求
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "SUCCESS");
                res.SetValue("return_msg", "OK");
                context.Response.Write(res.ToXml());
                context.Response.End();
            }
        }
        //回调根据PayResult处理回调
        public bool NotifyOper(PayResult result)
        {
            //log4net.LogHelper.WriteInfo(this.GetType(), "回调到这里");
            //log4net.LogHelper.WriteInfo(this.GetType(), JsonConvert.SerializeObject(result));

            if (WxUtils.getAttachValue(result.attach, "from") == "city")
            {
                CityMordersBLL citybll = new CityMordersBLL(result);
                string orderidstr = WxUtils.getAttachValue(result.attach, "orderid");
                if (string.IsNullOrEmpty(orderidstr))
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception(JsonConvert.SerializeObject(result)));
                    return false;
                }
                int orderid = Convert.ToInt32(orderidstr);
                CityMorders order = citybll.GetModel(orderid);//citybll.GetModel(" Id=" + orderid + "");
                //修改订单支付状态
                if (order == null)
                {
                    Exception ex = new Exception("警报:根据支付单号找不到相关订单！" + " out_trade_no='" + result.out_trade_no + "'");
                    log4net.LogHelper.WriteError(GetType(), ex);
                    log4net.LogHelper.WriteError(GetType(), new Exception(JsonConvert.SerializeObject(result)));
                    return false;
                }
                if (order.payment_status != 0)
                {
                    return false;
                }
                if (order.Percent < 0 || order.Percent > 100)
                {
                    Exception ex = new Exception("警报:出现异常订单！订单提成百分比为：" + order.Percent + " out_trade_no='" + result.out_trade_no + "'");
                    log4net.LogHelper.WriteError(GetType(), ex);
                    log4net.LogHelper.WriteError(GetType(), new Exception(JsonConvert.SerializeObject(result)));
                    return false;
                }
                citybll.Order = order;
                //修改order支付商户
                order.mch_id = result.mch_id;
                order.appid = result.appid;
                citybll.Update(order);
                switch (result.paytype)
                {
                    //小程序商城
                    case (int)ArticleTypeEnum.MiniappGoods:
                        return citybll.MiniappStoreGoods();
                    //小程序餐饮
                    case (int)ArticleTypeEnum.MiniappFoodGoods:
                        return citybll.MiniappFoodGoods();
                    //小程序储值
                    case (int)ArticleTypeEnum.MiniappSaveMoneySet:
                        return citybll.MiniappSaveMoney();
                    //小程序砍价
                    case (int)ArticleTypeEnum.MiniappBargain:
                        return citybll.MiniappBargainMoney();
                    //小程序拼团
                    case (int)ArticleTypeEnum.MiniappGroups:
                        return citybll.MiniappStoreGroup();
                    //小程序行业版
                    case (int)ArticleTypeEnum.MiniappEnt:
                        return citybll.MiniappEntGoods();
                    //小程序足浴版
                    case (int)ArticleTypeEnum.MiniappFootbath:
                        return citybll.MiniappFootbath();
                    case (int)ArticleTypeEnum.MiniappMultiStore:
                        return citybll.MiniappMultiStore();
                        //小程序专业版积分兑换(微信+积分方式兑换)
                    case (int)ArticleTypeEnum.MiniappExchangeActivity:
                        return citybll.PayMiniappExchangeActivity();
                        //小程序同城模拟板
                    case (int)ArticleTypeEnum.City_StoreBuyMsg:
                        return citybll.cityBuyMsg();
                }
            }
            return false;
        }
        //查询订单
        private bool QueryOrder(string transactionId, PayCenterSetting setting)
        {
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transactionId);
            WxPayData res = WxPayApi.OrderQuery(req, setting);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            return false;
        }
        private string getAttachValue(string attach, string value)
        {
            string regex = "(?:^|\\?|&)" + value.ToLower() + "=(?<value>[\\s\\S]+?)(?:&|$)";
            return  CRegex.GetText(attach.ToLower(), regex, "value");
        }
        /// <summary>
        /// 检测论坛数据有没有异常
        /// </summary>
        /// <param name="minisnsid"></param>
        /// <returns></returns>
        private bool CheckMinisns(int minisnsid)
        {
            Minisns minisns = new MinisnsBll().GetModelByCache(minisnsid);
            if (minisns == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("论坛为空，论坛ID：" + minisnsid));
                return false;
            }
            if (minisns.RewardPercent < 0 || minisns.RewardPercent > 100)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("论坛提成比率为：" + minisns.RewardPercent + ".论坛题ID：" + minisns.Id));
                return false;
            }
            return true;
        }
        /// <summary>
        /// 检测话题数据有没有异常
        /// </summary>
        /// <param name="topicid"></param>
        /// <returns></returns>
        //private bool CheckTopic(int topicid)
        //{
        //    zbTopics topic = new zbTopicsBLL().GetModelByCache(topicid);
        //    if (topic == null)
        //    {
        //        log4net.LogHelper.WriteError(GetType(), new Exception("话题为空，话题ID：" + topicid));
        //        return false;
        //    }
        //    if (topic.percentage < 0 || topic.percentage > 100)
        //    {
        //        log4net.LogHelper.WriteError(GetType(), new Exception("直播间提成比率为：" + topic.percentage + ".话题ID：" + topic.Id));
        //        return false;
        //    }
        //    zbSiteInfo siteM = new zbSiteInfoBLL().GetModel(topic.zbId);
        //    if (siteM.SharePrecent < 0 || siteM.SharePrecent > 70)
        //    {
        //        log4net.LogHelper.WriteError(GetType(), new Exception("直播间分享提成：" + siteM.SharePrecent + ".话题ID：" + topic.Id));
        //        return false;
        //    }
        //    return true;
        //}
    }
}