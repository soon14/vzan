using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.User
{
    public class NavMenuBLL:BaseMySql<NavMenu>
    {
        #region 单例模式
        private static NavMenuBLL _singleModel;
        private static readonly object SynObject = new object();

        private NavMenuBLL()
        {

        }

        public static NavMenuBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new NavMenuBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<NavMenu> GetListByAId(int aId)
        {
            string whereSql = BuildWhereSql(aId: aId);
            if(WebSiteConfig.Environment == "dev")
            {
                whereSql = BuildWhereSql(aId: aId, isGetDelete : true);
            }
            return GetList(whereSql);
        }
        
        /// <summary>
        /// 通过小程序模板类型枚举，获取菜单项
        /// </summary>
        /// <param name="pageType"></param>
        /// <returns></returns>
        public List<NavMenu> GetListByPageType(int pageType)
        {
            string whereSql = BuildWhereSql(pageType:pageType);
            if (WebSiteConfig.Environment == "dev")
            {
                whereSql = BuildWhereSql(pageType:pageType, isGetDelete: true);
            }
            return GetList(whereSql);
        }

        public string BuildWhereSql(int? aId = null, int? pageType = null, string ids = null, bool isGetDelete = false)
        {
            List<string> whereSql = new List<string>();
            if(aId.HasValue)
            {
                whereSql.Add($"AId = {aId.Value}");
            }
            if (!string.IsNullOrWhiteSpace(ids))
            {
                whereSql.Add($"Id IN ({aId.Value})");
            }
            if(pageType.HasValue)
            {
                whereSql.Add($"PageType = {pageType.Value}");
            }
            if (!isGetDelete)
            {
                whereSql.Add($"State > -1");
            }
            return string.Join(" AND ", whereSql);
        }
    }
}
