using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Plat;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Plat
{
    public class PlatStatisticalFlowConfigBLL : BaseMySql<PlatStatisticalFlowConfig>
    {
        #region 单例模式
        private static PlatStatisticalFlowConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatStatisticalFlowConfigBLL()
        {

        }

        public static PlatStatisticalFlowConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatStatisticalFlowConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<PlatStatisticalFlowConfig> GetListService()
        {
            string nowtime = DateTime.Now.ToShortDateString();
            string sqlwhere = $"state>=0 and appid is not null and appid <>'' and EndTime<'{nowtime}'";
            return base.GetList(sqlwhere, 300, 1,"", "updatetime asc");
        }

        /// <summary>
        /// 统计服务
        /// </summary>
        public void RunService()
        {
            List<PlatStatisticalFlowConfig> list = GetListService();
            if (list == null || list.Count <= 0)
                return;

            string appids = "'" + string.Join("','",list.Select(s=>s.AppId).Distinct())+"'";
            List<XcxAppAccountRelation> xcxrelationList = XcxAppAccountRelationBLL.SingleModel.GetListByAppids(appids);

            TransactionModel tran = new TransactionModel();
            
            string token = "";
            tran = new TransactionModel();
            foreach (PlatStatisticalFlowConfig item in list)
            {
                PlatStatisticalFlow model = new PlatStatisticalFlow();
                model.AddTime = DateTime.Now;
                model.AId = item.AId;
                model.AppId = item.AppId;
                model.DataType = 0;

                XcxAppAccountRelation tempXcxrelationModel = xcxrelationList?.FirstOrDefault(f=>f.AppId==item.AppId);
                if(tempXcxrelationModel==null)
                {
                    GetUpdateSql(item, ref tran,0);
                    GetUpdateFlowSql("没有找到权限数据",item, ref model, ref tran);
                    continue;
                }
                XcxApiBLL.SingleModel._openType = tempXcxrelationModel.ThirdOpenType;
                token = "";
                if (!XcxApiBLL.SingleModel.GetToken(tempXcxrelationModel, ref token))
                {
                    GetUpdateSql(item, ref tran,0);
                    GetUpdateFlowSql(token, item, ref model, ref tran);
                    continue;
                }
                //获取日概况数据
                string fwurl = XcxApiBLL.SingleModel.GetDailyVisitTrend(token);
                Return_Msg fwresult = XcxApiBLL.SingleModel.GetDataInfo<XCXDataModel<FWResultModel>>(fwurl, item.StartTime, item.EndTime);
                XCXDataModel<FWResultModel> data = (XCXDataModel<FWResultModel>)fwresult.dataObj;
                if(data!=null && data.list!=null)
                {
                    if(data.list.Count <= 0)
                    {
                        //GetUpdateFlowSql("微信接口返回空的统计数据", item,ref model,ref tran);
                        GetUpdateSql(item, ref tran, 1);
                    }
                    else
                    {
                        model.RefDate = data.list[0].ref_date;
                        model.Sessioncnt = Convert.ToInt32(data.list[0].session_cnt);
                        model.StayTimeSession = Convert.ToDouble(data.list[0].stay_time_session);
                        model.StayTimeUV = Convert.ToDouble(data.list[0].stay_time_uv);
                        model.VisitDepth = Convert.ToDouble(data.list[0].visit_depth);
                        model.VisitPV = Convert.ToInt32(data.list[0].visit_pv);
                        model.VisitUV = Convert.ToInt32(data.list[0].visit_uv);
                        model.VisitUVNew = Convert.ToInt32(data.list[0].visit_uv_new);
                        tran.Add(PlatStatisticalFlowBLL.SingleModel.BuildAddSql(model));

                        GetUpdateSql(item, ref tran,0);
                    }
                }
                else
                {
                    //GetUpdateFlowSql("拉取微信统计数据失败", item, ref model, ref tran);
                    GetUpdateSql(item, ref tran, 1);
                }
            }

            if (tran.sqlArray.Count() > 0)
            {
                base.ExecuteTransactionDataCorect(tran.sqlArray);
            }
        }

        private void GetUpdateSql(PlatStatisticalFlowConfig model,ref TransactionModel tran,int type)
        {
            if(type==0)
            {
                model.UpdateTime = DateTime.Now;
                model.StartTime = DateTime.Parse(model.StartTime.AddDays(1).ToShortDateString());
                model.EndTime = DateTime.Parse(model.EndTime.AddDays(1).ToShortDateString() + " 23:59:59");
                tran.Add(base.BuildUpdateSql(model, "UpdateTime,StartTime,EndTime"));
            }
            else
            {
                model.UpdateTime = DateTime.Now;
                tran.Add(base.BuildUpdateSql(model, "UpdateTime"));
            }
        }
        private void GetUpdateFlowSql(string desc,PlatStatisticalFlowConfig item, ref PlatStatisticalFlow model, ref TransactionModel tran)
        {
            model.RefDate = item.StartTime.ToString("yyyyMMdd");
            model.Sessioncnt = 0;
            model.StayTimeSession = 0;
            model.StayTimeUV = 0;
            model.VisitDepth = 0;
            model.VisitPV = 0;
            model.VisitUV = 0;
            model.VisitUVNew = 0;
            model.Remark = desc;
            tran.Add(PlatStatisticalFlowBLL.SingleModel.BuildAddSql(model));
        }

        /// <summary>
        /// 添加小程序统计配置
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="appid"></param>
        public void AddAppConfig(int aid,string appid,int type)
        {
            if(aid<=0 || string.IsNullOrEmpty(appid))
                return;
            if (type != (int)TmpType.小未平台 && type != (int)TmpType.小未平台子模版)
                return;

            string sqlwhere = $"aid = {aid} and appid = '{appid}'";
            PlatStatisticalFlowConfig platconfig = base.GetModel(sqlwhere);
            if(platconfig==null)
            {
                platconfig = new PlatStatisticalFlowConfig();
                platconfig.AddTime = DateTime.Now;
                platconfig.AId = aid;
                platconfig.AppId = appid;
                platconfig.EndTime = DateTime.Parse(DateTime.Now.ToShortDateString()+" 23:59:59");
                platconfig.StartTime = DateTime.Parse(DateTime.Now.ToShortDateString());
                platconfig.State = 0;
                platconfig.UpdateTime = DateTime.Now;
                base.Add(platconfig);
            }
        }
    }
}
