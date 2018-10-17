using BLL.MiniApp.cityminiapp;
using BLL.MiniApp.Ent;
using BLL.MiniApp.PlatChild;
using BLL.MiniApp.Tools;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.PlatChild;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace BLL.MiniApp.Conf
{
    public class GoodsCommentBLL : BaseMySql<GoodsComment>
    {
        #region 单例模式
        private static GoodsCommentBLL _singleModel;
        private static readonly object SynObject = new object();

        private GoodsCommentBLL()
        {

        }

        public static GoodsCommentBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new GoodsCommentBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        private static readonly string _redis_GoodsCommentKey = "redis_GoodsComment_{0}_{1}_{2}_{3}";
        private static readonly string _redis_GoodsCommentApiKey = "redis_GoodsCommentApi_{0}_{1}_{2}_{3}_{4}";
        private static readonly string _redis_GoodsCommentUserApiKey = "redis_GoodsCommentUserApi_{0}_{1}_{2}_{3}";
        public static readonly string _redis_GoodsCommentVersion = "redis_GoodsCommentVersion_{0}";//版本控制

        public List<GoodsComment> GetListByGoodsIds(int aid,string goodsids, int userid, int type)
        {
            string sqlwhere = $"AId={aid} and UserId = {userid} and GoodsId in ({goodsids}) and Type={type}";
            return base.GetList(sqlwhere);
        }

        public bool ExitModel(int aid,int userid,int goodsid,int orderid,int goodsType)
        {
            return base.Exists($"aid={aid} and userid={userid}  and goodsid={goodsid} and orderid={orderid} and Type={goodsType}");
        }

        public void DealGoodsCommentState<T>(ref List<T> goodslist ,int aid,int userid,int type, string idname,string orderidname)
        {
            if (goodslist == null || goodslist.Count <= 0)
                return;
            //PropertyInfo[] proinfo = goodslist[0].GetType().GetProperties();
            //if (proinfo == null || proinfo.Length <= 0)
            //    return;

            //string idname = proinfo.Where(w=>w.Name.ToLower()=="id").FirstOrDefault()?.Name;
            List<string> goodsidlist = new List<string>();
            foreach (T item in goodslist)
            {
                goodsidlist.Add(item.GetType().GetProperty(idname).GetValue(item).ToString());
            }

            string goodsids = string.Join(",", goodsidlist);
            if (goodsids.Length <= 0)
                return;
            
            List<GoodsComment> commentlist = GetListByGoodsIds(aid,goodsids,userid,type);
            
            if (commentlist==null || commentlist.Count<=0)
                return;
           
            foreach (T item in goodslist)
            {
                int id = Convert.ToInt32(item.GetType().GetProperty(idname).GetValue(item).ToString());
                int orderid = Convert.ToInt32(item.GetType().GetProperty(orderidname).GetValue(item).ToString());
                GoodsComment goodscomment = commentlist.Where(w => w.GoodsId == id && w.OrderId== orderid).FirstOrDefault();
                if (goodscomment != null)
                {
                    item.GetType().GetProperty("IsCommentting").SetValue(item, true);
                }
            }
        }

        public GoodsComment GetModelByIdState(int id,int state)
        {
            return base.GetModel($"id={id} and state={state}");
        }

        /// <summary>
        /// 获取用户评论
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <param name="reflesh"></param>
        /// <returns></returns>
        public List<GoodsComment> GetUserGoodsCommentListApi(int aid, int userid,int haveimg, int pageIndex, int pageSize, ref int count, bool reflesh = true)
        {
            string paramsqlwhere = "";
            if (haveimg >= 0)
            {
                paramsqlwhere = $" and haveimg={haveimg}";
                reflesh = true;
            }

            RedisModel<GoodsComment> model = new RedisModel<GoodsComment>();
            model = RedisUtil.Get<RedisModel<GoodsComment>>(string.Format(_redis_GoodsCommentUserApiKey, aid, userid, pageSize, pageIndex));
            int dataversion = RedisUtil.GetVersion(string.Format(_redis_GoodsCommentVersion, aid));

            if (reflesh || model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                model = new RedisModel<GoodsComment>();
                List<GoodsComment> list = new List<GoodsComment>();

                string sqlcount = $" select Count(*) from GoodsComment";
                string sql = $"  select gc.*,cu.HeadImgUrl from GoodsComment gc left join c_userinfo cu on gc.userid = cu.Id ";
                string sqlwhere = $" where state>-1 and aid = {aid} and userid={userid} {paramsqlwhere}";
                string sqllimit = $" ORDER BY gc.addtime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";

                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere + sqllimit))
                {
                    while (dr.Read())
                    {
                        GoodsComment amodel = base.GetModel(dr);
                        if (amodel != null)
                        {
                            amodel.HeadImgUrl = dr["HeadImgUrl"].ToString();
                            amodel.CommentImgs = C_AttachmentBLL.SingleModel.GetListByCache(amodel.Id, (int)AttachmentItemType.小程序商品评论轮播图);
                            list.Add(amodel);
                        }
                    }
                }

                if (list == null || list.Count <= 0)
                {
                    return new List<GoodsComment>();
                }

                //点赞
                string ids = string.Join(",",list.Select(s=>s.Id));
                List<CityUserFavoriteMsg> pointslist = CityUserFavoriteMsgBLL.SingleModel.GetListByaidAndMIds(aid,userid,ids,1,(int)PointsDataType.评论);
                if(pointslist!=null &&  pointslist.Count>0)
                {
                    foreach (GoodsComment item in list)
                    {
                        CityUserFavoriteMsg pmodel = pointslist.FirstOrDefault(f => f.msgId == item.Id);
                        if (pmodel != null && pmodel.state == 0)
                        {
                            item.UserPoints = true;
                        }
                    }
                }

                count = base.GetCountBySql(sqlcount + sqlwhere);
                model.DataList = list;
                model.DataVersion = dataversion;
                model.Count = count;

                if (paramsqlwhere == "")
                {
                    RedisUtil.Set<RedisModel<GoodsComment>>(string.Format(_redis_GoodsCommentUserApiKey, aid, userid, pageSize, pageIndex), model);
                }
            }
            else
            {
                count = model.Count;
            }

            return model.DataList;
        }

        /// <summary>
        /// 获取商品评论
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="goodsid"></param>
        /// <param name="storeid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <param name="reflesh"></param>
        /// <returns></returns>
        public List<GoodsComment> GetGoodsCommentListApi(int aid,int goodsid,int userid, int storeid, int pageIndex, int pageSize,int haveimg, ref int count, bool reflesh = true)
        {
            string paramsqlwhere = "";
            if(haveimg>=0)
            {
                paramsqlwhere = $" and gc.haveimg={haveimg}";
                reflesh = true;
            }
            
            RedisModel<GoodsComment> model = new RedisModel<GoodsComment>();
            model = RedisUtil.Get<RedisModel<GoodsComment>>(string.Format(_redis_GoodsCommentApiKey, aid, storeid,goodsid, pageSize, pageIndex));
            int dataversion = RedisUtil.GetVersion(string.Format(_redis_GoodsCommentVersion, aid));

            if (reflesh || model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                model = new RedisModel<GoodsComment>();
                List<GoodsComment> list = new List<GoodsComment>();

                string sqlcount = $" select Count(*) from GoodsComment gc";
                string sql = $" select gc.*,cu.HeadImgUrl from GoodsComment gc left join c_userinfo cu on gc.userid = cu.Id ";
                string sqlwhere = $" where gc.state>-1 and gc.aid = {aid} and gc.storeid={storeid} and gc.goodsid={goodsid} and gc.hidden=0 {paramsqlwhere}";
                string sqllimit = $" ORDER BY gc.addtime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";

                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere + sqllimit))
                {
                    while (dr.Read())
                    {
                        GoodsComment amodel = base.GetModel(dr);
                        if (amodel != null)
                        {
                            amodel.HeadImgUrl = dr["HeadImgUrl"].ToString();
                            amodel.CommentImgs = C_AttachmentBLL.SingleModel.GetListByCache(amodel.Id, (int)AttachmentItemType.小程序商品评论轮播图);
                            list.Add(amodel);
                        }
                    }
                }

                if (list == null || list.Count <= 0)
                {
                    return new List<GoodsComment>();
                }
                
                //点赞
                string ids = string.Join(",", list.Select(s => s.Id));
                List<CityUserFavoriteMsg> pointslist = CityUserFavoriteMsgBLL.SingleModel.GetListByaidAndMIds(aid, userid, ids, 1, (int)PointsDataType.评论);
                if (pointslist != null && pointslist.Count > 0)
                {
                    foreach (GoodsComment item in list)
                    {
                        CityUserFavoriteMsg pmodel = pointslist.FirstOrDefault(f => f.msgId == item.Id);
                        if (pmodel != null && pmodel.state==0)
                        {
                            item.UserPoints = true;
                        }
                    }
                }

                count = base.GetCountBySql(sqlcount + sqlwhere);
                model.DataList = list;
                model.DataVersion = dataversion;
                model.Count = count;

                if(paramsqlwhere=="")
                    RedisUtil.Set<RedisModel<GoodsComment>>(string.Format(_redis_GoodsCommentApiKey, aid, storeid, goodsid, pageSize, pageIndex), model);
            }
            else
            {
                count = model.Count;
            }


            return model.DataList;
        }

        /// <summary>
        /// 后台获取该模板商品的评论
        /// </summary>
        /// <param name="goodsname"></param>
        /// <param name="aid"></param>
        /// <param name="storeid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <param name="reflesh"></param>
        /// <returns></returns>
        public List<GoodsComment> GetGoodsCommentList(string goodsname,int aid,int storeid,int pageIndex, int pageSize, ref int count, bool reflesh = true)
        {
            List<MySqlParameter> parms = new List<MySqlParameter>();
            string extrawhere = "";
            if (!string.IsNullOrEmpty(goodsname))
            {
                extrawhere += $" and goodsname like @goodsname ";
                reflesh = true;
                parms.Add(new MySqlParameter("goodsname", "%"+goodsname+"%"));
            }

            RedisModel<GoodsComment> model = new RedisModel<GoodsComment>();
            model = RedisUtil.Get<RedisModel<GoodsComment>>(string.Format(_redis_GoodsCommentKey, aid, storeid, pageSize, pageIndex));
            int dataversion = RedisUtil.GetVersion(string.Format(_redis_GoodsCommentVersion, aid));

            if (reflesh || model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                model = new RedisModel<GoodsComment>();
                List<GoodsComment> list = new List<GoodsComment>();

                string sqlcount = $" select Count(*) from GoodsComment";
                string sql = $" select * from GoodsComment";
                string sqlwhere = $" where state>-1 and aid = {aid} and storeid={storeid} {extrawhere}";
                

                string sqllimit = $" ORDER BY addtime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";

                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere + sqllimit,parms.ToArray()))
                {
                    while (dr.Read())
                    {
                        GoodsComment amodel = base.GetModel(dr);
                        //if(amodel!=null)
                        //{
                        //    amodel.CommentImgs = C_AttachmentBLL.SingleModel.GetListByCache(amodel.Id, (int)AttachmentItemType.小程序商品评论轮播图);
                        //    list.Add(amodel);
                        //}
                        list.Add(amodel);
                    }
                }

                if (list == null || list.Count <= 0)
                {
                    return new List<GoodsComment>();
                }
                
                count = base.GetCountBySql(sqlcount + sqlwhere, parms.ToArray());
                model.DataList = list;
                model.DataVersion = dataversion;
                model.Count = count;
                if (extrawhere=="")
                {
                    RedisUtil.Set<RedisModel<GoodsComment>>(string.Format(_redis_GoodsCommentKey, aid, storeid, pageSize, pageIndex), model);
                }
            }
            else
            {
                count = model.Count;
            }

            return model.DataList;
        }
        
        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="agentid"></param>
        public void RemoveCache(int aid)
        {
            if (aid > 0)
            {
                RedisUtil.SetVersion(string.Format(_redis_GoodsCommentVersion, aid));
            }
        }

        public override bool Update(GoodsComment model,string confile)
        {
            bool isok = base.Update(model, confile);
            if(isok)
            {
                RemoveCache(model.AId);
            }
            return isok;
        }

        public override object Add(GoodsComment model)
        {
            RemoveCache(model.AId);
            return base.Add(model);
        }


        /// <summary>
        /// 获取商品名称
        /// </summary>
        /// <param name="xcxtemplatetype">模板类型</param>
        /// <param name="goodstype">商品类型</param>
        /// <param name="goodsid">商品ID（拼团entgoods表ID，团购groups表ID，普通商品entgoods表ID，砍价bargain表ID）</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string GetGoodsName(int xcxtemplatetype, int goodstype, int goodsid, ref string msg)
        {
            string goodsname = "";
            switch (xcxtemplatetype)
            {
                case (int)TmpType.小程序专业模板:
                    switch (goodstype)
                    {
                        case (int)EntGoodsType.团购商品:
                            Groups groups = GroupsBLL.SingleModel.GetModel(goodsid);
                            if (groups == null)
                                msg = "找不到商品数据";
                            else
                                goodsname = groups.GroupName;
                            break;
                        case (int)EntGoodsType.拼团产品:
                            EntGoods entgroupgoods = EntGoodsBLL.SingleModel.GetModel(goodsid);
                            if (entgroupgoods == null)
                                msg = "找不到商品数据";
                            else
                                goodsname = entgroupgoods.name;
                            break;
                        case (int)EntGoodsType.普通产品:
                            EntGoods entgoods = EntGoodsBLL.SingleModel.GetModel(goodsid);
                            if (entgoods == null)
                                msg = "找不到商品数据";
                            else
                                goodsname = entgoods.name;
                            break;
                        case (int)EntGoodsType.砍价产品:
                            Bargain bargain = BargainBLL.SingleModel.GetModel(goodsid);
                            if (bargain == null)
                                msg = "找不到商品数据";
                            else
                                goodsname = bargain.BName;
                            break;
                    }
                    break;
                case (int)TmpType.小未平台子模版:
                    PlatChildGoods platchildgoods = PlatChildGoodsBLL.SingleModel.GetModel(goodsid);
                    if (platchildgoods == null)
                        msg = "找不到商品数据";
                    else
                        goodsname = platchildgoods.Name;
                    break;
            }

            return goodsname;
        }


        /// <summary>
        /// 是否已评论
        /// </summary>
        /// <param name="xcxtemplateType">模板类型</param>
        /// <param name="goodsType">商品类型</param>
        /// <param name="goodsId">商品ID（拼团entgoods表ID，团购groups表ID，普通商品entgoods表ID，砍价bargain表ID）</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool IsComment(int xcxtemplateType, int goodsType, int goodsId,int orderId,int aid)
        {
            bool iscomment = false;
            switch (xcxtemplateType)
            {
                case (int)TmpType.小程序专业模板:
                    switch (goodsType)
                    {
                        case (int)EntGoodsType.团购商品:
                            GroupUser groupuser = GroupUserBLL.SingleModel.GetModel(orderId);
                            if (groupuser == null)
                                return true;
                            iscomment = ExitModel(aid,groupuser.ObtainUserId,groupuser.GroupId,orderId,goodsType);
                            groupuser.IsCommentting = true;
                            GroupUserBLL.SingleModel.Update(groupuser, "IsCommentting");
                            break;

                        case (int)EntGoodsType.拼团产品:
                            EntGoodsCart gentgoodscart = EntGoodsCartBLL.SingleModel.GetModelByGoodsId(orderId,goodsId);
                            if (gentgoodscart == null)
                                return true;
                            iscomment = ExitModel(aid, gentgoodscart.UserId, goodsId, orderId, goodsType);
                            gentgoodscart.IsCommentting = true;
                            EntGoodsCartBLL.SingleModel.Update(gentgoodscart, "IsCommentting");
                            break;

                        case (int)EntGoodsType.普通产品:
                            EntGoodsCart entgoodscart = EntGoodsCartBLL.SingleModel.GetModelByGoodsId(orderId, goodsId);
                            if (entgoodscart == null)
                                return true;
                            iscomment = ExitModel(aid, entgoodscart.UserId, goodsId, orderId, goodsType);
                            entgoodscart.IsCommentting = true;
                            EntGoodsCartBLL.SingleModel.Update(entgoodscart, "IsCommentting");
                            break;

                        case (int)EntGoodsType.砍价产品:
                            BargainUser bargainuser = BargainUserBLL.SingleModel.GetModel(orderId);
                            if (bargainuser == null)
                                return true;
                            iscomment = ExitModel(aid, bargainuser.UserId, bargainuser.BId, orderId, goodsType);
                            bargainuser.IsCommentting = true;
                            BargainUserBLL.SingleModel.Update(bargainuser, "IsCommentting");
                            break;
                    }
                    break;

                case (int)TmpType.小未平台子模版:
                    
                    PlatChildGoodsCart platChildGoodsCart = PlatChildGoodsCartBLL.SingleModel.GetModelByGoodsId(orderId, goodsId);
                    if (platChildGoodsCart == null)
                        return true;
                    iscomment = ExitModel(aid, platChildGoodsCart.UserId, goodsId, orderId, goodsType);
                    platChildGoodsCart.IsCommentting = true;
                    PlatChildGoodsCartBLL.SingleModel.Update(platChildGoodsCart, "IsCommentting");
                    break;
            }

            return iscomment;
        }

        /// <summary>
        /// 标记订单已评论
        /// </summary>
        /// <param name="xcxtemplateType">模板类型</param>
        /// <param name="goodsType">商品类型</param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool OrderCommenting(int xcxtemplateType, int goodsType, int orderId)
        {
            switch (xcxtemplateType)
            {
                case (int)TmpType.小程序专业模板:
                    switch (goodsType)
                    {
                        case (int)EntGoodsType.拼团产品:
                        case (int)EntGoodsType.普通产品:
                            EntGoodsOrder entOrder = EntGoodsOrderBLL.SingleModel.GetModel(orderId);
                            if (entOrder == null)
                                return false;
                            entOrder.IsCommentting = true;
                            return EntGoodsOrderBLL.SingleModel.Update(entOrder, "IsCommentting");

                        case (int)EntGoodsType.砍价产品:
                            BargainUser bargainuser = BargainUserBLL.SingleModel.GetModel(orderId);
                            if (bargainuser == null)
                                return true;
                            bargainuser.IsCommentting = true;
                            return BargainUserBLL.SingleModel.Update(bargainuser, "IsCommentting");
                    }
                    break;

                case (int)TmpType.小未平台子模版:
                    PlatChildGoodsOrder platOrder = PlatChildGoodsOrderBLL.SingleModel.GetModel(orderId);
                    if (platOrder == null)
                        return false;
                    platOrder.IsCommentting = true;
                    return PlatChildGoodsOrderBLL.SingleModel.Update(platOrder, "IsCommentting");
            }

            return false;
        }

        public int AddComment(int aid,C_UserInfo userinfo,int goodtype,int goodsid,string goodname,int price,string imgurl,int orderid,string specinfo,bool isdefalut=true,int haveImg=0)
        {
            GoodsComment commentmodel = new GoodsComment();
            commentmodel.AddTime = DateTime.Now;
            commentmodel.AId = aid;
            commentmodel.Anonymous = true;
            commentmodel.Comment = "";
            commentmodel.Hidden = false;
            commentmodel.NickName = userinfo.NickName;
            commentmodel.UserId = userinfo.Id;
            commentmodel.Praise = 2;
            commentmodel.LogisticsScore = 5;
            commentmodel.ServiceScore = 5;
            commentmodel.DescriptiveScore = 5;
            commentmodel.State = 1;
            commentmodel.Type = goodtype;
            commentmodel.Points = 1;
            commentmodel.UpdateTime = DateTime.Now;
            commentmodel.GoodsId = goodsid;
            commentmodel.GoodsName = goodname;
            commentmodel.GoodsPrice = price;
            commentmodel.GoodsImg = imgurl;
            commentmodel.OrderId = orderid;
            commentmodel.GoodsSpecification = specinfo;
            commentmodel.HaveImg = haveImg;
            commentmodel.IsDefault = isdefalut;
            commentmodel.Id = Convert.ToInt32(SingleModel.Add(commentmodel));

            return commentmodel.Id;
        }

        #region 自动评论服务
        /// <summary>
        /// 独立小程序：15天后订单商品自动评论
        /// </summary>
        public void StartPlatChildGoodsCommentServer(int timelength)
        {
            List<PlatChildGoodsCart> entgoodscartlist = PlatChildGoodsCartBLL.SingleModel.GetSuccessDataList(0,timelength);
            if (entgoodscartlist != null && entgoodscartlist.Count > 0)
            {
                string userids = string.Join(",", entgoodscartlist.Select(s => s.UserId).Distinct());
                List<C_UserInfo> userinfolist = C_UserInfoBLL.SingleModel.GetListByIds(userids);
                userinfolist = userinfolist == null ? new List<C_UserInfo>() : userinfolist;

                string aids = string.Join(",", entgoodscartlist.Select(s => s.AId).Distinct());
                List<XcxAppAccountRelation> xcxrelationlist = XcxAppAccountRelationBLL.SingleModel.GetListByIds(aids);
                xcxrelationlist = xcxrelationlist == null ? new List<XcxAppAccountRelation>() : xcxrelationlist;

                string goodsids = string.Join(",", entgoodscartlist.Select(s => s.GoodsId).Distinct());
                List<PlatChildGoods> goodslist = PlatChildGoodsBLL.SingleModel.GetListByIds(goodsids);
                goodslist = goodslist == null ? new List<PlatChildGoods>() : goodslist;

                foreach (PlatChildGoodsCart itemcart in entgoodscartlist)
                {
                    C_UserInfo userinfo = userinfolist?.FirstOrDefault(f => f.Id == itemcart.UserId);
                    XcxAppAccountRelation xcxrelation = xcxrelationlist?.FirstOrDefault(f => f.Id == itemcart.AId);
                    PlatChildGoods good = goodslist?.FirstOrDefault(f => f.Id == itemcart.GoodsId);
                    string imgurl = string.IsNullOrEmpty(itemcart.SpecImg) ? good.Img : itemcart.SpecImg;

                    itemcart.IsCommentting = true;
                    PlatChildGoodsCartBLL.SingleModel.Update(itemcart, "IsCommentting");
                    SingleModel.AddComment(xcxrelation.Id, userinfo, 0, itemcart.GoodsId, itemcart.GoodsName, itemcart.Price, imgurl, itemcart.OrderId, itemcart.SpecInfo);
                }
            }
        }

        /// <summary>
        /// 专业版普通订单：15天后订单商品自动评论
        /// </summary>
        public void StartEntGoodsCommentServer(int timelength)
        {
            List<EntGoodsCart> entgoodscartlist = EntGoodsCartBLL.SingleModel.GetSuccessDataList(0, timelength);
            if (entgoodscartlist != null && entgoodscartlist.Count > 0)
            {
                string userids = string.Join(",", entgoodscartlist.Select(s => s.UserId).Distinct());
                List<C_UserInfo> userinfolist = C_UserInfoBLL.SingleModel.GetListByIds(userids);
                userinfolist = userinfolist == null ? new List<C_UserInfo>() : userinfolist;

                string aids = string.Join(",", entgoodscartlist.Select(s => s.aId).Distinct());
                List<XcxAppAccountRelation> xcxrelationlist = XcxAppAccountRelationBLL.SingleModel.GetListByIds(aids);
                xcxrelationlist = xcxrelationlist == null ? new List<XcxAppAccountRelation>() : xcxrelationlist;

                string goodsids = string.Join(",", entgoodscartlist.Select(s => s.FoodGoodsId).Distinct());
                List<EntGoods> entgoodslist = EntGoodsBLL.SingleModel.GetListByIds(goodsids);
                entgoodslist = entgoodslist == null ? new List<EntGoods>() : entgoodslist;

                foreach (EntGoodsCart itemcart in entgoodscartlist)
                {
                    C_UserInfo userinfo = userinfolist?.FirstOrDefault(f => f.Id == itemcart.UserId);
                    XcxAppAccountRelation xcxrelation = xcxrelationlist?.FirstOrDefault(f => f.Id == itemcart.aId);
                    EntGoods entgood = entgoodslist?.FirstOrDefault(f => f.id == itemcart.FoodGoodsId);
                    string imgurl = string.IsNullOrEmpty(itemcart.SpecImg) ? entgood.img : itemcart.SpecImg;

                    itemcart.IsCommentting = true;
                    EntGoodsCartBLL.SingleModel.Update(itemcart, "IsCommentting");
                    SingleModel.AddComment(xcxrelation.Id, userinfo, entgood.goodtype, itemcart.FoodGoodsId, itemcart.GoodName, itemcart.Price, imgurl, itemcart.GoodsOrderId, itemcart.SpecInfo);
                }
            }
        }

        /// <summary>
        /// 团购订单：15天后订单商品自动评论
        /// </summary>
        public void StartGroupGoodsCommentServer(int timelength)
        {
            List<GroupUser> groupuserlist = GroupUserBLL.SingleModel.GetSuccessDataList(0, timelength);
            if (groupuserlist != null && groupuserlist.Count > 0)
            {
                string userids = string.Join(",", groupuserlist.Select(s => s.ObtainUserId).Distinct());
                List<C_UserInfo> userinfolist = C_UserInfoBLL.SingleModel.GetListByIds(userids);
                userinfolist = userinfolist == null ? new List<C_UserInfo>() : userinfolist;

                string aids = string.Join(",", groupuserlist.Select(s => s.AId).Distinct());
                List<XcxAppAccountRelation> xcxrelationlist = XcxAppAccountRelationBLL.SingleModel.GetListByIds(aids);
                xcxrelationlist = xcxrelationlist == null ? new List<XcxAppAccountRelation>() : xcxrelationlist;

                foreach (GroupUser itemgroup in groupuserlist)
                {
                    C_UserInfo userinfo = userinfolist?.FirstOrDefault(f => f.Id == itemgroup.ObtainUserId);
                    XcxAppAccountRelation xcxrelation = xcxrelationlist?.FirstOrDefault(f => f.Id == itemgroup.AId);
                    string imgurl = itemgroup.GroupImgUrl;

                    itemgroup.IsCommentting = true;
                    GroupUserBLL.SingleModel.Update(itemgroup, "IsCommentting");
                    SingleModel.AddComment(xcxrelation.Id, userinfo, (int)EntGoodsType.团购商品, itemgroup.GroupId, itemgroup.GroupName, itemgroup.BuyPrice, imgurl, itemgroup.Id, "");
                }
            }
        }
        /// <summary>
        /// 团购订单：15天后订单商品自动评论
        /// </summary>
        public void StartBargainGoodsCommentServer(int timelength)
        {
            List<BargainUser> bargainuserlist = BargainUserBLL.SingleModel.GetSuccessDataList(0, timelength);
            if (bargainuserlist != null && bargainuserlist.Count > 0)
            {
                string userids = string.Join(",", bargainuserlist.Select(s => s.UserId).Distinct());
                List<C_UserInfo> userinfolist = C_UserInfoBLL.SingleModel.GetListByIds(userids);
                userinfolist = userinfolist == null ? new List<C_UserInfo>() : userinfolist;

                string aids = string.Join(",", bargainuserlist.Select(s => s.aid).Distinct());
                List<XcxAppAccountRelation> xcxrelationlist = XcxAppAccountRelationBLL.SingleModel.GetListByIds(aids);
                xcxrelationlist = xcxrelationlist == null ? new List<XcxAppAccountRelation>() : xcxrelationlist;

                foreach (BargainUser itembargain in bargainuserlist)
                {
                    C_UserInfo userinfo = userinfolist?.FirstOrDefault(f => f.Id == itembargain.UserId);
                    XcxAppAccountRelation xcxrelation = xcxrelationlist?.FirstOrDefault(f => f.Id == itembargain.aid);
                    string imgurl = itembargain.ImgUrl;

                    itembargain.IsCommentting = true;
                    BargainUserBLL.SingleModel.Update(itembargain, "IsCommentting");
                    SingleModel.AddComment(xcxrelation.Id, userinfo, (int)EntGoodsType.砍价产品, itembargain.BId, itembargain.BName, itembargain.CurrentPrice, imgurl, itembargain.Id, "");
                }
            }
        }
        #endregion
    }
}
