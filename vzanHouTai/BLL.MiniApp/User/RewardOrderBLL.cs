using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Utility;

namespace BLL.MiniApp
{
    public class RewardOrderBLL : BaseMySql<RewardOrder>
    {
        public List<OAuthUser> GetRewardOAuthUserTop(int count, int articleId, int minisnsId)
        {
            List<OAuthUser> listuser = RedisUtil.Get<List<OAuthUser>>(string.Format(MemCacheKey.RewardUserKey, articleId));
            if (listuser == null)
            {
                listuser = GetRewoardListTop(30, articleId, minisnsId);
                RedisUtil.Set<List<OAuthUser>>(string.Format(MemCacheKey.RewardUserKey, articleId), listuser, TimeSpan.FromHours(24));
            }
            if (listuser != null && listuser.Count > count)
                listuser = listuser.Take(count).ToList();
            return listuser;
        }
        public List<OAuthUser> GetRewardOAuthUserTopByFriend(int count, int articleId, int minisnsId)
        {
            List<OAuthUser> listuser = RedisUtil.Get<List<OAuthUser>>(string.Format(MemCacheKey.RewardUserKey, articleId));
            if (listuser == null)
            {
                listuser = GetRewoardListTopByFriend(30, articleId, minisnsId);
                RedisUtil.Set<List<OAuthUser>>(string.Format(MemCacheKey.RewardUserKey, articleId), listuser, TimeSpan.FromHours(24));
            }
            if (listuser != null && listuser.Count > count)
                listuser = listuser.Take(count).ToList();
            return listuser;
        }
        public List<Base_OAuthUser> GetRewardBaseOAuthUserTop(int count, int articleId, int minisnsId)
        {
            List<Base_OAuthUser> listuser = RedisUtil.Get<List<Base_OAuthUser>>(string.Format(MemCacheKey.RewardUserKey, articleId));
            if (listuser == null)
            {
                listuser = Base_GetRewoardListTop(count, articleId, minisnsId);
                RedisUtil.Set<List<Base_OAuthUser>>(string.Format(MemCacheKey.RewardUserKey, articleId), listuser, TimeSpan.FromHours(24));
            }
            //if (listuser != null && count != 0)
            //{
            //    listuser = listuser.Take(count).ToList();
            //}
            if (listuser != null && listuser.Count > count)
                listuser = listuser.Take(count).ToList();
            return listuser;
        }
          
        public List<OAuthUser> GetRewoardListTop(int count, int articleId, int minisnsId)
        {
            OAuthUserBll ubll = new OAuthUserBll(minisnsId);
            return ubll.GetListBySql(string.Format("select u.* from rewardorder r LEFT JOIN {2} u on (r.rewardFromUserId=u.Id) where r.`status`=1 and r.articleId={0} and (r.commentId=0 or r.commentId is null) order by r.addTime desc limit {1}", articleId, count, ubll.TableName));
        }

        public List<OAuthUser> GetRewoardListTopByFriend(int count, int articleId, int minisnsId)
        {
            OAuthUserBll ubll = new OAuthUserBll(minisnsId);
            return ubll.GetListBySql(string.Format("select u.* from rewardorder r LEFT JOIN {2} u on (r.rewardFromUserId=u.Id) where r.`status`=1 and r.articleId={0} and (r.commentId=0 or r.commentId is null) group by r.rewardFromUserId order by r.addTime desc limit {1}", articleId, count, ubll.TableName));
        }

        public List<Base_OAuthUser> Base_GetRewoardListTop(int count, int articleId, int minisnsId)
        {
            Base_OAuthUserBLL baseBll = new Base_OAuthUserBLL(minisnsId);
            return new Base_OAuthUserBLL(minisnsId).GetListBySql(string.Format("select u.Id,u.AggregateScore,u.Nickname,u.Headimgurl from rewardorder r LEFT JOIN {2} u on (r.rewardFromUserId=u.Id) where r.`status`=1 and r.articleId={0} and (r.commentId=0 or r.commentId is null) order by r.addTime desc limit {1}", articleId, count, baseBll.GetTableName(minisnsId.ToString())));
        }
        public List<OAuthUser> GetCommentRewoardListTop(int count, int commentId, int minisnsId)
        {
            OAuthUserBll ubll = new OAuthUserBll(minisnsId);
            return ubll.GetListBySql(string.Format("select u.* from rewardorder r LEFT JOIN {2} u on (r.rewardFromUserId=u.Id) where r.`status`=1 and r.commentId ={0} order by r.addTime desc limit {1}", commentId, count, ubll.TableName));
        }

        public List<Base_OAuthUser> Base_GetCommentRewoardListTop(int count, int commentId, int minisnsId)
        {
            Base_OAuthUserBLL baseBll = new Base_OAuthUserBLL(minisnsId);
            return baseBll.GetListBySql(string.Format("select u.Id,u.AggregateScore,u.Nickname,u.Headimgurl from rewardorder r LEFT JOIN {2} u on (r.rewardFromUserId=u.Id) where r.`status`=1 and r.commentId ={0} order by r.addTime desc limit {1}", commentId, count, baseBll.TableName));
        }

        public bool RewardCancel(int id)
        {
            try
            {
                string sql = "update RewardOrder set status=2 where id=" + id + " and status=0";
                return ExecuteTransaction(sql);
            }
            catch (Exception)
            {
                log4net.LogHelper.WriteError(this.GetType(), new Exception("订单取消支付时，RewardCancel修改订单状态出错！RewardOrderID=" + id));
                throw;
            }
        }
         
        private static string getAttachValue(string attach, string value)
        {
            string regex = "(?:^|\\?|&)" + value.ToLower() + "=(?<value>[\\s\\S]+?)(?:&|$)";
            return  CRegex.GetText(attach.ToLower(), regex, "value");
        } 

        public List<RewardOrder> getRewardListFromArticle(int articleId)
        {
            return GetList(string.Format("articleId={0} and status=1", articleId.ToString()));
        }

        /// <summary>
        /// 获取论坛总打赏金额
        /// </summary>
        /// <param name="minisnsId"></param>
        /// <returns></returns>
        public int GetRewardSumByMinisns(int minisnsId)
        {
            int sum = 0;
            DataSet set = SqlMySql.ExecuteDataSet(SqlMySql.GetSqlConnection(dbEnum.QLWL.ToString()), System.Data.CommandType.Text, "select SUM(rewardMoney) from rewardorder WHERE `status`=1 and minisnsId=" + minisnsId.ToString(), null);
            if (set != null && set.Tables.Count > 0)
            {
                DataTable tb = set.Tables[0];
                if (tb != null && tb.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(tb.Rows[0][0].ToString()))
                    {
                        sum = Convert.ToInt32(tb.Rows[0][0]);
                    }
                }
            }
            return sum;
        }

        /// <summary>
        /// 获取帖子总打赏金额
        /// </summary>
        /// <param name="minisnsId"></param>
        /// <returns></returns>
        public int GetRewardSumByArticle(int articleid)
        {
            int sum = 0;
            DataSet set = SqlMySql.ExecuteDataSet(SqlMySql.GetSqlConnection(dbEnum.QLWL.ToString()), System.Data.CommandType.Text, "select SUM(rewardMoney) from rewardorder WHERE `status`=1 and articleid=" + articleid.ToString(), null);
            if (set != null && set.Tables.Count > 0)
            {
                DataTable tb = set.Tables[0];
                if (tb != null && tb.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(tb.Rows[0][0].ToString()))
                    {
                        sum = Convert.ToInt32(tb.Rows[0][0]);
                    }
                }
            }
            return sum;
        }

        /// <summary>
        /// 获取帖子的打赏人数，去重复，没人打赏不会去查询。
        /// </summary>
        /// <param name="ArticleId"></param>
        /// <returns></returns>
        public int GetDistinctRewardUserCount(int ArticleId)
        {
            if (ArticleId <= 0)
            {
                return 0;
            }
            string key = string.Format(MemCacheKey.ArticleRewardUserCount, ArticleId);
            int users = RedisUtil.Get<int>(key);
            if (users > 0)
            {
                return users;
            }
            string sql = string.Format("select COUNT(DISTINCT rewardFromUserId)  FROM rewardorder WHERE `status`=1 and articleId={0} and commentId=0", ArticleId);
            users = base.GetCountBySql(sql);
            RedisUtil.Set<int>(key, users, TimeSpan.FromHours(6));
            return users;
        }

        /// <summary>
        /// 查询被打赏的订单列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public List<RewardOrder> GetToRewardOrderListbyDate(int userId, DateTime date, int pageIndex, int pageSize)
        {
            string strWhere = string.Format("`status`=1 and rewardToUserId={0} and addTime>='{1} 00:00:00' and addTime<='{1} 23:59:59'", userId, date.ToString("yyyy-MM-dd"));
            return GetList(strWhere, pageSize, pageIndex, "*", "id desc");
        }

        /// <summary>
        /// 查询打赏列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<RewardOrder> GetToRewardOrderList(int userId,int minisnsid, int pageIndex, int pageSize)
        {
            string strWhere = string.Format("`status`=1 and rewardToUserId={0} and minisnsId ={1}", userId, minisnsid);
            return GetList(strWhere, pageSize, pageIndex, "*", "id desc");
        }

    }
}
