using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.MiniApp.Conf
{
    public class ConfParamBLL : BaseMySql<ConfParam>
    {
        #region 单例模式
        private static ConfParamBLL _singleModel;
        private static readonly object SynObject = new object();

        private ConfParamBLL()
        {

        }

        public static ConfParamBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ConfParamBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public ConfParam GetModelByParamappid(string name,string appid)
        {
            var info = GetModel($"AppId='{appid}' and Param='{name}'");
            return info;
        }
        public List<ConfParam> GetModelByappid(string appid)
        {
            var info = GetList($"AppId in('{appid}')");
            return info;
        }
        
        public List<ConfParam> GetListByRId(int rid,string param="")
        {
            string sqlwhere = $"RId={rid}";
            if (!string.IsNullOrEmpty(param)) {
                sqlwhere += $" and param in ({param})";
            }
            return base.GetList(sqlwhere);
        }
        public List<ConfParam> GetListByRIdAndParam(string aids, string param)
        {
            if (string.IsNullOrEmpty(aids))
                return new List<ConfParam>();

            string sqlwhere = $"RId in ({aids})";
            if (!string.IsNullOrEmpty(param))
            {
                sqlwhere += $" and param in ({param})";
            }
            return base.GetList(sqlwhere);
        }

        public int UpdateList(int rid,string value,string param,string appid)
        {
            string sql = $"update ConfParam set Value='{value}',AppId='{appid}' where RId={rid} and Param='{param}'";
            return SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql, null);
        }
        public int UpdateList(string value, string param, string appid)
        {
            string sql = $"update ConfParam set Value='{value}' where AppId='{appid}' and Param='{param}'";
            return SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql, null);
        }

        public int UpdateListByAid(int aid, string value, string param)
        {
            string sql = $"update ConfParam set Value='{value}' where rid={aid} and Param='{param}'";
            return SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql, null);
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="xcxrelation"></param>
        /// <param name="datajson"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool SaveData(XcxAppAccountRelation xcxrelation,string datajson,ref string msg)
        {
            bool success = false;
            if (xcxrelation == null)
                return success;
            if (string.IsNullOrEmpty(datajson))
                return success;

            List<ConfParam> data = JsonConvert.DeserializeObject<List<ConfParam>>(datajson);
            if (data == null || data.Count <= 0)
                return success;

            string param = "'"+string.Join("','",data.Select(s=>s.Param)) + "'";
            List<ConfParam> paramslist = GetListByRId(xcxrelation.Id,param);

            foreach (ConfParam item in data)
            {
                ConfParam tempdata = paramslist?.FirstOrDefault(f => f.Param == item.Param);
                if (tempdata != null)
                {
                    if (UpdateListByAid(xcxrelation.Id,item.Value, item.Param) <= 0)
                    {
                        msg = "修改失败_" + item.Param;
                        return success;
                    }
                }
                else
                {
                    ConfParam model = new ConfParam();
                    model.AppId = xcxrelation.AppId;
                    model.Param = item.Param;
                    model.Value = item.Value;
                    model.State = 0;
                    model.UpdateTime = DateTime.Now;
                    model.AddTime = DateTime.Now;
                    model.RId = xcxrelation.Id;
                    if (Convert.ToInt32(base.Add(model)) <= 0)
                    {
                        msg = "添加失败";
                        return success;
                    }
                }
                success = true;
            }
            
            return success;
        }

        /// <summary>
        /// 获取用户自定义配置数量
        /// </summary>
        /// <param name="accountid"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public int GetCustomConfigCount(string accountid,string param)
        {
            if (string.IsNullOrEmpty(accountid))
                return 0;

            string sql = $"select Count(*) from confparam c  left join  xcxappaccountrelation x on x.id = c.rid where accountid = '{accountid}' ";
            if(!string.IsNullOrEmpty(param))
            {
                sql += $" and param in ({param})";
            }

            return base.GetCountBySql(sql);
        }

        #region 代理水印配置
        /// <summary>
        /// 代理是否开启了免费版也可以自定义水印
        /// </summary>
        /// <param name="agentinfo"></param>
        /// <returns></returns>
        public bool GetAgentFeeOpenShuiying(Agentinfo agentinfo)
        {
            if (agentinfo != null)
            {
                //判断代理是否开启免费也能自定义水印
                if (!string.IsNullOrEmpty(agentinfo.configjson))
                {
                    AgentConfig agentConfig = JsonConvert.DeserializeObject<AgentConfig>(agentinfo.configjson);
                    if (agentConfig.OpenFeeShuiying == 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 代理是否付费单独模板开启水印
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="agentConfig"></param>
        /// <returns></returns>
        public bool CheckPayOpenShuiying(int aid,ref AgentConfig agentConfig)
        {
            List<ConfParam> confparamlist = GetListByRId(aid);
            if (confparamlist != null && confparamlist.Count > 0)
            {
                ConfParam tempLogo = confparamlist?.FirstOrDefault(w => w.Param == "agentcustomlogo");
                if (tempLogo != null)
                {
                    agentConfig.LogoImgUrl = tempLogo.Value;
                    agentConfig.LogoHost = "";
                    agentConfig.LogoTitle = "";
                    agentConfig.LogoText = "";
                    agentConfig.isdefaul = 1;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 免费版全部显示小未平台水印
        /// </summary>
        /// <param name="xcxrelationModel"></param>
        /// <param name="agentinfo"></param>
        /// <param name="agentConfig"></param>
        /// <param name="bottomLogHost"></param>
        /// <param name="bottomLogImg"></param>
        /// <param name="bottomLogChildTitle"></param>
        /// <param name="bottomLogTitle"></param>
        /// <returns></returns>
        public bool CheckFeeTemplateShuiying(XcxAppAccountRelation xcxrelationModel,Agentinfo agentinfo,ref AgentConfig agentConfig,string bottomLogHost,string bottomLogImg,string bottomLogChildTitle,string bottomLogTitle)
        {
            //如果是子模板，则另外处理，所有免费版都挂上公司自己的logo
            switch (xcxrelationModel.Type)
            {
                case (int)TmpType.小未平台子模版:
                case (int)TmpType.小程序单页模板:
                case (int)TmpType.小程序企业模板:
                case (int)TmpType.小程序专业模板:
                    //判断代理是否开启免费也能自定义水印
                    if (GetAgentFeeOpenShuiying(agentinfo))
                    {
                        break;
                    }
                    //只有专业基础版才是免费的
                    if (xcxrelationModel.Type == (int)TmpType.小程序专业模板 && xcxrelationModel.VersionId != 3)
                    {
                        break;
                    }

                    agentConfig.IsOpenAdv = 1;
                    agentConfig.LogoHost = bottomLogHost;
                    agentConfig.LogoImgUrl = bottomLogImg;
                    agentConfig.LogoTitle = xcxrelationModel.Type == (int)TmpType.小未平台子模版 ? bottomLogChildTitle : bottomLogTitle;

                    return true;
            }

            return false;
        }
        #endregion
    }
}
