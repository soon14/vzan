using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace User.MiniApp.Model
{
    public class MiniappStoreViewModel
    {
    }

    public class BusinessTimes
    {
        public List<int> Weeks { get; set; } = new List<int>();
        public string StartTime { get; set; }
        public string EndTime { get; set; }

    }
}