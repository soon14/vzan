using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace User.MiniApp.Model
{
    public class ViewModel<T>
    {
        public List<T> DataList { get; set; } = new List<T>();
        public T DataModel { get; set; }
        public int PageSize { get; set; } = 20;
        public int PageIndex { get; set; } = 1;
        public int TotalCount { get; set; }
        public int PageCount { get; set; }
        public string Msg { get; set; }
        public readonly Dictionary<string, string> PageTypeDic = new Dictionary<string, string>() {
            {"12","行业基本版" },
            {"22","专业版" },
        };
        public int PageType { get; set; } = 0;
        public string PageTypeName
        {
            get
            {
                if (!PageTypeDic.ContainsKey(PageType.ToString()))
                    return string.Empty;
                return PageTypeDic[PageType.ToString()];
            }
        }

        public List<UserRole> CurrentUserRoles { get; set; }

        public object extraConfig { get; set; }

        public int SecondTypeSwitch = 0;

        public int TypeIndex = -1;
        public List<T> FirstDataList { get; set; } = new List<T>();

        public string pageSetting { get; set; }
        public int VersionId { get; set; }
        public int AgentId { get; set; }
        public int AId { get; set; }
        public int ModelTemplateId { get; set; }

        public int aId { get; set; } = 0;
        public int storeId { get; set; } = 0;
    }
}