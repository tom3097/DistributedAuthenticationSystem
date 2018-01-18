using DistributedAuthSystem.Services;
using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using Unity;

namespace DistributedAuthSystem.Scheduler
{
    public class ThinRequestJob : IJob, IRegisteredObject
    {
        private readonly object _lock = new object();

        private IClientsRepository icl;

        private bool _shuttingDown;

        public ThinRequestJob(IClientsRepository dfdf)
        {
            // Register this job with the hosting environment.
            // Allows for a more graceful stop of the job, in the case of IIS shutting down.
            HostingEnvironment.RegisterObject(this);
            //icl = dfdf;
            //var container = (IUnityContainer)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IUnityContainer));

            icl = dfdf;
        }

        public void Execute()
        {
            try
            {
                lock (_lock)
                {
                    if (_shuttingDown)
                        return;

                    // Do work, son!
                }
            }
            finally
            {
                // Always unregister the job when done.
                HostingEnvironment.UnregisterObject(this);
            }
        }

        public void Stop(bool immediate)
        {
            // Locking here will wait for the lock in Execute to be released until this code can continue.
            lock (_lock)
            {
                _shuttingDown = true;
            }

            HostingEnvironment.UnregisterObject(this);
        }
    }
}