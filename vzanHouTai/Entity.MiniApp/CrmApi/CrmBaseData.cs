using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.CrmApi
{
    public class CrmBaseData<T>
    {
        public string code { get; set; }
        public T data { get; set; }
    }


    public class CRMUserData
    {
        public int user_id { get; set; }
        public string avatar_url { get; set; }
        public string user_token { get; set; }
        public bool is_expired { get; set; }
        public bool confirmed_phone { get; set; }
        public bool set_password { get; set; }
        public bool fill_user_info { get; set; }
    }
    /// <summary>
    /// 线索集合
    /// </summary>
    public class CRMLeadsBoxData
    {
        public List<CRMLeadsData> leads { get; set; }
        public int total_count { get; set; }
        public int per_page { get; set; }
        public int page { get; set; }
    }
    /// <summary>
    /// 线索数据
    /// </summary>
    public class CRMLeadsData
    {
        public int id { get; set; }
        public string name { get; set; }
        public string company_name { get; set; }
        public string status { get; set; }
        public string status_mapped { get; set; }
        public bool turned_to_customer { get; set; }
        public CRMAddressData address { get; set; }
        public bool need_hidden_dispose { get; set; }
        public bool is_user_self { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime created_at { get; set; }
    }
    /// <summary>
    /// 线索数据里面的客户资料
    /// </summary>
    public class CRMAddressData
    {
        public int id { get; set; }
        public int addressable_id { get; set; }
        public string addressable_type { get; set; }
        public string country_id { get; set; }
        public string province_id { get; set; }
        public string city_id { get; set; }
        public string district_id { get; set; }
        public string tel { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string qq { get; set; }
        public string fax { get; set; }
        public string wechat { get; set; }
        public string wangwang { get; set; }
        public string zip { get; set; }
        public string url { get; set; }
        public string etail_address { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
        public string off_distance { get; set; }
        public string region_info { get; set; }
        public string snippet { get; set; }
        public int organization_id { get; set; }
        public string tel_hidden_result { get; set; }
        public string phone_hidden_result { get; set; }

    }
    /// <summary>
    /// 查询条件数据集合
    /// </summary>
    public class CRMOptionsBoxData
    {
        public List<CRMOptionsData> options { get; set; }
        public int total_count { get; set; }
        public int per_page { get; set; }
        public int page { get; set; }
    }
    /// <summary>
    /// 查询条件数据
    /// </summary>
    public class CRMOptionsData
    {
        public string label { get; set; }
        public string name { get; set; }
        public string source { get; set; }
        public string value { get; set; }
    }
    /// <summary>
    /// 跟进记录集合
    /// </summary>
    public class CRMNewIndexBoxData
    {
        public List<CRMNewIndexData> revisit_logs { get; set; }
        public int total_count { get; set; }
        public int per_page { get; set; }
        public int page { get; set; }
    }
    /// <summary>
    /// 跟进记录
    /// </summary>
    public class CRMNewIndexData
    {
        public int id { get; set; }
        public string content { get; set; }
        public string loggable_type { get; set; }
        public int loggable_id { get; set; }
        public CRMKeUserData user { get; set; }
        public DateTime real_revisit_at { get; set; }
        public DateTime created_at { get; set; }
    }
    /// <summary>
    /// 客户数据
    /// </summary>
    public class CRMKeUserData
    {
        public int id { get; set; }
        public string email { get; set; }
        public DateTime created_at { get; set; }
        public string name { get; set; }
        public int organization_id { get; set; }
        public string phone { get; set; }
        public int role_id { get; set; }
        public string workflow_state { get; set; }
        public string job { get; set; }
        public string tel { get; set; }
        public string avatar_urltel { get; set; }
        public string department_name { get; set; }
    }

    public class CrmData
    {
        public string name { get; set; }
        public string phone { get; set; }
        public string statecontent { get; set; }
        public string content { get; set; }
        public DateTime AddTime { get; set; }
    }

}
