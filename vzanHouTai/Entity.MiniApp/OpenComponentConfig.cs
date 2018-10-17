using Entity.Base;
using System;
using Utility;
namespace Entity.MiniApp
{

    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class OpenComponentConfig
    {

        ///<summary>
        /// auto_increment
        /// </summary>		
        private int _id;
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int id
        {
            get { return _id; }
            set { _id = value; }
        }
        ///<summary>
        /// component_Appid
        /// </summary>		
        private string _component_appid;
        [SqlField]
        public string component_Appid
        {
            get { return _component_appid; }
            set { _component_appid = value; }
        }
        ///<summary>
        /// component_AppSecret
        /// </summary>		
        private string _component_appsecret;
        [SqlField]
        public string component_AppSecret
        {
            get { return _component_appsecret; }
            set { _component_appsecret = value; }
        }
        ///<summary>
        /// component_Token
        /// </summary>		
        private string _component_token;
        [SqlField]
        public string component_Token
        {
            get { return _component_token; }
            set { _component_token = value; }
        }
        ///<summary>
        /// component_EncodingAESKey
        /// </summary>		
        private string _component_encodingaeskey;
        [SqlField]
        public string component_EncodingAESKey
        {
            get { return _component_encodingaeskey; }
            set { _component_encodingaeskey = value; }
        }
        ///<summary>
        /// component_verify_ticket
        /// </summary>		
        private string _component_verify_ticket;
        [SqlField]
        public string component_verify_ticket
        {
            get { return _component_verify_ticket; }
            set { _component_verify_ticket = value; }
        }
        ///<summary>
        /// component_access_token
        /// </summary>		
        private string _component_access_token;
        [SqlField]
        public string component_access_token
        {
            get { return _component_access_token; }
            set { _component_access_token = value; }
        }
        ///<summary>
        /// addtime
        /// </summary>		
        private DateTime _ticket_time;
        [SqlField]
        public DateTime ticket_time
        {
            get { return _ticket_time; }
            set { _ticket_time = value; }
        }

        ///<summary>
        /// addtime
        /// </summary>		
        private DateTime _token_time;
        [SqlField]
        public DateTime token_time
        {
            get { return _token_time; }
            set { _token_time = value; }
        }

    }
}