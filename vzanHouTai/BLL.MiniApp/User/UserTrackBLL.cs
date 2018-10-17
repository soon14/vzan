using BLL.MiniApp.Ent;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.User
{
    /// <summary>
    /// 用户跟踪记录统计处理
    /// </summary>
    public class UserTrackBLL:BaseMySql<UserTrack>
    {
        #region 单例模式
        private static UserTrackBLL _singleModel;
        private static readonly object SynObject = new object();

        private UserTrackBLL()
        {

        }

        public static UserTrackBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new UserTrackBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 新增扫码记录
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="qrCodeId">二维码ID</param>
        /// <returns></returns>
        public bool AddQrCodeEntry(int userId, int qrCodeId)
        {
            return AddTrackRecord(userId, qrCodeId, UserTrackType.二维码扫描);
        }

        public bool AddQrCodeOrder(int userId, int qrCodeId)
        {
            if(qrCodeId == 0)
            {
                int? lastScanId = GetLastScanId(userId);
                qrCodeId = lastScanId.HasValue ? lastScanId.Value : qrCodeId;
            }
            if(qrCodeId == 0)
            {
                return false;
            }
            return AddTrackRecord(userId, qrCodeId, UserTrackType.二维码下单);
        }
        
        /// <summary>
        /// 新增跟踪记录
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="trackId">跟踪对象ID</param>
        /// <param name="trackType">跟踪类型</param>
        /// <param name="attrbute">附带信息</param>
        /// <returns></returns>
        private bool AddTrackRecord(int userId, int trackId, UserTrackType trackType, string attrbute = null)
        {
            UserTrack newRecord = new UserTrack { UserId = userId, TrackId = trackId, Type = (int)trackType, AddTime = DateTime.Now, Attrbute = attrbute };
            int recordId = 0;
            return int.TryParse(Add(newRecord).ToString(), out recordId);
        }

        /// <summary>
        /// 获取用户上次扫码ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int? GetLastScanId(int userId)
        {
            string whereSql = BuildWhereSql(userId: userId, trackType: UserTrackType.二维码扫描);
            return GetModel(whereSql, null, true, "TrackId", "ID")?.TrackId;
        }

        /// <summary>
        /// 获取新增（未处理）跟踪记录
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public List<UserTrack> GetWaitProcess(int amount)
        {
            string whereSql = BuildWhereSql(trackState: UserTrackState.新增);
            return GetList(whereSql, amount, 1);
        }

        /// <summary>
        /// 统计用户跟踪记录
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public bool ProcessRecord(List<UserTrack> record)
        {
            if(record == null || record.Count == 0)
            {
                return true;
            }
            UpdateQrCodeScanCount(record.FindAll(item => item.Type == (int)UserTrackType.二维码扫描));
            UpdateQrCodeOrderCount(record.FindAll(item => item.Type == (int)UserTrackType.二维码下单));
            return true;
        }

        /// <summary>
        /// 更新二维码用户扫描次数
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public bool UpdateQrCodeScanCount(List<UserTrack> record)
        {
            if(record.Count == 0)
            {
                return true;
            }
            TransactionModel tran = new TransactionModel();
            List<EntStoreCode> qrCode = EntStoreCodeBLL.SingleModel.GetListByIds(string.Join(",", record.Select(item => item.TrackId)));
            qrCode.ForEach(item =>
            {
                item.ScanCount += record.Count(thisItem => thisItem.TrackId == item.Id);
                tran.Add(EntStoreCodeBLL.SingleModel.BuildUpdateSql(item, "ScanCount"));
            });
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray) && UpdateRecored(record, UserTrackState.已统计);
        }

        /// <summary>
        /// 更新二维码用户下单次数
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public bool UpdateQrCodeOrderCount(List<UserTrack> record)
        {
            if (record.Count == 0)
            {
                return true;
            }
            TransactionModel tran = new TransactionModel();
            List<EntStoreCode> qrCode = EntStoreCodeBLL.SingleModel.GetListByIds(string.Join(",", record.Select(item => item.TrackId)));
            qrCode.ForEach(item =>
            {
                item.OrderCount += record.Count(thisItem => thisItem.TrackId == item.Id);
                tran.Add(EntStoreCodeBLL.SingleModel.BuildUpdateSql(item, "OrderCount"));
            });
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray) && UpdateRecored(record, UserTrackState.已统计);
        }

        public bool UpdateRecored(List<UserTrack> record, UserTrackState state)
        {
            TransactionModel tran = new TransactionModel();
            record.ForEach(item => { item.State = (int)state; tran.Add(BuildUpdateSql(item,"State")); });
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        private string BuildWhereSql(int? userId = null, int? trackId = null, UserTrackType? trackType = null, UserTrackState? trackState = null)
        {
            List<string> whereSql = new List<string>() { $"TrackId > 0" };
            if(userId.HasValue)
            {
                whereSql.Add($"UserId = {userId.Value}");
            }
            if(trackId.HasValue)
            {
                whereSql.Add($"TrackId = {trackId.Value}");
            }
            if(trackType.HasValue)
            {
                whereSql.Add($"Type = {(int)trackType.Value}");
            }
            if (trackState.HasValue)
            {
                whereSql.Add($"State = {(int)trackState.Value}");
            }
            return string.Join(" AND ", whereSql);
        }
    }
}
