using Mcx.Infrastructure.DbFactory;
using Mcx.Infrastructure.DbFactory.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Ent
{
    public class TestBll
    {
        private RepositoryContext CurrentContext;

        MiniApp.Ent.EntGoodsBLL ent = new EntGoodsBLL();
        private MySqlDefaultBase<Entity.MiniApp.User.MiniAccount> DefaultDb;

        private MySqlDefaultBase<Entity.MiniApp.User.MiniAccount> DefaultDb2;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Context"></param>
        public TestBll(RepositoryContext Context)
        {
            this.CurrentContext = Context;
        }

        /// <summary>
        /// /
        /// </summary>
        public void DoWork()
        {
            //DefaultDb 
        }
    }
}
