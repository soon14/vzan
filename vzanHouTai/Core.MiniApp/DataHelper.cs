using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.MiniApp
{
    public static class DataHelper
    {
        public static List<T> ConvertDataTableToList<T>(DataTable table)
            where T : class, new()
        {
            List<Tuple<DataColumn, PropertyInfo>> map =
                new List<Tuple<DataColumn, PropertyInfo>>();

            foreach (PropertyInfo pi in typeof(T).GetProperties())
            {
                //ColumnAttribute col = (ColumnAttribute)
                //    Attribute.GetCustomAttribute(pi, typeof(ColumnAttribute));
                //if (col == null) continue;
                if (table.Columns.Contains(pi.Name))
                {
                    map.Add(new Tuple<DataColumn, PropertyInfo>(
                        table.Columns[pi.Name], pi));
                }
            }

            List<T> list = new List<T>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                if (row == null)
                {
                    list.Add(null);
                    continue;
                }
                T item = new T();
                foreach (Tuple<DataColumn, PropertyInfo> pair in map)
                {
                    object value = row[pair.Value1];
                    if (value is DBNull) value = null;
                    if (pair.Value2.PropertyType.FullName == "System.Boolean")
                    {
                        pair.Value2.SetValue(item,Convert.ToBoolean(value) , null);
                    }
                    else
                    {
                        pair.Value2.SetValue(item, value, null);

                    }
                }
                list.Add(item);
            }
            return list;
        }

        public static string GetDistanceFormat(int distance,int type)
        {
            string str = distance+"m";
            switch(type)
            {
                //km
                case 1:
                    if(distance>1000)
                    {
                        str = (distance *0.001).ToString("0.0") + "km";
                    }
                    break;
                //公里
                case 2:
                    if (distance > 1000)
                    {
                        str=(distance * 0.001).ToString("0.0") + "公里";
                    }
                    break;
            }

            return str;
        }
    }

    sealed class Tuple<T1, T2>
    {
        public Tuple() { }
        public Tuple(T1 value1, T2 value2) { Value1 = value1; Value2 = value2; }
        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }
    }
}
