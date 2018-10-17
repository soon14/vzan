using Entity.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.User
{
    [SqlTable(dbEnum.MINIAPP)]
    public class AuthGroup
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public DateTime AddTime { get; set; }
        [SqlField]
        public string Name { get; set; } = string.Empty;
        [SqlField]
        public string Remark { get; set; } = string.Empty;
        [SqlField]
        public int State { get; set; }
        [SqlField]
        public int Aid { get; set; }
        [SqlField]
        public string NavMenu { get; set; } = string.Empty;
        public List<AuthMenu> GetAuthMenu()
        {
            return !string.IsNullOrWhiteSpace(NavMenu) ? JsonConvert.DeserializeObject<List<AuthMenu>>(NavMenu) : new List<AuthMenu>();
        }
    }

    public class AuthMenu
    {
        public int ItemId { get; set; }
        public bool Read { get; set; }
        public bool Write { get; set; }
    }

    public class AuthInfo
    {
        public bool MasterAccess { get; set; }
        public string AuthName { get; set; }
        public AuthRole AuthAdmin { get; set; }
        public List<AuthMenu> AuthMenu { get; set; } = new List<AuthMenu>();
        public List<NavMenu> AllMenu { get; set; } = new List<NavMenu>();
        public string CurrRoute { get; set; } = string.Empty;
        public bool CheckRouteAccess(string url = null)
        {
            string checkUrl = !string.IsNullOrWhiteSpace(url) ? url : CurrRoute;
            if (string.IsNullOrWhiteSpace(checkUrl) && AuthMenu.Count > 0)
            {
                return false;
            }
            List<NavMenu> DeniedAccess = AllMenu.FindAll(allItem => !AuthMenu.Exists(authItem => authItem.ItemId == allItem.Id));
            return !DeniedAccess.Exists(menu => checkUrl == menu.Url);
        }
    }
}
