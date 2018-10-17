using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.Base;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
    /// 同城消息列表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class C_Message
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        ///// <summary>
        ///// 消息接收者类型
        ///// </summary>
        //[SqlField]
        //public  MessageTarget Target { get; set; }

        /// <summary>
        /// 消息模板 Id
        /// </summary>
        [SqlField]
        public string TemplateId { get; set; }

        /// <summary>
        /// 同城 Id
        /// </summary>
        [SqlField]
        public int CityInfoId { get; set; } = 0;

        /// <summary>
        /// 店铺 Id
        /// </summary>
        [SqlField]
        public int StoreId { get; set; } = 0;

        /// <summary>
        /// 消息实体
        /// </summary>
        [SqlField]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 入列时间
        /// </summary>
        [SqlField]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 处理时间
        /// </summary>
        [SqlField]
        public DateTime SendTime { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        //[SqlField]
        //public  MessageState State { get; set; } =  MessageState.待发送;
    }


    public class C_TemplateModule
    {
        public string touser { get; set; }
        public string template_id { get; set; }
        public string url { get; set; }
        public C_Data data { get; set; }
    }

    public class C_Data
    {
        public C_DataObj first { get; set; }
        public C_DataObj keyword1 { get; set; }
        public C_DataObj keyword2 { get; set; }
        public C_DataObj keyword3 { get; set; }
        public C_DataObj remark { get; set; }
    }


    public class C_TemplateModule2
    {
        public string touser { get; set; }
        public string template_id { get; set; }
        public string url { get; set; }
        public C_Data2 data { get; set; }
    }

    public class C_TemplateModule_OutOrder
    {
        public string touser { get; set; }
        public string template_id { get; set; }
        public string url { get; set; }
        public C_Data_OutOrder data { get; set; }
    }


    public class C_Data2
    {
        public C_DataObj first { get; set; }
        public C_DataObj keyword1 { get; set; }
        public C_DataObj keyword2 { get; set; }
        public C_DataObj keyword3 { get; set; }
        public C_DataObj remark { get; set; }
    }

    public class C_Data_OutOrder
    {
        public C_DataObj first { get; set; }
        public C_DataObj keyword1 { get; set; }
        public C_DataObj keyword2 { get; set; }
        public C_DataObj keyword3 { get; set; }
        public C_DataObj keyword4 { get; set; }
        public C_DataObj keyword5 { get; set; }
        public C_DataObj remark { get; set; }
    }

    public class C_DataObj
    {
        public string value { get; set; }
        public string color { get; set; }
    }
}
