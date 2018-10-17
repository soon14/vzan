using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
    public class PlatConfigBLL : BaseMySql<PlatConfig>
    {

        #region 单例模式
        private static PlatConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatConfigBLL()
        {

        }

        public static PlatConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 获取对应的配置
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="totalCount"></param>
        /// <param name="configType">配置类别 0 广告图 1推荐商家 2置顶商家</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="orderWhere"></param>
        /// <returns></returns>
        public List<PlatConfig> getListByaid(int aid, out int totalCount, int configType = 0, int pageSize = 10, int pageIndex = 1, string orderWhere = "sortNumber desc,addTime desc")
        {
            string strWhere = $"aid={aid} and state<>-1 and configType={configType} ";
           
            totalCount = base.GetCount(strWhere);
            List<PlatConfig> list = base.GetListByParam(strWhere, null, pageSize, pageIndex, "*", orderWhere);
           
            list.ForEach(x=>{
                switch(configType)
                {
                    case 0:
                        //表示广告图跳转到目标为帖子
                        if (x.ADImgType == 1)
                        {
                            PlatMsg platMsg = PlatMsgBLL.SingleModel.GetModel(x.ObjId);
                            if (platMsg != null && platMsg.MsgDetail.Length > 0)
                            {
                                x.ObjName = platMsg.MsgDetail.Substring(0, platMsg.MsgDetail.Length > 20 ? 20 : platMsg.MsgDetail.Length);
                            }
                        }
                        else if (x.ADImgType == 0)
                        {
                            PlatStore platStoreMsg = PlatStoreBLL.SingleModel.GetPlatStore(x.ObjId, x.isStoreID == 0 ? 1 : 0);
                            if (platStoreMsg != null)
                            {
                                x.ObjName = platStoreMsg.Name;
                            }
                        }
                        else if (x.ADImgType == 2)
                        {
                           
                            //跳转小程序appid
                                x.ObjName = x.Name;
                          
                        }
                        else
                        {
                            x.ObjName = "不跳转";
                        }
                        break;
                    case 4:
                        break;
                    default:
                        //置顶商家跟推荐商家没有上传轮播图 使用店铺轮播图第一张
                        PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(x.ObjId, x.isStoreID == 0 ? 1 : 0);
                        if (platStore != null)
                        {
                            x.storeId = platStore.Id;
                            x.ObjName = platStore.Name;

                            string[] imgs = platStore.Banners.Split(',');
                            if (imgs != null && imgs.Length > 0)
                            {
                                x.ADImg = imgs.FirstOrDefault();
                            }
                        }
                        break;
                }
            });
            
            return list;
        }

        public List<PlatConfig> GetListByConfigType(int aid,int configType)
        {
            string sqlWhere = $"aid={aid} and ConfigType={configType}";
            return base.GetList(sqlWhere);
        }

        /// <summary>
        /// 获取指定类型配置条数
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="ConfigType"></param>
        /// <returns></returns>
        public int GetCountByType(int appId,int ConfigType)
        {
            return base.GetCount($"Aid={appId} and ConfigType={ConfigType} and State=0");
        }
        
        public PlatConfig GetPlatConfig(int aid, int ConfigType)
        {
            return base.GetModel($"Aid={aid} and ConfigType={ConfigType} and State=0");
        }

        public string GetUpdateAdImgSql(string adimg,int id)
        {
            if (adimg == null)
                adimg = "";

            string sql = $"update platconfig set ADImg='{adimg}' where id={id}";

            return sql;
        }
    }
}
