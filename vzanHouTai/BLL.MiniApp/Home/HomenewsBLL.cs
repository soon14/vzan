//using Core.MiniSNS;
using DAL.Base;
using Entity.MiniApp.Home;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BLL.MiniApp.Home
{
    public class HomenewsBLL : BaseMySql<Homenews>
    {
        #region 单例模式
        private static HomenewsBLL _singleModel;
        private static readonly object SynObject = new object();

        private HomenewsBLL()
        {

        }

        public static HomenewsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new HomenewsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        string newslistkey = "HomenewsList"; 
        string bknewslistkey = "bknewslist";
        /// <summary>
        /// 获取百科信息列表
        /// </summary>
        /// <param name="strWhere"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderFiled"></param>
        /// <returns></returns>
        public DataTable GetBknews(string title, int type, int childnode, int state, int pageIndex, int pageSize, string orderFiled,out int totalcount)
        {
            StringBuilder strWhere = new StringBuilder();
            strWhere = strWhere.Append($"a.type<> { (int)newsType.news}");
            string str = $"type<> { (int)newsType.news}";
            if (!string.IsNullOrEmpty(title))
            {
                strWhere = strWhere.Append($" and a.title like '%{title}%'");
                str += $" and title like '%{title}%'";
            }
            if (type != 0)
            {
                strWhere = strWhere.Append($" and a.type={type}");
                str += $" and type={type}";
            }
            if (childnode != 0)
            {
                strWhere = strWhere.Append($" and a.childnode={childnode}");
                str += $" and childnode={childnode}";
            }
            if (state != -1)
            {
                strWhere = strWhere.Append($" and a.state={state}");
                str += $" and state={state}";
            }
            string sql = $"select a.*,b.name as nodeName from homenews a left join homebkmenu b on a.childNode=b.id where {strWhere} limit {(pageIndex - 1) * pageSize},{pageSize}";
            DataTable dt = SqlMySql.ExecuteDataSet(Utility.dbEnum.QLWL.ToString(), CommandType.Text, sql).Tables[0];
            totalcount = this.GetCount(str);
            return dt;
        }
        /// <summary>
        /// 通过缓存获取首页新闻列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<Homenews> GetListByCache(int type, int count)
        {
            string key = string.Empty;
            string sqlwhere = string.Empty;
            if (type == 0)
            {
                key = bknewslistkey;
                sqlwhere = $"type <> {(int)newsType.news}";
            }
            else
            {
                key = newslistkey;
                sqlwhere = $"type = {(int)newsType.news}";
            }
            List<Homenews> List = RedisUtil.Get<List<Homenews>>(key);
            if (List == null || List.Count == 0)
            {
                List = this.GetList(sqlwhere, count, 1, "*", "sort desc,id desc");
                if (List != null && List.Count > 0)
                {
                    RedisUtil.Set<List<Homenews>>(key, List, TimeSpan.FromHours(1));
                }
            }
            return List;
        }
    }
}
