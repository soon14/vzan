
using Entity.MiniApp.Conf;
using System.Collections.Generic;

namespace BLL.MiniApp.Conf
{
    public class SaveMoneySetBLL : DAL.Base.BaseMySql<SaveMoneySet>
    {
        #region 单例模式
        private static SaveMoneySetBLL _singleModel;
        private static readonly object SynObject = new object();

        private SaveMoneySetBLL()
        {

        }

        public static SaveMoneySetBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SaveMoneySetBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 返回当前用户模板下储值设定
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public List<SaveMoneySet> getListByAppId(string appId, int State = -999,int pageIndex = 1,int pageSize = 100)
        {
            string strSql = ($" appId = '{appId}' {(State == -999 ? " and State >= 0 " : $"  and State = {State}  ")} ");
            return GetList($" appId = '{appId}' {(State == -999 ? " and State >= 0 " : $"  and State = {State}  ")} ", pageSize, pageIndex,"*"," Id desc ");
        }
            
        /// <summary>
        /// 返回当前用户模板下储值设定
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public int getCountByAppId(string appId, int State = -999, int pageIndex = 1, int pageSize = 10)
        {
            return GetCount($" appId = '{appId}' {(State == -999 ? " and State >= 0 " : $"  and State = {State}  ")} ");
        }


        public SaveMoneySet GetModel(int Id, string appId)
        {
            return GetModel($" Id = {Id} and appId = '{appId}' ");
        }

    }
}
