using BLL.MiniApp; 
using Entity.MiniApp;  
using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Threading.Tasks;
using System.Web.Mvc; 

namespace Api.MiniApp.Controllers
{
    public class apiExceptionLogController : InheritController
    {

    }
    [ExceptionLog]
    public class apiMiniAppExceptionLogController : apiExceptionLogController
    {    

        public async Task<ActionResult> SendEmail(Entity.MiniApp.CommandExceptionLog exLog)
        {
           
            if (exLog==null||string.IsNullOrEmpty(exLog.AppId) || string.IsNullOrEmpty(exLog.Version) || string.IsNullOrEmpty(exLog.SourcePath) || string.IsNullOrEmpty(exLog.ExceptionMsg))
            {
                return Json(new { isok = false, msg = "参数错误" });
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(exLog.AppId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "请先授权小程序模板" });
            }
         string body=System.IO.File.ReadAllText(Server.MapPath("/XcxExceptionLogEmail.html"));
            // 异步发送邮件
            List<string> toUser = WebSiteConfig.XcxAppReceiveEmail.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();           
            await Task.Run(() => { CommandExceptionLogBLL.SingleModel.Send(exLog,toUser,body); });
            return Json(new { isok = true, msg = "请求成功" });
        }  
    }
}