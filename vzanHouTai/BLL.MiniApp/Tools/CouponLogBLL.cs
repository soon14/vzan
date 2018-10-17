using BLL.MiniApp.Conf;
using BLL.MiniApp.Tools;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Utility;

namespace BLL.MiniApp.Tools
{
    public class CouponLogBLL : BaseMySql<CouponLog>
    {
        #region 单例模式
        private static CouponLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private CouponLogBLL()
        {

        }

        public static CouponLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CouponLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        #region 基础操作
        /// <summary>
        /// 店铺领券人数
        /// </summary>
        /// <returns></returns>
        public int GetUserCount(int aid)
        {
            string sql = $"select Count(*) from couponlog l left join coupons c on c.id = l.couponid WHERE c.appid = {aid} GROUP BY l.userid";
            object result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (DBNull.Value != result)
            {
                return Convert.ToInt32(result);
            }
            return 0;
        }
        public List<CouponLog> GetListByIds(string ids,int state=-999)
        {
            string sqlWhere = $"couponid in ({ids})";
            if(state!=-999)
            {
                sqlWhere += $" and state!=4";
            }
            return base.GetList(sqlWhere);
        }
        public List<CouponLog> GetListByOrderId(int couponId,int orderId)
        {
            return base.GetList($" CouponId={couponId} and fromorderid={orderId} ");
        }
        public List<CouponLog> GetListByUserId(int couponId, int userId)
        {
            return base.GetList($" CouponId={couponId} and userid={userId} ");
        }
        public int GetCountByCouponId(int couponid)
        {
            return base.GetCount($"couponid={couponid}");
        }
        public int GetCountById(int id)
        {
            return base.GetCountBySql($"select count(*) from couponlog where CouponId={id} group by fromorderid"); 
        }
        public int GetCountByCouponIdAndUserId(int couponid,int userid)
        {
            return base.GetCount($"couponid={couponid} and userid={userid}");
        }
        public bool UpdateCouponLogState(string ids,int state,DateTime startUseTime,DateTime endUseTime)
        {
            if (string.IsNullOrEmpty(ids))
                return false;
            string sql = $"update CouponLog set State={state},StartUseTime='{startUseTime}',EndUseTime='{endUseTime}' where id in ({ids})";
            return SqlMySql.ExecuteNonQuery(connName,CommandType.Text,sql) > 0;
        }

        #endregion
        /// <summary>
        /// storeId不查可传-999
        /// </summary>
        /// <param name="state"></param>
        /// <param name="userId"></param>
        /// <param name="storeid"></param>
        /// <param name="rid"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strOrder"></param>
        /// <param name="goodsId"></param>
        /// <param name="goodsinfo"></param>
        /// <param name="couponsLogId"></param>
        /// <returns></returns>
        public List<CouponLog> GetCouponList(int state, int userId, int storeid, int rid, int pageSize, int pageIndex, string strOrder, string goodsId, string goodsinfo, int couponsLogId = 0)
        {
            List<CouponLog> list = GetListByApi(state,userId.ToString(),storeid,rid.ToString(),pageSize,pageIndex,strOrder,goodsId,goodsinfo,couponsLogId);
            return list;
        }

        public List<CouponLog> GetListByApi(int state, string userIds, int storeid, string aids, int pageSize, int pageIndex, string strOrder, string goodsId, string goodsinfo, int couponsLogId = 0)
        {
            List<CouponLog> list = new List<CouponLog>();
            if (string.IsNullOrEmpty(aids) || string.IsNullOrEmpty(userIds))
                return list;

            string strSql = $"select l.id,p.couponname,l.userid,l.couponid,l.startusetime,l.endusetime,l.state,p.couponway,p.money,p.limitmoney,p.valtype,p.GoodsIdStr,p.discountType,p.storeid from couponlog l left join coupons p on l.couponid=p.id where p.appid in({aids}) and l.userid in({userIds}) ";
            if(storeid!=-999)
            {
                strSql += $" and p.storeid ={storeid} ";
            }
            string nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (couponsLogId > 0)
            {
                strSql += $" and l.id = {couponsLogId} ";
            }
            
            //判断是否获取指定产品可使用的优惠券
            if (string.IsNullOrEmpty(goodsId) || goodsId=="0")
            {
                switch(state)
                {
                    case 0: strSql += $" and l.EndUseTime>'{nowtime}' and l.state not in (1,4) and p.state!=-1"; break; //未使用
                    case 1: strSql += $" and l.state=1"; break;//已使用
                    case 2: strSql += $" and l.EndUseTime<'{nowtime}' and l.state not in (1,4)"; break;//已过期
                    case 3: strSql += $" and p.state=-1"; break;//已失效
                    case 4: strSql += $" and l.EndUseTime>'{nowtime}' and l.StartUseTime<='{nowtime}' and l.state not in (1,4) and p.state!=-1"; break;//可使用
                    case 99: strSql += $" and l.EndUseTime>'{nowtime}' and l.state not in (1,4) and p.state!=-1 and (p.GoodsIdStr is NULL or p.GoodsIdStr='')"; break; //未使用 不包含指定产品优惠券
                }
            }
            else
            {
                string goodswheresql = "";
                List<string> typeidSplit = goodsId.SplitStr(",");
                if (typeidSplit.Count > 0)
                {
                    typeidSplit = typeidSplit.Select(p => p = "FIND_IN_SET('" + p + "',p.goodsidstr)").ToList();
                    goodswheresql += string.Join(" or ", typeidSplit);
                }

                //判断指定商品是否满足优惠条件
                if (!string.IsNullOrEmpty(goodsinfo))
                {
                    goodswheresql = "";
                    List<string> goodinfoslist = new List<string>();
                    JArray goodArray = JArray.Parse(goodsinfo);
                    if (goodArray != null && goodArray.Count > 0)
                    {
                        goodswheresql += "(" + GetCouponGroupSql(goodArray, 0, "");
                        goodswheresql = goodswheresql.Substring(0, goodswheresql.Length - 3);
                    }
                }

                strSql += $" and l.StartUseTime<='{nowtime}' and l.EndUseTime>='{nowtime}' and l.state not in (1,4) and p.state!=-1 and ( {goodswheresql} or p.goodsidstr='' or p.goodsidstr is null)";
            }

            strSql += $" order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, strSql))
            {
                while (dr.Read())
                {
                    var model = GetModel(dr);
                    int couponway = Convert.ToInt32(dr["couponway"]);
                    int money = Convert.ToInt32(dr["money"]);
                    int limitmoney = Convert.ToInt32(dr["limitmoney"]);
                    model.LimitMoney = limitmoney;
                    model.Money = money.ToString();
                    model.CouponWay = couponway;
                    model.GoodsIdStr = dr["GoodsIdStr"].ToString();
                    model.ValType = dr.GetInt32("valtype");
                    model.discountType = dr.GetInt32("discountType");
                    if(dr["storeid"] !=DBNull.Value)
                    {
                        model.StoreId = Convert.ToInt32(dr["storeid"]);
                    }
                    //判断优惠方式，0：指定面值，1：折扣
                    if (couponway == 0)
                    {
                        model.Money_fmt = (money / 100.00).ToString();
                    }
                    else
                    {
                        model.Money_fmt = (money / 100.00).ToString();
                    }
                    //判断是否可用
                    if (model.State == 0)
                    {
                        if (DateTime.Now < DateTime.Parse($"{model.EndUseTimeStr} 23:59:59") && DateTime.Now > DateTime.Parse($"{model.StartUseTimeStr} 00:00:00"))
                        {
                            model.CanUse = true;
                        }
                    }

                    list.Add(model);
                }
            }
            return list;
        }

        public string GetCouponGroupSql(JArray goodArray, int totalprice, string sqlwhere)
        {
            string sqlwheretemp = "";
            for (int i = 0; i < goodArray.Count; i++)
            {
                string temp1 = "";
                int total2 = 0;
                JObject oitem = (JObject)goodArray[i];
                string goodid = oitem["goodid"].ToString();
                total2 += Convert.ToInt32(oitem["totalprice"].ToString());

                JArray goodarraytemp = new JArray();
                for (int j = i + 1; j < goodArray.Count; j++)
                {
                    goodarraytemp.Add((JObject)goodArray[j]);
                }

                if (goodArray.Count - 1 < i + 1)
                {
                    temp1 += sqlwhere + $" FIND_IN_SET('{goodid}',p.goodsidstr) and LimitMoney<={totalprice + total2})or(" + sqlwheretemp;
                    return temp1;
                }

                sqlwheretemp = sqlwhere + $" FIND_IN_SET('{goodid}',p.goodsidstr) and LimitMoney<={totalprice + total2})or(" + sqlwheretemp;
                temp1 += sqlwhere + $" FIND_IN_SET('{goodid}',p.goodsidstr) and ";
                sqlwheretemp = GetCouponGroupSql(goodarraytemp, total2 + totalprice, temp1) + sqlwheretemp;

            }

            return sqlwheretemp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="couponsstr">产品id+优惠券id,逗号隔开,例如：
        /// [{GoodsIdStr:1,Id:1},{GoodsIdStr:2,Id:2}]，命名搬用优惠券表Coupons，不想重新建实体类</param>
        /// <param name="goodid"></param>
        /// <param name="goodprice"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int GetCouponPrice2(List<Coupons> couponlist, int goodid, int goodprice, ref string msg, ref List<CouponLog> tempcouponloglist)
        {
            int couponsum = 0;

            #region 使用优惠券

            //判断是否有重复使用优惠券
            var templog = couponlist.GroupBy(g => g.Id)?.Where(w => w.Count() > 1).ToList();
            if (templog?.Count() > 0)
            {
                msg = "优惠券不能重复使用！";
                return -1;
            }

            string nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string couponsidstr = string.Join(",", couponlist.Select(s => s.Id));
            if (tempcouponloglist?.Count <= 0)
            {
                tempcouponloglist = GetList($"id in ({couponsidstr}) and state!=1 and startusetime<='{nowtime}' and endusetime>='{nowtime}'");
            }

            if (tempcouponloglist?.Count <= 0)
            {
                msg = "您的优惠券已使用过，请刷新重试！";
                return -1;
            }

            couponsidstr = string.Join(",", tempcouponloglist.Select(s => s.CouponId).Distinct());
            var tempcoupons = CouponsBLL.SingleModel.GetList($"id in ({couponsidstr})");
            if (tempcoupons?.Count <= 0)
            {
                msg = "没找到优惠券，请刷新重试！";
                return -1;
            }

            var tempgoodcoupon = couponlist.Where(w => w.GoodsIdStr == goodid.ToString()).FirstOrDefault();
            if (tempgoodcoupon != null)
            {
                var tempgoodcouponlog = tempcouponloglist.Where(w => w.Id == tempgoodcoupon.Id).FirstOrDefault();
                if (tempgoodcouponlog != null)
                {
                    var couponmodel = tempcoupons.Where(w => w.Id == tempgoodcouponlog.CouponId).FirstOrDefault();
                    if (couponmodel != null)
                    {
                        //优惠指定金额
                        if (couponmodel.CouponWay == 0)
                        {
                            couponsum = couponmodel.Money;
                        }
                        else
                        {
                            couponsum = goodprice - Convert.ToInt32(goodprice * (couponmodel.Money / 100.00));
                        }
                    }
                }
            }
            #endregion

            return couponsum;
        }
        

        /// <summary>
        /// 下单使用优惠券减免金额
        /// </summary>
        /// <param name="couponlogid">用户优惠券记录ID</param>
        /// <param name="endcardlist"></param>
        /// <param name="msg"></param>
        /// <returns>减免的优惠金额</returns>
        public int GetCouponPrice(int couponlogid, List<EntGoodsCart> endcardlist, ref string msg)
        {
            if (couponlogid <= 0)
                return 0;
            if (endcardlist?.Count <= 0)
            {
                msg = "没有商品可下单！";
                return 0;
            }
            string nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            int couponsum = 0;
            var couponlogmodel = GetModel($"id={couponlogid} and state=0 and startusetime<='{nowtime}' and endusetime>='{nowtime}'");
            if (couponlogmodel != null)
            {
                var couponmodel = CouponsBLL.SingleModel.GetModel($"id={couponlogmodel.CouponId} and state!=-1");
                if (couponmodel != null)
                {
                    string tempGoodsIdStr = "," + couponmodel.GoodsIdStr + ",";
                    foreach (var item in endcardlist)
                    {
                        if (string.IsNullOrEmpty(couponmodel.GoodsIdStr) || tempGoodsIdStr.Contains(item.FoodGoodsId.ToString()))
                        {
                            //能优惠的商品价格
                            couponsum += item.Price * item.Count;
                        }
                    }

                    //判断是否有可以优惠的商品总价
                    if (couponsum > 0)
                    {
                        //判断优惠方式
                        if (couponmodel.CouponWay == 1)
                        {
                            //折扣优惠
                            couponsum = couponsum - Convert.ToInt32(couponsum * (couponmodel.Money / 1000.00));
                        }
                        else
                        {
                            //指定金额优惠，判断优惠金额是否大于商品价格，如果优惠金额大于总商品价格，则实际优惠价为总商品价格
                            couponsum = couponsum > couponmodel.Money ? couponmodel.Money : couponsum;
                        }
                    }
                }
                else
                {
                    msg = "优惠券不可用！";
                }
            }
            else
            {
                msg = "用户优惠券不可用！";
            }

            return couponsum;
        }

        public int GetCouponPrice<T>(int couponlogid, List<T> cardlist,string gname,string pname,string cname, ref string msg)
        {
            if (couponlogid <= 0)
                return 0;
            if (cardlist?.Count <= 0)
            {
                msg = "没有商品可下单！";
                return 0;
            }
            string nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            int couponsum = 0;
            var couponlogmodel = GetModel($"id={couponlogid} and state=0 and startusetime<='{nowtime}' and endusetime>='{nowtime}'");
            if (couponlogmodel != null)
            {
                var couponmodel = CouponsBLL.SingleModel.GetModel($"id={couponlogmodel.CouponId} and state!=-1");
                if (couponmodel != null)
                {
                    string tempGoodsIdStr = "," + couponmodel.GoodsIdStr + ",";
                    foreach(T item in cardlist)
                    {
                        string goodsid = item.GetType().GetProperty(gname).GetValue(item).ToString();
                        if (string.IsNullOrEmpty(couponmodel.GoodsIdStr) || tempGoodsIdStr.Contains(goodsid))
                        {
                            int price = Convert.ToInt32(item.GetType().GetProperty(pname).GetValue(item));
                            int count = Convert.ToInt32(item.GetType().GetProperty(cname).GetValue(item));
                            //能优惠的商品价格
                            couponsum += price * count;
                        }
                    }

                    //判断是否有可以优惠的商品总价
                    if (couponsum > 0)
                    {
                        //判断优惠方式
                        if (couponmodel.CouponWay == 1)
                        {
                            //折扣优惠
                            couponsum = couponsum - Convert.ToInt32(couponsum * (couponmodel.Money / 1000.00));
                        }
                        else
                        {
                            //指定金额优惠，判断优惠金额是否大于商品价格，如果优惠金额大于总商品价格，则实际优惠价为总商品价格
                            couponsum = couponsum > couponmodel.Money ? couponmodel.Money : couponsum;
                        }
                    }
                }
                else
                {
                    msg = "优惠券不可用！";
                }
            }
            else
            {
                msg = "用户优惠券不可用！";
            }

            return couponsum;
        }

        /// <summary>
        /// 下单使用优惠券减免金额
        /// </summary>
        /// <param name="couponlogid">用户优惠券记录ID</param>
        /// <param name="endcardlist"></param>
        /// <param name="msg"></param>
        /// <returns>减免的优惠金额</returns>
        public int GetCouponPrice(int couponlogid, List<FoodGoodsCart> endcardlist, ref string msg)
        {
            if (endcardlist?.Count <= 0)
            {
                msg = "没有商品可下单！";
                return 0;
            }
            string nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            int couponsum = 0;
            var couponlogmodel = GetModel($"id={couponlogid} and state=0 and startusetime<='{nowtime}' and endusetime>='{nowtime}'");
            if (couponlogmodel != null)
            {
                var couponmodel = CouponsBLL.SingleModel.GetModel($"id={couponlogmodel.CouponId} and state!=-1");
                if (couponmodel != null)
                {
                    string tempGoodsIdStr = "," + couponmodel.GoodsIdStr + ",";
                    foreach (var item in endcardlist)
                    {
                        if (string.IsNullOrEmpty(couponmodel.GoodsIdStr) || tempGoodsIdStr.Contains(item.FoodGoodsId.ToString()))
                        {
                            //能优惠的商品价格
                            couponsum += item.Price * item.Count;
                        }
                    }

                    //判断是否有可以优惠的商品总价
                    if (couponsum > 0)
                    {
                        //判断优惠方式
                        if (couponmodel.CouponWay == 1)
                        {
                            //折扣优惠
                            couponsum = couponsum - Convert.ToInt32(couponsum * (couponmodel.Money / 1000.00));
                        }
                        else
                        {
                            //指定金额优惠，判断优惠金额是否大于商品价格，如果优惠金额大于总商品价格，则实际优惠价为总商品价格
                            couponsum = couponsum > couponmodel.Money ? couponmodel.Money : couponsum;
                        }
                    }
                }
                else
                {
                    msg = "优惠券不可用！";
                }
            }
            else
            {
                msg = "用户优惠券不可用！";
            }

            return couponsum;
        }

        /// <summary>
        /// 快速支付调用优惠券计算逻辑
        /// </summary>
        /// <param name="couponlogid">用户优惠券记录ID</param>
        /// <param name="sum">支付金额</param>
        /// <param name="userId">用户ID</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int GetCouponPrice_Cuzhi(int couponlogid, int sum, int userId, ref string msg, ref CouponLog couponlogmodel)
        {
            string nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            int couponsum = sum;
            couponlogmodel = GetModel($"id={couponlogid} and state=0 and startusetime<='{nowtime}' and endusetime>='{nowtime}'");
            if (couponlogmodel != null)
            {
                var couponmodel = CouponsBLL.SingleModel.GetModel($"id={couponlogmodel.CouponId} and state!=-1");
                if (couponmodel != null)
                {
                    //消费金额限制
                    if (couponmodel.LimitMoney > sum)
                    {
                        msg = "未满" + (couponmodel.LimitMoney / 100) + "元，不可用此优惠券！";
                        return 0;
                    }

                    //会员等级限制
                    if (couponmodel.VipLevel > 0)
                    {
                        var viprelationmodel = VipRelationBLL.SingleModel.GetModel($"uid={userId}");
                        if (viprelationmodel != null)
                        {
                            var viplevelmodel = VipLevelBLL.SingleModel.GetModel($"id={viprelationmodel.levelid}");
                            if (viplevelmodel != null && (viplevelmodel.level < couponmodel.VipLevel || (viplevelmodel.level == 0 && couponmodel.VipLevel == 1)))
                            {
                                msg = "优惠券不可用，你的会员等级未达到" + couponmodel.VipLevel + "级！";
                                return 0;
                            }
                        }
                    }

                    //判断优惠方式
                    if (couponmodel.CouponWay == 1)
                    {
                        //折扣优惠
                        couponsum = Convert.ToInt32(sum * (couponmodel.Money/1000f));
                    }
                    else
                    {
                        //指定金额优惠，如果支付金额比优惠面值小，则返回0
                        couponsum = sum > couponmodel.Money ? sum - couponmodel.Money : 0;
                    }
                }
                else
                {
                    msg = "优惠券不可用！";
                }
            }
            else
            {
                msg = "用户优惠券不可用！";
            }

            return couponsum;
        }

        /// <summary>
        /// 获取优惠券减免金额文案信息：（代金券，100元)：-90元
        /// </summary>
        /// <param name="userCoupon"></param>
        /// <returns></returns>
        public string GetCouponPriceStr(CouponLog userCoupon, int payment)
        {
            Coupons couponConf = CouponsBLL.SingleModel.GetModel(userCoupon.CouponId);
            string couponType = couponConf.CouponWay == 0 ? $"代金券，{couponConf.Money / 100f}元" : $"折扣，{couponConf.Money / 100f}折";
            return $"（{couponType})：-{GetCouponPrice(couponConf, payment) / 100f}元";
        }

        /// <summary>
        /// 获取优惠券减免金额（其它函数封装太差，零复用性，只好新增此函数）
        /// </summary>
        /// <param name="coupon"></param>
        /// <returns></returns>
        public int GetCouponPrice(Coupons coupon,int payment)
        {
            //判断是否有可以优惠的商品总价
            if (payment > 0)
            {
                //判断优惠方式
                if (coupon.CouponWay == 1)
                {
                    //折扣优惠
                    payment = payment - Convert.ToInt32(payment * (coupon.Money / 1000.00));
                }
                else
                {
                    //指定金额优惠，判断优惠金额是否大于商品价格，如果优惠金额大于总商品价格，则实际优惠价为总商品价格
                    payment = payment > coupon.Money ? coupon.Money : payment;
                }
            }
            return payment;
        }
    }
}