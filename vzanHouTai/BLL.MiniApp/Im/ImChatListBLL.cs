using BLL.MiniApp.Pin;
using BLL.MiniApp.Qiye;
using BLL.MiniApp.User;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Im;
using Entity.MiniApp.Pin;
using Entity.MiniApp.Qiye;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.MiniApp.Im
{
    public class ImContactBLL : BaseMySql<ImContact>
    {
        #region 单例模式
        private static ImContactBLL _singleModel;
        private static readonly object SynObject = new object();

        private ImContactBLL()
        {

        }

        public static ImContactBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ImContactBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public static string redis_ListEmployeekey = "ListEmployeekey_{0}";
        public static string redis_ListProContactkey = "ListProContactkey_{0}_{1}";
        public static string redis_ListPxhkey = "ListPxhkey_{0}";


        public List<ImContact> GetListByFuserid(int fuserid, int fusertype, int pageSize, int pageIndex, int ver = 0)
        {
            string key = string.Format(redis_ListEmployeekey, fuserid, fusertype);
            List<ImContact> list = RedisUtil.Get<List<ImContact>>(key);
            string sqlwhere = $"fuserid={fuserid} and fusertype={fusertype}";
            //parameters.Add(new MySqlParameter("@appid", appid));
            if (pageIndex == 1 || list == null || list.Count <= 0)
            {
                list = GetList(sqlwhere);
                if (list == null || list.Count <= 0)
                {
                    return list;
                }

                
                List<ImMessage> messageList = new List<ImMessage>();

                string fkey = string.Empty;
                string tkey = string.Empty; ;
                if (ver == 1)
                {
                    list = GetProContratList(list, fuserid, fusertype, ver);
                }
                else
                {
                    list = GetFootBathContratList(list, fuserid, fusertype);
                }
                list = list.OrderByDescending(imchat => imchat.newDate).ToList();
            }
            list = list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return list;
        }
        /// <summary>
        /// 足浴版联系人列表获取最近一条消息
        /// </summary>
        /// <param name="imchatlist"></param>
        /// <param name="fuserId"></param>
        /// <param name="fuserType"></param>
        /// <returns></returns>
        private List<ImContact> GetFootBathContratList(List<ImContact> imchatlist, int fuserId, int fuserType)
        {
            //普通客户获取联系人以及最新一条消息记录
            
            if (fuserType == 0)
            {
                
                foreach (var info in imchatlist)
                {
                    TechnicianInfo technicianinfo = TechnicianInfoBLL.SingleModel.GetModelById(info.tuserId);
                    if (technicianinfo != null)
                    {
                        info.tuserHeadImg = technicianinfo.headImg;
                        info.tuserNicename = technicianinfo.jobNumber;
                        info.message = ImMessageBLL.SingleModel.GetNewMessage(fuserId, info.tuserId, fuserType, 0);
                        if (info.message != null && !string.IsNullOrEmpty(info.message.sendDate))
                        {
                            info.newDate = Convert.ToDateTime(info.message.sendDate);
                        }
                    }
                }
            }
            //技师获取联系人以及最新一条消息记录
            else
            {
                string userIds = string.Join(",", imchatlist.Select(s=>s.tuserId));
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

                foreach (var info in imchatlist)
                {
                    C_UserInfo userInfo = userInfoList?.FirstOrDefault(f=>f.Id==info.tuserId);
                    if (userInfo != null)
                    {
                        info.tuserHeadImg = userInfo.HeadImgUrl;
                        info.tuserNicename = userInfo.NickName;
                        info.message = ImMessageBLL.SingleModel.GetNewMessage(fuserId, info.tuserId, fuserType, 0);
                        if (info.message != null && !string.IsNullOrEmpty(info.message.sendDate))
                        {
                            info.newDate = Convert.ToDateTime(info.message.sendDate);
                        }
                    }
                }
            }
            return imchatlist;
        }
        /// <summary>
        /// 专业版联系人列表获取最近一条消息和未读消息数
        /// </summary>
        /// <param name="list"></param>
        /// <param name="fuserid"></param>
        /// <param name="fusertype"></param>
        /// <param name="ver"></param>
        /// <returns></returns>
        private List<ImContact> GetProContratList(List<ImContact> list, int fuserid, int fusertype, int ver)
        {
            string userIds = string.Join(",", list?.Select(s => s.tuserId));
            List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

            foreach (var info in list)
            {
                info.unReadCount = ImMessageBLL.SingleModel.GetUserNoReadCount(info.fuserId, info.tuserId);
                C_UserInfo userInfo = userInfoList?.FirstOrDefault(f=>f.Id == info.tuserId);
                if (userInfo != null)
                {
                    info.tuserHeadImg = userInfo.HeadImgUrl;
                    info.tuserNicename = userInfo.NickName;
                    info.message = ImMessageBLL.SingleModel.GetNewMessage(fuserid, info.tuserId, fusertype, ver);
                    if (info.message != null && !string.IsNullOrEmpty(info.message.sendDate))
                    {
                        info.newDate = Convert.ToDateTime(info.message.sendDate);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取所有联系人
        /// </summary>
        /// <param name="fuserid"></param>
        /// <param name="appid"></param>
        /// <returns></returns>
        public List<ImContact> GetAllContact(int fuserid, string appid)
        {
            List<ImContact> list = new List<ImContact>();
            string sql = $"select im.*,p.name cardname,p.ImgUrl cardimgurl,p.userid carduserid from ImContact im left join platmycard p on im.tuserid = p.userid where im.appid = '{appid}' and im.fuserid={fuserid}";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    ImContact model = base.GetModel(dr);
                    model.tuserNicename = dr["cardname"].ToString();
                    model.tuserHeadImg = dr["cardimgurl"].ToString();
                    if (dr["carduserid"] != DBNull.Value)
                    {
                        model.tuserId = Convert.ToInt32(dr["carduserid"]);
                    }

                    list.Add(model);
                }
            }
            return list;
        }

        /// <summary>
        /// 企业智推版员工获取联系人
        /// </summary>
        /// <param name="fuserid"></param>
        /// <param name="appid"></param>
        /// <returns></returns>
        public List<ImMessage> GetListByQiye(int tuserId, string appid, int pageSize, int pageIndex, ref int count, string name)
        {
            string key = string.Format(redis_ListEmployeekey, tuserId);

            List<ImMessage> list = new List<ImMessage>();
            List<ImMessage> redisList = RedisUtil.Get<List<ImMessage>>(key);
            if (redisList == null || redisList.Count <= 0 || pageIndex == 1)
            {
                redisList = new List<ImMessage>();
                
                string sql = $"select {"{0}"} from ImContact im left join c_userinfo c on im.fuserid = c.id ";
                string sqlCount = string.Format(sql, "count(*)");
                string sqlList = string.Format(sql, "im.*,c.NickName,c.HeadImgUrl");
                string sqlWhere = $" where im.appid = '{appid}' and im.tuserid={tuserId}";
                string sqlPage = "";//$" limit {(pageIndex - 1) * pageSize},{pageSize}";

                List<MySqlParameter> parms = new List<MySqlParameter>();
                if (!string.IsNullOrEmpty(name))
                {
                    sqlWhere += $" and (c.NickName like @name or c.TelePhone like @name)";
                    parms.Add(new MySqlParameter("@name", $"%{name}%"));
                }

                count = base.GetCountBySql(sqlCount + sqlWhere, parms.ToArray());
                if (count <= 0)
                    return redisList;

                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqlList + sqlWhere + sqlPage, parms.ToArray()))
                {
                    while (dr.Read())
                    {
                        ImContact tempmodel = base.GetModel(dr);
                        ImMessage model = new ImMessage();
                        model.fuserId = tempmodel.fuserId;
                        model.tuserId = tuserId;
                        model.NoReadMessageCount = ImMessageBLL.SingleModel.GetUserNoReadCount(tuserId, tempmodel.fuserId);
                        ImMessage imModel = ImMessageBLL.SingleModel.GetNewMessage(model.fuserId, tuserId, 0, 1);
                        if (imModel != null)
                        {
                            model.LastMsg = imModel.msg;
                            model.msgType = imModel.msgType;
                            model.sendDate = imModel.sendDate;
                        }
                        model.CardImgUrl = dr["HeadImgUrl"].ToString();
                        model.CardName = dr["NickName"].ToString();

                        redisList.Add(model);
                    }
                }

                string userIds = string.Join(",", redisList.Select(s => s.fuserId).Distinct());
                List<QiyeCustomer> customerList = QiyeCustomerBLL.SingleModel.GetListByUserIds(userIds);
                if (customerList == null || customerList.Count <= 0)
                    return redisList;

                string empIds = string.Join(",", customerList.Select(s => s.StaffId).Distinct());
                List<QiyeEmployee> employeeList = QiyeEmployeeBLL.SingleModel.GetListByIds(empIds);
                if (employeeList == null || employeeList.Count <= 0)
                    return redisList;

                foreach (ImMessage item in redisList)
                {
                    QiyeCustomer customerModel = customerList.FirstOrDefault(f => f.UserId == item.fuserId);
                    if (customerModel == null)
                        continue;

                    QiyeEmployee empModel = employeeList.FirstOrDefault(f => f.Id == customerModel.StaffId);
                    if (empModel == null)
                        continue;

                    item.Desc = customerModel.Desc;
                    item.EmployeeName = empModel.Name;
                }

                RedisUtil.Set<List<ImMessage>>(key, redisList);
                redisList = redisList.OrderByDescending(o => o.sendDate).ToList();
                //list = redisList.Skip((pageIndex-1)*pageSize).Take(pageSize).ToList();
            }

            list = redisList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return list;
        }

        /// <summary>
        /// 拼享惠联系人
        /// </summary>
        /// <param name="tuserId"></param>
        /// <param name="appid"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ImMessage> GetListByPxh(int tuserId, string appid, int pageSize, int pageIndex, ref int count)
        {
            string key = string.Format(redis_ListPxhkey, tuserId);
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appid);
            if (xcxrelation == null)
                return new List<ImMessage>();

            List<ImMessage> redisList = RedisUtil.Get<List<ImMessage>>(key);
            if (pageIndex == 1 || redisList == null || redisList.Count <= 0)
            {
                redisList = new List<ImMessage>();
                
                string sql = $"select {"{0}"} from ImContact im left join c_userinfo c on im.fuserid = c.id ";
                string sqlCount = string.Format(sql, "count(*)");
                string sqlList = string.Format(sql, "im.*,c.NickName,c.HeadImgUrl");
                string sqlWhere = $" where im.appid = '{appid}' and im.tuserid={tuserId}";

                count = base.GetCountBySql(sqlCount + sqlWhere);
                if (count <= 0)
                    return redisList;

                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqlList + sqlWhere))
                {
                    while (dr.Read())
                    {
                        ImContact tempmodel = base.GetModel(dr);
                        ImMessage model = new ImMessage();
                        model.fuserId = tempmodel.fuserId;
                        model.tuserId = tuserId;
                        model.NoReadMessageCount = ImMessageBLL.SingleModel.GetUserNoReadCount(tuserId, tempmodel.fuserId);
                        ImMessage imModel = ImMessageBLL.SingleModel.GetNewMessage(model.fuserId, tuserId, 0, 1);
                        if (imModel != null)
                        {
                            model.LastMsg = imModel.msg;
                            model.msgType = imModel.msgType;
                            model.sendDate = imModel.sendDate;
                        }
                        model.CardImgUrl = dr["HeadImgUrl"].ToString();
                        model.CardName = dr["NickName"].ToString();

                        redisList.Add(model);
                    }
                }

                string userIds = string.Join(",",redisList.Select(s=>s.fuserId));
                List<PinStore> storeList = PinStoreBLL.SingleModel.GetListByAidUserId(xcxrelation.Id, userIds);
                if(storeList!=null && storeList.Count>0)
                {
                    foreach (ImMessage item in redisList)
                    {
                        PinStore tempStore = storeList.FirstOrDefault(f=>f.userId==item.fuserId);
                        if(tempStore!=null)
                        {
                            item.StoreName = tempStore.storeName;
                            item.StoreLogImgUrl = tempStore.logo;
                        }
                    }
                }

                redisList = redisList.OrderByDescending(o => o.sendDate).ToList();
                RedisUtil.Set<List<ImMessage>>(key, redisList);
            }

            redisList = redisList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return redisList;
        }

        public bool CreateChat(int fuserid, int tuserid, int fusertype, string appid)
        {
            ImContact model = null;
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@appid", appid));
            parameters.Add(new MySqlParameter("@fuserid", fuserid));
            parameters.Add(new MySqlParameter("@tuserid", tuserid));
            parameters.Add(new MySqlParameter("@fusertype", fusertype));
            string sqlwhere = "fuserid=@fuserid and tuserid=@tuserid and fusertype=@fusertype";
            model = GetModel(sqlwhere, parameters.ToArray());
            if (model == null)
            {

                model = new ImContact();
                //model.aId = aid;
                model.fuserId = fuserid;
                model.fuserType = fusertype;
                model.appId = appid;
                model.tuserId = tuserid;
                model.Id = Convert.ToInt32(Add(model));
                model.extra = GetExtraInfo(fuserid, tuserid, fusertype);
                return model.Id > 0;
            }
            else
            {
                if (model.state < 0)
                {
                    model.state = 0;
                    model.extra = GetExtraInfo(fuserid, tuserid, fusertype);
                    return Update(model, "state,extra");
                }
                return true;
            }
        }

        private string GetExtraInfo(int fuserid, int tuserid, int usertype)
        {
            
            ImContactListExtra extraInfo = new ImContactListExtra();
            if (usertype == 0)
            {
                C_UserInfo fuserInfo = C_UserInfoBLL.SingleModel.GetModel(fuserid);
                if (fuserInfo != null)
                {
                    extraInfo.fHeadImg = fuserInfo.HeadImgUrl;
                    extraInfo.fNickName = fuserInfo.NickName;
                }
                TechnicianInfo technician = TechnicianInfoBLL.SingleModel.GetModel(tuserid);
                if (technician != null)
                {
                    extraInfo.tHeadImg = technician.headImg;
                    extraInfo.tNickName = technician.jobNumber;
                }
            }
            else
            {
                C_UserInfo fuserInfo = C_UserInfoBLL.SingleModel.GetModel(tuserid);
                if (fuserInfo != null)
                {
                    extraInfo.fHeadImg = fuserInfo.HeadImgUrl;
                    extraInfo.fNickName = fuserInfo.NickName;
                }
                TechnicianInfo technician = TechnicianInfoBLL.SingleModel.GetModel(fuserid);
                if (technician != null)
                {
                    extraInfo.tHeadImg = technician.headImg;
                    extraInfo.tNickName = technician.jobNumber;
                }
            }
            return JsonConvert.SerializeObject(extraInfo);
        }
    }
}
