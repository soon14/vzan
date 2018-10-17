using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenWx.Models
{
    public class JieBangModel
    {
        public string userId { get; set; }
        public string appId { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string xcxname { get; set; }
        public string returnurl { get; set; }
        public int rid { get; set; }
        public int cityinfoid { get; set; }
        public int newtype { get; set; }
        
    }
}
