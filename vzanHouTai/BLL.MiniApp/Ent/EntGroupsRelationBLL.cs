using BLL.MiniApp.Fds;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Model;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Utility;

namespace BLL.MiniApp.Ent
{
    public class EntGroupsRelationBLL : BaseMySql<EntGroupsRelation>
    {
        #region 单例模式
        private static EntGroupsRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntGroupsRelationBLL()
        {

        }

        public static EntGroupsRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntGroupsRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        #region 基础操作
        /// <summary>
        /// 获取单个拼团关联表
        /// </summary>
        /// <param name="entgoodsid">产品ID</param>
        /// <param name="rid"></param>
        /// <param name="storeid">专为多门店设计，其他的为0，多门店分店为店铺ID</param>
        /// <returns></returns>
        public EntGroupsRelation GetModelByGroupGoodType(int entgoodsid, int rid, int storeid = 0,int state=0)
        {

            string sqlwhere = $"entgoodsid={entgoodsid} and rid={rid} and state >{state} and storeid={storeid}";
           
            return GetModel(sqlwhere);
        }

        /// <summary>
        /// 获取拼团关联表数据
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="entgoodsids"></param>
        /// <param name="storeid">专为多门店设计，其他的为0，多门店分店为店铺ID</param>
        /// <returns></returns>
        public List<EntGroupsRelation> GetListByGroupGoodType(int rid, string entgoodsids, int storeid = 0)
        {
            if (string.IsNullOrEmpty(entgoodsids))
            {
                return new List<EntGroupsRelation>();
            }

            return GetList($"entgoodsid in ({entgoodsids}) and rid={rid} and storeid={storeid}");
        }
        #endregion
        
        /// <summary>
        /// 后台组件获取可用拼团列表
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="name"></param>
        /// <param name="state"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public List<EntGroupsRelation> GetListGroups(int rid, int pageSize, int pageIndex,ref int count)
        {
            List<EntGroupsRelation> list = new List<EntGroupsRelation>();


            
            string sqlcount = "select count(*) ";
            string sqlinfo = "select eg.`name`,eg.price,eg.stock,eg.img,egr.*";
            string strSql = $@"from entgoods eg left join entgroupsrelation egr on eg.id = egr.entgoodsid where egr.rid={rid} and eg.goodtype = {(int)EntGoodsType.拼团产品} and eg.aid = {rid}  and egr.state =1";
            string orderslq = $" order by eg.addtime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sqlinfo+strSql+ orderslq, null))
            {
                while (dr.Read())
                {
                    var model = GetModel(dr);
                    model.Name = dr["name"].ToString();
                    model.ImgUrl = dr["img"].ToString();
                    if (DBNull.Value != dr["stock"])
                    {
                        model.CreateNum = int.Parse(dr["stock"].ToString());
                    }
                    if (DBNull.Value != dr["price"])
                    {
                        model.SinglePrice = float.Parse(dr["price"].ToString());
                    }

                    //获取团记录
                    var grouplist = EntGroupSponsorBLL.SingleModel.GetList($"EntGoodRId ={model.Id}");
                    if(grouplist!=null && grouplist.Count>0)
                    {
                        //根据团记录查找已团数量
                        var sponids = string.Join(",",grouplist.Select(s=>s.Id).Distinct());
                        var logcount = EntGoodsOrderBLL.SingleModel.GetCount($"ordertype=3 and state not in ({(int)MiniAppEntOrderState.已取消},{(int)MiniAppEntOrderState.待付款}) and groupid in ({sponids}) ");
                        model.GroupsNum = logcount;
                    }

                    list.Add(model);
                }
            }

            var result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sqlcount + strSql, null);
            
            if(result != DBNull.Value)
            {
                count = Convert.ToInt32(result);
            }
            return list;
        }
        
        /// <summary>
        /// 后台获取拼团列表
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="name"></param>
        /// <param name="state"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public List<EntGroupsRelation> GetListGroups(int pagetype,int rid,string name,int state,ref int count,int pageSize = 10,int pageIndex = 1,int storeid=0)
        {
            List<EntGroupsRelation> list = new List<EntGroupsRelation>();
            List<MySqlParameter> param = new List<MySqlParameter>();
            string strOrder = "eg.addtime desc";

            string sqlcommond = $" from entgoods eg left join entgroupsrelation egr on eg.id = egr.entgoodsid where egr.rid={rid} and eg.goodtype = {(int)EntGoodsType.拼团产品} and eg.aid = {rid} and egr.storeid={storeid} and egr.state<>-2";//state=-2表示拼团产品失效后被删除
            string strSqllist = $@"select eg.`name`,eg.price,egr.* ";
            string strsqlcount = "select count(*) ";

            if(pagetype==(int)TmpType.小程序餐饮模板)
            {
                Food food = FoodBLL.SingleModel.GetModelByAppId(rid);
                if(food==null)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(),"餐饮高级拼团查询拼团商品错误，没有找到餐饮店铺");
                }
                sqlcommond = $"from foodgoods eg left join entgroupsrelation egr on eg.id = egr.entgoodsid where egr.rid={rid} and eg.goodtype = {(int)EntGoodsType.拼团产品} and eg.foodid = {food.Id} and egr.state<>-2";
                strSqllist = $@"select eg.goodsname as name,eg.price,egr.* ";
                strOrder = "eg.createdate desc";
            }

            if (state>0)
            {
                sqlcommond += $" and egr.state = {state} ";
            }

            //拼团名称
            if (!string.IsNullOrEmpty(name))
            {
                if (pagetype == (int)TmpType.小程序餐饮模板)
                {
                    sqlcommond += " and eg.goodsname=@name ";
                    param.Add(new MySqlParameter("@name", name));
                }
                else
                {
                    sqlcommond += " and eg.name=@name ";
                    param.Add(new MySqlParameter("@name", name));
                }    
            }

            string ordersql= $" order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            //数量
            var result = SqlMySql.ExecuteScalar(connName, CommandType.Text, strsqlcount+sqlcommond, param.ToArray());
            if (DBNull.Value != result)
            {
                count= Convert.ToInt32(result);
            }

            
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, strSqllist + sqlcommond+ordersql, param.ToArray()))
            {
                while (dr.Read())
                {
                    var model = GetModel(dr);
                    model.Name = dr["name"].ToString();
                    if(DBNull.Value != dr["price"])
                    {
                        model.SinglePrice = float.Parse(dr["price"].ToString());
                    }

                    list.Add(model);
                }
            }
            return list;
        }

        /// <summary>
        /// 下单时判断是否是拼团
        /// </summary>
        /// <param name="isgroup"></param>
        /// <param name="groupid"></param>
        /// <param name="goodscar"></param>
        /// <param name="grouperprice"></param>
        /// <param name="groupmodel"></param>
        /// <returns></returns>
        public string CommandEntGroup(int isgroup, int groupid, int userid, int storeid, int goodid, ref int grouperprice, ref EntGroupsRelation groupmodel,int type,int buyCount)
        {
            if (isgroup <= 0 && groupid <= 0)
            {
                return "";
            }

            if (isgroup > 0 && groupid > 0)
            {
                return "拼团参数错误";
            }

            groupmodel = GetModelByGroupGoodType(goodid, groupmodel.RId, storeid);
            if (groupmodel == null)
            {
                return "产品匹配不到拼团信息";
            }

            #region 判断开团时，库存是否满足成团
            int stock = 0;

            //已团件数
            int groupnum = 0;
            switch(type)
            {
                case (int)TmpType.小程序餐饮模板:

                    FoodGoods entgood = FoodGoodsBLL.SingleModel.GetModel(groupmodel.EntGoodsId);
                    if (entgood == null)
                    {
                        return "拼团产品已下架，请浏览其它团商品";
                    }
                    stock = entgood.Stock;
                    groupnum = 0;
                    //判断是否已参加该团
                    if (groupid > 0)
                    {
                        List<FoodGoodsOrder> entgoodorder = FoodGoodsOrderBLL.SingleModel.GetListGroupOrder(groupid.ToString(), userid);
                        if (entgoodorder != null && entgoodorder.Count>0)
                        {
                            return "您已经参加过该拼团了，请浏览其它团商品";
                        }
                    }

                    break;
            }

            if (groupmodel.LimitNum > 0 && groupnum > groupmodel.LimitNum)
            {
                return "您已超出购买限额，请浏览其它团商品";
            }

            //拼团商品下单后剩下的数量必须能成团,如： 2个人能成团,此时下单剩余数据必须>=1件(能让多一个人下单的数量)
            if (isgroup > 0 && stock - buyCount < groupmodel.GroupSize - 1)
            {
                return "拼团商品库存不足，无法成团";
            }

            #endregion
            //判断是否是团长，团员不减团长优惠价
            if (isgroup <= 0)
            {
                grouperprice = 0;
            }
            else
            {
                grouperprice = groupmodel.HeadDeduct;
            }
            return "";
        }

        public string UpdateGroupState(int id,int state,int type)
        {
            TransactionModel tran = new TransactionModel();
            EntGroupsRelation model = base.GetModel(id);
            if (model == null)
            {
                return  "找不到拼团产品，请刷新重试！" ;
            }
            tran.Add($"update EntGroupsRelation set state = {state} where id = {id}");

            switch (type)
            {
                case (int)TmpType.小程序餐饮模板:
                    tran.Add($"update foodgoods set state={state} where id = {model.EntGoodsId}");
                    break;
                case (int)TmpType.小程序专业模板:
                    if(state == -1)
                    {
                        tran.Add($"update entgoods set state=0 where id = {model.EntGoodsId}");
                    }
                    break;
            }

            if(!base.ExecuteTransaction(tran.sqlArray, tran.ParameterArray))
            {
                return "保存失败";
            }

            return "保存成功";
        }

        #region 专业/多门店拼团

        /// <summary>
        /// 填充拼团关联表数据
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="entgoodsids"></param>
        /// <returns></returns>
        public List<EntGoods> GetEntGoodRelation(List<EntGoods> goodList, int aid, int storeid)
        {
            if (goodList == null || goodList.Count <= 0)
                return goodList;

            string groupids = string.Join(",", goodList.Select(s => s.id).Distinct());
            if (string.IsNullOrEmpty(groupids))
                return goodList;

            List<EntGroupsRelation> entgrouprelations = GetListEntGroups_api(groupids, aid, storeid);
            if (entgrouprelations == null || entgrouprelations.Count <= 0)
                return goodList;

            foreach (EntGoods item in goodList)
            {
                item.EntGroups = entgrouprelations?.FirstOrDefault(f => f.EntGoodsId == item.id);
            }

            return goodList;
        }
        /// <summary>
        /// 专业版接口获取拼团列表
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="name"></param>
        /// <param name="state"></param>
        /// <param name="pagesize"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public List<EntGroupsRelation> GetListEntGroups_api(string ids, int rid, int storeid = 0)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            List<EntGroupsRelation> list = new List<EntGroupsRelation>();
            string strSql = $@"select eg.`name`,eg.id as goodid,eg.img,eg.price,eg.stock,eg.salesCount,eg.virtualSalesCount,egr.* from entgoods eg left join entgroupsrelation egr on eg.id = egr.entgoodsid where find_in_set(eg.id,@ids) and egr.rid={rid} and egr.state=1 and egr.storeid={storeid} order by find_in_set(eg.id,@ids)";

            parameters.Add(new MySqlParameter("@ids", ids));

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, strSql, parameters.ToArray()))
            {
                while (dr.Read())
                {
                    EntGroupsRelation model = GetModel(dr);
                    if (model != null)
                    {
                        model.Name = dr["name"].ToString();
                        if (DBNull.Value != dr["price"])
                        {
                            model.SinglePrice = float.Parse(dr["price"].ToString());
                        }
                        if (DBNull.Value != dr["stock"])
                        {
                            model.CreateNum = int.Parse(dr["stock"].ToString());
                        }

                        if (!string.IsNullOrEmpty(dr["img"].ToString()))
                        {
                            model.ImgUrl = ImgHelper.ResizeImg(dr["img"].ToString(), 750, 750);
                        }

                        int totalcount = 0;
                        List<object> userlist = EntGroupSponsorBLL.SingleModel.GetGoupsUserImgs(model.Id, ref totalcount, (int)TmpType.小程序专业模板, model.EntGoodsId);

                        //判断是否已结束
                        if (totalcount >= model.CreateNum)
                        {
                            model.State = 2;
                        }
                        else if ((model.ValidDateStart < DateTime.Now))
                        {
                            //判断是否开始
                            model.State = 1;
                        }
                        else
                        {
                            model.State = -1;
                        }

                        //已团数量
                        model.GroupsNum = totalcount;
                        model.salesCount = Convert.ToInt32(dr["salesCount"]);
                        model.virtualSalesCount = Convert.ToInt32(dr["virtualSalesCount"]);

                        list.Add(model);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取多门店分店拼团的团购价和原价
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public List<SubStoreGoodsView> GetMStoreGroup(List<SubStoreGoodsView> groups)
        {
            List<EntGroupsRelation> list = new List<EntGroupsRelation>();
            if (groups == null || groups.Count <= 0)
            {
                return groups;
            }

            string pids = string.Join(",", groups.Where(w => w.goodtype == (int)EntGoodsType.拼团产品)?.Select(s => s.Pid).Distinct());
            int storeid = groups[0].StoreId;
            int rid = groups[0].Aid;

            string sqlwhere = $"rid={rid} and storeid = {storeid} and entgoodsid in ({pids})";

            list = GetList(sqlwhere);
            if (list == null || list.Count <= 0)
            {
                return groups;
            }

            foreach (SubStoreGoodsView item in groups)
            {
                EntGroupsRelation model = list.FirstOrDefault(f => f.EntGoodsId == item.Pid);
                if (model != null)
                {
                    item.groupPrice = model.GroupPrice;
                    item.originalPrice = model.OriginalPrice;
                }
            }

            return groups;
        }
        #endregion

        #region 餐饮拼团
        /// <summary>
        /// 餐饮菜品列表获取拼团信息
        /// </summary>
        /// <param name="goodslist"></param>
        /// <param name="rid"></param>
        public void GetFoodGoodListGroup(ref List<FoodGoods> goodslist,int rid)
        {
            if (goodslist == null || goodslist.Count <= 0)
                return;

            //拼团产品
            string entgoodids = string.Join(",", goodslist.Where(w => w.goodtype == (int)EntGoodsType.拼团产品)?.Select(s => s.Id));
            List<EntGroupsRelation> entgrouplist = GetListByGroupGoodType(rid, entgoodids);

            //判断是否是拼团产品
            if (entgrouplist != null && entgrouplist.Count > 0)
            {
                
                foreach (FoodGoods good in goodslist)
                {
                    EntGroupsRelation entgroup = entgrouplist.FirstOrDefault(f => good.Id == f.EntGoodsId);
                    if (entgroup != null)
                    {
                        entgroup.GroupsNum = FoodGoodsOrderBLL.SingleModel.GetGroupPersonCount(0, good.Id)+entgroup.InitSaleCount;//加上初始化销售量
                        good.EntGroups = entgroup;
                    }
                }
            }
        }
        /// <summary>
        /// 餐饮菜品获取拼团详情
        /// </summary>
        /// <param name="good"></param>
        /// <param name="rid"></param>
        public void GetFoodGoodGroup(ref FoodGoods good,int rid)
        {
            if (good == null || good.goodtype != (int)EntGoodsType.拼团产品)
                return;

            

            //获取开团成功的数据
            List<EntGroupSponsor> entsponsorlist = EntGroupSponsorBLL.SingleModel.GetListByGoodRid(good.EntGroups.Id, 0, 0);
            //团ID
            string groupids = string.Join(",", entsponsorlist.Select(s => s.Id).Distinct());
            
            //判断是否是拼团产品
            good.EntGroups = GetModelByGroupGoodType(good.Id, rid);
            if (good.EntGroups == null)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "餐饮获取菜品拼团详情位空");
                return;
            }
            if(groupids!=null && groupids.Length>0)
            {
                good.EntGroups.GroupUserList = FoodGoodsOrderBLL.SingleModel.GetPersonByGroup(groupids);
            }
            
            good.EntGroups.GroupsNum = FoodGoodsOrderBLL.SingleModel.GetGroupPersonCount(0, good.Id)+good.EntGroups.InitSaleCount;//加上初始化销售量
            good.EntGroups.GroupSponsorList = EntGroupSponsorBLL.SingleModel.GetHaveSuccessGroup(good.EntGroups.Id, 10, good.Id, (int)TmpType.小程序餐饮模板);
        }

        /// <summary>
        /// 餐饮接口获取拼团信息
        /// </summary>
        /// <param name="state"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        //public List<EntGroupsRelation> GetListFoodGroups_api(int state, int rid, int pageSize, int pageIndex)
        //{
        //    List<EntGroupsRelation> list = new List<EntGroupsRelation>();

        //    string sqlwhere = $@"select f.price,f.goodsname,f.ImgUrl,f.Inventory,g.id as grid,g.state gstate,g.groupsize,g.groupprice,g.limitnum,g.headdeduct,
        //                            g.originalprice,
        //                            from entgroupsrelation g left join  foodgoods f 
        //                            on f.id = g.entgoodsid
        //                            where f.goodtype={(int)EntGoodsType.拼团产品} and g.rid ={rid} and g.state = {state} ";
        //    string ordersql = $" order by g.createdate LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

        //    using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sqlwhere + ordersql))
        //    {
        //        while (dr.Read())
        //        {
        //            EntGroupsRelation model = GetModel(dr);
        //            if (model == null)
        //            {
        //                continue;
        //            }

        //            if (dr["price"] != DBNull.Value)
        //            {
        //                model.SinglePrice = Convert.ToInt32(dr["price"]);
        //            }

        //            if (dr["Inventory"] != DBNull.Value)
        //            {
        //                model.CreateNum = Convert.ToInt32(dr["Inventory"]);
        //            }

        //            if (!string.IsNullOrEmpty(dr["ImgUrl"].ToString()))
        //            {
        //                model.ImgUrl = ImgHelper.ResizeImg(dr["ImgUrl"].ToString(), 750, 750);
        //            }

        //            int totalcount = 0;
        //            List<object> userlist = new EntGroupSponsorBLL().GetGoupsUserImgs(model.Id, ref totalcount, (int)TmpType.小程序餐饮模板, model.Id);

        //            //判断是否已结束
        //            if (totalcount >= model.CreateNum)
        //            {
        //                model.State = 2;
        //            }
        //            else if ((model.ValidDateStart < DateTime.Now))
        //            {
        //                //判断是否开始
        //                model.State = 1;
        //            }
        //            else
        //            {
        //                model.State = -1;
        //            }

        //            //已团数量
        //            model.GroupsNum = totalcount;

        //            list.Add(model);
        //        }
        //    }

        //    return list;
        //}
        #endregion
    }
}
