using DAL.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;
using Utility;

namespace Core.MiniApp
{
    public static class Utils
    {
        public static string _redis_FoodGoodsOrder_HaveNewOrder_Key = "redis_FoodGoodsOrder_HaveNewOrder_Key_{0}_{1}";
        public static string _redis_HaveNewOrder_Key = "redis_HaveNewOrder_Key_{0}";


        #region 扩展方法
        public static string FormatSex(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }

            return input.Replace("1", "男").Replace("2", "女").Replace("0", "未知");
        }
        #endregion

        public static string ErrorMsg(this Controller controller)
        {
            StringBuilder sbError = new StringBuilder();
            List<string> errorList = new List<string>();
            foreach (var item in controller.ModelState.Values)
            {
                if (item.Errors.Count > 0)
                {
                    for (int i = item.Errors.Count - 1; i >= 0; i--)
                    {
                        errorList.Add(item.Errors[i].ErrorMessage);
                    }
                }
            }
            return string.Join(",", errorList);
        }

        public static string ErrorMsg(this ApiController controller)
        {
            StringBuilder sbError = new StringBuilder();
            List<string> errorList = new List<string>();
            foreach (var item in controller.ModelState.Values)
            {
                if (item.Errors.Count > 0)
                {
                    for (int i = item.Errors.Count - 1; i >= 0; i--)
                    {
                        if (!string.IsNullOrEmpty(item.Errors[i].ErrorMessage))
                        {
                            errorList.Add(item.Errors[i].ErrorMessage);
                        }

                    }
                }
            }
            if (errorList.Count > 0)
                return string.Join(",", errorList);
            else
                return string.Intern("");
        }

        public static int SafeGetDBObject(object input, int defaultValue)
        {
            if (Convert.IsDBNull(input))
            {
                return defaultValue;
            }

            return Convert.ToInt32(input);
        }

        /// <summary>
        /// 模糊查询
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FuzzyQuery(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }

            return $"%{string.Join("%", input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))}%";
        }

        /// <summary>
        /// 获取插入sql语句，特点：执行事务插入时可以把几个未得到ID的过程连接起来一起执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="insertname"></param>
        /// <param name="value"></param>
        /// <param name="tablename"></param>
        /// <param name="pis"></param>
        /// <returns></returns>
        public static string BuildAddSqlS<T>(T model, string insertname, string value, string tablename, PropertyInfo[] pis)
        {
            StringBuilder strSql = new StringBuilder();
            StringBuilder strParameter = new StringBuilder();
            strSql.Append(string.Format("insert into {0}(", tablename));

            for (int i = 1; i < pis.Length; i++)
            {
                object tempvalue = pis[i].GetValue(model);
                if (tempvalue == null || tempvalue == DBNull.Value)
                {
                    continue;
                }
                string strValue = tempvalue.ToString();
                if (TypeConvert.GetSqlDbType(pis[i].PropertyType).Equals(SqlDbType.DateTime))
                {
                    strValue = Convert.ToDateTime(pis[i].GetValue(model, null)).ToString("yyyy-MM-dd HH:mm:ss.fff");
                }
                strSql.Append(string.Format("{0},", pis[i].Name)); //构造SQL语句前半部份 
                if (insertname.Contains(pis[i].Name))
                {
                    strParameter.Append(value + ","); //构造参数SQL语句
                }
                else
                {
                    strParameter.Append(TypeConvert.GetPropValueField(pis[i].PropertyType.Name, strValue) + ","); //构造参数SQL语句
                }
            }

            strSql = strSql.Replace(",", ")", strSql.Length - 1, 1);
            strParameter = strParameter.Replace(",", ")", strParameter.Length - 1, 1);
            strSql.Append(" values (");
            strSql.Append(strParameter + ";");
            return strSql.ToString();
        }

        #region 新订单提示
        public static bool IsHaveNewOrder(string accountid)
        {
            return RedisUtil.Get<bool>(string.Format(_redis_HaveNewOrder_Key, accountid));
        }
        public static bool RemoveIsHaveNewOrder(string accountid, bool have = true)
        {
            return RedisUtil.Set(string.Format(_redis_HaveNewOrder_Key, accountid), have);
        }
        public static string IsHaveNewOrder(int aid, int storeid = 0)
        {
            return RedisUtil.Get<string>(string.Format(_redis_FoodGoodsOrder_HaveNewOrder_Key, aid, storeid));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aid">权限表ID</param>
        /// <param name="storeid">多门店备用，店铺ID</param>
        /// <param name="have">有新订单_订单类型（EntGoodsType）</param>
        /// <returns></returns>
        public static bool RemoveIsHaveNewOrder(int aid, int storeid = 0, string have = "1_0")
        {
            return RedisUtil.Set(string.Format(_redis_FoodGoodsOrder_HaveNewOrder_Key, aid, storeid), have);
        }
        #endregion


        public static string ResizeImg(string imgurl)
        {
            if (string.IsNullOrEmpty(imgurl))
            {
                return string.Empty;
            }

            if (imgurl.StartsWith("http://vzan-img.oss-cn-hangzhou.aliyuncs.com"))
            {
                return imgurl.Replace("http://vzan-img.oss-cn-hangzhou.aliyuncs.com/", "https://i.vzan.cc/");
            }

            return imgurl;
        }


        public static string BuildCookie(Guid id, DateTime updateTime)
        {
            return DESEncryptTools.DESEncrypt($@"{id.ToString()}\r\n{updateTime.ToString("yyyy-MM-dd HH:mm:ss")}");
        }
        public static Guid GetBuildCookieId(string cookieName)
        {
            string cookie = CookieHelper.GetCookie(cookieName);
            if (string.IsNullOrEmpty(cookie))
                return Guid.Empty;

            if (!string.IsNullOrEmpty(cookie))
                cookie = DESEncryptTools.DESDecrypt(cookie);
            Guid cookieId = Guid.Empty;
            if (!string.IsNullOrEmpty(cookie))
            {
                List<string> kv = cookie.SplitStr(@"\r\n");
                if (kv.Count == 2 && !string.IsNullOrEmpty(kv[0]) && !string.IsNullOrEmpty(kv[1]))
                {
                    Guid.TryParse(kv[0], out cookieId);
                }
            }
            return cookieId;
        }

        /// <summary>
        /// 获取中文域名
        /// 中文域名会进行punycode编码 例如:www.九美.com   System.Web.HttpContext.Current.Request.Url.Host;实际拿到的是 www.xn--sjq221j.com
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static string GetUnicodeDomain(this string domain)
        {
            if (domain.Contains("xn--"))
                domain = new System.Globalization.IdnMapping().GetUnicode(domain);

            return domain;
        }
    }
}
