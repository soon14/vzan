using BLL.MiniApp.Conf;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Utility;

namespace BLL.MiniApp.Stores
{
    public class StoreGoodsOrderBLL : BaseMySql<StoreGoodsOrder>
    {
        #region 单例模式
        private static StoreGoodsOrderBLL _singleModel;
        private static readonly object SynObject = new object();

        private StoreGoodsOrderBLL()
        {

        }

        public static StoreGoodsOrderBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new StoreGoodsOrderBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 获取订单号
        /// </summary>
        /// <param name="goodsOrder"></param>
        /// <returns></returns>
        public string GetGoodsOrderNo(StoreGoodsOrder goodsOrder)
        {
            return goodsOrder == null ? "" : goodsOrder.CreateDate.ToString("yyyy MMdd ") +
                   goodsOrder.Id.ToString().PadLeft(8, '0').Insert(4, " ");
        }

        //// <summary>
        /// 查购买者的订单
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="state">订单状态</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<StoreGoodsOrder> GetUserToOrder(int userId, int state, int pageIndex, int pageSize = 10)
        {
            string strWhere = $" ObtainUserId={userId}";

            switch (state)
            {
                case (int)OrderState.未付款:
                case (int)OrderState.待核销:
                case (int)OrderState.已核销:
                case (int)OrderState.待发货:
                case (int)OrderState.正在配送:
                case (int)OrderState.待收货:
                case (int)OrderState.已收货:
                case (int)OrderState.已退款:
                    strWhere += $" and State={state}";
                    break;
                case -10:
                    strWhere += $" and (State={(int)OrderState.待收货} or State={(int)OrderState.正在配送} or State={(int)OrderState.待核销})";
                    break;
            }
            string orderField = "CreateDate DESC";
            List<StoreGoodsOrder> list = GetList(strWhere, pageSize, pageIndex, "*", orderField);
            return list;
        }

        /// <summary>
        /// 商品订单过期-蔡华兴
        /// </summary>
        public void StoreGoodsOrderTimeOut(int timeoutlength = -30)
        {
            TransactionModel tranModel = new TransactionModel();
             
             
             
             

            //订单超过30分钟取消订单
            List<StoreGoodsOrder> orderList = GetList($"State=0 and  CreateDate < date_add(NOW(), interval {timeoutlength} MINUTE)", 100, 1);
            if (orderList != null && orderList.Count > 0)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "小程序商品订单过期（订单超过30分钟取消订单）");
                tranModel = new TransactionModel();
                //订单明细
                List<StoreGoodsCart> orderdetaillist = orderList.Any() ? StoreGoodsCartBLL.SingleModel.GetList($"State =1 and GoodsOrderId in ({string.Join(",", orderList.Select(s => s.Id).Distinct())})") : new List<StoreGoodsCart>();
                if (orderdetaillist != null && orderdetaillist.Count > 0)
                {
                    //商品
                    List<StoreGoods> goodlist = StoreGoodsBLL.SingleModel.GetList($"Id in ({string.Join(",", orderdetaillist.Select(s => s.GoodsId).Distinct())})");
                    if (goodlist != null && goodlist.Count > 0)
                    {
                        foreach (StoreGoodsOrder item in orderList)
                        {
                            //商品明细
                            List<StoreGoodsCart> orderdetails = orderdetaillist.Any() ? orderdetaillist.Where(w => w.GoodsOrderId == item.OrderId).ToList() : new List<StoreGoodsCart>();
                            if (orderdetails != null && orderdetails.Count > 0)
                            {
                                for (int i = 0; i < orderdetails.Count; i++)
                                {
                                    //商品
                                    StoreGoods good = goodlist.Where(w => w.Id == orderdetails[i].GoodsId).FirstOrDefault();
                                    if (good != null)
                                    {
                                        //商品加总库存
                                        good.Stock += orderdetails[i].Count;
                                        List<StoreGoodsAttrDetail> GASDetailList = good.GASDetailList;
                                        //订单明细中的规格属性，加规格属性库存
                                        for (int j = 0; j < GASDetailList.Count; j++)
                                        {
                                            if (GASDetailList[j].id == orderdetails[i].SpecIds)
                                            {
                                                GASDetailList[j].count += orderdetails[i].Count;
                                                good.AttrDetail =  SerializeHelper.SerToJson(GASDetailList);
                                                break;
                                            }
                                        }
                                        //更改商品总库存和规格属性库存
                                        tranModel.Add($"update StoreGoods set AttrDetail='{good.AttrDetail}',Inventory={good.Inventory} where Id={good.Id}");
                                    }
                                }
                            }

                            //订单状态改成已过期
                            item.State = -1;
                            tranModel.Add($"update StoreGoodsOrder set State={item.State} where Id={item.Id} and State = 0");

                            //加订单操作日志
                            StoreGoodsOrderLog model = new StoreGoodsOrderLog();
                            model.UserId = 0;
                            model.GoodsOrderId = item.Id;
                            model.LogInfo = " 30分钟未付款,系统自动改为已过期！ ";
                            model.CreateDate = DateTime.Now;
                            tranModel.Add(StoreGoodsOrderLogBLL.SingleModel.BuildAddSql(model));

                            ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 方法操作：1.创建订单,2.将购物车对应内容转为订单内容,3.减库存
        /// </summary>
        /// <param name="order"></param>
        /// <param name="goodsCar"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public bool addGoodsOrder(StoreGoodsOrder order, List<StoreGoodsCart> goodsCar, C_UserInfo userInfo, ref string msg)
        {
            TransactionModel TranModel = new TransactionModel();

            //创建订单
            TranModel.Add(BuildAddSql(order));

            //定义临时变量接收订单Id
            //var dbOrderId = "@dbOrderId";
            //TranModel.Add($" select {dbOrderId} := last_insert_id(); ");

            //将购物车记录转为订单明细记录
            TranModel.Add($" update StoreGoodsCart set GoodsOrderId = (select last_insert_id()),State = 1,UserId = {userInfo.Id} where id in ({string.Join(",", goodsCar.Select(s => s.Id).Distinct())}) and state = 0; ");

            //根据订单内记录数量减库存
            List<StoreGoods> goodsList = StoreGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsCar.Select(x => x.GoodsId).Distinct().ToList())}) ");

            Utility.Easyui.EasyuiHelper<StoreGoodsAttrDetail> goodDtlJsonHelper = new Utility.Easyui.EasyuiHelper<StoreGoodsAttrDetail>();
            //msg += "a";
            goodsCar.ForEach(x =>
            {
                StoreGoods good = goodsList.Where(y => y.Id == x.GoodsId).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(x.SpecIds))
                {

                    good.Stock -= x.Count;
                }
                else
                {
                    good.Stock -= x.Count;
                    //var goodList = good.GASDetailList.Where(z => z.id.Equals(x.SpecIds));
                    List<StoreGoodsAttrDetail> GASDetailList = new List<StoreGoodsAttrDetail>();
                    good.GASDetailList.ForEach(y =>
                    {
                        if (y.id.Equals(x.SpecIds))
                        {
                            y.count -= x.Count;
                        }
                        GASDetailList.Add(y);
                    });
                    //规格库存详情重新赋值
                    good.AttrDetail = goodDtlJsonHelper.SToJsonArray(GASDetailList);
                }
            });
            // msg += "b";
            
            goodsList.ForEach(x =>
            {
                TranModel.Add($" update storegoods set Stock={x.Stock},AttrDetail='{x.AttrDetail}' where Id = {x.Id} ");
            });

            
            string stringSql = "";
            for (int i = 0; i < TranModel.sqlArray.Length; i++)
            {
                stringSql += TranModel.sqlArray[i];
            }
            return ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray);
        }

        /// <summary>
        /// 方法操作：1.创建订单,2.将购物车对应内容转为订单内容,3.减库存
        /// </summary>
        /// <param name="order"></param>
        /// <param name="goodsCar"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public bool addGoodsOrder(StoreGoodsOrder order, List<StoreGoodsCart> goodsCar, C_UserInfo userInfo, StringBuilder sb, ref string msg)
        {

            TransactionModel TranModel = new TransactionModel();

            //更新会员打折后的购物车
            if (sb != null)
            {
                TranModel.Add(sb.ToString());
            }
            //创建订单
            TranModel.Add(BuildAddSql(order));

            //将购物车记录转为订单明细记录
            TranModel.Add($" update StoreGoodsCart set GoodsOrderId = (select last_insert_id()),State = 1,UserId = {userInfo.Id} where id in ({string.Join(",", goodsCar.Select(s => s.Id).Distinct())}) and state = 0; ");

            //根据订单内记录数量减库存
            List<StoreGoods> goodsList = StoreGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsCar.Select(x => x.GoodsId).Distinct().ToList())}) ");
            Utility.Easyui.EasyuiHelper<StoreGoodsAttrDetail> goodDtlJsonHelper = new Utility.Easyui.EasyuiHelper<StoreGoodsAttrDetail>();
            goodsCar.ForEach(x =>
            {
                StoreGoods good = goodsList.Where(y => y.Id == x.GoodsId).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(x.SpecIds))
                {

                    good.Stock -= x.Count;
                }
                else
                {
                    good.Stock -= x.Count;
                    List<StoreGoodsAttrDetail> GASDetailList = new List<StoreGoodsAttrDetail>();
                    good.GASDetailList.ForEach(y =>
                    {
                        if (y.id.Equals(x.SpecIds))
                        {
                            y.count -= x.Count;
                        }
                        GASDetailList.Add(y);
                    });
                    //规格库存详情重新赋值
                    good.AttrDetail = goodDtlJsonHelper.SToJsonArray(GASDetailList);
                }
            });
            
            goodsList.ForEach(x =>
            {
                StoreGoodsBLL.SingleModel.RemoveStoreGoodsCache(x.Id);
                TranModel.Add($" update storegoods set Stock={x.Stock},AttrDetail='{x.AttrDetail}' where Id = {x.Id} ");
            });
            
            return ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray);
        }

        /// <summary>
        /// 根据模板类型返回填充模板内容(餐饮) 
        /// </summary>
        /// <param name="OrderId"></param>
        /// <param name="sendMsgType"></param>
        /// <returns></returns>
        public object getTemplateMessageData(int OrderId, SendTemplateMessageTypeEnum sendMsgType)
        {
            StoreGoodsOrder model = GetModel(OrderId) ?? new StoreGoodsOrder();
            List<StoreGoodsCart> modelDtl = StoreGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {model.Id} ") ?? new List<StoreGoodsCart>();
            
            string modelDtlName = "";
            string goodsIds = string.Join(",",modelDtl.Select(s=>s.GoodsId).Distinct());
            List<StoreGoods> storeGoodsList = StoreGoodsBLL.SingleModel.GetListByIds(goodsIds);

            modelDtl.ForEach(x =>
            {
                StoreGoods good = storeGoodsList?.FirstOrDefault(f=>f.Id == x.GoodsId) ?? new StoreGoods();
                x.goodsMsg = good;
            });
            modelDtlName = string.Join("+", modelDtl.Select(x => x.goodsMsg.GoodsName));

            //店名
            string storeName = "";
            Store stroe = StoreBLL.SingleModel.GetModel(model.StoreId) ?? new Store();
            List<ConfParam> paramslist = ConfParamBLL.SingleModel.GetListByRId(stroe.appId);
            ConfParam cinfo = paramslist?.Where(w => w.Param == "nparam").FirstOrDefault();
            if (cinfo != null)
            {
                storeName = cinfo.Value;
            }

            object postData = new object();
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.电商订单支付成功通知:
                    postData = new
                    {
                        keyword1 = new { value = model.OrderNum, color = "#000000" },
                        keyword2 = new { value = model.CreateDateStr, color = "#000000" },
                        keyword3 = new { value = modelDtlName, color = "#000000" },
                        keyword4 = new { value = model.PayDateStr, color = "#000000" },
                        keyword5 = new { value = Enum.GetName(typeof(miniAppBuyMode), model.buyMode), color = "#000000" },
                        keyword6 = new { value = model.BuyPriceStr, color = "#000000" },
                        keyword7 = new { value = Enum.GetName(typeof(OrderState), model.State), color = "#000000" },
                    };
                    break;
                case SendTemplateMessageTypeEnum.电商订单配送通知:
                    postData = new
                    {
                        keyword1 = new { value = model.CreateDateStr, color = "#000000" },
                        keyword2 = new { value = storeName, color = "#000000" },
                        keyword3 = new { value = model.OrderNum, color = "#000000" },
                        keyword4 = new { value = model.Address, color = "#000000" },
                        keyword5 = new { value = model.DistributeDateStr, color = "#000000" },
                        keyword6 = new { value = modelDtlName, color = "#000000" },
                        keyword7 = new { value = Enum.GetName(typeof(OrderState), model.State), color = "#000000" },
                    };
                    break;
            }
            return postData;
        }


        //自动完成订单
        public void updateOrderStateForComplete(int timeoutlength = -(60 * 24 * 10))
        {
            TransactionModel tranModel = new TransactionModel();
            
            

            List<int> updateByOldStateList = new List<int>();
            updateByOldStateList.Add((int)OrderState.待收货);

            List< StoreGoodsOrder> list = GetList($" State in ({string.Join(", ", updateByOldStateList)}) and  DistributeDate <= (NOW()+INTERVAL {timeoutlength} MINUTE )");
            string updateSql = $" update  StoreGoodsOrder set State = {(int)OrderState.已收货},AcceptDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where  State in ({string.Join(",", updateByOldStateList)}) and  DistributeDate <= (NOW()+interval {timeoutlength} MINUTE) ";
            SqlMySql.ExecuteNonQuery(connName, CommandType.Text, updateSql, null);
            string updateSalesCount = "";
            if(list!=null && list.Count>0)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "小程序电商 - 10天未确认收货,自动完成");
                list.ForEach(order =>
                {
                    StoreGoodsOrderLogBLL.SingleModel.AddLog(order.Id, 0, "订单超过10天未确认收货,系统自动完成订单");
                    //会员加消费金额
                    if (!VipRelationBLL.SingleModel.updatelevel(order.UserId, "store"))
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常" + order.Id));
                    }
                    //加销量
                    List<StoreGoodsCart> list2 = StoreGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {order.Id} ");
                    list2.Select(x => x.GoodsId).Distinct().ToList().ForEach(x =>
                    {
                        int salesCount1 = list2.Where(y => y.GoodsId == x).Sum(y => y.Count);

                        updateSalesCount += $" update StoreGoods set salesCount = salesCount + {salesCount1},salesCount_real = salesCount_real + {salesCount1} where id = {x} ;";
                    });
                });
            }
            
            if (!string.IsNullOrWhiteSpace(updateSalesCount))
            {
                SqlMySql.ExecuteNonQuery(connName, CommandType.Text, updateSalesCount, null);
            }
        }

    }

    public class StoreAdminGoodsOrderBLL : BaseMySql<StoreAdminGoodsOrder>
    {
        //查找订单列表
        public List<StoreAdminGoodsOrder> GetAdminList(string where, int pagesize, int pageindex, out int totalCount, bool export = false)
        {
            List<StoreAdminGoodsOrder> list = new List<StoreAdminGoodsOrder>();
            string sql;
            string sqlCount;

            sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,orders.AccepterName as NickName,orders.AccepterTelePhone as TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address from storegoodsorder orders {(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by Id desc {(pagesize == 0 ? "" : " limit " + (pageindex <= 0 ? 0 : pageindex - 1) * pagesize + "," + pagesize)}";
            //sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,user.NickName,user.TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address from miniappgoodsorder orders inner join c_userinfo user on user.Id=orders.UserId {(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by Id desc {(pagesize == 0 ? "" : " limit " + (pageindex <= 0 ? 0 : pageindex - 1) * pagesize + "," + pagesize)}";
            if (export)//导出Excel的话，不需要分页
            {
                sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,orders.AccepterName as NickName,orders.AccepterTelePhone as TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address from storegoodsorder orders {(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by Id desc";
                //sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,user.NickName,user.TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address from miniappgoodsorder orders inner join c_userinfo user on user.Id=orders.UserId {(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by Id desc";
            }
            sqlCount = $@"select count(*) from storegoodsorder orders {(string.IsNullOrEmpty(where) ? "" : " where " + where)}";

             
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    StoreAdminGoodsOrder model = GetModel(dr);
                    List<StoreGoodsCart> cartlist = StoreGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={model.Id}");
                    List<StoreOrderCardDetail> detaillist = new List<StoreOrderCardDetail>();

                    string goodsIds = string.Join(",", cartlist?.Select(s=>s.GoodsId).Distinct());
                    List<StoreGoods> storeGoodsList = StoreGoodsBLL.SingleModel.GetListByIds(goodsIds);

                    foreach (StoreGoodsCart item in cartlist)
                    {
                        StoreOrderCardDetail cart = new StoreOrderCardDetail();
                        cart.Id = item.Id;
                        StoreGoods goods = storeGoodsList?.FirstOrDefault(f=>f.Id == item.GoodsId);
                        if (goods != null)
                        {
                            cart.GoodsName = goods.GoodsName;
                            cart.ImgUrl = goods.ImgUrl;
                        }
                        cart.SpecInfo = item.SpecInfo;
                        cart.Price = item.Price;
                        cart.Count = item.Count;
                        detaillist.Add(cart);
                    }
                    model.GoodsList = detaillist;
                    list.Add(model);
                }
            }
            totalCount = GetCountBySql(sqlCount);
            return list;
        }

        //查找订单列表-电商
        public List<StoreAdminGoodsOrder> GetAdminListForStores(string where, int pagesize, int pageindex, out int totalCount, bool export = false, MySqlParameter[] param = null)
        {
            List<StoreAdminGoodsOrder> list = new List<StoreAdminGoodsOrder>();
            string sql;
            string sqlCount;

            sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,orders.AccepterName as NickName,orders.AccepterTelePhone as TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address,orders.buyMode,c.nickName as userName,l.name as levelname from storegoodsorder orders ";
            sql += $"left join c_userinfo c on c.Id = orders.userid ";
            sql += $"left join viprelation v on v.uid = orders.userid and v.state >=0 ";
            sql += $"left join viplevel l on v.levelid = l.id and l.state >= 0 ";
            sql += $"{(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by orders.Id desc {(pagesize == 0 ? "" : " limit " + (pageindex <= 0 ? 0 : pageindex - 1) * pagesize + "," + pagesize)}";

            if (export)//导出Excel的话，不需要分页
            {
                sql = $@"select g.goodsname,s.count,s.price,orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,orders.AccepterName as NickName,orders.AccepterTelePhone as TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address,orders.buyMode,c.nickName as userName,l.name as levelname from storegoodsorder orders ";
                sql += $"left join c_userinfo c on c.Id = orders.userid ";
                sql += $"left join viprelation v on v.uid = orders.userid ";
                sql += $"left join viplevel l on v.levelid = l.id ";
                sql += $"left join storegoodscart s on orders.id = s.goodsorderid and s.state = 1 ";
                sql += $"left join storegoods g on g.id = s.goodsid ";
                sql += $"{(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by orders.Id desc ";
            }
            sqlCount = $@"select count(0) from storegoodsorder orders ";
            sqlCount += $"left join c_userinfo c on c.Id = orders.userid ";
            sqlCount += $"left join viprelation v on v.uid = orders.userid ";
            sqlCount += $"left join viplevel l on v.levelid = l.id ";
            sqlCount += $"{(string.IsNullOrEmpty(where) ? "" : " where " + where)}";

            
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, param))
            {
                while (dr.Read())
                {
                    StoreAdminGoodsOrder model = GetModel(dr);
                    List<StoreGoodsCart> cartlist = StoreGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={model.Id}");
                    List<StoreOrderCardDetail> detaillist = new List<StoreOrderCardDetail>();
                    foreach (StoreGoodsCart item in cartlist)
                    {
                        StoreOrderCardDetail cart = new StoreOrderCardDetail();
                        cart.Id = item.Id;
                        StoreGoods goods = StoreGoodsBLL.SingleModel.GetModel(item.GoodsId);
                        if (goods != null)
                        {
                            cart.GoodsName = goods.GoodsName;
                            cart.ImgUrl = goods.ImgUrl;
                        }
                        cart.SpecInfo = item.SpecInfo;
                        cart.Price = item.Price;
                        cart.Count = item.Count;
                        detaillist.Add(cart);
                    }
                    model.GoodsList = detaillist;
                    list.Add(model);
                }
            }
            //totalCount = GetCountBySql(sqlCount);
            totalCount = Convert.ToInt32(SqlMySql.ExecuteScalar(connName, CommandType.Text, sqlCount, param));
            return list;
        }

        public List<StoreAdminGoodsOrder> GetAdminListForStoresv2(string where, int pagesize, int pageindex, out int totalCount, bool export = false, MySqlParameter[] param = null)
        {
            List<StoreAdminGoodsOrder> list = new List<StoreAdminGoodsOrder>();
            string sql;
            string sqlCount;

            sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,orders.AccepterName as NickName,orders.AccepterTelePhone as TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address,orders.buyMode,c.nickName as userName,l.name as levelname from storegoodsorder orders ";
            sql += $"left join c_userinfo c on c.Id = orders.userid ";
            sql += $"left join viprelation v on v.uid = orders.userid and v.state >=0 ";
            sql += $"left join viplevel l on v.levelid = l.id and l.state >= 0 ";
            sql += $"{(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by orders.Id desc {(pagesize == 0 ? "" : " limit " + (pageindex <= 0 ? 0 : pageindex - 1) * pagesize + "," + pagesize)}";

            if (export)//导出Excel的话，不需要分页
            {
                sql = $@"select g.goodsname,s.count,s.price,orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,orders.AccepterName as NickName,orders.AccepterTelePhone as TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address,orders.buyMode,c.nickName as userName,l.name as levelname from storegoodsorder orders ";
                sql += $"left join c_userinfo c on c.Id = orders.userid ";
                sql += $"left join viprelation v on v.uid = orders.userid ";
                sql += $"left join viplevel l on v.levelid = l.id ";
                sql += $"left join storegoodscart s on orders.id = s.goodsorderid and s.state = 1 ";
                sql += $"left join storegoods g on g.id = s.goodsid ";
                sql += $"{(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by orders.Id desc ";
            }
            sqlCount = $@"select count(0) from storegoodsorder orders ";
            sqlCount += $"left join c_userinfo c on c.Id = orders.userid ";
            sqlCount += $"left join viprelation v on v.uid = orders.userid ";
            sqlCount += $"left join viplevel l on v.levelid = l.id ";
            sqlCount += $"{(string.IsNullOrEmpty(where) ? "" : " where " + where)}";
            
            totalCount = Convert.ToInt32(SqlMySql.ExecuteScalar(connName, CommandType.Text, sqlCount, param));
            if (totalCount <= 0)
                return list;

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, param))
            {
                while (dr.Read())
                {
                    StoreAdminGoodsOrder model = GetModel(dr);
                    list.Add(model);
                }
            }
            if(list==null || list.Count<=0)
            {
                return list;
            }

            string orderIds = string.Join(",", list.Select(s => s.Id));
            List<StoreGoodsCart> storeGoodsCartList = StoreGoodsCartBLL.SingleModel.GetListByOrderIds(orderIds);

            string goodsIds = string.Join(",", storeGoodsCartList?.Select(s => s.GoodsId).Distinct());
            List<StoreGoods> storeGoodsList = StoreGoodsBLL.SingleModel.GetListByIds(goodsIds);

            foreach (StoreAdminGoodsOrder item in list)
            {
                List<StoreGoodsCart> cartlist = storeGoodsCartList?.Where(w => w.GoodsOrderId == item.Id).ToList();
                if (cartlist == null || cartlist.Count <= 0)
                    continue;

                List<StoreOrderCardDetail> detaillist = new List<StoreOrderCardDetail>();
                foreach (StoreGoodsCart citem in cartlist)
                {
                    StoreOrderCardDetail cart = new StoreOrderCardDetail();
                    cart.Id = item.Id;
                    StoreGoods goods = storeGoodsList?.FirstOrDefault(f => f.Id == citem.GoodsId);
                    if (goods != null)
                    {
                        cart.GoodsName = goods.GoodsName;
                        cart.ImgUrl = goods.ImgUrl;
                    }
                    cart.SpecInfo = citem.SpecInfo;
                    cart.Price = citem.Price;
                    cart.Count = citem.Count;
                    detaillist.Add(cart);
                }
                item.GoodsList = detaillist;
            }

            return list;
        }
    }
}