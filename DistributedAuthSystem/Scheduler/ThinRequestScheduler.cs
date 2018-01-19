using FluentScheduler;
using Unity;
using System.Web.Http;
using DistributedAuthSystem.Services;
using DistributedAuthSystem.Models;

namespace DistributedAuthSystem.Scheduler
{
    public class ThinRequestScheduler : Registry
    {
        #region methods

        public ThinRequestScheduler()
        {
            var container = (IUnityContainer)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IUnityContainer));
            var clientRepository = container.Resolve<IClientsRepository>();
            var neighbourRepository = container.Resolve<INeighboursRepository>();
            var synchronizationRepository = container.Resolve<ISynchronizationsRepository>();
            var serverInfoRepository = container.Resolve<IServerInfoRepository>();
            var requestsMaker = new RequestsMaker(clientRepository, neighbourRepository,
                synchronizationRepository, serverInfoRepository);
            Schedule(() => new ThinRequestJob(requestsMaker)).ToRunNow().AndEvery(30).Seconds();
        }

        #endregion
    }
}