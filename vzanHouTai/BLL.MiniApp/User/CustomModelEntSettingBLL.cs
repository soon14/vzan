using BLL.MiniApp.Conf;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Utility;
/// <summary>
/// Member转移过来的
/// </summary>
namespace BLL.MiniApp
{
    public class CustomModelEntSettingBLL : BaseMySql<CustomModelEntSetting>
    {
        #region 单例模式
        private static CustomModelEntSettingBLL _singleModel;
        private static readonly object SynObject = new object();

        private CustomModelEntSettingBLL()
        {

        }

        public static CustomModelEntSettingBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CustomModelEntSettingBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}