using DistributedAuthSystem.Constants;
using DistributedAuthSystem.Models;
using DistributedAuthSystem.Requests;
using DistributedAuthSystem.Responses;
using DistributedAuthSystem.Services;
using DistributedAuthSystem.States;
using FluentScheduler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Script.Serialization;
using Unity;

namespace DistributedAuthSystem.Scheduler
{
    public class ThinRequestJob : IJob, IRegisteredObject
    {
        private readonly object _lock = new object();

        private readonly RequestsMaker _requestsMaker;

        private bool _shuttingDown;

        public ThinRequestJob(RequestsMaker requestsMaker)
        {
            _requestsMaker = requestsMaker;

            // Register this job with the hosting environment.
            // Allows for a more graceful stop of the job, in the case of IIS shutting down.
            HostingEnvironment.RegisterObject(this);
        }

        public void Execute()
        {
            try
            {
                lock (_lock)
                {
                    if (_shuttingDown)
                        return;

                    _requestsMaker.SendThinRequestsToAll();
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