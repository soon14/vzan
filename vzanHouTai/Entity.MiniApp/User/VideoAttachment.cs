using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Entity.MiniApp.User;

namespace Entity.MiniApp
{
    [Serializable]
    /// <summary>
    ///Account:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [SqlTable(dbEnum.QLWL)]
    public class VideoAttachment
    {
        public VideoAttachment()
        { }
        #region Model
        private int _id;
        private int _fid;
        private int _tid;
        private int _videowith;
        private int _videoheight;
        private string _videosize;
        private string _postfix;
        private string _videoposterpath;
        private string _convertfilepath;
        private int _userid;
        private string _sourcefilepath;
        private int _status;
        private DateTime _createdate;
        private string _videotime;
        private int _aid;
        private  int _cid;

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        /// <summary>
        /// auto_increment
        /// </summary>
        public int Id
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int Fid
        {
            set { _fid = value; }
            get { return _fid; }
        }
        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int Tid
        {
            set { _tid = value; }
            get { return _tid; }
        }


        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int Articleid
        {
            set { _aid = value; }
            get { return _aid; }
        }
        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int CommentId
        {
            set { _cid = value; }
            get { return _cid; }
        }
        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int VideoWith
        {
            set { _videowith = value; }
            get { return _videowith; }
        }
        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int VideoHeight
        {
            set { _videoheight = value; }
            get { return _videoheight; }
        }
        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public string VideoSize
        {
            set { _videosize = value; }
            get { return _videosize; }
        }
        /// <summary>
        /// 视频后缀
        /// </summary>
        [SqlField]
        public string Postfix
        {
            set { _postfix = value; }
            get { return _postfix; }
        }
        /// <summary>
        /// 视频截图封面路径
        /// </summary>
        [SqlField]
        public string VideoPosterPath
        {
            set { _videoposterpath = value; }
            get { return _videoposterpath; }
        }
        /// <summary>
        /// 视频网络地址
        /// </summary>
        [SqlField]
        public string ConvertFilePath
        {
            set { _convertfilepath = value; }
            get { return _convertfilepath; }
        }
        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int UserId
        {
            set { _userid = value; }
            get { return _userid; }
        }
        /// <summary>
        /// 视频源文件 物理路径
        /// </summary>
        [SqlField]
        public string SourceFilePath
        {
            set { _sourcefilepath = value; }
            get { return _sourcefilepath; }
        }
        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int Status
        {
            set { _status = value; }
            get { return _status; }
        }
        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public DateTime CreateDate
        {
            set { _createdate = value; }
            get { return _createdate; }
        }
        /// <summary>
        /// 视频时长
        /// </summary>
        [SqlField]
        public string VideoTime
        {
            set { _videotime = value; }
            get { return _videotime; }
        }
        #endregion Model
        public string FName { get; set; }
        public string Content { get; set; }
        public string ImgurlList { get; set; }
        public string MinisnsLogoUrl { get; set; }
        public DateTime ForumCreateDate { get; set; }
    }
}
