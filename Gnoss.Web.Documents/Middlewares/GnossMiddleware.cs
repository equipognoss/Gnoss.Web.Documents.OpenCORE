using Es.Riam.Util;
using System;
using System.Xml;
using Es.Riam.Gnoss.Util.General;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.ApplicationInsights.Extensibility;
using Gnoss.Web.Documents.Controllers;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Gnoss.Web.Documents.Services;

namespace Gnoss.Web.Documents.Middlewares
{
    public class GnossMiddleware
    {
        //private static GestorParametroAplicacion mGestorParametroAplicacion;

        private ConfigService _configService;
        private RequestDelegate _next;

        public GnossMiddleware(RequestDelegate next, ConfigService configService)
        {
            _configService = configService;
            _next = next;
        }

        protected void Application_Start(UtilTelemetry utilTelemetry, IHostEnvironment env)
        {
            //AreaRegistration.RegisterAllAreas();
            //GlobalConfiguration.Configure(WebApiConfig.Register);
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);

            string nodoRutaLogstash = _configService.GetLogstashEndpoint();
            if (!string.IsNullOrEmpty(nodoRutaLogstash))
            {
                LoggingService.InicializarLogstash(nodoRutaLogstash);
            }

            //Establezco la ruta del fichero de error por defecto
            //Util.General.Error.RUTA_FICHERO_ERROR = this.Server.MapPath("~/logs") + "\\" + "error" + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";        
            LoggingService.RUTA_DIRECTORIO_ERROR = Path.Combine(env.ContentRootPath, "logs");
            //Util.General.Error.RUTA_FICHERO_CONSULTA_COSTOSA = this.Server.MapPath("~/logs") + "\\" + "consulta_costosa" + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";

            LeerConfiguracionApplicationInsights(utilTelemetry);
        }

        public async Task Invoke(HttpContext context, UtilTelemetry utilTelemetry, LoggingService error, IHostEnvironment env)
        {
            Application_Start(utilTelemetry, env);
            await _next(context);
        }

        /// <summary>
        /// Obtiene la configuración de application insights
        /// </summary>
        private void LeerConfiguracionApplicationInsights(UtilTelemetry utilTelemetry)
        {
            string implementationKey = _configService.GetImplementationKey();

            if (!string.IsNullOrEmpty(implementationKey))
            {
                implementationKey = implementationKey.ToLower();
                TelemetryConfiguration.Active.InstrumentationKey = implementationKey;
                utilTelemetry.Telemetry.InstrumentationKey = implementationKey;
            }

            string logsLocation = _configService.GetLogLocation();

            int valorInt;
            if (int.TryParse(logsLocation, out valorInt))
            {
                if (Enum.IsDefined(typeof(UtilTelemetry.UbicacionLogsYTrazas), valorInt))
                {
                    LoggingService.UBICACIONLOGS = (UtilTelemetry.UbicacionLogsYTrazas)valorInt;
                }
            }
        }
    }

    public static class ApplicationStartExtensions
    {
        public static IApplicationBuilder UseGnossMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GnossMiddleware>();
        }
    }
}
