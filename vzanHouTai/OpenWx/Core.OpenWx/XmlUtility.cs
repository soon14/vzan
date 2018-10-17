using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Core.OpenWx
{
    public static class XmlUtility
    {
        /// <summary>
        /// 序列化将流转成XML字符串
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static XDocument Convert(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);//强制调整指针位置
            using (XmlReader xr = XmlReader.Create(stream))
            {
                try
                {
                    return XDocument.Load(xr);
                }
                catch (Exception ex)
                {
                    ex.Data.Add("XmlReader", xr);
                    log4net.LogHelper.WriteError(typeof(XmlUtility),ex);
                }
            }

            return null;
        }

    }
}
