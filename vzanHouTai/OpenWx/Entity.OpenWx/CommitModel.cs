using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.OpenWx
{
    public class CommitModel
    {
        public CommitModel()
        { }
        #region Model

        /// <summary>
        ///  第三方平台上小程序的模板号
        /// </summary>
        public int template_id
        {
            set;
            get;
        }

        /// <summary>
        /// 自定义配置
        /// </summary>
        public string ext_json
        {
            get; set;
        }

        /// <summary>
        /// 版本
        /// </summary>
        public string user_version
        {
            get; set;
        }

        /// <summary>
        /// 描述
        /// </summary>
        public string user_desc
        {
            get; set;
        }
        #endregion Model
    }
}
