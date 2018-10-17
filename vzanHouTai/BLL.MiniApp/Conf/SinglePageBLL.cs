using DAL.Base;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Conf
{
    public class SinglePageBLL : BaseMySql<SinglePage>
    {
        #region 单例模式
        private static SinglePageBLL _singleModel;
        private static readonly object SynObject = new object();

        private SinglePageBLL()
        {

        }

        public static SinglePageBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SinglePageBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private const string MiniappSinglePageverkey = "MiniappSinglePagever_{0}";
        private const string MiniappSinglePagelistkey = "MiniappSinglePagelist_{0}_{1}_{2}";
        public List<SinglePage> GetListByRelationIdByCash(int id,int pageindex,int pagesize)
        {
            //要查询的版本
            int ver = RedisUtil.GetVersion(string.Format(MiniappSinglePageverkey, id));
            string key = string.Format(MiniappSinglePagelistkey, id,pageindex,pagesize);
            var info = RedisUtil.Get<List<SinglePage>>(key);
            if(info == null || info.Count<=0)
            {
                info = GetList($"RelationId='{id}' and State>=0", pagesize, pageindex,"Id Desc");
                if (info != null && info.Count > 0)
                {
                    RedisUtil.Set(key, info, TimeSpan.FromHours(5));
                }
            }
            
            return info;
        }
        public List<SinglePage> GetListByRelationId(int id, int pageindex, int pagesize)
        {
            var info = GetList($"RelationId='{id}' and State>=0", pagesize, pageindex,"", "Id Desc");

            return info;
        }
        public int GetCounttByRelationId(int id)
        {
            var info = GetCount($"RelationId='{id}' and State>=0");
            return info;
        }

        public List<SinglePage> GetListByUserIdandRid(int userId,int relationId,int pageindex,int pagesize)
        {
            var info = GetList($"RelationId='{relationId}' and UserId={userId}",pagesize,pageindex);
            return info;
        }

        ///// <summary>
        ///// 返回-4是重复提交
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //public override object Add(MiniappSinglePage model)
        //{
        //    object o = base.Add(model);
        //    return o;
        //}
        //public override bool Update(MiniappSinglePage model)
        //{
        //    bool b = base.Update(model);
        //    RemoveCache(model);
        //    return b;
        //}

        
    }
}
