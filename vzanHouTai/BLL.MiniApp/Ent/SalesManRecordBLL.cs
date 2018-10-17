using DAL.Base;
using Entity.MiniApp.Ent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Ent
{
    public class SalesManRecordBLL : BaseMySql<SalesManRecord>
    {
        
        
        



        /// <summary>
        /// 返回购物车里对应的产品佣金比例
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="goodsId"></param>
        /// <returns></returns>
        public CpsRateCar GetCps_rate(int userId, int goodsId, int appId, int salesManRecordId)
        {
            CpsRateCar cpsRateCar = new CpsRateCar();
            EntGoods entGoods = EntGoodsBLL.SingleModel.GetModel(goodsId);
            SalesManConfig salesManConfig = SalesManConfigBLL.SingleModel.GetModel($"appId={appId}");
            if (entGoods != null && entGoods.isDistribution != 0 && salesManConfig != null && !string.IsNullOrEmpty(salesManConfig.configStr))
            {

                #region 表示开启分销员购买权限,分销员购买自己分享的产品不受保护期限制,佣金算自己的
                SalesMan salesMan = SalesManBLL.SingleModel.GetModel($"UserId={userId} and state=2");
                ConfigModel configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManConfig.configStr);
                if (configModel.payMentManager.allow_seller_buy == 1 && salesMan != null)
                {
                     salesManRecordId = Convert.ToInt32(base.Add(new SalesManRecord()
                    {
                        appId = entGoods.aid,
                        salesManId = salesMan.Id,
                        configStr = salesManConfig.configStr,
                        salesmanGoodsId = goodsId,
                        state = 1,
                        addTime = DateTime.Now,
                        cps_rate = (entGoods.isDefaultCps_Rate == 0 ? configModel.payMentManager.cps_rate : entGoods.cps_rate)
                    }));
                    if (salesManRecordId<=0)
                        return cpsRateCar;


                    cpsRateCar.salesManRecordUserId = -1;
                    cpsRateCar.recordId = salesManRecordId;
                    cpsRateCar.cps_rate = entGoods.cps_rate;
                    return cpsRateCar;
                } 

                #endregion



                // SalesManRecordUser salesManRecordUser = salesManRecordUserBLL.GetModel($" userId={userId} and goodsId={goodsId} and DATE_ADD(UpdateTime,INTERVAL protected_time Day)>now()");
                SalesManRecordUser salesManRecordUser = SalesManRecordUserBLL.SingleModel.GetModel($" userId={userId}  and DATE_ADD(UpdateTime,INTERVAL protected_time Day)>now()");//MINUTE
                if (salesManRecordUser != null)//用户-分销员存在绑定关系 计算佣金才返回对应每条购物车产品佣金比例
                {
                    SalesManRecord salesManRecord = base.GetModel($"Id={salesManRecordId} and state=1");
                    if (salesManRecord != null)//表示从分享推广页面进入购买的
                    {
                        salesManRecord.configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManRecord.configStr);
                         salesMan = SalesManBLL.SingleModel.GetModel(salesManRecord.salesManId);
                        if (salesManRecord.configModel.payMentManager.allow_seller_buy == 1 && salesMan != null && salesMan.UserId == userId)
                        {
                            //表示开启分销员购买权限,分销员购买自己分享的产品不受保护期限制,佣金算自己的
                            cpsRateCar.salesManRecordUserId = -1;
                            cpsRateCar.recordId = salesManRecord.Id;
                            cpsRateCar.cps_rate = entGoods.cps_rate;


                            return cpsRateCar;
                        }
                    }

                    //表示从普通页面进入购买的
                    salesManRecord = base.GetModel($"Id={salesManRecordUser.recordId} and state=1");




                    if (salesManRecord != null && entGoods != null)
                    {
                        if (entGoods.isDefaultCps_Rate == 0)
                        {
                            salesManRecord.configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManConfig.configStr);
                            if (salesManRecord.configModel != null && salesManRecord.configModel.payMentManager != null)
                            {

                                cpsRateCar.cps_rate = salesManRecord.configModel.payMentManager.cps_rate;
                            }

                        }
                        else
                        {
                            cpsRateCar.cps_rate = entGoods.cps_rate;
                        }
                        
                        cpsRateCar.salesManRecordUserId = salesManRecordUser.Id;
                        cpsRateCar.recordId = salesManRecordUser.recordId;

                        return cpsRateCar;
                    }

                }
                else
                {
                  
                    //这里表示没有保护期或者保护期失效 再根据是不是直接通过分销分享链接买的如果是则算给该分销员
                    SalesManRecord salesManRecord = base.GetModel($"Id={salesManRecordId} and state=1");
                    if (salesManRecord != null)
                    {
                        
                        //表示关闭了保护期设置 直接通过分享链接购买
                        salesManRecordUser = SalesManRecordUserBLL.SingleModel.GetModel($" userId={userId} and  protected_time=0 and salesmanId={salesManRecord.salesManId}");
                        if (salesManRecordUser != null)
                        {

                            cpsRateCar.salesManRecordUserId = salesManRecordUser.Id;
                            cpsRateCar.recordId = salesManRecord.Id;
                            cpsRateCar.cps_rate = entGoods.cps_rate;
                        }



                    }
                   
                }

            }


            return cpsRateCar;
        }

    }




}
