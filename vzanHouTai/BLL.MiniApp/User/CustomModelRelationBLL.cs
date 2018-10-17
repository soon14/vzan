using BLL.MiniApp.Ent;
using BLL.MiniApp.Tools;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
/// <summary>
/// Member转移过来的
/// </summary>
namespace BLL.MiniApp
{
    public class CustomModelRelationBLL : BaseMySql<CustomModelRelation>
    {
        #region 单例模式
        private static CustomModelRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private CustomModelRelationBLL()
        {

        }

        public static CustomModelRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CustomModelRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private readonly string _redis_CustomModelRelationKey = "redis_CustomModelRelation_{0}_{1}_{2}";
        public readonly string _redis_CustomModelRelationVersion = "redis_CustomModelRelationVersion";//版本控制
        public readonly string _redis_CustomModelRelationUpGradeKey = "redis_CustomModelRelationUpGrade_{0}";//代理商在小程序管理页面中点击升级时的缓存

        public CustomModelRelation GetModelByAid(int aid)
        {
            return base.GetModel($"aid={aid}");
        }
        public CustomModelRelation GetModelByDataType(int datatype=1)
        {
            return base.GetModel($"datatype={datatype} and state=1");
        }

        public List<CustomModelRelation> GetListByAIds(string aids)
        {
            if(aids.Length<=0)
            {
                return new List<CustomModelRelation>();
            }
            return base.GetList($"aid in ({aids})");
        }
        
        public List<CustomModelRelation> GetCustomModelRelationList(int aid,int pageSize,int pageIndex,ref int count, bool reflesh=true)
        {
            RedisModel<CustomModelRelation> model = RedisUtil.Get<RedisModel<CustomModelRelation>>(string.Format(_redis_CustomModelRelationKey, aid, pageSize, pageIndex));
            int dataversion = RedisUtil.GetVersion(string.Format(_redis_CustomModelRelationVersion));

            if (reflesh || model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                model = new RedisModel<CustomModelRelation>();
                List<CustomModelRelation> list = new List<CustomModelRelation>();
                string sql = $@"select cm.*,xr.versionid,cur.custommodelid from custommodelrelation cm
                            left join xcxappaccountrelation xr on cm.aid = xr.id
                            left join (select * from custommodeluserrelation where aid={aid}) cur on cur.custommodelid = cm.id ";
                string sqlcount = $@"select Count(*) from custommodelrelation cm";
                string sqlwhere = $" where cm.state =1";
                string sqlorderby = $" order by cur.custommodelid desc,cm.updatetime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";
                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere + sqlorderby, null))
                {
                    while (dr.Read())
                    {
                        CustomModelRelation amodel = base.GetModel(dr);
                        if (dr["custommodelid"] != DBNull.Value)
                        {
                            amodel.CustommodelId = Convert.ToInt32(dr["custommodelid"]);
                        }
                        if(dr["versionid"] !=DBNull.Value)
                        {
                            amodel.VersionId = Convert.ToInt32(dr["versionid"]);
                        }
                        list.Add(amodel);
                    }
                }
                if(list==null && list.Count<=0)
                {
                    return new List<CustomModelRelation>();
                }

                count = base.GetCountBySql(sqlcount + sqlwhere);
                model.DataList = list;
                model.DataVersion = dataversion;
                model.Count = count;
                if (!reflesh)
                {
                    RedisUtil.Set<RedisModel<CustomModelRelation>>(string.Format(_redis_CustomModelRelationKey, aid, pageSize, pageIndex), model);
                }
            }
            else
            {
                count = model.Count;
            }
            return model.DataList;
        }
        
        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="agentid"></param>
        public void RemoveCache()
        {
            RedisUtil.SetVersion(_redis_CustomModelRelationVersion);
        }

        public int GetUpGradeAid(int agentId)
        {
            int dataid = 0;
            if (agentId > 0)
            {
                dataid= RedisUtil.Get<int>(string.Format(_redis_CustomModelRelationUpGradeKey, agentId));
            }
            RemoveUpGradeAid(agentId);
            return dataid;
        }
        public void SaveUpGradeAid(int aid,int agentId)
        {
            if(aid>0&& agentId>0)
            {
                RedisUtil.Set<int>(string.Format(_redis_CustomModelRelationUpGradeKey, agentId), aid);
            }
        }
        public void RemoveUpGradeAid(int agentId)
        {
            if (agentId > 0)
            {
                RedisUtil.Remove(string.Format(_redis_CustomModelRelationUpGradeKey, agentId));
            }
        }
    }
}