using DAL.Base;
using Entity.MiniApp.MiappTribune;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.MiniApp
{
    public class MiniappTribuneChargeTypeInfoBLL : BaseMySql<MiniappTribuneChargeTypeInfo>
    {
        public int GetPaymentFee(int chType, int rid, int extype, int exlength, int coupon = 100)
        {
            if (coupon < 0 || coupon > 100)
            {
                throw new Exception("优惠券异常，优惠券打折比率：" + coupon);
            }
            MiniappTribuneChargeTypeInfo info = GetModel($"Rid={rid} and PayType={chType} and ExtendType={extype} ");
            if (info != null)
            {
                return Convert.ToInt32(info.Price * exlength * coupon / 100);
            }
            else//如果商家没有设置，那获取默认的
            {
                info = GetModel($"AreaCode=0 and PayType={chType} and ExtendType={extype}");
                if (info == null)
                {
                    throw new Exception("系统没有设置过默认付费额度，请添加！" + $"AreaCode={rid} and PayType={chType} and ExtendType={extype} " + $"AreaCode=0 and PayType={chType} and ExtendType={extype}");
                }
                return Convert.ToInt32(info.Price * exlength * coupon / 100);
            }
        }
        public MiniappTribuneChargeTypeInfo GetChargeType(int chType, int rid, int extype)
        {
            return GetModel($"rid={rid} and PayType={chType} and ExtendType={extype} ") ?? GetModel($"AreaCode=0 and PayType={chType} and ExtendType={extype}");
        }
        public List<MiniappTribuneChargeTypeInfo> GetTypeInfoListByAreaCode(int rid, int extype, bool isGetDefault = true)
        {
            List<MiniappTribuneChargeTypeInfo> list = GetList("rid=" + rid + " and paytype=" + extype + " and state>0 ", 10, 1, "*", "ExtendType asc");
            if (!isGetDefault)
            {
                return list ?? new List<MiniappTribuneChargeTypeInfo>();
            }
            return list?.Count > 0 ? list : GetTypeInfoListDefault(extype);
        }
        public List<MiniappTribuneChargeTypeInfo> GetTypeInfoListByAreaCode_new(int areaCode, int extype, bool isGetDefault = true)
        {
            List<MiniappTribuneChargeTypeInfo> list = GetList("AreaCode=" + areaCode + " and paytype=" + extype, 10, 1, "*", "ExtendType asc");
            if (!isGetDefault)
            {
                return list ?? new List<MiniappTribuneChargeTypeInfo>();
            }
            return list?.Count > 0 ? list : GetTypeInfoListDefault(extype);
        }
        public List<MiniappTribuneChargeTypeInfo> GetTypeInfoListDefault(int paytype)
        {
            return GetList("rid=0 and paytype=" + paytype, 10, 1, "*", "ExtendType asc");
        }

        /// <summary>
        /// 查询总版块开关，默认开1；
        /// </summary>
        /// <param name="areacode"></param>
        /// <param name="paytype"></param>
        /// <returns>1：开；0：关</returns>
        public int GetArticleTopAll(int rid, int paytype)
        {
            int iSatae = 1;
            string sql = $"rid={rid} and paytype={paytype} and extendtype=1";

            var listInfo = GetList(sql);
            if (listInfo == null || (listInfo.Count == 0))
            {
                sql = $"rid=0 and paytype={paytype} and extendtype=1";
                listInfo = GetList(sql);
            }

            foreach (MiniappTribuneChargeTypeInfo entity in listInfo)
            {
                iSatae = entity.State;
                break;
            }

            return iSatae;
        }

        /// <summary>
        /// 查询总版块开关，默认开1；
        /// </summary>
        /// <param name="areacode"></param>
        /// <param name="paytype"></param>
        /// <returns>1：开；0：关</returns>
        public int GetChargeTypeInfoAll(int rid, int paytype)
        {
            int iSatae = 1;
            string sql = $"rid={rid} and paytype={paytype} and state>0";
            var iCount = GetCount(sql);
            if (iCount == 0)
            {
                sql = $"rid=0 and paytype={paytype} and state>0";
                iCount = GetCount(sql);
            }
            if (iCount == 0)
            {
                iSatae = 0;
            }
            return iSatae;
        }

        /// <summary>
        /// 查询总版块开关，默认开1；
        /// </summary>
        /// <param name="areacode"></param>
        /// <param name="paytype"></param>
        /// <returns>1：开；0：关</returns>
        public int GetStickTopAll(int rid, int paytype)
        {
            int iSatae = 1;
            string sql = $"rid={rid} and paytype={paytype} and state>0";
            var iCount = GetCount(sql);
            if (iCount == 0)
            {
                iSatae = 0;
            }
            return iSatae;
        }

        /// <summary>
        /// 同城是否开启了配置，商家入驻高级版
        /// </summary>
        /// <param name="areacode"></param>
        /// <param name="paytype"></param>
        /// <returns></returns>
        public bool CheckCityIsOpenConfig(int rid, int paytype)
        {
            var chargeconfigList = GetList($"rid={rid} and state>-1 and  PayType ={paytype}");
            if (chargeconfigList?.Count > 0 && chargeconfigList.Count(p => p.State == 1) > 0)
            {
                return true;
            }
            return false;
        }

    }
}
