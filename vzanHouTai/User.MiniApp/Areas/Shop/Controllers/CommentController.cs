using BLL.MiniApp;
using BLL.MiniApp.Dish;
using Core.MiniApp;
using Core.MiniApp.Common;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Shop.Filters;
using User.MiniApp.Areas.Shop.Models;
using Utility;

namespace User.MiniApp.Areas.Shop.Controllers
{
    [RouteArea("Shop"), RoutePrefix("Comment"), Route("{action}")]
    [LoginFilter(storePara: "store", getAuthStore: true)]
    public class CommentController : BaseController
    {
        [HttpGet,Route("List/{storeId?}")]
        public ActionResult List(DishStore store, string productId, int pageIndex = 1, int pageSize = 10)
        {
            List<DishComment> comment = DishCommentBLL.SingleModel.GetListByStore(storeId: store.id, pageIndex: pageIndex, pageSize: pageSize);
            int total = DishCommentBLL.SingleModel.GetCountByStore(store.id);

            string userId = string.Join(",", comment.Select(item => item.uId));
            List<C_UserInfo> users = new List<C_UserInfo>();
            if(!string.IsNullOrWhiteSpace(userId))
            {
                users = C_UserInfoBLL.SingleModel.GetListByIds(userId);
            }
            object formatComment = comment.Select(item => new
            {
                Id = item.id,
                Star = item.star,
                Content = item.content,
                Date = item.addTime.ToString(),
                Imgs = item.imgsList,
                User = item.nickName,
                UserHeadImg = users.FirstOrDefault(user => user.Id == item.uId)?.HeadImgUrl,
            });

            return ApiModel(isok: true, message: "获取评论成功", data: new { page = formatComment, total });
        }

        [HttpPost, Route("Delete/{commentId?}")]
        public JsonResult Delete(DishStore store, int? commentId)
        {
            if (!commentId.HasValue)
            {
                return ApiModel(message: "参数不能为空[commentId]");
            }

            DishComment comment = DishCommentBLL.SingleModel.GetModel(commentId.Value);
            if (comment == null || comment.storeId != store.id)
            {
                return ApiModel(message: "非法操作");
            }

            bool success = DishCommentBLL.SingleModel.DeleteComment(comment);

            return ApiModel(isok: success, message: success ? "删除成功" : $"删除失败[{commentId}]");
        }
    }
}