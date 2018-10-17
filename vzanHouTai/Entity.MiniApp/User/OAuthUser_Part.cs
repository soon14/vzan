using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.User
{
    /// <summary>
    /// --用户信息表实体类
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class OAuthUser_Part : OAuthUser
    {
        public OAuthUser_Part()
        { }
        public OAuthUser_Part(OAuthUser oauser)
        {
            Id = oauser.Id;
            Openid = oauser.Openid;
            AggregateScore = oauser.AggregateScore;
            ArticleCount = oauser.ArticleCount;
            Cash = oauser.Cash;
            CCash = oauser.CCash;
            CHistoryCash = oauser.CHistoryCash;
            City = oauser.City;
            CommentCount = oauser.CommentCount;
            Country = oauser.Country;
            CreateDate = oauser.CreateDate;
            Email = oauser.Email;
            GuerdonOutCash = oauser.GuerdonOutCash;
            Headimgurl = oauser.Headimgurl;
            HistoryCash = oauser.HistoryCash;
            IntegrateScore = oauser.IntegrateScore;
            IsSign = oauser.IsSign;
            IsWholeAdmin = oauser.IsWholeAdmin;
            Latitude = oauser.Latitude;
            Longitude = oauser.Longitude;
            MinisnsId = oauser.MinisnsId;
            msgopenid = oauser.msgopenid;
            msgtips = oauser.msgtips;
            Nickname = oauser.Nickname;
            OutCash = oauser.OutCash;
            Password = oauser.Password;
            PayArticleCash = oauser.PayArticleCash;
            Phone = oauser.Phone;
            PraiseCount = oauser.PraiseCount;
            Privilege = oauser.Privilege;
            Province = oauser.Province;
            RedBagVerifyNum = oauser.RedBagVerifyNum;
            Sex = oauser.Sex;
            State = oauser.State;
            Sysnc = oauser.Sysnc;
            unionid = oauser.unionid;
            UserLevel = oauser.UserLevel;
        }
        /// <summary>
        /// --用户的唯一标识
        /// </summary>
        [SqlField(IsPrimaryKey = true)]
        public new int Id { set; get; }

    }
}
