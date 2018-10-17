using DAL.Base;
using Entity.MiniApp.Conf;

namespace BLL.MiniApp.Conf
{
    /// <summary>
    /// 代理商网站咨询问题列表
    /// </summary>
    public class AgentWebSiteQuestionBLL : BaseMySql<AgentWebSiteQuestion>
    {
        #region 单例模式
        private static AgentWebSiteQuestionBLL _singleModel;
        private static readonly object SynObject = new object();

        private AgentWebSiteQuestionBLL()
        {

        }

        public static AgentWebSiteQuestionBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AgentWebSiteQuestionBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }

}
