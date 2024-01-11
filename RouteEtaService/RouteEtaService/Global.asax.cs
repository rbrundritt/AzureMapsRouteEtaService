using RouteEtaService.Models;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace RouteEtaService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        //In-Memory cache of routes. In production, use a shared distributed cache like Redis.
        internal static Dictionary<long, CachedRouteInfo> CachedRoutes = new Dictionary<long, CachedRouteInfo>();

        //In production, use a secure way to store the Azure Maps subscription key. Or use Managed Identities in Azure.
        internal static string AzureMapsSubscriptionKey = "<Your Azure Maps Key>";

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.Formatters.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(new JsonMediaTypeFormatter());
        }
    }
}