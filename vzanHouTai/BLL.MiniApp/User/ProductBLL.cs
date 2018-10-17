using DAL.Base;
using Entity.MiniApp;
using System.Collections.Generic;
/// <summary>
/// Member转移过来的
/// </summary>
namespace BLL.MiniApp
{
    public class ProductBLL : BaseMySql<Product>
    {
        public List<Product> GetSoftwareProductList(string productName)
        {
            string strWhere = " ClassCode='01' ";//--软件商品
            if(!string.IsNullOrEmpty(productName))
                strWhere += string.Format(" AND ProductName LIKE '%{0}%'", productName);
            return base.GetList(strWhere);
        }

        public List<Product> GetAgentProductList()
        {
            string strWhere = " ClassCode='02' ";//--软件商品
            return base.GetList(strWhere);
        }

        public string GetProductClassCode(int ProductCode)
        {
            Product model = GetModel(ProductCode);

            return model == null ? "" : model.ClassCode;
        }
    }
}