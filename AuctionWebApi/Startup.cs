using System.Web.Http;
using Owin;
using Swashbuckle.Application;

namespace AuctionWebApi {
  public static class Startup {
    // This code configures Web API. The Startup class is specified as a type
    // parameter in the WebApp.Start method.
    public static void ConfigureApp(IAppBuilder appBuilder) {
      // Configure Web API for self-host. 
      HttpConfiguration config = new HttpConfiguration();

      config.EnableSwagger(c => c.SingleApiVersion("v1", "Service Fabric Bay API")).EnableSwaggerUi();

      config.MapHttpAttributeRoutes();

      config.Routes.MapHttpRoute(
          name: "DefaultApi",
          routeTemplate: "api/{controller}/{id}",
          defaults: new { id = RouteParameter.Optional }
      );

      appBuilder.UseWebApi(config);
    }
  }
}
