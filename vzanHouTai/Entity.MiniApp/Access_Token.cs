using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{ 
    public class access_token_model : Access_Token
    {
        public DateTime refreshtime { get; set; }
    } 

    public class Access_Token
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    } 
}
