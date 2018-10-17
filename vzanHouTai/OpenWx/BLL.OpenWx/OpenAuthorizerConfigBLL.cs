using DAL.Base;
using Entity.OpenWx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.OpenWx
{
    public class OpenAuthorizerConfigBLL : BaseMySql<OpenAuthorizerConfig>
    {
        #region 单例模式
        private static OpenAuthorizerConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private OpenAuthorizerConfigBLL()
        {

        }

        public static OpenAuthorizerConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new OpenAuthorizerConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public OpenAuthorizerConfig GetModelByAppid(string appid, int rid = 0)
        {
            string sql = $"appid ='{appid}'";
            if (rid > 0)
            {
                sql += $" and RId={rid}";
            }
            var info = GetModel(sql);

            return info;
        }
    }
}
