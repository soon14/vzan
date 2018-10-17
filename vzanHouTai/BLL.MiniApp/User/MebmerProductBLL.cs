using DAL.Base;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
/// <summary>
/// Member转移过来的
/// </summary>
namespace BLL.MiniApp
{
    public class MemberProductBLL : BaseMySql<MemberProduct>
    {
       
        /// <summary>
        /// 根据用户购买的情况去更新使用期限
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="orderProduct"></param>
        //public void UpdateMemberProduct(Guid accountId, OrderProduct orderProduct,MemberProduct memberProduct)
        //{
            
        //    if (memberProduct == null)
        //    {
        //        memberProduct = new MemberProduct();
        //        memberProduct.AccountId = accountId;
        //        memberProduct.ProductCode = orderProduct.ProductCode;
        //        memberProduct.InvalidTime = GetInvalidTime(DateTime.Now, orderProduct.Amount, orderProduct.Unit);

        //        Add(memberProduct);
        //    }
        //    else
        //    {
        //        //--过期续费，要重新从现在开始计算过期时间
        //        memberProduct.InvalidTime = GetInvalidTime(memberProduct.InvalidTime < DateTime.Now ? DateTime.Now : memberProduct.InvalidTime, orderProduct.Amount, orderProduct.Unit);
        //        memberProduct.LastModified = DateTime.Now;
        //        Update(memberProduct);
        //    }
        //}
        public DateTime GetInvalidTime(DateTime dtNow,int amount,string unit)
        {
            if (unit == "年")
                return dtNow.AddYears(amount);
            else if (unit == "月")
                return dtNow.AddMonths(amount);
            else if (unit == "周")
                return dtNow.AddDays(amount * 7);
            else if (unit == "日")
                return dtNow.AddDays(amount);
            else
                return dtNow;
        }

        public List<MemberProduct> GetSoftwareList(Guid AccountId)
        {
            string sql = string.Format("select mp.* from MemberProduct mp left join Product p on mp.ProductCode=p.ProductCode where p.ClassCode='01' and mp.AccountId='{0}'", AccountId);
            return base.GetListBySql(sql);
        }

        public List<MemberProduct> GetAgentList(Guid AccountId)
        {
            string sql = string.Format("select mp.* from MemberProduct mp left join Product p on mp.ProductCode=p.ProductCode where p.ClassCode='02' and mp.AccountId='{0}'", AccountId);
            return base.GetListBySql(sql);
        }
    }
}
