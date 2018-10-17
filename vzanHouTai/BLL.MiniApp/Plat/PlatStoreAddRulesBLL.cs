using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
   public class PlatStoreAddRulesBLL : BaseMySql<PlatStoreAddRules>
    {
        #region 单例模式
        private static PlatStoreAddRulesBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatStoreAddRulesBLL()
        {

        }

        public static PlatStoreAddRulesBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatStoreAddRulesBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<PlatStoreAddRules> getListByaid(int aid, out int totalCount, int pageSize = 10, int pageIndex = 1)
        {
            string strWhere = $"aid={aid} and state=0";
            List<PlatStoreAddRules> list = base.GetList(strWhere, pageSize, pageIndex, "*", " updateTime desc");
            totalCount = base.GetCount(strWhere);
            return list;

        }
        public int GetRuleCount(int aid)
        {
            return base.GetCount($"aid={aid} and state=0");
        }
        public PlatStoreAddRules getRule(int aid, int ruleId)
        {
            return base.GetModel($"aid={aid} and Id={ruleId} and state=0");
        }

        /// <summary>
        /// 获取指定年限的规则
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="yearCount"></param>
        /// <returns></returns>
        public PlatStoreAddRules GetRuleByYearCount(int aid, int yearCount,int ruleId)
        {
            return base.GetModel($"aid={aid} and YearCount={yearCount} and state=0 and Id<>{ruleId}");
        }

    }
}
