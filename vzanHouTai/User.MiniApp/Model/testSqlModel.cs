using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace User.MiniApp.Model
{
    public class testSqlModel
    {
        public bool isok { get; set; } = false;
        public string msg { get; set; } = string.Empty;
        public string sql { get; set; } = string.Empty;
        public DataTable table { get; set; } = null;
        public string bfsql { get; set; } = string.Empty;
    }
}