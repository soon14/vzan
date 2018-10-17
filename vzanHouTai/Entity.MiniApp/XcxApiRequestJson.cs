using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.MiniApp
{
    /// <summary>
    /// 默认板块
    /// </summary>
    public class XcxApiRequestJson<T>
    {
        /// <summary>
        /// 
        ///  
        /// </summary>
        public int isok { get; set; }
        /// <summary>
        /// 选中id
        /// </summary>
        public string msg { get; set; }

        public string src { get; set; }

        public ObjJson obj { get; set; }

        public ServerHost host { get; set; }

        public T data { get; set; }
    }

    public class ObjJson
    {
        public int errcode { get; set; }
        public string errmsg { get; set; }
        /// <summary>
        /// 接口调用凭证
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// access_token接口调用凭证超时时间，单位（秒）
        /// </summary>
        public int expires_in { get; set; }
        /// <summary>
        /// 用户刷新access_token
        /// </summary>
        public string refresh_token { get; set; }
        /// <summary>
        /// 授权用户唯一标识
        /// </summary>
        public string openid { get; set; }
        /// <summary>
        /// 会话密钥
        /// </summary>
        public string session_key { get; set; }
        /// <summary>
        /// 用户授权的作用域，使用逗号（,）分隔
        /// </summary>
        public string scope { get; set; }
        /// <summary>
        /// 授权方令牌
        /// </summary>
        public string authorizer_access_token { get; set; }
        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string authorizer_refresh_token { get; set; }
        /// <summary>
        /// 审核Id
        /// </summary>
        public int auditid { get; set; }
        /// <summary>
        /// 审核状态，其中0为审核成功，1为审核失败，2为审核中
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 当status=1，审核被拒绝时，返回的拒绝原因
        /// </summary>
        public string reason { get; set; }

        public List<membersdata> members { get; set; }
    }

    public class membersdata
    {
        public string userstr { get; set; }
    }

    public class ServerHost 
    {
        /// <summary>
        /// request合法域名，当action参数是get时不需要此字段。
        /// </summary>
        public List<string> requestdomain { get; set; }
        /// <summary>
        ///  socket合法域名，当action参数是get时不需要此字段。
        /// </summary>
        public List<string> wsrequestdomain { get; set; }
        /// <summary>
        /// uploadFile合法域名，当action参数是get时不需要此字段。
        /// </summary>
        public List<string> uploaddomain { get; set; }
        /// <summary>
        /// downloadFile合法域名，当action参数是get时不需要此字段。
        /// </summary>
        public List<string> downloaddomain { get; set; }

    }

    /// <summary>
    /// 可选类目
    /// </summary>
    public class CategoryList
    {
        /// <summary>
        /// 一级类目名称
        /// </summary>
        public string first_class { get; set; }
        /// <summary>
        /// 二级类目名称
        /// </summary>
        public string second_class { get; set; }
        /// <summary>
        /// 三级类目名称
        /// </summary>
        public string third_class { get; set; }

        /// <summary>
        /// 一级类目的ID编号
        /// </summary>
        public int first_id { get; set; }
        /// <summary>
        ///  二级类目的ID编号
        /// </summary>
        public int second_id { get; set; }
        /// <summary>
        ///  三级类目的ID编号
        /// </summary>
        public int third_id { get; set; }
    }
}