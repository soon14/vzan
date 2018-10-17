using DAL.Base;
using Entity.MiniApp;
using System.Collections.Generic;

namespace BLL.MiniApp
{
    public class PayResultBLL : BaseMySql<PayResult>
    {
        #region 单例模式
        private static PayResultBLL _singleModel;
        private static readonly object SynObject = new object();

        private PayResultBLL()
        {

        }

        public static PayResultBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PayResultBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public int GetMothPayByOpenid(string openid)
        {
            List<PayResult> resultList = GetListBySql($"SELECT SUM(total_fee) as total_fee from payresult where (mch_id = '1275690901' or mch_id IS NULL) and openid = '{openid}' and time_end>= date_format(now(), '%Y-%m') and result_code = 'SUCCESS';");
            if (resultList == null || resultList.Count == 0)
            {
                return 0;
            }
            else
            {
                return resultList[0].total_fee;
            }
        }
        public bool CheckPay(string openid, int fee)
        {
            return true;
            //int sum = GetMothPayByOpenid(openid);
            //return (sum + fee) < 500;
            
        }
    }
}
