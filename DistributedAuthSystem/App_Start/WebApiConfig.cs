using DistributedAuthSystem.Resolver;
using DistributedAuthSystem.Services;
using System.Web.Http;
using System.Web.Http.Tracing;
using Unity;
using Unity.Lifetime;

namespace DistributedAuthSystem
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var container = new UnityContainer();
            container.RegisterType<INeighboursRepository, NeighboursRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IClientsRepository, ClientsRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISynchronizationsRepository, SynchronizationsRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IServerInfoRepository, ServerInfoRepository>(new ContainerControlledLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);

            // Web API routes
            config.MapHttpAttributeRoutes();
            SystemDiagnosticsTraceWriter traceWriter = config.EnableSystemDiagnosticsTracing();
            traceWriter.IsVerbose = true;
            traceWriter.MinimumLevel = TraceLevel.Debug;

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
