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
	///Member:实体类(属性说明自动提取数据库字段的描述信息)
	/// </summary>
	[SqlTable(dbEnum.QLWL)]
    public partial class Member
    {		
		#region Model
 		private Guid _id =Guid.NewGuid();// Id
        /// <summary>
        /// 
        /// </summary>
		[SqlField(IsPrimaryKey = true)]
   		public Guid Id
		{			
			get{return _id;}
			set{ _id=value;}
		}
		
		private DateTime _creationdate =DateTime.Now;// CreationDate
        /// <summary>
        /// 
        /// </summary>
		[SqlField]		
		public DateTime CreationDate
		{			
			get{return _creationdate;}
			set{ _creationdate=value;}
		}
		
		private DateTime _lastmodified =DateTime.Now;// LastModified
        /// <summary>
        /// 
        /// </summary>
		[SqlField]		
		public DateTime LastModified
		{			
			get{return _lastmodified;}
			set{ _lastmodified=value;}
		}
		
		private Guid _accountid =Guid.NewGuid();// AccountId
        /// <summary>
        /// 登录帐号
        /// </summary>
		[SqlField]		
		public Guid AccountId
		{			
			get{return _accountid;}
			set{ _accountid=value;}
		}

        private string _companyName = string.Empty;// 
        /// <summary>
        /// 公司名称
        /// </summary>
		[SqlField]
        public string CompanyName
		{
            get { return _companyName; }
            set { _companyName = value; }
		}
		
		private string _membername =string.Empty;// MemberName
        /// <summary>
        /// 姓名
        /// </summary>
		[SqlField]		
		public string MemberName
		{			
			get{return _membername;}
			set{ _membername=value;}
		}
		
		private int _sex =0;// Sex
        /// <summary>
        /// 性别
        /// </summary>
		[SqlField]		
		public int Sex
		{			
			get{return _sex;}
			set{ _sex=value;}
		}
		
		private DateTime _birthday =DateTime.Now;// Birthday
        /// <summary>
        /// 生日
        /// </summary>
		[SqlField]		
		public DateTime Birthday
		{			
			get{return _birthday;}
			set{ _birthday=value;}
		}

        private string _companyRemark = string.Empty;// 
        /// <summary>
        /// 公司简介
        /// </summary>
		[SqlField]
        public string CompanyRemark
		{
            get { return _companyRemark; }
            set { _companyRemark = value; }
		}
		
		private string _consigneephone1 =string.Empty;// ConsigneePhone1
        /// <summary>
        /// 手机
        /// </summary>
		[SqlField]		
		public string ConsigneePhone1
		{			
			get{return _consigneephone1;}
			set{ _consigneephone1=value;}
		}
		
		private string _consigneephone2 =string.Empty;// ConsigneePhone2
        /// <summary>
        /// 电话
        /// </summary>
		[SqlField]
        public string ConsigneePhone2
		{			
			get{return _consigneephone2;}
			set{ _consigneephone2=value;}
		}

        private string _email = string.Empty;// EMail
        /// <summary>
        /// 
        /// </summary>
		[SqlField]
        public string EMail
		{			
			get{return _email;}
			set{ _email=value;}
		}

        private string _companyUrl = string.Empty;// 
		[SqlField]
        public string CompanyUrl
		{
            get { return _companyUrl; }
            set { _companyUrl = value; }
		}
		

        private string _avatar = string.Empty;// Avatar
        /// <summary>
        /// 头像文件名（用户自定义的）
        /// </summary>
        [SqlField]
        public string Avatar
        {
            get { return _avatar; }
            set { _avatar = value; }
        }

        private string _avatarurl = string.Empty;
        /// <summary>
        /// 第三方用户的头像URL
        /// </summary>
        [SqlField]
        public string AvatarUrl
        {
            get { return _avatarurl; }
            set { _avatarurl = value; }
        }


        private string _syncstatus = "I";//
        [SqlField]
        public string SyncStatus
        {
            get { return _syncstatus; }
            set { _syncstatus = value; }
        }

        private bool _isencryption = false;
        [SqlField]
        public bool IsEncryption
        {
            get { return _isencryption; }
            set { _isencryption = value; }
        }
		
		#endregion Model
	}
}