using System;
using System.Web;
using System.Web.Security;
using Entity.MiniApp;
//using BLL.MiniApp; 
 
namespace Core.MiniApp
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


        /**
        * 
        * 网页授权获取用户基本信息的全部过程
        * 详情请参看网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
        * 第一步：利用url跳转获取code
        * 第二步：利用code去获取openid和access_token
        * 
        */
        //public void GetOpenidAndAccessToken()
        //{
        //    if (!string.IsNullOrEmpty(context.Request.QueryString["code"]))
        //    {
        //        //获取code码，以获取openid和access_token
        //        string code = context.Request.QueryString["code"];
        //        log4net.LogHelper.WriteInfo(this.GetType(), "Get code : " + code);
        //        GetOpenidAndAccessTokenFromCode(code);
        //    }
        //    else
        //    {
        //        //构造网页授权获取code的URL
        //        string host = context.Request.Url.Host;
        //        string path = context.Request.Path;
        //        string redirect_uri = HttpUtility.UrlEncode("http://" + host + path);
        //        WxPayData data = new WxPayData();
        //        data.SetValue("appid", WxPayConfig.APPID);
        //        data.SetValue("redirect_uri", redirect_uri);
        //        data.SetValue("response_type", "code");
        //        data.SetValue("scope", "snsapi_base");
        //        data.SetValue("state", "STATE" + "#wechat_redirect");
        //        string url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();
        //        log4net.LogHelper.WriteInfo(this.GetType(), "Will Redirect to URL : " + url);
        //        try
        //        {
        //            //触发微信返回code码         
        //            context.Response.Redirect(url);//Redirect函数会抛出ThreadAbortException异常，不用处理这个异常
        //        }
        //        catch (System.Threading.ThreadAbortException )
        //        {
        //        }
        //    }
        //}


        /**
	    * 
	    * 通过code换取网页授权access_token和openid的返回数据，正确时返回的JSON数据包如下：
	    * {
	    *  "access_token":"ACCESS_TOKEN",
	    *  "expires_in":7200,
	    *  "refresh_token":"REFRESH_TOKEN",
	    *  "openid":"OPENID",
	    *  "scope":"SCOPE",
	    *  "unionid": "o6_bmasdasdsad6_2sgVt7hMZOPfL"
	    * }
	    * 其中access_token可用于获取共享收货地址
	    * openid是微信支付jsapi支付接口统一下单时必须的参数
        * 更详细的说明请参考网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
        * @失败时抛异常WxPayException
	    */
        //public void GetOpenidAndAccessTokenFromCode(string code)
        //{
        //    try
        //    {
        //        //构造获取openid及access_token的url
        //        WxPayData data = new WxPayData();
        //        data.SetValue("appid", WxPayConfig.APPID);
        //        data.SetValue("secret", WxPayConfig.APPSECRET);
        //        data.SetValue("code", code);
        //        data.SetValue("grant_type", "authorization_code");
        //        string url = "https://api.weixin.qq.com/sns/oauth2/access_token?" + data.ToUrl();

        //        //请求url以获取数据
        //        string result = WxHelper.HttpGet(url);

        //        //保存access_token，用于收货地址获取
        //        WxAuthorize jd = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<WxAuthorize>(result);
        //        access_token = jd.access_token;

        //        //获取用户openid
        //        openid = jd.openid;
        //    }
        //    catch (Exception ex)
        //    {
        //        log4net.LogHelper.WriteError(this.GetType(), ex);
        //        throw ex;
        //    }
        //}

        ///// <summary>
        ///// 文章打赏
        ///// </summary>
        ///// <param name="article">文章实体</param>
        ///// <returns></returns>
        //public WxPayData GetUnifiedOrderResult(PayCenterSetting setting,Article article, int luserid, out int orderid, int artcommentid = 0)
        //{
        //    OAuthUserBll bll = new OAuthUserBll(article.MinisnsId.ToString());
        //    OAuthUser looker = bll.GetUserByCache(luserid);
        //    OAuthUser maker = bll.GetUserByCache(article.UserId);

        //    //统一下单
        //    string out_trade_no = WxPayApi.GenerateOutTradeNo();//商户订单号
        //    string body = "赞赏" + (maker == null ? "匿名用户" : Utility.ReplaceSpecialChar(maker.Nickname, '?'));//商品描述
        //    if (article.IsGuerdon == (int)ArticleTypeEnum.Donation)
        //    {
        //        body = "支持" + (maker == null ? "匿名用户" : Utility.ReplaceSpecialChar(maker.Nickname, '?'));//商品描述
        //    }
        //    string attach = "paytype=1&minisnsid=" + article.MinisnsId + "&articleid=" + article.Id + "&userid=" + article.UserId + "&luserId=" + looker.Id;//自带的信息
        //    if (article.IsGuerdon == (int)ArticleTypeEnum.Donation && artcommentid != 0)
        //    {
        //        attach += "&articlecommentid=" + artcommentid;
        //    }
        //    //统一下单
        //    WxPayData data = new WxPayData();
        //    data.SetValue("body", body);
        //    data.SetValue("attach", attach);
        //    data.SetValue("out_trade_no", out_trade_no);
        //    data.SetValue("total_fee", total_fee);
        //    data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
        //    data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
        //    data.SetValue("goods_tag", "test");
        //    data.SetValue("trade_type", "JSAPI");
        //    data.SetValue("openid", openid);

        //    WxPayData result = WxPayApi.UnifiedOrder(data, setting);

        //    if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
        //    {
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("data:" + data.ToJson()));
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("result:" + result.ToJson()));
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("统一下单请求失败!prepay_id:" + result.GetValue("prepay_id").ToString()));
        //        throw new WxPayException("UnifiedOrder response error!");
        //    }
        //    //插入打赏记录
        //    if (maker == null)
        //    {
        //        //作者为空，不能打赏
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("文章作者为空!"));
        //        throw new WxPayException("文章作者为空!");
        //    }
        //    if (looker == null)
        //    {
        //        //打赏者为空，新增用户信息（在前面获取用户信息已经跳转注册过）
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("获取用户信息失败!"));
        //        throw new WxPayException("获取用户信息失败!");
        //    }
        //    RewardOrder order = new RewardOrder();
        //    order.userip = Utility.Web.WebHelper.GetIP();
        //    order.addTime = DateTime.Now;
        //    order.articleId = article.Id;
        //    order.minisnsId = article.MinisnsId;
        //    order.rewardFromUserId = looker.Id;
        //    order.rewardMoney = total_fee;
        //    order.rewardToUserId = maker.Id;
        //    order.out_trade_no = out_trade_no;
        //    order.status = 0;
        //    order.percent = ((100 - WebConfigBLL.VzanRewardPercent) - new MinisnsBll().GetModel(article.MinisnsId).RewardPercent);
        //    //众筹
        //    if (article.IsGuerdon == (int)ArticleTypeEnum.Donation)
        //    {
        //        order.percent = 100 - WebConfigBLL.DonationPercent;
        //        order.rewardtype = 3;
        //    }
        //    //新增区分文章和回复类型
        //    order.rewardtype = 0;
        //    RewardOrderBLL bllorder = new RewardOrderBLL();
        //    try
        //    {
        //        orderid = Convert.ToInt32(bllorder.Add(order));
        //        if (orderid <= 0)
        //        {
        //            throw new WxPayException("打赏新增订单失败!");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("插入RewardOrder失败!" + ex.Message));
        //        throw;
        //    }
        //    unifiedOrderResult = result;
        //    return result;
        //}

        ///// <summary>
        ///// 文章悬赏
        ///// </summary>
        ///// <param name="article">文章实体</param>
        ///// <returns></returns>
        //public WxPayData GetUnifiedOrderResult(PayCenterSetting setting,Article article)
        //{
        //    OAuthUserBll bll = new OAuthUserBll(article.MinisnsId.ToString());
        //    OAuthUser maker = bll.GetUserByCache(article.UserId);
        //    //ArticleType articleType = new ArticleTypeBll().GetModel(article.ArticleTypeId);
        //    //统一下单
        //    string out_trade_no = WxPayApi.GenerateOutTradeNo();//商户订单号
        //    string body = string.Empty;
        //    int paytype = 0;
        //    if (article.GuerdonMoney > 0)
        //    {
        //        body += string.Format("悬赏贴：{0}元", article.GuerdonMoney * 0.01);//商品描述
        //        paytype = 2;
        //    }
        //    if (article.payment_free > 0)
        //    {
        //        if (article.IsGuerdon == (int)ArticleTypeEnum.Pay)
        //        {
        //            body += string.Format("付费贴：{0}元", article.payment_free * 0.01);//商品描述
        //            paytype = 3;
        //        }
        //        if (article.IsGuerdon == (int)ArticleTypeEnum.Stick)
        //        {
        //            body += string.Format("置顶贴：{0}元", article.payment_free * 0.01);//商品描述
        //            paytype = 4;
        //        }
        //    }

        //    string attach = string.Empty;
        //    attach = "paytype=" + paytype + "&minisnsid=" + article.MinisnsId + "&articleid=" + article.Id + "&userid=" + article.UserId;//自带的信息

        //    //统一下单
        //    WxPayData data = new WxPayData();
        //    data.SetValue("body", body);
        //    data.SetValue("attach", attach);
        //    data.SetValue("out_trade_no", out_trade_no);
        //    data.SetValue("total_fee", total_fee);
        //    data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
        //    data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
        //    data.SetValue("goods_tag", "test");
        //    data.SetValue("trade_type", "JSAPI");
        //    data.SetValue("openid", openid);

        //    WxPayData result = WxPayApi.UnifiedOrder(data, setting);

        //    if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
        //    {
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("文章付款ID：" + article.Id + "，data:" + data.ToJson()));
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("result:" + result.ToJson()));
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("统一下单请求失败!prepay_id:" + result.GetValue("prepay_id").ToString()));
        //        throw new WxPayException("UnifiedOrder response error!");
        //    }
        //    //插入打赏记录
        //    if (maker == null)
        //    {
        //        //作者为空，不能打赏
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("文章作者为空!"));
        //        throw new WxPayException("文章作者为空!");
        //    }
        //    if (article.IsGuerdon == 1)
        //    {
        //        GuerdonOrder order = new GuerdonOrder();
        //        order.Addtime = DateTime.Now;
        //        order.Articleid = article.Id;
        //        order.MinisnsId = article.MinisnsId;
        //        order.GuerdonFromUserId = maker.Id;
        //        order.GuerdonMoney = article.GuerdonMoney;
        //        order.OutTradeNo = out_trade_no;
        //        order.Status = 0;
        //        order.Percent = ((100 - WebConfigBLL.VzanRewardPercent) - new MinisnsBll().GetModel(article.MinisnsId).RewardPercent);
        //        //新增区分文章和回复类型
        //        order.OperStatus = 0;
        //        GuerdonOrderBLL bllorder = new GuerdonOrderBLL();
        //        bllorder.Add(order);
        //    }
        //    unifiedOrderResult = result;
        //    return result;
        //}
        ///// <summary>
        ///// 回复打赏
        ///// </summary>
        ///// <param name="article">回复实体</param>
        ///// <returns></returns>
        //public WxPayData GetUnifiedOrderResult(PayCenterSetting setting, ArticleComment comment, OAuthUser looker, out int orderid)
        //{
            
        //    OAuthUserBll bll = new OAuthUserBll(comment.MinisnsId.ToString());
        //    OAuthUser maker = bll.GetUserByCache(comment.UserId);
        //    //统一下单
        //    string out_trade_no = WxPayApi.GenerateOutTradeNo();//商户订单号
        //    string body = "打赏" + (maker == null ? "匿名用户" : Utility.ReplaceSpecialChar(maker.Nickname, '?'));//商品描述
        //    string attach = "paytype=1&minisnsid=" + comment.MinisnsId + "&commentid=" + comment.Id + "&userid=" + comment.UserId + "&luserId=" + looker.Id;//自带的信息

        //    //统一下单
        //    WxPayData data = new WxPayData();
        //    data.SetValue("body", body);
        //    data.SetValue("attach", attach);
        //    data.SetValue("out_trade_no", out_trade_no);
        //    data.SetValue("total_fee", total_fee);
        //    data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
        //    data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
        //    data.SetValue("goods_tag", "test");
        //    data.SetValue("trade_type", "JSAPI");

        //    data.SetValue("openid", openid);
        //    WxPayData result = WxPayApi.UnifiedOrder(data, setting);
        //    if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
        //    {
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("文章回复实体ID" + comment.Id + ",data:" + data.ToJson()));
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("result:" + result.ToJson()));
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("统一下单请求失败!prepay_id:" + result.GetValue("prepay_id").ToString()));
        //        throw new WxPayException("UnifiedOrder response error!");
        //    }
        //    //插入打赏记录
        //    if (maker == null)
        //    {
        //        //作者为空，不能打赏
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("文章作者为空!"));
        //        throw new WxPayException("文章作者为空!");
        //    }
        //    if (looker == null)
        //    {
        //        //打赏者为空，新增用户信息（在前面获取用户信息已经跳转注册过）
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("获取用户信息失败!"));
        //        throw new WxPayException("获取用户信息失败!");
        //    }
        //    RewardOrder order = new RewardOrder();
        //    order.userip = Utility.Web.WebHelper.GetIP();
        //    order.addTime = DateTime.Now;
        //    order.articleId = comment.ArticleId;
        //    order.minisnsId = comment.MinisnsId;
        //    order.rewardFromUserId = looker.Id;
        //    order.rewardMoney = total_fee;
        //    order.rewardToUserId = maker.Id;
        //    order.out_trade_no = out_trade_no;
        //    order.status = 0;
        //    order.percent = ((100 - WebConfigBLL.VzanRewardPercent) - new MinisnsBll().GetModel(comment.MinisnsId).RewardPercent);
        //    //新增区分文章和回复类型
        //    order.rewardtype = 1;
        //    order.commentId = comment.Id;//如果是回复打赏，commentId为空
        //    RewardOrderBLL bllorder = new RewardOrderBLL();
        //    try
        //    {
        //        orderid = Convert.ToInt32(bllorder.Add(order));
        //    }
        //    catch (Exception ex)
        //    {
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("插入RewardOrder失败!" + ex.Message));
        //        throw;
        //    }
        //    //orderid = bllorder.GetModel(string.Format("out_trade_no='{0}'", order.out_trade_no)).Id;
        //    unifiedOrderResult = result;
        //    return result;
        //}

        ///// <summary>
        ///// 支付中心
        ///// </summary>
        ///// <param name="article">Morders订单实体</param>
        ///// <returns></returns>
        //public WxPayData GetUnifiedOrderResult(PayCenterSetting setting, Morders morder, bool livePay = false)
        //{
        //    //统一下单
        //    string out_trade_no = morder.orderno;//商户订单号
        //    if (string.IsNullOrEmpty(morder.orderno))
        //    {
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("统一下单失败，订单的内部订单号为空！:OrderID:" + morder.Id));
        //        throw new WxPayException("UnifiedOrder response error!");
        //    }
        //    string body = string.Empty;
        //    if (!string.IsNullOrEmpty(morder.ShowNote))
        //    {
        //        body = morder.ShowNote;
        //    }
        //    else
        //    {
        //        string paytype = string.Empty;
        //        switch (morder.OrderType)
        //        {
        //            case (int)ArticleTypeEnum.Offer:
        //                paytype = "悬赏贴";
        //                break;
        //            case (int)ArticleTypeEnum.Pay:
        //                paytype = "付费贴";
        //                break;
        //            case (int)ArticleTypeEnum.Stick:
        //                paytype = "置顶帖";
        //                break;
        //            case (int)ArticleTypeEnum.VoiceRedPacket:
        //                paytype = "语音红包";
        //                break;
        //            case (int)ArticleTypeEnum.Advert:
        //                paytype = "广告";
        //                break;
        //            case (int)ArticleTypeEnum.VoiceRedPacketRecharge:
        //                paytype = "红包充值";
        //                break;
        //            case (int)ArticleTypeEnum.LiveReward:
        //                paytype = "直播间打赏";
        //                break;
        //            case (int)ArticleTypeEnum.CashWishes:
        //                paytype = "送彩礼";
        //                break;
        //            case (int)ArticleTypeEnum.CityArticleTop:
        //            case (int)ArticleTypeEnum.CityBannerShow:
        //            case (int)ArticleTypeEnum.CityStoreTop:
        //                paytype = "同城推广";
        //                break;
        //        }
        //        body = string.Format("支付中心，{0}支付{1}元", paytype, (morder.payment_free * 0.01));//商品描述;
        //    }
        //    string attach = string.Format("paytype={0}&orderid={1}&orderno={2}&from=center", morder.OrderType, morder.Id, morder.orderno);//自带的信息
        //    //统一下单
        //    WxPayData data = new WxPayData();
        //    data.SetValue("body", body);
        //    data.SetValue("attach", attach);
        //    data.SetValue("out_trade_no", out_trade_no);
        //    data.SetValue("total_fee", total_fee);
        //    data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
        //    data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
        //    data.SetValue("goods_tag", "test");
        //    data.SetValue("trade_type", "JSAPI");
        //    data.SetValue("openid", openid);

        //    WxPayData result = WxPayApi.UnifiedOrder(data, setting,60,livePay);

        //    if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
        //    {
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("data:" + data.ToJson()));
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("result:" + result.ToJson()));
        //        log4net.LogHelper.WriteError(this.GetType(), new WxPayException("统一下单请求失败!prepay_id:" + result.GetValue("prepay_id").ToString()));
        //        throw new WxPayException("UnifiedOrder response error!");
        //    }
        //    unifiedOrderResult = result;
        //    return result;
        //}
        public WxPayData GetUnifiedOrderResultByCity(PayCenterSetting setting, CityMorders morder,string notify_url)
        {
            //统一下单
            string out_trade_no = morder.orderno;//商户订单号
            if (string.IsNullOrEmpty(morder.orderno))
            {
                //log4net.LogHelper.WriteError(this.GetType(), new WxPayException("统一下单失败，订单的内部订单号为空！:OrderID:" + morder.Id));
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
            //同城回调到同城站点
            //data.SetValue("notify_url", WebConfigBLL.citynotify_url);
            //data.SetValue("notify_url", WebConfigBLL.citynotify_url);
            data.SetValue("notify_url", notify_url);
            //log4net.LogHelper.WriteInfo(GetType(),"支付回调链接："+ WebConfigBLL.citynotify_url);
            WxPayData result = WxPayApi.UnifiedOrder(data, setting);
            
            if (result!=null&&(!result.IsSet("appid") || !result.IsSet("prepay_id") || !result.IsSet("prepay_id")))
            {
                //log4net.LogHelper.WriteError(this.GetType(), new WxPayException("data:" + data.ToJson()));
                //log4net.LogHelper.WriteError(this.GetType(), new WxPayException("result:" + result.ToJson()));
                //log4net.LogHelper.WriteError(this.GetType(), new WxPayException("统一下单请求失败!prepay_id:" + result.GetValue("prepay_id").ToString()));
                throw new WxPayException("UnifiedOrder response error!"+ "result:" + result.ToJson());
            }
            unifiedOrderResult = result;
            return result;
        }

        /**
        *  
        * 从统一下单成功返回的数据中获取微信浏览器调起jsapi支付所需的参数，
        * 微信浏览器调起JSAPI时的输入参数格式如下：
        * {
        *   "appId" : "wx2421b1c4370ec43b",     //公众号名称，由商户传入     
        *   "timeStamp":" 1395712654",         //时间戳，自1970年以来的秒数     
        *   "nonceStr" : "e61463f8efa94090b1f366cccfbbb444", //随机串     
        *   "package" : "prepay_id=u802345jgfjsdfgsdg888",     
        *   "signType" : "MD5",         //微信签名方式:    
        *   "paySign" : "70EA570631E4BB79628FBCA90534C63FF7FADD89" //微信签名 
        * }
        * @return string 微信浏览器调起JSAPI时的输入参数，json格式可以直接做参数用
        * 更详细的说明请参考网页端调起支付API：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_7
        * 
        */
        //public string GetJsApiParameters()
        //{
        //    WxPayData jsApiParam = new WxPayData();
        //    jsApiParam.SetValue("appId", unifiedOrderResult.GetValue("appid"));
        //    jsApiParam.SetValue("timeStamp", WxPayApi.GenerateTimeStamp());
        //    jsApiParam.SetValue("nonceStr", WxPayApi.GenerateNonceStr());
        //    jsApiParam.SetValue("package", "prepay_id=" + unifiedOrderResult.GetValue("prepay_id"));
        //    jsApiParam.SetValue("signType", "MD5");
        //    //第三方支付需要查询KEY的值
        //    string key = string.Empty;
        //    string appid = unifiedOrderResult.GetValue("appid").ToString();
        //    PayCenterSetting setting = new PayCenterSettingBLL().GetPayCenterSetting(appid);
        //    if (setting != null && setting.Id > 0)
        //    {
        //        key = setting.Key;
        //    }
        //    jsApiParam.SetValue("paySign", jsApiParam.MakeSign(key));
        //    return jsApiParam.ToJson();

        //}

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



        /**
	    * 
	    * 获取收货地址js函数入口参数,详情请参考收货地址共享接口：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_9
	    * @return string 共享收货地址js函数需要的参数，json格式可以直接做参数使用
	    */
        //public string GetEditAddressParameters()
        //{
        //    string parameter = "";
        //    try
        //    {
        //        string host = context.Request.Url.Host;
        //        string path = context.Request.Path;
        //        string queryString = context.Request.Url.Query;
        //        //这个地方要注意，参与签名的是网页授权获取用户信息时微信后台回传的完整url
        //        string url = "http://" + host + path + queryString;

        //        //构造需要用SHA1算法加密的数据
        //        WxPayData signData = new WxPayData();
        //        signData.SetValue("appid", WxPayConfig.APPID);
        //        signData.SetValue("url", url);
        //        signData.SetValue("timestamp", WxPayApi.GenerateTimeStamp());
        //        signData.SetValue("noncestr", WxPayApi.GenerateNonceStr());
        //        signData.SetValue("accesstoken", access_token);
        //        string param = signData.ToUrl();

        //        //SHA1加密
        //        string addrSign = FormsAuthentication.HashPasswordForStoringInConfigFile(param, "SHA1");

        //        //获取收货地址js函数入口参数
        //        WxPayData afterData = new WxPayData();
        //        afterData.SetValue("appId", WxPayConfig.APPID);
        //        afterData.SetValue("scope", "jsapi_address");
        //        afterData.SetValue("signType", "sha1");
        //        afterData.SetValue("addrSign", addrSign);
        //        afterData.SetValue("timeStamp", signData.GetValue("timestamp"));
        //        afterData.SetValue("nonceStr", signData.GetValue("noncestr"));

        //        //转为json格式
        //        parameter = afterData.ToJson();
        //    }
        //    catch (Exception ex)
        //    {
        //        log4net.LogHelper.WriteError(this.GetType(), ex);
        //        throw new WxPayException(ex.ToString());
        //    }

        //    return parameter;
        //}
        ///// <summary>
        ///// 商城支付专用
        ///// </summary>
        ///// <param name="amout"></param>
        ///// <param name="openid"></param>
        ///// <param name="orderid"></param>
        ///// <param name="paytype"></param>
        ///// <returns></returns>
        //public WxPayData GetUnifiedOrderResult(int amout, string openid, string orderid, int paytype, PayCenterSetting setting)
        //{
        //    //统一下单
        //    if (string.IsNullOrEmpty(orderid))
        //    {
        //        //log4net.LogHelper.WriteError(this.GetType(), new WxPayException("统一下单失败，订单的内部订单号为空！:orderid:" + orderid));
        //        throw new WxPayException("UnifiedOrder response error!");
        //    }
        //    string body = string.Empty;
        //    body = string.Format("微赞好店支付中心，商品支付{0}元", (amout * 0.01));//商品描述;
        //    string attach = string.Format("paytype={0}&orderid={1}&from=shop", paytype, orderid);//自带的信息
        //    //统一下单
        //    WxPayData data = new WxPayData();
        //    data.SetValue("body", body);
        //    data.SetValue("attach", attach);
        //    data.SetValue("out_trade_no", WxPayApi.GenerateOutTradeNo());
        //    data.SetValue("total_fee", amout);
        //    data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
        //    data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
        //    data.SetValue("goods_tag", "vzangoods");
        //    data.SetValue("trade_type", "JSAPI");
        //    data.SetValue("openid", openid);
        //    WxPayData result = WxPayApi.UnifiedOrder(data, setting);
        //    //log4net.LogHelper.WriteError(this.GetType(), new WxPayException("data:" + data.ToJson()));
        //    if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
        //    {
        //        //log4net.LogHelper.WriteError(this.GetType(), new WxPayException("data:" + data.ToJson()));
        //        //log4net.LogHelper.WriteError(this.GetType(), new WxPayException("result:" + result.ToJson()));
        //        //log4net.LogHelper.WriteError(this.GetType(), new WxPayException("统一下单请求失败!prepay_id:" + result.GetValue("prepay_id").ToString()));
        //        throw new WxPayException("UnifiedOrder response error!");
        //    }
        //    unifiedOrderResult = result;
        //    return result;
        //}
    }
}