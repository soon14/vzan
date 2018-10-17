using System;
using System.Collections.Generic;
using System.Linq;
using BLL.MiniApp.Conf;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Stores;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace BLL.MiniApp.Ent
{
    public class EntUserFormBLL : BaseMySql<EntUserForm>
    {
        #region 单例模式
        private static EntUserFormBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntUserFormBLL()
        {

        }

        public static EntUserFormBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntUserFormBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 验证是否已经预约过
        /// </summary>
        /// <param name="goodsId">预约的产品Id</param>
        /// <param name="userId">预约的用户Id</param>
        /// <returns></returns>
        public bool IsSubscribe(int goodsId, int userId)
        {
            if (goodsId <= 0 || userId <= 0)
            {
                return false;
            }
            string sqlwhere = $"uid={userId} and type=1 and remark like '%\"id\":\"{goodsId}\"%'";
            EntUserForm form = GetModel(sqlwhere);
            return form != null;
        }

        /// <summary>
        /// 获取预约信息
        /// </summary>
        /// <param name="goodsId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public EntUserForm GetGoodsSubscribeInfo(int goodsId, int userId)
        {
            if (goodsId <= 0 || userId <= 0)
            {
                return null;
            }
            string sqlwhere = $"uid={userId} and type=1 and remark like '%\"id\":\"{goodsId}\"%'";
            EntUserForm form = GetModel(sqlwhere);

            return form;
        }

        public List<EntUserForm> GetListByAidUid(int aid, int uid, int type, int pagesize, int pageindex, string orderstr)
        {
            string sqlwhere = $"aid={aid} and uid={uid} and type=1 and state>0";
            return GetList(sqlwhere, pagesize, pageindex, "*", orderstr);
        }

        public List<EntUserForm> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<EntUserForm>();

            return base.GetList($"id in ({ids})");
        }

        #region 商家小程序

        /// <summary>
        /// 根据条件获取相应的订单数量
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetOrderSumByCondition(int aid, int type, string value, string startDate = "", string endDate = "")
        {
            int count = 0;
            if (aid <= 0)
            {
                return count;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            string sqlwhere = GetSqlwhere(paramters, aid, type, value, startDate, endDate); ;
            sqlwhere += " and state> -1";
            count = GetCount(sqlwhere, paramters.ToArray());
            return count;
        }

        /// <summary>
        /// 根据条件和状态获取相应的订单数量
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public int GetOrderSumByCondition(int aid, int type, string value, int state)
        {
            int count = 0;
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            string sqlwhere = GetSqlwhere(paramters, aid, type, value, "", "", state); //$"aid={aid} and state={state}";
            count = GetCount(sqlwhere, paramters.ToArray());
            return count;
        }

        public object GetListByCondition(int aid, int pageIndex, int pageSize, int state, int type, string value, string startDate, string endDate)
        {
            List<EntUserForm> formList = new List<EntUserForm>();
            if (aid <= 0)
            {
                return formList;
            }
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string sqlwhere = GetSqlwhere(parameters, aid, type, value, startDate, endDate, state);
            formList = GetListByParam(sqlwhere, parameters.ToArray(), pageSize, pageIndex, "*", "id desc");
            return formList;
        }

        private string GetSqlwhere(List<MySqlParameter> parameters, int aid, int type, string value, string startDate = "", string endDate = "", int state = -999)
        {
            string sqlwhere = $"aid={aid} and type=1";
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                sqlwhere += " and addtime>=@startDate and addtime <= @endDate";
                parameters.Add(new MySqlParameter("@startDate", startDate));
                parameters.Add(new MySqlParameter("@endDate", endDate));
            }
            if (!string.IsNullOrEmpty(value))
            {
                switch (type)
                {
                    case 1://商品名称
                        sqlwhere += $" and remark like @goods";
                        parameters.Add(new MySqlParameter("@goods", $"%\"goods\":{{\"name\":\"{value}%\"}}%"));
                        break;

                    case 2://手机号码
                        sqlwhere += $" and formdatajson like @phone";
                        parameters.Add(new MySqlParameter("@phone", $"%\"手机号码\":\"{value}%\"%"));
                        break;

                    case 3://客户名称
                        sqlwhere += $" and formdatajson like @name";
                        parameters.Add(new MySqlParameter("@name", $"%\"姓名\":\"{value}%\"%"));
                        break;
                }
            }
            if (state != -999)
            {
                sqlwhere += $" and state={state}";
            }
            return sqlwhere;
        }

        #endregion 商家小程序
        /// <summary>
        /// 创建预约付费订单
        /// </summary>
        /// <param name="aid">小程序aid</param>
        /// <param name="uid">userId</param>
        /// <param name="remark">预约商品信息Json字符串 EntFormRemark</param>
        /// <returns></returns>
        public EntGoodsOrder CreateOrder(int aid, int userId, Store store, string remark, int buyMode, out string msg)
        {

            EntGoodsOrder order = null;
            try
            {
                EntFormRemark formRemark = JsonConvert.DeserializeObject<EntFormRemark>(remark);
                EntGoods goods = EntGoodsBLL.SingleModel.GetModel(formRemark.goods.id);
                if (goods == null)
                {
                    msg = "订单生成失败：商品信息不存在";
                    return order;
                }
                //商品标价
                int originalPrice = originalPrice = Convert.ToInt32(!string.IsNullOrWhiteSpace(formRemark.attrSpacStr) ? goods.GASDetailList.First(x => x.id.Equals(formRemark.attrSpacStr)).originalPrice * 100 : goods.originalPrice * 100);
                //商品未打折价格
                int price = Convert.ToInt32(!string.IsNullOrWhiteSpace(formRemark.attrSpacStr) ? goods.GASDetailList.First(x => x.id.Equals(formRemark.attrSpacStr)).price * 100 : goods.price * 100);
                //清单
                EntGoodsCart goodsCar = new EntGoodsCart
                {
                    NotDiscountPrice = price,
                    originalPrice = originalPrice,
                    GoodName = goods.name,
                    FoodGoodsId = goods.id,
                    SpecIds = formRemark.attrSpacStr,
                    Count = formRemark.count,
                    Price = price,
                    SpecInfo = formRemark.SpecInfo,
                    SpecImg = formRemark.SpecImg,//规格图片
                    UserId = userId,
                    CreateDate = DateTime.Now,
                    State = 0,
                    GoToBuy = 1,
                    aId = aid,
                    type = (int)EntGoodCartType.预约表单
                };
                goodsCar.Id = Convert.ToInt32(EntGoodsCartBLL.SingleModel.Add(goodsCar));
                if (goodsCar.Id <= 0)
                {
                    msg = "订单生成失败：购物车添加失败";
                    return order;
                }
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
                if (userInfo == null)
                {
                    msg = "订单生成失败：用户信息不存在";
                    return order;
                }
                XcxAppAccountRelation xcxAppAccount = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
                if (xcxAppAccount == null)
                {
                    msg = "订单生成失败：小程序信息不存在";
                    return order;
                }
                int buyPrice = 0;
                if (store.funJoinModel.YuyuePayType == 0)//固定金额付费
                {
                    buyPrice = store.funJoinModel.YuyuePayCount * 100 * goodsCar.Count;
                }
                else
                {
                    double priceData = goodsCar.Price * goodsCar.Count * store.funJoinModel.YuyuePayCount * 0.01;
                    buyPrice = Convert.ToInt32(Math.Ceiling(priceData));
                }
                order = new EntGoodsOrder()
                {
                    BuyPrice = buyPrice,
                    GoodsGuid = goodsCar.FoodGoodsId.ToString(),
                    UserId = userInfo.Id,
                    CreateDate = DateTime.Now,
                    OrderType = (int)EntOrderType.预约付费订单,
                    QtyCount = goodsCar.Count,
                    aId = aid,
                    BuyMode = buyMode,
                    
                };
                order.Id = Convert.ToInt32(EntGoodsOrderBLL.SingleModel.Add(order));
                if (order.Id <= 0)
                {
                    msg = "订单生成失败：生成失败";
                    return null;
                }
                //将订单id添加到清单
                goodsCar.GoodsOrderId = order.Id;
                goodsCar.State = 1;
                EntGoodsCartBLL.SingleModel.Update(goodsCar, "state,goodsorderid");

                //生成对外订单号
                string outTradeNo = order.Id.ToString();
                if (outTradeNo.Length >= 3)
                {
                    outTradeNo = outTradeNo.Substring(outTradeNo.Length - 3, 3);
                }
                else
                {
                    outTradeNo.PadLeft(3, '0');
                }
                outTradeNo = $"{DateTime.Now.ToString("yyyyMMddHHmm")}{outTradeNo}";
                order.OrderNum = outTradeNo;
                if (order.BuyMode == (int)miniAppBuyMode.微信支付)
                {
                    CityMordersBLL cityMordersBLL = new CityMordersBLL();
                    //创建微信订单
                    CityMorders cityMorders = cityMordersBLL.CreateCityMorder((int)ArticleTypeEnum.EntSubscribeFormPay, (int)ArticleTypeEnum.EntSubscribeFormPay, order.BuyPrice, 99, aid, userInfo.Id, userInfo.NickName, order.Id, xcxAppAccount.AppId, xcxAppAccount.Title);
                    if (cityMorders == null)
                    {
                        msg = "订单生成失败：生成微信订单失败";
                        return null;
                    }
                    order.OrderId = cityMorders.Id;
                    if(!EntGoodsOrderBLL.SingleModel.Update(order, "orderid,OrderNum"))
                    {
                        msg = "订单生成失败：微信支付错误";
                        return null;
                    }
                    msg = string.Empty;
                    return order;
                }
                else if (order.BuyMode == (int)miniAppBuyMode.储值支付)
                {
                    SaveMoneySetUser saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(xcxAppAccount.AppId, userInfo.Id);
                    if (saveMoneyUser == null || saveMoneyUser.AccountMoney < order.BuyPrice)
                    {
                        msg = "订单生成失败：预存款余额不足";
                        return null;
                    }
                    if(!SaveMoneySetUserBLL.SingleModel.paySubscribeFromOrderBySaveMoneyUser(order, saveMoneyUser))
                    {
                        msg = "订单生成失败：储值支付失败";
                        return null;
                    }
                    msg = string.Empty;
                    return order;
                }
                else
                {
                    msg = "订单生成失败：支付方式错误";
                    return null;
                }
            }
            catch
            {
                msg = "订单生成失败：remark error";
                return order;
            }

        }

    }
}