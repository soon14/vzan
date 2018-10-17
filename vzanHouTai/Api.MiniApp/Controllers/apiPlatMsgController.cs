using Api.MiniApp.Filters;
using BLL.MiniApp.Plat;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.IO;
namespace Api.MiniApp.Controllers
{
    /// <summary>
    /// 平台版小程序分类信息接口
    /// </summary>
    public partial class apiPlatController : InheritController
    {

        /// <summary>
        /// 获取商家审核设置模式 小程序发帖时请求
        /// </summary>
        /// <returns></returns>

        public ActionResult GetCityReviewSetting()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("appId", string.Empty);
            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            PlatMsgConf platMsgConf = PlatMsgConfBLL.SingleModel.GetMsgConf(r.Id);
            if (platMsgConf == null)
            {
                returnObj.Msg = "审核配置异常";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            returnObj.isok = true;
            returnObj.dataObj = new { obj = platMsgConf };

            return Json(returnObj, JsonRequestBehavior.AllowGet);


        }


        /// <summary>
        /// 获取发帖类别
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMsgTypeList()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("appId", string.Empty);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            int totalCount = 0;
            List<PlatMsgType> listPlatMsgType = PlatMsgTypeBLL.SingleModel.getListByaid(r.Id, out totalCount, pageSize, pageIndex);

            returnObj.isok = true;
            returnObj.dataObj = new { list = listPlatMsgType, totalCount = totalCount };

            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取发帖规则列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetRuleList()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Utility.IO.Context.GetRequest("appId", string.Empty);
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);
            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            int totalCount = 0;
            List<PlatMsgRule> list = PlatMsgRuleBLL.SingleModel.GetListByaid(r.Id, out totalCount, pageSize, pageIndex);

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { totalCount = totalCount, list = list };
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 获取一条帖子的详情
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMsgDetail()
        {
            returnObj = new Return_Msg_APP();
            string appId = Utility.IO.Context.GetRequest("appId", string.Empty);
            int msgId = Utility.IO.Context.GetRequestInt("msgId", 0);//帖子信息Id
            int userId = Utility.IO.Context.GetRequestInt("userId", 0);// 类似用户Id
            if (string.IsNullOrEmpty(appId) || msgId <= 0)
            {
                returnObj.code = "200";
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.code = "200";
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }


            PlatMsg model = PlatMsgBLL.SingleModel.GetMsg(r.Id, msgId);
            if (model == null)
            {
                returnObj.code = "200";
                returnObj.Msg = "找不到数据";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }



            PlatUserFavoriteMsg _userFavoriteMsg = PlatUserFavoriteMsgBLL.SingleModel.GetUserFavoriteMsg(r.Id, msgId, userId, (int)PointsActionType.收藏);
            model.IsFavorited = (_userFavoriteMsg != null && _userFavoriteMsg.State != -1);

            PlatUserFavoriteMsg _userFavoriteMsgDz = PlatUserFavoriteMsgBLL.SingleModel.GetUserFavoriteMsg(r.Id, msgId, userId, (int)PointsActionType.点赞);
            model.IsDzed = (_userFavoriteMsgDz != null && _userFavoriteMsgDz.State != -1);

            PlatMsgReport _platMsgReport = PlatMsgReportBLL.SingleModel.GetMsgReport(userId, msgId);
            model.IsReported = (_platMsgReport != null);

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { msg = model };
            return Json(returnObj, JsonRequestBehavior.AllowGet);


        }


        /// <summary>
        /// 获取我发布的帖子
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMsgByUserId()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Utility.IO.Context.GetRequest("appId", string.Empty);
            int userId = Utility.IO.Context.GetRequestInt("mycardId", 0);
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            int totalCount = 0;
            List<PlatMsg> list = PlatMsgBLL.SingleModel.GetListByUserId(r.Id, userId, out totalCount, pageSize, pageIndex);

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { totalCount = totalCount, list = list };
            return Json(returnObj, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 获取用户收藏帖子列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetListMyFavoriteMsg()
        {
            returnObj = new Return_Msg_APP();
            string appId = Utility.IO.Context.GetRequest("appId", string.Empty);
            int userId = Utility.IO.Context.GetRequestInt("userId", 0);
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.code = "200";
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.code = "200";
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            int totalCount = 0;
            List<PlatMsg> list = PlatUserFavoriteMsgBLL.SingleModel.GetListMyFavoriteMsg(r.Id, userId, out totalCount, pageSize, pageIndex);

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { totalCount = totalCount, list = list.OrderByDescending(x => x.FavoriteId) };
            return Json(returnObj, JsonRequestBehavior.AllowGet);



        }


        /// <summary>
        /// 获取帖子列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMsgList()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            int userId = Context.GetRequestInt("userId", 0);
            string appId = Context.GetRequest("appId", string.Empty);
            string keyMsg = Context.GetRequest("keyMsg", string.Empty);
            int orderType = Context.GetRequestInt("orderType", 0);//默认为0 表示最新发布  1表示距离最近
            int msgTypeId = Context.GetRequestInt("msgTypeId", 0);//类别默认为0 表示获取所有
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            string latStr = Context.GetRequest("lat", string.Empty);
            string lngStr = Context.GetRequest("lng", string.Empty);
            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            int totalCount = 0;
            int commentTotalCount = 0;//帖子对应的评论条数
            int count = 0;//帖子对应的点赞用户数

            List<PlatMsg> list = new List<PlatMsg>();

            if (orderType == 1)
            {
                
                double lat = 0.00;
                double lng = 0.00;
                //表示按照距离最近的
                //表示没有传坐标 通过客户端IP获取经纬度
                // log4net.LogHelper.WriteInfo(this.GetType(), $"!!!!orderType={orderType};{lat},{lng}");
                if (!double.TryParse(latStr, out lat) || !double.TryParse(lngStr, out lng) || lat==0 || lng==0)
                {
                    string IP = WebHelper.GetIP();

                    IPToPoint iPToPoint = CommondHelper.GetLoctionByIP(IP);
                    if (iPToPoint != null)
                    {

                        lat = iPToPoint.result.location.lat;
                        lng = iPToPoint.result.location.lng;
                        //log4net.LogHelper.WriteInfo(this.GetType(), $"IP={IP};{lat},{lng}");
                    }
                }
                //log4net.LogHelper.WriteInfo(this.GetType(),$"orderType={orderType};{lat},{lng}" );

                list= PlatMsgBLL.SingleModel.GetListMsg(r.Id, out totalCount, keyMsg, msgTypeId, pageSize, pageIndex,1,lat,lng);

                list.ForEach(x =>
                {
  
                    PlatUserFavoriteMsg _userFavoriteMsg = PlatUserFavoriteMsgBLL.SingleModel.GetUserFavoriteMsg(r.Id, x.Id, userId, (int)PointsActionType.收藏);
                    x.IsFavorited = (_userFavoriteMsg != null && _userFavoriteMsg.State != -1);

                    _userFavoriteMsg = PlatUserFavoriteMsgBLL.SingleModel.GetUserFavoriteMsg(r.Id, x.Id, userId, (int)PointsActionType.点赞);
                    x.IsDzed = (_userFavoriteMsg != null && _userFavoriteMsg.State != -1);

                    PlatMsgReport _platMsgReport = PlatMsgReportBLL.SingleModel.GetMsgReport(userId, x.Id);
                    x.IsReported = (_platMsgReport != null);


                    x.Comments = PlatMsgCommentBLL.SingleModel.GetPlatMsgComment(r.Id, out commentTotalCount, 0, string.Empty, 1000, 1, x.Id, 0);


                    x.DzUsers = PlatUserFavoriteMsgBLL.SingleModel.GetMsgUserFavoriteList(0, x.Aid, x.Id, (int)PointsActionType.点赞, (int)PointsDataType.帖子, 1, 1000, ref count);
                });
 
            }
            else
            {
                list = PlatMsgBLL.SingleModel.GetListMsg(r.Id, out totalCount, keyMsg, msgTypeId, pageSize, pageIndex);

                list.ForEach(x =>
                {
                    PlatUserFavoriteMsg _userFavoriteMsg = PlatUserFavoriteMsgBLL.SingleModel.GetUserFavoriteMsg(r.Id, x.Id, userId, (int)PointsActionType.收藏);
                    x.IsFavorited = (_userFavoriteMsg != null && _userFavoriteMsg.State != -1);

                    _userFavoriteMsg = PlatUserFavoriteMsgBLL.SingleModel.GetUserFavoriteMsg(r.Id, x.Id, userId, (int)PointsActionType.点赞);
                    x.IsDzed = (_userFavoriteMsg != null && _userFavoriteMsg.State != -1);

                    PlatMsgReport _platMsgReport = PlatMsgReportBLL.SingleModel.GetMsgReport(userId, x.Id);
                    x.IsReported = (_platMsgReport != null);


                    x.Comments = PlatMsgCommentBLL.SingleModel.GetPlatMsgComment(r.Id, out commentTotalCount, 0, string.Empty, 1000, 1, x.Id, 0);

                    x.DzUsers = PlatUserFavoriteMsgBLL.SingleModel.GetMsgUserFavoriteList(0, x.Aid, x.Id, (int)PointsActionType.点赞, (int)PointsDataType.帖子, 1, 1000, ref count);


                });
              

            }

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { totalCount = totalCount, list = list };
            return Json(returnObj, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 删除我收藏的记录或者发布的帖子
        /// </summary>
        /// <returns></returns>
        public ActionResult DelMyFavoriteOrMyMsg()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("myCardId", 0);
            int Id = Context.GetRequestInt("Id", 0);
            int delType = Context.GetRequestInt("delType", 0);//默认为0 表示删除我发布的帖子 1表示删除我收藏的帖子
            if (string.IsNullOrEmpty(appId) || userId <= 0 || Id <= 0)
            {

                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {

                returnObj.Msg = "小程序未授权";
                return Json(returnObj);
            }

            bool delresult = false;
            // log4net.LogHelper.WriteInfo(this.GetType(), "删除" + $"delType={delType} ， userId={userId} ， aid={appId}，Id={Id}");
            if (delType == 0)
            {
                //删除我发布的帖子
                delresult = PlatMsgBLL.SingleModel.DelMsg(Id, userId, r.Id);
            }
            else
            {
                //  log4net.LogHelper.WriteInfo(this.GetType(), "删除收藏");
                delresult = PlatUserFavoriteMsgBLL.SingleModel.DelMolde(Id, userId, r.Id);
            }

            if (!delresult)
            {

                returnObj.Msg = "操作异常";
                return Json(returnObj);
            }

            returnObj.isok = true;
            returnObj.Msg = "删除成功";
            return Json(returnObj);


        }


        /// <summary>
        /// 新增评论
        /// </summary>
        /// <returns></returns>
        public ActionResult AddComment()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            int Id = Context.GetRequestInt("Id", 0);
            int dataType = Context.GetRequestInt("dataType", 0);//默认为0 0：帖子，1：商品，2：评论(PointsDataType)
            string commentDetail = Context.GetRequest("commentDetail", string.Empty);
            if (string.IsNullOrEmpty(appId) || userId <= 0 || Id <= 0)
            {

                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            if (string.IsNullOrEmpty(commentDetail))
            {
                returnObj.Msg = "评论详情不能为空";
                return Json(returnObj);
            }
            if (commentDetail.Length > 1000)
            {
                returnObj.Msg = "评论详情最大1000字符";
                return Json(returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {

                returnObj.Msg = "小程序未授权";
                return Json(returnObj);
            }
            PlatMsgComment platMsgComment = new PlatMsgComment();
            platMsgComment.AId = r.Id;
            platMsgComment.UserId = userId;
            platMsgComment.CommentDetail = commentDetail;
            platMsgComment.AddTime = DateTime.Now;
            switch (dataType)
            {
                case (int)PointsDataType.帖子:
                    //表示帖子的评论
                    PlatMsg platMsg = PlatMsgBLL.SingleModel.GetMsg(r.Id, Id);
                    if (platMsg == null || platMsg.State == -1)
                    {
                        returnObj.Msg = "帖子不存在!";
                        return Json(returnObj);
                    }
                    platMsgComment.ToUserId = platMsg.MyCardId;
                    platMsgComment.MsgId = Id;
                    break;
            }

            int commentId = Convert.ToInt32(PlatMsgCommentBLL.SingleModel.Add(platMsgComment));
            if (commentId > 0)
            {
                int commentTotalCount = 0;
                switch (dataType)
                {
                    case (int)PointsDataType.帖子:
                        returnObj.dataObj = new { comments = PlatMsgCommentBLL.SingleModel.GetPlatMsgComment(r.Id, out commentTotalCount, 0, string.Empty, 1000, 1, Id, 0) };
                        break;
                };

                //添加活动轨迹
                PlatActivityTrajectoryBLL.SingleModel.AddData(r.Id, userId, platMsgComment.MsgId, (int)PointsActionType.评论, dataType, commentId,"", platMsgComment.CommentDetail);

                returnObj.isok = true;
                returnObj.Msg = "评论成功";
                return Json(returnObj);
            }
            else
            {
                returnObj.Msg = "评论失败";
                return Json(returnObj);
            }


        }

        /// <summary>
        /// 获取评论
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMsgComment()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int Id = Context.GetRequestInt("Id", 0);//指定某条帖子的 评论
            int userId = Context.GetRequestInt("userId", 0);//指定用户Id 表示获取我发出去的评论
            int actionType = Context.GetRequestInt("actionType", 0);//默认为0 0：帖子，1：商品，2：评论(PointsDataType)
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            string keyMsg = Context.GetRequest("keyMsg", string.Empty);
            if (string.IsNullOrEmpty(appId))
            {

                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {

                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            int totalCount = 0;
            List<PlatMsgComment> listComment = PlatMsgCommentBLL.SingleModel.GetPlatMsgComment(r.Id, out totalCount, actionType, keyMsg, pageSize, pageIndex, Id, userId);

            returnObj.dataObj = new { totalCount = totalCount, list = listComment };
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);


        }

        /// <summary>
        /// 删除某条评论
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteMsgComment()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int Id = Context.GetRequestInt("Id", 0);

            if (string.IsNullOrEmpty(appId) || Id <= 0)
            {

                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {

                returnObj.Msg = "小程序未授权";
                return Json(returnObj);
            }

            PlatMsgComment platMsgComment = PlatMsgCommentBLL.SingleModel.GetModel(Id);
            if (platMsgComment == null || platMsgComment.AId != r.Id)
            {
                returnObj.Msg = "帖子评论不存在";
                return Json(returnObj);
            }

            platMsgComment.State = -1;
           if(PlatMsgCommentBLL.SingleModel.Update(platMsgComment, "State"))
            {
                returnObj.isok = true;
                returnObj.Msg = "操作成功";
                return Json(returnObj);
            }
            else
            {
                returnObj.Msg = "操作失败";
                return Json(returnObj);
            }


           


        }


        /// <summary>
        ///用户举报帖子
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult AddReportMsg()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Utility.IO.Context.GetRequest("appId", string.Empty);
            int msgId = Utility.IO.Context.GetRequestInt("msgId", 0);
            int userId = Utility.IO.Context.GetRequestInt("mycardId", 0);
            string reportReason = Utility.IO.Context.GetRequest("reportReason", string.Empty);
            if (string.IsNullOrEmpty(appId) || msgId <= 0 || userId <= 0 || string.IsNullOrEmpty(reportReason))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {

                returnObj.Msg = "小程序未授权";
                return Json(returnObj);
            }
            PlatMsgReport platMsgReport = new PlatMsgReport();
           platMsgReport.AddTime = DateTime.Now;
           platMsgReport.Aid = r.Id;
           platMsgReport.MsgId = msgId;
           platMsgReport.ReportReason = HttpUtility.HtmlEncode(reportReason);//编码保存
            platMsgReport.ReportcardId = userId;

            int id = Convert.ToInt32(PlatMsgReportBLL.SingleModel.Add(platMsgReport));
            if (id <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            returnObj.isok = true;
            returnObj.Msg = "举报成功";
            returnObj.dataObj = new { id = id };
            return Json(returnObj);


        }






    }
}