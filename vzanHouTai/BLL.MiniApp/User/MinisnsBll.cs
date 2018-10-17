using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp
{
    public class MinisnsBll : BaseMySql<Minisns>
    {   
        public void RemoveCache(int minisnsId)
        {
            RedisUtil.Remove(string.Format(MemCacheKey.MinisnsDetailKey, minisnsId));
        }

        private bool Cheak(Minisns model)
        {
            if (model.RewardPercent < 0 || model.RewardPercent > 100)
            {
                return true;
            }
            return false;
        }

        public override bool Update(Minisns model, string columnFields)
        {
            if (Cheak(model))
            {
                return false;
            }
            bool b = base.Update(model, columnFields);
            RemoveCache(model.Id);
            return b;
        }
        
    }
}