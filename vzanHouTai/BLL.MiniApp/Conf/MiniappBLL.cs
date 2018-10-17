using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using System;
using System.Data;

namespace BLL.MiniApp.Conf
{
    public class MiniappBLL : BaseMySql<Miniapp>
    {
        #region 单例模式
        private static MiniappBLL _singleModel;
        private static readonly object SynObject = new object();

        private MiniappBLL()
        {

        }

        public static MiniappBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new MiniappBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public Miniapp GetModelByRelationId(int relationid)
        {
            return GetModel($"xcxRelationId={relationid}");
        }

        /// <summary>
        /// 根据AppId和rid获取官网数据
        /// </summary>
        /// <param name="relationid"></param>
        /// <returns></returns>
        public Miniapp GetModelByAppId(string appid,int rid)
        {
            return GetModel($"ModelId='{appid}' and xcxRelationId<> {rid}");
        }

        /// <summary>
        /// 根据AppId修改modelid
        /// </summary>
        /// <param name="relationid"></param>
        /// <returns></returns>
        public bool UpdateModelAppid(string appid, int rid,int type)
        {
            if (!string.IsNullOrEmpty(appid))
            {
                TransactionModel tran = new MiniApp.TransactionModel();
                //先判断是否有需要修改的数据存在，如果没有不要包含该sql，不然事务会回滚
                var miniappcount = GetCount($"modelid = '{appid}'");
                if(type == (int)TmpType.小程序企业模板 && miniappcount > 0)
                {
                    tran.Add($"update miniapp set modelid='' where modelid='{appid}'");
                }
                
                //先清除所有appid
                tran.Add($"update xcxappaccountrelation set appid='' where appid = '{appid}'");
                //根据授权表返回的RId判断哪个才是真正绑定的
                if (rid > 0)
                {
                    tran.Add($"update xcxappaccountrelation set appid='{appid}' where id={rid}");
                }

                try
                {
                    if(!base.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), $"执行清除重复appid事务出错_sql："+Newtonsoft.Json.JsonConvert.SerializeObject(tran.sqlArray));
                        return false;
                    }
                    //企业版
                    if (type == (int)TmpType.小程序企业模板)
                    {

                        Miniapp listtemp = GetModelByRelationId(rid);

                        if (listtemp != null)
                        {
                            listtemp.ModelId = appid;
                            if (!Update(listtemp))
                            {
                                log4net.LogHelper.WriteInfo(this.GetType(), $"修改企业版appid重复出错_{appid}_{rid}");
                                return false;
                            }
                        }
                        else
                        {
                            listtemp = new Miniapp();
                            listtemp.CreateDate = DateTime.Now;
                            listtemp.Description = "官网小程序";
                            listtemp.xcxRelationId = rid;
                            listtemp.State = 1;
                            listtemp.ModelId = appid;
                            listtemp.Id = Convert.ToInt32(Add(listtemp));
                            if (listtemp.Id <= 0)
                            {
                                log4net.LogHelper.WriteInfo(this.GetType(), $"新增企业版数据出错_{appid}_{rid}");
                                return false;
                            }
                        }
                    }
                    
                    //清除缓存
                    XcxAppAccountRelationBLL.SingleModel.RemoveRedis(rid);
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteError(this.GetType(),ex);
                    return false;
                }
            }
            return true;
        }

        public void UpdateModelAppId(string appId, int rid, int type)
        {
            if (type == (int)TmpType.小程序企业模板)
            {

                Miniapp listtemp = GetModelByRelationId(rid);

                if (listtemp != null)
                {
                    listtemp.ModelId = appId;
                    if (!Update(listtemp))
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), $"修改企业版appid重复出错_{appId}_{rid}");
                    }
                }
                else
                {
                    listtemp = new Miniapp();
                    listtemp.CreateDate = DateTime.Now;
                    listtemp.Description = "官网小程序";
                    listtemp.xcxRelationId = rid;
                    listtemp.State = 1;
                    listtemp.ModelId = appId;
                    listtemp.Id = Convert.ToInt32(Add(listtemp));
                    if (listtemp.Id <= 0)
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), $"新增企业版数据出错_{appId}_{rid}");
                    }
                }
            }
        }

    }
}
