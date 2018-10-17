using DAL.Base;
using Entity.MiniApp.Pin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Pin
{
   public class PinAgentLevelConfigBLL:BaseMySql<PinAgentLevelConfig>
    {
        #region 单例模式
        private static PinAgentLevelConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinAgentLevelConfigBLL()
        {

        }

        public static PinAgentLevelConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinAgentLevelConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }

        #endregion

        public List<PinAgentLevelConfig> GetListByAid(int aid)
        {
            return base.GetList($"Aid={aid} and State<>-1");
        }
        public PinAgentLevelConfig GetPinAgentLevelConfig(int id)
        {
            return base.GetModel($"Id={id} and State<>-1");
        }

        public PinAgentLevelConfig GetPinAgentLevelConfig(int levelId, int aid)
        {
            return base.GetModel($"LevelId={levelId} and State<>-1 and Aid={aid}");
        }


        public List<PinAgentLevelConfig> GetUpdatePinAgentLevelList(int levelId, int aid)
        {
            return base.GetList($"LevelId>{levelId} and State<>-1 and Aid={aid}");
        }


    }
}
