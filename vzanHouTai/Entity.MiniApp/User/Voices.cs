using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp
{
    [Serializable]
    [SqlTable(ConnName = dbEnum.QLWL, UseMaster = true)]
    public class Voices
    {
        public Voices() { }
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 微信端的ServerId
        /// </summary>
        [SqlField]
        public string ServerId { get; set; } = string.Empty;

        /// <summary>
        /// 翻译的文字
        /// </summary>
        [SqlField]
        public string MessageText { get; set; } = string.Empty;


        /// <summary>
        /// 录音时长
        /// </summary>
        [SqlField]
        public int VoiceTime { get; set; } = 0;

        /// <summary>
        /// 下载到自己服务器的地址（源文件）
        /// </summary>
        [SqlField]
        public string DownLoadFile { get; set; } = string.Empty;

        /// <summary>
        /// 转换的地址（MP3） 网络路径
        /// </summary>
        [SqlField]
        public string TransFilePath { get; set; } = string.Empty;


        /// <summary>
        /// 语音状态(以后审核)
        /// </summary>
        [SqlField]
        public int VoiceState { get; set; } = 1;

        /// <summary>
        /// 转换状态
        /// </summary>
        [SqlField]
        public int ConvertState { get; set; } = 0;


        /// <summary>
        /// 文章ID
        /// </summary>
        [SqlField]
        public int ArticleId { get; set; } = 0;

        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserId { get; set; } = 0;

        /// <summary>
        /// 微社区ID
        /// </summary>
        [SqlField]
        public int FId { get; set; } = 0;

        /// <summary>
        /// 时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 分享图片地址
        /// </summary>
        [SqlField]
        public string SharePic { get; set; }

        //MP3播放需要信息
        private string _singer;
        private string _album;
        private string _songName;
        private string _songPic;
        /// <summary>
        /// 歌手
        /// </summary>
        [SqlField]
        public string Singer
        {
            get
            {
                return _singer;
            }

            set
            {
                _singer = value;
            }
        }
        /// <summary>
        /// 专辑
        /// </summary>
        [SqlField]
        public string Album
        {
            get
            {
                return _album;
            }

            set
            {
                _album = value;
            }
        }
        /// <summary>
        /// 歌名
        /// </summary>
        [SqlField]
        public string SongName
        {
            get
            {
                return _songName;
            }

            set
            {
                _songName = value;
            }
        }
        /// <summary>
        /// 歌曲图片
        /// </summary>
        [SqlField]
        public string SongPic
        {
            get
            {
                return _songPic;
            }

            set
            {
                _songPic = value;
            }
        }
        /// <summary>
        /// 回复ID
        /// </summary>
        [SqlField]
        public int CommentId { get; set; } = 0;

        /// <summary>
        /// 语音相似度
        /// </summary>
        [SqlField]
        public float MatchRate { get; set; } = 0;

        /// <summary>
        /// 语音类型，1 本地语音，2 普通语音发帖，3 语音回复，4 语音红包 ,8 直播语音 9交友语音介绍
        /// </summary>
        [SqlField]
        public int VoiceType { get; set; } = 0;

        [SqlField]
        public string OpenId { get; set; }

        [SqlField]
        public string NickName { get; set; }
    }
}
