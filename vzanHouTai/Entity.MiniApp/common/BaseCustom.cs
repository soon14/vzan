using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.common
{
    public abstract class BaseCustom
    {
        public T GetJsonModel<T>(string jsonStr) where T : class, new()
        {

            return string.IsNullOrWhiteSpace(jsonStr) ?
                new T() :
                JsonConvert.DeserializeObject<T>(jsonStr);
        }

        //public string SetJsonField<T>(T json, string key, object value)
        //{
        //    Type configType = typeof(T);
        //    object newValue = Convert.ChangeType(value, configType.GetProperty(key).PropertyType);
        //    configType.GetProperty(key).SetValue(json, newValue);
        //    return JsonConvert.SerializeObject(json);
        //}
    }
}
