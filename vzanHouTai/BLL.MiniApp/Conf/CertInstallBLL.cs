using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Conf
{
    public class CertInstallBLL : BaseMySql<CertInstall>
    {
        #region 单例模式
        private static CertInstallBLL _singleModel;
        private static readonly object SynObject = new object();

        private CertInstallBLL()
        {

        }

        public static CertInstallBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CertInstallBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<CertInstall> GetListByNoInstall()
        {
            string sqlWhere = $"state>=0";
            return base.GetList(sqlWhere);
        }

        public CertInstall GetModelByAidAndAppId(int aid,string appid)
        {
            string sqlWhere = $"aid={aid} and appid='{appid}' and ProjectType={(int)ProjectType.小程序}";
            return base.GetModel(sqlWhere);
        }
    }
}
