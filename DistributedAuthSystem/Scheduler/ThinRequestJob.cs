using DistributedAuthSystem.Models;
using FluentScheduler;
using System.Web.Hosting;

namespace DistributedAuthSystem.Scheduler
{
    public class ThinRequestJob : IJob, IRegisteredObject
    {
        #region fields

        private readonly object _lock = new object();

        private readonly RequestsMaker _requestsMaker;

        private bool _shuttingDown;

        #endregion

        #region methods

        public ThinRequestJob(RequestsMaker requestsMaker)
        {
            _requestsMaker = requestsMaker;

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
                HostingEnvironment.UnregisterObject(this);
            }
        }

        public void Stop(bool immediate)
        {
            lock (_lock)
            {
                _shuttingDown = true;
            }

            HostingEnvironment.UnregisterObject(this);
        }

        #endregion
    }
}