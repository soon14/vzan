using DAL.Base;
using Entity.MiniApp.Conf;
using System.Collections.Generic;

namespace BLL.MiniApp.Conf
{
    public class ModulsBLL: BaseMySql<Moduls>
    {
        #region 单例模式
        private static ModulsBLL _singleModel;
        private static readonly object SynObject = new object();

        private ModulsBLL()
        {

        }

        public static ModulsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ModulsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<Moduls> GetListByAppidandLevel(int appid,int level,int pageIndex=1,int pageSize=10)
        {
            string sql = $"appId = {appid} and Level = {level} and State = 1";
            return base.GetList(sql,pageSize,pageIndex,"","sort desc,Id Desc");
        }
        public int GetListByAppidandLevelCount(int appid, int level)
        {
            string sql = $"appId = {appid} and Level = {level} and State = 1";
            return GetCount(sql);
        }

        public Moduls GetModelByAppidandLevel(int appid, int level)
        {
            string sql = $"appId = {appid} and Level = {level} and State = 1";
            return base.GetModel(sql);
        }
    }
}
