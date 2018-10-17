using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BLL.MiniApp;
using Core.MiniApp;
using Newtonsoft.Json;
using User.MiniApp.Comment;
using User.MiniApp.Model;
using Entity.MiniApp;
using System.IO;
using System.Text;
using Utility.AliOss;
using System.Web.Script.Serialization;
using Entity.MiniApp.Tools;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using DAL.Base;
using User.MiniApp.Filters;
using MySql.Data.MySqlClient;
using Entity.MiniApp.Stores;
using BLL.MiniApp.Helper;
using Utility.IO;
using Entity.MiniApp.FunctionList;
using BLL.MiniApp.FunList;

namespace User.MiniApp.Controllers
{
    public class  MiniappGroupsController: groupsController
    {

    }
    public class groupsController : baseController
    {



        public groupsController()
        {



        }
        //团购列表管理
        // GET: CityCoupon

        [LoginFilter][RouteAuthCheck]
        public ActionResult MiniappStoreGroupsManager(int appId, int PageType, int pageIndex = 1, int pageSize = 10, string Title = "", int State = 10)
        {
            if(appId<=0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (PageType <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            
            Store store = StoreBLL.SingleModel.GetModelByRid(appId);
            if(store==null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有找到店铺!", code = "500" });
            }

            #region 专业版 版本控制
           
            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int JoingroupSwtich = 0;//团购开关
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                 versionId = app.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }

                if (!string.IsNullOrEmpty(functionList.ComsConfig))
                {
                    ComsConfig comsConfig = JsonConvert.DeserializeObject<ComsConfig>(functionList.ComsConfig);
                    JoingroupSwtich = comsConfig.Joingroup;
                }

            }
            ViewBag.versionId = versionId;
            ViewBag.JoingroupSwtich = JoingroupSwtich;
            #endregion
            
            List<MySqlParameter> param = new List<MySqlParameter>();

            List<Groups> list = new List<Groups>();
            list = GroupsBLL.SingleModel.GetList($"StoreId={store.Id}",pageSize,pageIndex,"", "CreateDate desc");

            ViewBag.TotalCount = GroupsBLL.SingleModel.GetCount($"StoreId={store.Id}");
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;
            @ViewBag.StoreId = store.Id;
            ViewBag.pageSize = pageSize;

            return View(list);
        }


        //团购购买记录
        [LoginFilter][RouteAuthCheck]
        public ActionResult MiniappUserGroupList(int appId,int PageType,int sgid, int pageIndex = 1, int pageSize = 20)
        {
            string recieveusername = Context.GetRequest("recieveusername", string.Empty);
            string buyusername = Context.GetRequest("buyusername", string.Empty);
            int groupstate = Context.GetRequestInt("groupstate", 10);
            ViewBag.PageType = PageType;
            ViewBag.GroupId = sgid;
            ViewBag.appId = appId;
            ViewBag.pageSize = pageSize;
            ViewBag.buyusername = buyusername;
            ViewBag.groupstate = groupstate;

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if(xcxrelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            string wheresql = $"a.GroupId ={sgid} and a.appid='{xcxrelation.AppId}'";
            if(!string.IsNullOrEmpty(buyusername))
            {
                C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelByNickName(xcxrelation.AppId,buyusername);
                if(userinfo!=null)
                {
                    wheresql += $" and a.ObtainUserId = {userinfo.Id} ";
                }
            }
            if(groupstate==10)//全部，屏蔽未付款成功的记录
            {
                wheresql += $" and a.State not in ({(int)MiniappPayState.待支付}) ";
            }
            else if (groupstate==-1)//已收货
            {
                wheresql += $" and a.State = -1 ";
            }
            else if (groupstate==-2)//成团失败
            {
                wheresql += $" and (b.State = -1 or b.EndDate<now()) and a.State not in ({(int)MiniappPayState.待支付,(int)MiniappPayState.取消支付}) ";
            }
            else if(groupstate==1)//已发货
            {
                wheresql += $" and a.State = 1 ";
            }
            else if(groupstate==0)
            {
                //待发货
                wheresql += $" and a.State = 0 and b.State <> -1 and b.State <> 1 and b.EndDate>now()";
            }
            else if (groupstate == 2)//团购中
            {
                //待发货
                wheresql += $" and b.State =1 and a.State not in ({(int)MiniappPayState.待支付,(int)MiniappPayState.取消支付}) ";
            }

            List<GroupUser> list = GroupUserBLL.SingleModel.getcguRList(wheresql, pageSize, pageIndex);
            if(list!=null && list.Count>0)
            {
                string groupgoodids = string.Join(",",list.Select(s=>s.GroupId).Distinct());
                List<Groups> grouplist = GroupsBLL.SingleModel.GetListByIds(groupgoodids);            
                if(grouplist!=null && grouplist.Count>0)
                {
                    foreach (GroupUser item in list)
                    {
                        Groups groupmodel = grouplist.FirstOrDefault(f => f.Id == item.GroupId);
                        if(groupmodel!=null)
                        {
                            item.Name = groupmodel.GroupName;
                        }
                    }
                }
            }

            ViewBag.TotalCount = GroupUserBLL.SingleModel.GetCountBySql($"select count(*) from groupuser a LEFT join groupsponsor b on a.GroupSponsorId=b.id where {wheresql}");
            return View(list);
        }

        /// <summary>
        /// 审核团购
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteStoreGroup(int type, int id, int storeid = 0)
        {
            MiniappState s = MiniappState.待审核;
            if (type == -1)
            {
                s = MiniappState.删除;

                //查询是否有正在团购的活动
                int groupsporcount = GroupSponsorBLL.SingleModel.GetCount($"GroupId in ({id}) and State=1");
                if (groupsporcount > 0)
                {
                    return Json(new { Success = false, Msg = "删除失败，该商品还有未结束的团购活动" });
                }
            }
            else if (type == 0)
                s = MiniappState.待审核;
            else if (type == 1)
            {
                s = MiniappState.通过;
            }
            else if(type == -2)
            {
                s = MiniappState.彻底删除;
            }

            Groups group = GroupsBLL.SingleModel.GetModel(id);
            if(group==null)
            {
                return Json(new { Success = false, Msg = "删除失败，请刷新重试" });
            }

            group.State = (int)s;
            if (GroupsBLL.SingleModel.Update(group, "state"))
            {
                return Json(new { Success = true, Msg = "成功" },JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { Success = false, Msg = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除团购
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult DeleteStoreGroupsBatch(int type, string[] id, string[] storeid = null)
        {
            MiniappState s = MiniappState.待审核;
            if (type == -1)
            {
                s = MiniappState.删除;
            }
            else if (type == 0)
                s = MiniappState.待审核;
            else if (type == 1)
            {
                s = MiniappState.通过;
            }
            else if (type == -2)
            {
                s = MiniappState.彻底删除;
            }

            if (GroupsBLL.SingleModel.StoreDiscount_DeleteBacth(s, id))
            {
                return Json(new { Success = true, Msg = "成功" });
            }
            else
                return Json(new { Success = false, Msg = "失败" });
        }

        //添加/编辑团购
        [LoginFilter]
        [RouteAuthCheck]
        public ActionResult AddOrEdit(int appId, int PageType, int sgid = 0, int storeId = 0, int tag = 0)
        {
            ViewBag.Vid = 0;
            Groups viewModel = null;
            ViewBag.tag = tag;
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;
            if (sgid > 0)
            {
                viewModel = GroupsBLL.SingleModel.GetModel(sgid);
                if (null == viewModel)
                    return View("PageError", new Return_Msg() { Msg = "没有数据!", code = "500" });
                viewModel.ImgList = C_AttachmentBLL.SingleModel.GetListByCache( sgid ,(int)AttachmentItemType.小程序拼团轮播图);
                viewModel.DescImgList = C_AttachmentBLL.SingleModel.GetListByCache( sgid , (int)AttachmentItemType.小程序拼团详情图);

                ViewBag.CarouselList = viewModel.ImgList;


                List<object> DescImgList = new List<object>();
                foreach (C_Attachment attachment in viewModel.DescImgList)
                {
                    DescImgList.Add(new { id = attachment.id, url = attachment.filepath });
                }
                ViewBag.DescImgList = DescImgList;
                //时间格式转换
                viewModel.ShowTimeS = viewModel.ValidDateStart.ToString("yyyy-MM-dd HH:mm");
                viewModel.ShowTime = viewModel.ValidDateEnd.ToString("yyyy-MM-dd HH:mm");
                viewModel.ShowUseTimeEnd = viewModel.UserDateEnd.ToString("yyyy-MM-dd HH:mm");
                //var store = new C_StoreBLL().GetModel(storeid);
                //if (store == null)
                //{
                //    return View("Error404");
                //}
                return View(viewModel);
            }
            return View(new Groups() { StoreId = storeId, DiscountPrice = 0, UnitPrice = 0, OriginalPrice = 0, CreateNum = 20, ShowTimeS = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                ShowTime = DateTime.Now.AddMonths(1).ToString("yyyy-MM-dd HH:mm"),
                ShowUseTimeEnd = DateTime.Now.AddMonths(2).ToString("yyyy-MM-dd HH:mm")
            });
        }



        /// <summary>
        /// add coupon添加团购
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult addorupdategroup(Groups group, string ImgList = "", string DescImgList = "", bool isEncryption = false, int oldhvid = 0, string videopath = "")
        {
            #region Base64解密
            if (group != null && isEncryption)
            {
                try
                {
                    string strDescription = group.Description.Replace(" ", "+");
                    byte[] bytes = Convert.FromBase64String(strDescription);
                    group.Description = System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                }
            }
            #endregion
            
            //表单验证
            if (group.CreateNum > 999 || group.CreateNum < 1)
            {
                return Json(new { code = -1, msg = "团购的生成数量为1-999！" }, JsonRequestBehavior.AllowGet);
            }
            if (group.GroupSize > 99 || group.GroupSize < 2)
            {
                return Json(new { code = -1, msg = "团购的成团人数为2-99！" }, JsonRequestBehavior.AllowGet);
            }
            if (group.ValidDateEnd < DateTime.Now)
            {
                return Json(new { code = -1, msg = "团购购买结束日期必须大于今天！" }, JsonRequestBehavior.AllowGet);
            }
            if (group.UnitPrice < group.DiscountPrice)
            {
                return Json(new { code = -1, msg = "单买金额必须大于团购金额！" }, JsonRequestBehavior.AllowGet);
            }
            if (group.OriginalPrice < group.UnitPrice)
            {
                return Json(new { code = -1, msg = "原价金额必须大于单买金额！" }, JsonRequestBehavior.AllowGet);
            }

            bool result = false;
            if (group.Id > 0)
            {
                //如果添加了团购，剩余数量添加
                Groups oldModel = GroupsBLL.SingleModel.GetModel(group.Id);
                if (null == oldModel)
                    return Json(new { code = -1, msg = "系统错误！" }, JsonRequestBehavior.AllowGet);
                //if (oldModel.State == 1)
                //    return Json(new { code = -1, msg = "此优惠券已通过审核 , 不能更改该信息 !" });

                int groupUserNum = GroupUserBLL.SingleModel.GetList("GroupId=" + group.Id).Sum(m => m.BuyNum);
                if (groupUserNum > group.CreateNum)
                {
                    return Json(new { code = -1, msg = $"当前团购已售出{groupUserNum}份 , 生成数量不能小于已售出数量！" }, JsonRequestBehavior.AllowGet);
                }
                group.RemainNum = group.CreateNum - groupUserNum;
                string columnField = "ImgUrl,ImageCount,GroupName,ValidDateStart,ValidDateEnd,UserDateEnd,Description,OriginalPrice,GroupSize,CreateNum,LimitNum,RemainNum,virtualSalesCount";
                result = GroupsBLL.SingleModel.Update(group, columnField);
            }
            else
            {
                


                if(group.IsConcern)
                {
                    group.ValidDateStart = DateTime.Now;
                }
                //添加团购
                Store store = StoreBLL.SingleModel.GetModel(group.StoreId);
                
                group.State = (int)MiniappState.通过;
                group.RemainNum = group.CreateNum;
                int id = group.Id = Convert.ToInt32(GroupsBLL.SingleModel.Add(group));
                result = id > 0;
            }
            if (result)
            {
                #region 轮播图
                if (!string.IsNullOrWhiteSpace(ImgList))
                {
                    string[] Imgs = ImgList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (Imgs.Length > 0)
                    {
                        foreach (string img in Imgs)
                        {
                            //判断上传图片是否以http开头，不然为破图-蔡华兴
                            if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http://") == 0)
                            {
                                C_AttachmentBLL.SingleModel.Add(new C_Attachment
                                {
                                    itemId = group.Id,
                                    createDate = DateTime.Now,
                                    filepath = img,
                                    itemType = (int)AttachmentItemType.小程序拼团轮播图,
                                    thumbnail = img,
                                    status = 0
                                });
                            }

                        }
                    }
                }

                List<C_Attachment> groupimglist = C_AttachmentBLL.SingleModel.GetListByCache(group.Id, (int)AttachmentItemType.小程序拼团轮播图);
                if(groupimglist!=null && groupimglist.Count>0)
                {
                    group.ImgUrl = groupimglist[0].filepath;
                    GroupsBLL.SingleModel.Update(group, "ImgUrl");
                }

                #endregion

                #region 详情图

                if (!string.IsNullOrEmpty(DescImgList))
                {
                    string[] imgArray = DescImgList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (imgArray.Length > 0)
                    {
                        foreach (string img in imgArray)
                        {
                            //判断上传图片是否以http开头，不然为破图-蔡华兴
                            if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http://") == 0)
                            {
                                C_AttachmentBLL.SingleModel.Add(new C_Attachment
                                {
                                    itemId = group.Id,
                                    createDate = DateTime.Now,
                                    filepath = img,
                                    itemType = (int)AttachmentItemType.小程序拼团详情图,
                                    thumbnail = img,
                                    status = 0
                                });
                            }

                        }
                    }
                }
                #endregion

                #region 视频
                C_AttachmentVideoBLL.SingleModel.HandleVideoLogicStrategyAsync(videopath, oldhvid, group.Id, (int)AttachmentVideoType.小程序拼团视频);
                #endregion
                return Json(new { code = 1, msg = "操作成功！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { code = -1, msg = "系统错误！" }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// add coupon添加团购
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult AddOrEdite(Groups group, string ImgList = "",bool isEncryption = false)
        {
            #region Base64解密
            if (group != null && isEncryption)
            {
                try
                {
                    string strDescription = group.Description.Replace(" ", "+");
                    byte[] bytes = Convert.FromBase64String(strDescription);
                    group.Description = System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                }
            }
            #endregion

            //表单验证
            if (group.CreateNum > 999 || group.CreateNum < 1)
            {
                return Json(new { code = -1, msg = "团购的生成数量为1-999！" }, JsonRequestBehavior.AllowGet);
            }
            if (group.GroupSize > 99 || group.GroupSize < 2)
            {
                return Json(new { code = -1, msg = "团购的成团人数为2-99！" }, JsonRequestBehavior.AllowGet);
            }
            if (group.ValidDateEnd < DateTime.Now)
            {
                return Json(new { code = -1, msg = "团购购买结束日期必须大于今天！" }, JsonRequestBehavior.AllowGet);
            }
            if(group.specificationdetail==null || group.specificationdetail.Length<=0)
            {
                if (group.UnitPrice < group.DiscountPrice)
                {
                    return Json(new { code = -1, msg = "单买金额必须大于团购金额！" }, JsonRequestBehavior.AllowGet);
                }
                if (group.OriginalPrice < group.UnitPrice)
                {
                    return Json(new { code = -1, msg = "原价金额必须大于单买金额！" }, JsonRequestBehavior.AllowGet);
                }
            }
           
            bool result = false;
            if (group.Id > 0)
            {
                //如果添加了团购，剩余数量添加
                Groups oldModel = GroupsBLL.SingleModel.GetModel(group.Id);
                if (null == oldModel)
                    return Json(new { code = -1, msg = "系统错误！" }, JsonRequestBehavior.AllowGet);

                int groupUserNum = GroupUserBLL.SingleModel.GetList("GroupId=" + group.Id).Sum(m => m.BuyNum);
                if (groupUserNum > group.CreateNum)
                {
                    return Json(new { code = -1, msg = $"当前团购已售出{groupUserNum}份 , 生成数量不能小于已售出数量！" }, JsonRequestBehavior.AllowGet);
                }
                group.RemainNum = group.CreateNum - groupUserNum;
                string columnField = "specificationdetail,pickspecification,ImgUrl,ImageCount,GroupName,ValidDateStart,ValidDateEnd,UserDateEnd,Description,OriginalPrice,GroupSize,CreateNum,LimitNum,RemainNum";
                result = GroupsBLL.SingleModel.Update(group, columnField);
            }
            else
            {
                if (group.IsConcern)
                {
                    group.ValidDateStart = DateTime.Now;
                }
                //添加团购
                Store store = StoreBLL.SingleModel.GetModel(group.StoreId);

                group.State = (int)MiniappState.通过;
                group.RemainNum = group.CreateNum;
                int id = group.Id = Convert.ToInt32(GroupsBLL.SingleModel.Add(group));
                result = id > 0;
            }
            if (result)
            {
                #region 轮播图
                if (!string.IsNullOrWhiteSpace(ImgList))
                {
                    string[] Imgs = ImgList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (Imgs.Length > 0)
                    {
                        foreach (string img in Imgs)
                        {
                            //判断上传图片是否以http开头，不然为破图-蔡华兴
                            if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http://") == 0)
                            {
                                C_AttachmentBLL.SingleModel.Add(new C_Attachment
                                {
                                    itemId = group.Id,
                                    createDate = DateTime.Now,
                                    filepath = img,
                                    itemType = (int)AttachmentItemType.小程序拼团轮播图,
                                    thumbnail = img,
                                    status = 0
                                });
                            }

                        }
                    }
                }
                #endregion
                
                return Json(new { code = 1, msg = "操作成功！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { code = -1, msg = "系统错误！" }, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 修改用户团购订单资料
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult ModifyGroupUser(GroupUser groupUser,string colNameStr)
        {
            Return_Msg return_Msg = new Return_Msg();

            string[] cols = colNameStr.Split(',');
            bool isSuccess = GroupUserBLL.SingleModel.Update(groupUser, string.Join(",", cols));

            return_Msg.isok = isSuccess;
            return_Msg.Msg = isSuccess ? "成功" : "失败";
            return Json(return_Msg);
        }

        /// <summary>
        /// 获取用户团购订单资料
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGroupUser(int groupUserId)
        {
            Return_Msg return_Msg = new Return_Msg();

            GroupUser groupUser =  GroupUserBLL.SingleModel.GetModel(groupUserId);

            return_Msg.isok = groupUser?.Id > 0;
            return_Msg.Msg = return_Msg.isok ? "获取资料成功" : "获取资料失败";
            return_Msg.dataObj = new
            {
                groupUser = groupUser
            };
            return Json(return_Msg);
        }

        #region 团购退款
        //团购退款按钮
        [LoginFilter]
        public ActionResult RefundGroup(int appId,int id)
        {
            GroupUser groupuser = GroupUserBLL.SingleModel.GetModel(id);
            if (groupuser == null)
            {
                return Json(new { isok = -1, msg = "团购信息异常 , 请刷新页面后重试" }, JsonRequestBehavior.AllowGet);
            }
            
            if (groupuser.BuyPrice <= 0)
            {
                return Json(new { isok = -1, msg = "此团购购买价格为0，不需要退款" }, JsonRequestBehavior.AllowGet);
            }
            //if (groupuser.State != (int)MiniappPayState.待发货 || groupuser.State != (int)MiniappPayState.已发货)
            //{
            //    return Json(new { isok = -1, msg = "此团购信息异常，请刷新后重试" }, JsonRequestBehavior.AllowGet);
            //}
            string msg = "";
            try
            {   
                if (!GroupUserBLL.SingleModel.RefundOne(groupuser,ref msg,1))
                {
                    return Json(new { isok = -1, msg = "退款失败"+ msg }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = -1, msg = "退款失败" + msg +"|"+ ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = 1, msg = "退款成功" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 团购发货
        [LoginFilter]
        public ActionResult SendGoods(int appId, int id,string storeremark)
        {
            GroupUser groupuser = GroupUserBLL.SingleModel.GetModel(id);
            if (groupuser == null)
            {
                return Json(new { isok = -1, msg = "团购信息异常 , 请刷新页面后重试" }, JsonRequestBehavior.AllowGet);
            }

            if (groupuser.State != (int)MiniappPayState.待发货)
            {
                return Json(new { isok = -1, msg = "此团购信息异常，请刷新后重试" }, JsonRequestBehavior.AllowGet);
            }

            groupuser.State = (int)MiniappPayState.已发货;
            groupuser.SendGoodTime = DateTime.Now;
            groupuser.StorerRemark = storeremark;
            if(!GroupUserBLL.SingleModel.Update(groupuser))
            {
                return Json(new { isok = -1, msg = "发货失败" }, JsonRequestBehavior.AllowGet);
            }
            
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(groupuser.AppId);
            if (xcx == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"发送模板消息,参数不足,XcxAppAccountRelation_null:appId = {groupuser.AppId}"));
                return Json(new { isok = 1, msg = "发货成功" }, JsonRequestBehavior.AllowGet);
            }

            //发给用户发货通知
            object groupData = TemplateMsg_Miniapp.GroupGetTemplateMessageData(string.Empty, groupuser, SendTemplateMessageTypeEnum.拼团基础版订单发货提醒);
            TemplateMsg_Miniapp.SendTemplateMessage(groupuser.ObtainUserId, SendTemplateMessageTypeEnum.拼团基础版订单发货提醒, xcx.Type, groupData);
            return Json(new { isok = 1, msg = "发货成功" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}