using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entity.Base;
using Utility;

namespace Entity.MiniApp
{
    [Serializable]
    /// <summary>
    ///Account:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [SqlTable(dbEnum.QLWL)]
    public partial class CategoryRole
    { 
        private int _id = 0;
        [SqlField(IsPrimaryKey = true,IsAutoId =true)]
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _categoryId = 0;
        [SqlField]
        public int CategoryId
        {
            get { return _categoryId; }
            set { _categoryId = value; }
        } 
        
        private int _rightCode = 0;
        [SqlField]
        public int RightCode
        {
            get { return _rightCode; }
            set { _rightCode = value; }
        } 

        private string _rightJson = string.Empty;
        [SqlField]
        public string RightJson
        {
            get { return _rightJson; }
            set { _rightJson = value; }
        }

        public RightItem RightObj
        {
            get
            {
                return SerializeHelper.DesFromJson<RightItem>(RightJson);
            }
            set
            {
                RightJson =  SerializeHelper.SerToJson(value);
            }
        }
    }
}