using DAL.Base;
using Entity.MiniApp.User;
using Utility;

namespace BLL.MiniApp
{
    public class ManagerInfoBLL : BaseMySql<ManagerInfo>
    {
        public bool isValidate(string openId)
        {
            int count = base.GetCount(string.Format("OpenId='{0}' and State<>-1", openId));
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
