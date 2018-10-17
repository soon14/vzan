using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{

    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class Attachment
    {
        public Attachment() { }
        private int _id ;
        private int _fid = 0;
        private int _tid = 0;
        private int _imgwith = 0;
        private int _imgheight = 0;
        private int _imgsize = 0;
        private string _postfix = string.Empty;
        private string _filepath = string.Empty;
        private string _thumbnail = string.Empty;
        private int _imgtype = 0;
        private int _userid = 0;
        private string _nickname = string.Empty;
        private int _status = 1;
        private DateTime _createDate = DateTime.Now;
        private int _isnew = 0;


        [SqlField(IsPrimaryKey = true, IsAutoId = true)]

        public int id {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// imgtype 同城的，就是 店铺ID
        /// </summary>
        [SqlField]
        public int fid
        {
            set { _fid = value; }
            get { return _fid; }
        }

        /// <summary>
        /// 文章ID
        /// </summary>
        [SqlField]
        public  int tid
        {
            set { _tid = value; }
            get { return _tid; }
        }

        [SqlField]
        public int imgwith
        {
            set { _imgwith= value; }
            get { return _imgwith; }
        }

        [SqlField]
        public int imgheight
        {
            set {_imgheight = value; }
            get { return _imgheight; }
        }

        [SqlField]
        public int imgsize
        {
            set { _imgsize = value; }
            get { return _imgsize; }
        }

        [SqlField]
        public string postfix
        {
            set { _postfix = value; }
            get { return _postfix; }

        }

        /// <summary>
        /// 原图片
        /// </summary>
        [SqlField]
        public string filepath
        {
            set { _filepath = value; }
            get { return _filepath; }
        }


        /// <summary>
        /// 缩略图
        /// </summary>
        [SqlField]
        public string thumbnail
        {
            set { _thumbnail= value; }
            get { return _thumbnail; }
        }

        /// <summary>
        /// 图片类别
        /// </summary>
        [SqlField]
        public int imgtype
        {
            set { _imgtype = value; }
            get { return _imgtype; }
        }

        [SqlField]
        public int userid
        {
            set { _userid = value; }
            get { return _userid; }
        }

        [SqlField]
        public string nickname
        {
            set { _nickname = value; }
            get { return _nickname; }
        }

        [SqlField]
        public int status
        {
            set { _status = value; }
            get { return _status; }
        }

        [SqlField]
        public DateTime createDate
        {
            set { _createDate = value; }
            get { return _createDate; }
        }


        [SqlField]
        public int isnew
        {
            set { _isnew = value; }
            get { return _isnew; }
        }

        /// <summary>
        /// 回复ID
        /// </summary>
        [SqlField]
        public int commentid
        {
            set { _isnew = value; }
            get { return _isnew; }
        }
    }
}
