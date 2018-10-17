using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OpenWx.Startup))]
namespace OpenWx
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
