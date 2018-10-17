using DAL.Base;
using Entity.MiniApp.Pin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Pin
{
    public class PinPictureBLL : BaseMySql<PinPicture>
    {
        #region 单例模式
        private static PinPictureBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinPictureBLL()
        {

        }

        public static PinPictureBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinPictureBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 根据aid获取有效的首页广告
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public List<PinPicture> GetListByAid_Type(int aid, int type)
        {
            List<PinPicture> picList = new List<PinPicture>();
            if (aid <= 0)
            {
                return picList;
            }
            string sqlwhere = $" aid={aid} and state>=0 and type={type}";
            picList = GetList(sqlwhere);
            return picList;
        }

        public PinPicture GetModelByAid_Id(int aid, int id)
        {
            PinPicture pic = null;
            if (aid <= 0 || id <= 0)
            {
                return pic;
            }
            string sqlwhere = $" aid={aid} and id={id} and state>=0 and type=0";
            pic = GetModel(sqlwhere);
            return pic;
        }
    }
}