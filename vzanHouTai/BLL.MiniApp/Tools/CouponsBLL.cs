using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.Plat;
using BLL.MiniApp.Stores;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.Plat;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Tools
{
    public class CouponsBLL : BaseMySql<Coupons>
    {
        #region 单例模式
        private static CouponsBLL _singleModel;
        private static readonly object SynObject = new object();

        private CouponsBLL()
        {

        }

        public static CouponsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CouponsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }



        public CreateCardResult AddWxCoupons(Coupons coupons, XcxAppAccountRelation xcx, string accountId)
        {
            //默认专业版的
            string center_app_brand_pass = "pages/my/myInfo";//专业版 个人中心
            string custom_app_brand_pass = "pages/index/index";//首页 专业版
            string logo_url = string.Empty;
            string brand_name = string.Empty;
            string appOriginalId = string.Empty;
            CreateCardResult _createCardResult = new CreateCardResult();
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={xcx.TId}");
            if (xcxTemplate == null)
            {
                _createCardResult.errcode = 1;
                _createCardResult.errmsg = "小程序模板不存在";
                return _createCardResult;
            }

            List<OpenAuthorizerConfig> listOpenAuthorizerConfig = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(accountId, xcx.Id, 4);
            if (listOpenAuthorizerConfig == null)
            {
                _createCardResult.errcode = 1;
                _createCardResult.errmsg = "请先绑定认证服务号才有生成卡券权限";
                return _createCardResult;
            }
            OpenAuthorizerConfig umodel = listOpenAuthorizerConfig[0];
            switch (xcxTemplate.Type)
            {
                case (int)TmpType.小程序专业模板:
                    EntSetting ent = EntSettingBLL.SingleModel.GetModel(coupons.appId);
                    if (ent == null)
                    {
                        _createCardResult.errcode = 1;
                        _createCardResult.errmsg = "该专业版信息找不到";
                        return _createCardResult;
                    }
                    OpenAuthorizerConfig XUserList = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcx.AppId);
                    if (XUserList == null)
                    {
                        _createCardResult.errcode = 1;
                        _createCardResult.errmsg = "请先授权给平台";
                        return _createCardResult;
                    }
                    ConfParam imginfo = ConfParamBLL.SingleModel.GetModelByParamappid("logoimg", xcx.AppId);
                    if (imginfo == null)
                    {
                        _createCardResult.errcode = 1;
                        _createCardResult.errmsg = "请先到小程序管理配置底部Logo";
                        return _createCardResult;
                    }
                    logo_url = imginfo.Value;
                    brand_name = XUserList.nick_name;

                    break;
                case (int)TmpType.小程序电商模板:
                    center_app_brand_pass = "pages/me/me";//个人中心页面
                    Store store = StoreBLL.SingleModel.GetModelByAId(xcx.Id);
                    if (store == null)
                    {
                        _createCardResult.errcode = 1;
                        _createCardResult.errmsg = "电商版店铺不存在";
                        return _createCardResult;
                    }
                    logo_url = store.logo;
                    brand_name = store.name;
                    break;

                case (int)TmpType.小程序餐饮模板:
                    center_app_brand_pass = "pages/me/me";//个人中心页面
                    custom_app_brand_pass = "pages/home/home";//首页
                    Food miAppFood = FoodBLL.SingleModel.GetModel($"appId={xcx.Id}");
                    if (miAppFood == null)
                    {
                        _createCardResult.errcode = 1;
                        _createCardResult.errmsg = "餐饮版店铺不存在";
                        return _createCardResult;
                    }

                    logo_url = miAppFood.Logo;
                    brand_name = miAppFood.FoodsName;
                    break;
                case (int)TmpType.小程序足浴模板:
                case (int)TmpType.小程序多门店模板:
                    int t = 0;
                    if ((int)TmpType.小程序足浴模板 == xcxTemplate.Type)
                    {
                        center_app_brand_pass = "pages/me/me";//个人中心页面
                        custom_app_brand_pass = "pages/book/book";//首页
                    }
                    else
                    {
                        center_app_brand_pass = "pages/me/me";//个人中心页面
                        t = 1;
                    }
                    FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={xcx.Id}");
                    if (storeModel == null)
                    {
                        _createCardResult.errcode = 1;
                        _createCardResult.errmsg = "找不到该足浴版";
                        return _createCardResult;
                    }
                    brand_name = storeModel.StoreName;
                    List<C_Attachment> LogoList = C_AttachmentBLL.SingleModel.GetListByCache(storeModel.Id, t == 0 ? (int)AttachmentItemType.小程序足浴版店铺logo : (int)AttachmentItemType.小程序多门店版门店logo);
                    if (LogoList != null && LogoList.Count > 0)
                    {
                        logo_url = LogoList[0].filepath;
                    }
                    break;
                case (int)TmpType.小未平台子模版:
                    center_app_brand_pass = "pages/my/my-index/index";//个人中心页面
                    custom_app_brand_pass = "pages/home/shop-detail/index";//首页
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(xcx.Id, 2);
                    if (platStore == null)
                    {
                        _createCardResult.errcode = 1;
                        _createCardResult.errmsg = "平台版店铺不存在";
                        return _createCardResult;
                    }
                    brand_name = platStore.Name;
                    logo_url = platStore.StoreHeaderImg;
                    break;

            }

            if (string.IsNullOrEmpty(logo_url) || string.IsNullOrEmpty(brand_name))
            {
                _createCardResult.errcode = 1;
                _createCardResult.errmsg = "请先配置Logo以及名称";
                return _createCardResult;

            }

            //这里可能会出现token失效 个人发布的未授权给我们第三方平台的卡券会生成不了
            string xcxapiurl = XcxApiBLL.SingleModel.GetOpenAuthodModel(umodel.user_name);
            string authorizer_access_token = CommondHelper.GetAuthorizer_Access_Token(xcxapiurl);

            string uploadImgResult = CommondHelper.WxUploadImg(authorizer_access_token, logo_url);
            if (!uploadImgResult.Contains("url"))
            {
                _createCardResult.errcode = 1;
                _createCardResult.errmsg = $"上传Logo到微信失败uploadImgResult={uploadImgResult}";
                return _createCardResult;
            }
            if (brand_name.Length >= 12)
            {
                brand_name = brand_name.Substring(0, 12);
            }



            WxUploadImgResult wxUploadImgResult = JsonConvert.DeserializeObject<WxUploadImgResult>(uploadImgResult);
            logo_url = wxUploadImgResult.url;

            base_info _base_info = new base_info();
            _base_info.logo_url = logo_url;
            _base_info.code_type = "CODE_TYPE_TEXT";
            _base_info.brand_name = brand_name;
            _base_info.title = coupons.CouponName.Length > 9 ? Utility.StringHelper.strSubstring(coupons.CouponName, 0, 8) : coupons.CouponName;
            _base_info.color = "Color010";

            _base_info.center_title = "立即使用";
            _base_info.center_app_brand_user_name = $"{appOriginalId}@app";
            _base_info.center_app_brand_pass = center_app_brand_pass;

            _base_info.custom_url_name = "小程序";
            _base_info.custom_app_brand_user_name = $"{appOriginalId}@app";
            _base_info.custom_app_brand_pass = custom_app_brand_pass;
            _base_info.custom_url_sub_title = "点击进入";


            _base_info.description = coupons.Desc.Length > 1024 ? Utility.StringHelper.strSubstring(coupons.Desc, 0, 1023) : coupons.Desc;
            _base_info.notice = "使用时向服务员出示此券";
            _base_info.sku.quantity = coupons.CreateNum;
            if (coupons.LimitReceive > 0)
            {
                //大于0表示限制,0表示无限制
                _base_info.get_limit = coupons.LimitReceive;
            }


            if (coupons.ValType == 0)
            {
                //表示固定日期
                _base_info.date_info = new Firstdate_infoItem()
                {
                    begin_timestamp = WxUtils.GetWeixinDateTime(coupons.StartUseTime),
                    end_timestamp = WxUtils.GetWeixinDateTime(coupons.EndUseTime)
                };
            }
            if (coupons.ValType == 2|| coupons.ValType == 1)
            {
                //fixed_begin_term=0表示领取后当天开始生效 1表示次日后开始生效 领取当日N天内有效
                _base_info.date_info = new Seconddate_infoItem()
                {
                    fixed_begin_term = coupons.ValType==2?0:1,
                    fixed_term = coupons.ValDay
                };
            }


            string json = string.Empty;
            if (coupons.CouponWay == 0)
            {
                //表示是需要生成微信代金券
                WxCashCoupons wxCashCoupons = new WxCashCoupons();
                Cash cash = new Cash();
                cash.base_info = _base_info;//基础字段信息

                Use_condition use_condition = new Use_condition();

                use_condition.accept_category = coupons.GoodsType == 0 ? "全部产品" : "部分产品";
                if (coupons.LimitMoney > 0)
                {
                    cash.least_cost = coupons.LimitMoney;
                    //满减门槛
                    use_condition.least_cost = coupons.LimitMoney;
                }
                use_condition.can_use_with_other_discount = coupons.discountType == 0;

                cash.reduce_cost = coupons.Money;
                cash.advanced_info = new Advanced_info() { use_condition = use_condition };
                wxCashCoupons.cash = cash;
                json = JsonConvert.SerializeObject(new { card = wxCashCoupons });
            }
            else
            {
                //表示是需要生成微信折扣券
                WxDiscountCoupons wxDiscountCoupons = new WxDiscountCoupons();
                Discount discount = new Discount();
                discount.base_info = _base_info;
                discount.discount =(int)(100 - coupons.Money*0.1);
                wxDiscountCoupons.discount = discount;
                json = JsonConvert.SerializeObject(new { card = wxDiscountCoupons });

            }

            string result = Utility.IO.Context.PostData($"https://api.weixin.qq.com/card/create?access_token={authorizer_access_token}", json);
            if (string.IsNullOrEmpty(result))
                return _createCardResult;
            _createCardResult = JsonConvert.DeserializeObject<CreateCardResult>(result);
            return _createCardResult;
        }


        #endregion
        /// <summary>
        /// 获取已设置首页弹窗显示优惠券的数量
        /// </summary>
        /// <param name="rid"></param>
        /// <returns></returns>
        public int GetShowTipCount(int rid, int couponsId = 0)
        {
            string strSql = $"select Count(*) from coupons where  appid={rid} and ticketType={(int)TicketType.优惠券} and IsShowTip=1 and State!=-1";
            if (couponsId > 0)
            {
                strSql += $" and Id<>{couponsId}";
            }
            var result = SqlMySql.ExecuteScalar(connName, CommandType.Text, strSql);
            if (DBNull.Value != result)
            {
                return Convert.ToInt32(result);
            }

            return 0;
        }

        #region 基础操作

        /// <summary>
        /// 获取有效的优惠券记录
        /// </summary>
        /// <returns></returns>
        public Coupons GetValidModel(int id, string nowtime)
        {
            //调整为未到时间的优惠券也能领取 但是不能使用
            return base.GetModel($"id={id} and state!=-1 and ( endusetime>'{nowtime}' or valtype!=0)");

            // return base.GetModel($"id={id} and state!=-1 and (startusetime<'{nowtime}' and endusetime>'{nowtime}' or valtype!=0)");
        }

        public List<Coupons> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<Coupons>();

            return base.GetList($"id in ({ids})");
        }
        #endregion

        public List<Coupons> GetCouponList(string couponname, int state, int storeid, int rid, TicketType ticketType, int pageSize, int pageIndex, string strOrder, int goodstype = -1, int IsShowTip = 0)
        {
            List<Coupons> list = new List<Coupons>();
            List<MySqlParameter> param = new List<MySqlParameter>();
            string strSql = $"select * from coupons where storeid={storeid} and appid={rid} and ticketType={(int)ticketType} ";
            if (IsShowTip != 0)
            {
                strSql += $" and IsShowTip={IsShowTip}";
            }
            strSql = GetSelectWhereSql(strSql, couponname, state, storeid, rid, ticketType, ref param);

            //是否指定商品有优惠
            if (goodstype == 0)
            {
                //全部商品
                strSql += $" and (goodsidstr is NULL or goodsidstr = '')";
            }
            else if (goodstype > 0)
            {
                //指定某件商品
                strSql += $" and  FIND_IN_SET(@goodsidstr,goodsidstr)";
                param.Add(new MySqlParameter("@goodsidstr", goodstype));
            }
            strSql += $" order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            //log4net.LogHelper.WriteInfo(this.GetType(),strSql);

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, strSql, param.ToArray()))
            {
                while (dr.Read())
                {
                    var model = GetModel(dr);

                    list.Add(model);
                }
            }
            return list;
        }

        public int GetCouponListCount(string couponname, int state, int storeid, int rid, TicketType ticketType)
        {
            List<MySqlParameter> param = new List<MySqlParameter>();

            string strSql = $"select Count(*) from coupons where storeid={storeid} and appid={rid} and ticketType={(int)ticketType}";
            strSql = GetSelectWhereSql(strSql, couponname, state, storeid, rid, ticketType, ref param);

            var result = SqlMySql.ExecuteScalar(connName, CommandType.Text, strSql, param.ToArray());
            if (DBNull.Value != result)
            {
                return Convert.ToInt32(result);
            }

            return 0;
        }

        private string GetSelectWhereSql(string strSql, string couponname, int state, int storeid, int rid, TicketType ticketType, ref List<MySqlParameter> param)
        {
            string nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (state == 0)//全部
            {
                strSql += $" and state=0";
            }
            else if (state == 1)//未开始
            {
                switch ((int)ticketType)
                {
                    case (int)TicketType.优惠券:
                        strSql += $" and StartUseTime>'{nowtime}' and valtype=0 and state<>-1";
                        break;
                    case (int)TicketType.立减金:
                        strSql += $" and StartUseTime>'{nowtime}' and valtype=0 and state=11";
                        break;
                }
            }
            else if (state == 2)
            {
                //进行中
                switch ((int)ticketType)
                {
                    case (int)TicketType.优惠券:
                        strSql += $" and (StartUseTime<'{nowtime}' and EndUseTime>'{nowtime}' or valtype!=0) and state<>-1";
                        break;
                    case (int)TicketType.立减金:
                        strSql += $" and (StartUseTime<'{nowtime}' and EndUseTime>'{nowtime}' or valtype!=0) and state=1";
                        break;
                }


            }
            else if (state == 3)
            {
                //已结束
                strSql += $" and EndUseTime<'{nowtime}' and valtype=0 and state not in(-1)";
            }
            else if (state == 4)
            {
                //已无效
                strSql += $" and state = -1";
            }
            else if (state == 5)
            {
                //全部,但是除了已经结束的
                strSql += $" and state=0 and (EndUseTime>'{nowtime}'or valtype!=0)";
            }

            //优惠券名称
            if (!string.IsNullOrEmpty(couponname))
            {
                strSql += " and couponname like @couponname";
                param.Add(new MySqlParameter("@couponname", "%" + couponname + "%"));
            }

            return strSql;
        }

        /// <summary>
        /// 立减金的状态情况
        /// </summary>
        /// <param name="couponsModel"></param>
        /// <returns></returns>
        public string GetStateName(Coupons couponsModel)
        {
            string result = "已失效";
            switch (couponsModel.State)
            {
                case (int)CouponState.已失效:
                    result = "已失效";
                    break;
                case (int)CouponState.已开启:
                    result = "进行中";
                    if (DateTime.Compare(couponsModel.StartUseTime, DateTime.Now) > 0)
                    {
                        result = "未开始";
                    }
                    if (DateTime.Compare(couponsModel.EndUseTime, DateTime.Now) < 0 && couponsModel.ValType == 0)
                    {
                        result = "已结束";
                    }
                    if (couponsModel.CreateNum <= 0)
                    {
                        result = "已领完";
                    }
                    break;
                case (int)CouponState.已关闭:
                    result = "已关闭";
                    break;
                default:
                    result = "一个奇怪的状态";
                    break;
            }
            return result;
        }

        /// <summary>
        /// 根据id，appid获取model
        /// </summary>
        /// <param name="id"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public Coupons GetModelByIdAndAppId(int id, int appId, int? state = null)
        {
            string sqlwhere = $"id={id} and appid={appId}";
            if (state != null)
            {
                sqlwhere += $" and state={state.Value}";
            }
            return GetModel(sqlwhere);
        }

        /// <summary>
        /// 获取已开启的立减金（立减金只能开启一份）
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public Coupons GetOpenedModel(int appId)
        {
            DateTime date = DateTime.Now;
            return GetModel($"appid={appId} and state={(int)CouponState.已开启} and ticketType={(int)TicketType.立减金} and ((startusetime<='{date.ToString("yyyy-MM-dd HH:mm:ss")}' and endusetime>'{date.ToString("yyyy-MM-dd HH:mm:ss")}' and valtype=0) or valtype in(1,2))");
        }
        /// <summary>
        ///  获取已开启的立减金（立减金只能开启一份）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public Coupons GetOpenedModel(int appId, int storeId)
        {
            DateTime date = DateTime.Now;
            return GetModel($"appid={appId} and storeid={storeId} and state={(int)CouponState.已开启} and ticketType={(int)TicketType.立减金} and ((startusetime<='{date.ToString("yyyy-MM-dd HH:mm:ss")}' and endusetime>'{date.ToString("yyyy-MM-dd HH:mm:ss")}' and valtype=0) or valtype in(1,2))");
        }
        /// <summary>
        /// 获取已开通的立减金
        /// </summary>
        /// <returns></returns>
        public Coupons GetVailtModel(int aid)
        {
            if (aid <= 0)
                return new Coupons();
            Coupons model = GetOpenedModel(aid);
            if (model != null)
            {
                int count = GetCountBySql($"SELECT count(fromorderid) from(select fromorderid from couponlog where CouponId = {model.Id} group by fromorderid) a");//已领取的份数
                model.RemNum = model.CreateNum - count;
            }

            return model;
        }
        /// <summary>
        /// 获取已开通的立减金
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public Coupons GetVailtModel(int aid, int storeId)
        {
            if (aid <= 0 || storeId <= 0)
                return new Coupons();
            Coupons model = GetOpenedModel(aid, storeId);
            if (model != null)
            {
                int count = GetCountBySql($"SELECT count(fromorderid) from(select fromorderid from couponlog where CouponId = {model.Id} group by fromorderid) a");//已领取的份数
                model.RemNum = model.CreateNum - count;
            }

            return model;
        }

        /// <summary>
        /// 获取已开启的立减金（立减金只能开启一份）
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public Coupons GetVailtModelByAppId(string appId)
        {
            XcxAppAccountRelation appRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appId);
            if (appRelation == null)
            {
                return null;
            }
            return GetVailtModel(appRelation.Id);
        }

        /// <summary>
        /// 根据状态判断是否有开启立减金活动（后台操作使用到）
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public Coupons GetOpenedModelByState(int appId, int storeId)
        {
            DateTime date = DateTime.Now;
            string sqlwhere = $"appid={appId} and state={(int)CouponState.已开启} and (( endusetime>'{date.ToString("yyyy-MM-dd HH:mm:ss")}' and valtype=0) or valtype in(1,2)) and ticketType={(int)TicketType.立减金}";
            if (storeId > 0)
            {
                sqlwhere += $" and storeId={storeId}";
            }
            return GetModel(sqlwhere);
        }

        /// <summary>
        /// 获取未满足条件的立减金
        /// </summary>
        /// <param name="storeid"></param>
        /// <param name="aid"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<Coupons> GetUnsatisfiedReductionCard(int storeid, int aid, int userid)
        {
            string date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            List<Coupons> list = null;
            string sql = $"select c.*,l.fromorderid from couponlog l left join coupons c on l.couponId=c.id where l.state=4 and c.state ={(int)CouponState.已开启} and c.storeid={storeid} and c.appid={aid} and l.userid={userid} and (c.valtype!=0 or (c.valtype=0 and c.startusetime<='{date}' and c.endusetime>'{date}' ))";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql))
            {
                list = new List<Coupons>();
                while (dr.Read())
                {
                    Coupons coupon = GetModel(dr);
                    coupon.fromOrderId = Convert.ToInt32(dr["fromorderid"]);
                    list.Add(coupon);
                }
            }
            return list;
        }
        /// <summary>
        /// 获取平台未满足条件的立减金
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<Coupons> GetUnsatisfiedReductionCard(int aid, int userid)
        {
            string date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            List<Coupons> list = null;
            string sql = $"select c.*,l.fromorderid from couponlog l left join coupons c on l.couponId=c.id where l.state=4 and c.state ={(int)CouponState.已开启} and c.appid={aid} and l.userid={userid} and (c.valtype!=0 or (c.valtype=0 and c.startusetime<='{date}' and c.endusetime>'{date}' )) order by c.id desc ";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql))
            {
                list = new List<Coupons>();
                while (dr.Read())
                {
                    Coupons coupon = GetModel(dr);
                    coupon.fromOrderId = Convert.ToInt32(dr["fromorderid"]);
                    list.Add(coupon);
                }
            }
            return list;
        }
        public int GetAbleCountByAppId(int aid, TicketType ticketType)
        {
            return base.GetCount($"appid={aid} and ticketType={(int)ticketType} and  state=0 and (EndUseTime>'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'or valtype!=0)");
        }

    }
}
