using System.Web.Http;
using Microsoft.Owin;
using Owin;
[assembly: OwinStartup(typeof(KodakE1040_redExe.Program))]
public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        HttpConfiguration config = new HttpConfiguration();

        // Enable attribute-based routing
        config.MapHttpAttributeRoutes();

        // Default route
        config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
        );

        // Use Web API with OWIN
        app.UseWebApi(config);
    }
}
