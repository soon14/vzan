using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using BLL.MiniApp.Stores;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Plat;
using Entity.MiniApp.ViewModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.MiniApp.Conf
{
    public class VipRelationBLL : BaseMySql<VipRelation>
    {
        #region 单例模式
        private static VipRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private VipRelationBLL()
        {

        }

        public static VipRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new VipRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        



        #region 基础操作
        public VipRelation GetModelByRidAndUid(string appid,int uid)
        {
            return base.GetModel($"appId='{appid}' and uid={uid}");
        }
        public VipRelation GetModelByUserid(int userid)
        {
            return base.GetModel($"uid={userid} and state>=0");
        }
        #endregion

        /// <summary>
        /// 获取单个会员的详细信息
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="appid"></param>
        ///  <param name="PageType">模板对应例如PageType=22 表示专业版 PageType=6表示电商版</param>
        /// <returns></returns>
        public VipRelation GetVipModel(int uid, string appid, int PageType = 6, int aid = 0)
        {
            VipRelation vipRelation = GetModel($"uid={uid} and appid='{appid}' and state>=0");
            if (vipRelation == null)
            {
                VipLevel def_level = null;
                int pricesum = 0;

                switch (PageType)
                {
                    case (int)TmpType.小程序多门店模板:
                    case (int)TmpType.小程序足浴模板:
                    case (int)TmpType.小程序专业模板:
                        pricesum = GetEntGoodsVipPriceSum(uid);
                        break;
                    case (int)TmpType.小程序电商模板:
                        pricesum = GetVipConsumptionrecord(uid);
                        break;
                    case (int)TmpType.小程序餐饮模板:
                        pricesum = GetFoodVipPriceSum(uid);
                        break;
                    case (int)TmpType.小未平台子模版:
                        pricesum = GetPlatChildGoodsVipPriceSum(uid);
                        break;
                }

                if (pricesum <= 0)
                {
                    def_level = VipLevelBLL.SingleModel.GetDefModel(appid, PageType);
                }
                else
                {
                    VipRule rule = VipRuleBLL.SingleModel.GetModel($"minMoney<={pricesum} and maxMoney>{pricesum} and state>=0 and RuleType=0");
                    if (rule == null)
                    {
                        rule = VipRuleBLL.SingleModel.GetList($"appid='{appid}' and state>=0 and RuleType=0", 11, 1, "*", "minMoney desc")?.FirstOrDefault();//取最高级别规则
                    }
                    def_level = VipLevelBLL.SingleModel.GetModel(rule.levelid);
                }
                vipRelation = new VipRelation();
                vipRelation.appId = appid;
                vipRelation.addtime = vipRelation.updatetime = DateTime.Now;
                vipRelation.uid = uid;
                vipRelation.levelid = def_level.Id;
                vipRelation.PriceSum = pricesum;
                vipRelation.Id = Convert.ToInt32(Add(vipRelation));
                if (vipRelation.Id <= 0)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), $"用户注册会员失败 uid"+uid);
                }
                vipRelation.levelInfo = def_level;
            }
            else
            {
                vipRelation.levelInfo = VipLevelBLL.SingleModel.GetModel($"id={vipRelation.levelid} and state>=0");
                if (vipRelation.levelInfo != null)
                {
                    if (vipRelation.levelInfo.type == 2 && !string.IsNullOrEmpty(vipRelation.levelInfo.gids))
                    {
                        switch (PageType)
                        {
                            case (int)TmpType.小程序多门店模板:
                            case (int)TmpType.小程序足浴模板:
                            case (int)TmpType.小程序专业模板:
                                vipRelation.levelInfo.entGoodsList = EntGoodsBLL.SingleModel.GetList($"id in ({vipRelation.levelInfo.gids}) and state=1 and tag = 1 ");
                                break;
                            case (int)TmpType.小程序电商模板:
                                vipRelation.levelInfo.goodslist = StoreGoodsBLL.SingleModel.GetList($"id in ({vipRelation.levelInfo.gids}) and state>=0");
                                break;
                            case (int)TmpType.小程序餐饮模板:
                                vipRelation.levelInfo.foodgoodslist = FoodGoodsBLL.SingleModel.GetList($"id in ({vipRelation.levelInfo.gids}) and IsSell=1 and state=1");
                                break;
                            case (int)TmpType.小未平台子模版:
                                vipRelation.levelInfo.PlatChildGoodsList = PlatChildGoodsBLL.SingleModel.GetListByIds(vipRelation.levelInfo.gids);
                                break;
                        }
                    }
                }
                //专业版对接预约功能
                if(PageType == (int)TmpType.小程序专业模板)
                {
                    vipRelation.reservation = FoodReservationBLL.SingleModel.GetOnGoingReservation(aid, uid)?.Id.ToString();
                }
            }

            return vipRelation;
        }
        /// <summary>
        /// 获取会员列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="userName"></param>
        /// <param name="levelid"></param>
        /// <param name="leveltype"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public MiniappVipInfo GetVipList(string appId, int pageIndex, int pageSize, string userName, int levelid, int leveltype, string startDate, string endDate, string telePhone = "", int isor = 0)
        {
            string userIds = string.Empty;
            string sqlwhere = $" viprelation.appid='{appId}' and viprelation.state>=0 and c_userinfo.nickname is not NULL and c_userinfo.headimgurl is not NULL";
            List<MySql.Data.MySqlClient.MySqlParameter> mysqlParameter = new List<MySql.Data.MySqlClient.MySqlParameter>();

            if (isor > 0)
            {
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(telePhone))
                {
                    sqlwhere += $" and ( c_userinfo.nickname like @userName or c_userinfo.TelePhone like @telePhone )";
                    mysqlParameter.Add(new MySql.Data.MySqlClient.MySqlParameter("@userName", "%" + userName + "%"));
                    mysqlParameter.Add(new MySql.Data.MySqlClient.MySqlParameter("@TelePhone", "%" + telePhone + "%"));
                }

            }
            else
            {
                if (!string.IsNullOrEmpty(userName))
                {
                    sqlwhere += $" and c_userinfo.nickname like @userName ";
                    mysqlParameter.Add(new MySql.Data.MySqlClient.MySqlParameter("@userName", "%" + userName + "%"));
                }
                if (!string.IsNullOrEmpty(telePhone))
                {
                    //再将电话号码匹配订单的收货号码 找出下单的这个用户
                    userIds = EntGoodsOrderBLL.SingleModel.GetListUserIdByAccepterTelePhone(telePhone);

                    if (!string.IsNullOrEmpty(userIds))
                    {
                        sqlwhere += $" and (c_userinfo.Id in({userIds}) or c_userinfo.TelePhone like @telePhone)";

                    }
                    else
                    {
                        sqlwhere += $" and c_userinfo.TelePhone like @telePhone ";


                    }
                    mysqlParameter.Add(new MySql.Data.MySqlClient.MySqlParameter("@TelePhone", "%" + telePhone + "%"));

                }
            }
            if (levelid > 0)
            {
                sqlwhere += $" and viprelation.levelid={levelid}";
            }

            if (leveltype != -1)
            {
                sqlwhere += $" and viplevel.type={leveltype}";
            }
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                sqlwhere += $" and viprelation.addtime>=@startDate ";
                mysqlParameter.Add(new MySql.Data.MySqlClient.MySqlParameter("@startDate", startDate + " 00:00:00"));
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                sqlwhere += $" and viprelation.addtime<=@endDate ";
                mysqlParameter.Add(new MySql.Data.MySqlClient.MySqlParameter("@endDate", endDate + " 23:59:59"));
            }
            string sql = $"select viprelation.*,c_userinfo.Remark,c_userinfo.id as u_id,c_userinfo.TelePhone,c_userinfo.nickname,c_userinfo.headimgurl,c_userInfo.usertype,viplevel.type,viplevel.discount,viplevel.name,savemoneysetuser.AccountMoney,VipWxCardCode.Code from viprelation left join c_userinfo on viprelation.uid=c_userinfo.id left join viplevel on viprelation.levelid= viplevel.id left join savemoneysetuser on viprelation.uid=savemoneysetuser.userid left join VipWxCardCode on c_userinfo.id=VipWxCardCode.UserId  where {sqlwhere} order by id desc limit {pageSize * (pageIndex - 1)},{pageSize}";
            string countsql = $"select count(*) as count from viprelation left join c_userinfo on viprelation.uid=c_userinfo.id left join viplevel on viprelation.levelid= viplevel.id where {sqlwhere}";

           
            MiniappVipInfo model = new MiniappVipInfo();
            DataSet ds = SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql, mysqlParameter.ToArray());
           
            if (ds.Tables.Count <= 0) return model;
            DataTable dt = ds.Tables[0];
            if (dt == null || dt.Rows.Count <= 0) return model;
            model.relationList = new List<VipRelation>();


            foreach (DataRow row in dt.Rows)
            {
               

                VipRelation viprelation = new VipRelation();
                viprelation.levelInfo = new VipLevel();
                if (row["Code"] != DBNull.Value)
                {
                    viprelation.WxVipCode = row["Code"].ToString();
                }
                if (row["TelePhone"] != DBNull.Value)
                {
                    viprelation.TelePhone = row["TelePhone"].ToString();
                }
               
                if (row["id"] != DBNull.Value)
                {
                    viprelation.Id = Convert.ToInt32(row["id"]);
                }
                if (row["addtime"] != DBNull.Value)
                {
                    viprelation.addtime = Convert.ToDateTime(row["addtime"]);
                }
                if (row["updatetime"] != DBNull.Value)
                {
                    viprelation.updatetime = Convert.ToDateTime(row["updatetime"]);
                }
                if (row["nickname"] != DBNull.Value)
                {
                    viprelation.username = row["nickname"].ToString();
                }
                if (row["pricesum"] != DBNull.Value)
                {
                    viprelation.PriceSum = Convert.ToInt32(row["pricesum"]);
                }
                if (row["name"] != DBNull.Value)
                {
                    viprelation.levelInfo.name = row["name"].ToString();
                }
                if (row["discount"] != DBNull.Value)
                {
                    viprelation.levelInfo.discount = Convert.ToInt32(row["discount"]);
                }
                if (row["type"] != DBNull.Value)
                {
                    viprelation.levelInfo.type = Convert.ToInt32(row["type"]);
                }
                if (row["levelid"] != DBNull.Value)
                {
                    viprelation.levelid = Convert.ToInt32(row["levelid"]);
                }
                if (row["headimgurl"] != DBNull.Value)
                {
                    viprelation.headimgurl = row["headimgurl"].ToString();
                }
                if (row["AccountMoney"] != DBNull.Value)
                {
                    viprelation.AccountMoney = Convert.ToInt32(row["AccountMoney"]);
                }
                if (row["uid"] != DBNull.Value)
                {
                    viprelation.uid = Convert.ToInt32(row["uid"]);
                }
                if (row["usertype"] != DBNull.Value)
                {
                    viprelation.userType = Convert.ToInt32(row["usertype"]);
                }
                if (row["Remark"] != DBNull.Value)
                {
                    viprelation.Remark = Convert.ToString(row["Remark"]);
                }
                if (string.IsNullOrEmpty(viprelation.TelePhone))
                {
                    //暂时获取该用户普通订单的电话号码，待下次微信授权电话号码再拿微信授权电话号码
                    EntGoodsOrder order=   EntGoodsOrderBLL.SingleModel.GetModel($"UserId={viprelation.uid}");
                    if (order != null)
                    {
                        viprelation.TelePhone = order.AccepterTelePhone;
                    }
                }
                viprelation.SaveMoneySum= SaveMoneySetUserBLL.SingleModel.GetSaveMoneySum(viprelation.uid)*0.01;
               
                model.relationList.Add(viprelation);
            }
            DataSet dc = SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, countsql, mysqlParameter.ToArray());
            if (dc.Tables.Count <= 0) return model;
            DataTable cdt = dc.Tables[0];
            model.recordCount = Convert.ToInt32(cdt.Rows[0]["count"]);
            model.kfCount = model.relationList.Where(relation => relation.userType == 1).Count();
            return model;

        }

        public bool DelModel(VipRelation viprelation)
        {
            if (viprelation == null) return false;
            viprelation.state = -1;
            viprelation.updatetime = DateTime.Now;
            return Update(viprelation, "state,updatetime");
        }

        #region 会员自动升级和消费记录插入
        /// <summary>
        ///会员自动升级和消费记录插入
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool updatelevel(int userId, string xcxTemplate, int pricesum = 0)
        {
            switch (xcxTemplate)
            {
                case "food": pricesum = GetFoodVipPriceSum(userId); break;
                case "entpro": pricesum = GetEntGoodsVipPriceSum(userId); break;
                case "footbath": pricesum = GetFootbathVipPriceSum(userId); break;
                case "multistore": pricesum = GetMultiStoreVipPriceSum(userId); break;
                case "platchild": pricesum = GetPlatChildGoodsVipPriceSum(userId); break;
                case "qiye": pricesum = 0; break;
                default: pricesum = GetVipConsumptionrecord(userId); break;
            }

            bool result = false;
            string sqlwhere = string.Empty;
            VipRelation viprelation = GetModel($"uid={userId} and state>=0");
            if (viprelation != null)
            {
                viprelation.PriceSum = pricesum;
                //获取会员卡参数配置
                VipConfig vipconfig = VipConfigBLL.SingleModel.GetModel($"appid='{viprelation.appId}' and state>=0");

                //当前会员等级规则
                VipRule rule = VipRuleBLL.SingleModel.GetModel($"levelid={viprelation.levelid} and state>=0 and RuleType=0");
                List<VipRule> updateRules = VipRuleBLL.SingleModel.GetList($" state>=0 and RuleType=0 and appid='{viprelation.appId}' and {viprelation.PriceSum}>minMoney ", 12, 1, "*", "minMoney desc");
                //是否开启自动升级
                if (vipconfig != null && vipconfig.autoupdate == 1)//&& rule != null
                {
                    if (updateRules != null && updateRules.Count > 0)
                    {
                        if (rule==null||rule.minMoney <= updateRules[0].minMoney)
                        {
                            viprelation.levelid = updateRules[0].levelid;
                            viprelation.updatetime = DateTime.Now;
                        }
                    }
                }

                result = Update(viprelation, "levelid,pricesum,updatetime");
            }
            return result;
        }

        /// <summary>
        /// 储值 会员升级
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public bool updatelevelBySaveMoney(int userId,int price)
        {
            VipRelation viprelation = new VipRelationBLL().GetModel($"uid={userId} and state>=0");
            if (viprelation != null)
            {
                //获取储值升级会员规则 获取会员卡参数配置

                VipConfig vipconfig = VipConfigBLL.SingleModel.GetModel($"appid='{viprelation.appId}' and state>=0");
    
                //当前会员等级规则
                VipRule rule = VipRuleBLL.SingleModel.GetModel($"levelid={viprelation.levelid} and state>=0 and RuleType=1");

                if (vipconfig != null && vipconfig.SaveMoneyAutoUpdate == 1)//&& rule != null
                {

                    int viprelationPrice = price;//默认单次充值不包含赠送的
                    if (vipconfig.SaveMoneyType == 1)
                    {
                        //表示累计充值
                        viprelationPrice = SaveMoneySetUserBLL.SingleModel.GetSaveMoneySum(userId);
                    }


                    List<VipRule> updateRules = VipRuleBLL.SingleModel.GetList($" state>=0 and RuleType=1 and appid='{viprelation.appId}' and {viprelationPrice}>minMoney ", 12, 1, "*", "minMoney desc");

                    if (updateRules != null && updateRules.Count > 0)
                    {
                        if (rule == null || rule.minMoney <= updateRules[0].minMoney)
                        {
                            viprelation.levelid = updateRules[0].levelid;
                            viprelation.updatetime = DateTime.Now;
                        }
                    }


                  return  base.Update(viprelation, "levelid,updatetime");

                }
            }
            return false;

        }


        private int GetMultiStoreVipPriceSum(int userid)
        {
            int sum = 0;
            string sql = $"select sum(buyprice) as count from EntGoodsOrder where USERID = {userid} and State ={(int)MiniAppEntOrderState.交易成功} and templatetype ={(int)TmpType.小程序多门店模板}";//购物消费总额  专业版已收货才确定计算为消费
            var result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (DBNull.Value != result)
            {
                sum += Convert.ToInt32(result);
            }

            sum += GetBargainAndGroupPriceSum(userid);
            sum += GetStoredvaluePaySum(userid);

            return sum;
        }

        /// <summary>
        /// 获取用户消费记录（电商）
        /// </summary>
        /// <returns></returns>
        public int GetVipConsumptionrecord(int userid)
        {
            int sum = 0;
            string sql = $"select sum(buyprice) as count from storegoodsorder where USERID = {userid} AND state = 6";//购物消费总额
            var result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);

            if (DBNull.Value != result)
            {
                sum += Convert.ToInt32(result);
            }

            sum += GetBargainAndGroupPriceSum(userid);
            sum += GetEditSaveMoney(userid);
            return sum;
        }
        /// <summary>
        /// 获取用户消费记录（餐饮）
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int GetFoodVipPriceSum(int userid)
        {
            int sum = 0;
            string sql = $"select sum(buyprice) as count from FoodGoodsOrder where USERID = {userid} and State = {(int)miniAppFoodOrderState.已完成}";//购物消费总额
            var result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (DBNull.Value != result)
            {
                sum += Convert.ToInt32(result);
            }
            sum += GetEditSaveMoney(userid);
            return sum;
        }

        /// <summary>
        /// 获取足浴版用户消费记录
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int GetFootbathVipPriceSum(int userid)
        {
            int sum = 0;
            string sql = $"select sum(buyprice) as count from EntGoodsOrder where USERID = {userid} and State in ({(int)MiniAppEntOrderState.已超时},{(int)MiniAppEntOrderState.交易成功}) and OrderType=0 and templatetype={(int)TmpType.小程序足浴模板}";//服务消费总额  足浴版开单后算为消费
            var result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (DBNull.Value != result)
            {
                sum += Convert.ToInt32(result);
            }

            sql = $"select sum(buyprice) as count from EntGoodsOrder where USERID = {userid} and State =3 and OrderType=2 and templatetype={(int)TmpType.小程序足浴模板}";
            var result2 = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (DBNull.Value != result2)
            {
                sum += Convert.ToInt32(result2);
            }
            return sum;
        }

        /// <summary>
        /// 获取专业版用户消费记录
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int GetEntGoodsVipPriceSum(int userid)
        {
            int sum = 0;
            string sql = $"select sum(buyprice) as count from EntGoodsOrder where USERID = {userid} and State = 3 and templatetype ={(int)TmpType.小程序专业模板}";//购物消费总额  专业版已收货才确定计算为消费
            var result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (DBNull.Value != result)
            {
                sum += Convert.ToInt32(result);
            }

            sum += GetBargainAndGroupPriceSum(userid);
            sum += GetStoredvaluePaySum(userid);
            sum += GetEditSaveMoney(userid);
            return sum;
        }

        /// <summary>
        /// 获取平台独立小程序用户消费记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetPlatChildGoodsVipPriceSum(int userId)
        {
            int sum = 0;
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userinfo == null)
                return sum;
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(userinfo.appId);

            if (xcxrelation == null)
                return sum;
            PlatStore store = PlatStoreBLL.SingleModel.GetModelByAId(xcxrelation.Id);

            if (store == null)
                return sum;
            xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(store.BindPlatAid);

            if (xcxrelation==null)
                return sum;
            userinfo = C_UserInfoBLL.SingleModel.GetModelByTelephone_appid(userinfo.TelePhone,xcxrelation.AppId);
            int puserId = 0;

            if (userinfo!=null)
            {
                puserId = userinfo.Id;
            }

            string sql = $"select sum(buyprice) as count from PlatChildGoodsOrder where ((USERID = {userId} and templatetype ={(int)TmpType.小未平台子模版}) or (userid={puserId} and storeid={store.Id} and templatetype={(int)TmpType.小未平台})) and State = {(int)PlatChildOrderState.已完成}";//购物消费总额  已收货才确定计算为消费
            var result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (DBNull.Value != result)
            {
                sum += Convert.ToInt32(result);
            }

            //sum += GetBargainAndGroupPriceSum(userid);
            sum += GetStoredvaluePaySum(userId);
            sum += GetEditSaveMoney(userId);
            return sum;
        }

        /// <summary>
        /// 获取平台独立小程序用户消费记录
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int GetQiyeGoodsVipPriceSum(int userid)
        {
            int sum = 0;
            string sql = $"select sum(buyprice) as count from QiyeGoodsOrder where USERID = {userid} and State = {(int)QiyeOrderState.已完成} and templatetype ={(int)TmpType.企业智推版}";//购物消费总额  已收货才确定计算为消费
            var result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (DBNull.Value != result)
            {
                sum += Convert.ToInt32(result);
            }

            //sum += GetBargainAndGroupPriceSum(userid);
            sum += GetStoredvaluePaySum(userid);
            sum += GetEditSaveMoney(userid);
            return sum;
        }

        public int GetEntGoodsOrderCount(int userid)
        {
            int sum = 0;
            string sql = $"select count(Id) as number from EntGoodsOrder where USERID = {userid} and State = 3 and templatetype ={(int)TmpType.小程序专业模板}";//购物消费总额  专业版已收货才确定计算为消费
            var result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (DBNull.Value != result)
            {
                sum += Convert.ToInt32(result);
            }

            sum += GetBargainAndGroupOrderSum(userid);
            sum += GetStoredvaluePayOrderSum(userid);
            return sum;
        }



        /// <summary>
        /// 获取砍价.拼团  累计消费
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int GetBargainAndGroupPriceSum(int userid)
        {
            int sum = 0;
            string bargain_sql = $"SELECT sum(b.CurrentPrice+g.GoodsFreight) as count from bargainuser b LEFT JOIN  bargain g on b.bid=g.id  where b.USERID = {userid} and b.State=8 ";//砍价消费总额
            var result1 = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, bargain_sql);
            if (result1 != DBNull.Value)
            {
                sum += Convert.ToInt32(result1);
            }

            string group_sql = $"SELECT sum(BuyPrice) as count from groupuser where ObtainUserId = {userid} and State={(int)MiniappPayState.已收货} ";//拼图消费总额

            var result2 = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, group_sql);
            if (result2 != DBNull.Value)
            {
                sum += Convert.ToInt32(result2);
            }

            return sum;

        }

        /// <summary>
        /// 获取专业版砍价.拼团 确认收货后的订单数量
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int GetBargainAndGroupOrderSum(int userid)
        {
            int sum = 0;
            string bargain_sql = $"SELECT Count(b.Id) as count from bargainuser b LEFT JOIN  bargain g on b.bid=g.id  where b.USERID = {userid} and b.State=8 ";//砍价消费总额
            var result1 = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, bargain_sql);
            if (result1 != DBNull.Value)
            {
                sum += Convert.ToInt32(result1);
            }

            string group_sql = $"SELECT Count(Id) as count from groupuser where ObtainUserId = {userid} and State={(int)MiniappPayState.已收货} ";//拼图消费总额

            var result2 = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, group_sql);
            if (result2 != DBNull.Value)
            {
                sum += Convert.ToInt32(result2);
            }

            return sum;

        }


        /// <summary>
        /// 获取到店使用储值消费记录
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int GetStoredvaluePaySum(int userid)
        {
            int sum = 0;
            //获取到店使用储值消费记录
            string storedvaluePay_sql = $"SELECT sum(payment_free) as count from CityMorders where FuserId = {userid} and payment_status=1 and  OrderType ={(int)ArticleTypeEnum.MiniappStoredvaluePay} ";
            var result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, storedvaluePay_sql);
            if (result != DBNull.Value)
            {
                sum += Convert.ToInt32(result);
            }

            return sum;
        }

        public int GetStoredvaluePayOrderSum(int userid)
        {
            int sum = 0;
            //获取到店使用储值消费记录条数
            string storedvaluePay_sql = $"SELECT Count(Id) as count from CityMorders where FuserId = {userid} and payment_status=1 and  OrderType ={(int)ArticleTypeEnum.MiniappStoredvaluePay} ";
            var result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, storedvaluePay_sql);
            if (result != DBNull.Value)
            {
                sum += Convert.ToInt32(result);
            }

            return sum;
        }


        /// <summary>
        /// 获取商家在后台手动扣除用户储值余额  没有订单,只在CityMorders有记录
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int GetEditSaveMoney(int userid)
        {
            int sum = 0;
            //获取商家在后台手动扣除用户储值余额
            string storedvaluePay_sql = $"SELECT sum(payment_free) as count from CityMorders where FuserId = {userid} and payment_status=1 and  OrderType ={(int)ArticleTypeEnum.MiniappEditSaveMoney} ";
            var result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, storedvaluePay_sql);
            if (result != DBNull.Value)
            {
                sum += Convert.ToInt32(result);
            }

            return sum;
        }

        public VipRelation GetModelByAppid_Id(string appId, int id)
        {
            VipRelation model = null;
            if (string.IsNullOrEmpty(appId) || id <= 0)
            {
                return model;
            }
            string sqlwhere = $"id={id} and appid=@appid and state>=0";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@appid", appId));
            model = GetModel(sqlwhere, parameters.ToArray());
            return model;
        }

        public VipRelation GetVipInfoByUserId(int userId)
        {
            VipRelation relation = null;
            string sql = $"select a.*,b.name as levelname, c.nickname,c.headimgurl from viprelation a left join viplevel b on a.levelid=b.id left join c_userInfo c on c.id=a.uid where a.uid={userId}";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    relation = GetModel(dr);
                    relation.username = dr["nickname"].ToString();
                    relation.levelName = dr["levelname"].ToString();
                    relation.headimgurl = dr["headimgurl"].ToString();
                }
            }
            return relation;
        }

        public List<VipRelation> GetListByUserIds(string userIds)
        {
            List<VipRelation> list = new List<VipRelation>();
            if (string.IsNullOrEmpty(userIds))
                return list;

            string sql = $"select a.*,b.name as levelname, c.nickname,c.headimgurl from viprelation a left join viplevel b on a.levelid=b.id left join c_userInfo c on c.id=a.uid where a.uid in({userIds})";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    VipRelation model = GetModel(dr);
                    model.username = dr["nickname"].ToString();
                    model.levelName = dr["levelname"].ToString();
                    model.headimgurl = dr["headimgurl"].ToString();
                    list.Add(model);
                }
            }
            return list;
        }
        #endregion
    }
}
