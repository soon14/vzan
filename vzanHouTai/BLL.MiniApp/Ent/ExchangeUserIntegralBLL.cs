using DAL.Base;
using Entity.MiniApp.Ent;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Ent
{
    public class ExchangeUserIntegralBLL : BaseMySql<ExchangeUserIntegral>
    {
        #region 单例模式
        private static ExchangeUserIntegralBLL _singleModel;
        private static readonly object SynObject = new object();

        private ExchangeUserIntegralBLL()
        {

        }

        public static ExchangeUserIntegralBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ExchangeUserIntegralBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        
        




        
        
        /// <summary>
        /// 根据userId appId 更新用户在对应小程序里消费获赠的积分
        /// 直接到店使用储值支付消费 线上没有显示的商品 消费则传入userId appId price 否则线上买单的传入userId appId orderId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <param name="price">直接到店使用储值支付消费的金额</param>
        /// <param name="orderId">线上买单产生的订单Id EntGoodsOrder 里的Id</param> 
        /// <returns></returns>
        public bool AddUserIntegral(int userId, int appId, int price = 0, int orderId = 0,int fromType=0)
        {
            bool result = false;
            int curSumIntegral = 0;//本次消费获赠总积分
            int curIntegral = 0;//本条规则消费获赠积分
            List<ExchangeRule> listRule = new List<ExchangeRule>();
            var TranModel = new TransactionModel();
            List<string> listSql = new List<string>();
            try
            {
                if (orderId == 0)
                {
                    int ordertype = 3;//表示储值支付(潇哥那个储值支付)过来的(直接到店消费储余额)
                    int ruleType = 0;//表示储值支付(潇哥那个储值支付)过来的(直接到店消费储余额) 直接按照全场商品积分规则计算
                    if (fromType == 1)
                    {
                        ruleType = 2;//表示充钱储值过来的
                        ordertype = 2;
                    }
                    
                    listRule = ExchangeRuleBLL.SingleModel.GetList($"appId={appId} and state=0 and ruleType={ruleType}");
                    if (listRule != null && listRule.Count > 0)
                    {
                        foreach (var rule in listRule)//遍历全场赠送积分规则
                        {
                            if (price >= rule.price)//消费金额大于设置的金额才进行积分是否赠送判断
                            {
                                curIntegral = (price / rule.price) * rule.integral;
                                curSumIntegral += curIntegral;
                                #region 积分变动日志记录
                                ExchangeUserIntegralLog integralLog = ExchangeUserIntegralLogBLL.SingleModel.GetAddUserIntegralLog(rule, userId, orderId, "0", curIntegral,ordertype, price);
                                if (integralLog.Id == 0)
                                {
                                    listSql.Add(ExchangeUserIntegralLogBLL.SingleModel.BuildAddSql(integralLog));
                                }

                                #endregion
                            }
                        }

                    }
                }
                else
                {
                    var entOrder = EntGoodsOrderBLL.SingleModel.GetModel(orderId);
                    if (entOrder != null)
                    {
                        //  bool isAddFreightPrice = false;
                        //表示有订单
                        listRule = ExchangeRuleBLL.SingleModel.GetList($"appId={appId} and state=0");
                        if (listRule != null && listRule.Count > 0)
                        {
                            foreach (var rule in listRule)//遍历积分规则
                            {
                                if (rule.ruleType == 1)
                                {//部分商品赠送积分
                                    int curTotalPrice = 0;
                                    curTotalPrice += entOrder.FreightPrice;
                                    List<string> listusegoodis = new List<string>();
                                    //部分商品获赠积分
                                    //通过订单获取购物车然后取得订单的商品Id

                                    var cartModelList = EntGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {orderId} ");
                                    foreach (var item in cartModelList)
                                    {

                                        if (rule.goodids.Contains(item.FoodGoodsId.ToString()))
                                        {
                                            //表示当前订单所包含的商品属于赠送积分商品内,计算出这些商品的价钱
                                            curTotalPrice += item.Price * item.Count;
                                            listusegoodis.Add(item.FoodGoodsId.ToString());
                                        }
                                    }

                                    if (curTotalPrice >= rule.price)
                                    {
                                        curIntegral = (curTotalPrice / rule.price) * rule.integral;
                                        curSumIntegral += curIntegral;
                                        #region 记录日志
                                        ExchangeUserIntegralLog integralLog = ExchangeUserIntegralLogBLL.SingleModel.GetAddUserIntegralLog(rule, userId, orderId, string.Join(",", listusegoodis), curIntegral,0, curTotalPrice);
                                        if (integralLog.Id == 0)
                                        {
                                            listSql.Add(ExchangeUserIntegralLogBLL.SingleModel.BuildAddSql(integralLog));
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        log4net.LogHelper.WriteInfo(this.GetType(), $"订单Id={orderId}全场商品赠送积分记录日志,【ruleprice={rule.price}】【BuyPrice={entOrder.BuyPrice}】curIntegral={curSumIntegral}");
                                    }
                                }
                                else if(rule.ruleType==0)
                                {
                                    //全场商品送积分规则
                                    if (entOrder.BuyPrice >= rule.price)
                                    {
                                        curIntegral = (entOrder.BuyPrice / rule.price) * rule.integral;
                                        curSumIntegral += curIntegral;
                                        #region 记录日志
                                        ExchangeUserIntegralLog integralLog = ExchangeUserIntegralLogBLL.SingleModel.GetAddUserIntegralLog(rule, userId, orderId, rule.goodids, curIntegral,0, entOrder.BuyPrice);
                                        if (integralLog.Id == 0)
                                        {
                                            listSql.Add(ExchangeUserIntegralLogBLL.SingleModel.BuildAddSql(integralLog));
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        log4net.LogHelper.WriteInfo(this.GetType(), $"订单Id={orderId}全场商品赠送积分记录日志,【ruleprice={rule.price}】【BuyPrice={entOrder.BuyPrice}】curIntegral={curSumIntegral}");
                                    }
                                }
                            }
                        }
                       
                    }
                   
                }
                
                ExchangeUserIntegral exchangeUserIntegral = GetModel($"userId={userId}");
                if (exchangeUserIntegral != null)
                {
                    exchangeUserIntegral.integral = exchangeUserIntegral.integral + curSumIntegral;
                    exchangeUserIntegral.UpdateDate = DateTime.Now;

                    listSql.Add(this.BuildUpdateSql(exchangeUserIntegral, "integral,UpdateDate"));
                }
                else
                {
                    //表示新增
                    exchangeUserIntegral = new ExchangeUserIntegral { UserId = userId, integral = curSumIntegral, AddTime = DateTime.Now, UpdateDate = DateTime.Now };
                    listSql.Add(BuildAddSql(exchangeUserIntegral));
                }

                //执行事务 积分变动日志与积分表都执行成功才正确
                if (listSql.Count > 0)
                {
                    TranModel.Add(listSql.ToArray());
                    result = ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray);
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "积分处理异常："+ex.Message);
            }
            return result;
        }





        /// <summary>
        /// 用于普通商品退款时候扣除积分 原路扣除(好像不会用到,因为确认收货后才赠送积分表示交易完成 就不能退款了)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool SubUserIntegral(int userId, int appId, int orderId)
        {
            bool result = false;
            var TranModel = new TransactionModel();
            List<string> listSql = new List<string>();
            List<ExchangeUserIntegralLog> listUserIntegralLog = ExchangeUserIntegralLogBLL.SingleModel.GetList($"appId={appId} and userId={userId} and orderId={orderId}");
            int curSumIntegral = 0;
            try
            {
                foreach (var item in listUserIntegralLog)
                {
                    //插入日志 扣除积分日志 原路扣除
                    item.actiontype = -1;
                    item.Id = 0;
                    item.AddTime = DateTime.Now;
                    item.UpdateDate = DateTime.Now;
                    listSql.Add(ExchangeUserIntegralLogBLL.SingleModel.BuildAddSql(item));
                    curSumIntegral += item.curintegral;
                }

                ExchangeUserIntegral exchangeUserIntegral = GetModel($"userId={userId}");
                if (exchangeUserIntegral != null)
                {  //更新当前用户积分

                    exchangeUserIntegral.integral = exchangeUserIntegral.integral - curSumIntegral;
                    if (exchangeUserIntegral.integral < 0)//如果当前积分小于0则返回false表示异常 结束退款操作
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), "出错了,积分赠送与扣除对不上" + orderId);
                        return false;
                    }

                    exchangeUserIntegral.UpdateDate = DateTime.Now;
                    listSql.Add(this.BuildUpdateSql(exchangeUserIntegral, "integral,UpdateDate"));
                }
                if (listSql.Count > 0)
                {

                    TranModel.Add(listSql.ToArray());
                    result = ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray);
                }

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }


            return result;
        }


        /// <summary>
        /// 积分兑换商品 只有返回为2才表示成功
        /// </summary>
        /// <param name="exchangeActivity">兑换商品</param>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <param name="address">收货地址</param>
        /// <returns>0→积分不足,1→生成订单失败,2→成功,3→系统异常</returns>
        public int SubUserIntegral(ExchangeActivity exchangeActivity, int userId, int appId, string address,int way=0)
        {
            //.先生成订单,生成成功后拿到 订单Id 
            //1.根据订单Id更新订单状态 2.扣除积分以及3.插入积分记录日志 放在事务里执行
            ExchangeUserIntegral exchangeUserIntegral = GetModel($"userId={userId}");
            if (exchangeUserIntegral == null || exchangeUserIntegral.integral < exchangeActivity.integral)
                return -1;//积分不足(请先进行消费获得积分)！

            ExchangeActivityOrder exchangeActivityOrder = new ExchangeActivityOrder
            {
                appId = appId,
                UserId = userId,
                ActivityId = exchangeActivity.id,
                integral = exchangeActivity.integral,
                PayWay = 0,
                BuyCount = 1,
                address = address,
                state = 0,
                AddTime = DateTime.Now,
                activityImg=exchangeActivity.activityimg,
                activityName=exchangeActivity.activityname,
                originalPrice=exchangeActivity.originalPrice,
                Way=way
            };

            int orderId = Convert.ToInt32(ExchangeActivityOrderBLL.SingleModel.Add(exchangeActivityOrder));
            if (orderId <= 0)
                return -2;//支付失败(生成订单失败)

            var TranModel = new TransactionModel();
            List<string> listSql = new List<string>();

            //减积分商品库存
            exchangeActivity.stock--;
            listSql.Add(ExchangeActivityBLL.SingleModel.BuildUpdateSql(exchangeActivity, "stock"));


            #region 生成积分兑换商品订单操作
            //对外订单号规则：年月日时分 + 商品ID最后3位数字
            var idStr = exchangeActivity.id.ToString();
            if (idStr.Length >= 3)
            {
                idStr = idStr.Substring(idStr.Length - 3, 3);
            }
            else
            {
                idStr.PadLeft(3, '0');
            }
            idStr = $"{DateTime.Now.ToString("yyyyMMddHHmm")}{idStr}";

            exchangeActivityOrder.Id = orderId;
            exchangeActivityOrder.OrderNum = idStr;

            exchangeActivityOrder.state = 2;
            exchangeActivityOrder.PayTime = DateTime.Now;
            listSql.Add(ExchangeActivityOrderBLL.SingleModel.BuildUpdateSql(exchangeActivityOrder, "state,PayTime,OrderNum"));


            #endregion

            #region 扣除用户积分操作
            exchangeUserIntegral.integral = exchangeUserIntegral.integral - exchangeActivity.integral;
            exchangeUserIntegral.UpdateDate = DateTime.Now;
            listSql.Add(BuildUpdateSql(exchangeUserIntegral, "integral,UpdateDate"));
            #endregion


            #region 插入积分兑换商品后扣除积分的日志 
            ExchangeUserIntegralLog userIntegralLog = new ExchangeUserIntegralLog
            {
                ruleId = 0,
                appId = appId,
                integral = -1,
                price = 0,
                ruleType = -1,
                goodids = "-1",
                orderId = orderId,
                usegoodids = "-1",
                userId = userId,
                actiontype = -1,
                curintegral = exchangeActivity.integral,
                AddTime = DateTime.Now,
                UpdateDate = DateTime.Now,
                ordertype = 1,
                buyPrice=exchangeActivity.price
            };

            listSql.Add(ExchangeUserIntegralLogBLL.SingleModel.BuildAddSql(userIntegralLog));
            #endregion

            if (listSql.Count > 0)
            {

                TranModel.Add(listSql.ToArray());
                if (ExchangeActivityOrderBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
                    return orderId;

            }

            return -3;
        }





        /// <summary>
        /// 获取积分榜
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="aid"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public List<UserIntegralDetail> GetUserIntegralDetailList(string appId,int aid, out int totalCount, int pageIndex = 1, int pageSize = 10, string nickName = "")
        {
            totalCount = 0;
            string sqlWhere = $"where v.appid = '{appId}' and v.state >= 0 and u.nickname is not NULL and u.headimgurl is not NULL  ";
            List<UserIntegralDetail> list = new List<UserIntegralDetail>();
            List<MySqlParameter> mysqlParameter = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(nickName))
            {
                sqlWhere += " and u.NickName like @userName";
                mysqlParameter.Add(new MySql.Data.MySqlClient.MySqlParameter("@userName", "%" + nickName + "%"));

            }
            string sql = $"select u.id as userid,u.nickname,u.headimgurl, u.Remark,e.integral from viprelation v left join c_userinfo u on v.uid=u.id left join ExchangeUserIntegral e on e.userId=u.Id { sqlWhere}  order by e.integral desc limit {pageSize * (pageIndex - 1)},{pageSize}";
            DataSet ds = SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql, mysqlParameter.ToArray());
            if (ds.Tables == null || ds.Tables.Count <= 0)
            {
                return list;
            }


            DataTable dt = ds.Tables[0];
            if (dt == null || dt.Rows.Count <= 0)
                return list;

            int userId = 0;
            foreach (DataRow row in dt.Rows)
            {
                userId = Convert.ToInt32(row["userid"]);
                UserIntegralDetail userIntegralDetail = new UserIntegralDetail();
                userIntegralDetail.UserId = userId;

                userIntegralDetail.NickName = row["NickName"] != DBNull.Value ? row["NickName"].ToString() : string.Empty;
                if(row["Remark"] != DBNull.Value)
                {
                    userIntegralDetail.NickName +=!string.IsNullOrEmpty(row["Remark"].ToString()) ? "(" + row["Remark"].ToString() + ")" : string.Empty;
                }
              
                userIntegralDetail.Avatar = row["HeadImgUrl"] != DBNull.Value ? row["HeadImgUrl"].ToString() : string.Empty;
                userIntegralDetail.TotalPoints = row["integral"] != DBNull.Value ? Convert.ToInt32(row["integral"]) : 0;
                userIntegralDetail.PlayCardNum = GetPlayCardNumByUserId(userId);
                userIntegralDetail.FromConsumPoints = GetUserIntegralByType(0, userId);
                userIntegralDetail.FromSavmeMoneyPoints = GetUserIntegralByType(2, userId);
                userIntegralDetail.CostTotalPoints = GetUserIntegralByType(1, userId);
                userIntegralDetail.FromPlayCardPoints = GetUserIntegralByType(3, userId);
                list.Add(userIntegralDetail);

            }


            sql = $"select count(u.id) from viprelation v left join c_userinfo u on v.uid=u.id { sqlWhere}";
            var obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql, mysqlParameter.ToArray());
            if (obj != DBNull.Value)
            {
                totalCount = Convert.ToInt32(obj);
            }
            return list;



        }



        /// <summary>
        /// 根据用户Id以及积分日志类型获取不同类型的积分数据
        /// </summary>
        /// <param name="type">0→购买普通商品赠送积分  1→兑换商品消耗积分 2→充值赠送积分 3→签到赠送积分</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetUserIntegralByType(int type,int userId)
        {
            int points = 0;
           string sql = $"SELECT SUM(curintegral) from ExchangeUserIntegralLog where ordertype={type} and userId={userId}";
            var obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != DBNull.Value)
            {
                points = Convert.ToInt32(obj);
            }

            return points;

        }


        /// <summary>
        /// 获取累计签到次数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetPlayCardNumByUserId(int userId)
        {
            int num = 0;
            string sql = sql = $"SELECT COUNT(userid) as number from exchangeuserintegrallog where userId = {userId} and ordertype = 3";

            var obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != DBNull.Value)
            {
                num = Convert.ToInt32(obj);
            }

            return num;

        }


        /// <summary>
        /// 后台手动修改用户积分信息
        /// </summary>
        /// <param name="exchangeUserIntegral"></param>
        /// <param name="aid"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public bool ChangeUserPoints(ExchangeUserIntegral exchangeUserIntegral, int aid,int points)
        {
            TransactionModel tran = new TransactionModel();

            if (points > 0)
            {
                //表示赠送
                exchangeUserIntegral.integral += points;
            }
            else
            {
                //表示减少
                if (Math.Abs(points) > exchangeUserIntegral.integral)
                {
                    //表示减少的积分大于当前现有积分,不够减少直接变为0
                    exchangeUserIntegral.integral = 0;
                }
                else
                {
                    exchangeUserIntegral.integral-= Math.Abs(points);
                }
               

            }
            exchangeUserIntegral.UpdateDate = DateTime.Now;
            tran.Add(base.BuildUpdateSql(exchangeUserIntegral, "integral,UpdateDate"));
            #region 插入积分变动日志 
            ExchangeUserIntegralLog userIntegralLog = new ExchangeUserIntegralLog
            {
                ruleId = 0,
                appId = aid,
                integral = points>0?points:-1,//这里表示赠送的积分 -1 表示扣除
                price = 0,
                ruleType = -1,
                goodids = "-1",
                orderId = -1,
                usegoodids = "-1",
                userId = exchangeUserIntegral.UserId,
                actiontype = points > 0 ? 0 : -1,
                curintegral = Math.Abs(points),
                AddTime = DateTime.Now,
                UpdateDate = DateTime.Now,
                ordertype =points>0?4:5,
                buyPrice =0
            };

            tran.Add(ExchangeUserIntegralLogBLL.SingleModel.BuildAddSql(userIntegralLog));
            #endregion

            return base.ExecuteTransactionDataCorect(tran.sqlArray);

        }








    }
}
