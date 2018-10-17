using Api.MiniApp.Filters;
using BLL.MiniApp;
using BLL.MiniApp.Pin;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Http;
using Api.MiniApp.Models.Pin;
using static Entity.MiniApp.Pin.PinEnums;
using Newtonsoft.Json;
using System.Linq;
using BLL.MiniApp.Conf;
using Entity.MiniApp.Conf;
using BLL.MiniApp.Ent;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Tools;
using BLL.MiniApp.Tools;
using Entity.MiniApp.Im;
using BLL.MiniApp.Im;

namespace Api.MiniApp.Controllers
{
    /// <summary>
    /// 拼享惠
    /// </summary>
    [ApiAuthCheck]
    public class apiPinController : BaseController
    {
        #region 产品

        /// <summary>
        /// 获取产品列表
        /// </summary>
        /// <param name="utoken">utoken</param>
        /// <param name="aId"></param>
        /// <param name="storeId">店铺ID，默认0</param>
        /// <param name="cateIdOne">一级分类ID，默认0</param>
        /// <param name="cateId">二级分类ID，默认0</param>
        /// <param name="pageSize">每页显示条数，默认20</param>
        /// <param name="pageIndex">第几页，默认1</param>
        /// <param name="kw">按产品名称模糊查询</param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg GoodList(string utoken, int aId = 0, int storeId = 0, int cateIdOne = 0, int cateId = 0, int pageSize = 20, int pageIndex = 1, string kw = "", int storeOwner = 0)
        {
            pageIndex = pageIndex - 1;
            if (pageIndex < 0)
                pageIndex = 0;

            if (storeId > 0)
            {
                PinStore storeModel = PinStoreBLL.SingleModel.GetModel(storeId);
                if (storeModel == null || !storeModel.isAvailable)
                {
                    result.msg = "门店不可用";
                    return result;
                }
            }
            if (aId <= 0)
            {
                result.msg = "非法请求";
                return result;
            }
            
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            StringBuilder filterSql = new StringBuilder();

            filterSql.Append($" and g.aid = {aId} ");

            if (storeId > 0)
                filterSql.Append($" and g.storeId = {storeId} ");

            if (cateIdOne > 0)
                filterSql.Append($" and g.cateIdOne = {cateIdOne} ");

            if (cateId > 0)
                filterSql.Append($" and g.cateId={cateId} ");

            if (!string.IsNullOrEmpty(kw))
            {
                filterSql.Append(" and g.name like @kw ");
                parameters.Add(new MySqlParameter("@kw", Utils.FuzzyQuery(kw)));
            }
            if (storeOwner == 1)
            {
                filterSql.Append(" and g.state<>-1 ");
            }
            else
            {
                filterSql.Append(" and g.state=1 and g.auditState=1 ");
            }

            List<PinGoods> list = PinGoodsBLL.SingleModel.GetListBySql($" SELECT g.* from PinGoods g inner join pinstore  s on g.storeId=s.id where s.state=1 and s.startdate<NOW() and s.enddate>NOW()  {filterSql} and g.cateidone not in(SELECT id from pincategory where state=-1 and fid=0 and aid={aId}) order by g.indexrank desc,g.id desc limit {pageSize * pageIndex},{pageSize}", parameters.ToArray());
            list?.ForEach(p =>
            {
                p.pinGoodsCount = orderBLL.GetPinGoodsCount(p.id);
            });
            result.code = 1;
            result.obj = list;
            return result;
        }

        /// <summary>
        /// 获取单个产品详情
        /// </summary>
        /// <param name="utoken">utoken</param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg GoodInfo(string utoken, int id = 0)
        {
            if (id <= 0)
            {
                result.msg = "非法请求";
                return result;
            }


            PinGoods model = PinGoodsBLL.SingleModel.GetModel(id);
            if (model == null)
            {
                result.msg = "产品不存在";
                return result;
            }
            if (model.state == -1)
            {
                result.msg = "产品已删除";
                return result;
            }
            if (model.auditState == (int)GoodsAuditState.已拒绝)
            {
                result.code = -1;
                result.msg = "该商品未通过审核，去看看其他商品吧";
                return result;
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            //TODO 由于产品管理和产品展示用的同一个接口，导致下架的产品不能编辑，这里判断一下，如果当前产品属于当前用户店铺的就不再判断是否下架
            if (model.state == 0)
            {
                PinStore store = PinStoreBLL.SingleModel.GetModelByAid_UserId(model.aId, userInfo.Id);
                if (store == null)
                {
                    result.msg = "产品已下架";
                    return result;
                }
            }



            PinLikes likesModel = PinLikesBLL.SingleModel.GetModel($"aid={model.aId} and userid={userInfo.Id} and type=0 and likeid={id}");
            if (likesModel == null)
            {
                likesModel = new PinLikes
                {
                    id = 0,
                    aId = model.aId,
                    type = 0,
                    likeId = model.id,
                    userId = userInfo.Id,
                };
            }
            //if (model != null)
            //{
            //    model.labelList?.ForEach(p=> {

            //    });
            //}

            result.code = 1;
            result.obj = new
            {
                goodInfo = model,
                pinGoodsCount = orderBLL.GetPinGoodsCount(model.id),
                pinUserCount = orderBLL.GetPinUserCount(model.id),
                goodLikesInfo = new
                {
                    likesModel.id,
                    likesModel.aId,
                    likesModel.type,
                    likesModel.userId
                },
            };
            return result;
        }

        /// <summary>
        /// 获取所有产品分类
        /// </summary>
        /// <param name="utoken">utoken</param>
        /// <param name="aId"></param>
        /// <param name="storeId">storeId=0查询所有分类，storeId>0查询当前店铺产品使用到的分类</param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg CategoryList(string utoken, int aId = 0, int storeId = 0)
        {
            if (aId <= 0)
            {
                result.msg = "非法请求";
                return result;
            }
            List<PinCategory> list = null;

            if (storeId == 0)
                list = PinCategoryBLL.SingleModel.GetAllCategory(aId);
            else
                list = PinCategoryBLL.SingleModel.GetStoreCategory(aId, storeId);

            result.code = 1;
            result.obj = list;
            return result;
        }

        /// <summary>
        /// 添加，修改产品
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg EditGoods(string utoken, dynamic postData)
        {
            if (postData == null)
            {
                result.msg = "非法请求";
                return result;
            }


            if (postData.id == 0)
            {
                int goodsid = Convert.ToInt32(PinGoodsBLL.SingleModel.Add(postData));
                if (goodsid > 0)
                {
                    result.code = 1;
                    result.msg = "添加成功";
                }
                else
                {
                    result.msg = "添加失败";
                }
            }
            else
            {
                int goodsId = postData.id;
                PinGoods goodsModel = PinGoodsBLL.SingleModel.GetModel(goodsId);
                if (goodsModel == null || goodsModel.state == -1)
                {
                    result.msg = "产品不存在或已删除";
                    return result;
                }
                goodsModel.state = postData.state;
                if (goodsModel.state == 0)
                    goodsModel.auditState = (int)GoodsAuditState.待审核;

                if (PinGoodsBLL.SingleModel.Update(goodsModel))
                {
                    result.code = 1;
                    result.msg = "更新成功";


                }
                else
                {
                    result.msg = "更新失败";
                }
            }
            return result;
        }

        #endregion 产品

        #region 店铺

        /// <summary>
        /// 店铺详情
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="id">店铺ID</param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg StoreInfo(string utoken, int id)
        {
            if (id <= 0)
            {
                result.msg = "非法请求";
                return result;
            }

            PinStore store = PinStoreBLL.SingleModel.GetModel(id);
            DateTime now = DateTime.Now;
            if (store == null || !store.isAvailable)
            {
                result.msg = "门店不可用";
                return result;
            }
            if (store.agentId > 0)
            {
                store.fuserInfo = PinAgentBLL.SingleModel.GetUserInfoByAgentId(store.agentId);
            }
            C_UserInfo userInfo = GetUserInfo(utoken);

            PinLikes likesModel = PinLikesBLL.SingleModel.GetModel($"aid={store.aId} and userid={userInfo.Id} and type=1 and likeid={id}");
            if (likesModel == null)
            {
                likesModel = new PinLikes
                {
                    id = 0,
                    aId = store.aId,
                    type = 1,
                    likeId = store.id,
                    userId = userInfo.Id,
                };
            }

            C_UserInfo storeUser = C_UserInfoBLL.SingleModel.GetModel(store.userId);
            store.kfUserInfo = storeUser;

            result.code = 1;
            result.obj = new
            {
                storeInfo = store,
                goodsCount = orderBLL.GetStoreGoodsCount(store.aId, store.id),
                pinGoodsCount = orderBLL.GetStorePinGoodsCount(store.id),
                storeLikesInfo = new
                {
                    likesModel.id,
                    likesModel.aId,
                    likesModel.type,
                    likesModel.userId,
                },
            };
            return result;
        }

        /// <summary>
        /// 获取评论列表
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="pid">产品ID，查询产品全部评价时必传</param>
        /// <param name="pageSize">默认显示2条</param>
        /// <param name="pageIndex"></param>
        /// <param name="isImg">0=全部，1=有图</param>
        /// <param name="userId">查询我的评价时必传</param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg CommentList(string utoken, int pid = 0, int pageSize = 2, int pageIndex = 1, int isImg = 0, int userId = 0)
        {
            string filterSql = $" state=1 ";
            string filterImgSql = "";
            if (pid > 0)
            {
                filterSql += $" and goodsid={pid} ";
            }
            if (isImg > 0)
            {
                filterImgSql += " and Imgs<>'' ";
            }
            if (userId > 0)
            {
                filterSql += $" and userid={userId} ";
            }
            C_UserInfo reqUserInfo = GetUserInfo(utoken);
            List<PinComment> list = PinCommentBLL.SingleModel.GetList(filterSql + filterImgSql, pageSize, pageIndex, "*", "id desc");

            string userIds = string.Join(",",list?.Select(s=>s.UserId).Distinct());
            List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

            string goodsIds = userId > 0 ? string.Join(",",list?.Select(s=>s.GoodsId).Distinct()) : "";
            List<PinGoods> pinGoodsList = PinGoodsBLL.SingleModel.GetListByIds(goodsIds);
            
            list?.ForEach(p =>
            {
                C_UserInfo userInfo = userInfoList?.FirstOrDefault(f=>f.Id == p.UserId);
                if (userInfo != null)
                {
                    p.UserPhoto = userInfo.HeadImgUrl;
                    p.NickName = userInfo.NickName;
                }
                p.AttrName = orderBLL.GetModel(p.OrderId)?.specificationPhotoModel?.name;
                if (userId > 0)
                {
                    p.Goods = pinGoodsList?.FirstOrDefault(f=>f.id == p.GoodsId);
                }
            });
            int count = PinCommentBLL.SingleModel.GetCount(filterSql);
            int countImgs = PinCommentBLL.SingleModel.GetCount(filterSql + " and Imgs<>'' ");
            result.code = 1;
            result.obj = new
            {
                count,
                countImgs,
                list
            };
            return result;
        }


        /// <summary>
        /// 提交评论
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg PostComment(string utoken, [FromBody] PinComment model)
        {
            if (!ModelState.IsValid)
            {
                result.msg = this.ErrorMsg();
                return result;
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            PinGoodsOrderBLL orderBLL = new PinGoodsOrderBLL();
            PinGoodsOrder order = orderBLL.GetModel(model.OrderId);
            if (order == null || order.userId != userInfo.Id)
            {
                result.msg = "非法操作";
                return result;
            }
            if (order.state == (int)PinOrderState.已评价)
            {
                result.msg = "不能重复评价";
                return result;
            }
            bool commentResult = PinCommentBLL.SingleModel.AddComment(model, order);
            if (commentResult)
            {
                result.code = 1;
                result.msg = "评价成功";
                return result;
            }

            result.msg = "评价失败";
            return result;
        }
        /// <summary>
        /// 申请入驻，用户需要先绑定手机号码，一个用户只能开通一个店铺， 注意参数大小写
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">
        /// {
        ///     aId:0
        /// }
        /// </param>
        /// <returns>返回开通的店铺ID</returns>
        [HttpPost]
        public ReturnMsg ApplyIn(string utoken, dynamic postData)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo.IsValidTelePhone == 0)
            {
                result.code = 0;
                result.msg = "请先绑定手机号码";
                return result;
            }
            int aid = postData.aId;
            if (aid <= 0)
            {
                result.code = 0;
                result.msg = "参数错误 aid error";
                return result;
            }
            //if (string.IsNullOrEmpty(userInfo.NickName) || string.IsNullOrEmpty(userInfo.HeadImgUrl))
            //{
            //    result.code = 0;
            //    result.msg = "请先登录";
            //    return result;
            //}
            int fuserId = postData.fuserId;
            int agentId = 0;
            if (fuserId > 0)
            {
                
                PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(fuserId);
                if (agent == null)
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception($"代理邀请入驻错误，找不到代理信息 userid:{fuserId}"));
                }
                else
                {
                    agentId = agent.id;
                }
            }
            if (userInfo.StoreId != 0)
            {
                result.code = 1;
                result.msg = "您已开通店铺";
                result.obj = userInfo.StoreId;
                return result;
            }

            PinStore store = PinStoreBLL.SingleModel.GetModel($"loginName=@loginName and state>-1 and aid={aid}", new MySqlParameter[] {
                new MySqlParameter("@loginName",userInfo.TelePhone)
            });
            if (store != null)
            {
                result.code = 1;
                result.msg = "店铺已存在";
                result.obj = store.id;
                return result;
            }
            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid((int)postData.aId);
            if (platform == null)
            {
                result.code = 0;
                result.msg = "平台信息错误";
                return result;

            }

            PinStore storeInfo = new PinStore
            {
                aId = postData.aId,
                rz = 1,
                state = 1,
                endDate = DateTime.Now.AddDays(platform.freeDays),
                loginName = userInfo.TelePhone,
                password = Utility.DESEncryptTools.GetMd5Base32("123456"),
                startDate = DateTime.Now,
                userId = userInfo.Id,
                phone = userInfo.TelePhone,
                logo = userInfo.HeadImgUrl,
                storeName = userInfo.NickName,
                agentId = agentId
            };
            int storeId = PinStoreBLL.SingleModel.OpenStore(userInfo, storeInfo);
            if (storeId > 0)
            {
                result.code = 1;
                result.msg = "开通成功";
                result.obj = storeId;
            }
            else
            {
                result.msg = "开通失败";
                result.obj = 0;
            }
            return result;

        }

        /// <summary>
        /// 获取用户店铺
        /// </summary>
        /// <param name="utoken"></param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg UserStore(string utoken)
        {
            C_UserInfo userInfo = (C_UserInfo)RequestContext.RouteData.Values["userInfo"];

            PinStore storeModel = PinStoreBLL.SingleModel.GetModel($"userid={userInfo.Id}");
            if (PinStoreBLL.SingleModel.IsAvailable(storeModel))
            {
                result.code = 1;
                result.obj = storeModel;
            }
            else
            {
                result.code = 0;
                result.msg = "店铺不可用";
            }
            return result;
        }

        /// <summary>
        /// 编辑门店信息
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">
        /// id必须大于0
        /// {
        ///     id:0,
        ///     logo:"",
        ///     loginName:"",
        ///     phone:"",
        /// }
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg EditStore(string utoken, dynamic postData)
        {
            if (postData == null)
            {
                result.msg = "非法请求";
                return result;
            }
            C_UserInfo userInfo = (C_UserInfo)RequestContext.RouteData.Values["userInfo"];
            int storeId = Convert.ToInt32(postData.id);
            PinStore storeInfo = PinStoreBLL.SingleModel.GetModel(storeId);
            if (storeInfo == null || storeInfo.userId != userInfo.Id)
            {
                result.msg = "非法操作";
                return result;
            }
            if (!storeInfo.isAvailable)
            {
                result.msg = "店铺不可用";
                return result;
            }
            storeInfo.logo = postData.logo;
            storeInfo.storeName = postData.storeName;
            storeInfo.phone = postData.phone;
            if (PinStoreBLL.SingleModel.Update(storeInfo, "logo,storeName,phone"))
            {
                result.code = 1;
                result.msg = "保存成功";
            }
            else
            {
                result.msg = "保存失败";
            }
            return result;
        }
        /// <summary>
        /// 申请标杆店铺
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">{aid:aid}</param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg ApplyBiaogan(string utoken, dynamic postData)
        {
            if (postData == null)
            {
                result.msg = "非法请求";
                return result;
            }
            int aid = postData.aid;
            C_UserInfo userInfo = (C_UserInfo)RequestContext.RouteData.Values["userInfo"];
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_UserId(aid, userInfo.Id);
            if (store == null)
            {
                result.msg = "店铺不存在";
                return result;
            }
            if (store.biaogan == (int)BiaoGanState.申请中)
            {
                result.msg = "您的申请已提交，请不要重复提交";
                return result;
            }
            if (store.biaogan == (int)BiaoGanState.申请成功)
            {
                result.msg = "您已成为标杆店铺";
                return result;
            }
            store.biaogan = 1;
            result.code = PinStoreBLL.SingleModel.Update(store, "biaogan") ? 1 : 0;
            result.msg = result.code > 0 ? "您的申请提交，后台加速审核中" : "申请失败";
            return result;
        }
        /// <summary>
        /// 获取标杆店铺列表
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize">默认10条数据</param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg BiaoganStoreList(string utoken, int aid, int pageIndex = 1, int pageSize = 10)
        {
            if (aid <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcx == null)
            {
                result.msg = "小程序不存在";
                return result;
            }
            int recordCount = 0;
            List<PinStore> storeList = PinStoreBLL.SingleModel.GetListByCondition(xcx.AppId, aid, pageIndex, pageSize, out recordCount, biaogan: 2);
            List<object> list = new List<object>();
            if (storeList != null && storeList.Count > 0)
            {
                
                foreach (var store in storeList)
                {
                    store.goodsCount = PinGoodsBLL.SingleModel.GetCountByStoreId(store.id);
                    var obj = new
                    {
                        store.goodsCount,
                        store.id,
                        store.logo,
                        store.storeName
                    };
                    list.Add(obj);
                }
            }
            result.code = 1;
            result.obj = new { list };
            return result;
        }
        #endregion 店铺

        #region 收货地址

        /// <summary>
        /// 添加、修改用户收货地址，将地址设为默认地址
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="model">地址对象： model.id=0表示添加，model.id>0表示修改</param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg AddAddress(string utoken, [FromUri] PinUserAddress model)
        {
            if (!ModelState.IsValid)
            {
                result.code = 0;
                result.msg = this.ErrorMsg();
            }
            else
            {
                if (model == null)
                {
                    result.code = 0;
                    result.msg = "参数错误";
                    return result;
                }

                
                C_UserInfo userInfo = GetUserInfo(utoken);
                if (model.id == 0)
                {
                    model.userId = userInfo.Id;
                    int count = PinUserAddressBLL.SingleModel.GetCount($"userid={userInfo.Id} and state=1");
                    if (count == 0)
                        model.isDefault = 1;
                    else
                        model.isDefault = 0;

                    int newid = Convert.ToInt32(PinUserAddressBLL.SingleModel.Add(model));
                    if (newid > 0)
                    {
                        result.code = 1;
                        result.msg = "添加成功";
                    }
                    else
                        result.msg = "添加失败";
                }
                else
                {

                    model.updateTime = DateTime.Now;
                    bool updateResult = PinUserAddressBLL.SingleModel.Update(model);
                    result.code = updateResult ? 1 : 0;
                    result.msg = updateResult ? "设置成功" : "设置成功";

                    if (model.isDefault == 1)
                    {
                        PinUserAddressBLL.SingleModel.setDefaultAddress(model);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取用户所有收货地址
        /// </summary>
        /// <param name="utoken"></param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg AddressList(string utoken)
        {
            
            C_UserInfo userInfo = GetUserInfo(utoken);
            result.code = 1;
            result.obj = PinUserAddressBLL.SingleModel.GetList($"userid={userInfo.Id} and state=1");
            return result;
        }

        /// <summary>
        /// 删除用户收货地址
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg DeleteAddress(string utoken, int id)
        {
            if (id <= 0)
            {
                result.msg = "请求参数错误";
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            
            PinUserAddress model = PinUserAddressBLL.SingleModel.GetModel(id);
            if (model == null)
            {
                result.msg = "收货地址不存";
            }
            else if (model.state != 1)
            {
                result.msg = "收货地址已删除";
            }
            else if (model.userId != userInfo.Id)
            {
                result.msg = "非法操作";
            }
            else
            {
                model.state = -1;
                bool updateResult = PinUserAddressBLL.SingleModel.Update(model, "state");
                result.code = updateResult ? 1 : 0;
                result.msg = updateResult ? "删除成功" : "删除失败";
            }
            return result;
        }
        /// <summary>
        /// 自取门店列表
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg ZqStoreList(string utoken, int aid = 0, int storeId = 0)
        {
            if (aid <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            if (storeId <= 0)
            {
                result.code = 1;
                result.obj = new { placeList = new List<PickPlace>() };
                return result;
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            
            //获取门店自提地点
            List<PickPlace> placeList = PickPlaceBLL.SingleModel.GetListByAid_StoreId(aid, storeId) ?? new List<PickPlace>();
            result.code = 1;
            result.obj = new { placeList };
            return result;
        }
        #endregion 收货地址

        #region 收藏

        /// <summary>
        /// 获取我的收藏
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aId"></param>
        /// <param name="type">收藏类型，0=产品，1=店铺</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg LikesList(string utoken, int aId, int type, int pageSize = 20, int pageIndex = 1)
        {
            if (pageIndex == 1)
                pageIndex = 0;

            if (aId <= 0 || type < 0)
            {
                result.msg = "aId不能小于0，type不能小于0";
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            List<PinLikes> list = PinLikesBLL.SingleModel.GetList($"aid={aId} and type={type} and userid={userInfo.Id}", pageSize, pageIndex, "*", "id desc");
            //收藏产品
            if (type == 0)
            {
                string goodsIds = string.Join(",",list?.Select(s=>s.likeGood));
                List<PinGoods> pinGoodsList = PinGoodsBLL.SingleModel.GetListByIds(goodsIds);
                list?.ForEach(p =>
                {
                    PinGoods good = pinGoodsList?.FirstOrDefault(f=>f.id == p.likeId);
                    if (good != null && good.state == 1)
                        p.likeGood = good;
                });
            }
            //收藏店铺
            else if (type == 1)
            {
                string storeIds = string.Join(",",list?.Select(s=>s.likeId));
                List<PinStore> pinStoreList = PinStoreBLL.SingleModel.GetListByIds(storeIds);
                DateTime now = DateTime.Now;
                list?.ForEach(p =>
                {
                    PinStore store = pinStoreList?.FirstOrDefault(f=>f.id == p.likeId);
                    if (store != null && store.state == 1 && now > store.startDate && now < store.endDate)
                        p.likeStore = store;
                });
            }
            result.code = 1;
            result.obj = list;
            return result;
        }

        /// <summary>
        /// 添加收藏,取消搜藏
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="model">收藏对象，当id=0 表示收藏，当id>0，表示取消收藏</param>
        /// <returns>添加时返回 收藏id</returns>
        [HttpPost]
        public ReturnMsg AddLikes(string utoken, [FromBody] PinLikesModel model)
        {
            if (model == null || model.aId <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            PinLikes likeModel = new PinLikes
            {
                userId = userInfo.Id,
                aId = model.aId,
                likeId = model.likeId,
                type = model.type,
            };
            if (model.id == 0)
            {
                PinLikes oldLikeModel = PinLikesBLL.SingleModel.GetModel($"aid={model.aId} and userid={userInfo.Id} and type={model.type} and likeid={model.likeId}");

                if (oldLikeModel != null)
                {
                    result.obj = oldLikeModel.id;
                    result.msg = "收藏成功";
                    return result;
                }

                int newid = Convert.ToInt32(PinLikesBLL.SingleModel.Add(likeModel));
                if (newid > 0)
                {
                    result.code = 1;
                    result.msg = "收藏成功";
                    result.obj = newid;
                }
                else
                    result.msg = "收藏失败";
            }
            else
            {
                PinLikesBLL.SingleModel.Delete(model.id);
                result.code = 1;
                result.msg = "设置成功";
                result.obj = 0;
            }
            return result;
        }

        #endregion 收藏

        #region 下单

        /// <summary>
        /// 创建订单 utoken跟在链接后面
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg AddOrder(string utoken, [FromBody] PinOrderModel orderModel)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            if (!ModelState.IsValid)
            {
                result.msg = this.ErrorMsg();
                return result;
            }
            if (orderModel == null)
            {
                result.msg = "实例错误";
                return result;
            }
            if (orderModel.sendway == (int)SendWay.到店自取 || orderModel.sendway == (int)SendWay.商家配送)
            {
                if (string.IsNullOrEmpty(orderModel.consignee))
                {
                    result.msg = "请填写收件人";
                    return result;
                }
                if (string.IsNullOrEmpty(orderModel.phone))
                {
                    result.msg = "请填写收件人联系号码";
                    return result;
                }
                if (string.IsNullOrEmpty(orderModel.address))
                {
                    result.msg = "请选择收件地址";
                    return result;
                }
            }

            //添加唯一锁
            string lockkey = $"{orderModel.aid}_{orderModel.storeId}_{(int)TmpType.拼享惠}_{orderModel.goodsId}";
            if (!_lockObjectDictOrder.ContainsKey(lockkey))
            {
                if (!_lockObjectDictOrder.TryAdd(lockkey, new object()))
                {
                    result.msg = "系统繁忙,请稍候再试！";
                    return result;
                }
            }
            lock (_lockObjectDictOrder[lockkey])
            {
                string msg = string.Empty;
                bool status = false;
                
                PinGroup groupInfo = new PinGroup();

                if (orderModel.groupId > 0)
                {
                    status = PinGroupBLL.SingleModel.UpdateEntrantCount(orderModel.groupId, out msg, ref groupInfo);
                    if (!status)
                    {
                        result.msg = msg;
                        return result;
                    }
                }

                
                PinGoods goods = PinGoodsBLL.SingleModel.GetModelById_State(orderModel.goodsId, 1);
                if (goods == null)
                {
                    result.msg = "商品信息错误";
                    return result;
                }
                PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
                if (goods.groupUserBuyLimit > 0)
                {
                    int hasBuyCount = pinGoodsOrderBLL.GetCountByUserId_GoodsId(userInfo.Id, goods.id);
                    if (hasBuyCount >= goods.groupUserBuyLimit)
                    {
                        result.msg = $"本商品每人限购{ goods.groupUserBuyLimit}件";
                        return result;
                    }
                }

                //运费
                int freightFee = 0;
                string freightInfo = string.Empty;
                if (orderModel.sendway == (int)SendWay.商家配送 && orderModel.addressId > 0)
                {
                    PinUserAddress address = PinUserAddressBLL.SingleModel.GetModel(orderModel.addressId);
                    if (address == null)
                    {
                        result.msg = "配送地址不能为空";
                        return result;
                    }
                    DeliveryFeeResult freight = DeliveryTemplateBLL.SingleModel.GetFreightInfo(good: goods, buyCount: orderModel.count, province: address.province, city: address.city);
                    if (!freight.InRange)
                    {
                        result.msg = freight.Message;
                        return result;
                    }
                    freightFee = freight.Fee;
                    freightInfo = freight.Message;
                }

                //减库存
                SpecificationDetailModel specificationDetail = null;
                status = PinGoodsBLL.SingleModel.UpdateStock(goods, orderModel.specificationId, orderModel.count, ref specificationDetail, out msg);
                if (!status)
                {
                    result.msg = msg;
                    return result;
                }

                OrderAttachData attach = new OrderAttachData { FreightInfo = freightInfo };
                PinGoodsOrder order = new PinGoodsOrder()
                {
                    aid = orderModel.aid,
                    storeId = orderModel.storeId,
                    goodsId = orderModel.goodsId,
                    groupId = orderModel.groupId,
                    specificationId = orderModel.specificationId,
                    sourceType = orderModel.sourceType,
                    count = orderModel.count,
                    sendway = orderModel.sendway,
                    payway = orderModel.payway,
                    consignee = orderModel.consignee,
                    phone = orderModel.phone,
                    address = orderModel.address,
                    buyerRemark = orderModel.buyerRemark,
                    freightId = goods.FreightTemplate,
                    freight = freightFee,
                    attachData = JsonConvert.SerializeObject(attach)
                };
                pinGoodsOrderBLL.CreateOrder(order, userInfo, goods, specificationDetail);
                if (order.id > 0)
                {
                    if (order.groupId <= 0)
                    {
                        if (!PinGroupBLL.SingleModel.CreateGroup(order, groupInfo))
                        {
                            result.msg = "创建团活动失败";
                            return result;
                        }
                        if (!pinGoodsOrderBLL.Update(order, "groupId"))
                        {
                            result.msg = "拼团号插入失败";
                            return result;
                        }
                    }
                    //pinGoodsOrderBLL.SendTemplateMsg_Refund(order);
                    //pinGoodsOrderBLL.SendTemplateMsg_PaySuccess(order);
                    result.code = 1;
                    result.obj = new { cityMordersId = order.payNo, order.groupId, groupState = groupInfo.state, groupInfo, orderid = order.id };
                    result.msg = "订单生成成功";
                }
                else
                {
                    #region 回滚库存
                    if (!string.IsNullOrEmpty(orderModel.specificationId) && goods.stockLimit)
                    {
                        specificationDetail = goods.SpecificationDetailList.Find(s => s.id == orderModel.specificationId);

                        if (goods.stockLimit && specificationDetail != null)
                        {
                            List<SpecificationDetailModel> list = goods.SpecificationDetailList;
                            list.ForEach(spec =>
                            {
                                if (spec.id == specificationDetail.id)
                                {
                                    spec.stock = spec.stock + orderModel.count;
                                }
                            });
                            goods.specificationdetail = JsonConvert.SerializeObject(list);
                            status = PinGoodsBLL.SingleModel.Update(goods, "specificationdetail");//回滚库存
                        }
                    }
                    else if (goods.stockLimit)
                    {
                        goods.stock = goods.stock + orderModel.count;
                        status = PinGoodsBLL.SingleModel.Update(goods, "stock");//回滚库存
                    }
                    #endregion
                    result.code = 0;
                    result.msg = "订单生成失败";
                }
                return result;
            }

        }
        /// <summary>
        /// 再次支付
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">{aid:aid,storeId:storeId,orderId:orderId}</param>
        /// <returns>orderDetail：订单详情，storeInfo：店铺信息，goodsInfo：购买商品快照，specification：已选规格快照，cityMordersId：微信支付id，groupId：拼团id</returns>
        [HttpPost]
        public ReturnMsg PayAgain(string utoken, dynamic postData)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            int aid = postData.aid;
            int storeId = postData.storeId;
            int orderId = postData.orderId;
            if (aid <= 0 || storeId <= 0 || orderId <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            //店铺信息
            PinStore store = PinStoreBLL.SingleModel.GetModel(storeId);

            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            PinGoodsOrder order = pinGoodsOrderBLL.GetModelByAid_StoreId_Id(aid, storeId, orderId);
            if (order == null)
            {
                result.msg = "订单错误";
                return result;
            }
            if (order.state < 0)
            {
                result.msg = "订单已过期";
                return result;
            }
            
            PinGroup group = PinGroupBLL.SingleModel.GetModelByAid_StoreId_Id(aid, storeId, order.groupId);
            if (group == null)
            {
                result.msg = "拼团信息错误";
                return result;
            }
            var orderDetail = new
            {
                order.consignee,
                order.phone,
                order.address,
                order.count,
                order.buyerRemark,
                order.outTradeNo,
                order.addtimeStr,
                order.paywayStr,
                order.moneyStr,
                order.state,
                order.stateStr,
                order.freightStr
            };

            result.code = 1;
            result.obj = new
            {
                orderDetail,
                storeInfo = store,
                goodsInfo = order.goodsPhotoModel,
                specification = order.specificationPhotoModel,
                cityMordersId = order.payNo,
                order.groupId,
                group.entrantCount,
                group.groupCount
            };
            return result;
        }

        /// <summary>
        /// 获取运费信息
        /// </summary>
        /// <param name="addressId">用户保存地址ID</param>
        /// <param name="goodId">商品ID</param>
        /// <param name="buyCount">购买数量</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public ReturnMsg GetFreightFee(string utoken, dynamic postData)
        {
            int addressId = postData.addressId;
            int goodId = postData.goodId;
            int buyCount = postData.buyCount;
            if (addressId <= 0 || goodId <= 0 || buyCount <= 0)
            {
                result.msg = "无效参数";
                return result;
            }
            PinUserAddress address = PinUserAddressBLL.SingleModel.GetModel(addressId);
            if (address == null)
            {
                result.msg = "找不到输入地址";
                return result;
            }
            PinGoods good = PinGoodsBLL.SingleModel.GetModel(goodId);
            if (good == null)
            {
                result.msg = "找不到商品";
                return result;
            }
            DeliveryFeeResult freight = DeliveryTemplateBLL.SingleModel.GetFreightInfo(good, buyCount, address.province, address.city);
            result.code = 1;
            result.obj = new { InRange = freight.InRange, Fee = freight.Fee * 0.01, Message = freight.Message };
            return result;
        }

        /// <summary>
        /// 临时接口，用于检查订单超时服务
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ReturnMsg CheckOrder()
        {
            try
            {
                PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
                string msg = pinGoodsOrderBLL.CancelTimeoutOrder();
                result.code = 1;
                result.msg = "执行成功:" + msg;

            }
            catch (Exception ex)
            {
                result.code = 0;
                result.msg = "message" + ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 临时接口，用于拼团成功返利服务
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ReturnMsg CheckGroupSuccess()
        {
            
            result.msg = PinGroupBLL.SingleModel.CheckGroupSuccess();
            return result;
        }

        /// <summary>
        /// 临时接口，用于检测拼团是否已过期
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ReturnMsg CheckGroupTimeOut()
        {
            
            result.msg = PinGroupBLL.SingleModel.CheckGroupTimeout();
            return result;
        }
        /// <summary>
        /// 临时接口，用于测试订单自动交易完成服务
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ReturnMsg CheckOrderSuccess()
        {
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            pinGoodsOrderBLL.CheckOrderSuccess();
            return result;
        }
        #endregion 下单

        #region 拼团
        /// <summary>
        /// 获取拼团详情
        /// </summary>
        /// <param name="utoken">用户token</param>
        /// <param name="aid">小程序aid</param>
        /// <param name="storeId">店铺id</param>
        /// <param name="groupId">拼团号：拼团id</param>
        /// <returns>成功返回{groupDetail：拼团详情，users：参团人员信息，goodsInfo：商品信息（拼团详情中：groupCount为成团人数，entrantCount为参团人数）}</returns>
        [HttpGet]
        public ReturnMsg GroupDetail(string utoken, int aid = 0, int storeId = 0, int groupId = 0)
        {
            result.code = 0;
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            if (aid <= 0 || storeId <= 0 || groupId <= 0)
            {
                result.msg = "找不到活动详情";
                return result;
            }
            
            PinGroup group = PinGroupBLL.SingleModel.GetModelByAid_StoreId_Id(aid, storeId, groupId);
            if (group == null)
            {
                result.msg = "找不到活动详情~";
                return result;
            }
            
            PinGoods goods = PinGoodsBLL.SingleModel.GetModelById_State(group.goodsId, 1);
            List<object> userData = null;
            int orderId = 0;

            //获取参团订单
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            List<PinGoodsOrder> orders = pinGoodsOrderBLL.GetListByGroupId(group.id);
            if (orders != null && orders.Count > 0)
            {
                //当前用户自己的订单
                PinGoodsOrder orderInfo = orders.Where(order => order.userId == userInfo.Id).FirstOrDefault();
                if (orderInfo != null) orderId = orderInfo.id;

                //获取参团人员信息（用户id，头像，用户昵称）
                string ids = string.Join(",", orders.Select(order => order.userId).ToList());
                List<C_UserInfo> users = C_UserInfoBLL.SingleModel.GetListByIds(ids);
                if (users != null && users.Count > 0)
                {
                    userData = new List<object>();
                    foreach (var user in users)
                    {
                        userData.Add(new { userid = user.Id, headImg = user.HeadImgUrl, nickName = user.NickName });
                    }
                }
            }
            var groupData = new { group.id, group.state, group.stateStr, endtime = group.endTimeStr, group.entrantCount, group.groupCount };
            result.code = 1;
            result.obj = new { groupDetail = groupData, users = userData, goodsInfo = goods, orderId };
            return result;
        }

        #endregion

        #region 订单
        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="orderId"></param>
        /// <returns>orderDtail:订单详情,groupDetail：拼团详情,users：参团人员,storeInfo：店铺信息,goodsInfo：购买商品快照,specification：购买商品规格快照</returns>
        [HttpGet]
        public ReturnMsg OrderDetail(string utoken, int aid = 0, int storeId = 0, int orderId = 0)
        {
            result.code = 0;
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            if (aid <= 0 || storeId <= 0 || orderId <= 0)
            {
                result.msg = "找不到订单详情";
                return result;
            }
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            //店铺信息
            PinStore store = PinStoreBLL.SingleModel.GetModel(storeId);

            //订单信息
            PinGoodsOrder order = pinGoodsOrderBLL.GetModelByAid_StoreId_Id(aid, storeId, orderId);
            if (order == null)
            {
                result.msg = "订单不存在";
                return result;
            }
            if (order.sendway == (int)PinEnums.SendWay.到店自取)
            {
                
                PickPlace place = PickPlaceBLL.SingleModel.GetModel(Convert.ToInt32(order.address));
                if (place != null)
                {
                    order.storeName = place.name;
                    order.address = place.address;
                }
                else
                {
                    order.address = string.Empty;
                }
            }

            
            PinGroup pinGroup = PinGroupBLL.SingleModel.GetModel(order.groupId);

            //拼团详情
            object groupDetail = null;
            //参团人员
            List<object> userData = null;
            if (pinGroup != null)
            {
                groupDetail = new
                {
                    pinGroup.id,
                    pinGroup.groupCount,
                    pinGroup.entrantCount,
                    pinGroup.endTimeStr,
                    pinGroup.state,
                    pinGroup.stateStr
                };
                //获取当前参团人员信息
                List<PinGoodsOrder> orders = pinGoodsOrderBLL.GetListByGroupId(pinGroup.id);
                if (orders != null && orders.Count > 0)
                {
                    //获取 用户id，头像，用户昵称
                    string ids = string.Join(",", orders.Select(item => item.userId).ToList());
                    List<C_UserInfo> users = C_UserInfoBLL.SingleModel.GetListByIds(ids);
                    if (users != null && users.Count > 0)
                    {
                        userData = new List<object>();
                        foreach (C_UserInfo user in users)
                        {
                            PinGoodsOrder orderInfo = orders.Where(o => o.userId == user.Id).FirstOrDefault();
                            userData.Add(new { userid = user.Id, headImg = user.HeadImgUrl, nickName = user.NickName, orderState = orderInfo.state, orderStateStr = orderInfo.stateStr });
                        }
                    }
                }
            }
            if (order.isReturnMoney == 1)//判断是否审核通过
            {
                DrawCashApply apply = DrawCashApplyBLL.SingleModel.GetModelByOrderId_DrawStates(order.id, $"{(int)DrawCashState.提现成功},{(int)DrawCashState.人工提现}");
                if (apply == null) order.isReturnMoney = 0;
            }
            //订单详情
            var orderDetail = new
            {
                order.consignee,
                order.phone,
                order.address,
                order.count,
                order.buyerRemark,
                order.outTradeNo,
                order.addtimeStr,
                order.paywayStr,
                order.moneyStr,
                order.state,
                order.isReturnMoney,
                order.stateStr,
                order.sendway,
                order.sendwayStr,
                order.receivingNo,
                order.lastState,
                order.freightStr,
                timeout = order.receivingtime.AddDays(15) < DateTime.Now//交易完成时间是否超过15天
            };
            result.code = 1;

            result.obj = new
            {
                orderDetail,
                groupDetail,
                users = userData,
                storeInfo = store,
                goodsInfo = order.goodsPhotoModel,
                specification = order.specificationPhotoModel
            };
            return result;
        }
        /// <summary>
        /// 订单列表
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="state"></param>
        /// <param name="commentState">评论状态：1：待评论 -999：所有</param>
        /// <param name="groupState">拼团状态</param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg OrderList(string utoken, int aid = 0, int pageIndex = 1, int pageSize = 20, int state = -999, int groupState = -999, int commentState = -999)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            if (aid <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            
            
            List<PinGoodsOrder> orders = pinGoodsOrderBLL.GetListByCondition(aid, userInfo.Id, state, groupState, commentState, pageIndex, pageSize);
            List<object> list = new List<object>();
            if (orders != null && orders.Count > 0)
            {
                string storeIds = string.Join(",",orders.Select(s=>s.storeId).Distinct());
                List<PinStore> pinStoreList = PinStoreBLL.SingleModel.GetListByIds(storeIds);

                string groupIds = string.Join(",",orders.Select(s=>s.groupId));
                List<PinGroup> pinGroupList = PinGroupBLL.SingleModel.GetListByIds(groupIds);

                foreach (PinGoodsOrder order in orders)
                {
                    PinStore storeInfo = pinStoreList?.FirstOrDefault(f=>f.id == order.storeId);
                    var orderDetail = new
                    {
                        order.id,
                        order.outTradeNo,
                        order.payState,
                        order.payStateStr,
                        order.moneyStr,
                        order.count,
                        order.state,
                        order.stateStr,
                        order.lastState,
                        hasDelivery = DeliveryFeedbackBLL.SingleModel.GetOrderFeedCount(order) > 0,
                    };
                    PinGroup groupDetail = pinGroupList?.FirstOrDefault(f=>f.id == order.groupId);
                    list.Add(new { orderDetail, storeInfo, goods = order.goodsPhotoModel, specification = order.specificationPhotoModel, groupDetail });
                }
            }
            result.code = 1;
            result.obj = new { list };
            return result;
        }
        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">{aid:aid,storeId:storeId,orderId:orderId}</param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg OrderSuccess(string utoken, dynamic postData)
        {
            int aid = postData.aid;
            int storeId = postData.storeId;
            int orderId = postData.orderId;
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            if (aid <= 0 || storeId <= 0 || orderId <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            PinGoodsOrder order = pinGoodsOrderBLL.GetModelByAid_StoreId_Id(aid, storeId, orderId);
            if (order == null)
            {
                result.msg = "订单不存在";
                return result;
            }
            List<PinGoodsOrder> orders = new List<PinGoodsOrder>();
            orders.Add(order);
            result.code = pinGoodsOrderBLL.OrderSuccess(orders);
            result.msg = result.code == 1 ? "操作成功" : "操作失败";
            return result;
        }

        /// <summary>
        /// 用户删除订单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg DeleteOrder(string utoken, dynamic postData)
        {
            int orderId = postData?.orderId ?? 0;
            if (orderId <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            PinGoodsOrderBLL orderBLL = new PinGoodsOrderBLL();
            PinGoodsOrder order = orderBLL.GetModel(orderId);
            if (order == null || order.userId != userInfo.Id)
            {
                result.msg = "非法请求";
                return result;
            }
            if (order.state >= 0 && order.state <= 3)
            {
                result.msg = "当前状态不允许删除";
                return result;
            }
            order.state = (int)PinOrderState.已删除;
            if (orderBLL.Update(order, "state"))
            {
                result.code = 1;
                result.msg = "删除成功";
                return result;
            }

            result.code = 0;
            result.msg = "删除失败";
            return result;
        }
        /// <summary>
        /// 用户取消订单
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">{orderId:id}</param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg CancelOrder(string utoken, dynamic postData)
        {
            int orderId = postData?.orderId ?? 0;
            if (orderId <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            PinGoodsOrderBLL orderBLL = new PinGoodsOrderBLL();
            PinGoodsOrder order = orderBLL.GetModel(orderId);
            if (order == null || order.userId != userInfo.Id)
            {
                result.msg = "非法请求";
                return result;
            }
            if (!(order.state == (int)PinOrderState.待发货 || order.state == (int)PinOrderState.待支付))
            {
                result.msg = "当前状态无法取消订单，请申请售后";
                return result;
            }
            List<PinGoodsOrder> orders = new List<PinGoodsOrder>() { order };
            result.code = orderBLL.CancelOrder(orders, "用户取消订单");
            if (result.code == 1)
            {
                string msg = string.Empty;
                if (order.payState == (int)PinEnums.PayState.已付款)
                {
                    result.code = orderBLL.Refund(order, ref msg);

                    result.msg = msg;
                }
                return result;
            }
            else
            {
                result.msg = "订单取消失败";
                return result;
            }
        }
        #endregion
        #region 商家端
        /// <summary>
        /// 商家查看订单列表
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <param name="state"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="seltype">检索类型 1：订单号，2：提货号，3：商品名称，4：收货人，5：手机号</param>
        /// <param name="val">检索值</param>
        /// <param name="dateType">检索时间范围 -999:全部，1：7日订单，  2：今日订单</param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg StoreOrderList(string utoken, int aid = 0, int state = -999, int pageIndex = 1, int pageSize = 20, int seltype = -999, string val = "", int dateType = -999)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            if (aid <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_UserId(aid, userInfo.Id);
            if (store == null)
            {
                result.msg = "门店信息错误";
                return result;
            }
            DateTime startDate = DateTime.Now.Date;
            DateTime endDate = startDate.AddDays(1).AddSeconds(-1);
            DateTime? start = null;
            DateTime? end = null;

            switch (dateType)
            {
                case 1://7日
                    start = startDate.AddDays(-7);
                    end = endDate;
                    break;
                case 2://今日
                    start = startDate;
                    end = endDate;
                    break;
                default:
                    break;
            }
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            List<PinGoodsOrder> orderList = pinGoodsOrderBLL.AdminGetList(aid, store.id, seltype, val, state, pageIndex, pageSize, start, end);
            if (orderList != null & orderList.Count > 0)
            {
                
                orderList.ForEach(order =>
                {
                    if (order.sendway == (int)PinEnums.SendWay.到店自取)
                    {
                        PickPlace place = PickPlaceBLL.SingleModel.GetModel(Convert.ToInt32(order.address));
                        if (place != null)
                        {
                            order.storeName = place.name;
                            order.address = place.address;
                        }
                        else
                        {
                            order.address = string.Empty;
                        }
                    }
                    else
                    {
                        order.storeName = store.storeName;
                    }
                });
            }
            result.code = 1;
            result.obj = orderList;
            return result;
        }
        /// <summary>
        /// 切换订单状态
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">{aid:aid,storeId:storeId,orderId:orderId,state:state}</param>
        /// state:发货：1，交易成功：4，交易取消：-1
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg UpdateOrderState(string utoken, dynamic postData)
        {
            int aid = postData.aid;
            int storeId = postData.storeId;
            int orderId = postData.orderId;
            int state = postData.state;
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            if (aid <= 0 || orderId <= 0)
            {
                result.msg = $"参数错误{aid},{orderId}";
                return result;
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_UserId(aid, userInfo.Id);
            if (store == null)
            {
                result.msg = "门店信息错误";
                return result;
            }
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            PinGoodsOrder order = pinGoodsOrderBLL.GetModelByAid_StoreId_Id(aid, store.id, orderId);
            if (order == null)
            {
                result.msg = "订单不存在";
                return result;
            }
            List<PinGoodsOrder> orders = new List<PinGoodsOrder>();
            orders.Add(order);
            switch (state)
            {
                case (int)PinEnums.PinOrderState.待发货:
                    result.code = pinGoodsOrderBLL.SendGoods(orders);
                    break;
                case (int)PinEnums.PinOrderState.交易成功:
                    result.code = pinGoodsOrderBLL.OrderSuccess(orders);
                    break;
                case (int)PinEnums.PinOrderState.交易取消:
                    result.code = pinGoodsOrderBLL.CancelOrder(orders);
                    if (result.code == 1)
                    {
                        string msg = string.Empty;
                        if (order.payState == (int)PinEnums.PayState.已付款)
                        {
                            result.code = pinGoodsOrderBLL.Refund(order, ref msg);

                            result.msg = msg;
                        }
                        return result;
                    }
                    break;
                case (int)PinEnums.PinOrderState.已删除:
                    if (order.state == (int)PinOrderState.待支付 ||
                       order.state == (int)PinOrderState.待发货 ||
                       order.state == (int)PinOrderState.待收货 ||
                       order.state == (int)PinOrderState.待自取)
                    {
                        result.code = 0;
                        result.msg = $"“{Enum.GetName(typeof(PinOrderState), order.state)}”状态的订单不能删除";
                        return result;
                    }
                    order.state = (int)PinOrderState.已删除;
                    result.code = pinGoodsOrderBLL.Update(order, "state") ? 1 : 0;
                    result.msg = result.code == 0 ? "删除失败" : "删除成功";
                    return result;
                default:
                    result.msg = "参数错误!";
                    return result;
            }
            result.msg = result.code == 1 ? "操作成功" : "操作失败";
            return result;
        }

        /// <summary>
        /// 获取平台配置
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="islog">1：记录utoken到日志</param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg PlatFormInfo(string utoken, int aid = 0, int storeId = 0, int islog = 0)
        {
            if (islog == 1)
            {
                log4net.LogHelper.WriteInfo(GetType(), $"拼享惠获取用户utoken：{utoken}");
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            if (aid <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            
            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aid);
            if (platform == null)
            {
                result.msg = "平台信息错误";
                return result;
            }
            PinStore store = new PinStore();
            if (storeId > 0)
            {
                store = PinStoreBLL.SingleModel.GetModel(storeId);
            }
            result.code = 1;
            string agentFeeStr = "1688.00";
            int agentExtract = 400;
            int orderExtract = 20;
            PinAgentLevelConfig pinAgentLevelConfig= PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(1, aid);
            if (pinAgentLevelConfig != null)
            {
                agentFeeStr = pinAgentLevelConfig.AgentFeeStr;
             
            }

            PinAgent pinAgent = PinAgentBLL.SingleModel.GetModelByUserId(userInfo.Id);
            if (pinAgent != null)
            {
                PinAgentLevelConfig userAgentLevelConfig = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(pinAgent.AgentLevel, aid);
                if (userAgentLevelConfig != null)
                {
                    agentExtract = userAgentLevelConfig.AgentExtract;
                    orderExtract = userAgentLevelConfig.OrderExtract;

                }
            }

           



            List<PinPicture> picList = PinPictureBLL.SingleModel.GetListByAid_Type(aid, 0);
            result.obj = new
            {
                platform.toBank,//允许银行卡提现 0：不允许 1：允许
                platform.toWx,//允许微信提现 0：不允许 1：允许
                serviceFee = platform.serviceFee * 10 * 0.01, //平台提现费率
                qrcodeServiceFee = platform.qrcodeServiceFee * 10 * 0.01,//店内扫码提现费率
                agentServiceFee = platform.agentServiceFee * 10 * 0.01,//代理提现费率
                store.cashStr,//可提现金额
                store.bankcardName,//银行卡账户名
                store.bankcardNum,//银行卡账号
                store.logo,//门店logo
                userInfo.NickName,//微信昵称,
                platform.freeDays,//免费体验天数
                platform.openKf,//开启客服
                platform.kfPhone,//客服电话
                agentFee = agentFeeStr,// platform.agentFeeStr,
                agentExtract = agentExtract * 10 * 0.01,
                orderExtract = orderExtract * 10 * 0.01,
                minTxMoney = platform.minTxMoneyStr,
                platform.dealDays,
                picList,
                platform.ServiceWeiXin,
                poster = Microsoft.JScript.GlobalObject.unescape(platform.poster),
                platform.AgentSearchPort,
                platform.AddStorePort
            };
            return result;
        }
        /// <summary>
        /// 发起提现申请
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">{aid:aid,storeId:storeId,drawCashWay:提现方式 （1：银行卡 0：微信）,cash:提现金额（单位：分）,applyType:（3：平台提现，4：店内扫码提现，5代理收益提现）}</param>
        /// <param name="xwkjUserId">通过小未科技公众号的用户Id，大于0：通过小未科技公众号提现</param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg AddApply(string utoken, dynamic postData, int xwkjUserId = 0)
        {
            int aid = postData.aid;
            int storeId = postData.storeId;
            int drawCashWay = postData.drawCashWay;
            int applyType = postData.applyType;
            int cash = postData.cash;
            //int xwkjUserId = postData.xwkjUserId;
            if (aid <= 0 || storeId <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            
            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aid);
            if (platform == null)
            {
                result.msg = "平台信息错误";
                return result;
            }
            if (platform.toWx != 1 && xwkjUserId == 0)
            {
                result.msg = "暂时无法通过微信提现";
                return result;
            }
            //if (cash < platform.minTxMoney)
            //{
            //    result.msg = $"提现金额不可低于{platform.minTxMoneyStr}元";
            //    return result;
            //}
            
            string account = userInfo.NickName;
            string accountName = "微信账号";
            int serviceFee = 0;
            if (applyType == (int)DrawCashApplyType.拼享惠代理收益)//代理收益提现
            {
                
                PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(userInfo.Id);
                if (agent == null)
                {
                    result.msg = "代理信息错误";
                    return result;
                }
                if (cash * 1000 > agent.cash)
                {
                    result.msg = "超出可提现金额";
                    return result;
                }
                double fee = cash * platform.agentServiceFee * 0.001;
                serviceFee = (int)Math.Ceiling(fee);
                result.code = DrawCashApplyBLL.SingleModel.PxhAddApply(agent, userInfo.Id, account, accountName, cash, drawCashWay, serviceFee, xwkjUserId);
            }
            else
            {
                PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
                if (store == null)
                {
                    result.msg = "店铺信息错误";
                    return result;
                }
                if (applyType == (int)DrawCashApplyType.拼享惠平台交易)//平台交易提现
                {
                    if (cash > store.cash)
                    {
                        result.msg = "超出可提现金额";
                        return result;
                    }
                    double fee = cash * platform.serviceFee * 0.001;
                    serviceFee = (int)Math.Ceiling(fee);
                }
                else//店内扫码提现
                {
                    if (cash > store.qrcodeCash)
                    {
                        result.msg = "超出可提现金额";
                        return result;
                    }
                    double fee = cash * platform.qrcodeServiceFee * 0.001;
                    serviceFee = (int)Math.Ceiling(fee);
                }
                result.code = DrawCashApplyBLL.SingleModel.PxhAddApply(store, userInfo.Id, account, accountName, cash, drawCashWay, serviceFee, applyType, xwkjUserId);
            }
            result.msg = result.code == 1 ? "申请已提交" : "提交失败";
            return result;
        }

        /// <summary>
        /// 提现记录
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="applyType">提现类型：5：代理收益 3：商家收益</param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg DrawCashRecord(string utoken, int aid = 0, int storeId = 0, int pageIndex = 1, int pageSize = 20, int applyType = (int)DrawCashApplyType.拼享惠代理收益)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            if (aid <= 0 || storeId <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            
            int recordCount = 0;
            List<DrawCashApply> applyList = DrawCashApplyBLL.SingleModel.GetListByAid_UserId_applyType(aid, userInfo.Id, pageIndex, pageSize, out recordCount, applyType);
            result.code = 1;
            result.obj = new { list = applyList };
            return result;
        }

        /// <summary>
        /// 获取二维码
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="type">1:产品详情，2:店铺首页，3:分享拼团,4:分享入驻</param>
        /// <param name="scene">二维码所带参数返回</param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg GetQrCode(string utoken, int aid = 0, int storeId = 0, int type = 0, string scene = "")
        {
            if (aid <= 0 || string.IsNullOrEmpty(scene))
            {
                result.msg = $"参数错误";
                return result;
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcx == null)
            {
                result.msg = "小程序不存在";
                return result;
            }
            string path = string.Empty;
            switch (type)
            {
                case 1://产品详情
                    path = "pages/shopping/goodInfo/goodInfo";
                    // path = "pages/index/index";
                    break;
                case 2://店铺首页
                    path = "pages/store/storeIndex/storeIndex";
                    break;
                case 3://分享拼团
                    path = "pages/shopping/pintuanInfo/pintuanInfo";
                    break;
                case 4://分享入驻成为代理
                    path = "pages/store/applyEnter/applyEnter";
                    break;
                case 5://生成日志二维码获取用户utoken
                    path = "pages/index/index";
                    scene = "1";
                    break;
                default:
                    result.msg = "参数错误";
                    return result;
            }

            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcx, ref token))
            {
                result.msg = token;
                return result;
            }

            //获取二维码
            qrcodeclass resultQcode = CommondHelper.GetWxQrcode(token, path, scene);
            if (resultQcode == null)
            {
                result.msg = "生成二维码失败";
                return result;
            }
            if (resultQcode.isok <= 0)
            {
                result.msg = resultQcode.msg;
            }
            else
            {
                result.code = 1;
                result.obj = new { resultQcode.url };
            }
            return result;
        }

        /// <summary>
        /// 获取各状态订单数量
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <returns>{total:全部订单数, todayTotal:今日订单数, weekTotal:7日订单数, sendTotal:待发货订单数, receiveTotal:待收货订单数, zqTotal:待自取订单数, successTotal:交易成功订单数}</returns>
        [HttpGet]
        public ReturnMsg OrderTotal(string utoken, int aid = 0, int storeId = 0)
        {
            if (aid <= 0 || storeId <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            DateTime startDate = DateTime.Now.Date;
            DateTime endDate = startDate.AddDays(1).AddSeconds(-1);
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            int total = pinGoodsOrderBLL.GetCountByState_Date(aid, storeId);//全部订单数
            int todayTotal = pinGoodsOrderBLL.GetCountByState_Date(aid, storeId, startDate: startDate, endDate: endDate);//今日订单数
            int weekTotal = pinGoodsOrderBLL.GetCountByState_Date(aid, storeId, startDate: startDate.AddDays(-7), endDate: endDate);//7日订单数
            int sendTotal = pinGoodsOrderBLL.GetCountByState_Date(aid, storeId, (int)PinEnums.PinOrderState.待发货);//待发货订单数
            int receiveTotal = pinGoodsOrderBLL.GetCountByState_Date(aid, storeId, (int)PinEnums.PinOrderState.待收货); ;//待收货订单数
            int zqTotal = pinGoodsOrderBLL.GetCountByState_Date(aid, storeId, (int)PinEnums.PinOrderState.待自取);//待自取订单数
            int successTotal = pinGoodsOrderBLL.GetCountByState_Date(aid, storeId, (int)PinEnums.PinOrderState.交易成功);//交易成功订单数
            result.code = 1;
            result.obj = new { total, todayTotal, weekTotal, sendTotal, receiveTotal, zqTotal, successTotal };
            return result;
        }

        /// <summary>
        /// 获取所有类型定义
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <param name="storeid"></param>
        /// <returns></returns>
        public ReturnMsg GetGoodsTypesAll(string utoken, int aid, int storeid)
        {
            List<PinCategory> goodTypes = PinCategoryBLL.SingleModel.GetList($"aid={aid} and state=1 and storeid=0  ");
            List<PinGoodsLabel> goodLabels = PinGoodsLabelBLL.SingleModel.GetList($"aid={aid} and state=1 and storeid={storeid} ");
            List<PinAttr> goodAttrList = PinAttrBLL.SingleModel.GetList($"aid={aid} and state=1 and storeid={storeid} ");
            List<PinGoodsUnit> goodUnits = PinGoodsUnitBLL.SingleModel.GetList($" aid={aid} and state=1 and storeid={storeid} ");
            List<DeliveryTemplate> goodFreight = DeliveryTemplateBLL.SingleModel.GetByAid(aid, storeId: storeid, pageIndex: 1, pageSize: 999);
            result.code = 1;
            result.obj = new
            {
                goodTypes,//分类
                goodLabels,//标签
                goodAttrList,//规格属性
                goodUnits,//单位
                goodTypeModel = new PinCategory { aId = aid, storeId = storeid },
                goodModel = new PinGoods { aId = aid, storeId = storeid },
                attrModel = new PinAttr { aId = aid, storeId = storeid, sel = false, state = 1 },
                goodFreight = goodFreight.Select(freight => new { Id = freight.Id, Name = freight.Name, Unit = freight.UnitType, IsFree = freight.IsFree }),
            };
            return result;
        }

        /// <summary>
        /// 删除，添加，修改产品分类
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">
        /// utoken，aId,storeId,是必传的
        /// 删除时传id,act=del
        /// 添加时传act=add,model
        /// 修改时传act=update,model
        /// </param>
        /// <returns></returns>
        public ReturnMsg GoodType(string utoken, [FromBody]PinCategory model, string act = "")
        {
            int storeId = model.storeId;
            int aId = model.aId;
            #region 删除

            
            
            if (act == "del")
            {

                //检查是否有已经有产品使用了分类
                int goodsCount = PinGoodsBLL.SingleModel.GetCount($"(cateidone={model.id} or cateid={model.id}) and aid={aId} and storeid={storeId} and state<>-1");
                if (goodsCount > 0)
                {
                    result.msg = $"该分类下已有{goodsCount}个产品，不可删除！";
                    return result;
                }

                //检查有没有子分类
                if (model.id > 0)
                {
                    int childCategoryCount = PinCategoryBLL.SingleModel.GetCount($"fid={model.id} and aid={aId} and storeid={storeId} and state<>-1");
                    if (childCategoryCount > 0)
                    {
                        result.msg = $"该分类下已有{childCategoryCount}个二级分类,不可删除,请先删其下的二级分类！";
                        return result;
                    }
                }

                model = PinCategoryBLL.SingleModel.GetModel(model.id);
                if (model == null)
                {
                    result.msg = "对象不存在";
                    return result;
                }

                model.state = -1;
                if (PinCategoryBLL.SingleModel.Update(model, "state"))
                {
                    result.code = 1;
                    result.msg = "删除成功";
                }
                else
                {
                    result.msg = "删除失败";
                }
                return result;
            }

            #endregion 删除

            #region 添加和修改
            if (model != null)
            {
                if (model.id == 0)
                {
                    if (PinCategoryBLL.SingleModel.Exists($"state=1 and aid={aId} and storeid={storeId} and name=@name", new MySql.Data.MySqlClient.MySqlParameter[] {
                             new MySql.Data.MySqlClient.MySqlParameter("@name",model.name)
                        }))
                    {
                        result.code = 0;
                        result.msg = $"“{ model.name}” 已存在，不能重复添加！";
                        return result;
                    }
                    int newid = Convert.ToInt32(PinCategoryBLL.SingleModel.Add(model));
                    result.msg = newid > 0 ? "添加成功" : "添加失败";
                    result.code = newid > 0 ? 1 : 0;
                    return result;
                }
                else if (model.id > 0)
                {
                    if (PinCategoryBLL.SingleModel.Exists($"state=1 and aid={aId} and storeid={storeId} and name=@name and id<>{model.id}", new MySql.Data.MySqlClient.MySqlParameter[] {
                             new MySql.Data.MySqlClient.MySqlParameter("@name",model.name)
                        }))
                    {
                        result.code = 0;
                        result.msg = $"“{ model.name}” 已存在，不能重复添加！";
                        return result;
                    }
                    bool updateResult = PinCategoryBLL.SingleModel.Update(model);
                    result.msg = updateResult ? "修改成功" : "修改失败";
                    result.code = updateResult ? 1 : 0;
                    return result;
                }
            }
            result.msg = "非法请求";
            return result;
            #endregion 添加和修改
        }

        /// <summary>
        /// 删除，添加，修改规格属性
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="model"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public ReturnMsg GoodAttr(string utoken, [FromBody]PinAttr model, string act = "")
        {
            int storeId = model.storeId;
            int aId = model.aId;
            #region 删除

            
            if (act == "del")
            {

                //检查是否有已经有产品使用了分类
                //int goodsCount = goodsBLL.GetCount($"(cateidone={model.id} or cateid={model.id}) and aid={aId} and storeid={storeId} and state<>-1");
                //if (goodsCount > 0)
                //{
                //    result.msg = $"该分类下已有{goodsCount}个产品，不可删除！";
                //    return result;
                //}

                //检查有没有子分类
                if (model.id > 0)
                {
                    int childAttrCount = PinAttrBLL.SingleModel.GetCount($"fid={model.id} and aid={aId} and storeid={storeId} and state<>-1");
                    if (childAttrCount > 0)
                    {
                        result.msg = $"该分类下已有{childAttrCount}个规格值,不可删除,请先删其下的规格值！";
                        return result;
                    }
                }

                model = PinAttrBLL.SingleModel.GetModel(model.id);
                if (model == null)
                {
                    result.msg = "对象不存在";
                    return result;
                }

                model.state = -1;
                if (PinAttrBLL.SingleModel.Update(model, "state"))
                {
                    result.code = 1;
                    result.msg = "删除成功";
                }
                else
                {
                    result.msg = "删除失败";
                }
                return result;
            }

            #endregion 删除

            #region 添加和修改
            if (model != null)
            {
                if (model.id == 0)
                {
                    if (PinAttrBLL.SingleModel.Exists($"state=1 and aid={aId} and storeid={storeId} and name=@name", new MySql.Data.MySqlClient.MySqlParameter[] {
                             new MySql.Data.MySqlClient.MySqlParameter("@name",model.name)
                        }))
                    {
                        result.code = 0;
                        result.msg = $"“{ model.name}” 已存在，不能重复添加！";
                        return result;
                    }
                    int newid = Convert.ToInt32(PinAttrBLL.SingleModel.Add(model));
                    result.msg = newid > 0 ? "添加成功" : "添加失败";
                    result.code = newid > 0 ? 1 : 0;
                    return result;
                }
                else if (model.id > 0)
                {
                    if (PinAttrBLL.SingleModel.Exists($"state=1 and aid={aId} and storeid={storeId} and name=@name and id<>{model.id}", new MySql.Data.MySqlClient.MySqlParameter[] {
                             new MySql.Data.MySqlClient.MySqlParameter("@name",model.name)
                        }))
                    {
                        result.code = 0;
                        result.msg = $"“{ model.name}” 已存在，不能重复添加！";
                        return result;
                    }
                    bool updateResult = PinAttrBLL.SingleModel.Update(model);
                    result.msg = updateResult ? "修改成功" : "修改失败";
                    result.code = updateResult ? 1 : 0;
                    return result;
                }
            }
            result.msg = "非法请求";
            return result;
            #endregion 添加和修改
        }

        /// <summary>
        /// 添加，修改产品
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg SaveGoodsInfo(string utoken, [FromBody]PinGoods model)
        {

            if (!ModelState.IsValid)
            {
                result.msg = this.ErrorMsg();
                return result;
            }
            if (model.id == 0)
            {
                int newid = Convert.ToInt32(PinGoodsBLL.SingleModel.Add(model));
                model.id = newid;
                if (newid > 0)
                {
                    DAL.Base.RedisUtil.Remove("temp_p_pin_description_0");
                    result.code = 1;
                    result.msg = "添加成功";
                    return result;
                }
                else
                {
                    result.msg = "添加失败";
                }
            }
            else
            {
                model.updateTime = DateTime.Now;
                if (PinGoodsBLL.SingleModel.Update(model))
                {
                    result.code = 1;
                    result.msg = "保存成功";
                }
                else
                {
                    result.msg = "保存失败";
                }
            }
            return result;
        }

        /// <summary>
        /// 确认发货
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">{aid:aid,storeId:storeId,orderId:orderId}</param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg DeliveryOrder(string utoken, [FromBody]DeliveryUpdatePost postData)
        {
            if (postData.OrderId <= 0)
            {
                result.msg = "参数错误";
                return result;
            }

            if ((!postData.SelfDelivery || (string.IsNullOrWhiteSpace(postData.CompanyCode) || string.IsNullOrWhiteSpace(postData.CompanyTitle))) && (
                string.IsNullOrWhiteSpace(postData.Address) ||
                string.IsNullOrWhiteSpace(postData.ContactName) ||
                string.IsNullOrWhiteSpace(postData.ContactTel) ||
                string.IsNullOrWhiteSpace(postData.DeliveryNo)))
            {
                result.msg = "请完整填写发货信息";
                return result;
            }

            C_UserInfo userInfo = GetUserInfo(utoken);
            PinGoodsOrderBLL orderBLL = new PinGoodsOrderBLL();
            PinGoodsOrder order = orderBLL.GetModel(postData.OrderId);
            if (order == null || order.state != (int)PinOrderState.待发货)
            {
                result.msg = "非法请求";
                return result;
            }

            result.code = orderBLL.SendGoods(new List<PinGoodsOrder> { order }, JsonConvert.SerializeObject(postData));
            result.msg = result.code == 1 ? "发货成功" : "发货失败";
            return result;
        }

        /// <summary>
        /// 确认发货
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">{aid:aid,storeId:storeId,orderId:orderId}</param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg GetDeliveryFeed(string utoken, [FromBody]dynamic postData)
        {
            int? orderId = postData.orderId;
            if (!orderId.HasValue || orderId.Value <= 0)
            {
                result.msg = "参数错误";
                return result;
            }

            DeliveryFeedback deliveryFeed = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId.Value, DeliveryOrderType.拼享惠订单商家发货);

            result.code = 1;
            result.msg = "获取成功";
            result.obj = deliveryFeed;
            return result;
        }

        /// <summary>
        /// 获取物流公司列表
        /// </summary>
        /// <returns></returns>
        public ReturnMsg GetDeliveryCompany()
        {
            List<DeliveryCompany> companys = DeliveryCompanyBLL.SingleModel.GetCompanys();
            result.code = 1;
            result.obj = companys;
            return result;
        }
        #endregion


        #region 代理推广分润
        /// <summary>
        /// 验证是否是代理
        /// </summary>
        /// <param name="utoken"></param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg IsAgent(string utoken)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            
            PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(userInfo.Id);
            result.code = 1;
            result.obj = new { agent };
            return result;
        }
        /// <summary>
        /// 代理付费
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">{aid:aid,fuserId:推广者uid}</param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg PayAgent(string utoken, dynamic postData)
        {
            if (postData == null)
            {
                result.msg = "非法请求";
                return result;
            }
            int aid = postData.aid;
            if (aid <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            int fuserId = postData.fuserId;
            
            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aid);
            if (platform == null)
            {
                result.msg = "平台信息错误";
                return result;
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            
            PinAgent fagent = PinAgentBLL.SingleModel.GetModelByUserId(fuserId);
            fuserId = fagent == null ? 0 : fuserId;
            PinAgent agent = new PinAgent
            {
                aId = aid,
                fuserId = fuserId,
                userId = userInfo.Id,
                addTime = DateTime.Now,
                AgentLevel=1
            };
            agent.id = Convert.ToInt32(PinAgentBLL.SingleModel.Add(agent));
            if (agent.id <= 0)
            {
                result.msg = "注册失败";
                return result;
            }

            int money =168800;
            PinAgentLevelConfig pinAgentLevel = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(1, aid);
            if (pinAgentLevel != null)
            {
                money = pinAgentLevel.AgentFee;
            }

            PinGoodsOrder order = new PinGoodsOrder()
            {
                aid = aid,
                orderType = 1,
                count = 1,
                payway = (int)PinEnums.PayWay.微信支付,
                money = money,// platform.agentFee
                userId = userInfo.Id,
                goodsId = agent.id,
            };
            if (userInfo.Id == 23645122 || userInfo.Id == 21558520)
            {
                order.money = 50;
            }
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            order.outTradeNo = $"{DateTime.Now.ToString("yyyyMMddmmss")}";
            order.id = Convert.ToInt32(pinGoodsOrderBLL.Add(order));
            if (order.id > 0)
            {
                order.payNo = pinGoodsOrderBLL.CreateWxOrder(order, userInfo.NickName);
                pinGoodsOrderBLL.Update(order, "payno");
                result.code = 1;
                result.obj = new { cityMordersId = order.payNo, orderid = order.id };
                result.msg = "订单生成成功";
                return result;
            }
            result.msg = "订单生成失败";
            return result;
        }
        /// <summary>
        /// 获取收益详情
        /// </summary>
        /// <param name="utoken"></param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg GetAgentIncome(string utoken)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            
            PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(userInfo.Id);
            if (agent == null)
            {
                result.msg = "代理信息错误";
                return result;
            }
            result.code = 1;
            int storeCount = PinStoreBLL.SingleModel.GetCountByAgentId(agent.id);
            int agentCount = PinAgentBLL.SingleModel.GetCountByFuserId(agent.id);
            result.obj = new { income = agent.moneyStr, storeCount, agentCount };
            return result;
        }
        /// <summary>
        /// 收益列表 type:0代理收益 1:商户收益
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="type">0代理收益 1:商户收益</param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg GetIncomeList(string utoken, int pageIndex = 1, int pageSize = 10, int type = 0)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            
            PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(userInfo.Id);
            if (agent == null)
            {
                result.msg = "代理信息错误";
                return result;
            }
            
            List<PinAgentIncomeLog> agentlist = new List<PinAgentIncomeLog>();
            List<PinStore> storeList = new List<PinStore>();
            int count = 0;
            string sum = string.Empty;
           int extractType= Utility.IO.Context.GetRequestInt("extractType", 0);//默认0 表示下级 1表示下下级
            sum = PinAgentIncomeLogBLL.SingleModel.GetIncomeSum(agent.id, type,extractType);
            List<object> list = new List<object>();
            switch (type)
            {
                case 0:
                    list = PinAgentBLL.SingleModel.GetListByAgentId_type(agent.aId, agent.userId, pageIndex, pageSize, out count,extractType);
                    break;
                case 1:
                    list = PinStoreBLL.SingleModel.GetStoreListByAgentId_type(agent.id, pageIndex, pageSize, out count,extractType);
                    break;
                default:
                    result.msg = "参数错误";
                    return result;
            }
            result.obj = new { count, sum, list };
            result.code = 1;
            return result;
        }
        /// <summary>
        /// 查询可提现金额
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <returns> agentCash：代理金额, cash：平台可提现金额, qrcodeCash：扫码可提现金额</returns>
        [HttpGet]
        public ReturnMsg CashDetail(string utoken, int aid = 0)
        {
            if (aid <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            
            PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(userInfo.Id);
            string agentCash = string.Empty;
            string cash = string.Empty;
            string qrcodeCash = string.Empty;
            if (agent != null)
            {
                agentCash = agent.cashStr;
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_UserId(aid, userInfo.Id);
            if (store != null)
            {
                cash = store.cashStr;
                qrcodeCash = store.qrcodeCashStr;
            }
            result.code = 1;
            result.obj = new { agentCash, cash, qrcodeCash };
            return result;
        }
        /// <summary>
        /// 根据userid获取代理信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg GetAgentInfo(int userId = 0)
        {
            if (userId <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            
            PinAgent pinAgent = PinAgentBLL.SingleModel.GetModelByUserId(userId);
            if (pinAgent == null)
            {
                result.msg = "不是代理";
                return result;
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(pinAgent.userId);
            if (userInfo == null)
            {
                result.msg = "用户信息不存在";
                return result;
            }
           
            result.code = 1;
            result.obj = new { userInfo};
            return result;
        }
        /// <summary>
        /// 获取分享代理userid
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <param name="fuserId"></param>
        /// <returns></returns>
        [HttpGet]
        public ReturnMsg GetFuserId(string utoken, int aid, int fuserId = 0)
        {
            if (aid <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_UserId(aid, userInfo.Id);

            
            if (store != null && store.agentId > 0)//入驻店铺时已有上级代理
            {
                PinAgent agent = PinAgentBLL.SingleModel.GetModelById(store.agentId);
                if (agent != null)
                {
                    result.code = 1;
                    result.obj = new { fuserId = agent.userId };
                    return result;
                }
            }
            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aid);
            if (platform == null)
            {
                result.msg = "平台信息错误";
                return result;
            }
            PinAgent agentInfo = null;
            if (fuserId > 0)
            {
                agentInfo = PinAgentBLL.SingleModel.GetModelByUserId(fuserId);
            }
            
            PinAgentProtect agentProtect = PinAgentProtectBLL.SingleModel.GetModelByUserId_State(userInfo.Id);
          //  log4net.LogHelper.WriteInfo(this.GetType(), $"userInfoId={userInfo.Id};fuserId={fuserId};agentProtect={JsonConvert.SerializeObject(agentProtect)}");

            if (agentProtect != null)//之前扫过码
            {
                if (agentProtect.addTime.AddDays(platform.agentProtectDays) > DateTime.Now && (agentInfo == null || agentInfo.userId == agentProtect.fuserId))//代理保护期内且没有通过其他代理分享进入
                {
                    result.code = 1;
                    result.obj = new { fuserId = agentProtect.fuserId };
                    return result;
                }
                else if (agentProtect.addTime.AddDays(platform.agentProtectDays) <= DateTime.Now && agentInfo == null)//代理保护期已过且直接通过小程序进入入驻页面
                {

                    agentProtect.state = -1;
                    PinAgentProtectBLL.SingleModel.Update(agentProtect, "state");
                    result.code = 1;
                    result.obj = new { fuserId = 0 };
                    return result;

                }
                else//通过扫码或分享进入
                {
                    if (agentInfo.userId == agentProtect.fuserId)//扫同个用户的二维码进入，重置保护期
                    {
                        agentProtect.addTime = DateTime.Now;
                        PinAgentProtectBLL.SingleModel.Update(agentProtect, "addTime");
                        result.code = 1;
                        result.obj = new { fuserId = agentInfo.userId };
                        return result;
                    }
                    else//通过其他用户分享进入
                    {
                        agentProtect.state = -1;
                        PinAgentProtectBLL.SingleModel.Update(agentProtect, "state");
                        agentProtect = new PinAgentProtect()
                        {
                            aid = aid,
                            userId = userInfo.Id,
                            fuserId = agentInfo.userId,
                        };
                        agentProtect.id = Convert.ToInt32(PinAgentProtectBLL.SingleModel.Add(agentProtect));
                        if (agentProtect.id > 0)
                        {
                            result.code = 1;
                            result.obj = new { fuserId = agentProtect.fuserId };
                            return result;
                        }
                        else
                        {
                            result.msg = "数据异常";
                            return result;
                        }
                    }
                }
            }
            else//未扫过码
            {
                if (agentInfo == null)
                {
                    result.code = 1;
                    result.obj = new { fuserId = 0 };
                    return result;
                }
                else
                {
                    agentProtect = new PinAgentProtect()
                    {
                        aid = aid,
                        userId = userInfo.Id,
                        fuserId = agentInfo.userId,
                    };

                   
                    agentProtect.id = Convert.ToInt32(PinAgentProtectBLL.SingleModel.Add(agentProtect));
                    if (agentProtect.id > 0)
                    {
                        result.code = 1;
                        result.obj = new { fuserId = agentProtect.fuserId };
                        return result;
                    }
                    else
                    {
                        result.msg = "数据异常";
                        return result;
                    }
                }
            }
        }
        #endregion
        #region 投诉维权
        /// <summary>
        /// 提交投诉：同个订单只能提交一次
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">{aid:aid,orderId:订单Id,phone:联系电话,question:问题描述,imgUrls:图片说明（';'分割）}</param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg SendComplaint(string utoken, dynamic postData)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            int aid = postData.aid;
            if (aid <= 0)
            {
                result.msg = "参数错误(aid error)";
                return result;
            }
            int orderId = postData.orderId;
            if (orderId <= 0)
            {
                result.msg = "参数错误(orderId error)";
                return result;
            }
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            PinGoodsOrder order = pinGoodsOrderBLL.GetModel(orderId);
            if (order == null)
            {
                result.msg = "订单不存在";
                return result;
            }
            string phone = postData.phone;
            if (string.IsNullOrEmpty(phone))
            {
                result.msg = "请输入手机号";
                return result;
            }
            string question = postData.question;
            if (string.IsNullOrEmpty(question))
            {
                result.msg = "请填写文字说明";
                return result;
            }
            string imgUrls = postData.imgUrls;
            if (!string.IsNullOrEmpty(imgUrls) && imgUrls.Split(';').Length > 9)
            {
                result.msg = "图片不能超过9张";
                return result;
            }

            

            PinComplaint model = PinComplaintBLL.SingleModel.GetModelByUserId_OrderId(userInfo.Id, orderId);
            if (model != null)
            {
                result.msg = "您已发起投诉，请留意平台或商家的消息。您也可以直接联系平台客服，咨询处理进度";
                return result;
            }
            model = new PinComplaint()
            {
                aid = aid,
                userId = userInfo.Id,
                orderId = orderId,
                orderOutTradeNo = order.outTradeNo,
                phone = phone,
                content = question,
                storeId = order.storeId,
                pictures = imgUrls,
            };
            model.id = Convert.ToInt32(PinComplaintBLL.SingleModel.Add(model));
            if (model.id > 0)
            {
                result.code = 1;
                result.msg = "您的投诉已接收，平台运营人员将会马上介绍协调，请留意平台或商家的消息。您也可以直接联系平台客服，咨询处理进度。";
            }
            else
            {
                result.msg = "提交失败";
            }
            return result;
        }
        #endregion

        #region 私信
        /// <summary>
        /// 获取聊天联系人
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ReturnMsg GetConnetUserList(string utoken, int userId = 0, int pageSize = 10, int pageIndex = 1)
        {
            result.code = 0;
            if (userId <= 0)
            {
                result.msg = "无效参数";
                return result;
            }

            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userinfo == null)
            {
                result.msg = "无效用户";
                return result;
            }

            int count = 0;
            List<ImMessage> list = ImContactBLL.SingleModel.GetListByPxh(userId, userinfo.appId, pageSize, pageIndex, ref count);
            result.obj = new { list = list, count = count };

            result.code = 1;
            return result;
        }
        #endregion
        #region 售后
        /// <summary>
        /// 提交退换货申请
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="refundApply"></param>
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg RefundApply(string utoken, [FromBody] PinRefundApply refundApply)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            if (refundApply == null)
            {
                result.msg = "数据错误";
                return result;
            }
            if (refundApply.aId <= 0 || refundApply.storeId <= 0 || refundApply.orderId <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(refundApply.aId, refundApply.storeId);
            if (store == null)
            {
                result.msg = "店铺不存在";
                return result;
            }
            PinGoodsOrder order = orderBLL.GetModelByAid_StoreId_Id(store.aId, store.id, refundApply.orderId);
            if (order == null)
            {
                result.msg = "订单不存在";
                return result;
            }
            if (order.payState != (int)PayState.已付款)
            {
                result.msg = "您的订单未付款或已退款";
                return result;
            }
            if (refundApply.count <= 0)
            {
                result.msg = "请选择要退换的产品数量";
                return result;
            }
            int applyCount = PinRefundApplyBLL.SingleModel.GetCountByOrderId_State(order.id, (int)RefundApplyState.待处理);//获取同一订单申请中的产品数量
            if (refundApply.count > (order.count - order.returnCount + applyCount))
            {
                result.msg = "申请退换的产品数量超过实际购买数量";
                return result;
            }

            refundApply.orderState = order.state;
            refundApply.serviceNo = PinRefundApplyBLL.SingleModel.CreateServiceNo(refundApply, order.outTradeNo);//服务单号为外部订单号加该订单申请次数
            refundApply.userId = userInfo.Id;
            refundApply.addTime = DateTime.Now;
            if (order.isReturnMoney == 0)
            {
                refundApply.money = order.goodsPhotoModel.price * refundApply.count;
            }
            else
            {
                refundApply.money = (order.goodsPhotoModel.price - order.goodsPhotoModel.groupPrice) * refundApply.count;
            }
            int money = 0;//订单实际剩余金额
            if (order.isReturnMoney == 1)
            {
                money = order.money - order.refundMoney - order.returnMoney;
            }
            else
            {
                money = order.money - order.refundMoney;
            }
            if (money < refundApply.money)
            {
                result.msg = $"当前订单支付剩余金额不足以退款";
                return result;
            }

            var id = PinRefundApplyBLL.SingleModel.Add(refundApply);
            if (Convert.ToInt32(id) > 0)
            {
                PinGroup pinGroup = PinGroupBLL.SingleModel.GetModel(order.groupId);

                //拼团未成功、退货数量刚好全退 修改订单状态为
                if ((order.count - order.returnCount + applyCount) == refundApply.count && pinGroup.state < (int)PinEnums.GroupState.拼团成功 && refundApply.type != 3)
                {
                    order.state = (int)PinOrderState.申请售后;
                    orderBLL.Update(order, "state");
                }
                result.code = 1;
                result.msg = "申请已提交";
            }
            else
            {
                result.msg = "申请提交失败";
            }
            return result;
        }
        /// <summary>
        /// 退换货申请详情
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ReturnMsg GetRefundApplyDetail(string utoken, int id)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            if (id <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            PinRefundApply refundApply = PinRefundApplyBLL.SingleModel.GetModel(id);
            if (refundApply == null || refundApply.state == (int)PinEnums.RefundApplyState.删除)
            {
                result.msg = "申请不存在";
                return result;
            }
            refundApply.order = orderBLL.GetModel(refundApply.orderId);
            if (refundApply.order == null)
            {
                result.msg = "订单不存在";
                return result;
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(refundApply.order.aid, refundApply.order.storeId);
            if (store == null)
            {
                result.msg = "店铺不存在";
                return result;
            }
            result.code = 1;
            result.obj = new { apply = refundApply, store.setting.place, store.setting.name, store.setting.phone };
            return result;
        }
        /// <summary>
        /// 提交/修改物流单号
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="postData">{act:操作,name:物流公司,orderNo:物流单号,id:申请id}</param>
        /// act="wuliu":提交/修改物流单号，act="cancel"：取消售后申请
        /// <returns></returns>
        [HttpPost]
        public ReturnMsg UpdateRefundApply(string utoken, dynamic postData)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            string act = postData?.act ?? string.Empty;
            int id = postData?.id ?? 0;
            if (id <= 0 || string.IsNullOrEmpty(act))
            {
                result.msg = "参数错误";
                return result;
            }
            PinRefundApply refundApply = PinRefundApplyBLL.SingleModel.GetModel(id);
            if (refundApply == null || refundApply.state == (int)PinEnums.RefundApplyState.删除)
            {
                result.msg = "申请不存在";
                return result;
            }
            bool status = false;
            string msg = "提交";
            switch (act)
            {
                case "wuliu"://物流单号提交/修改
                    string name = postData?.name ?? string.Empty;
                    if (string.IsNullOrEmpty(name))
                    {
                        result.msg = "请选择物流公司";
                        return result;
                    }
                    string orderNo = postData?.orderNo ?? string.Empty;
                    if (string.IsNullOrEmpty(orderNo))
                    {
                        result.msg = "请填写物流单号";
                        return result;
                    }
                    refundApply.wuliuName = name;
                    refundApply.wuliuOrder = orderNo;
                    refundApply.updateTime = DateTime.Now;
                    status = PinRefundApplyBLL.SingleModel.Update(refundApply, "wuliuName,wuliuOrder,updateTime");
                    break;
                case "cancel":
                    msg = "取消";
                    refundApply.state = (int)RefundApplyState.删除;
                    refundApply.updateTime = DateTime.Now;
                    status = PinRefundApplyBLL.SingleModel.Update(refundApply, "state,updateTime");
                    break;

            }

            if (status)
            {
                result.code = 1;
                result.msg = $"{msg}成功";
            }
            else
            {
                result.msg = $"{msg}失败";
            }
            return result;
        }
        /// <summary>
        /// 售后申请列表
        /// </summary>
        /// <param name="utoken"></param>
        /// <param name="aid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ReturnMsg GetRefundApplyList(string utoken, int aid = 0, int pageIndex = 1, int pageSize = 20)
        {
            C_UserInfo userInfo = GetUserInfo(utoken);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return result;
            }
            if (aid <= 0)
            {
                result.msg = "参数错误";
                return result;
            }
            int recordCount = 0;
            List<object> list = new List<object>();
            List<PinRefundApply> applyList = PinRefundApplyBLL.SingleModel.GetListByAid_UserId(aid, userInfo.Id, pageIndex, pageSize, out recordCount);
            if (applyList != null && applyList.Count > 0)
            {
                string storeIds = string.Join(",",applyList.Select(s=>s.storeId).Distinct());
                List<PinStore> pinStoreList = PinStoreBLL.SingleModel.GetListByIds(storeIds);
                foreach (var apply in applyList)
                {
                    apply.order = orderBLL.GetModel(apply.orderId);
                    apply.store = pinStoreList?.FirstOrDefault(f=>f.id == apply.storeId);
                    var obj = new
                    {
                        id = apply.id,
                        logo = apply.store == null ? "" : apply.store.logo,
                        storeName = apply.store == null ? "未知店铺" : apply.store.storeName,
                        state = apply.stateStr,
                        goodsInfo = apply.order == null ? null : apply.order.goodsPhotoModel,
                        specification = apply.order == null ? null : apply.order.specificationPhotoModel,
                        count = apply.order == null ? 0 : apply.order.count,
                        money = apply.order == null ? "0.00" : apply.order.moneyStr
                    };
                    list.Add(obj);
                }
            }
            result.code = 1;
            result.obj = new { list, recordCount };
            return result;
        }
        #endregion 售后
    }

}