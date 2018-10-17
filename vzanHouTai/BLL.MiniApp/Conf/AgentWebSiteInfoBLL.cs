using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Microsoft.Web.Administration;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text.RegularExpressions;

namespace BLL.MiniApp.Conf
{
    /// <summary>
    /// 代理商网站页面
    /// </summary>
    public class AgentWebSiteInfoBLL : BaseMySql<AgentWebSiteInfo>
    {
        #region 单例模式
        private static AgentWebSiteInfoBLL _singleModel;
        private static readonly object SynObject = new object();

        private AgentWebSiteInfoBLL()
        {

        }

        public static AgentWebSiteInfoBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AgentWebSiteInfoBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private readonly string HostName = "localhost";


        /// <summary>
        /// 在小程序网站上绑定指定域名与解除指定域名
        /// </summary>
        /// <param name="listDomain">需要绑定的域名集合</param>
        /// <param name="listClearDomin">需要解除绑定的域名集合</param>
        /// <returns></returns>
        public bool BindDominInWebSite(List<string> listDomain,List<string> listClearDomin,int domainType=0)
        {
            string entPath = string.Format("IIS://{0}/w3svc", HostName);
            DirectoryEntry rootEntry = GetDirectoryEntry(entPath);
            string newSiteNum = string.Empty;
            if (RootSiteEnavaible(WebSiteConfig.DzWebSiteBindString))
            {
                newSiteNum = GetWebSiteNum(WebSiteConfig.DzWebSiteName);
                DirectoryEntry newSiteEntry = rootEntry.Children.Find(newSiteNum, "IIsWebServer");
                string bindHost = string.Empty;

                try
                {
                    //先解绑
                    foreach (string domain in listClearDomin)
                    {
                        bindHost = string.Format($":80:{domain}");
                        if (newSiteEntry.Properties["ServerBindings"].Contains(bindHost))
                        {
                            //网站有当前要解绑的域名再解绑
                            newSiteEntry.Properties["ServerBindings"].Remove(bindHost);
                        }
                    }
                    newSiteEntry.CommitChanges();

                    //然后再重新绑定当前需要绑定的域名
                    if (domainType == 0)//只有自定义域名才进行主机头绑定到IIS网站
                    {
                        foreach (string domain in listDomain)
                        {
                            bindHost = string.Format($":80:{domain}");
                            if (!newSiteEntry.Properties["ServerBindings"].Contains(bindHost))
                            {
                                //网站没有添加当前域名再进行绑定
                                if (newSiteEntry.Properties["ServerBindings"].Add(bindHost) <= 0)
                                {
                                    return false;
                                }
                            }
                        }
                        newSiteEntry.CommitChanges();
                    }

                 
                    return true;
                }catch( Exception ex)
                {
                    log4net.LogHelper.WriteError(this.GetType(),ex);
                }

            }
            return false;

        }


        /// <summary>
        /// 根据指定域名返回当前绑定的代理商网站 如果有则返回代理商网站信息否则返回NULL
        /// </summary>
        /// <param name="domian"></param>
        /// <returns></returns>
        public AgentWebSiteInfo GetModelByDomian(string domian)
        {
           
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@domian", $"%{domian}%"));
            List<AgentWebSiteInfo> listAgentWebSiteInfo = GetListByParam($"domian like @domian", parameters.ToArray());

            if (listAgentWebSiteInfo != null && listAgentWebSiteInfo.Count > 0)
            {
                foreach (AgentWebSiteInfo item in listAgentWebSiteInfo)
                {

                   string[] domianArry= item.domian.Split(new char[] {';'},StringSplitOptions.RemoveEmptyEntries);
                    if (domianArry != null && domianArry.Length > 0)
                    {
                        foreach(string domainItem in domianArry)
                        {
                            if (domainItem == domian)
                            {
                                //表示找到了绑定当前域名的代理商网站信息
                                return item;
                            }
                        }
                    }
                    
                }
            }

            return null;
        }
        
        /// <summary>
        /// 获取一个网站的编号。根据网站的ServerBindings或者ServerComment来确定网站编号
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns>返回网站的编号如果为空则表示没有找到网站</returns>
        public string GetWebSiteNum(string siteName)
        {
            Regex regex = new Regex(siteName);
            string tmpStr;
            string entPath = String.Format("IIS://{0}/w3svc", HostName);
            DirectoryEntry ent = GetDirectoryEntry(entPath);
            foreach (DirectoryEntry child in ent.Children)
            {
                if (child.SchemaClassName == "IIsWebServer")
                {
                    if (child.Properties["ServerBindings"].Value != null)
                    {
                        tmpStr = child.Properties["ServerBindings"].Value.ToString();
                        if (regex.Match(tmpStr).Success)
                        {
                            return child.Name;
                        }
                    }
                    if (child.Properties["ServerComment"].Value != null)
                    {
                        tmpStr = child.Properties["ServerComment"].Value.ToString();
                        if (regex.Match(tmpStr).Success)
                        {
                            return child.Name;
                        }
                    }
                }
            }
            Console.WriteLine($"没有找到{siteName}网站");
            return string.Empty;
        }




        /// <summary>
        /// 确定宿主机是否存在
        /// </summary>
        /// <param name="bindStr"></param>
        /// <returns></returns>
        public bool RootSiteEnavaible(string bindStr)
        {
            string entPath = String.Format("IIS://{0}/w3svc", HostName);
            DirectoryEntry ent = GetDirectoryEntry(entPath);
            foreach (DirectoryEntry child in ent.Children)
            {
                if (child.SchemaClassName == "IIsWebServer")
                {
                    if (child.Properties["ServerBindings"].Value != null)
                    {
                        if (child.Properties["ServerBindings"].Contains(bindStr))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// 根据是否有用户名来判断是否是远程服务器。
        /// 然后再构造出不同的DirectoryEntry出来
        /// </summary>
        /// <param name="entPath">DirectoryEntry的路径</param>
        /// <returns>返回的是DirectoryEntry实例</returns>
        public DirectoryEntry GetDirectoryEntry(string entPath, string UserName = "", string Password = "")
        {
            DirectoryEntry ent;
            if (UserName == null)
            {
                ent = new DirectoryEntry(entPath);
            }
            else
            {
                // ent = new DirectoryEntry(entPath, HostName+"\\"+UserName, Password, AuthenticationTypes.Secure);
                ent = new DirectoryEntry(entPath, UserName, Password, AuthenticationTypes.Secure);
            }
            return ent;
        }






    }

}
