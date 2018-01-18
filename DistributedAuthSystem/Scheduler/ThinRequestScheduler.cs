using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentScheduler;
using Unity;
using System.Web.Http;
using DistributedAuthSystem.Services;

namespace DistributedAuthSystem.Scheduler
{
    public class ThinRequestScheduler : Registry
    {
        public ThinRequestScheduler()
        {
            var container = (IUnityContainer)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IUnityContainer));
            //Schedule<ThinRequestJob>().ToRunNow().AndEvery(20).Seconds();
            Schedule(() => new ThinRequestJob(container.Resolve<IClientsRepository>())).ToRunNow().AndEvery(20).Seconds();
        }
    }
}