using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Core.MiniApp
{
    public class AppHelper
    {
        public static void Log(object currentType)
        {
            Exception exception =HttpContext.Current.Server.GetLastError();
            if (exception is HttpException)
            {
                HttpException httpException = (HttpException)exception;
                int httpcode = httpException.GetHttpCode();
                switch (httpcode)
                {
                    case 404:
                        //不记录这种异常
                        break;
                    default:
                        LogHelper.WriteError(currentType.GetType(), httpException);
                        //HttpContext.Current.Server.ClearError();
                        break;
                }
            }
            else
            {
                LogHelper.WriteError(currentType.GetType(), exception);
                //HttpContext.Current.Server.ClearError();
            }

            
        }
    }
}
