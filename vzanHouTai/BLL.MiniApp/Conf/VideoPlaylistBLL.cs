using DAL.Base;
using Entity.MiniApp.Conf;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Conf
{
    public class VideoPlaylistBLL : BaseMySql<VideoPlayList>
    {
        #region 单例模式
        private static VideoPlaylistBLL _singleModel;
        private static readonly object SynObject = new object();

        private VideoPlaylistBLL()
        {

        }

        public static VideoPlaylistBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new VideoPlaylistBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public VideoPlayList GetModelByVideoId(int videoid)
        {
            return GetModel($"videoid={videoid}");
        }
        
        public List<VideoPlayList> GetVideoPlaylist(int pageIndex, int pageSize, int typeid, int orderby, int themeid, int orderbytype)
        {
            string sql = "select p.*,t.name themename from VideoPlayList p left join videoplaytheme t on p.theme=t.id ";
            string sqlwhere = $" where p.state>=0 ";
            string sqllimit = $" limit {(pageIndex - 1) * pageSize},{pageSize}";
            if(typeid>-1)
            {
                sqlwhere += $" and p.typeid={typeid} ";
            }
            if (themeid >= 0)
            {
                sqlwhere += $" and p.theme={themeid} ";
            }
            string orderbystr = " p.sort desc,p.id desc";
            switch (orderbytype)
            {
                case 1:
                    orderbystr = (orderby == 1 ? " p.playcount asc," : " p.playcount desc,") + orderbystr;
                    break;
                case 2:
                    orderbystr = (orderby == 1 ? " p.PlayTime asc," : " p.PlayTime desc,") + orderbystr;
                    break;
            }
            
            List<VideoPlayList> list = new List<VideoPlayList>();
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere+ " order by "+orderbystr + sqllimit, null))
            {
                while (dr.Read())
                {
                    VideoPlayList model = base.GetModel(dr);
                    if(dr["themename"]!=null)
                    {
                        model.ThemeName = dr["themename"].ToString();
                    }
                    
                    list.Add(model);
                }
            }

            return list;
        }
    }
}