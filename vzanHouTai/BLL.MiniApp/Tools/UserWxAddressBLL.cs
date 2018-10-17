using DAL.Base;
using Entity.MiniApp.Tools;
using System;

namespace BLL.MiniApp.Tools
{
    public class UserWxAddressBLL : BaseMySql<UserWxAddress>
    {
        #region 单例模式
        private static UserWxAddressBLL _singleModel;
        private static readonly object SynObject = new object();

        private UserWxAddressBLL()
        {

        }

        public static UserWxAddressBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new UserWxAddressBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public int UpdateUserWxAddress(UserWxAddress m)
        {
            if (m.Id > 0)
                return Update(m, "WxAddress") ? m.Id : 0;
            else
                return Convert.ToInt32(Add(m));
        }
    }
}
