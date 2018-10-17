using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.MiniApp.Models.Pin
{
    public class ApiBaseModel
    {
        public string utoken { get; set; } = string.Empty;
        public int aId { get; set; } = 0;
    }
}