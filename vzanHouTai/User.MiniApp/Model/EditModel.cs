using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace User.MiniApp.Model
{
    public class EditModel<T> where T : new()
    {
        public T DataModel { get; set; } = new T();
        public int aId { get; set; } = 0;
        public int appId { get; set; } = 0;
        public int storeId { get; set; } = 0;
    }
}