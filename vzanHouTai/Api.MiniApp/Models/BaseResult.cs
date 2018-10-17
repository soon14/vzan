using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Api.MiniApp.Models
{
    [Serializable]
    public class BaseResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool result { get; set; }

        /// <summary>
        /// 返回的信息 (包括返回成功信息如：修改昵称成功 ; 返回的具体错误信息如：接口参数不对，账号或者密码错误 ， 名字重复  等信息)
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 实体的JSON 
        /// </summary>
        public object obj { get; set; }

        public int errcode { get; set; }
        
    }

    public class AppResult
    {
        /// <summary>
        /// 返回结果
        /// </summary>
        public bool result { get; set; }
        /// <summary>
        /// 返回信息
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 返回实体
        /// </summary>
        public object obj { get; set; }
        /// <summary>
        /// 返回数组
        /// </summary>
        public object objArray { get; set; }
        /// <summary>
        /// 错误代码
        /// </summary>
        public int errcode { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int pageIndex { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int totalPage { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int totalCount { get; set; }
        
    }

}