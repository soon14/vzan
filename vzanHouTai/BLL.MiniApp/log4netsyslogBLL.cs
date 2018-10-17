using DAL.Base;
using Entity.MiniApp;

namespace BLL.MiniApp
{
    public class log4netsyslogBLL : BaseMySql<log4netsyslog>
    {

        #region 单例模式
        private static log4netsyslogBLL _singleModel;
        private static readonly object SynObject = new object();

        private log4netsyslogBLL()
        {

        }

        public static log4netsyslogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new log4netsyslogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

    }
}
