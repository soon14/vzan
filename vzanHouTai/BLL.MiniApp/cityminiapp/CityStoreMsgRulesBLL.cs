using DAL.Base;
using Entity.MiniApp.cityminiapp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.cityminiapp
{
    public class CityStoreMsgRulesBLL : BaseMySql<CityStoreMsgRules>
    {
        #region 单例模式
        private static CityStoreMsgRulesBLL _singleModel;
        private static readonly object SynObject = new object();

        private CityStoreMsgRulesBLL()
        {

        }

        public static CityStoreMsgRulesBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CityStoreMsgRulesBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<CityStoreMsgRules> getListByaid(int aid, out int totalCount,int pageSize=10,int pageIndex=1)
        {
            string strWhere = $"aid={aid} and state=0";
            List<CityStoreMsgRules> list = base.GetList(strWhere,pageSize,pageIndex,"*", " updateTime desc");
            totalCount = base.GetCount(strWhere);
            return list;

        }

        public CityStoreMsgRules getCity_StoreMsgRules(int aid,int ruleId)
        {
            return base.GetModel($"aid={aid} and Id={ruleId} and state=0");
        }




    }
}
