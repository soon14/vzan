using Entity.MiniApp;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Utility;

namespace BLL.MiniApp.Conf
{
    public static class MsnModelHelper
    {
        /// <summary>
        ///  构造模拟远程HTTP的POST请求，POST数据Json字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static string DoPostJson(string url, string jsonData)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json;charset=utf-8";
            using (Stream dataStream = request.GetRequestStream())
            {
                byte[] paramBytes = ASCIIEncoding.UTF8.GetBytes(jsonData);
                dataStream.Write(paramBytes, 0, paramBytes.Length);
                dataStream.Close();
            }
            HttpWebResponse res;
            try
            {
                res = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                res = (HttpWebResponse)ex.Response;
            }
            StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
            return sr.ReadToEnd();
        }
        
        #region 组合模板并添加至帐号下的个人模板库
        /// <summary>
        /// 组合模板并添加至帐号下的个人模板库
        /// </summary>
        /// <param name="AppId"></param>
        /// <param name="offset"></param>
        /// <param name="keyword_id_list">[3,4,5]</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static addResultModel addMsnToMy(string AppId, string id ,string[] keyword_id_list, ref string msg)
        {
            addResultModel model = new addResultModel();
            try
            {
                string token = "";
                if (!GetToken(AppId, ref token))
                {
                    model.errcode = -1;
                    model.errmsg = token;
                    msg = token;
                    return model;
                }

                string returnData = "";

                //表示新增
                string postData = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    id = id,
                    keyword_id_list = keyword_id_list
                });
                returnData = DoPostJson("https://api.weixin.qq.com/cgi-bin/wxopen/template/add?access_token=" + token, postData);


                if (string.IsNullOrEmpty(returnData))
                {
                    model.errcode = -1;
                    model.errmsg = "组合模板并添加至帐号下的个人模板库失败";
                    return model;
                }
                model = !string.IsNullOrEmpty(returnData) ?  SerializeHelper.DesFromJson<addResultModel>(returnData) : new addResultModel();
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(MsnModelHelper), ex);
            }

            return model;
        }
        #endregion
        #region 发送模板消息

        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="AppId"></param>
        /// <param name="touser">接收者（用户）的 openid</param>
        /// <param name="template_id">所需下发的模板消息的id</param>
        /// <param name="page">点击模板卡片后的跳转页面，仅限本小程序内的页面。支持带参数,（示例index?foo=bar）。该字段不填则模板无跳转。</param>
        /// <param name="form_id">表单提交场景下，为 submit 事件带上的 formId；支付场景下，为本次支付的 prepay_id</param>
        /// <param name="data">模板内容，不填则下发空模板</param>
        /// <param name="color">模板内容字体的颜色，不填默认黑色</param>
        /// <param name="emphasis_keyword">模板需要放大的关键词，不填则默认无放大</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static MyMsnReturenModel_result sendMyMsn(string AppId, string touser, string template_id, string page, string form_id, object data, string color, string emphasis_keyword, ref string msg)
        {
            msg += 1;
            MyMsnReturenModel_result model = new MyMsnReturenModel_result();
            try
            {
                string token = "";
                if (!GetToken(AppId, ref token))
                {
                    model.errcode = -1;
                    model.errmsg = token;
                    return model;
                }

                string returnData = "";

                //表示新增
                string postData = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    touser = touser,
                    template_id = template_id,
                    page = page,
                    form_id = form_id,
                    data = data,
                    color = color,
                    emphasis_keyword = emphasis_keyword
                });
                returnData = DoPostJson("https://api.weixin.qq.com/cgi-bin/message/wxopen/template/send?access_token=" + token, postData);
                if (string.IsNullOrEmpty(returnData))
                {
                    model.errcode = -1;
                    model.errmsg = "发送模板消息失败";
                    return model;
                }

                model = !string.IsNullOrEmpty(returnData) ? SerializeHelper.DesFromJson<MyMsnReturenModel_result>(returnData) : new MyMsnReturenModel_result();
            }
            catch (Exception)
            {

            }

            return model;
        }
        
        #endregion

        public static bool GetToken(string appId, ref string token)
        {
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appId);
            if (xcxrelation == null)
            {
                token = "模板过期";
                return false;
            }

            if (!XcxApiBLL.SingleModel.GetToken(xcxrelation, ref token))
            {
                return false;
            }

            return true;
        }
    }
    public class addResultModel
    {
        public int errcode = 0;

        public string errmsg = "";

        public string template_id = "";


    }
    #region 删除帐号下的某个模板

    public class MyMsnReturenModel_result
    {
        public int errcode = 0;

        public string errmsg = "";
    }
    #endregion
}
