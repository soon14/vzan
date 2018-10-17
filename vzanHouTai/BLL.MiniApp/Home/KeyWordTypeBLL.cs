using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Home;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace BLL.MiniApp.Home
{
    public class KeyWordTypeBLL : BaseMySql<KeyWordType>
    {
        private readonly string _redis_KeyWordTypeListKey = "KeyWordTypeListKey";
        private readonly string _redis_KeyWordTypeListVersion = "KeyWordTypeListVersion";//版本控制

        #region 单例模式
        private static KeyWordTypeBLL _singleModel;
        private static readonly object SynObject = new object();

        private KeyWordTypeBLL()
        {

        }

        public static KeyWordTypeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new KeyWordTypeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<KeyWordType> GetListByParentId(int parentId, int state)
        {
            RedisModel<KeyWordType> model = RedisUtil.Get<RedisModel<KeyWordType>>(_redis_KeyWordTypeListKey);
            int dataversion = RedisUtil.GetVersion(_redis_KeyWordTypeListVersion);
            if (model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                model = new RedisModel<KeyWordType>();
                string sqlWhere = $"parentid={parentId} and state>={0}";
                model.DataList = base.GetList(sqlWhere);
                RedisUtil.Set<RedisModel<KeyWordType>>(_redis_KeyWordTypeListKey, model);
            }

            return model.DataList;
        }
        
        /// <summary>
        /// 判断是否有重复的名称
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ExitModelByName(string name, int id)
        {
            MySqlParameter[] param = new MySqlParameter[] { new MySqlParameter("@name", name) };
            string sqlWhere = $"name=@name and id !={id} and state>=0";
            return base.Exists(sqlWhere, param);
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="agentid"></param>
        public void RemoveCache()
        {
            RedisUtil.SetVersion(_redis_KeyWordTypeListVersion);
        }
    }
}
