using BLL.MiniApp;
using System;
using System.IO;
using System.Web.Mvc;

namespace User.MiniApp.Controllers
{
    public class wxController : baseController
    {
        /// <summary>
        /// 微信消息推送
        /// </summary>
        /// <returns></returns>
        public ActionResult wxcheck(int id = 0)
        {
            try
            {
                WeixinServer _wx = new WeixinServer();

                if (Request.HttpMethod.ToUpper() == "POST")
                {
                    using (StreamReader stream = new StreamReader(Request.InputStream))
                    {
                        string xml = stream.ReadToEnd();
                        //log4net.LogHelper.WriteInfo(this.GetType(), "微信事件推送：" + xml);
                        if (!string.IsNullOrEmpty(xml))
                        {
                            _wx.processRequest(xml, id);
                            Response.End();
                        }
                    }
                }
                else
                {
                    _wx.Auth(string.Empty);
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
            return View();
        }
    }
}