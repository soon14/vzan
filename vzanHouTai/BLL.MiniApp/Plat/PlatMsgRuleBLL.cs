using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
    public class PlatMsgRuleBLL : BaseMySql<PlatMsgRule>
    {
        #region 单例模式
        private static PlatMsgRuleBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatMsgRuleBLL()
        {

        }

        public static PlatMsgRuleBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatMsgRuleBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<PlatMsgRule> GetListByaid(int aid, out int totalCount, int pageSize = 10, int pageIndex = 1)
        {
            string strWhere = $"aid={aid} and state=0";
            List<PlatMsgRule> list = base.GetList(strWhere, pageSize, pageIndex, "*", " updateTime desc");
            totalCount = base.GetCount(strWhere);
            return list;

        }

        public PlatMsgRule GetMsgRules(int aid, int ruleId)
        {
            return base.GetModel($"aid={aid} and Id={ruleId} and state=0");
        }




    }
}
