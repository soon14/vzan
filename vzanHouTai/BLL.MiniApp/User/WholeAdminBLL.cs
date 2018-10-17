using DAL.Base;
using Entity.MiniApp;

namespace BLL.MiniApp
{
    public class WholeAdminBLL : BaseMySql<WholeAdmin>
    {
        //public bool checkWholeAdmin(string openid)
        //{
        //    List<WholeAdmin> list = getListbyCache();
        //    WholeAdmin model = list.Find(i => i.Openid == openid);
        //    return model != null;
        //}
        //public List<WholeAdmin> getListbyCache()
        //{
        //    List<WholeAdmin> list = null; 
        //    if (list == null)
        //    {
        //        list = GetList();
        //        RedisUtil.Set<List<WholeAdmin>>(MemCacheKey.WholeAdminKey, list, TimeSpan.FromHours(1));
        //    }
        //    return list;
        //}
    }
}
