using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Text;
using System.Web.Mvc.Html;
using System.Collections.Specialized;

namespace Core.MiniApp
{
    public static class MvcPager
    {
        /// <summary>  
        /// 分页Pager显示  
        /// </summary>   
        /// <param name="html">this.Html</param>  
        /// <param name="currentPageStr">标识当前页码的QueryStringKey</param>   
        /// <param name="pageSize">每页显示</param>  
        /// <param name="totalCount">总数据量</param>  
        /// <returns></returns> 
        public static MvcHtmlString Pager(this HtmlHelper html, string currentPageStr, int pageSize, int totalCount)
        {
            //得到Get参数集合变量
            // var queryString = null;
            NameValueCollection queryString = html.ViewContext.HttpContext.Request.QueryString;
           
            //当前页  
            int currentPage = 1;
            //总页数  
            int totalPages = Math.Max((totalCount + pageSize - 1) / pageSize, 1);
            //获取路由url参数集合
            RouteValueDictionary dict = new System.Web.Routing.RouteValueDictionary();//html.ViewContext.RouteData.Values
            StringBuilder output = new StringBuilder();
            if (!string.IsNullOrEmpty(queryString[currentPageStr]))
            {
                //与相应的QueryString绑定 
                foreach (string key in queryString.Keys)
                    if (queryString[key] != null && !string.IsNullOrEmpty(key))
                        dict[key] = queryString[key];
                int.TryParse(queryString[currentPageStr], out currentPage);
            }
            else
            {
                //获取 ～/Page/{page number} 的页号参数
                if (dict.ContainsKey(currentPageStr))
                    int.TryParse(dict[currentPageStr].ToString(), out currentPage);
            }

            // 保留查询字符到下一页 
            if (queryString.AllKeys.Count()>0 && queryString.AllKeys[0] != null)
            {
                foreach (string key in queryString.Keys)
                    dict[key] = queryString[key];
            }

            //如果有需要，保留表单值到下一页
            //var formValue = html.ViewContext.HttpContext.Request.Form;
            //foreach (string key in formValue.Keys)
            //    if (formValue[key] != null && !string.IsNullOrEmpty(key))
            //        dict[key] = formValue[key]; 
            UrlHelper urlHelper = new UrlHelper(html.ViewContext.RequestContext);

            if (currentPage <= 0) currentPage = 1;
            if (totalPages > 1)
            {
                //if (currentPage != 1)
                //{
                //    //处理首页连接  
                //    dict[currentPageStr] = 1;
                //    //output.AppendFormat("<li class=\"nex_page\">{0}</li>", html.RouteLink("首页", dict));
                //    output.AppendFormat(@"<li class=""nex_page""><a href=""{0}"">首页</a></li>", urlHelper.RouteUrl(dict));
                //} 
                if (currentPage > 1)
                {
                    //处理上一页的连接  
                    dict[currentPageStr] = currentPage - 1;
                    output.AppendFormat(@"<span class=""nex_page""><a href=""{0}"">上一页</a></span>", urlHelper.RouteUrl(dict));
                }
                else
                {
                    output.Append("<span class=\"disabled\">上一页</span>");
                }
                int currint = 4;
                for (int i = 0; i <= 8; i++)
                {
                    //一共最多显示10个页码，前面5个，后面5个  
                    if ((currentPage + i - currint) >= 1 && (currentPage + i - currint) <= totalPages)
                        if (currint == i)
                        {
                            //当前页处理  
                            output.Append(string.Format("<span class='active'><a href=\"#\" style='background: #198dd2;color: #fff;border: 1px solid #198dd2;'>{0}</a></span>", currentPage));
                        }
                        else
                        {
                            //一般页处理 
                            dict[currentPageStr] = currentPage + i - currint;
                            // output.AppendFormat("<li>{0}</li>", html.RouteLink((currentPage + i - currint).ToString(), dict));
                            output.AppendFormat(@"<span><a href=""{0}"">{1}</a></span>", urlHelper.RouteUrl(dict), (currentPage + i - currint).ToString());
                        }
                }
                if (currentPage < totalPages)
                {
                   
                  
                    dict[currentPageStr] = "replace";
                    output.AppendFormat("<span class=\"page-picker\"><input type=\"text\" custompage onkeyup=\"if(event.keyCode==13){{window.location.href=\'{2}\'+this.value;}}\" address=\"{1}\" class=\"px\" size=\"2\"  ><span> / {0} 页</span></span>", totalPages, urlHelper.RouteUrl(dict), urlHelper.RouteUrl(dict).Replace("replace", ""));
                    dict[currentPageStr] = currentPage + 1;
                    output.AppendFormat(@"<span class=""nex_page""><a href=""{0}"">下一页</a></span>", urlHelper.RouteUrl(dict));
                    //output.AppendFormat("<li class=\"nex_page\">{0}</li>", html.RouteLink("下一页", dict));
                  
                  
                }
                else
                {
                    dict[currentPageStr] = "replace";
                    output.AppendFormat("<span class=\"page-picker\"><input type=\"text\" custompage onkeyup=\"if(event.keyCode==13){{window.location.href=\'{2}\'+this.value;}}\" address=\"{1}\" class=\"px\" size=\"2\"  ><span> / {0} 页</span></span>", totalPages, urlHelper.RouteUrl(dict), urlHelper.RouteUrl(dict).Replace("replace",""));
                    output.Append("<span class=\"disabled\">下一页</span>");
                }
                //if (currentPage != totalPages)
                //{
                //    //处理末页
                //    dict[currentPageStr] = totalPages;
                //    //output.AppendFormat("<li class=\"nex_page\">{0}</li>", html.RouteLink("末页", dict));
                //    output.AppendFormat(@"<li class=""nex_page""><a href=""{0}"">末页</a></li>", urlHelper.RouteUrl(dict));
                //}
            }
            else
            {
                //当前只有一页
                output.Append("<span class=\"disabled\">上一页</span>");
                output.Append(string.Format("<span class=\"font3\">{0}</span>", currentPage));
                output.Append("<span class=\"disabled\">下一页</span>");
            }
            //output.AppendFormat("{0} / {1}", currentPage, totalPages);//这个统计加不加都行 
            return MvcHtmlString.Create(output.ToString());
        }

    }
}