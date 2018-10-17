using DAL.Base;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Conf
{
    public class VideoPlayThemeBLL : BaseMySql<VideoPlayTheme>
    {
        #region 单例模式
        private static VideoPlayThemeBLL _singleModel;
        private static readonly object SynObject = new object();

        private VideoPlayThemeBLL()
        {

        }

        public static VideoPlayThemeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new VideoPlayThemeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<VideoPlayTheme> GetDataList()
        {
            return base.GetList("state>=0");
        }
    }
}
