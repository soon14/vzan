using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Utility;
using Utility.AliOss;

namespace BLL.MiniApp
{
    public class VoicesBll : BaseMySql<Voices>
    {
        #region 单例模式
        private static VoicesBll _singleModel;
        private static readonly object SynObject = new object();

        private VoicesBll()
        {

        }

        public static VoicesBll SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new VoicesBll();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public override bool Update(Voices model)
        {
            bool b = base.Update(model);
            RedisUtil.Remove(string.Format(MemCacheKey.Voice_Key, model.Id));
            return b;
        }
    }
}
