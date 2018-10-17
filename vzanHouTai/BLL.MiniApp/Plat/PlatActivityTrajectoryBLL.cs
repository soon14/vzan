using BLL.MiniApp.Conf;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Plat;
using Entity.MiniApp.User;
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
    public class PlatActivityTrajectoryBLL : BaseMySql<PlatActivityTrajectory>
    {
        #region 单例模式
        private static PlatActivityTrajectoryBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatActivityTrajectoryBLL()
        {

        }

        public static PlatActivityTrajectoryBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatActivityTrajectoryBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public static readonly string _redis_PlatActivityTrajectoryKey = "PlatActivityTrajectory_{0}_{1}_{2}";
        public static readonly string _redis_PlatActivityTrajectoryVersion = "PlatActivityTrajectoryVersion_{0}";

        public void AddData(int aid, int myuserid, int othercardid, int actiontype, int datatype, int dataid = 0, string dataimgurl = "", string datacomment = "")
        {
            if (myuserid <= 0 || othercardid <= 0 || aid <= 0)
                return;

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null)
                return;

            
            PlatMyCard mycard = PlatMyCardBLL.SingleModel.GetModelByUserId(myuserid, aid);
            if (mycard == null)
                return;

            long otheruserid = 0;
            string othername = "";
            string otherimgurl = "";
            string datacontent = "";
            switch (datatype)
            {
                case (int)PointsDataType.名片:
                    PlatMyCard othercard = PlatMyCardBLL.SingleModel.GetModel(othercardid);
                    if (othercard == null)
                        return;
                    otheruserid = othercard.UserId;
                    othername = othercard.Name;
                    otherimgurl = othercard.ImgUrl;
                    break;
                case (int)PointsDataType.帖子:
                    PlatMsg platmsg =PlatMsgBLL.SingleModel.GetModel(othercardid);
                    if (platmsg == null)
                        return;
                    PlatMyCard msgothercard = PlatMyCardBLL.SingleModel.GetModel(platmsg.MyCardId);
                    if (msgothercard == null)
                        return;
                    datacontent = platmsg.MsgDetail;
                    otheruserid = msgothercard.UserId;
                    othername = msgothercard.Name;
                    otherimgurl = msgothercard.ImgUrl;
                    dataid = platmsg.Id;
                    dataimgurl = platmsg.ImgList != null && platmsg.ImgList.Count > 0 ? platmsg.ImgList[0] : "";
                    break;
            }


            PlatActivityTrajectory model = new PlatActivityTrajectory();
            model.MyUserId = mycard.UserId;
            model.MyName = mycard.Name;
            model.MyImgUrl = mycard.ImgUrl;
            model.OtherUserId = otheruserid;
            model.OtherImgUrl = otherimgurl;
            model.OtherName = othername;
            model.ActionType = actiontype;
            model.Datatype = datatype;
            model.DataId = dataid;
            model.DataImgUrl = dataimgurl;
            model.DataContent = datacontent;
            model.AId = aid;
            model.AppId = xcxrelation.AppId;
            model.AddTime = DateTime.Now;
            model.DataComment = datacomment;
            RemoveCache(myuserid);
            base.Add(model);
        }

        public List<PlatActivityTrajectory> GetListByMyUserId(int myuserid, int pageIndex, int pageSize, ref int count, bool reflesh = false)
        {
            RedisModel<PlatActivityTrajectory> model = new RedisModel<PlatActivityTrajectory>();
            model = RedisUtil.Get<RedisModel<PlatActivityTrajectory>>(string.Format(_redis_PlatActivityTrajectoryKey, myuserid, pageSize, pageIndex));
            int dataversion = RedisUtil.GetVersion(string.Format(_redis_PlatActivityTrajectoryVersion, myuserid));

            if (reflesh || model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                model = new RedisModel<PlatActivityTrajectory>();

                string sqlwhere = $"myuserid = {myuserid}";
                List<PlatActivityTrajectory> list = base.GetList(sqlwhere, pageSize, pageIndex, "", "addtime desc");
                if (list != null && list.Count > 0)
                {
                    //名片
                    string userid1 = string.Join(",", list.Select(s => s.MyUserId).Distinct());
                    string userid2 = string.Join(",", list.Select(s => s.OtherUserId).Distinct());
                    if (!string.IsNullOrEmpty(userid2))
                    {
                        if(!string.IsNullOrEmpty(userid1))
                        {
                            userid1 = userid1 + "," + userid2;
                        }
                        else
                        {
                            userid1 = userid2;
                        }
                    }
                    
                    List<PlatMyCard> cardlist = PlatMyCardBLL.SingleModel.GetListByUserIds(userid1);
                    
                    //评论
                    List<PlatMsgComment> commentlist = new List<PlatMsgComment>();
                    List<PlatActivityTrajectory> templist = list.Where(w => w.Datatype == (int)PointsDataType.评论).ToList();
                    if (templist != null && templist.Count > 0)
                    {
                        string commondids = string.Join(",", templist.Select(s => s.DataId).Distinct());
                        commentlist = PlatMsgCommentBLL.SingleModel.GetListByIds(commondids);
                    }
                    foreach (PlatActivityTrajectory item in list)
                    {
                        PlatMyCard tempcardmodel = cardlist?.FirstOrDefault(f => f.UserId == item.MyUserId);
                        item.MyCardId = tempcardmodel != null ? tempcardmodel.Id : 0;
                        tempcardmodel = cardlist?.Where(w => w.UserId == item.OtherUserId).FirstOrDefault();
                        item.OtherCardId = tempcardmodel != null ? tempcardmodel.Id : 0;

                        if (item.Datatype == (int)PointsDataType.帖子)
                        {
                            item.MsgId = item.DataId;
                        }
                        else if(item.Datatype == (int)PointsDataType.评论)
                        {
                            PlatMsgComment commentmodel = commentlist?.FirstOrDefault(f => f.Id == item.DataId);
                            item.MsgId = commentmodel != null ? commentmodel.MsgId : 0;
                        }
                    }
                }
                count = base.GetCount(sqlwhere);
                model.DataList = list;
                model.DataVersion = dataversion;
                model.Count = count;
                if (!reflesh)
                {
                    RedisUtil.Set<RedisModel<PlatActivityTrajectory>>(string.Format(_redis_PlatActivityTrajectoryKey, myuserid, pageSize, pageIndex), model);
                }
            }
            else
            {
                count = model.Count;
            }

            return model.DataList;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="agentid"></param>
        public void RemoveCache(int myuserid)
        {
            if (myuserid > 0)
            {
                RedisUtil.SetVersion(string.Format(_redis_PlatActivityTrajectoryVersion, myuserid));
            }
        }
    }
}
