using DAL.Base;
using Entity.MiniApp;
using System.Data;

namespace BLL.MiniApp
{
    public partial class MiniappUploadTaskBLL : BaseMySql<MiniappUploadTask>
    {
        #region 单例模式
        private static MiniappUploadTaskBLL _singleModel;
        private static readonly object SynObject = new object();

        private MiniappUploadTaskBLL()
        {

        }

        public static MiniappUploadTaskBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new MiniappUploadTaskBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public int UpdateListByIds(string ids)
        {
            string sql = $"update MiniappUploadTask set state=-1 where id in ({ids})";
            return SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql, null);
        }
    }
}
