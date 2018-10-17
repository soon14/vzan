using DAL.Base;
using Entity.MiniApp.User;
using System.Collections.Generic;
/// <summary>
/// Member转移过来的
/// </summary>
namespace BLL.MiniApp
{
    public class RoleBLL : BaseMySql<Role>
    {
        /// <summary>
        /// 获取当前appId,当前店铺可设定的权限角色
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public List<Role> getRoles(int appId, int storeId = 0)
        {
            return GetList($" (appId = {appId} or IFNULL(appId,0) = 0)  AND  (storeId = {storeId} or IFNULL(storeId,0) = 0) ");
        }

    }
}