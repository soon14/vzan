using DAL.Base;
using Entity.MiniSNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniSNS
{
    public class DrawBackUserBLL : BaseMySql<DrawBackUser>
    {
        /// <summary>
        /// 检测是否提现在黑名单，ture说明是黑名单，false说明不是黑名单
        /// </summary>
        /// <param name="minisnsid"></param>
        /// <param name="IsMinisns"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
        public bool CheckEnable(int minisnsid, int IsMinisns, string openid)
        {
            if (IsMinisns == 1)
            {
                return Exists("minisnsid=" + minisnsid + " and drawtype=1");
            }
            else
            {
                return Exists("openid='" + openid + "'");
            }
        }

        public bool AddBackUser(OAuthUser user, string operid, string opername, string note)
        {
            DrawBackUser buser = new DrawBackUser()
            {
                addtime = DateTime.Now,
                drawtype = 0,
                minisnsid = user.MinisnsId,
                nickname = user.Nickname,
                openid = user.Openid,
                operid = operid,
                opername = opername,
                note = note
            };
            return Convert.ToInt32(Add(buser))>0;
        }

        public bool AddBackMinisns(Minisns minisns, string operid, string opername, string note)
        {
            DrawBackUser buser = new DrawBackUser()
            {
                addtime = DateTime.Now,
                drawtype = 1,
                minisnsid = minisns.Id,
                nickname = minisns.Name,
                operid = operid,
                opername = opername,
                note = note
            };
            return Convert.ToInt32(Add(buser)) > 0;
        }
    }
}
