using DAL.Base;
using Entity.OpenWx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.OpenWx
{
    public class OpenAuthorizerConfigSNSBLL : BaseMySql<OpenAuthorizerConfigSNS>
    {
        public OpenAuthorizerConfigSNSBLL()
        {
            this.TableName = "OpenAuthorizerConfig";
        }

        public void Unauthorized(string authorizerAppid)
        {
            OpenAuthorizerConfigSNSBLL configSNSBLL = new OpenAuthorizerConfigSNSBLL();
            List<OpenAuthorizerConfigSNS> configSns = GetList("appid='" + authorizerAppid + "'");
            if (configSns != null)
            {
                foreach (OpenAuthorizerConfigSNS config in configSns)
                {
                    if (config != null)
                    {
                        config.state = -6;
                    }
                    Update(config);
                }
            }
        }
    }
}
