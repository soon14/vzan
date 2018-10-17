using BLL.MiniApp;
using BLL.MiniApp.Conf;
using Core.MiniApp;
using Entity.MiniApp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Utility.AliOss;
using Entity.MiniApp.Conf;
using User.MiniApp.Filters;
using Utility.IO;

namespace User.MiniApp.Controllers
{
    public class MiappPageIndexController : pageIndexController
    {


    }
    public class pageIndexController : baseController
    {
        
        


        
        
        
        /// <summary>
        /// 实例化对象
        /// </summary>
        public pageIndexController()
        {
            
            
            
            
            
            
        }



        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult Index(int appId = 0)
        {
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            ViewBag.Id = appId;
            ViewBag.PageIndex = PageIndexControlBLL.SingleModel.GetModel($"RelationId={appId}");

            int TotalCount = PageFormMsgBLL.SingleModel.GetCount($"State<>-1 and Rid={appId}");
            ViewBag.TotalCount = TotalCount;

            return View();
        }


        /// <summary>
        /// 表单数据列表
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult FormDataList(int appId = 0, int pageIndex = 1, int pageSize = 20)
        {
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            ViewBag.Id = appId;

            int TotalCount = 0;
            List<PageFormMsg> vm = new List<PageFormMsg>();

            string strwhere = $"Rid={appId} and state<>-1";
            string act = Context.GetRequest("act", "");
            string start = Context.GetRequest("start", string.Empty);
            string end = Context.GetRequest("end", string.Empty);
            if (act == "search" && !string.IsNullOrEmpty(start))
            {
                if (string.IsNullOrEmpty(end))
                {
                    end = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }


                start = Convert.ToDateTime(start).ToString("yyyy-MM-dd HH:mm:ss");
                end = Convert.ToDateTime(end).ToString("yyyy-MM-dd 23:59:59");

                strwhere += $" and UpdateDate between @start and @end ";

                vm = PageFormMsgBLL.SingleModel.GetListByParam(strwhere, new MySql.Data.MySqlClient.MySqlParameter[] {
                        new MySql.Data.MySqlClient.MySqlParameter("@start",start),
                        new MySql.Data.MySqlClient.MySqlParameter("@end",end)
                    }, pageSize, pageIndex, "*", " id desc ");
                TotalCount = PageFormMsgBLL.SingleModel.GetCount(strwhere, new MySql.Data.MySqlClient.MySqlParameter[] {
                        new MySql.Data.MySqlClient.MySqlParameter("@start",start),
                        new MySql.Data.MySqlClient.MySqlParameter("@end",end)
                    });
            }
            else
            {
                vm = PageFormMsgBLL.SingleModel.GetList(strwhere, pageSize, pageIndex, "*", " id desc ");
                TotalCount = PageFormMsgBLL.SingleModel.GetCount(strwhere);
            }

            ViewBag.pageSize = pageSize;

            ViewBag.TotalCount = TotalCount;
            return View(vm);
        }


        public void appformexport(int appId = 0)
        {


            string act = Context.GetRequest("act", "");
            string ckIds = Context.GetRequest("Ids", string.Empty);
            DataTable exportTable = new DataTable();
            exportTable.Columns.AddRange(new DataColumn[] {
                        new DataColumn("表单名称"),
                        new DataColumn("表单详情"),
                        new DataColumn("提交时间"),
                    });
            string filename = "表单导出" + "-" + DateTime.Now.ToString("yyyy-MM-dd");
            if (act == "all")
            {
                string sql = $"select * from PageFormMsg where  Rid={appId} and state<>-1 order by id desc ";
                DataSet ds = DAL.Base.SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow item in ds.Tables[0].Rows)
                    {
                        DataRow dr = exportTable.NewRow();
                        dr[0] = item["FormTitle"].ToString();
                        dr[1] = item["FormMsg"].ToString().Replace("{", "").Replace("}", "").Replace("\"", "");
                        dr[2] = item["UpdateDate"].ToString();
                        exportTable.Rows.Add(dr);
                    }

                }
            }
            else if (act == "range")
            {
                string start = Context.GetRequest("start", string.Empty);
                string end = Context.GetRequest("end", string.Empty);
                if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
                {
                    Response.Write("请选择开始时间和结束时间");
                    return;
                }
                filename = "表单导出" + start + "到" + end;
                start = Convert.ToDateTime(start).ToString("yyyy-MM-dd HH:mm:ss");
                end = Convert.ToDateTime(end).ToString("yyyy-MM-dd 23:59:59");
                string sql = $"select * from PageFormMsg where  Rid={appId} and state=1 and UpdateDate between @start and @end order by id desc";
                DataSet ds = DAL.Base.SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql, new MySql.Data.MySqlClient.MySqlParameter[] {
                        new MySql.Data.MySqlClient.MySqlParameter("@start",start),
                        new MySql.Data.MySqlClient.MySqlParameter("@end",end)
                    });
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow item in ds.Tables[0].Rows)
                    {
                        DataRow dr = exportTable.NewRow();
                        dr[0] = item["FormTitle"].ToString();
                        dr[1] = item["FormMsg"].ToString().Replace("{", "").Replace("}", "").Replace("\"", "");
                        dr[2] = item["UpdateDate"].ToString();
                        exportTable.Rows.Add(dr);
                    }

                }

            }
            else if (act == "ck")
            {

                if (string.IsNullOrEmpty(ckIds))
                {
                    Response.Write("<script>alert('请先勾选需要导出的数据!');window.opener=null;window.close();</script>");
                    return;

                }
                if (!Utility.StringHelper.IsNumByStrs(',', ckIds))
                {
                    Response.Write("<script>alert('非法操作!');window.opener=null;window.close();</script>");
                    return;
                }

                string sql = $"select * from PageFormMsg where Id in({ckIds}) and  Rid={appId} and state<>-1 order by id desc ";
                DataSet ds = DAL.Base.SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow item in ds.Tables[0].Rows)
                    {
                        DataRow dr = exportTable.NewRow();
                        dr[0] = item["FormTitle"].ToString();
                        dr[1] = item["FormMsg"].ToString().Replace("{", "").Replace("}", "").Replace("\"", "");
                        dr[2] = item["UpdateDate"].ToString();
                        exportTable.Rows.Add(dr);
                    }

                }
            }
            if (exportTable.Rows.Count <= 0)
            {
                Response.Write("没有数据");
                return;
            }

            ExcelHelper<Entity.MiniApp.User.UserForm>.Out2Excel(exportTable, filename);//导出
        }


        /// <summary>
        /// 删除指定表单数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelFormData(int relationId = 0, int Id = 0)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙umodel_null!" }, JsonRequestBehavior.AllowGet);
            }

            if (relationId <= 0)
                return Json(new { isok = false, msg = "relationId不能为空!" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(relationId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "没有权限对象!" }, JsonRequestBehavior.AllowGet);

            }

            if (umodel.AccountId != dzaccount.Id)
            {
                return Json(new { isok = false, msg = "非法操作没有权限删除!" }, JsonRequestBehavior.AllowGet);

            }


            PageIndexControl model = PageIndexControlBLL.SingleModel.GetModel($"RelationId={relationId}");
            if (model == null)
                return Json(new { isok = false, msg = "首页未进行配置没有表单数据" });

            PageFormMsg form = PageFormMsgBLL.SingleModel.GetModel(Id);
            if (form == null)
                return Json(new { isok = false, msg = "没有该表单数据" });

            form.State = -1;
            if (PageFormMsgBLL.SingleModel.Update(form))
                return Json(new { isok = true, msg = "删除成功!" });
            else
                return Json(new { isok = false, msg = "删除失败!" });



        }


        /// <summary>
        /// 删除指定表单数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelFormDataAll(int relationId = 0)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "删除失败(请先登录)!" }, JsonRequestBehavior.AllowGet);
            }

            if (relationId <= 0)
                return Json(new { isok = false, msg = "relationId不能为空!" }, JsonRequestBehavior.AllowGet);
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(relationId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "删除失败(没有权限对象)!" }, JsonRequestBehavior.AllowGet);

            }

            if (umodel.AccountId != dzaccount.Id)
            {
                return Json(new { isok = false, msg = "删除失败(没有权限)!" }, JsonRequestBehavior.AllowGet);

            }


            PageIndexControl model = PageIndexControlBLL.SingleModel.GetModel($"RelationId={relationId}");
            if (model == null)
                return Json(new { isok = false, msg = "首页未进行配置没有表单数据" });


            if (PageFormMsgBLL.SingleModel.ExecuteNonQuery($"update MiniappPageFormMsg set State=-1 where Rid={relationId}") > 0)
                return Json(new { isok = true, msg = "删除成功!" });
            else
                return Json(new { isok = false, msg = "删除失败!" });



        }



        /// <summary>
        /// 设置页面自定义
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetPageIndex(PageIndexControl m)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "1系统繁忙account_null!" }, JsonRequestBehavior.AllowGet);

            }
            if (m.RelationId <= 0)
            {
                return Json(new { isok = false, msg = "权限Id不能为空!" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(m.RelationId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "2系统繁忙umodel_null!" }, JsonRequestBehavior.AllowGet);

            }

            if (umodel.AccountId != dzaccount.Id)
            {
                return Json(new { isok = false, msg = "3系统繁忙umodel_null!" }, JsonRequestBehavior.AllowGet);

            }

            PageIndexControl model = PageIndexControlBLL.SingleModel.GetModel($"RelationId={umodel.Id}");
            if (model != null)
            {

                //表示更新
                if (PageIndexControlBLL.SingleModel.Update(m))
                    return Json(new { isok = true, msg = "更新成功", obj = m.Id });
                else
                    return Json(new { isok = false, msg = "更新异常", obj = m.Id });

            }
            else
            {
                //表示新增
                int id = Convert.ToInt32(PageIndexControlBLL.SingleModel.Add(m));
                if (id > 0)
                    return Json(new { isok = true, msg = "新增成功", obj = id });
                else
                    return Json(new { isok = false, msg = "新增异常" });
            }


        }


        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> UploadVedioAsync()
        {
            try
            {
                var file = System.Web.HttpContext.Current.Request.Files[0];
                if (file.InputStream.Length == 0)
                {
                    return Json(new { isok = false, msg = "上传失败，视频大小不能空", Path = "" }, JsonRequestBehavior.AllowGet);
                }

                if (file.InputStream.Length > 20971520)
                {
                    return Json(new { isok = false, msg = "上传失败，视频文件不能超过20MB", Path = "" }, JsonRequestBehavior.AllowGet);
                }

                string fileType = System.IO.Path.GetExtension(file.FileName).ToLower();
                string[] VideoType = new string[] { ".mpeg4", ".mp4", ".mov", ".avi", ".wmv", ".3gp" };
                if (!VideoType.Contains<string>(fileType.ToLower()))
                {
                    return Json(new { isok = false, msg = "不支持的格式,目前视频格式暂支持.mpeg4,.mp4,.mov,.avi,.wmv,.3gp" }, JsonRequestBehavior.AllowGet);

                }


                byte[] byteData = new byte[file.InputStream.Length];

                string tempurl = string.Empty;
                string tempkey = VideoAliMtsHelper.GetOssVideoKey(fileType.Replace(".", ""), true, out tempurl);
                Stream fileStream = file.InputStream;
                if (null != fileStream)
                {
                    using (System.IO.BinaryReader br = new System.IO.BinaryReader(fileStream))
                    {
                        byteData = br.ReadBytes(file.ContentLength);
                    }
                }
                if (null == byteData)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "视频获取byte[]失败！");
                    return Json(new { isok = false, msg = "系统错误！请联系管理员." });
                }
                bool putrsult = AliOSSHelper.PutObjectFromByteArray(tempkey, byteData, 1, fileType.ToLower());
                Tuple<string, string> pathDic = await C_AttachmentVideoBLL.SingleModel.GetConvertVideoPathAsync(tempkey);
                return Json(new { isok = true, msg = "上传成功", videoconvert = pathDic.Item2, Vid = 0, soucepath = pathDic.Item1 });
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = "系统错误，请重新尝试" + ex.Message, Path = "" });
            }
        }


        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadImg()
        {
            Stream imgStreamWithWaterMark = null;
            try
            {
                var file = System.Web.HttpContext.Current.Request.Files[0];

                Stream filestrem = file.InputStream; string ext = string.Empty;
                if (null != filestrem && file.ContentLength > 0)
                {
                    if (file.ContentLength > 1048576)
                    {
                        return Json(new { Success = false, Msg = "上传失败(请按照规定大小上传)", Path = "" }, JsonRequestBehavior.AllowGet);
                    }
                    imgStreamWithWaterMark = filestrem;
                }// 上传的文件为空
                else
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "UploadImg fileStream为null!");
                    return Json(new { Success = false, Msg = "上传图片失败", Path = "" }, JsonRequestBehavior.AllowGet);
                }
                imgStreamWithWaterMark.Position = 0;//保证流可读
                byte[] byteData = StreamHelpers.ReadFully(imgStreamWithWaterMark);
                Utility.ImgHelper.IsImgageType(byteData, "jpg", out ext);
                //不是图片格式不让上传
                string[] ImgType = new string[] { "jpg", "jpeg", "gif", "png", "bmp" };
                if (!ImgType.Contains<string>(ext.ToLower()))
                {
                    return Json(new { Success = false, Msg = "不支持的格式,目前只支持jpg，jpeg，gif，png，bmp", Path = "" }, JsonRequestBehavior.AllowGet);

                }

                string aliTempImgKey = SaveImageToAliOSS(byteData);
                return Json(new { Success = true, Msg = "上传图片成功", Path = aliTempImgKey }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), ex);
                return Json(new { Success = false, Msg = "上传图片失败", Path = "" }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                if (null != imgStreamWithWaterMark)
                {
                    imgStreamWithWaterMark.Dispose();
                }
            }
        }

        public string SaveImageToAliOSS(byte[] byteArray)
        {
            string aliTempImgKey = string.Empty;
            string aliTempImgFolder = AliOSSHelper.GetOssImgKey("jpg", false, out aliTempImgKey);
            AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, byteArray, 1, ".jpg");
            return aliTempImgKey;
        }

    }
}