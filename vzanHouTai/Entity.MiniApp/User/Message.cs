using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    public class Message
    {
        public int id { get; set; }
        public int docId { get; set; }
        public string score { get; set; }
        public int stateId { get; set; }
        public string keyWord { get; set; }
        public string bestFragment { get; set; }
        public int categoryId { get; set; }
        public string categoryName { get; set; }
        public int viewCount { get; set; }
        public string pinYin { get; set; }
        public string pcUrl { get; set; }
        public string wapUrl { get; set; }
        public int articleType { get; set; }
        public int linkType { get; set; }
        public DateTime createDate { get; set; }
        public DateTime lastModified { get; set; }
        public string summary { get; set; }
        public string title { get; set; }
        public string mainBody { get; set; }
        public string allBody { get; set; }
        public int imagesCount { get; set; }
        public string[] imagesUrl { get; set; }
        public string[] imagesThumbUrl { get; set; }
        public int spanDays { get; set; }
        public int isVisible { get; set; }
        public string indexType { get; set; }
        public string sortKey { get; set; }
        public string answerContent { get; set; }
        public int isShow { get; set; }
        public int answerCount { get; set; }
        public int userId { get; set; }
        public string nickName { get; set; }
        public string headImgUrl { get; set; }
    }
}
