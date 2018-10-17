
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Tools
{
    public class GroupSponsorBLL : BaseMySql<GroupSponsor>
    {

        public static readonly string cacheKey = "MiniappGroupSponsor_{0}";

        #region 单例模式
        private static GroupSponsorBLL _singleModel;
        private static readonly object SynObject = new object();

        private GroupSponsorBLL()
        {

        }

        public static GroupSponsorBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new GroupSponsorBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public  List<GroupSponsor> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<GroupSponsor>();

            return base.GetList($"id in ({ids})");
        }

        /// <summary>
        /// 获取同一个拼团商品还能参团的拼团数据，按最少需要参团人数和快参团结束日期排序
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public List<GroupSponsor> GetListJoiningGroup(int groupid,int length)
        {
            List<GroupSponsor> storediscountList = new List<GroupSponsor>();
            string wheresql = $"select ms.*, (ms.GroupSize - (SELECT count(*) from groupuser where ms.Id=GroupSponsorId and state not in ({(int)MiniappPayState.取消支付},{(int)MiniappPayState.待支付}))) neednum from GroupSponsor ms where GroupId={groupid} and State=1 and EndDate>now() ORDER BY neednum asc,EndDate DESC LIMIT "+length;
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, wheresql, null))
            {
                while (dr.Read())
                {
                    var model = GetModel(dr);
                    if(DBNull.Value!= dr["neednum"])
                    {
                        model.NeedNum = Convert.ToInt32(dr["neednum"].ToString());//还差几人成团
                    }
                    
                    storediscountList.Add(model);
                }
            }
            return storediscountList;
        }
        
        public void RemoveCache(int Id)
        {
            RedisUtil.Remove(string.Format(cacheKey, Id));
        }
        public override object Add(GroupSponsor model)
        {
            object o=  base.Add(model);
            RemoveCache(model.Id);
            return o;
        }
        public override bool Update(GroupSponsor model, string columnFields)
        {
            bool b = base.Update(model, columnFields);
            RemoveCache(model.Id);
            return b;
        }
        public override bool Update(GroupSponsor model)
        {
            bool b = base.Update(model);
            RemoveCache(model.Id);
            return b;
        }
    }
}
