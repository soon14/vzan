using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using System.Collections.Generic;

namespace BLL.MiniApp
{
    public class WholeBlackBLL : BaseMySql<WholeBlack>
    {

        public override object Add(WholeBlack model)
        {
            try
            {
                object obj = base.Add(model);
                RedisUtil.SetEntryInHash<int>(MemCacheKey.WholeBlackHashKey, model.Openid, model.id);
                return obj;
            }
            catch
            {
                return 0;
            } 
        }
        public object Add(int uid, int type, string note)
        { 
            OAuthUser_Master user = OAuthUser_MasterBll.SingleModel.GetModel(uid);
            if (!CheckWholeBlack(user.OpenId, true))
            {
                WholeBlack mode = new WholeBlack() { BlackType = type, Note = note, Openid = user.OpenId };
                object obj= Add(mode);
                RedisUtil.Remove(MemCacheKey.WholeBlackHashKey);
                return obj;
            }
            else
            {
                return null;
            }
        }
        public override bool Update(WholeBlack model)
        {
            return base.Update(model);
        }
        public override int Delete(int id)
        {
            return base.Delete(id);
        }
        public bool CheckWholeBlack(string openid, bool isfromcache)
        {
            if (string.IsNullOrEmpty(openid))
                return false;

            if (string.IsNullOrWhiteSpace(openid))
                return false;
            List<WholeBlack> list = null;
            if (!RedisUtil.ContainsKey(MemCacheKey.WholeBlackHashKey))
            {
                list = GetList(null, 10000, 1);
                foreach (WholeBlack mWholeBlak in list)
                {
                    RedisUtil.SetEntryInHash<int>(MemCacheKey.WholeBlackHashKey, mWholeBlak.Openid, mWholeBlak.id);
                }
            }            
            return RedisUtil.HashContainsEntry<int>(MemCacheKey.WholeBlackHashKey, openid);
        }
    }
}
