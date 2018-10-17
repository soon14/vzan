using DAL.Base;
using Entity.MiniSNS;
using System;
using System.Configuration;

namespace BLL.MiniSNS
{
    public partial class opencomponentconfigBLL : BaseMySql<opencomponentconfig>
    {
        public bool ComponentVerifyTicket(string appid, string ticket, DateTime createtime)
        {
            opencomponentconfig model = GetModel(string.Format("component_Appid='{0}'", appid));
            if (model == null)
            {
                model = new opencomponentconfig();
            }
            model.component_Appid = appid;
            model.component_verify_ticket = ticket;
            model.ticket_time = createtime;
            if (model.id > 0)
                return Update(model);
            Add(model);
            return true;

        }
        /// <summary>
        /// 获取Component_access_token
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        
        /// <summary>
        /// 获取VerifyTicket
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns> 

        public override bool Update(opencomponentconfig model)
        {
            bool result = base.Update(model);
            RedisUtil.Remove(string.Format(MemCacheKey.Opencomponentconfig, model.component_Appid));
            return result;
        }
    }
}