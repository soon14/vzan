using Entity.Base;
using System;
using Utility;
namespace Entity.MiniApp
{ 
    [Serializable]
    [SqlTable(dbEnum.VZAN)]
    public class OpenAuthorizerInfo
    {

        ///<summary>
        /// auto_increment
        /// </summary>		
        private int _id;
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id
        {
            get { return _id; }
            set { _id = value; }
        }
        ///<summary>
        /// user_name
        /// </summary>		
        private string _user_name;
        [SqlField]
        public string user_name
        {
            get { return _user_name; }
            set { _user_name = value; }
        }
        ///<summary>
        /// authorizer_appid
        /// </summary>		
        private string _authorizer_appid;
        [SqlField]
        public string authorizer_appid
        {
            get { return _authorizer_appid; }
            set { _authorizer_appid = value; }
        }
        ///<summary>
        /// authorizer_refresh_token
        /// </summary>		
        private string _authorizer_refresh_token;
        [SqlField]
        public string authorizer_refresh_token
        {
            get { return _authorizer_refresh_token; }
            set { _authorizer_refresh_token = value; }
        }
        ///<summary>
        /// authorizer_access_token
        /// </summary>		
        private string _authorizer_access_token;
        [SqlField]
        public string authorizer_access_token
        {
            get { return _authorizer_access_token; }
            set { _authorizer_access_token = value; }
        }
        ///<summary>
        /// addtime
        /// </summary>		
        private DateTime _addtime;
        [SqlField]
        public DateTime addtime
        {
            get { return _addtime; }
            set { _addtime = value; }
        }///<summary>
         /// addtime
         /// </summary>		
        private DateTime _refreshtime;
        [SqlField]
        public DateTime refreshtime
        {
            get { return _refreshtime; }
            set { _refreshtime = value; }
        }
        ///<summary>
        /// addtime
        /// </summary>		
        private int _status;
        [SqlField]
        public int status
        {
            get { return _status; }
            set { _status = value; }
        }

        /// <summary>
        /// 关联用户Id
        /// </summary>
        [SqlField]
        public string minisnsid
        {
            get;
            set;
        }

    }

   
}