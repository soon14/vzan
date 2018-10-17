using DAL.Base;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Conf
{
    public class VipRuleBLL : BaseMySql<VipRule>
    {
        #region 单例模式
        private static VipRuleBLL _singleModel;
        private static readonly object SynObject = new object();

        private VipRuleBLL()
        {

        }

        public static VipRuleBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new VipRuleBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public bool saveRuleList(List<VipRule> ruleList)
        {
            TransactionModel tranModel = new TransactionModel();
            DateTime timeNow = DateTime.Now;
            bool result = false;
            foreach (VipRule rule in ruleList)
            {
                rule.updatetime = timeNow;
                if (rule.id > 0)
                {
                    tranModel.Add(BuildUpdateSql(rule));
                }
                else
                {
                    rule.addtime = timeNow;
                    tranModel.Add(BuildAddSql(rule));
                }
            }
            try
            {
                result = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            }
            catch (Exception)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(tranModel.sqlArray));
            }
            return result;
        }
    }
}
