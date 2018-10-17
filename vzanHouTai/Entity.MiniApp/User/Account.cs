using System; 
using Entity.Base; 

using Utility;

/// <summary>
/// Member转移过来的
/// </summary>
namespace Entity.MiniApp.User
{
	[Serializable]	
	/// <summary>
	///Account:实体类(属性说明自动提取数据库字段的描述信息)
	/// </summary>
	[SqlTable(dbEnum.QLWL)]
    public   class Account
    {		 
		#region Model
 		private Guid _id =Guid.NewGuid();// Id
		[SqlField(IsPrimaryKey = true)]
   		public Guid Id
		{			
			get{return _id; }
			set{ _id=value;}
		}

        private string _unionid = string.Empty;
        [SqlField]
        public string UnionId
        {
           get { return _unionid; }
           set { _unionid = value; }
        }

        private DateTime _creationdate =DateTime.Now;// CreationDate
		[SqlField]		
		public DateTime CreationDate
		{			
			get{return _creationdate;}
			set{ _creationdate=value;}
		}
		
		private string _loginid =string.Empty;// LoginId
		[SqlField]		
		public string LoginId
		{			
			get{return _loginid;}
			set{ _loginid=value;}
		}
		
		private string _password =string.Empty;// Password
		[SqlField]		
		public string Password
		{			
			get{return _password;}
			set{ _password=value;}
		}
		
		private string _email =string.Empty;// EMail
		[SqlField]		
		public string EMail
		{			
			get{return _email;}
			set{ _email=value;}
		}
		
		private string _consigneephone =string.Empty;// ConsigneePhone
		[SqlField]		
		public string ConsigneePhone
		{			
			get{return _consigneephone;}
			set{ _consigneephone=value;}
		}
		
		private string _ip =string.Empty;// Ip
		[SqlField]		
		public string Ip
		{			
			get{return _ip;}
			set{ _ip=value;}
		}
		
		private string _organizationcode =string.Empty;// OrganizationCode
		[SqlField]		
		public string OrganizationCode
		{			
			get{return _organizationcode;}
			set{ _organizationcode=value;}
		}
		
		private int _membertype =0;// 会员类型
        /// <summary>
        /// 1.个人，2.企业
        /// </summary>
		[SqlField]		
		public int MemberType
		{			
			get{return _membertype;}
			set{ _membertype=value;}
		}
		
		
		private Guid _referraid =Guid.NewGuid();// ReferraId
		[SqlField]		
		public Guid ReferraId
		{			
			get{return _referraid;}
			set{ _referraid=value;}
		}
		
		private string _syncstatus =string.Empty;// SyncStatus
		[SqlField]		
		public string SyncStatus
		{			
			get{return _syncstatus;}
			set{ _syncstatus=value;}
		}


        private bool _isencryption = false;
        [SqlField]
        public bool IsEncryption
        {
            get { return _isencryption; }
            set { _isencryption = value; }
        } 

     
        private int memberStatus=0;
        /// <summary>
        /// 0.不是代理，1.一级代理
        /// </summary>
        [SqlField]
        public int MemberStatus
        {
            get { return memberStatus; }
            set { memberStatus = value; }
        }


        private bool _status = true;
       /// <summary>
       /// 激活状态
       /// </summary>
        [SqlField]
        public bool Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private string _openid = string.Empty;
        /// <summary>
        /// 第三方OPENID
        /// </summary>
        [SqlField]
        public string OpenId
        {
            get { return _openid; }
            set { _openid = value; }
        }

        private bool _ismark = false;
        /// <summary>
        /// 确认了提示修改帐号弹出框
        /// </summary>
        [SqlField]
        public bool IsMark
        {
            get { return _ismark; }
            set { _ismark = value; }
        }

        private bool _isupdateId = false;
        /// <summary>
        /// 修改了LoginID，只能修改一次
        /// </summary>
        [SqlField]
        public bool IsUpdateId
        {
            get { return _isupdateId; }
            set { _isupdateId = value; }
        }

        [SqlField]
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        #endregion Model 
    }
}