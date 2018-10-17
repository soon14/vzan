using AutoMapper;
using DAL.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.MiniApp.Common
{
    public class BaseMySqlSingle<T,T2> : BaseMySql<T> where T: class, new() where T2 : class, new()
    {
        protected static T2 _singleModel;
        protected static readonly object SynObject = new object();

        protected BaseMySqlSingle()
        {

        }

        public static T2 SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new T2();
                        }
                    }
                }
                return _singleModel;
            }
        }
    }
}
