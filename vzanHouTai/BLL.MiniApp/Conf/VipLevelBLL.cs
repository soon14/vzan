
using BLL.MiniApp.Ent;
using BLL.MiniApp.Stores;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utility;

namespace BLL.MiniApp.Conf
{
    public class VipLevelBLL : BaseMySql<VipLevel>
    {
        #region 单例模式
        private static VipLevelBLL _singleModel;
        private static readonly object SynObject = new object();

        private VipLevelBLL()
        {

        }

        public static VipLevelBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new VipLevelBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private const int CACHE_TIME_SPAN = 30;
        private const string DEFAULT_LEVEL_KEY = "deflevel_{0}";
        private const string VIP_LEVEL_KEY = "viplevel_{0}";

        #region 基础操作
        /// <summary>
        /// 处理计算商品的会员价格(餐饮-MiniappFoodGoods)
        /// </summary>
        /// <param name="good">要计算的商品</param>
        /// <param name="vipLevel">会员等级Model</param>
        public void CalculateVipGoodPrice(FoodGoods good, VipLevel userVipLevel)
        {
            CalculateVipGoodsPrice(new List<FoodGoods>() { good }, userVipLevel, true);
        }

        /// <summary>
        /// 足浴版获取会员折扣（用到EntGoods的都可用）
        /// </summary>
        /// <param name="goods"></param>
        /// <param name="userVipLevel"></param>
        public void CalculateVipGoodsPrice(EntGoods goods, VipLevel userVipLevel)
        {
            if (userVipLevel == null)
            {
                goods.discount = 100;
                goods.discountPrice = goods.price;
                //多规格处理
                if (goods.GASDetailList != null && goods.GASDetailList.Count > 0)
                {
                    var detaillist = goods.GASDetailList.ToList();
                    detaillist.ForEach(g => g.originalPrice = g.discountPrice = g.price);
                    goods.specificationdetail = JsonConvert.SerializeObject(detaillist);
                }
                return;
            }
            switch (userVipLevel.type)
            {
                //全场折扣
                case 1:
                    goods.discount = userVipLevel.discount;
                    float discountPrice = goods.price * (userVipLevel.discount * 0.01F);
                    goods.discountPrice = discountPrice < 0.01 ? 0.01F : discountPrice;
                    //多规格处理
                    if (goods.GASDetailList != null && goods.GASDetailList.Count > 0)
                    {
                        var detaillist = goods.GASDetailList.ToList();
                        detaillist.ForEach(g =>
                        {
                            g.originalPrice = g.price;
                            g.discount = userVipLevel.discount;
                            discountPrice = g.price * (userVipLevel.discount * 0.01F);
                            g.discountPrice = discountPrice < 0.01 ? 0.01F : discountPrice;
                        });
                        goods.specificationdetail = JsonConvert.SerializeObject(detaillist);
                    }
                    break;
                //部分折扣
                case 2:
                    goods.discount = 100;
                    goods.discountPrice = goods.price;
                    //多规格处理
                    if (goods.GASDetailList != null && goods.GASDetailList.Count > 0)
                    {
                        var detaillist = goods.GASDetailList.ToList();
                        detaillist.ForEach(g => g.discountPrice = g.price);
                        goods.specificationdetail = JsonConvert.SerializeObject(detaillist);
                    }
                    if (!string.IsNullOrEmpty(userVipLevel.gids))
                    {
                        List<string> gidList = userVipLevel.gids.Split(',').ToList();

                        if (gidList.Contains(goods.id.ToString()))
                        {
                            goods.discount = userVipLevel.discount;
                            discountPrice = goods.price * (userVipLevel.discount * 0.01F);
                            goods.discountPrice = discountPrice < 0.01 ? 0.01F : discountPrice;
                            //多规格处理
                            if (goods.GASDetailList != null && goods.GASDetailList.Count > 0)
                            {
                                var detaillist = goods.GASDetailList.ToList();
                                detaillist.ForEach(g =>
                                {
                                    g.originalPrice = g.price;
                                    g.discount = userVipLevel.discount;
                                    discountPrice = g.price * (userVipLevel.discount * 0.01F);
                                    g.discountPrice = discountPrice < 0.01 ? 0.01F : discountPrice;
                                });
                                goods.specificationdetail = JsonConvert.SerializeObject(detaillist);
                            }
                        }
                    }
                    break;
                default:
                    goods.discount = 100;
                    goods.discountPrice = goods.price;
                    //多规格处理
                    if (goods.GASDetailList != null && goods.GASDetailList.Count > 0)
                    {
                        var detaillist = goods.GASDetailList.ToList();
                        detaillist.ForEach(g => g.discountPrice = g.price);
                        goods.specificationdetail = JsonConvert.SerializeObject(detaillist);
                    }
                    break;
            }
        }
        

        public VipLevel CreateDefLevel(string appid)
        {
            VipLevel level = new VipLevel();
            level = new VipLevel();
            level.addtime = DateTime.Now;
            level.appId = appid;
            level.name = "普通会员";
            level.bgcolor = "#4a86e8";
            level.updatetime = level.addtime;
            level.Id = Convert.ToInt32(Add(level));
            return level;
        }

        /// <summary>
        /// 根据id获取等级信息
        /// </summary>
        /// <param name="levelId"></param>
        /// <returns></returns>
        public VipLevel GetModelById(int levelId, int state = -1)
        {
            return GetModel($"id={levelId} and state>{state}");
        }
        #endregion

        #region 逻辑代码
        /// <summary>
        /// 更新会员等级信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="filed"></param>
        /// <returns></returns>
        public override bool Update(VipLevel model, string filed = "")
        {
            bool result = false;
            if (!string.IsNullOrEmpty(filed))
            {
                result = base.Update(model, filed);
            }
            else
            {
                result = base.Update(model);
            }
            if (result)
            {
                //清除缓存
                RemoveCache(model.Id, model.appId);
            }
            return result;
        }

        /// <summary>
        /// 清除会员等级缓存
        /// </summary>
        /// <param name="id"></param>
        public void RemoveCache(int id, string appid = "")
        {

            string key = string.Empty;
            if (id > 0)
            {
                key = string.Format(VIP_LEVEL_KEY, id);
                RedisUtil.Remove(key);
            }

            if (!string.IsNullOrEmpty(appid))
            {
                key = string.Format(DEFAULT_LEVEL_KEY, appid);
                RedisUtil.Remove(key);
            }

        }


        /// <summary>
        /// 处理计算商品的会员价格(餐饮-MiniappFoodGoodsCart)
        /// </summary>
        /// <returns>处理后的商品信息集合</returns>
        public void CalculateVipGoodsCartPrice(List<FoodGoodsCart> goodsCar, VipLevel userVipLevel)
        {
            if (!CheckList(goodsCar)) return;
            //记录原价
            goodsCar.ForEach(g => g.originalPrice = g.Price);

            if (userVipLevel == null) return;

            switch (userVipLevel.type)
            {
                case 1://全场打折
                    goodsCar.ForEach(g => g.Price = Convert.ToInt32(g.Price * (userVipLevel.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.Price * (userVipLevel.discount * 0.01)));
                    break;

                case 2://部分打折
                    List<string> gids = userVipLevel.gids.Split(',').ToList();
                    goodsCar.ForEach(g =>
                    {
                        if (gids.Contains(g.FoodGoodsId.ToString()))
                        {
                            g.Price = Convert.ToInt32(g.Price * (userVipLevel.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(g.Price * (userVipLevel.discount * 0.01));
                        }
                    });
                    break;

            }
        }

        /// <summary>
        /// 验证list是否有值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        private bool CheckList<T>(List<T> list)
        {
            return list != null && list.Count > 0;
        }
        /// <summary>
        /// 处理计算商品的会员价格(通用版：目前专业版在用)
        /// </summary>
        /// <param name="carts"></param>
        /// <param name="uid"></param>
        public void CalculateVipGoodsCartPrice(List<EntGoodsCart> carts, int uid)
        {
            if (!CheckList(carts)) return;
            carts.ForEach(g => g.originalPrice = g.Price);
            //获取会员信息
            VipRelation vipInfo = VipRelationBLL.SingleModel.GetModel($"uid={uid} and state>=0");
            if (vipInfo == null) return;
            VipLevel levelinfo = new VipLevelBLL().GetModel($"id={vipInfo.levelid} and state>=0");
            if (levelinfo == null) return;
            switch (levelinfo.type)
            {
                case 1://全场打折
                    carts.ForEach(g =>
                    {
                        g.discount = levelinfo.discount;
                        g.Price = Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)) < 1 ? 1 : Convert.ToInt32(g.Price * (levelinfo.discount * 0.01));
                    });
                    break;
                case 2://部分打折
                    List<string> gids = levelinfo.gids.Split(',').ToList();
                    carts.ForEach(g =>
                    {
                        if (gids.Contains(g.FoodGoodsId.ToString()))
                        {
                            g.discount = levelinfo.discount;
                            g.Price = Convert.ToInt32(g.Price * (levelinfo.discount * 0.01)) < 1 ? 1 : Convert.ToInt32(g.Price * (levelinfo.discount * 0.01));
                        }
                    });
                    break;
            }
        }


        /// <summary>
        /// 处理计算商品的会员价格(餐饮-MiniappFoodGoods)
        /// </summary>
        /// <param name="goodsList">要计算的商品集合</param>
        /// <param name="userVipLevel">会员等级Model</param>
        /// <param name="canCalculateDtl">是否需要计算多规格的优惠价格</param>
        public void CalculateVipGoodsPrice(List<FoodGoods> goodsList, VipLevel userVipLevel, bool canCalculateDtl = false)
        {
            if (goodsList != null && goodsList.Any())
            {
                //goodsList.ForEach(goods => goods.discountPrice = goods.Price);
                //无折扣
                goodsList.ForEach(goods =>
                {
                    goods.discount = 100;
                    goods.discountPrice = goods.Price;
                    goods.discountGroupPrice = goods.EntGroups.GroupPrice;

                    //多规格处理
                    List<FoodGoodsAttrDetail> attrDetailList = goods.GASDetailList.ToList();
                    if (attrDetailList != null && attrDetailList.Any())
                    {
                        attrDetailList.ForEach(attrDetail =>
                        {
                            attrDetail.discount = 100;
                            attrDetail.discountPrice = attrDetail.price;
                            attrDetail.discountGroupPrice = attrDetail.groupPrice;
                        });
                        goods.AttrDetail = SerializeHelper.SerToJson(attrDetailList);
                    }
                });

                if (userVipLevel != null && userVipLevel.type != 0)
                {
                    List<FoodGoods> list = new List<FoodGoods>();
                    switch (userVipLevel.type)
                    {
                        //全场折扣,所有商品都放入价格优惠处理集合
                        case 1:
                            list = goodsList;
                            break;

                        //部分折扣,只操作部分商品
                        case 2:
                            list = goodsList.FindAll(g => userVipLevel.gids.Split(',').Contains(g.Id.ToString()));
                            break;
                    }

                    if (list != null && list.Count > 0)
                    {
                        list.ForEach(goods =>
                        {
                            goods.discount = userVipLevel.discount;
                            goods.discountPrice = Convert.ToInt32(goods.Price * (userVipLevel.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(goods.Price * (userVipLevel.discount * 0.01));
                            goods.discountGroupPrice = Convert.ToInt32(goods.EntGroups.GroupPrice * (userVipLevel.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(goods.EntGroups.GroupPrice * (userVipLevel.discount * 0.01));

                            //多规格处理
                            List<FoodGoodsAttrDetail> attrDetailList = goods.GASDetailList.ToList();
                            if (canCalculateDtl && attrDetailList != null && attrDetailList.Any())
                            {
                                attrDetailList.ForEach(attrDetail =>
                                {
                                    attrDetail.discount = userVipLevel.discount;
                                    attrDetail.discountPrice = Convert.ToInt32(attrDetail.price * (attrDetail.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(attrDetail.price * (attrDetail.discount * 0.01));
                                    attrDetail.discountGroupPrice = Convert.ToInt32(attrDetail.groupPrice * (attrDetail.discount * 0.01)) == 0 ? 1 : Convert.ToInt32(attrDetail.groupPrice * (attrDetail.discount * 0.01));
                                });
                                goods.AttrDetail = SerializeHelper.SerToJson(attrDetailList);
                            }
                        });
                    }
                }
            }
        }


        public VipLevel GetDefModel(string appid, int PageType = 0)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return null;
            }
            string key = string.Format(DEFAULT_LEVEL_KEY, appid);
            VipLevel level = RedisUtil.Get<VipLevel>(key);
            if (level == null)
            {
                level = GetModel($"appid='{appid}' and level=0 and state>=0");
                if (level != null)
                {
                    RedisUtil.Set(key, level, TimeSpan.FromMinutes(CACHE_TIME_SPAN));
                }
            }


            if (level == null)
            {
                level = CreateDefLevel(appid);
            }
            if (level.type == 2 && !string.IsNullOrEmpty(level.gids))
            {
                switch (PageType)
                {
                    case (int)TmpType.小程序多门店模板:
                    case (int)TmpType.小程序足浴模板:
                    case (int)TmpType.小程序专业模板:
                        level.entGoodsList = EntGoodsBLL.SingleModel.GetList($"id in ({level.gids}) and state=1 and tag = 1 ");
                        break;
                    case (int)TmpType.小程序电商模板:
                        level.goodslist = StoreGoodsBLL.SingleModel.GetList($"id in ({level.gids}) and state>=0");
                        break;
                }
            }
            return level;
        }

        /// <summary>
        /// 通过缓存获取会员等级  
        /// </summary>
        /// <param name="id">viplevel 的 id</param>
        /// <returns></returns>
        public VipLevel GetModelByCache(int id)
        {
            string key = string.Format(VIP_LEVEL_KEY, id);
            VipLevel model = RedisUtil.Get<VipLevel>(key);
            if (model == null)
            {
                model = GetModel($"id={id} and state>=0");
                if (model != null)
                {
                    RedisUtil.Set(key, model, TimeSpan.FromMinutes(CACHE_TIME_SPAN));
                }
            }
            return model;
        }


        public List<VipLevel> GetListByAppId(string appId)
        {
            List<VipLevel> levelList = new List<VipLevel>();
            if (string.IsNullOrEmpty(appId))
            {
                return levelList;
            }
            string sqlwhere = $"appid =@appid and state>= 0";
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            levelList = GetListByParam(sqlwhere, paramters.ToArray());
            return levelList;
        }

        public VipLevel GetModelByAppid_Id(string appId, int levelId)
        {
            VipLevel level = null;
            if (string.IsNullOrEmpty(appId) || levelId <= 0)
            {
                return level;
            }
            string sqlwhere = $"id={levelId} and appid=@appId and state>=0";
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            level = GetModel(sqlwhere, paramters.ToArray());
            return level;
        }

        public void GetVipDiscount<T>(ref List<T> list, VipRelation vipInfo, VipLevel levelinfo,int userid, string discountstr,string pricestr)
        {
            if (list == null || list.Count <= 0)
                return;

            //VipRelation vipInfo = VipRelationBLL.SingleModel.GetModelByUserid(userid);
            if (vipInfo == null)
                return;

            //VipLevel levelinfo = base.GetModel(vipInfo.levelid);
            if (levelinfo == null)
                return;
         
            if (levelinfo.type == 1)//全场打折
            {
                foreach (T orderitem in list)
                {
                    int price = Convert.ToInt32(orderitem.GetType().GetProperty(pricestr).GetValue(orderitem).ToString());
                    orderitem.GetType().GetProperty(discountstr).SetValue(orderitem, levelinfo.discount);
                    orderitem.GetType().GetProperty(pricestr).SetValue(orderitem, Convert.ToInt32(price * (levelinfo.discount * 0.01)) < 1 ? 1 : Convert.ToInt32(price * (levelinfo.discount * 0.01)));
                }
            }
            else if (levelinfo.type == 2)//部分打折
            {
                List<string> gids = levelinfo.gids.Split(',').ToList();
                foreach (T orderitem in list)
                {
                    string goodsId = orderitem.GetType().GetProperty("GoodsId").GetValue(orderitem).ToString();
                    if (gids.Contains(goodsId))
                    {
                        int price = Convert.ToInt32(orderitem.GetType().GetProperty(pricestr).GetValue(orderitem).ToString());
                        orderitem.GetType().GetProperty(discountstr).SetValue(orderitem, levelinfo.discount);
                        orderitem.GetType().GetProperty(pricestr).SetValue(orderitem, Convert.ToInt32(price * (levelinfo.discount * 0.01)) < 1 ? 1 : Convert.ToInt32(price * (levelinfo.discount * 0.01)));
                    }
                }
            }
        }
        #endregion
    }
}
